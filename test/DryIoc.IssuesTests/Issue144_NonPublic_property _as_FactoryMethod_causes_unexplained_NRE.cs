using System.Reflection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue144_NonPublic_property_as_FactoryMethod_causes_unexplained_NRE : ITest
    {
        public int Run()
        {
            There_should_not_be_NRE();
            return 1;
        }

        [Test]
        public void There_should_not_be_NRE()
        {
            var c = new Container();

            var prop = typeof(C).GetProperty("P", BindingFlags.NonPublic | BindingFlags.Static);

            var factory = typeof(string).ToFactory(made: Made.Of(prop));

            c.Register(factory, typeof(string), null, IfAlreadyRegistered.Replace, true);

            var s = c.Resolve<string>();

            Assert.AreEqual("boo!", s);
        }

        class C
        {
            static string P { get { return "boo!"; } }
        }
    }
}
