namespace NuGetPackageManager.Loggers
{
    using Catel.Logging;
    using NuGet.Common;
    using System;
    using System.Threading.Tasks;

    public class DebugLogger : ILogger
    {
        private ILog _log = LogManager.GetCurrentClassLogger();

        private bool _verbose;

        public DebugLogger(bool verbose)
        {
            _verbose = verbose;
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(data);
                    break;
                case LogLevel.Error:
                    LogError(data);
                    break;
                case LogLevel.Information:
                    LogInformation(data);
                    break;
                case LogLevel.Warning:
                    LogWarning(data);
                    break;
                case LogLevel.Minimal:
                    LogMinimal(data);
                    break;
                case LogLevel.Verbose:
                    LogVerbose(data);
                    break;
            }
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        public async Task LogAsync(LogLevel level, string data)
        {
            var logginTask = Task.Run(() => Log(level, data));
            await logginTask;
        }

        public async Task LogAsync(ILogMessage message)
        {
            await LogAsync(message.Level, message.Message);
        }

        public void LogDebug(string data)
        {
            _log.Debug(data);
        }

        public void LogError(string data)
        {
            _log.Error(data);
        }

        public void LogInformation(string data)
        {
            _log.Info(data);
        }

        public void LogInformationSummary(string data)
        {
            _log.Info(data);
        }

        public void LogMinimal(string data)
        {
            LogInformation(data);
        }

        public void LogVerbose(string data)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string data)
        {
            _log.Warning(data);
        }
    }
}
