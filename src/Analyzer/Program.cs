// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using Analyzer;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;


var projectPaths = new string[] {
    // "/home/mateusz/Documents/School/MGR/project-to-analize/gitextensions-a866c36b3948dbdddc5e62ade639edc79603a81d/GitExtensions.sln", // error
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Newtonsoft.Json-52e257ee57899296d81a868b32300f0b3cfeacbe/Src/Newtonsoft.Json.sln", // error
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Ryujinx-81e9b86cdb4b2a01cc41b8e8a4dff2c9e3c13843/Ryujinx.sln", // error
    // "/home/mateusz/Documents/School/MGR/project-to-analize/ScreenToGif-2d318f837946f730e1b2e5c708ae9f776b9e360b/ScreenToGif/ScreenToGif.csproj", // too long
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Files-89c33841813a5590e6bf44fb02bb7d06348320c3/Files.sln", // too long 
    // "/home/mateusz/Documents/School/MGR/project-to-analize/osu-2cac373365309a40474943f55c56159ed8f9433c/osu.sln", // too long
    // "/home/mateusz/Documents/School/MGR/project-to-analize/duplicati-0a1b32e1887c98c6034c9fafdfddcb8f8f31e207/Duplicati.sln", // too long
    // "/home/mateusz/Documents/School/MGR/project-to-analize/mRemoteNG-e6d2c9791d7a5e55630c987a3c81fb287032752b/mRemoteNG.sln", // too long
    // "/home/mateusz/Documents/School/MGR/project-to-analize/ShareX-c9a71ed00eda0e7c5a45237b9bcd3f8f614cda63/ShareX.sln", // too long
    // "/home/mateusz/Documents/School/MGR/project-to-analize/BurningKnight-a55594c11ab681087356af2c129c2d493eba4bd2/Lens.sln", // too long


    // "/home/mateusz/Documents/School/MGR/project-to-analize/OpenRA-920d00bbae9fa8e62387bbff705ca4bea6a26677/OpenRA.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Sonarr-6378e7afef6072eae20f6408818c6fb1c85661b7/src/Sonarr.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/jellyfin-6c2eb5fc7e872a29b4a0951849681ae0764dbb8e/MediaBrowser.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Ryujinx-master/Ryujinx.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Jackett-db695e5dc01755ff52b5cd7b4f0004ff1035649d/src/Jackett.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/LiteDB-00d28bfafe3c685ae239f759f812def495278eaf/LiteDB.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/Radarr-5ce1829709e7e1de3953c04e5dab4f3a9d450b38/src/Radarr.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/ImageSharp-5a7c1f4b2f2f96ccdc38a99d5130b3326d3958fb/ImageSharp.sln",
    // "/home/mateusz/Documents/School/MGR/project-to-analize/ml-agents-cbc1993c6235cdd033754f9659561e840d4b8708",
    // "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj",
    "/home/mateusz/Documents/School/MGR/project-to-analize/Playnite-master/source/Playnite.sln"
};

var diagnostics = new List<Diagnostic>();

foreach (var projectPath in projectPaths) {
    var projects = await ProjectProvider.GetFromPath(projectPath);
    

    if (!projects.Any()) {
        Console.WriteLine("Project not found.");
        return;
    }
    
    foreach (var project in projects) {
        var compilation = await project.GetCompilationAsync();
    
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            // new FeatureEnvyAnalyzer()
            new RefusedBequestAnalyzer()
        );
        
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        
        diagnostics.AddRange(await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()); 
    }
}


foreach (var diagnostic in diagnostics) {
    Console.WriteLine($"{diagnostic.Location}: {diagnostic.GetMessage()}");
}