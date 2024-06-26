using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue610_CustomDynamicRegistrationProvider_ConstructorWithResolvableArguments : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            // prepare registrations for the GetDynamicRegistrations method
            RegisterCommands();
            Assert.AreEqual(1, DynamicRegistrations.Count()); // one interface: ICommand
            Assert.AreEqual(2, DynamicRegistrations.First().Value.Count()); // two implementations: Command1, Command2

            // make sure that dynamic registration provider works as expected
            var registrations = GetDynamicRegistrations(typeof(ICommand), serviceKey: null);
            Assert.AreEqual(2, registrations.Count());

            // attach the dynamic registration provider and try resolving the services
            var container = new Container().WithMef()
                .With(r => r.WithDynamicRegistrations(GetDynamicRegistrations)
                .With(FactoryMethod.ConstructorWithResolvableArguments));

            // resolve the commands lazily
            var commands = container.Resolve<Lazy<ICommand, IScriptMetadata>[]>();
            Assert.NotNull(commands);
            Assert.AreEqual(2, commands.Length);

            // lazy resolution part works fine:
            Assert.NotNull(commands[0]);
            Assert.NotNull(commands[1]);
            Assert.NotNull(commands[0].Metadata);
            Assert.NotNull(commands[1].Metadata);
            Assert.AreEqual(3, commands[0].Metadata.ScriptID + commands[1].Metadata.ScriptID); // should be 1 and 2, in any order

            // instantiation should also work now:
            Assert.IsNotNull(commands[0].Value);
        }

        // index: ServiceTypeFullName -> list of ServiceRegistrations
        private ConcurrentDictionary<string, List<DynamicRegistration>> DynamicRegistrations { get; } =
            new ConcurrentDictionary<string, List<DynamicRegistration>>();

        private void RegisterCommands()
        {
            // index only registrations related to this issue
            var lazyRegistrations = AttributedModel.Scan(new[] { typeof(Command1).Assembly })
                .MakeLazyAndEnsureUniqueServiceKeys()
                .Where(r => r.ImplementationTypeFullName.IndexOf("GHIssue610") >= 0)
                .ToArray();

            // Command1 and Command2
            Assert.AreEqual(2, lazyRegistrations.Length);

            var typeProvider = new Func<string, Type>(t => typeof(Command1).Assembly.GetType(t));

            // index export registrations by exported service type
            foreach (var reg in lazyRegistrations)
            {
                foreach (var export in reg.Exports)
                {
                    var regs = DynamicRegistrations.GetOrAdd(export.ServiceTypeFullName, _ => new List<DynamicRegistration>());
                    regs.Add(new DynamicRegistration(reg.CreateFactory(typeProvider), serviceKey: export.ServiceKey));
                }
            }
        }

        private IEnumerable<DynamicRegistration> GetDynamicRegistrations(Type serviceType, object serviceKey)
        {
            if (DynamicRegistrations.TryGetValue(serviceType.FullName, out var regs))
            {
                // You may rely on DryIoc to find the dynamic registration by key.
                return regs;
            }

            return null;
        }

        public interface IScriptMetadata
        {
            long ScriptID { get; }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false), MetadataAttribute]
        public class ScriptAttribute : Attribute, IScriptMetadata
        {
            public ScriptAttribute(long id) { ScriptID = id; }
            public long ScriptID { get; private set; }
        }

        public interface ICommand
        {
        }

        [Export(typeof(ICommand)), Script(1)]
        public class Command1 : ICommand
        {
        }

        [Export(typeof(ICommand)), Script(2)]
        public class Command2 : ICommand
        {
        }
    }
}
