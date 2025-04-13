using System.Diagnostics;

namespace ConsoleApp7;

public class Program
{
    public static void Main(string[] args)
    {
        var lang = "node";
        var code = "console.log(\"Hello World!\")";
        var guid = Guid.NewGuid();
        var path = $"client-src/{lang}/{guid.ToString()}.js";
        
        var buildProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "docker",
                Arguments = "build -t node-executor -f executor-images/node-image.dockerfile .",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        buildProcess.Start();
        buildProcess.WaitForExit();
        Console.WriteLine("build complete");
        File.WriteAllText(path, code);

        var execProcess = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "docker", //--read-only
                Arguments = $"run -i --rm --name node{guid} --memory 256m --cpus 0.5 {lang}-executor",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        execProcess.Start();
        execProcess.StandardInput.Write(File.ReadAllText(path));
        execProcess.StandardInput.Close();
        execProcess.WaitForExit();

        var output = execProcess.StandardOutput.ReadToEnd();
        var error = execProcess.StandardError.ReadToEnd();  // Add this

        Console.WriteLine($"Output: {output}");
        Console.WriteLine($"Error: {error}");
        File.Delete(path);
    }
}