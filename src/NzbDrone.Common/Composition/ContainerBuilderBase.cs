using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using TinyIoC;

#if NETCOREAPP
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
#endif

namespace NzbDrone.Common.Composition
{
    public abstract class ContainerBuilderBase
    {
        private readonly List<Type> _loadedTypes;

        protected IContainer Container { get; }

        protected ContainerBuilderBase(IStartupContext args, List<string> assemblies)
        {
            _loadedTypes = new List<Type>();

            assemblies.Add(OsInfo.IsWindows ? "Radarr.Windows" : "Radarr.Mono");
            assemblies.Add("Radarr.Common");

#if !NETCOREAPP
            foreach (var assembly in assemblies)
            {
                _loadedTypes.AddRange(Assembly.Load(assembly).GetTypes());
            }
#else
            var startupPath = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var assemblyName in assemblies)
            {
                _loadedTypes.AddRange(AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(startupPath, $"{assemblyName}.dll")).GetTypes());
            }

            var toRegisterResolver = new List<string> { "System.Data.SQLite" };
            toRegisterResolver.AddRange(assemblies.Intersect(new[] { "Radarr.Core" }));
            RegisterNativeResolver(toRegisterResolver);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ContainerResolveEventHandler);
#endif

            Container = new Container(new TinyIoCContainer(), _loadedTypes);
            AutoRegisterInterfaces();
            Container.Register(args);
        }

#if  NETCOREAPP
        private static Assembly ContainerResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var resolver = new AssemblyDependencyResolver(args.RequestingAssembly.Location);
            var assemblyPath = resolver.ResolveAssemblyToPath(new AssemblyName(args.Name));

            if (assemblyPath == null)
            {
                return null;
            }

            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }

        public static void RegisterNativeResolver(IEnumerable<string> assemblyNames)
        {
            // This ensures we look for sqlite3 using libsqlite3.so.0 on Linux and not libsqlite3.so which
            // is less likely to exist.
            foreach (var name in assemblyNames)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{name}.dll"));

                try
                {
                    NativeLibrary.SetDllImportResolver(assembly, LoadNativeLib);
                }
                catch (InvalidOperationException)
                {
                    // This can only be set once per assembly
                    // Catch required for NzbDrone.Host tests
                }
            }
        }

        private static IntPtr LoadNativeLib(string libraryName, Assembly assembly, DllImportSearchPath? dllImportSearchPath)
        {
            var mappedName = libraryName;
            if (OsInfo.IsLinux)
            {
                if (libraryName == "sqlite3")
                {
                    mappedName = "libsqlite3.so.0";
                }
                else if (libraryName == "mediainfo")
                {
                    mappedName = "libmediainfo.so.0";
                }
            }

            return NativeLibrary.Load(mappedName, assembly, dllImportSearchPath);
        }
#endif

        private void AutoRegisterInterfaces()
        {
            var loadedInterfaces = _loadedTypes.Where(t => t.IsInterface).ToList();
            var implementedInterfaces = _loadedTypes.SelectMany(t => t.GetInterfaces());

            var contracts = loadedInterfaces.Union(implementedInterfaces).Where(c => !c.IsGenericTypeDefinition && !string.IsNullOrWhiteSpace(c.FullName))
                .Where(c => !c.FullName.StartsWith("System"))
                .Except(new List<Type> { typeof(IMessage), typeof(IEvent), typeof(IContainer) }).Distinct().OrderBy(c => c.FullName);

            foreach (var contract in contracts)
            {
                AutoRegisterImplementations(contract);
            }
        }

        protected void AutoRegisterImplementations<TContract>()
        {
            AutoRegisterImplementations(typeof(TContract));
        }

        private void AutoRegisterImplementations(Type contractType)
        {
            var implementations = Container.GetImplementations(contractType).Where(c => !c.IsGenericTypeDefinition).ToList();

            if (implementations.Count == 0)
            {
                return;
            }

            if (implementations.Count == 1)
            {
                var impl = implementations.Single();
                Container.RegisterSingleton(contractType, impl);
            }
            else
            {
                Container.RegisterAllAsSingleton(contractType, implementations);
            }
        }
    }
}
