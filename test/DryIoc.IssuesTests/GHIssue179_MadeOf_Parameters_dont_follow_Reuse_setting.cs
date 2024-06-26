using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue179_MadeOf_Parameters_do_not_follow_Reuse_setting : ITest
    { 
        public int Run()
        {
            Test();
            Test_with_parameter_selector();
            Test_made_with_parameters();
            return 3;
        }

        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<A>();
            container.Register<B>();

            container.Register<ILog>(made: Made.Of(
                () => new Log(Arg.Index<Type>(0)),
                request => request.Parent.ImplementationType));

            var b = container.Resolve<B>();

            Assert.AreNotSame(b.Log.ContextType, b.A.Log.ContextType);
        }

        [Test]
        public void Test_with_parameter_selector()
        {
            var container = new Container();
            container.Register<A>();
            container.Register<B>();

            container.Register(typeof(ILog), typeof(Log),
                made: Parameters.Of.Type<Type>(r => r.Parent.ImplementationType));

            var b = container.Resolve<B>();

            Assert.AreNotSame(b.Log.ContextType, b.A.Log.ContextType);
        }

        [Test]
        public void Test_made_with_parameters()
        {
            var container = new Container();
            container.Register<A>();
            container.Register<B>();

            container.Register<ILog>(
                made: Made.Of(
                    typeof(Log).SingleConstructor(),
                    parameters: Parameters.Of.Type<Type>(r => r.Parent.ImplementationType)));

            var b = container.Resolve<B>();

            Assert.AreNotSame(b.Log.ContextType, b.A.Log.ContextType);
        }

        public interface ILog
        {
            Type ContextType { get; }
        }

        public class Log : ILog
        {
            public Type ContextType { get; }
            public Log(Type contextType) => ContextType = contextType;
        }

        public class B
        {
            public ILog Log { get; }
            public A A { get; }

            public B(A a, ILog log)
            {
                Log = log;
                A = a;
            }
        }

        public class A
        {
            public ILog Log { get; }
            public A(ILog log) => Log = log;
        }
    }
}
