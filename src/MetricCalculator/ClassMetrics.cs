namespace MetricCalculator;

public class ClassMetrics {
    public string ClassName { get; set; }
    public int Cloc { get; set; } // Lines of Code in the class
    public int Celoc { get; set; } // Effective Lines of Code
    public int Nmd { get; set; } // Number of Methods Declared
    public int Nad { get; set; } // Number of Attributes Declared
    public int NmdNad { get; set; } // Sum of Methods and Attributes
    public int Wmc { get; set; } // Weighted Methods per Class
    public int WmcNoCase { get; set; } // Weighted Methods per Class without counting cases
    public int Lcom { get; set; } // Lack of Cohesion of Methods
    public double Lcom3 { get; set; } // Improved LCOM measure
    public double Lcom4 { get; set; } // Further Improved LCOM measure
    public double Tcc { get; set; } // Tight Class Cohesion
    public int Atfd { get; set; } // Access to Foreign Data
    public int Cnor { get; set; } // Number of Return Statements
    public int Cnol { get; set; } // Number of loops
    public int Cnoc { get; set; } // Number of comparison operators
    public int Cnoa { get; set; } // Number of Assignments
    public int Nopm { get; set; } // Number of Public Methods
    public int Nopf { get; set; } // Number of Protected fields
    public int Cmnb { get; set; } // Max nested blocks
    public int Rfc { get; set; } // unique method invocations
    public int Cbo { get; set; } // Number of Dependencies
    public int Dit { get; set; } // Depth of Inheritance Tree
    public double Dcc { get; set; } // Direct Class Coupling
    public int Atfd10 { get; set; } // Access to Foreign Data with threshold 10
    public int Nic { get; set; } // Number of Inner Classes
    public double Woc { get; set; } // Weight of Class (Percentage of Methods that are Public)
    public int Nopa { get; set; } // Number of Public Fields
    public int Nopp { get; set; } // Number of Public Properties
    public int Wmcnamm { get; set; } // WMC Not Counting Accessor Methods
    public double Bur { get; set; } // BUR Base class Usage Ratio
    public double BOvR { get; set; } // Base Overridden Methods Ratio

    public override string ToString() {
        return
            $"Class: {ClassName}, CLOC: {Cloc}, CELOC: {Celoc}, NMD: {Nmd}, NAD: {Nad}, NMD_NAD: {NmdNad}, WMC: {Wmc}, " +
            $"WMC_NO_CASE: {WmcNoCase}, LCOM: {Lcom}, LCOM3: {Lcom3}, LCOM4: {Lcom4}, TCC: {Tcc}, ATFD: {Atfd}, " +
            $"CNOR: {Cnor}, CNOL: {Cnol}, CNOC: {Cnoc}, CNOA: {Cnoa}, NOPM: {Nopm}, NOPF: {Nopf}, CMNB: {Cmnb}, " +
            $"RFC: {Rfc}, CBO: {Cbo}, DIT: {Dit}, DCC: {Dcc}, ATFD_10: {Atfd10}, NIC: {Nic}, WOC: {Woc}, " +
            $"NOPA: {Nopa}, NOPP: {Nopp}, WMCNAMM: {Wmcnamm}, BOvR: {BOvR}";
    }
}