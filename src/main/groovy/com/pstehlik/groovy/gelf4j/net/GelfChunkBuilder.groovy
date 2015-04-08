package com.pstehlik.groovy.gelf4j.net

import java.nio.ByteBuffer

class GelfChunkBuilder {

  static final int MAX_CHUNK_COUNT = 128

  static final int HEADER_LENGTH = 12

  static final int CHUNK_NUMBER_POSITION = 10

  ByteBuffer header

  int maxChunkSize

  int totalChunkCount

  GelfChunkBuilder(int totalChunkCount, int maxChunkSize) {

    if (totalChunkCount > MAX_CHUNK_COUNT) {
      throw new IllegalArgumentException("Chunk count cannot exceed ${MAX_CHUNK_COUNT}: [${totalChunkCount}]")
    }

    this.maxChunkSize = maxChunkSize
    this.totalChunkCount = totalChunkCount
    header = ByteBuffer.allocate(HEADER_LENGTH)

      .put(0x1e.byteValue())                            // The GELF magic numbers
      .put(0x0f.byteValue())
      .putLong(UUID.randomUUID().mostSignificantBits)   // a new random Message GUID
      .put(0.byteValue())                               // the current chunk number
      .put(totalChunkCount.byteValue())                 // the total chunk count

  }

  void setChunkNumber(int chunkNumber) {

    if (chunkNumber > totalChunkCount - 1) {
      throw new IllegalArgumentException("Chunk number cannot be bigger than total count [${totalChunkCount}]: ${chunkNumber}")
    }

    header.put(CHUNK_NUMBER_POSITION, chunkNumber.byteValue())

  }

  byte[] buildChunk(byte[] fullMessage) {

    if (fullMessage.size() > maxChunkSize * totalChunkCount) {
      throw new IllegalArgumentException("Total message cannot be bigger than total chunk * chunk size [${maxChunkSize * totalChunkCount}]: [${fullMessage.size()}]")
    }

    byte[] headerBytes = header.array()
    int currentChunk = headerBytes[CHUNK_NUMBER_POSITION] as int
    int chunkLength = maxChunkSize
    if (currentChunk == totalChunkCount - 1) {
      chunkLength = fullMessage.size() - (currentChunk * maxChunkSize)
    }

    ByteBuffer chunk = ByteBuffer.allocate(HEADER_LENGTH + chunkLength)
    chunk.put(headerBytes)
    chunk.put(fullMessage, currentChunk * maxChunkSize, chunkLength)
    chunk.array()

  }

}
