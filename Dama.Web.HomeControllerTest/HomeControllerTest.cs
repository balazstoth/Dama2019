using System.Web.Mvc;
using Dama.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dama.Web.HomeControllerTest
{
    [TestClass]
    public class HomeControllerTest
    {
        HomeController controller = new HomeController();

        [TestMethod]
        public void TestIndexView()
        {
            var result = controller.Index() as ViewResult;
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void TestAboutView()
        {
            var result = controller.About() as ViewResult;
            Assert.AreEqual("About", result.ViewName);
        }
    }
}
