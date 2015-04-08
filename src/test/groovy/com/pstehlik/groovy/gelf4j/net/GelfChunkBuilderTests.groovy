package com.pstehlik.groovy.gelf4j.net

import org.junit.Test

class GelfChunkBuilderTests {

  @Test
  void testSimpleChunking() {

    byte[] message = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9].toArray() as byte[]
    def builder = new GelfChunkBuilder(3, 4)
    List<byte[]> chunks = []
    (0..2).each {
      builder.chunkNumber = it
      chunks << builder.buildChunk(message)
    }

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

  @Test(expected = IllegalArgumentException)
  void testSanityCheckOnCreation() {
    new GelfChunkBuilder(129, 10)
  }

  @Test(expected = IllegalArgumentException)
  void testSanityCheckOnChunkBuilding() {
    def builder = new GelfChunkBuilder(4, 10)
    builder.buildChunk(new byte[50])
  }

}
