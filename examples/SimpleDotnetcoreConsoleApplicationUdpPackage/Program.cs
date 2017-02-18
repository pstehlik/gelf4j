using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace SimpleDotnetcoreConsoleApplicationUdpPackage
{
    public class Program
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            XmlDocument log4netConfig = new XmlDocument();
            using (StreamReader reader = new StreamReader(new FileStream("log4net.config", FileMode.Open, FileAccess.Read)))
            {
                log4netConfig.Load(reader);
            }

            ILoggerRepository rep = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);

            Console.WriteLine("Write a sentence, q to quit");

            var text = Console.ReadLine();

            while (text != "q")
            {
                //log.Debug(String.Format("Randomizer Sentence: {0}", text));
                log.Debug(new
                {
                    Message = String.Format("Randomizer Sentence: {0}", text),
                    Open = DateTime.UtcNow
                });
                Console.WriteLine("Sent {0}", text);

                text = Console.ReadLine();

                Console.WriteLine("Sent");
            }
        }
    }
}