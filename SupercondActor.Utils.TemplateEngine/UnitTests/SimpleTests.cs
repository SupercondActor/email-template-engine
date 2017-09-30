using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupercondActor.Utils.TemplateEngine;

namespace UnitTests
{
    [TestClass]
    public class SimpleTests
    {
        private ITemplateRenderer _renderer;

        [TestInitialize]
        public void TestInitialize()
        {
            _renderer = new TemplateRenderer();
        }

        [TestMethod]
        public void OneField()
        {
            var templateContent = "One field: {{name}}";
            var data = new { name = "fieldName" };

            var result = _renderer.Render(templateContent, data);

            Assert.AreEqual("One field: fieldName", result);
        }

        [TestMethod]
        public void OneFieldHtmlEncoded()
        {
            var templateContent = "One html field: {{name}}";
            var data = new { name = "<p>fieldName</p>" };

            var result = _renderer.Render(templateContent, data);

            Assert.AreEqual("One html field: &lt;p&gt;fieldName&lt;/p&gt;", result);
        }

        [TestMethod]
        public void OneFieldNotHtmlEncoded()
        {
            var templateContent = "One html field: {{name}}";
            var data = new { name = "<p>fieldName</p>" };

            var result = _renderer.Render(templateContent, data, false);

            Assert.AreEqual("One html field: <p>fieldName</p>", result);
        }

        [TestMethod]
        public void JsonDataFields()
        {
            var templateContent = "From json: {{firstName}}, {{lastName}}";
            var jsonData = "{'firstName': 'MyFirstName', 'lastName': 'MyLastName'}";

            var result = _renderer.Render(templateContent, jsonData);

            Assert.AreEqual("From json: MyFirstName, MyLastName", result);
        }

    }
}
