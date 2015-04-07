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

package com.pstehlik.groovy.gelf4j.net

import org.junit.Test

class GelfChunkBuilderTests {

  @Test
  void testSimpleChunking() {

    byte[] message = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9].toArray() as byte[]
    def builder = new GelfChunkBuilder(3, 4)
    List<byte[]> chunks = []
    builder.chunkNumber = 0
    chunks << builder.buildChunk(message)
    builder.chunkNumber = 1
    chunks << builder.buildChunk(message)
    builder.chunkNumber = 2
    chunks << builder.buildChunk(message)

    assert chunks[0][2..9] == chunks[1][2..9]
    assert chunks[1][2..9] == chunks[2][2..9]
    (0..2).each { int it ->
      assert chunks[it][10] == it.byteValue()
    }

    chunks.each {
      assert it[0] == 0x1e as byte
      assert it[1] == 0x0f as byte
      assert it[11] == 3 as byte
    }

    assert chunks[0].size() == 16
    assert chunks[1].size() == 16
    assert chunks[2].size() == 14

  }

}
