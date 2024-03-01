// See https://aka.ms/new-console-template for more information

using MostModifiedMethodFinder;

Console.WriteLine("Hello, World!");


string filePath = "TwitchEverywhereCLI/TwitchConnection.cs";
string repoPath = "C:\\Users\\uzi\\RiderProjects\\TwitchEverywhere";

CommitFinder commitFinder = new( repoPath );
RoslynIntegration roslynIntegration = new();
Dictionary<string, long> methodChanges = new();

List<string> commitShas = commitFinder.GetCommitShas(filePath);

for (int i = 0; i < commitShas.Count - 1; i++)
{
    var oldContent = commitFinder.GetFileContentAtCommit(filePath, commitShas[i]);
    var newContent = commitFinder.GetFileContentAtCommit(filePath, commitShas[i + 1]);
    
    var oldTree = roslynIntegration.ParseToSyntaxTree(oldContent);
    var newTree = roslynIntegration.ParseToSyntaxTree(newContent);
    
    var changedMethods = roslynIntegration.GetDetailedChanges(oldTree, newTree);
    
    foreach (var method in changedMethods)
    {
        Console.WriteLine($"Changed method: {method} between commits {commitShas[i]} and {commitShas[i + 1]}");
        if (methodChanges.ContainsKey(method))
        {
            methodChanges[method]++;
        }
        else
        {
            methodChanges[method] = 1;
        }
    }
}

foreach (var methodChange in methodChanges) {
    Console.WriteLine($"{methodChange.Key}: {methodChange.Value}");
}