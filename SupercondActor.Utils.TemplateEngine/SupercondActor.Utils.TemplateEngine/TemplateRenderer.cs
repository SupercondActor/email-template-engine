#region License
//Copyright(c) 2017 Aleksey Solonchev

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE. 
#endregion

using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SupercondActor.Utils.TemplateEngine
{
    public class TemplateRenderer : ITemplateRenderer
    {
        Regex repeatRx = new Regex(@"\[\[(.+?)\s*\]\](.*)\[\[/\1\s*~?(.*?)\]\]", RegexOptions.Singleline);
        Regex fieldRx = new Regex(@"\{\{(.*?)\}\}");

        private const string ReplacementDBS = "DBS_FB00DCC9857147D5B40F4F4EAFBFC98C";
        private const string ReplacementOB = "OB_EFFECBE43E544F07852F4236FB282F57";
        private const string ReplacementCB = "CB_F8FCF57B5DF24C21B115643E3F813044";
        private const string ReplacementOCB = "OCB_CE00270295E04500A1853129F40E122C";
        private const string ReplacementCCB = "CCB_B383DC9DB9C9422F8B1CF92CCC844834";

        public string Render(string templateFileContent, object data, bool htmlEncode = true)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
            return Render(templateFileContent, json, htmlEncode);
        }

        public string Render(string templateFileContent, string dataJson, bool htmlEncode = true)
        {
            if (string.IsNullOrWhiteSpace(templateFileContent)) throw new ArgumentException("Missing template value.");
            if (string.IsNullOrWhiteSpace(dataJson)) return templateFileContent;

            var json = JToken.Parse(dataJson);

            var tmpTemplate = templateFileContent?.Replace(@"\\", ReplacementDBS).Replace(@"\[", ReplacementOB).Replace(@"\]", ReplacementCB).Replace(@"\{", ReplacementOCB).Replace(@"\}", ReplacementCCB);
            var res = ProcessFragmentWithRepeats(tmpTemplate, json, htmlEncode);
            return res?.Replace(ReplacementDBS, @"\").Replace(ReplacementOB, @"[").Replace(ReplacementCB, @"]").Replace(ReplacementOCB, @"{").Replace(ReplacementCCB, @"}");
        }

        private string ProcessFragmentWithRepeats(string template, JToken json, bool htmlEncode)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }
            var rptMatches = repeatRx.Matches(template);

            var frSb = new StringBuilder(template);
            foreach (Match mt in rptMatches)
            {
                var rptsSb = new StringBuilder();
                var pattern = mt.Groups[0].Value;
                var path = mt.Groups[1].Value ?? "";
                var fragment = mt.Groups[2].Value ?? "";
                var noItemsTxt = mt.Groups[3].Value ?? "";
                var fragmentToken = SelectToken(json, path);
                var itemsFound = false;
                if (fragmentToken?.Type == JTokenType.Boolean && fragmentToken.Value<bool>())
                {
                    itemsFound = true;
                    rptsSb.Append(ProcessFragmentWithRepeats(fragment, fragmentToken, htmlEncode));
                }
                else if (fragmentToken?.Type == JTokenType.Array)
                {
                    var rows = fragmentToken.AsJEnumerable();
                    foreach (var jt in rows)
                    {
                        itemsFound = true;
                        rptsSb.Append(ProcessFragmentWithRepeats(fragment, jt, htmlEncode));
                    }
                }
                else if (fragmentToken?.Type == JTokenType.String && !string.IsNullOrWhiteSpace(fragmentToken.Value<string>()))
                {
                    itemsFound = true;
                    rptsSb.Append(ProcessFragmentWithRepeats(fragment, fragmentToken, htmlEncode));
                }

                if (!itemsFound)
                {
                    rptsSb.Append(ProcessTemplateFragment(noItemsTxt, json, htmlEncode));
                }
                frSb.Replace(pattern, rptsSb.ToString());
            }

            var res = ProcessTemplateFragment(frSb.ToString(), json, htmlEncode);
            return res;
        }

        private string ProcessTemplateFragment(string fragment, JToken json, bool htmlEncode)
        {
            var mts = fieldRx.Matches(fragment);
            var templateSb = new StringBuilder(fragment);
            foreach (Match mt in mts)
            {
                ProcessField(templateSb, json, mt, htmlEncode);
            }
            return templateSb.ToString();
        }

        private static void ProcessField(StringBuilder templateSb, JToken json, Match mt, bool htmlEncode)
        {
            string fldVal = "";
            var match = mt.Groups[1].Value ?? "";
            if (!string.IsNullOrWhiteSpace(match))
            {
                var fldContent = match.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                var path = fldContent[0].Trim();
                var format = fldContent.Length > 1 ? fldContent[1].Trim() : null;

                var token = SelectToken(json, path);
                if (token != null)
                {
                    if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                    {
                        if (string.IsNullOrWhiteSpace(format))
                        {
                            fldVal = token.Value<Decimal>().ToString();
                        }
                        else
                        {
                            fldVal = token.Value<Decimal>().ToString(format);
                        }
                    }
                    else if (token.Type == JTokenType.Date)
                    {
                        if (string.IsNullOrWhiteSpace(format))
                        {
                            fldVal = token.Value<DateTime>().ToString("g");
                        }
                        else
                        {
                            fldVal = token.Value<DateTime>().ToString(format);
                        }
                    }
                    else
                    {
                        fldVal = token.ToString();
                    }
                }
            }

            if (htmlEncode)
            {
                fldVal = System.Net.WebUtility.HtmlEncode(fldVal);
            }
            templateSb.Replace("{{" + match + "}}", fldVal).ToString();
        }

        private static JToken SelectToken(JToken jsonToken, string path)
        {
            var exToken = jsonToken;
            var extPath = path.Trim();
            while (extPath.StartsWith("$$"))
            {
                extPath = extPath.Substring(1);
                exToken = exToken.Parent;
            }
            return exToken.SelectToken(extPath);
        }
    }
}
