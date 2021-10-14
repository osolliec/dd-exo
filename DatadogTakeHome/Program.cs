using DatadogTakeHome.Core;
using DatadogTakeHome.Core.Alerts;
using DatadogTakeHome.Core.Csv;
using DatadogTakeHome.Core.Logger;
using DatadogTakeHome.Core.RequestParser;
using DatadogTakeHome.Core.Stats;
using System;
using System.Collections.Generic;

namespace DatadogTakeHome
{
    class Program
    {
        private static int TWO_MINUTES_DURATION_SECONDS = 120;
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var orchestrator = new Orchestrator(
                new HttpRequestParser(),
                logger,
                new List<ILogAggregator> {
                    new PeriodicSummaryReport(10),
                    new AverageHitAlert(TWO_MINUTES_DURATION_SECONDS, 10),
                }
            );



            try
            {
                //Stream inputStream = Console.OpenStandardInput();
                using (var parser = new StreamingCsvParser(logger))
                {
                    var lines = parser.Parse("sample_csv.txt");

                    foreach (var line in lines)
                    {
                        orchestrator.Collect(line);
                        orchestrator.DisplayMessages();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Error in main program");
            }
        }
    }
}
