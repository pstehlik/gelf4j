using gelf4net.Util.TypeConverters;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace gelf4net.Appender
{
    /// <summary>
    /// Gelf Udp Appender
    /// </summary>
    public class GelfUdpAppender : log4net.Appender.UdpAppender
    {
        /// <summary>
        /// The MessageId used in Chunked mode.
        /// </summary>
        private static long ChunkMessageId;

        /// <summary>
        /// Gets or sets GrayLogServerHost.
        /// </summary>
        public string RemoteHostName { get; set; }

        public GelfUdpAppender()
        {
            Encoding = Encoding.UTF8;
            MaxChunkSize = 1024;
            ChunkMessageId = DateTime.Now.Ticks % (2 ^ 16);
            log4net.Util.TypeConverters.ConverterRegistry.AddConverter(typeof(IPAddress), new IPAddressConverter());
        }

        public override void ActivateOptions()
        {
            if (RemoteAddress == null)
            {
                RemoteAddress = IPAddress.Parse(GetIpAddressFromHostName());
            }

            base.ActivateOptions();
        }

        protected override void InitializeClientConnection()
        {
            base.InitializeClientConnection();
        }

        public int MaxChunkSize { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {

                byte[] bytes = this.RenderLoggingEvent(loggingEvent).GzipMessage(this.Encoding);

                if (MaxChunkSize < bytes.Length)
                {
                    var chunkCount = (bytes.Length / MaxChunkSize) + 1;
                    var messageId = GenerateMessageId();
                    var state = new UdpState() { SendClient = Client, Bytes = bytes, ChunkCount = chunkCount, MessageId = messageId, SendIndex = 0 };
                    var messageChunkFull = GetMessageChunkFull(state.Bytes, state.MessageId, state.SendIndex, state.ChunkCount);
                    Client.BeginSend(messageChunkFull, messageChunkFull.Length, RemoteEndPoint, SendCallback, state);
                }
                else
                {
                    var state = new UdpState() { SendClient = Client, Bytes = bytes, ChunkCount = 0, MessageId = null, SendIndex = 0 };
                    Client.BeginSend(bytes, bytes.Length, RemoteEndPoint, SendCallback, state);
                }
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Unable to send logging event to remote host " + this.RemoteAddress + " on port " + this.RemotePort + ".", ex, ErrorCode.WriteFailure);
            }
        }

        private byte[] GetMessageChunkFull(byte[] bytes, byte[] messageId, int i, int chunkCount)
        {
            var messageChunkPrefix = CreateChunkedMessagePart(messageId, i, chunkCount);
            var skip = i * MaxChunkSize;
            var messageChunkSuffix = bytes.Skip(skip).Take(MaxChunkSize).ToArray<byte>();

            var messageChunkFull = new byte[messageChunkPrefix.Length + messageChunkSuffix.Length];
            messageChunkPrefix.CopyTo(messageChunkFull, 0);
            messageChunkSuffix.CopyTo(messageChunkFull, messageChunkPrefix.Length);

            return messageChunkFull;
        }

        private void SendCallback(IAsyncResult ar)
        {
            var state = (UdpState)ar.AsyncState;
            var u = state.SendClient;

            u.EndSend(ar);

            state.SendIndex++;
            if (state.SendIndex < state.ChunkCount)
            {
                var messageChunkFull = GetMessageChunkFull(state.Bytes, state.MessageId, state.SendIndex, state.ChunkCount);
                state.SendClient.BeginSend(messageChunkFull, messageChunkFull.Length, RemoteEndPoint, SendCallback, state);
            }
        }

        private class UdpState
        {
            public UdpClient SendClient { set; get; }
            public int ChunkCount { set; get; }
            public byte[] MessageId { set; get; }
            public int SendIndex { set; get; }
            public byte[] Bytes { set; get; }
        }

        private string GetIpAddressFromHostName()
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(RemoteHostName);
            return addresslist[0].ToString();
        }

        public static byte[] GenerateMessageId()
        {
            Interlocked.Increment(ref ChunkMessageId);
            return BitConverter.GetBytes(ChunkMessageId);
        }

        public static byte[] CreateChunkedMessagePart(byte[] messageId, int index, int chunkCount)
        {
            var result = new List<byte>();
            var gelfHeader = new byte[2] { Convert.ToByte(30), Convert.ToByte(15) };
            result.AddRange(gelfHeader);
            result.AddRange(messageId);
            result.Add(Convert.ToByte(index));
            result.Add(Convert.ToByte(chunkCount));

            return result.ToArray<byte>();
        }
    }
}
