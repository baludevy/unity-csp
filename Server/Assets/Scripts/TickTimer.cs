using System;
using System.Diagnostics;
using System.Threading;

public class TickTimer {
    public int tick;

    private Thread tickThread;

    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    public double lastTickTime;
    public double previousLastTickTime;

    public Action<int> onTick;

    public double GetTime() => stopwatch.Elapsed.TotalSeconds;

    public void Stop() {
    }
}