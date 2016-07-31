using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleConsoleApplication
{
    internal class Program
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            var stop = new Stopwatch();
            stop.Start();
            log4net.Config.XmlConfigurator.Configure();
            stop.Stop();
            Console.WriteLine(string.Format("Time elapsed configuration {0}", stop.ElapsedMilliseconds));

            Console.WriteLine("Write a sentence, q to quit");

            var text = Console.ReadLine();

            while (text != "q")
            {
                stop.Start();
                //log.Debug(String.Format("Randomizer Sentence: {0}", text));
                log.Debug(new
                {
                    Message = String.Format("Randomizer Sentence: {0}", text),
                    Open = DateTime.UtcNow
                });
                stop.Stop();
                Console.WriteLine(string.Format("Time elapsed configuration {0}", stop.ElapsedMilliseconds));
                Console.WriteLine("Sent {0}", text);

                text = Console.ReadLine();

                Console.WriteLine("Sent");
            }
        }
    }
}