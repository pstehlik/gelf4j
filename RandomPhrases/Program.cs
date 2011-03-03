using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace RandomPhrases
{
    class Program
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Debug("Hello Nurse!");
        }
    }
}
