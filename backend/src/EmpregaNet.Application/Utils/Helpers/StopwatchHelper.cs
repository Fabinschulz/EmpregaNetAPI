using System.Diagnostics;

namespace EmpregaNet.Application.Utils.Helpers
{
    public class StopwatchHelper : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _operationName;

        public StopwatchHelper(string operationName)
        {
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
        }

        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        public void Stop()
        {
            _stopwatch.Stop();
            Console.WriteLine($"{_operationName} levou {_stopwatch.ElapsedMilliseconds} ms");
        }

        public void Dispose()
        {
            Stop();
        }
    }
}