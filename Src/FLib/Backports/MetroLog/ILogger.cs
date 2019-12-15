using System;
using System.Threading.Tasks;

namespace MetroLog
{
    public interface ILogger
    {
        bool IsTraceEnabled { get; }
        void Trace(string v, params object[] args);
        void Debug(string v, params object[] args);
        void Info(string v, params object[] args);
        void Warn(string v, Exception ex);
        void Warn(string v, params object[] args);
        void Error(string v, params object[] args);
        void Fatal(string v, Exception ex);
    }

    public interface ILoggerAsync : ILogger
    {
        Task FatalAsync( string msg, Exception ex);
    }
}
