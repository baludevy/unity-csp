using System;
using System.Diagnostics;
using System.Threading;

public class TickTimer {
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    public double lastTickTime;

    public Action<int> onTick;
    public double previousLastTickTime;
    public uint tick;

    private Thread tickThread;

    public double GetTime() {
        return stopwatch.Elapsed.TotalSeconds;
    }

    public void Stop() {
    }
}