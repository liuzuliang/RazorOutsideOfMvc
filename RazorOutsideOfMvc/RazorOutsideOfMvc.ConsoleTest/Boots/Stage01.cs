using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RazorOutsideOfMvc.ConsoleTest.Boots
{
    public class Stage01
    {
        protected internal const string DynamicTemplateNamespace = "CompiledRazorTemplates.Dynamic";

        private static string GetClassFullName(string className)
        {
            return String.Format("{0}.{1}", DynamicTemplateNamespace, className);
        }

        public static void Test()
        {
            // points to the local path
            RazorProjectFileSystem fs = RazorProjectFileSystem.Create(".");

            // customize the default engine a little bit
            RazorProjectEngine engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                InheritsDirective.Register(builder);
                builder.SetNamespace(DynamicTemplateNamespace); // define a namespace for the Template class
            });

            // get a razor-templated file. My "hello.txt" template file is defined like this:
            //
            // @inherits RazorLanguage3TestDemo.MyTemplate
            // Hello @Model.Name, welcome to Razor World!
            //

            string basePath = Directory.GetCurrentDirectory();
            string razorRelativePath = @"RazorFiles\hello.txt";
            string razorFullPath = Path.Combine(basePath, razorRelativePath);
            RazorProjectItem item =
                fs.GetItem(razorFullPath);
            //fs.GetItem(razorRelativePath);

            // parse and generate C# code, outputs it on the console
            //var cs = te.GenerateCode(item);
            //Console.WriteLine(cs.GeneratedCode);

            RazorCodeDocument codeDocument = engine.Process(item);
            RazorCSharpDocument cs = codeDocument.GetCSharpDocument();

            // now, use roslyn, parse the C# code
            SyntaxTree tree = CSharpSyntaxTree.ParseText(cs.GeneratedCode);

            // define the dll
            const string dllName = "hello";
            var compilation = CSharpCompilation.Create(dllName, new[] { tree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // include corlib
                    MetadataReference.CreateFromFile(typeof(RazorCompiledItemAttribute).Assembly.Location), // include Microsoft.AspNetCore.Razor.Runtime
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location), // this file (that contains the MyTemplate base class)

                    // for some reason on .NET core, I need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")),

                    // as found out by @Isantipov, for some other reason on .NET Core for Mac and Linux, we need to add this... this is not needed with .NET framework
                    MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "netstandard.dll"))
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)); // we want a dll


            // compile the dll
            string path = Path.Combine(Path.GetFullPath("."), dllName + ".dll");
            var result = compilation.Emit(path);
            if (!result.Success)
            {
                Console.WriteLine(string.Join(Environment.NewLine, result.Diagnostics));
                Console.ReadLine();
                return;
            }

            // load the built dll
            Console.WriteLine(path);
            var asm = Assembly.LoadFile(path);
            
            // the generated type is defined in our custom namespace, as we asked. "Template" is the type name that razor uses by default.
            var template = (MyTemplate)Activator.CreateInstance(asm.GetType(GetClassFullName("Template")));

            // run the code.
            // should display "Hello Killroy, welcome to Razor World!"
            template.ExecuteAsync().Wait();


        }

        private static void Test2(string code)
        {
            RazorSourceDocument item = RazorSourceDocument.Create(code, Encoding.UTF8, new RazorSourceDocumentProperties
            {
                
            });

        }
    }
}
