using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Esilog.Gelf4net.Transport
{
    abstract class Transport
    {
        public abstract void Send(string serverHostName, string serverIpAddress, int serverPort, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected byte[] GzipMessage(String message)
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
    }
}
