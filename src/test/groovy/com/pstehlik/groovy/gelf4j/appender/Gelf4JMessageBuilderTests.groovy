/*
 * Copyright (c) 2015. Taulia Inc. All rights reserved.
 *
 * All content of this file, its package and related information is to be treated
 * as confidential, proprietary information.
 *
 * This notice does not imply restricted, unrestricted or public access to these materials
 * which are a trade secret of Taulia Inc ('Taulia') and which may not be reproduced, used,
 * sold or transferred to any third party without Taulia's prior written consent.
 *
 * Any rights not expressly granted herein are reserved by Taulia.
 */

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
