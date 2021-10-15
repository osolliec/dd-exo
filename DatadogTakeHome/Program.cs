using DatadogTakeHome.Core;
using DatadogTakeHome.Core.Alerts;
using DatadogTakeHome.Core.Csv;
using DatadogTakeHome.Core.Logger;
using DatadogTakeHome.Core.RequestParser;
using DatadogTakeHome.Core.Stats;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace DatadogTakeHome
{
    class Program
    {
        static void Main(string[] args)
        {
            var mainCommand = BuildCommand();

            mainCommand.Handler = CommandHandler.Create<int, int, int, string>((alertWindowSeconds, alertAverageThreshold, reportWindowSeconds, file) =>
            {
                var logger = new ConsoleLogger();

                if (alertWindowSeconds < 1 || alertAverageThreshold < 1 || reportWindowSeconds < 1)
                {
                    logger.Log(LogLevel.Error, null, "Value must be at least 1");
                    System.Environment.Exit(-1);
                }

                var orchestrator = new Orchestrator(
                    new HttpRequestParser(),
                    logger,
                    new List<ILogAggregator> {
                        // add here new reports or alerts
                        new PeriodicSummaryReport(reportWindowSeconds),
                        new AverageHitAlert(alertWindowSeconds, alertAverageThreshold),
                    }
                );

                try
                {
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        ReadFromStdin(logger, orchestrator);
                    }
                    else
                    {
                        ReadFromFile(file, logger, orchestrator);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex, "Error in main program");
                }

            }); ;

            mainCommand.Invoke(args);
        }

        /// <summary>
        /// Non blocking. Reads from STDIN.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="orchestrator"></param>
        static void ReadFromStdin(ConsoleLogger logger, Orchestrator orchestrator)
        {
            logger.Log(LogLevel.Information, null, "Starting to read from STDIN. If you still see this, the program needs input.");
            using (var inputStream = Console.OpenStandardInput())
            using (var parser = new StreamingCsvParser(logger))
            {
                var lines = parser.Parse(inputStream);

                foreach (var line in lines)
                {
                    orchestrator.Collect(line);
                    orchestrator.DisplayMessages();
                }
            }
        }

        /// <summary>
        /// Blocking. Will continuously read from a file that is appended to.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="logger"></param>
        /// <param name="orchestrator"></param>
        static void ReadFromFile(string path, ILogger logger, Orchestrator orchestrator)
        {
            if (!File.Exists(path))
            {
                logger.Log(LogLevel.Information, null, $"File  {path} not found, aborting");
                System.Environment.Exit(-1);
            }

            using (var parser = new StreamingCsvParser(logger))
            {
                var lines = parser.Parse(path);

                foreach (var line in lines)
                {
                    orchestrator.Collect(line);
                    orchestrator.DisplayMessages();
                }
            }
        }

        /// <summary>
        /// Build the Command that parses the args. Visit https://github.com/dotnet/command-line-api for more info.
        /// </summary>
        /// <returns></returns>
        static RootCommand BuildCommand()
        {
            return new RootCommand("Reads from either http log csv file or STDIN, and inputs statistics and alerts. If no file was specified, will read from STDIN. Reading from the file is blocking.")
            {
                new Option<int>(
                    new string[] { "--alert-window-seconds" },
                    getDefaultValue: () => 120,
                    description: "The window size of the alert in seconds."),
                new Option<int>(
                    new string[] { "--alert-average-threshold" },
                    getDefaultValue: () => 10,
                    description: "The average hit threshold of the alert."),
                new Option<int>(
                    new string[] { "--report-window-seconds" },
                    getDefaultValue: () => 10,
                    description: "The window size of the report in seconds."),
                new Option<string>(
                    "--file",
                    description: "The path to the file to be parsed. If no file, will default to STDIN."
                )
            };
        }
    }
}
