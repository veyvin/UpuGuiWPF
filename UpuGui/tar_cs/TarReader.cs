// Decompiled with JetBrains decompiler
// Type: tar_cs.TarReader
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System.Collections.Generic;
using System.IO;

namespace tar_cs
{
  public class TarReader
  {
    private readonly byte[] dataBuffer = new byte[512];
    private readonly UsTarHeader header;
    private readonly Stream inStream;
    private long remainingBytesInFile;

    public ITarHeader FileInfo
    {
      get
      {
        return (ITarHeader) this.header;
      }
    }

    public TarReader(Stream tarredData)
    {
      this.inStream = tarredData;
      this.header = new UsTarHeader();
    }

    public void ReadToEnd(string destDirectory)
    {
      while (this.MoveNext(false))
      {
        string fileName1 = this.FileInfo.FileName;
        string path = destDirectory + (object) Path.DirectorySeparatorChar + fileName1;
        if (UsTarHeader.IsPathSeparator(fileName1[fileName1.Length - 1]) || this.FileInfo.EntryType == EntryType.Directory)
        {
          Directory.CreateDirectory(path);
        }
        else
        {
          string fileName2 = Path.GetFileName(path);
          Directory.CreateDirectory(path.Remove(path.Length - fileName2.Length));
          using (FileStream fileStream = File.Create(path))
            this.Read((Stream) fileStream);
        }
      }
    }

    public void Read(Stream dataDestanation)
    {
      byte[] buffer;
      int count;
      while ((count = this.Read(out buffer)) != -1)
        dataDestanation.Write(buffer, 0, count);
    }

    protected int Read(out byte[] buffer)
    {
      if (this.remainingBytesInFile == 0L)
      {
        buffer = (byte[]) null;
        return -1;
      }
      int num1 = -1;
      long num2;
      if (this.remainingBytesInFile - 512L > 0L)
      {
        num2 = 512L;
      }
      else
      {
        num1 = 512 - (int) this.remainingBytesInFile;
        num2 = this.remainingBytesInFile;
      }
      int num3 = this.inStream.Read(this.dataBuffer, 0, (int) num2);
      this.remainingBytesInFile -= (long) num3;
      if (this.inStream.CanSeek && num1 > 0)
      {
        this.inStream.Seek((long) num1, SeekOrigin.Current);
      }
      else
      {
        for (; num1 > 0; --num1)
          this.inStream.ReadByte();
      }
      buffer = this.dataBuffer;
      return num3;
    }

    private static bool IsEmpty(IEnumerable<byte> buffer)
    {
      foreach (int num in buffer)
      {
        if (num != 0)
          return false;
      }
      return true;
    }

    public bool MoveNext(bool skipData)
    {
      if (this.remainingBytesInFile > 0L)
      {
        if (!skipData)
          throw new TarException("You are trying to change file while not all the data from the previous one was read. If you do want to skip files use skipData parameter set to true.");
        if (this.inStream.CanSeek)
        {
          long num = this.remainingBytesInFile % 512L;
          this.inStream.Seek(this.remainingBytesInFile + (512L - (num == 0L ? 512L : num)), SeekOrigin.Current);
        }
        else
        {
          byte[] buffer;
          while (this.Read(out buffer) != -1)
            ;
        }
      }
      byte[] bytes = this.header.GetBytes();
      if (this.inStream.Read(bytes, 0, this.header.HeaderSize) < 512)
        throw new TarException("Can not read header");
      if (TarReader.IsEmpty((IEnumerable<byte>) bytes))
      {
        if (this.inStream.Read(bytes, 0, this.header.HeaderSize) == 512 && TarReader.IsEmpty((IEnumerable<byte>) bytes))
          return false;
        throw new TarException("Broken archive");
      }
      if (this.header.UpdateHeaderFromBytes())
        throw new TarException("Checksum check failed");
      this.remainingBytesInFile = this.header.SizeInBytes;
      return true;
    }
  }
}
