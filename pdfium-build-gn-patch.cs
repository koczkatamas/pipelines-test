using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class PrepareBuild
{
    private static bool errorHappened = false;

    static void Replace(ref string content, string fromRegex, string to, string filename = null)
    {
        var replacedContent = Regex.Replace(content, fromRegex, to, RegexOptions.Singleline);
        if (replacedContent == content)
        {
            Console.WriteLine("[ERROR] Failed to replace regex '" + fromRegex + "' to '" + to + "'" + (filename != null ? " in file '" + filename + "'" : ""));
            errorHappened = true;
        }
        content = replacedContent;
    }
    
    static void ModifyFile(string fn, string fromRegex, string to)
    {
        Console.WriteLine("[START] Modifying "+fn+"...");

        var fileContent = File.ReadAllText(fn);

        Replace(ref fileContent, fromRegex, to, fn);

        File.WriteAllText(fn, fileContent);

        Console.WriteLine("[END] Modifying "+fn);
    }
    
    static void Main(string[] args)
    {
        try
        {
            ModifyFile("BUILD.gn", @"}\n$", @"}

shared_library(""pdfium_dll"") {
  defines = [""FPDF_IMPLEMENTATION"", ""COMPONENT_BUILD""]
  deps = ["":pdfium""]
  sources = [""fpdfsdk/pdfiumviewer.cpp"", ""fpdfsdk/pdfium.rc""]
  output_name = ""pdfium""
}");
    
            ModifyFile("fpdfsdk/BUILD.gn", @"(jumbo_source_set..fpdfsdk[^\n]*)", "$1\n  defines = [\"FPDF_IMPLEMENTATION\", \"COMPONENT_BUILD\"]\n");
    
            //ModifyFile(@"third_party\BUILD.gn", @"_H=<(.*?)>", "_H=\\\"$1\\\"");
            
            //ModifyFile(@"build\config\win\BUILD.gn", @"NTDDI_VERSION=0x0A000000", @"NTDDI_VERSION=0x06000000");
            //ModifyFile(@"build\config\win\BUILD.gn", @"_WIN32_WINNT=0x0A00", @"_WIN32_WINNT=0x0600");
            //ModifyFile(@"build\config\win\BUILD.gn", @"WINVER=0x0A00", @"WINVER=0x0600");
            
            if (errorHappened)
                Environment.Exit(1);
        }
        catch (Exception e)
        {
            Console.WriteLine("[Exception] " + e);
            Environment.Exit(2);
        }
    }
}