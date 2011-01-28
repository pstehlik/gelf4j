package com.pstehlik.groovy.gelf4j.net

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import com.pstehlik.groovy.graylog.Graylog2UdpSender

/**
 * Transports GELF messages over the wire to a graylog2 server
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class GelfTransport {
  public static void sendGelfMessageToGraylog(Gelf4JAppender appender, String gelfMessage){
    Graylog2UdpSender.sendPacket(gzipMessage(gelfMessage), appender.graylogServerHost, appender.graylogServerPort)
  }

  private static byte[] gzipMessage(String message){
    def targetStream = new ByteArrayOutputStream()
    def zipStream = new java.util.zip.GZIPOutputStream(targetStream)
    zipStream.write(message.getBytes())
    zipStream.close()
    def zipped = targetStream.toByteArray()
    targetStream.close()
    return zipped
  }
}
