/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.graylog

import org.apache.log4j.helpers.LogLog

/**
 * Connection handling for sending data to Graylog
 *
 * @author Philip Stehlik
 * @since 0.7
 */
class Graylog2UdpSender {
  /**
   * Sends data via UDP to a host at a given port
   *
   * @param data
   * @param hostname
   * @param port
   */
  public static void sendPacket(byte[] data, String hostname, Integer port){
    DatagramSocket socket
    def address
    try{
      address = InetAddress.getByName(hostname)
      def packet = new DatagramPacket(data, data.length, address, port)
      socket = new DatagramSocket()
      socket.send(packet)
    } catch(UnknownHostException ex) {
      LogLog.error("Could determine address for [${hostname}]", ex)
    } catch(IOException ex) {
      LogLog.error("Error when sending data to  [${address}/${port}]", ex)
    } finally {
      socket?.close()
    }
  }
}
