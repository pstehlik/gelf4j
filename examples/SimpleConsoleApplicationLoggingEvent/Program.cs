using log4net;
using log4net.Core;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace SimpleConsoleApplicationLoggingEvent
{
    class Program
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            Console.WriteLine("Write a sentence, q to quit");

            var text = Console.ReadLine();

            while (text != "q")
            {
                var loggingEventDataCC = new LoggingEventData
                {
                    Message = text,
                    LoggerName = "Test.Logger.Class",
                    Level = Level.Debug,
                    TimeStamp = DateTime.Now
                };
                var loggingEventCC = new LoggingEvent(loggingEventDataCC);


                var loggingEventData = new LoggingEventData
                {
                    Message = JsonConvert.SerializeObject(new { Message = text, Open = DateTime.UtcNow }),
                    LoggerName = "Test.Logger.Class",
                    Level = Level.Debug,
                    TimeStamp = DateTime.Now
                };
                var loggingEvent = new LoggingEvent(loggingEventData);

                var gelfCC = LogManager.GetRepository().GetAppenders().FirstOrDefault(x => x.Name == "GelfUdpAppenderCC");
                var gelf = LogManager.GetRepository().GetAppenders().FirstOrDefault(x => x.Name == "GelfUdpAppender");

                gelf.DoAppend(loggingEvent);
                gelfCC.DoAppend(loggingEventCC);

                text = Console.ReadLine();

                Console.WriteLine("Sent");
            }
        }
    }
}
