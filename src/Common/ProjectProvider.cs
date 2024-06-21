using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Common;

public class ProjectProvider {

    public static async Task<IEnumerable<Project>> GetFromPath(string path) {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            Console.WriteLine("Invalid path.");
            return Enumerable.Empty<Project>();
        }

        using var workspace = MSBuildWorkspace.Create();

        if (Directory.Exists(path))
        {
            var solution = await workspace.OpenSolutionAsync(path);
            return solution.Projects;
        }
        else
        {
            return [ await workspace.OpenProjectAsync(path) ];
        }
    }
}