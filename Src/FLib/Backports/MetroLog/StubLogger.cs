using System;
using System.Threading.Tasks;

using Dbg = System.Diagnostics.Debug;

namespace MetroLog
{
    public class StubLogger : ILoggerAsync
    {
        public bool IsTraceEnabled => true;

        public void Trace(string v, params object[] args)
        {
            Dbg.WriteLine(v, args);
        }

        public void Debug(string v, params object[] args)
        {
            Dbg.WriteLine(v, args);
        }

        public void Info(string v, params object[] args)
        {
            Dbg.WriteLine(v, args);
        }

        public void Warn(string v, Exception ex)
        {
            Dbg.WriteLine(v);
            Dbg.WriteLine(ex.Message);
        }

        public void Warn(string v, params object[] args)
        {
            Dbg.WriteLine(v, args);
        }

        public void Error(string v, params object[] args)
        {
            Dbg.WriteLine(v, args);
        }

        public void Fatal(string msg, Exception ex)
        {
            Dbg.WriteLine(msg);
            Dbg.WriteLine(ex.Message);
        }

        public Task FatalAsync(string msg, Exception ex)
        {
            Dbg.WriteLine(msg);
            Dbg.WriteLine(ex.Message);
            return Task.CompletedTask;
        }
    }
}
