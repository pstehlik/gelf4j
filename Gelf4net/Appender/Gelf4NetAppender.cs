using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using RabbitMQ.Client;

namespace Esilog.Gelf4net.Appender
{
	public class Gelf4NetAppender : AppenderSkeleton
	{
		public static int SHORT_MESSAGE_LENGTH = 250;
		public static string UNKNOWN_HOST = "unknown_host";
		private static string GELF_VERSION = "1.0";

		//---------------------------------------
		//configuration settings for the appender
		public Dictionary<string, string> AdditionalFields { get; set; }
		public string Facility { get; set; }
		public string GrayLogServerHost { get; set; }
		public string GrayLogServerHostIpAddress { get; set; }
		public int GrayLogServerPort { get; set; }
		public string Host { get; set; }
		public bool IncludeLocationInformation { get; set; }
		public bool SendAsGelfOrAmqp { get; set; }
		public int MaxChunkSize { get; set; }


		public int GrayLogServerAmqpPort { get; set; }
		public string GrayLogServerAmqpUser { get; set; }
		public string GrayLogServerAmqpPassword { get; set; }
		public string GrayLogServerAmqpVirtualHost { get; set; }
		public string GrayLogServerAmqpQueue { get; set; }


		public Gelf4NetAppender()
			: base()
		{
			AdditionalFields = null;
			Facility = null;
			GrayLogServerHost = "";
			GrayLogServerHostIpAddress = "";
			GrayLogServerPort = 12201;
			Host = null;
			IncludeLocationInformation = false;
			MaxChunkSize = 8154;
			SendAsGelfOrAmqp = true;


			GrayLogServerAmqpPort = 5672;
			GrayLogServerAmqpUser = "guest";
			GrayLogServerAmqpPassword = "guest";
			GrayLogServerAmqpVirtualHost = "/";
			GrayLogServerAmqpQueue = "queue1";


		}

		
		/// <summary>
		/// Test porpuse used only in the unit test
		/// </summary>
		/// <param name="loggingEvent"></param>
		public void TestAppend(log4net.Core.LoggingEvent loggingEvent)
		{
			Append(loggingEvent);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loggingEvent"></param>
		protected override void Append(log4net.Core.LoggingEvent loggingEvent)
		{
			String gelfJsonString = CreateGelfJsonFromLoggingEvent(loggingEvent);
			if (SendAsGelfOrAmqp)
				SendGelfMessageToGrayLog(gelfJsonString);
			else
				SendAmqpMessageToGrayLog(gelfJsonString);
		}

		/// <summary>
		/// 
		/// </summary>
		private String LoggingHostName
		{
			get
			{
				String ret = Host;
				if (ret == null)
				{
					try
					{
						ret = System.Net.Dns.GetHostName();
					}
					catch (SocketException)
					{
						ret = UNKNOWN_HOST;
					}
				}
				return ret;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private string GrayLogServerUrl
		{
			get
			{
				return String.Format("udp://{0}:{1}", GrayLogServerHost, GrayLogServerPort);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		private void SendGelfMessageToGrayLog(string message)
		{
			UdpClient udpClient = new UdpClient();
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}
			IPAddress ipAddress = IPAddress.Parse(GrayLogServerHostIpAddress);
			IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, GrayLogServerPort);

			Byte[] sendBytes = GzipMessage(message);
			try
			{

				udpClient.Send(sendBytes, sendBytes.Length, ipEndPoint);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		private void SendAmqpMessageToGrayLog(string message)
		{
			if (GrayLogServerHostIpAddress == string.Empty)
			{
				GrayLogServerHostIpAddress = GetIpAddressFromHostName();
			}

			//Create the Connection 
			var factory = new ConnectionFactory()
			{
				Protocol = Protocols.FromEnvironment(),
				HostName = GrayLogServerHostIpAddress,
				Port = GrayLogServerAmqpPort,
				VirtualHost = GrayLogServerAmqpVirtualHost,
				UserName = GrayLogServerAmqpUser,
				Password = GrayLogServerAmqpPassword
			};

			using (IConnection conn = factory.CreateConnection())
			{
				var model = conn.CreateModel();
				model.ExchangeDeclare("sendExchange", ExchangeType.Direct);
				model.QueueDeclare(GrayLogServerAmqpQueue, true, true, true, null);
				model.QueueBind(GrayLogServerAmqpQueue, "sendExchange", "key");
				byte[] messageBodyBytes = GzipMessage(message);
				model.BasicPublish(GrayLogServerAmqpQueue, "key", null, messageBodyBytes);

			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private string GetIpAddressFromHostName()
		{
			IPAddress[] addresslist = Dns.GetHostAddresses(GrayLogServerHost);
			return addresslist[0].ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private byte[] GzipMessage(String message)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			MemoryStream ms = new MemoryStream();
			using (GZipStream zip = new System.IO.Compression.GZipStream(ms, CompressionMode.Compress, true))
			{
				zip.Write(buffer, 0, buffer.Length);
			}
			ms.Position = 0;
			MemoryStream outStream = new MemoryStream();
			byte[] compressed = new byte[ms.Length];
			ms.Read(compressed, 0, compressed.Length);
			return compressed;

		}

		/// <summary>
		/// Creates the JSON String for a given <code>LoggingEvent</code>.
		/// The "short_message" of the GELF message is max 50 chars long.
		/// Message building and skipping of additional fields etc is based on
		/// https://github.com/Graylog2/graylog2-docs/wiki/GELF from Jan 7th 2011.
		/// </summary>
		/// <param name="loggingEvent"> The logging event to base the JSON creation on</param>
		/// <returns></returns>
		private string CreateGelfJsonFromLoggingEvent(log4net.Core.LoggingEvent loggingEvent)
		{
			var fullMessage = loggingEvent.RenderedMessage;
			var shortMessage = fullMessage;

			if (shortMessage.Length > SHORT_MESSAGE_LENGTH)
			{
				shortMessage = shortMessage.Substring(0, SHORT_MESSAGE_LENGTH - 1);
			}

			

			var gelfMessage = new GelfMessage
			{
				Facility = (this.Facility ?? "GELF"),
				File = "",
				FullMesage = fullMessage,
				Host = LoggingHostName,
				Level = GetSyslogSeverity(loggingEvent.Level),
				Line = "",
				ShortMessage = shortMessage,
				TimeStamp = loggingEvent.TimeStamp,
				Version = GELF_VERSION
			};

			//only set location information if configured
			if (IncludeLocationInformation)
			{
				gelfMessage.File = loggingEvent.LocationInformation.FileName;
				gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
			}

			var gelfJsonMessage = JsonConvert.SerializeObject(gelfMessage);

			var jsonObject = JObject.Parse(gelfJsonMessage);

			//add additional fields and prepend with _ if not present already
			if (AdditionalFields != null)
			{
				foreach (var item in AdditionalFields)
				{
					var key = item.Key;
					if (!key.StartsWith("_"))
					{
						key = String.Format("_{0}", key);
					}

					if (key != "_id")
					{
						jsonObject.Add(key, item.Value);
						//gelfMessage.key ) it.value as string
					}
				}
			}


			return jsonObject.ToString();
		}

		private int GetSyslogSeverity(log4net.Core.Level level)
		{
            if(level == log4net.Core.Level.Alert)
                return (int) LocalSyslogAppender.SyslogSeverity.Alert;

            if(level == log4net.Core.Level.Critical || level == log4net.Core.Level.Fatal)
                return (int)LocalSyslogAppender.SyslogSeverity.Critical;

            if(level == log4net.Core.Level.Debug)
                return (int)LocalSyslogAppender.SyslogSeverity.Debug;

            if(level == log4net.Core.Level.Emergency)
                return (int)LocalSyslogAppender.SyslogSeverity.Emergency;

            if(level == log4net.Core.Level.Error)
                return (int)LocalSyslogAppender.SyslogSeverity.Error;

            if(level == log4net.Core.Level.Fine 
                || level == log4net.Core.Level.Finer 
                || level ==  log4net.Core.Level.Finest 
                || level ==  log4net.Core.Level.Info 
                || level ==  log4net.Core.Level.Off)
                return (int)LocalSyslogAppender.SyslogSeverity.Informational;

            if(level == log4net.Core.Level.Notice 
                || level == log4net.Core.Level.Verbose 
                || level == log4net.Core.Level.Trace)
                return (int) LocalSyslogAppender.SyslogSeverity.Notice;

            if(level == log4net.Core.Level.Severe)
                return (int) LocalSyslogAppender.SyslogSeverity.Emergency;
            
            if(level == log4net.Core.Level.Warn)
                return (int) LocalSyslogAppender.SyslogSeverity.Warning;

            return (int) LocalSyslogAppender.SyslogSeverity.Debug;
		}

	}

	/// <summary>
	/// 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	class GelfMessage
	{
		[JsonProperty("facility")]
		public string Facility { get; set; }

		[JsonProperty("file")]
		public string File { get; set; }

		[JsonProperty("full_message")]
		public string FullMesage { get; set; }

		[JsonProperty("host")]
		public string Host { get; set; }

		[JsonProperty("level")]
		public int Level { get; set; }

		[JsonProperty("line")]
		public string Line { get; set; }

		[JsonProperty("short_message")]
		public string ShortMessage { get; set; }

		[JsonProperty("timestamp")]
		public DateTime TimeStamp { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }
	}

}
