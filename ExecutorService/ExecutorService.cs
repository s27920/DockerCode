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
        var guid = Guid.NewGuid();
        var path = $"client-src/{usedLang}/{guid.ToString()}.js";
        string[] arr = { usedLang, guid.ToString() };
        var shRunArgs = string.Join(" ", arr.Select(arg => $"\"{arg}\""));
        File.WriteAllText(path, code);
        Console.WriteLine("write complete");

        var fileContests = File.ReadAllText(path);

        ValidateFileContents(path);
        InsertTestCases(path);
        
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
        File.Delete(path);
        execProcess.WaitForExit();
        
        var output = execProcess.StandardOutput.ReadToEnd();
        var error = execProcess.StandardError.ReadToEnd();
        return new ExecuteResultDto(output, error);
    }

    private void ValidateFileContents(string path)
    {
        Console.WriteLine("checking file validity");

    }

    private void InsertTestCases(string path)
    {
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

        EnumerateTestCases(testCases);
    }

    static IEnumerable<string[]> EnumerateTestCases(string testCases)
    {
        for (var i = 0; i < testCases.Length;)
        {
            var endOfTest = testCases.IndexOf('<', i);
            var str1 = testCases.Substring(i, endOfTest - i);
            i = endOfTest + 2;

            var endOfCorr = testCases.IndexOf("<<", i, StringComparison.Ordinal);
            var str2 = testCases.Substring(i, endOfCorr - i);
            i = endOfCorr + 3;
            yield return [str1, str2];
        }
    }
}