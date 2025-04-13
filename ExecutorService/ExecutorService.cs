using System.Text.RegularExpressions;

namespace ConsoleApp7.ExecutorService;

using System.Diagnostics;

public class ExecutorService
{
    private string[] Langs { get; set; }

    public ExecutorService(string[] langs)
    {
        Langs = langs;
    }

    public void BuildImages()
    {
        var shBuildArgs = string.Join(" ", Langs.Select(arg => $"\"{arg}\""));
        Console.WriteLine("building images...");
        var buildProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = $"\"./scripts/build-images.sh\" {shBuildArgs}",
            }
        };
        buildProcess.Start();
        buildProcess.WaitForExit();
        Console.WriteLine("build complete");
    }

    public ExecuteResultDto Execute(string usedLang, string code)
    {
        string funcName = GetFuncSignature();
        
        if (!ValidateFileContents(code, funcName))
        {
            return new ExecuteResultDto("","Code not executed, incorrect function signature");
        }

        var guid = Guid.NewGuid();
        var path = $"client-src/{usedLang}/{guid.ToString()}.js";
        string[] arr = { usedLang, guid.ToString() };
        var shRunArgs = string.Join(" ", arr.Select(arg => $"\"{arg}\""));

        File.WriteAllText(path, code);
        
        Console.WriteLine($"\n==================written==================\n\n{code}\n\n==================end==================\n");
        
        var fileContests = File.ReadAllText(path);
        InsertTestCases(path, funcName);
        
        Console.WriteLine($"==================executed==================\n\n{File.ReadAllText(path)}\n\n==================end==================\n");
        
        Console.WriteLine($"\"./scripts/deploy-executor-container.sh\" {shRunArgs}");
        
        var execProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = $"\"./scripts/deploy-executor-container.sh\" {shRunArgs}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        execProcess.StandardInput.Write(fileContests);
        execProcess.StandardInput.Close();
        execProcess.WaitForExit();
        File.Delete(path);

        var output = execProcess.StandardOutput.ReadToEnd();
        var error = execProcess.StandardError.ReadToEnd();
        return new ExecuteResultDto(output, error);
    }

    private bool ValidateFileContents(string code, string funcName /*temporary*/)
    {
        //source for the regex pattern: https://stackoverflow.com/a/58278733
        string matcher = $@"^(?:[\s]+)?(?:const|let|var|)?(?:[a-z0-9.]+(?:\.prototype)?)?(?:\s)?(?:[a-z0-9-_]+\s?=)?\s?(?:[a-z0-9]+\s+\:\s+)?(?:function\s?)?{funcName}\s?\(.*\)\s?(?:.+)?([=>]:)?\{{(?:(?:[^}}{{]+|\{{(?:[^}}{{]+|\{{[^}}{{]*\}})*\}})*\}}(?:\s?\(.*\)\s?\)\s?)?)?(?:\;)?";
        var regex = new Regex(matcher);
        var match = regex.Match(code);
        return match.Success;

    }

    private void InsertTestCases(string path, string funcName)
    {
        // TODO fetch test cases, for now hardcoded
        /*
         proposed test case format
         test data<
         expected output<<
         test data<
         expected output<<
         ...
         Could also have them all in one line beats me, less space but less readable.
         Furthermore enumerateTestCases would offset by 1 and 2 respectively instead of 2 and 3
         */
        var testCases = "[1,5,2,4,3]<\n" +
                   "[1,2,3,4,5]<<\n" +
                   "[94,37,9,52,17]<\n" +
                   "[9,17,37,52,94]<<\n" ;

        using (var fileWriter = new StreamWriter(path, true))
        {
            foreach (var testCase in EnumerateTestCases(testCases))
            {
                fileWriter.WriteLine(PrintComparingStatement(testCase, funcName));
            }
        }
    }

    private string GetFuncSignature()
    {
        // TODO fetch and parse file template, for now hardcoded
        string funcName = "func";
        return funcName;
    }

    private string GetComparingStatement(TestCase testCase, string funcName)
    {
        return $"JSON.stringify({testCase.ExpectedOutput}) === JSON.stringify({funcName}({testCase.TestInput}))";
    }

    private string PrintComparingStatement(TestCase testCase, string funcName)
    {
        return $"\nconsole.log({GetComparingStatement(testCase, funcName)});";
    }

    IEnumerable<TestCase> EnumerateTestCases(string testCases)
    {
        for (var i = 0; i < testCases.Length;)
        {
            var endOfTest = testCases.IndexOf('<', i);
            var str1 = testCases.Substring(i, endOfTest - i);
            i = endOfTest + 2;

            var endOfCorr = testCases.IndexOf("<<", i, StringComparison.Ordinal);
            var str2 = testCases.Substring(i, endOfCorr - i);
            i = endOfCorr + 3;
            yield return new TestCase(str1, str2);
        }
    }

    internal record TestCase(string TestInput, string ExpectedOutput) { }
}