using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SHS.Platform.Compilation
{
    public static class CSharpCompiler
    {
        public static CompilationResponse Compile(CompilationRequest request)
        {
            if (!Path.IsPathRooted(request.OuputPath))
                request.OuputPath = Path.Combine(Path.GetDirectoryName(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), request.OuputPath);

            // that's all we need to do with the template, now just compile it and return the new type
            CodeDomProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            // setup the basic compiler options
            compilerParams.CompilerOptions = "/target:library";
            compilerParams.GenerateExecutable = false;

            Directory.CreateDirectory(request.OuputPath);

            compilerParams.OutputAssembly = Path.Combine(request.OuputPath, String.Format("{0}.dll", request.AssemblyName));
            compilerParams.GenerateInMemory = !request.Debuggable;
            compilerParams.IncludeDebugInformation = request.Debuggable;
            compilerParams.TempFiles = new TempFileCollection(request.OuputPath, request.Debuggable);

            // add references to external assemblies
            compilerParams.ReferencedAssemblies.Add(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            string caller = Assembly.GetCallingAssembly().CodeBase;
            AddReference(compilerParams, caller);

            if (request.References != null)
            {
                foreach (string reference in request.References)
                    AddReference(compilerParams, reference);
            }

            // compile the source code
            CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, request.CompleteCode());
            var response = new CompilationResponse(request.OuputPath, request.References, request.Debuggable, request.CompilationUnits, results.CompiledAssembly, results.Errors);
            return response;
        }

        private static void AddReference(CompilerParameters compilerParams, string reference)
        {
            try
            {
                Assembly asm = null;
                if (Path.GetExtension(reference).Equals(".dll", StringComparison.InvariantCultureIgnoreCase)
                    || Path.GetExtension(reference).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    asm = Assembly.LoadFrom(reference);
                }
                else
                {
                    try
                    {
                        asm = Assembly.LoadFrom(reference + ".dll");
                    }
                    catch
                    {
                        asm = Assembly.LoadFrom(reference + ".exe");
                    }
                }
                Uri path = new Uri(asm.CodeBase);
                if (!compilerParams.ReferencedAssemblies.Contains(asm.ManifestModule.Name.ToLowerInvariant())
                    && !compilerParams.ReferencedAssemblies.Contains(path.LocalPath))
                    compilerParams.ReferencedAssemblies.Add(path.LocalPath);
            }
            catch
            {
                compilerParams.ReferencedAssemblies.Add(reference);
            }
        }
    }


    public class CompilationUnit
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="sourceCode">the source code which can consist of 1 namespace containing 1 - N Types</param>
        public CompilationUnit(string sourceCode)
        {
            var nsReg = new Regex(@"namespace\s+(?<ns>.+)");
            var classReg = new Regex(@"class\s+(?<class>.+)");
            this.Code = sourceCode;
            var m = nsReg.Match(sourceCode);
            var ns = "";
            if (m.Success)
            {
                ns = m.Groups["ns"].Value;
            }

            var types = new List<string>();
            m = classReg.Match(sourceCode);
            while(m.Success)
            {
                types.Add(m.Groups["class"].Value);
                m = m.NextMatch();
            }
            this.TypeNames = types.ToArray();
        }
        public string Code { get; private set; }
        public IEnumerable<string> TypeNames { get; private set; }
    }

    public class CompilationRequest
    {
        public static string[] STANDARD_REFERENCES = new string[]
        {
            "mscorlib.dll",
            "system.dll",
            "system.core.dll",
            "system.data.dll",
            "system.xml.dll",
            "system.xml.linq.dll",
            "microsoft.csharp.dll",
            "system.linq.expressions.dll",
            "system.runtime.dll"
        };
        public CompilationRequest(string outputPath, string[] references, bool debuggable = true, params CompilationUnit[] compilationUnits)
        {
            this.OuputPath = outputPath;
            this.References = references;
            this.Debuggable = debuggable;
            this.CompilationUnits = compilationUnits;
            this.AssemblyName = "TempAssembly";
        }

        public string OuputPath { get; set; }
        public string[] References { get; set; }
        public bool Debuggable { get; set; }
        public CompilationUnit[] CompilationUnits { get; set; }
        public string AssemblyName { get; set; }

        public string CompleteCode()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var code in CompilationUnits.Select(c => c.Code))
            {
                sb.AppendLine(code);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    public class CompilationResponse : CompilationRequest
    {
        public CompilationResponse(string outputPath, string[] references, bool debuggable, CompilationUnit[] compilationUnits, Assembly assembly = null, CompilerErrorCollection errors = null)
            : base(outputPath, references, debuggable, compilationUnits)
        {
            this.Errors = errors;
            this.Assembly = assembly;
        }
        public Assembly Assembly { get; private set; }
        public CompilerErrorCollection Errors { get; private set; }
        public bool HasErrors { get { return Errors != null && Errors.HasErrors; } }
    }

    public static class CompilerErrorCollectionEx
    {
        public static string ToErrorString(this CompilerErrorCollection errors)
        {
            // log failure
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < errors.Count; i++)
            {
                // LogError(results.Errors[i].ErrorText);
                sb.AppendLine(String.Format("Error {0}: {1}, Line: {2}, Column: {3}",
                    errors[i].ErrorNumber,
                    errors[i].ErrorText,
                    errors[i].Line,
                    errors[i].Column));
            }
            return sb.ToString();
        }
    }
}
