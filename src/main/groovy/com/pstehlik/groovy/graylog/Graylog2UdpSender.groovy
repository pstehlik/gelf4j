/*
 * Copyright (c) 2011 - Philip Stehlik - p [at] pstehlik [dot] com
 * Licensed under Apache 2 license - See LICENSE for details
 */
package com.pstehlik.groovy.graylog

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
    try{
      def address = InetAddress.getByName(hostname)
      def packet = new DatagramPacket(data, data.length, address, port)
      socket = new DatagramSocket()
      socket.send(packet)
    } finally {
      socket?.close()
    }
  }
}
