using System.Globalization;
using CsvHelper;

namespace MetricCalculator;

public class CsvFileHelper {
    public static void SaveToFile<T>(IEnumerable<T> metrics, string path) where T : IMetric {
        if (!metrics.Any()) {
            return;
        }

        using var writer = new StreamWriter(path);

        writer.WriteLine(metrics.First().GetHeadline());

        foreach (var metric in metrics) {
            writer.WriteLine(metric.ToString());
        }
    }

    public static Dictionary<string, int> ReadFinalAnnotation(
        string csvFilePath,
        string className = "Code Snippet ID",
        string annotationName = "Final annotation") {
        var annotations = new Dictionary<string, int>();

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        csv.Read();
        csv.ReadHeader();
        
        while (csv.Read()) {
            if (!csv.TryGetField(className, out string @class)) {
                continue;
            }

            if (!csv.TryGetField(annotationName, out int annotation)) {
                annotation = 0;
            } 

            // var splitedName = @class.Split(".", StringSplitOptions.RemoveEmptyEntries);
            // annotations.TryAdd(splitedName[^1], annotation);
            
            annotations.TryAdd(@class, annotation);
        }
        
        return annotations;
    }
}