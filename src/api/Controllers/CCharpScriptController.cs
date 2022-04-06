using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using dotnetcore_apis.Tools;
using DotNetCoreApis.Models;
using DotNetCoreApis.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;


namespace DotNetCoreApis.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CSharpScriptController : ControllerBase
    {
        private readonly ILogger _logger = null;
        public CSharpScriptController(ILogger<FileController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Download example of the code that can be uploaded for execution
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="406">Invalid Parameter Value</response>
        /// <response code="400">Invalid Parameter Format</response>
        /// <response code="500">Internal Error</response>
        [HttpGet("downloadSampleCSharpCode")]
        public async Task<FileStreamResult> DownloadSampleCSharpCode()
        {
            await Task.Delay(0);
            try
            {
                MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes("" +
                    "using System;" + Environment.NewLine +
                    "const string s = $\"Hello Dynamic C# Code\";" + Environment.NewLine +
                    "string hello = $\"{ s } from { args[0]}!\";" + Environment.NewLine +
                    "Console.WriteLine(hello);" + Environment.NewLine +
                    ""));
                return new FileStreamResult(stream, "text/plain")
                {
                    FileDownloadName = "dynamic_code.cs"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unknown Exception. Type: {ex.GetType().ToString()} ; Message: {ex.Message} ; Details: {ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// Run C# Code that is uploaded
        /// </summary>
        /// <param name="files"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        /// <response code="500">Internal Error</response>
        /// <response code="413">File too Large</response>
        /// <response code="400">Did not receive any file</response>

        [HttpPost("runCSharpCode")]
        public ActionResult<List<FileUploadResponseModel>> RunCSharpCode([FromForm] IFormFileCollection files, string parameter = "Param_01")
        {
            List<FileUploadResponseModel> savedProperties = null;
            string uploadDir = Path.Combine(Path.GetTempPath(), "dotnet_apis", "csharp_script", "upload_code_files");
            if (!FileTools.SaveFilesToDisk(uploadDir, files, out savedProperties))
                return Ok("Could not upload file");

            byte[] compiled = Compile(savedProperties[0].uploadPath);
            ExecuteCompiledCode(compiled, new[] { parameter });

            return Ok(savedProperties);
        }

        private byte[] Compile(string filepath)
        {
            _logger.LogInformation($"Starting compilation of: '{filepath}'");
            string sourceCode = System.IO.File.ReadAllText(filepath);
            using (MemoryStream peStream = new MemoryStream())
            {
                Microsoft.CodeAnalysis.Emit.EmitResult result = GenerateCode(sourceCode).Emit(peStream);
                if (!result.Success)
                {
                    _logger.LogError("Compilation done with error.");

                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        _logger.LogError("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return null;
                }

                _logger.LogInformation("Compilation done without any error.");
                peStream.Seek(0, SeekOrigin.Begin);
                return peStream.ToArray();
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            SourceText codeString = SourceText.From(sourceCode);
            CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.LatestMajor);
            SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            };

            return CSharpCompilation.Create("Hello.dll",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private void ExecuteCompiledCode(byte[] compiledAssembly, string[] args)
        {
            WeakReference assemblyLoadContextWeakRef = LoadAndExecute(compiledAssembly, args);

            for (int i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Console.WriteLine(assemblyLoadContextWeakRef.IsAlive ? "Unloading failed!" : "Unloading success!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference LoadAndExecute(byte[] compiledAssembly, string[] args)
        {
            using (MemoryStream asm = new MemoryStream(compiledAssembly))
            {
                SimpleUnloadableAssemblyLoadContext assemblyLoadContext = new SimpleUnloadableAssemblyLoadContext();

                System.Reflection.Assembly assembly = assemblyLoadContext.LoadFromStream(asm);

                System.Reflection.MethodInfo entry = assembly.EntryPoint;

                _ = entry != null && entry.GetParameters().Length > 0
                    ? entry.Invoke(null, new object[] { args })
                    : entry.Invoke(null, null);

                assemblyLoadContext.Unload();

                return new WeakReference(assemblyLoadContext);
            }
        }

    }
}
