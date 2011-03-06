package com.pstehlik.groovy.gelf4j.appender

import org.junit.Test
import static org.junit.Assert.assertEquals
import static org.junit.Assert.assertNull
import org.apache.log4j.spi.LoggingEvent
import org.apache.log4j.Priority
import org.apache.log4j.spi.LocationInfo

/**
 * @author Philip Stehlik
 * @since 0.82
 */
class Gelf4JAppenderTests {
  Gelf4JAppender appender

  @org.junit.Before
  void setUp() {
    appender = new Gelf4JAppender()
    appender.layout = new org.apache.log4j.PatternLayout()
    appender.layout.conversionPattern = '%d{ABSOLUTE} %5p %c{1}:%L - %m%n'
  }

  @org.junit.After
  void tearDown() {
    appender = null
  }

  private org.apache.log4j.Category getCat() {
    return new org.apache.log4j.Category('testCategory')
  }

  @Test
  void testBasicMessageBuilding() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Priority.WARN,
      'someMessage',
      null
    )
    def res = appender.createGelfMapFromLoggingEvent(le)
    assertEquals 'GELF', res.facility
    assertEquals '', res.file
    assert res.full_message.contains('someMessage')
    assertEquals InetAddress.getLocalHost().getHostName(), res.host
    assertEquals '4', res.level
    assertEquals '', res.line
    assert res.short_message.contains('someMessage')
    assertEquals le.getTimeStamp(), res.timestamp
    assertEquals '1.0', res.version
  }

  @Test
  void testMessageBuildingWithExceptions() {
    try {
      throw new Exception('Some thrown up in here')
    } catch (Exception ex) {
      LoggingEvent le = new LoggingEvent(
        this.class.name,
        cat,
        Priority.WARN,
        'some message before exception',
        ex
      )
      println le.getRenderedMessage()
      def res = appender.createGelfMapFromLoggingEvent(le)
      println res.full_message
    }
  }

  @Test
  void testShortMessageHandling() {
    def longMessage = '''some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message.
some long message. some long message. some long message. some long message. some long message. '''
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Priority.WARN,
      longMessage,
      null
    )
    def res = appender.createGelfMapFromLoggingEvent(le)
    assert res.full_message.size() >= Gelf4JAppender.SHORT_MESSAGE_LENGTH
    assertEquals Gelf4JAppender.SHORT_MESSAGE_LENGTH, res.short_message.size()
  }

  @Test
  void testIncludeLocationInformation() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      System.currentTimeMillis(),
      Priority.WARN,
      'someMessage',
      'thread name',
      null,
      '',
      new LocationInfo(
        'mySourceFile.groovy',
        '',
        '',
        '200'
      ),
      [:]
    )
    appender.includeLocationInformation = false
    def res = appender.createGelfMapFromLoggingEvent(le)
    assertEquals '', res.file
    assertEquals '', res.line

    appender.includeLocationInformation = true
    res = appender.createGelfMapFromLoggingEvent(le)
    assertEquals 'mySourceFile.groovy', res.file
    assertEquals '200', res.line
  }


  @Test
  void testAdditionalFieldHandling() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Priority.WARN,
      'someMessage',
      null
    )
    appender.additionalFields = [
      'someField':'someValue',
      'id':'idValue',
      '_prefixedField':'prefixedValue'
    ]
    def res = appender.createGelfMapFromLoggingEvent(le)
    assertEquals 'someValue',res._someField
    assertNull res._id
    assertEquals 'prefixedValue',res._prefixedField
  }
}
