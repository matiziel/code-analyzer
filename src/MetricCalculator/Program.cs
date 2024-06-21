// See https://aka.ms/new-console-template for more information

using MetricCalculator;

var projectPath = "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj";

var calculator = new MethodMetricCalculator();
var metrics = await calculator.Calculate(projectPath);

foreach (var metric in metrics) {
    Console.WriteLine(metric.ToString());
}