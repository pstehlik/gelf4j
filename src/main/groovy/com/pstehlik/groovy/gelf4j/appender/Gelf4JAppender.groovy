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
import org.apache.log4j.spi.ThrowableInformation

/**
 * Log4J appender to log to Graylog2 via GELF
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class Gelf4JAppender extends AppenderSkeleton {

  public static final Integer MAX_LOGGED_LINES = 500
  public static final String UNKNOWN_HOST = 'unknown_host'

  //---------------------------------------
  //configuration settings for the appender
  public Map additionalFields = null
  public String facility = null
  public String graylogServerHost = 'localhost'
  public Integer graylogServerPort = 12201
  public String host = null
  public Boolean includeLocationInformation = false
  public Boolean logStackTraceFromMessage = false
  public Integer maxChunkSize = 8154
  public List mdcFields = null
  public String mdcFieldsJson = null
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

    GelfMessageBuilder messageBuilder = new GelfMessageBuilder(this, loggingEvent)
    messageBuilder.setField('host', loggingHostName)
    messageBuilder.setAdditionalField('facility', facility ?: 'GELF')

    //add additional fields and prepend with _ if not present already
    additionalFields.each {
      messageBuilder.setAdditionalField(it.key as String, it.value as String)
    }

    mdcFields.each {
      def mdcValue = loggingEvent.getMDC(it as String)
      if (mdcValue != null) {
        messageBuilder.setAdditionalField(it as String, mdcValue as String)
      }
    }

    messageBuilder.build()

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

  public void setAdditionalFieldsJson(String json) {
    if (json) {
      this.additionalFields = [:]
      JSONValue.parse(json).each {
        this.additionalFields.put(it.key, it.value)
      }
    }
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

  public void setLogStackTraceFromMessage(String trueFalse) {
    logStackTraceFromMessage = Boolean.parseBoolean(trueFalse)
  }

  public void setMdcFields(List mdcFields) {
    this.mdcFields = mdcFields
  }

  public void setMdcFieldsJson(String json) {
    this.mdcFieldsJson = json
    if (this.mdcFieldsJson) {
      this.mdcFields = []
      JSONValue.parse(this.mdcFieldsJson).each {
        this.mdcFields.add(it)
      }
    }
  }

  int getMaxLoggedLines() {
    MAX_LOGGED_LINES
  }

}
