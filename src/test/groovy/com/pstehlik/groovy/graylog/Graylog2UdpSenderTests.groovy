package com.pstehlik.groovy.graylog

import com.pstehlik.groovy.gelf4j.appender.Gelf4JAppender
import com.pstehlik.groovy.test.BaseUnitTest
import org.junit.Test

/**
 * Description missing
 *
 * @author Philip Stehlik
 * @since
 */
class Graylog2UdpSenderTests
extends BaseUnitTest{
  @org.junit.Before
  void setUp() {
    super.setUp()
    registerMetaClass(Gelf4JAppender)
  }

  @org.junit.After
  void tearDown() {
    super.tearDown()
  }

  @Test
  void testSendDataShouldNotFail_getHostName(){
    registerMetaClass InetAddress
    InetAddress.metaClass.'static'.getByName = {String name ->
      throw new UnknownHostException("can not find ${name}")
    }
    Graylog2UdpSender.sendPacket('lala'.bytes, 'someWeirdHostName', 999)
  }

  @Test
  void testSendDataShouldNotFail_DatagramSocket(){
    registerMetaClass DatagramSocket

    DatagramSocket.metaClass.send = {DatagramPacket packet ->
      throw new IOException('test exception')
    }
    Graylog2UdpSender.sendPacket('lala'.bytes, 'www.google.com', 999)
  }


}