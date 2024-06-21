// See https://aka.ms/new-console-template for more information

using MetricCalculator;

var projectPath = "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj";

var calculator = new MethodMetricCalculator();
calculator.Calculate(projectPath);