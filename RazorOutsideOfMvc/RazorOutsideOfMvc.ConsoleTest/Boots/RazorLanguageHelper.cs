using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RazorOutsideOfMvc.ConsoleTest.Boots
{
    /// <summary>
    /// Razor Language Helper
    /// </summary>
    public static class RazorLanguageHelper
    {
        /// <summary>
        /// Getting the generated code through Microsoft.AspNetCore.Razor.Language
        /// </summary>
        /// <param name="dynamicTemplateNamespace"></param>
        /// <param name="templateSourceCode"></param>
        /// <param name="generatedCSharpClassName"></param>
        /// <param name="classBaseType"></param>
        /// <param name="templateFile"></param>
        /// <returns></returns>
        public static string GetGeneratedCode(string dynamicTemplateNamespace,
            string templateSourceCode,
            string generatedCSharpClassName,
            string classBaseType,
            string templateFile = null)
        {

            string systemPath = Directory.GetCurrentDirectory();
            string path = null;
            if (string.IsNullOrWhiteSpace(templateFile))
            {
                path = systemPath;
            }
            else
            {
                path = Path.GetDirectoryName(templateFile);
            }
            RazorProjectFileSystem fs = RazorProjectFileSystem.Create(path); // or '.'
            RazorProjectEngine engine = RazorProjectEngine.Create(RazorConfiguration.Default, fs, (builder) =>
            {
                InheritsDirective.Register(builder);
                FunctionsDirective.Register(builder);
                SectionDirective.Register(builder);
                builder.ConfigureClass((document, @class) =>
                {
                    @class.ClassName = generatedCSharpClassName;
                });
                builder.SetNamespace(dynamicTemplateNamespace); // define a namespace for the Template class
                builder.SetBaseType(classBaseType);
            });
            string razorRelativePath;
            string randomRazorFileFullPath = null;
            if (string.IsNullOrEmpty(templateFile))
            {
                razorRelativePath = Path.GetRandomFileName();
                randomRazorFileFullPath = Path.Combine(systemPath, razorRelativePath);
                File.AppendAllText(randomRazorFileFullPath, templateSourceCode ?? string.Empty, System.Text.Encoding.UTF8);
            }
            else
            {
                razorRelativePath = templateFile;
            }
            RazorProjectItem item = fs.GetItem(razorRelativePath);
            RazorCodeDocument codeDocument = engine.Process(item);
            RazorCSharpDocument cs = codeDocument.GetCSharpDocument();
            if (!string.IsNullOrEmpty(randomRazorFileFullPath))
            {
                try
                {
                    File.Delete(randomRazorFileFullPath);
                }
                catch (Exception) { }
            }
            var razorCompiledItemAssembly = typeof(RazorCompiledItemAttribute).Assembly;
            //手动加载程序集，防止编译 Razor 类时找不到 DLL
            return cs.GeneratedCode;
        }
    }
}
