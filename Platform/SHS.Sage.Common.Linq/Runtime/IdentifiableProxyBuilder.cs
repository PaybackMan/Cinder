using SHS.Platform.Compilation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Runtime
{
    public static class IdentifiableProxyBuilder
    {
        static ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        static Dictionary<Type, Type> _proxyTypes = new Dictionary<Type, Type>();
        public static Type GetProxyType<T>() where T : IIdentifiable
        {
            return GetProxyType(typeof(T));
        }

        public static T CreateProxy<T>(IRepository repository) where T : IIdentifiable
        {
            var type = GetProxyType<T>();
            return (T)Activator.CreateInstance(type, new object[] { repository });
        }

        public static IIdentifiable CreateProxy(Type identifiableType, IRepository repository)
        {
            var type = GetProxyType(identifiableType);
            return (IIdentifiable)Activator.CreateInstance(type, new object[] { repository });
        }

        public static Type GetProxyType(Type identifiableType)
        {
            if (!identifiableType.Implements(typeof(IIdentifiable)))
                throw new InvalidOperationException("Type must implement IIdentifiable");

            Type proxyType;
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                if (!_proxyTypes.TryGetValue(identifiableType, out proxyType))
                {
                    try
                    {
                        _rwLock.EnterWriteLock();
                        proxyType = CreateProxyType(identifiableType);
                        _proxyTypes.Add(identifiableType, proxyType);
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
            return proxyType;
        }

        private static Type CreateProxyType(Type identifiableType)
        {
            var classTemplate = GetClassTemplate();
            var scalarPropTemplate = GetScalarPropertyTemplate();
            var enumerablePropTemplate = GetEnumerablePropertyTemplate();

            var baseType = identifiableType;
            var baseTypeName = baseType.Name;
            var baseNamespace = baseType.Namespace;
            var genTypeName = "";
            var className = "IdentifiableProxy_" + baseTypeName;
            var scalarProps = new StringBuilder();
            var enumerableProps = new StringBuilder();
            var aggregator = "";

            foreach(var pi in baseType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(mi => mi.MemberType == MemberTypes.Property 
                                                    && mi.IsComplex()
                                                    && ((PropertyInfo)mi).GetMethod.IsVirtual
                                                    && !((PropertyInfo)mi).GetMethod.IsFinal)
                                       .OfType<PropertyInfo>())
            {
                if (pi.IsScalar())
                {
                    var propTypeName = pi.PropertyType.FullName;
                    // format the property override from the template
                    scalarProps.AppendLine(scalarPropTemplate
                        .Replace("@baseTypeName", propTypeName)
                        .Replace("@propertyName", pi.Name));
                }
                else
                {
                    var propTypeName = pi.PropertyType.FullName;
                    // determine the aggregate type, Array, List or IEnumerable
                    if (pi.PropertyType.IsArray)
                    {
                        genTypeName = pi.PropertyType.GetElementType().FullName;
                        aggregator = ".ToArray()";
                    }
                    else if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition().Equals(typeof(ObservableCollection<>)))
                    {
                        propTypeName = GetGenericTypeString(pi.PropertyType);
                        genTypeName = pi.PropertyType.GetGenericArguments()[0].FullName;
                        aggregator = ".ToObservable()";
                    }
                    else if (pi.PropertyType.Implements(typeof(IList<>)))
                    {
                        propTypeName = GetGenericTypeString(pi.PropertyType);
                        genTypeName = pi.PropertyType.GetGenericArguments()[0].FullName;
                        aggregator = ".ToList()";
                    }
                    else if (pi.PropertyType.Implements(typeof(IEnumerable<>)))
                    {
                        propTypeName = GetGenericTypeString(pi.PropertyType);
                        genTypeName = pi.PropertyType.GetGenericArguments()[0].FullName;
                    }

                    enumerableProps.AppendLine(enumerablePropTemplate
                        .Replace("@baseTypeName", propTypeName)
                        .Replace("@propertyName", pi.Name)
                        .Replace("@aggregator", aggregator)
                        .Replace("@genTypeName", genTypeName));
                }
            }

            var newClass = classTemplate.Replace("@baseNamespace", baseNamespace)
                .Replace("@className", className)
                .Replace("@baseTypeName", baseTypeName)
                .Replace("@scalarProps", scalarProps.ToString())
                .Replace("@enumerableProps", enumerableProps.ToString());

            var compilationResponse = CompileProxyType(identifiableType, newClass);
            if (!compilationResponse.HasErrors)
            {
                return compilationResponse.Assembly.GetTypes().SingleOrDefault(t => t.BaseType.Equals(baseType));
            }
            else throw new InvalidOperationException(compilationResponse.Errors.ToErrorString());
        }

        private static string GetGenericTypeString(Type type)
        {
            if (type.IsConstructedGenericType)
            {
                var genericType = type.FullName.Substring(0, type.FullName.IndexOf("`"));
                var genericArgs = "";
                foreach (var arg in type.GetGenericArguments())
                {
                    if (genericArgs.Length > 0)
                    {
                        genericArgs += ", ";
                    }
                    genericArgs += GetGenericTypeString(arg);
                }
                return string.Format("{0}<{1}>", genericType, genericArgs);
            }
            else return type.FullName;
        }

        private static CompilationResponse CompileProxyType(Type identifiableType, string newClass)
        {
            var compilationUnit = new CompilationUnit(newClass);
            var path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "Temp");
            Directory.CreateDirectory(path);
            var request = new CompilationRequest(path,
                CompilationRequest.STANDARD_REFERENCES
                .Union(GetDependentAssemblies(identifiableType))
                .ToArray(),
                true,
                new[] { compilationUnit })
            {
                AssemblyName = string.Format("Proxies_{0}", Environment.TickCount)
            };
            return CSharpCompiler.Compile(request);
        }

        private static IEnumerable<string> GetDependentAssemblies(Type identifiableType)
        {
            //var assemblies = new List<string>();

            //_visited.Clear();
            //GetDependentAssembliesRecurse(identifiableType.Assembly, assemblies);

            //return assemblies;

            return identifiableType.Assembly.GetReferencedAssemblies()
                .Select(asmName => Assembly.ReflectionOnlyLoad(asmName.FullName))
                .Where(asm => !asm.IsDynamic && !CompilationRequest.STANDARD_REFERENCES.Contains(asm.ManifestModule.Name.ToLower()))
                .Select(asm => asm.Location)
                .ToArray()
                .Union(new[] { identifiableType.Assembly.Location });
        }

        //[ThreadStatic]
        //static HashSet<Assembly> _visited = new HashSet<Assembly>();
        //private static void GetDependentAssembliesRecurse(Assembly asm, List<string> assemblies)
        //{
        //    if (_visited.Contains(asm)) return;
        //    _visited.Add(asm);

        //    if (assemblies.Contains(asm.CodeBase)) return;

        //    if (asm.GlobalAssemblyCache)
        //    {
        //        if (CompilationRequest.STANDARD_REFERENCES.Contains(asm.ManifestModule.Name)) return;
        //        try
        //        {
        //            Assembly.Load(asm.ManifestModule.Name);
        //            assemblies.Add(asm.ManifestModule.Name);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //        }
        //    }
        //    else
        //    {
        //        assemblies.Add(asm.CodeBase);
        //    }

        //    foreach (var assemblyName in asm.GetReferencedAssemblies())
        //    {
        //        GetDependentAssembliesRecurse(Assembly.Load(assemblyName), assemblies);
        //    }
        //}

        private static bool IsComplex(this MemberInfo mi)
        {
            var type = TypeHelper.GetMemberType(mi);
            return type.Implements(typeof(IIdentifiable))
                || 
                (
                    type.IsGenericType
                    &&
                    type.Implements(typeof(IEnumerable<>)) 
                    && 
                    type.GetGenericArguments()[0].Implements(typeof(IIdentifiable))
                )
                ||
                (
                    type.IsArray
                    && type.GetElementType().Implements(typeof(IIdentifiable))
                );
        }

        private static bool IsScalar(this PropertyInfo pi)
        {
            return !pi.PropertyType.Implements(typeof(IEnumerable<>))
                && !pi.PropertyType.IsArray;
        }

        private static string GetClassTemplate()
        {
            var asm = Assembly.GetExecutingAssembly();
            return new StreamReader(asm.GetManifestResourceStream("SHS.Sage.Linq.Runtime.IdentifiableProxy.cs_template")).ReadToEnd();
        }

        private static string GetScalarPropertyTemplate()
        {
            var asm = Assembly.GetExecutingAssembly();
            return new StreamReader(asm.GetManifestResourceStream("SHS.Sage.Linq.Runtime.ScalarProperty.cs_template")).ReadToEnd();
        }

        private static string GetEnumerablePropertyTemplate()
        {
            var asm = Assembly.GetExecutingAssembly();
            return new StreamReader(asm.GetManifestResourceStream("SHS.Sage.Linq.Runtime.EnumerableProperty.cs_template")).ReadToEnd();
        }

    }
}
