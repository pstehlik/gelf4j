package com.pstehlik.groovy.gelf4j.appender

import org.apache.log4j.Category
import org.apache.log4j.Level
import org.apache.log4j.spi.LoggingEvent
import org.junit.Test

import static org.junit.Assert.assertEquals

class Gelf4JMessageBuilderTests {

  @Test
  void testShortMessageHandling() {

    def map = new GelfMessageBuilder(new Gelf4JAppender(),
      new LoggingEvent(
        this.class.name,
        new Category('testCategory'),
        Level.WARN,
        'some long message. ' * 20,
        (Throwable) null
      )
    ).build()
    assert map['full_message'].size() >= GelfMessageBuilder.SHORT_MESSAGE_LENGTH
    assertEquals GelfMessageBuilder.SHORT_MESSAGE_LENGTH, map['short_message'].size()

  }

}
