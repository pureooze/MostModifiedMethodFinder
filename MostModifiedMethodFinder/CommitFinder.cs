using System.Diagnostics;
using System.Text;

namespace MostModifiedMethodFinder;

public class CommitFinder( string? repoPath )
{
    public async Task<string> GetFileContentAtCommitAsync(string filePath, string commitSha)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            Arguments = $"show {commitSha}:{filePath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = repoPath
        };
        
        using (var process = new Process { StartInfo = startInfo })
        {
            var output = new StringBuilder();
            var error = new StringBuilder();
            
            process.OutputDataReceived += (sender, args) => { if (args.Data != null) output.AppendLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { if (args.Data != null) error.AppendLine(args.Data); };
            
            process.Start();
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await Task.Run(() => process.WaitForExit());
            
            if (process.ExitCode == 0)
            {
                return output.ToString();
            }
            else
            {
                throw new Exception($"Error retrieving file content: {error}");
            }
        }
    }
    
    public List<string> GetCommitShas(string? filePath)
    {
        var commitShas = new List<string>();
        var startInfo = new ProcessStartInfo("git")
        {
            Arguments = $"log --since=\"1 year ago\" --pretty=format:\"%H\" -- \"{filePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = repoPath
        };
        
        using (Process? process = Process.Start(startInfo))
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                commitShas.Add(line);
            }
        }
        
        return commitShas;
    }
}