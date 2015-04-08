/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.gelf4j.net

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import com.pstehlik.groovy.graylog.Graylog2UdpSender
import org.apache.log4j.helpers.LogLog

import java.util.zip.GZIPOutputStream

/**
 * Transports GELF messages over the wire to a graylog2 server based on the config on a <code>Gelf4JAppender</code>
 * ID generation based on the Hibernate UUIDHexGenerator
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
  public void sendGelfMessageToGraylog(Gelf4JAppender appender, String gelfMessage) {

    byte[] gzipMessage = gzipMessage(gelfMessage)

    if (gzipMessage.size() <= appender.maxChunkSize) {
      sendUdpPacket(gzipMessage, appender)
      return
    }

    int chunkCount = Math.ceil(gzipMessage.size() / appender.maxChunkSize)
    GelfChunkBuilder headers = new GelfChunkBuilder(chunkCount, appender.maxChunkSize)
    LogLog.debug("Sending ${chunkCount} chunks to Graylog2")
    chunkCount.times {
      headers.chunkNumber = it
      sendUdpPacket(headers.buildChunk(gzipMessage), appender)
    }

  }

  private void sendUdpPacket(byte[] payload, Gelf4JAppender appender) {
    Graylog2UdpSender.sendPacket(payload, appender.graylogServerHost, appender.graylogServerPort)
  }


  /**
   * GZips a given message
   *
   * @param message
   * @return
   */
  private byte[] gzipMessage(String message) {

    def targetStream = new ByteArrayOutputStream()
    def zipStream = new GZIPOutputStream(targetStream)
    zipStream.write(message.getBytes('UTF-8'))
    zipStream.close()
    def zipped = targetStream.toByteArray()
    targetStream.close()
    zipped

  }

}
