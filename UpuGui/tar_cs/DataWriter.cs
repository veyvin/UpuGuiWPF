// Decompiled with JetBrains decompiler
// Type: tar_cs.DataWriter
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System.IO;

namespace tar_cs
{
  internal class DataWriter : IArchiveDataWriter
  {
    private bool canWrite = true;
    private readonly long size;
    private long remainingBytes;
    private readonly Stream stream;

    public bool CanWrite
    {
      get
      {
        return this.canWrite;
      }
    }

    public DataWriter(Stream data, long dataSizeInBytes)
    {
      this.size = dataSizeInBytes;
      this.remainingBytes = this.size;
      this.stream = data;
    }

    public int Write(byte[] buffer, int count)
    {
      if (this.remainingBytes == 0L)
      {
        this.canWrite = false;
        return -1;
      }
      int count1 = this.remainingBytes - (long) count >= 0L ? count : (int) this.remainingBytes;
      this.stream.Write(buffer, 0, count1);
      this.remainingBytes -= (long) count1;
      return count1;
    }
  }
}
