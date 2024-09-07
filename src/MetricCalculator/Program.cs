// See https://aka.ms/new-console-template for more information

using MetricCalculator;

var projectPaths = new [] {
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/gitextensions-a866c36b3948dbdddc5e62ade639edc79603a81d/GitExtensions.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/ScreenToGif-2d318f837946f730e1b2e5c708ae9f776b9e360b/GifRecorder.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/osu-2cac373365309a40474943f55c56159ed8f9433c/osu.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/OpenRA-920d00bbae9fa8e62387bbff705ca4bea6a26677/OpenRA.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Sonarr-6378e7afef6072eae20f6408818c6fb1c85661b7/Sonarr.sln",
    "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/jellyfin-6c2eb5fc7e872a29b4a0951849681ae0764dbb8e/MediaBrowser.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/mRemoteNG-e6d2c9791d7a5e55630c987a3c81fb287032752b/mRemoteNG.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Files-89c33841813a5590e6bf44fb02bb7d06348320c3/Files.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Newtonsoft.Json-52e257ee57899296d81a868b32300f0b3cfeacbe/Src/Newtonsoft.Json.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Ryujinx-81e9b86cdb4b2a01cc41b8e8a4dff2c9e3c13843/Ryujinx.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/duplicati-0a1b32e1887c98c6034c9fafdfddcb8f8f31e207/Duplicati.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Jackett-db695e5dc01755ff52b5cd7b4f0004ff1035649d/src/Jackett.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/ShareX-c9a71ed00eda0e7c5a45237b9bcd3f8f614cda63/ShareX.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/BurningKnight-a55594c11ab681087356af2c129c2d493eba4bd2/Lens.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/LiteDB-00d28bfafe3c685ae239f759f812def495278eaf/LiteDB.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/Radarr-5ce1829709e7e1de3953c04e5dab4f3a9d450b38/src/Radarr.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/ImageSharp-5a7c1f4b2f2f96ccdc38a99d5130b3326d3958fb/ImageSharp.sln",
    // "/home/mateusz/Documents/School/MGR/ProjectsToAnalize/ml-agents-cbc1993c6235cdd033754f9659561e840d4b8708",
    // "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj",
};

var calculator = new ClassMetricCalculator();

var annotations = CsvFileHelper.ReadFinalAnnotation("/home/mateusz/Documents/School/MGR/data/DataSet_Refused_Bequest.csv");

var tasks = projectPaths.Select(t => calculator.Calculate(t, annotations)).ToList();

await Task.WhenAll(tasks);

var metrics = tasks.SelectMany(t => t.Result).ToList();

CsvFileHelper.SaveToFile(metrics, "/home/mateusz/Documents/School/MGR/Results/test_file.csv");



