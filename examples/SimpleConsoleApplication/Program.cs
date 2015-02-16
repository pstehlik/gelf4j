using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConsoleApplication
{
    class Program
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            Console.WriteLine("Write a sentence, q to quit");

            var text = Console.ReadLine();

            while(text != "q")
            {
                log.Debug(String.Format("Randomizer Sentence: {0}", text));
                Console.WriteLine("Sent {0}", text);

                text = Console.ReadLine();

                Console.WriteLine("Sent");
            }
        }
    }
}
