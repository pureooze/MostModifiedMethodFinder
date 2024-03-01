using System.Diagnostics;

namespace MostModifiedMethodFinder;

public class CommitFinder( string repoPath )
{
    
    public string GetFileContentAtCommit(string filePath, string commitSha)
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
        
        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                return process.StandardOutput.ReadToEnd();
            }
            else
            {
                var errorMessage = process.StandardError.ReadToEnd();
                throw new Exception($"Error retrieving file content: {errorMessage}");
            }
        }
    }
    
    public List<string> GetCommitShas(string filePath)
    {
        var commitShas = new List<string>();
        var startInfo = new ProcessStartInfo("git")
        {
            Arguments = $"log --pretty=format:\"%H\" -- \"{filePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = repoPath
        };
        
        using (var process = Process.Start(startInfo))
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