namespace ConsoleApp7.ExecutorService;

using System.Diagnostics;

public class ExecutorService
{
    public ExecutorService()
    {
        string[] langs = { "node" };
        var usedLang = "node";
        var code = "console.log(`Hello docker! ${1+1}`)";
        var guid = Guid.NewGuid();
        var path = $"client-src/{usedLang}/{guid.ToString()}.js";
        var shBuildArgs = string.Join(" ", langs.Select(arg => $"\"{arg}\""));
        string[] arr = { usedLang, guid.ToString() };
        var shRunArgs = string.Join(" ", arr.Select(arg => $"\"{arg}\""));

        
        
        
        Console.WriteLine("building images...");
        var buildProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = $"\"./buildImages.sh\" {shBuildArgs}",
            }
        };
        buildProcess.Start();
        buildProcess.WaitForExit();
        Console.WriteLine("build complete");
        File.WriteAllText(path, code);
        Console.WriteLine("write complete");


        var fileContests = File.ReadAllText(path);
        
        var execProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/sh",
                Arguments = $"\"./deployExecutorContainer.sh\" {shRunArgs}",
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

        var output = execProcess.StandardOutput.ReadToEnd();
        var error = execProcess.StandardError.ReadToEnd();

        Console.WriteLine($"Output here: {output}");
        Console.WriteLine($"Error: {error}");
        File.Delete(path);

    }
}