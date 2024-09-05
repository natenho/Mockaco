namespace Mockaco;

public class ChaosOptions
{
    public bool Enabled { get; set; }
    public int ChaosRate { get; set; }
    public int MinimumLatencyTime { get; set; }
    public int MaximumLatencyTime { get; set; }
    public int TimeBeforeTimeout { get; set; }

    public ChaosOptions()
    {
        Enabled = false;
        ChaosRate = 10;
        MinimumLatencyTime = 500;
        MaximumLatencyTime = 3000;
        TimeBeforeTimeout = 10000;
    }
}