namespace MetricCalculator;

public class MethodMetrics {
    public string MethodName { get; set; }
    public int Cyclo { get; set; } // Cyclomatic Complexity
    public int CycloSwitch { get; set; } // Cyclomatic Complexity Without Cases
    public int Mloc { get; set; } // Method Lines of Code
    public int Meloc { get; set; }  // Method Effective Lines of Code
    public int Nop { get; set; } // Number of Parameters
    public int Nolv { get; set; } // Number of Local Variables
    public int Notc { get; set; } // Number of Try-Catch Blocks
    public int Mnol { get; set; } // Method Number of Loops
    public int Mnor { get; set; } // Method Number of Returns
    public int Mnoc { get; set; } // Method Number of Comparison Operators
    public int Mnoa { get; set; } // Method Number of Assignments
    public int Nonl { get; set; } // Number of Numeric Literams
    public int Nosl { get; set; } // Number of String Literals
    public int Nomo { get; set; } // Number of Math Operations
    public int Nope { get; set; } // Number of Parenthesized Expressions
    public int Nole { get; set; } // Number of Lambda Expressions
    public int Mmnb { get; set; } // Maximum Nesting Block
    public int Nouw { get; set; } // Unique words
    public double Aid { get; set; } // Foreign data access

    public override string ToString() {
        return $"Method: {MethodName}, CYCLO: {Cyclo}, CYCLO_SWITCH: {CycloSwitch}, MLOC: {Mloc}, MELOC: {Meloc}, " +
               $"NOP: {Nop}, NOLV: {Nolv}, NOTC: {Notc}, MNOL: {Mnol}, MNOR: {Mnor}, MNOC: {Mnoc}, MNOA: {Mnoa}, " +
               $"NONL: {Nonl}, NOSL: {Nosl}, NOMO: {Nomo}, NOPE: {Nope}, NOLE: {Nole}, MMNB: {Mmnb}, NOUW: {Nouw}, AID: {Aid}";
    }
}