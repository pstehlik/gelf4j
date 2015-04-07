package com.pstehlik.groovy.gelf4j.appender

import org.apache.log4j.Layout
import org.apache.log4j.spi.LoggingEvent
import org.apache.log4j.spi.ThrowableInformation

class GelfMessageBuilder {

  private static final String GELF_VERSION = '1.1'

  public static final Integer SHORT_MESSAGE_LENGTH = 250

  Map<String, String> message

  GelfMessageBuilder(Gelf4JAppender appender, LoggingEvent loggingEvent) {

    message = [:]
    setField('level', "${loggingEvent.level.syslogEquivalent}" as String)
    setField('timestamp', new BigDecimal(Math.floor(loggingEvent.getTimeStamp() / 1000)) as String)
    setField('version', GELF_VERSION)

    if (appender.includeLocationInformation && loggingEvent.locationInformationExists()) {
      setAdditionalField('file', loggingEvent.locationInformation.fileName)
      setAdditionalField('line', loggingEvent.locationInformation.lineNumber)
      setAdditionalField('method_name', loggingEvent.locationInformation.methodName)
    }

    setAdditionalField('thread_name', loggingEvent.threadName)
    setFullMessage(appender, loggingEvent)

  }

  GelfMessageBuilder setFullMessage(Gelf4JAppender appender, LoggingEvent loggingEvent) {

    StringBuilder fullMessage = new StringBuilder(100 * appender.getMaxLoggedLines())
    String[] throwableAsString

    //detecting if the actual log-message is a throwable - because if so, might want the whole stacktrace
    if (loggingEvent.message instanceof Throwable) {
      if (appender.logStackTraceFromMessage) {
        def tI = new ThrowableInformation(loggingEvent.message as Throwable)
        throwableAsString = tI.throwableStrRep
      } else {
        fullMessage.append(appender.layout ? appender.layout.format(loggingEvent) : (loggingEvent.getMessage() as String))
      }
    } else {
      fullMessage.append(appender.layout ? appender.layout.format(loggingEvent) : loggingEvent.getMessage())
    }

    if (appender.layout == null || appender.layout.ignoresThrowable() || !fullMessage.length()) {
      throwableAsString = throwableAsString?:loggingEvent.getThrowableStrRep()
      if (throwableAsString != null) {
        int len = throwableAsString.length
        if(fullMessage.length()){ // newline after the 'original log message' to have nicer formatting
          fullMessage.append Layout.LINE_SEP
        }
        for (int i = 0; i < len && i <  appender.getMaxLoggedLines(); i++) {
          fullMessage.append throwableAsString[i]
          fullMessage.append Layout.LINE_SEP
        }
      }
    }

    if (!fullMessage.length()) { //failsave to prevent a 'null' or empty message even though it should(!) not happen
      fullMessage.append loggingEvent.message as String
    }

    setField('full_message', fullMessage.toString())

    String shortMessage = fullMessage.toString()
    if (shortMessage.length() > SHORT_MESSAGE_LENGTH) {
      shortMessage = shortMessage.substring(0, SHORT_MESSAGE_LENGTH)
    }
    setField('short_message', shortMessage)

    this

  }

  GelfMessageBuilder setField(String name, String value) {
    message[name] = value as String
    this
  }

  GelfMessageBuilder setAdditionalField(String name, String value) {

    if (!name.startsWith('_')) {
      name = '_' + name
    }
    //skip additional field called '_id'
    if (name == '_id') {
      return this
    }
    setField(name, value)

  }

  Map<String, String> build() {
    message
  }

}
