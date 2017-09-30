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
