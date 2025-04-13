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
        Console.WriteLine("inserting test cases");
    }
}