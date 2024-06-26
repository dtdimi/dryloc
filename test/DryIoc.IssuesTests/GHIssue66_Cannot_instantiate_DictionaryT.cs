using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue66_Cannot_instantiate_DictionaryT : ITest
    {
        public int Run()
        {
            AutoConcreteTypeResolution_should_be_able_to_create_with_default_ctor();
            WithConcreteTypeDynamicRegistrations_should_be_able_to_create_with_default_ctor();
            Should_throw_on_nested_unresolved_dep();
            Should_resolve_dep();
            Should_throw_when_no_constructors_are_match();
            Should_throw_when_single_constructor_cant_be_used();
            Should_consider_arguments_passed_to_Resolve();
            return 7;
        }

        [Test]
        public void AutoConcreteTypeResolution_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void WithConcreteTypeDynamicRegistrations_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void Should_throw_on_nested_unresolved_dep()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            Assert.Throws<ContainerException>(() =>
                container.Resolve<A>());
        }

        [Test]
        public void Should_resolve_dep()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            container.Register<I, X>();

            var a = container.Resolve<A>();
            Assert.IsNotNull(a.B.I);
        }

        [Test]
        public void Should_throw_when_no_constructors_are_match()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var d = container.Resolve<D>();

            Assert.IsNull(d.I);
        }

        [Test]
        public void Should_throw_when_single_constructor_cant_be_used()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<E>());
            Assert.AreEqual(
                Error.NameOf(Error.UnableToResolveUnknownService),
                ex.ErrorName);
        }

        [Test]
        public void Should_consider_arguments_passed_to_Resolve()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var d = container.Resolve<D>(new object[] { (I)new X() });
            Assert.IsNotNull(d.I);
        }

        public class A
        {
            public B B { get; }
            public A(B b)
            {
                B = b;
            }
        }

        public class B
        {
            public I I { get; }
            public B(I i)
            {
                I = i;
            }
        }

        public interface I { }
        public class X : I { }

        public class D
        {
            public I I { get; }
            public D() { }
            public D(I i)
            {
                I = i;
            }
        }

        public class E
        {
            public I I { get; }
            public E(I i)
            {
                I = i;
            }
        }
    }
}
