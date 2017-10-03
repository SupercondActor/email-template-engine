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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupercondActor.Utils.TemplateEngine;

namespace UnitTests
{
    [TestClass]
    public class ArrayTests
    {
        private ITemplateRenderer _renderer;

        [TestInitialize]
        public void TestInitialize()
        {
            _renderer = new TemplateRenderer();
        }

        [TestMethod]
        public void ListOfRecords()
        {
            var templateContent = @"<h1>Dear {{name}},</h1>
<p>Here is your order:</p>
[[$.orderLines]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>[[/$.orderLines]]
";

            var dataJson = @"
{'name': 'John',
'orderLines': [
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.0199 },
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 },
{ 'LineNumber': 3, 'Item': 'iTV', 'Price': 1200 }
]
}";

            var result = _renderer.Render(templateContent, dataJson);

            Assert.IsTrue(result.Contains("John"));
            Assert.IsTrue(result.Contains("$600.02"));
            Assert.IsTrue(result.Contains("$900.99"));
            Assert.IsTrue(result.Contains("$1,200.00"));
        }

        [TestMethod]
        public void ArrayDataObject()
        {
            var templateContent = @"<p>Here is your order:</p>
[[$]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>[[/$]]
";

            var dataJson = @"[
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.0199 },
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 },
{ 'LineNumber': 3, 'Item': 'iTV', 'Price': 1200 }
]";

            var result = _renderer.Render(templateContent, dataJson);

            Assert.IsTrue(result.Contains("$600.02"));
            Assert.IsTrue(result.Contains("$900.99"));
            Assert.IsTrue(result.Contains("$1,200.00"));
        }


        [TestMethod]
        public void ParentReferenceInTheList()
        {
            var templateContent = @"<h1>Dear {{name}},</h1>
<p>Here is your order:</p>
[[$.orderLines]]
Item {{$$$$.orderNumber}}/{{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>[[/$.orderLines]]
";

            var dataJson = @"
{
'orderNumber': 42,
'name': 'John',
'orderLines': [
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.0199 },
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 },
{ 'LineNumber': 3, 'Item': 'iTV', 'Price': 1200 }
]
}";

            var result = _renderer.Render(templateContent, dataJson);

            Assert.IsTrue(result.Contains("John"));
            Assert.IsTrue(result.Contains("Item 42/1"));
            Assert.IsTrue(result.Contains("$600.02"));
            Assert.IsTrue(result.Contains("Item 42/2"));
            Assert.IsTrue(result.Contains("$900.99"));
            Assert.IsTrue(result.Contains("Item 42/3"));
            Assert.IsTrue(result.Contains("$1,200.00"));

        }

        [TestMethod]
        public void ShouldRenderRecursiveArrays()
        {
            var json = @"{'fname':'John', 'lname':'Doe', 'Amount': 13.25, 'Date': '2013-10-21T13:28:06.419Z',
'OrderLines': [
{ 'LineNumber': 1, 'Item': 'iPhone', 'Price': 600.01,
'Accessories': [
{ 'Number': 1, 'Name': 'Cord' },
{ 'Number': 2, 'Name': 'Cover' }
]},
{ 'LineNumber': 2, 'Item': 'iPad', 'Price': 900.99 }
]
}"; 

            var template = @"
<h1>Dear {{$.fname}} {{$.lname}},</h1>
Thank you for your order.<br/>
Your total amount is ${{$.Amount ~ 0000.0}}, Date: {{$.Date~d}}

<p>Ordered items</p>
[[$.OrderLines  ]]
Item {{$.LineNumber}}: {{$.Item}}, your price: {{$.Price~C}} <br/>
 Accessories:<br/>
[[$.Accessories]] {{$.Number}}: {{$.Name}} for {{$$$$.Item}}
[[/$.Accessories ~ No accessories found for {{$.Item}}]][[/$.OrderLines]]

Note the iPad price: {{$.OrderLines[?(@.Item == 'iPad')].Price~c}}

<p>Thank you.</p>
";

            var res = _renderer.Render(template, json);

            Assert.IsNotNull(res);
            Assert.IsTrue(res.Contains("John Doe"));
            Assert.IsTrue(res.Contains("$0013.3,"));
            Assert.IsTrue(res.Contains("10/21/2013"));
            Assert.IsTrue(res.Contains("$600.01"));
        }
    }
}
