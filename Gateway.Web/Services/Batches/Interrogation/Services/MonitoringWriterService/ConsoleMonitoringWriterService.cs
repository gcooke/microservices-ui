using System;
using Gateway.Web.Services.Batches.Interrogation.Models.Enums;

namespace Gateway.Web.Services.Batches.Interrogation.Services.MonitoringWriterService
{
    public class ConsoleMonitoringWriterService : IMonitoringWriterService
    {
        public void Write(MonitoringLevel monitoringLevel, string text)
        {
            switch (monitoringLevel)
            {
                case MonitoringLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case MonitoringLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case MonitoringLevel.Ok:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case MonitoringLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case MonitoringLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(text);
                    Console.ResetColor();
                    break;
                case MonitoringLevel.Heading:
                    WriteHeading(text);
                    break;
                case MonitoringLevel.Text:
                    WriteText(text);
                    break;
            }
        }

        public void WriteHeading(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"+ {text}");
            Console.ResetColor();
        }

        public void WriteText(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }
    }
}