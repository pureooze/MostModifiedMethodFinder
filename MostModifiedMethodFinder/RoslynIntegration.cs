using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MostModifiedMethodFinder;

public class RoslynIntegration
{
    public SyntaxTree ParseToSyntaxTree(string fileContent)
    {
        return CSharpSyntaxTree.ParseText(fileContent);
    }
    
    public IEnumerable<string> GetDetailedChanges(SyntaxTree oldTree, SyntaxTree newTree)
    {
        var oldRoot = oldTree.GetRoot();
        var newRoot = newTree.GetRoot();
        
        var oldMethods = oldRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToDictionary(m => m.Identifier.ValueText + m.ParameterList, m => m.ToString());
        var newMethods = newRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToDictionary(m => m.Identifier.ValueText + m.ParameterList, m => m.ToString());
        
        var allMethodKeys = oldMethods.Keys.Union(newMethods.Keys).Distinct();
        
        var changedMethods = new List<string>();
        
        foreach (var key in allMethodKeys)
        {
            oldMethods.TryGetValue(key, out var oldMethodBody);
            newMethods.TryGetValue(key, out var newMethodBody);
            
            if (!string.Equals(oldMethodBody, newMethodBody))
            {
                changedMethods.Add(key.Split('(').First()); // Extract method name without parameters for reporting
            }
        }
        
        return changedMethods.Distinct();
    }
}