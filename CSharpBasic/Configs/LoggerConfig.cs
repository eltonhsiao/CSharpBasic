using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Mnemosyne.Logging.Config;
using Mnemosyne.Logging.Interfaces;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace CSharpBasic.Configs
{
    public class LoggerConfig :NLoggerConfig
    {
        public LoggerConfig(
            IConfiguration config,
            ILogNameProvider logNameProvider,
            ITraceIdProvider traceIdProvider)
        {
            LogNameProvider = logNameProvider;
            TraceIdProvider = traceIdProvider;
            TargetProvider = logName =>
            {
                var baseDir = config.GetSection("LogBaseDir").Value;
                return new AsyncTargetWrapper(
                    logName,
                    new FileTarget
                    {
                        FileName = $"{baseDir}/${{date:format=yyyy-MM-dd}}/{logName}/{logName}.log",
                        Layout =
                            Layout.FromString(
                                "[${pad:padding=5:inner=${level:uppercase=true}}] ${longdate} - ${message}"),
                        ArchiveFileName =
                            Layout.FromString(
                                $"{baseDir}/${{date:format=yyyy-MM-dd}}/{logName}/{logName}.{{###}}.log"),
                        ArchiveDateFormat = "HHmm",
                        ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                        ArchiveAboveSize = 1024 * 100, // 100KB 
                        ArchiveEvery = FileArchivePeriod.Hour,
                        KeepFileOpen = true,
                        ConcurrentWrites = false,
                        AutoFlush = false,
                        OpenFileFlushTimeout = 1 // sec 
                    })
                {
                    OverflowAction = AsyncTargetWrapperOverflowAction.Grow,
                    TimeToSleepBetweenBatches = 0,
                    BatchSize = 500
                };
            };
        }

        public override ILogNameProvider LogNameProvider { get; }

        public override bool LogWithMethodName { get; }

        public override Func<string, Target> TargetProvider { get; }

        public override ITraceIdProvider TraceIdProvider { get; }
    }
}
