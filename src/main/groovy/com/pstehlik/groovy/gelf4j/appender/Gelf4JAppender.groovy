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
  public static final String UNKNOWN_HOST = 'unknown_host'
  public static final Integer SHORT_MESSAGE_LENGTH = 50

  //configuration of the appender
  public String graylogServerHost = 'localhost'
  public int graylogServerPort = 12201
  public boolean includeLocationInformation = false

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

  private String mapToJsonString(Map gelfMessage){
    String json = "{"
    gelfMessage.each{
      String addElement = "\"${it.key}\":"
      String addValue = it.value.toString().replaceAll("\\\\","\\\\\\\\")
      addValue = addValue.replaceAll(/"/,/\\"/)
      addElement += "\"${addValue}\""
      addElement += ','
      json += addElement
    }
    json = json.substring(0, json.length()- 1)
    json += "}"
    return json
  }

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
