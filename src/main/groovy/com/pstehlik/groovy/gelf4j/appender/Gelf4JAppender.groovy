/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.gelf4j.appender

import org.apache.log4j.spi.LoggingEvent
import org.apache.log4j.AppenderSkeleton

import com.pstehlik.groovy.gelf4j.net.GelfTransport
import org.json.simple.JSONValue

/**
 * Log4J appender to log to Graylog2 via GELF
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class Gelf4JAppender
extends AppenderSkeleton {
  public static final Integer SHORT_MESSAGE_LENGTH = 250
  public static final String UNKNOWN_HOST = 'unknown_host'

  //---------------------------------------
  //configuration settings for the appender
  public String graylogServerHost = 'localhost'
  public int graylogServerPort = 12201
  public boolean includeLocationInformation = false
  //---------------------------------------

  protected void append(LoggingEvent loggingEvent) {
    String gelfJsonString = createGelfJsonFromLoggingEvent(loggingEvent)
    GelfTransport.sendGelfMessageToGraylog(this, gelfJsonString)
  }

  void close() {
    //nothing to close
  }

  boolean requiresLayout() {
    return false
  }

  /**
   * Creates the JSON String for a given <code>LoggingEvent</code>.
   * The 'short_message' of the GELF message is max 50 chars long.
   *
   * @param loggingEvent The logging event to base the JSON creation on
   * @return The JSON String created from <code>loggingEvent</code>
   */
  private String createGelfJsonFromLoggingEvent(LoggingEvent loggingEvent) {
    String fullMessage = loggingEvent.getMessage()
    String shortMessage = fullMessage
    if (shortMessage.length() > SHORT_MESSAGE_LENGTH) {
      shortMessage = shortMessage.substring(0, SHORT_MESSAGE_LENGTH - 1)
    }
    def gelfMessage = [
      'level': "${loggingEvent.getLevel().getSyslogEquivalent()}",
      "short_message": shortMessage,
      "full_message": fullMessage,
      "host": loggingHostName,
      "file": '',
      "line": '',
      "version": "1"
    ]
    //only set location information if configured
    if(includeLocationInformation){
      gelfMessage.file = loggingEvent.getLocationInformation().fileName
      gelfMessage.line = loggingEvent.getLocationInformation().lineNumber
    }
    return JSONValue.toJSONString(gelfMessage as Map)
  }

  /**
   * Determine local host name that the GELF message will originate from
   * @return
   */
  private String getLoggingHostName() {
    String ret
    try {
      ret = InetAddress.getLocalHost().getHostName()
    } catch (UnknownHostException e) {
      ret = UNKNOWN_HOST
    }
    return ret
  }
}
