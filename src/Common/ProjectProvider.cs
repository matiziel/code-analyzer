using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Common;

public class ProjectProvider {

    public static async Task<IEnumerable<Project>> GetFromPath(string path) {
        try {
            if (!File.Exists(path) && !Directory.Exists(path)) {
                Console.WriteLine("Invalid path.");
                return Enumerable.Empty<Project>();
            }

            using var workspace = MSBuildWorkspace.Create();

            if (Directory.Exists(path)) {
                var solution = await workspace.OpenSolutionAsync(path);
                return solution.Projects;
            }
            else {
                var project = await workspace.OpenProjectAsync(path);
                return new List<Project>() { project };
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return Enumerable.Empty<Project>(); 
        }
    }
}