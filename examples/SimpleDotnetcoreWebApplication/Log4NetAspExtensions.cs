using System.IO;
using System.Reflection;
using System.Xml;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleDotnetcoreWebApplication
{
     public static class Log4NetAspExtensions
    {
        public static void ConfigureLog4Net(this IHostingEnvironment appEnv)
        {
            XmlDocument log4netConfig = new XmlDocument();

            using (StreamReader reader = new StreamReader(new FileStream("log4net.config", FileMode.Open, FileAccess.Read)))
            {
                log4netConfig.Load(reader);
            }

            ILoggerRepository rep = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));

            XmlConfigurator.Configure(rep, log4netConfig["log4net"]);
        }

        public static void AddLog4Net(this ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new Log4NetProvider());
        }
}
}
