/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.gelf4j.appender

import org.apache.log4j.spi.LoggingEvent
import org.apache.log4j.AppenderSkeleton

import com.pstehlik.groovy.gelf4j.net.GelfTransport
import org.json.simple.JSONValue
import org.apache.log4j.Layout
import org.apache.log4j.helpers.LogLog

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
  private static final String GELF_VERSION = "1.0"

  //---------------------------------------
  //configuration settings for the appender
  public Map additionalFields = null
  public String facility = null
  public String graylogServerHost = 'localhost'
  public Integer graylogServerPort = 12201
  public String host = null
  public Boolean includeLocationInformation = false
  public Integer maxChunkSize = 8154
  //---------------------------------------

  private GelfTransport _gelfTransport

  protected void append(LoggingEvent loggingEvent) {
    try {
      String gelfJsonString = createGelfJsonFromLoggingEvent(loggingEvent)
      gelfTransport.sendGelfMessageToGraylog(this, gelfJsonString)
    } catch (Exception ex) {
      LogLog.error("An unexpected error occured", ex)
    }
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
   * Message building and skipping of additional fields etc is based on
   * https://github.com/Graylog2/graylog2-docs/wiki/GELF from Jan 7th 2011.
   *
   * @param loggingEvent The logging event to base the JSON creation on
   * @return The JSON String created from <code>loggingEvent</code>
   */
  private String createGelfJsonFromLoggingEvent(LoggingEvent loggingEvent) {
    Map gelfMessage = createGelfMapFromLoggingEvent(loggingEvent)
    return JSONValue.toJSONString(gelfMessage as Map)
  }

  Map createGelfMapFromLoggingEvent(LoggingEvent loggingEvent) {
    String fullMessage = layout ? layout.format(loggingEvent) : loggingEvent.getMessage()
    if (layout) {
      if (layout.ignoresThrowable()) {
        String[] s = loggingEvent.getThrowableStrRep()
        if (s != null) {
          int len = s.length
          for (int i = 0; i < len; i++) {
            fullMessage += s[i]
            fullMessage += Layout.LINE_SEP
          }
        }
      }
    }

    String shortMessage = fullMessage
    if (shortMessage.length() > SHORT_MESSAGE_LENGTH) {
      shortMessage = shortMessage.substring(0, SHORT_MESSAGE_LENGTH)
    }
    def gelfMessage = [
      "facility": facility ?: 'GELF',
      "file": '',
      "full_message": fullMessage,
      "host": loggingHostName,
      "level": "${loggingEvent.getLevel().getSyslogEquivalent()}" as String,
      "line": '',
      "short_message": shortMessage,
      "timestamp": loggingEvent.getTimeStamp(),
      "version": GELF_VERSION
    ]
    //only set location information if configured
    if (includeLocationInformation) {
      gelfMessage.file = loggingEvent.getLocationInformation().fileName
      gelfMessage.line = loggingEvent.getLocationInformation().lineNumber
    }
    //add additional fields and prepend with _ if not present already
    if (additionalFields != null) {
      additionalFields.each {
        String key = it.key
        if (!key.startsWith('_')) {
          key = '_' + key
        }
        //skip additional field called '_id'
        if (key != '_id') {
          gelfMessage[key] = it.value as String
        }
      }
    }
    return gelfMessage
  }

  /**
   * Determine local host name that the GELF message will originate from
   * @return
   */
  private String getLoggingHostName() {
    String ret = host
    if (ret == null) {
      try {
        ret = InetAddress.getLocalHost().getHostName()
      } catch (UnknownHostException e) {
        ret = UNKNOWN_HOST
      }
    }
    return ret
  }


  private GelfTransport getGelfTransport() {
    if (!_gelfTransport) {
      _gelfTransport = new GelfTransport()
    }
    return _gelfTransport
  }

  public void setAdditionalFields(Map fields) {
    additionalFields = fields
  }

  public void setFacility(String fac) {
    facility = fac
  }

  public void setGraylogServerHost(String srvHost) {
    graylogServerHost = srvHost
  }

  public void setGraylogServerPort(Integer srvPort) {
    graylogServerPort = srvPort
  }

  public void setHost(String hostName) {
    host = hostName
  }

  public void setIncludeLocationInformation(Boolean includeLocation) {
    includeLocationInformation = includeLocation
  }

  public void setMaxChunkSize(Integer maxChunk) {
    if (maxChunk > 8154) {
      throw new IllegalArgumentException("Can not configure maxChunkSize > 8154")
    }
    maxChunkSize = maxChunk
  }
}
