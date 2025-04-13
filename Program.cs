using System.Diagnostics;
using ConsoleApp7.ExecutorService;

namespace ConsoleApp7;

public class Program
{
    public static void Main(string[] args)
    {
        // example data
        string[] langs = { "node" };
        var usedLang = "node";
        
        // TODO get from controller, for now hardcoded
        var code = "function func(arr){\n    return arr.sort((a, b) => a - b);\n}";
        
        var executorService = new ExecutorService.ExecutorService(langs);
        
        executorService.BuildImages();
        
        var resultDto = executorService.Execute(usedLang, code);
        
        Console.WriteLine($"Output: {resultDto.stdOut}");
        Console.WriteLine($"Error: {resultDto.stdErr}");
    }
}