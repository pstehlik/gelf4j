using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace Esilog.Gelf4net.Transport
{
    class UdpTransport : Transport
    {
        public int MaxChunkSize { get; set; }
		
		private int _maxHeaderSize = 8;

        public override void Send(string serverHostName, string serverIpAddress, int serverPort, string message)
        {
            var ipAddress = IPAddress.Parse(serverIpAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, serverPort);

            using(UdpClient udpClient = new UdpClient()){
                var gzipMessage = GzipMessage(message);

                if (MaxChunkSize < gzipMessage.Length)
                {
                    var chunkCount = (gzipMessage.Length / MaxChunkSize) + 1;
                    var messageId = GenerateMessageId(serverHostName);
                    for (int i = 0; i < chunkCount; i++)
                    {
                        var messageChunkPrefix = CreateChunkedMessagePart(messageId, i, chunkCount);
                        var skip = i * MaxChunkSize;
                        var messageChunkSuffix = gzipMessage.Skip(skip).Take(MaxChunkSize).ToArray<byte>();

                        var messageChunkFull = new byte[messageChunkPrefix.Length + messageChunkSuffix.Length];
                        messageChunkPrefix.CopyTo(messageChunkFull, 0);
                        messageChunkSuffix.CopyTo(messageChunkFull, messageChunkPrefix.Length);

                        udpClient.Send(messageChunkFull, messageChunkFull.Length, ipEndPoint);
                    }
                }
                else
                {
                    udpClient.Send(gzipMessage, gzipMessage.Length, ipEndPoint);
                }
            }
        }

        private byte[] CreateChunkedMessagePart(string messageId, int index, int chunkCount)
        {
            var result = new List<byte>();
			//Chunked GELF ID: 0x1e 0x0f (identifying this message as a chunked GELF message)
            result.Add(Convert.ToByte(30));
            result.Add(Convert.ToByte(15));
			
			//Message ID: 8 bytes 
            result.AddRange(Encoding.Default.GetBytes(messageId).ToArray<byte>());
			
			//Sequence Number: 1 byte (The sequence number of this chunk)
            //var indexShifted = (int)((uint)index >> 8);
			//result.Add(Convert.ToByte(indexShifted));
            result.Add(Convert.ToByte(index));
			
			//Total Number: 1 byte (How many chunks does this message consist of in total)
            //var chunkCountShifted = (int)((uint)chunkCount >> 8);
			//result.Add(Convert.ToByte(chunkCountShifted));
            result.Add(Convert.ToByte(chunkCount));

            return result.ToArray<byte>();
        }

        private string GenerateMessageId(string serverHostName)
        {
            var md5String = String.Join("", MD5.Create().ComputeHash(Encoding.Default.GetBytes(serverHostName)).Select(it => it.ToString("x2")).ToArray<string>());
            var random = new Random((int)DateTime.Now.Ticks);
            var sb = new StringBuilder();
            var t = DateTime.Now.Ticks % 1000000000;
            var s = String.Format("{0}{1}", md5String.Substring(0, 10), md5String.Substring(20, 10));
            var r = random.Next(10000000).ToString("00000000");

            sb.Append(t);
            sb.Append(s);
            sb.Append(r);
			
			//Message ID: 8 bytes 
            return sb.ToString().Substring(0, _maxHeaderSize) ;
        }
    }
}
