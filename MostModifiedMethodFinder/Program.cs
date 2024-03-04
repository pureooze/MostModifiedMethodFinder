// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using MostModifiedMethodFinder;

Console.WriteLine("Hello, World!");

IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

IConfiguration config = builder.Build();

string? repoPath = config["RepositoryPath"];
string? filePath = config["FilePath"];

if (string.IsNullOrEmpty(repoPath) || string.IsNullOrEmpty(filePath)) {
    Console.WriteLine("Please provide a repository path and a file path in the appsettings.json file.");
    return;
}

CommitFinder commitFinder = new(repoPath);
RoslynIntegration roslynIntegration = new();
Dictionary<string, long> methodChanges = new();

List<string> commitShas = commitFinder.GetCommitShas(filePath);

for (int i = 0; i < commitShas.Count - 1; i++) {
    string oldContent = await commitFinder.GetFileContentAtCommitAsync(filePath, commitShas[i]);
    string newContent = await commitFinder.GetFileContentAtCommitAsync(filePath, commitShas[i + 1]);
    
    SyntaxTree oldTree = roslynIntegration.ParseToSyntaxTree(oldContent);
    SyntaxTree newTree = roslynIntegration.ParseToSyntaxTree(newContent);
    
    IEnumerable<string> changedMethods = roslynIntegration.GetDetailedChanges(oldTree, newTree);
    
    foreach (string method in changedMethods) {
        Console.WriteLine($"Changed method: {method} between commits {commitShas[i]} and {commitShas[i + 1]}");
        if (methodChanges.ContainsKey(method)) {
            methodChanges[method]++;
        } else {
            methodChanges[method] = 1;
        }
    }
}

foreach (var methodChange in methodChanges.OrderBy(m => m.Value)) {
    Console.WriteLine($"{methodChange.Key}: {methodChange.Value}");
}