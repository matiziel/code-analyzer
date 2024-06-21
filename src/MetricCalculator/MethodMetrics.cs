namespace MetricCalculator;

public class MethodMetrics
{
    public string MethodName { get; set; }
    
    public int Cyclo { get; set; }
    
    public int CycloSwitch { get; set; }
    
    public int Mloc { get; set; }
    
    public int Meloc { get; set; }
    
    public int Nop { get; set; }
    
    public int Nolv { get; set; }
    
    public int Notc { get; set; }
    
    public int Mnol { get; set; }
    
    public int Mnor { get; set; }
    
    public int Mnoc { get; set; }
    
    public int Mnoa { get; set; }
    
    public int Nonl { get; set; }
    
    public int Nosl { get; set; }
    
    public int Nomo { get; set; }
    
    public int Nope { get; set; }
    
    public int Nole { get; set; }
    
    public int Mmnb { get; set; }
    
    public int Nouw { get; set; }
    
    public double Aid { get; set; }

    public override string ToString()
    {
        return $"Method: {MethodName}, CYCLO: {Cyclo}, CYCLO_SWITCH: {CycloSwitch}, MLOC: {Mloc}, MELOC: {Meloc}, " +
               $"NOP: {Nop}, NOLV: {Nolv}, NOTC: {Notc}, MNOL: {Mnol}, MNOR: {Mnor}, MNOC: {Mnoc}, MNOA: {Mnoa}, " +
               $"NONL: {Nonl}, NOSL: {Nosl}, NOMO: {Nomo}, NOPE: {Nope}, NOLE: {Nole}, MMNB: {Mmnb}, NOUW: {Nouw}, AID: {Aid}";
    }
}