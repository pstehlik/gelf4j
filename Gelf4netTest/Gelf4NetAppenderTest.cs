using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Esilog.Gelf4net.Appender;
using log4net.Core;

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
            gelfAppender.GrayLogServerHost = "192.168.3.105";
            gelfAppender.TestAppend(logEvent);

        }
    }
}
