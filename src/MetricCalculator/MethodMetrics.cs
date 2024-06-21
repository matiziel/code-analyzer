namespace MetricCalculator;

public class MethodMetrics
{
    public string MethodName { get; set; }
    public int CYCLO { get; set; }
    public int CYCLO_SWITCH { get; set; }
    public int MLOC { get; set; }
    public int MELOC { get; set; }
    public int NOP { get; set; }
    public int NOLV { get; set; }
    public int NOTC { get; set; }
    public int MNOL { get; set; }
    public int MNOR { get; set; }
    public int MNOC { get; set; }
    public int MNOA { get; set; }
    public int NONL { get; set; }
    public int NOSL { get; set; }
    public int NOMO { get; set; }
    public int NOPE { get; set; }
    public int NOLE { get; set; }
    public int MMNB { get; set; }
    public int NOUW { get; set; }
    public double AID { get; set; }

    public override string ToString()
    {
        return $"Method: {MethodName}, CYCLO: {CYCLO}, CYCLO_SWITCH: {CYCLO_SWITCH}, MLOC: {MLOC}, MELOC: {MELOC}, NOP: {NOP}, NOLV: {NOLV}, NOTC: {NOTC}, MNOL: {MNOL}, MNOR: {MNOR}, MNOC: {MNOC}, MNOA: {MNOA}, NONL: {NONL}, NOSL: {NOSL}, NOMO: {NOMO}, NOPE: {NOPE}, NOLE: {NOLE}, MMNB: {MMNB}, NOUW: {NOUW}, AID: {AID}";
    }
}