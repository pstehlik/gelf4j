/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.gelf4j.net

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import com.pstehlik.groovy.graylog.Graylog2UdpSender
import java.nio.ByteBuffer
import java.security.MessageDigest

/**
 * Transports GELF messages over the wire to a graylog2 server based on the config on a <code>Gelf4JAppender</code>
 * ID generation based on the Hibernate UUIDHexGenerator
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class GelfTransport {
  private static final USED_CHARSET = 'UTF-8'
  private static short counter = (short) 0
  private static final int JVM = (int) (System.currentTimeMillis() >>> 8)
  private static final int IP

  static {
    int ipadd
    try {
      ipadd = BytesHelper.toInt(InetAddress.getLocalHost().getAddress())
    }
    catch (Exception e) {
      ipadd = 0
    }
    IP = ipadd;
  }

  private String sep = ""

  /**
   * GZips the <code>gelfMessage</code> and sends it to the server as indicated on the <code>appender</code>
   *
   * @param appender Appender to use for the configuration retrieval of where to send the message to
   * @param gelfMessage The message to send
   */
  public void sendGelfMessageToGraylog(Gelf4JAppender appender, String gelfMessage) {
    byte[] gzipMessage = gzipMessage(gelfMessage)

    //set up chunked transfer if larger than maxChunkSize
    if (appender.maxChunkSize < gzipMessage.size()) {
      Integer chunkCount = (((gzipMessage.size() / appender.maxChunkSize) + 0.5) as Double).round().toInteger()
      String messageId = generateMessageId()
      chunkCount.times {
        byte[] messageChunkPrefix = createChunkedMessagePart(messageId, it , chunkCount)
        Integer endOfChunk
        Integer messagePartSize
        if (gzipMessage.size() < ((it + 1) * appender.maxChunkSize)) {
          endOfChunk = gzipMessage.size()
          messagePartSize = gzipMessage.size() - (appender.maxChunkSize * it)
        } else {
          endOfChunk = (appender.maxChunkSize * (it + 1))
          messagePartSize = appender.maxChunkSize
        }

        byte[] chunkedMessagPart = new byte[messageChunkPrefix.size() + messagePartSize]
        System.arraycopy(
          messageChunkPrefix,
          0,
          chunkedMessagPart,
          0,
          messageChunkPrefix.size()
        )
        System.arraycopy(
          gzipMessage,
          endOfChunk - messagePartSize,
          chunkedMessagPart,
          messageChunkPrefix.size(),
          messagePartSize
        )
        Graylog2UdpSender.sendPacket(
          chunkedMessagPart,
          appender.graylogServerHost,
          appender.graylogServerPort
        )
      }
    } else {
      Graylog2UdpSender.sendPacket(gzipMessage, appender.graylogServerHost, appender.graylogServerPort)
    }
  }

  /**
   * Creates the bytes that identify a chunked message.
   *
   * @param messageId The unique identifier for this message
   * @param sequenceNo The number of the message in the chunk sequence
   * @param sequenceCount The total number of chunks
   * @return The prepared byte array
   */
  private byte[] createChunkedMessagePart(String messageId, Integer sequenceNo, Integer sequenceCount) {
    Collection ret = []
    ret << 30.byteValue()
    ret << 15.byteValue()
    messageId.getBytes('ISO-8859-1').each {
      ret << it
    }
    ret << (sequenceNo.byteValue())
    ret << (sequenceCount.byteValue())
    println "${sequenceNo} ${sequenceCount}" 
    println (ret as byte[])
    return ret as byte[]
  }

  /**
   * GZips a given message
   *
   * @param message
   * @return
   */
  private byte[] gzipMessage(String message) {
    def targetStream = new ByteArrayOutputStream()
    def zipStream = new java.util.zip.GZIPOutputStream(targetStream)
    zipStream.write(message.getBytes(USED_CHARSET))
    zipStream.close()
    def zipped = targetStream.toByteArray()
    targetStream.close()
    return zipped
  }

  /**
   * Generate unique Message ID
   * @return
   */
  private String generateMessageId() {
     MessageDigest digest = MessageDigest.getInstance("MD5")
     digest.update(System.currentTimeMillis().byteValue())
     BigInteger big = new BigInteger(1,digest.digest())
     big.toString(16).padLeft(32,"0")[0..7]
  }

  protected String format(int intval) {
    String formatted = Integer.toHexString(intval)
    StringBuffer buf = new StringBuffer("00000000")
    buf.replace(8 - formatted.length(), 8, formatted)
    return buf.toString()
  }

  protected static String format(short shortval) {
    String formatted = Integer.toHexString(shortval)
    StringBuffer buf = new StringBuffer("0000")
    buf.replace(4 - formatted.length(), 4, formatted)
    return buf.toString()
  }

  /**
   * Unique across JVMs on this machine (unless they load this class
   * in the same quater second - very unlikely)
   */
  protected int getJVM() {
    return JVM
  }

  /**
   * Unique in a millisecond for this JVM instance (unless there
   * are > Short.MAX_VALUE instances created in a millisecond)
   */
  protected short getCount() {
    synchronized (this.class) {
      if (counter < 0) counter = 0
      return counter++
    }
  }

  /**
   * Unique in a local network
   */
  protected int getIP() {
    return IP
  }

  /**
   * Unique down to millisecond
   */
  protected short getHiTime() {
    return (short) (System.currentTimeMillis() >>> 32)
  }

  protected int getLoTime() {
    return (int) System.currentTimeMillis()
  }

  protected String format(int intval, int lastDigits) {
    String formatted = Integer.toHexString(intval)
    formatted = formatted.substring(0, formatted.size() >= lastDigits ? lastDigits : formatted.size())
    return formatted.padLeft(lastDigits, '0')
  }

  protected static String format(short shortval, int lastDigits) {
    String formatted = Integer.toHexString(shortval)
    formatted = formatted.substring(0, formatted.size() >= lastDigits ? lastDigits : formatted.size())
    return formatted.padLeft(lastDigits, '0')
  }
}
