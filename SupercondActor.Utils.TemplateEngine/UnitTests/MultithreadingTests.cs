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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupercondActor.Utils.TemplateEngine;

namespace UnitTests
{
    [TestClass]
    public class MultithreadingTests
    {
        private ITemplateRenderer _renderer;

        [TestInitialize]
        public void TestInitialize()
        {
            _renderer = new TemplateRenderer();
        }

        [TestMethod]
        public void RenderMultipleThreads()
        {
            var taskCount = 100;
            var tasks = new List<Task<string>>();

            for (int i = 0; i < taskCount; i++)
            {
                var templateContent = "One field " + i + ": {{name}}";
                var data = new { name = $"fieldName{i}" };
                tasks.Add(Task.Run(() =>
                {
                    return _renderer.Render(templateContent, data);
                }));
            }
            Task.WhenAll(tasks).Wait();

            for (int i = 0; i < taskCount; i++)
            {
                Assert.AreEqual($"One field {i}: fieldName{i}", tasks[i].Result);
            }
        }
    }
}
