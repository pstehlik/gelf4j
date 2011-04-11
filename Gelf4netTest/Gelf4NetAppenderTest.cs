using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Esilog.Gelf4net.Appender;
using log4net.Core;
using System.Security.Cryptography;
using System.IO;

namespace Gelf4netTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Gelf4NetAppenderTest
    {
        public Gelf4NetAppenderTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void AppendTest()
        {
            var gelfAppender = new Gelf4NetAppender();
            //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
            var data = new LoggingEventData
            {
                Domain = this.GetType().Name,
                Level = Level.Debug,
                LoggerName = "Tester",
                Message = "GrayLog4Net!!!",
                TimeStamp = DateTime.Now,
                UserName = "ElTesto"
            };

            var logEvent = new LoggingEvent(data);
            gelfAppender.GrayLogServerHost = "public-graylog2.taulia.com";
            gelfAppender.TestAppend(logEvent);

        }

        [TestMethod]
        public void AppendTestChunkMessage()
        {
            var gelfAppender = new Gelf4NetAppender();
            //def logEvent = new LoggingEvent(this.GetType().Name, new Category('catName'), System.currentTimeMillis(), Priority.WARN, "Some Short Message", new Exception('Exception Message'))
            var data = new LoggingEventData
            {
                Domain = this.GetType().Name,
                Level = Level.Debug,
                LoggerName = "Big Tester",
                Message = LoremIpsum.Text,
                TimeStamp = DateTime.Now,
                UserName = "ElTesto"
            };

            var logEvent = new LoggingEvent(data);
            gelfAppender.GrayLogServerHost = "public-graylog2.taulia.com";
            gelfAppender.MaxChunkSize = 500;
            gelfAppender.AdditionalFields = "nombre:pedro,apellido:jimenez";
            logEvent.Properties["customProperty"] = "My Custom Property";

            gelfAppender.TestAppend(logEvent);

        }

        [TestMethod]
        public string TestMessageId()
        {
            var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes("thousandsunny")).Select(it => it.ToString("x2")).ToArray<string>());
            var random = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            var t = DateTime.Now.Ticks % 1000000000;
            var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
            var r = random.Next(10000000).ToString("00000000");

            sb.Append(t);
            sb.Append(s);
            sb.Append(r);

            Console.WriteLine(t.ToString());
            Console.WriteLine(s.ToString());
            Console.WriteLine(r.ToString());
            Console.WriteLine(sb.ToString());

            var result = sb.ToString().Substring(0, 32);
            Console.WriteLine(result);
            //Assert.AreEqual(result.Count, 32);
            return result;

        }

        [TestMethod]
        public void TestCreateChunkMessage()
        {

            //var result = new List<byte>();
            //result.Add(Convert.ToByte(30));
            //result.Add(Convert.ToByte(15));

            //result.AddRange(Encoding.Default.GetBytes(TestMessageId()).ToArray<byte>());

            //var indexShifted = (int)((uint)1 >> 8);
            //var chunkCountShifted = (int)((uint)2 >> 8);

            //result.Add(Convert.ToByte(indexShifted));
            //result.Add(Convert.ToByte(index));

            //result.Add(Convert.ToByte(chunkCountShifted));
            //result.Add(Convert.ToByte(chunkCount));

            //return result.ToArray<byte>();
        }

        [TestMethod]
        public void TestSendMessageIteration()
        {
            var array = new List<int>{1,2,3,4,5,6,7,8,9};
            var max = 2;
            var iterations = array.Count / max;

            for (int i = 0; i < iterations + 1; i++)
            {
                array.Skip(i * max).Take(max).ToList<int>().ForEach(it =>{
                    Console.WriteLine(it);
                });
            }
        }
    }
}
