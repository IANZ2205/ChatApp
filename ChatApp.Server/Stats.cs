using System;
using System.Diagnostics;

namespace ChatApp.Server
{
    public static class Stats
    {
        private static readonly Process _process = Process.GetCurrentProcess();
        private static DateTime _processStart = DateTime.UtcNow;

        private static DateTime _lastCpuCheck = DateTime.UtcNow;
        private static TimeSpan _lastCpuTime = TimeSpan.Zero;
        private static double _lastCpuUsage = 0;

        // Cumulative counters (reset on server start)
        public static long BytesSent;
        public static long BytesReceived;
        public static long MessagesTotal;
        public static long FilesTotal;
        public static long ConnectionsTotal;
        public static long LoginsTotal;

        // Historical graph data (last 60 samples, 1 per second)
        public static readonly double[] CpuHistory = new double[60];
        public static readonly double[] MemHistory = new double[60];
        private static int _historyIndex = 0;

        public static void Reset()
        {
            BytesSent = 0;
            BytesReceived = 0;
            MessagesTotal = 0;
            FilesTotal = 0;
            ConnectionsTotal = 0;
            LoginsTotal = 0;

            _processStart = DateTime.UtcNow;
            _lastCpuCheck = DateTime.UtcNow;
            _lastCpuTime = _process.TotalProcessorTime;
            _lastCpuUsage = 0;

            for (int i = 0; i < CpuHistory.Length; i++) { CpuHistory[i] = 0; MemHistory[i] = 0; }
            _historyIndex = 0;
        }

        public static TimeSpan Uptime => DateTime.UtcNow - _processStart;

        public static double CpuPercent
        {
            get
            {
                var now = DateTime.UtcNow;
                var elapsed = (now - _lastCpuCheck).TotalMilliseconds;
                if (elapsed < 250) return _lastCpuUsage;

                _process.Refresh();
                var cpuTime = _process.TotalProcessorTime;
                var deltaCpu = (cpuTime - _lastCpuTime).TotalMilliseconds;
                var percent = deltaCpu / (elapsed * Environment.ProcessorCount) * 100;
                _lastCpuUsage = Math.Max(0, Math.Min(100, percent));
                _lastCpuCheck = now;
                _lastCpuTime = cpuTime;
                return _lastCpuUsage;
            }
        }

        public static double MemoryMB
        {
            get
            {
                _process.Refresh();
                return _process.WorkingSet64 / (1024.0 * 1024.0);
            }
        }

        // Called once per second to update the graph history
        public static void Sample()
        {
            CpuHistory[_historyIndex] = CpuPercent;
            MemHistory[_historyIndex] = MemoryMB;
            _historyIndex = (_historyIndex + 1) % CpuHistory.Length;
        }

        public static string UptimeText
        {
            get
            {
                var u = Uptime;
                return $"{u.Hours:D2}:{u.Minutes:D2}:{u.Seconds:D2}";
            }
        }
    }
}