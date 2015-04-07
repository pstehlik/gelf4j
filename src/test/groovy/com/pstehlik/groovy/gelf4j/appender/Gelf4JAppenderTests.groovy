package com.pstehlik.groovy.gelf4j.appender

import com.pstehlik.groovy.test.BaseUnitTest
import org.apache.log4j.Category
import org.apache.log4j.Level
import org.apache.log4j.MDC
import org.apache.log4j.spi.LocationInfo
import org.apache.log4j.spi.LoggingEvent
import org.junit.After
import org.junit.Before
import org.junit.Test

import static org.junit.Assert.assertEquals
import static org.junit.Assert.assertNull

/**
 * @author Philip Stehlik
 * @since 0.82
 */
class Gelf4JAppenderTests extends BaseUnitTest {

  Gelf4JAppender appender

  @Before
  void setUp() {
    super.setUp()
    registerMetaClass(Gelf4JAppender)
    appender = new Gelf4JAppender()
    appender.layout = new org.apache.log4j.PatternLayout()
    appender.layout.conversionPattern = '%d{ABSOLUTE} %5p %c{1}:%L - %m%n'
  }

  @After
  void tearDown() {
    super.tearDown()
    appender = null
  }

  private Category getCat() {
    new Category('testCategory')
  }

  @Test
  void testBasicMessageBuilding() {

    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
      'someMessage',
      null
    )
    def res = appender.createGelfMapFromLoggingEvent(le)
    assert 'GELF' == res['_facility']
    assert null == res.file
    assert res.full_message.contains('someMessage')
    assert InetAddress.getLocalHost().getHostName() == res.host
    assert '4' == res.level
    assert null == res['_line']
    assert res.short_message.contains('someMessage')
    int gelfTimeStamp = Math.floor(le.getTimeStamp() / 1000)
    assert gelfTimeStamp as String == res.timestamp
    assert '1.1' == res.version
  }

  @Test
  void testMessageBuildingWithExceptions() {
    try {
      throw new Exception('Some thrown up in here')
    } catch (Exception ex) {
      LoggingEvent le = new LoggingEvent(
        this.class.name,
        cat,
        Level.WARN,
        'some message before exception',
        ex
      )
      println le.getRenderedMessage()
      def res = appender.createGelfMapFromLoggingEvent(le)
      println res.full_message
    }
  }

  @Test
  void testMessageBuildingWithExceptionsOfOver500StackTraceLines() {

    registerMetaClass(Gelf4JAppender)
    Gelf4JAppender.metaClass.getMaxLoggedLines = { -> 10 }

    def ex = new Throwable('Some Exception')
    assert ex.getStackTrace().length > 10
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
      'some message before exception',
      ex
    )
    assertEquals 12, appender.createGelfMapFromLoggingEvent(le).full_message.split('\n').size()

  }


  @Test
  void testIncludeLocationInformation() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      System.currentTimeMillis(),
      Level.WARN,
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
    assert null == res.file
    assert null == res.line

    appender.includeLocationInformation = true
    res = appender.createGelfMapFromLoggingEvent(le)
    assert 'mySourceFile.groovy' == res['_file']
    assert '200' == res['_line']
  }


  @Test
  void testAdditionalFieldHandling() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
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

  @Test
  void testAdditionalFieldJsonHandling() {
    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
      'someMessage',
      null
    )
    appender.additionalFieldsJson = '{"someField":"someValue","id":"idValue","_prefixedField":"prefixedValue"}'
    def res = appender.createGelfMapFromLoggingEvent(le)
    assertEquals 'someValue',res._someField
    assertNull res._id
    assertEquals 'prefixedValue',res._prefixedField
  }

  @Test
  void testAllSurroundingCatch() {
    appender.metaClass.createGelfMapFromLoggingEvent = { LoggingEvent loggingEvent ->
      throw new IllegalStateException('error should not break stuff')
    }
    def res = appender.append(null)
    assertNull res
  }


  @Test
  void testExceptionMessagePartOfLog() {
    appender.metaClass.createGelfMapFromLoggingEvent = { LoggingEvent loggingEvent ->
      throw new IllegalStateException('error should not break stuff')
    }
    def res = appender.append(null)
    assertNull res
  }

  @Test
  void testMdcFields() {
    appender.mdcFields = ['mdc', 'someField']

    MDC.put('someField', 100)

    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
      'someMessage',
      null
    )

    def res = appender.createGelfMapFromLoggingEvent(le)

    assert res._someField == '100'
    assert !res.containsKey('_mdc')

  }

  @Test
  void testMdcFieldsJson() {
    appender.mdcFieldsJson = '["mdc", "someField"]'

    MDC.put('someField', 100)

    LoggingEvent le = new LoggingEvent(
      this.class.name,
      cat,
      Level.WARN,
      'someMessage',
      null
    )

    def res = appender.createGelfMapFromLoggingEvent(le)

    assert res._someField == '100'
    assert !res.containsKey('_mdc')
  }
}
