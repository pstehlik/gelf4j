/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.gelf4j.net

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import com.pstehlik.groovy.graylog.Graylog2UdpSender

/**
 * Transports GELF messages over the wire to a graylog2 server based on the config on a <code>Gelf4JAppender</code>
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class GelfTransport {
  /**
   * GZips the <code>gelfMessage</code> and sends it to the server as indicated on the <code>appender</code>
   *
   * @param appender Appender to use for the configuration retrieval of where to send the message to
   * @param gelfMessage The message to send
   */
  public static void sendGelfMessageToGraylog(Gelf4JAppender appender, String gelfMessage){
    Graylog2UdpSender.sendPacket(gzipMessage(gelfMessage), appender.graylogServerHost, appender.graylogServerPort)
  }

  /**
   * GZips a given message
   *
   * @param message
   * @return
   */
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
