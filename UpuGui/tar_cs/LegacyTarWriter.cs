// Decompiled with JetBrains decompiler
// Type: tar_cs.LegacyTarWriter
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.IO;
using System.Threading;

namespace tar_cs
{
  public class LegacyTarWriter : IDisposable
  {
    protected byte[] buffer = new byte[1024];
    public bool ReadOnZero = true;
    private readonly Stream outStream;
    private bool isClosed;

    protected virtual Stream OutStream
    {
      get
      {
        return this.outStream;
      }
    }

    public LegacyTarWriter(Stream writeStream)
    {
      this.outStream = writeStream;
    }

    public void Dispose()
    {
      this.Close();
    }

    public void WriteDirectoryEntry(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("path");
      if ((int) path[path.Length - 1] != 47)
        path += (string) (object) '/';
      DateTime lastModificationTime = !Directory.Exists(path) ? DateTime.Now : Directory.GetLastWriteTime(path);
      this.WriteHeader(path, lastModificationTime, 0L, 101, 101, 777, EntryType.Directory);
    }

    public void WriteDirectory(string directory, bool doRecursive)
    {
      if (string.IsNullOrEmpty(directory))
        throw new ArgumentNullException("directory");
      this.WriteDirectoryEntry(directory);
      foreach (string fileName in Directory.GetFiles(directory))
        this.Write(fileName);
      foreach (string str in Directory.GetDirectories(directory))
      {
        this.WriteDirectoryEntry(str);
        if (doRecursive)
          this.WriteDirectory(str, true);
      }
    }

    public void Write(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      using (FileStream fileStream = File.OpenRead(fileName))
        this.Write((Stream) fileStream, fileStream.Length, fileName, 61, 61, 511, File.GetLastWriteTime(fileStream.Name));
    }

    public void Write(FileStream file)
    {
      string name = Path.GetFullPath(file.Name).Replace(Path.GetPathRoot(file.Name), string.Empty).Replace(Path.DirectorySeparatorChar, '/');
      this.Write((Stream) file, file.Length, name, 61, 61, 511, File.GetLastWriteTime(file.Name));
    }

    public void Write(Stream data, long dataSizeInBytes, string name)
    {
      this.Write(data, dataSizeInBytes, name, 61, 61, 511, DateTime.Now);
    }

    public virtual void Write(string name, long dataSizeInBytes, int userId, int groupId, int mode, DateTime lastModificationTime, WriteDataDelegate writeDelegate)
    {
      IArchiveDataWriter writer = (IArchiveDataWriter) new DataWriter(this.OutStream, dataSizeInBytes);
      this.WriteHeader(name, lastModificationTime, dataSizeInBytes, userId, groupId, mode, EntryType.File);
      while (writer.CanWrite)
        writeDelegate(writer);
      this.AlignTo512(dataSizeInBytes, false);
    }

    public virtual void Write(Stream data, long dataSizeInBytes, string name, int userId, int groupId, int mode, DateTime lastModificationTime)
    {
      if (this.isClosed)
        throw new TarException("Can not write to the closed writer");
      this.WriteHeader(name, lastModificationTime, dataSizeInBytes, userId, groupId, mode, EntryType.File);
      this.WriteContent(dataSizeInBytes, data);
      this.AlignTo512(dataSizeInBytes, false);
    }

    protected void WriteContent(long count, Stream data)
    {
      while (count > 0L && count > (long) this.buffer.Length)
      {
        int count1 = data.Read(this.buffer, 0, this.buffer.Length);
        if (count1 < 0)
          throw new IOException("LegacyTarWriter unable to read from provided stream");
        if (count1 == 0)
        {
          if (this.ReadOnZero)
            Thread.Sleep(100);
          else
            break;
        }
        this.OutStream.Write(this.buffer, 0, count1);
        count -= (long) count1;
      }
      if (count <= 0L)
        return;
      int count2 = data.Read(this.buffer, 0, (int) count);
      if (count2 < 0)
        throw new IOException("LegacyTarWriter unable to read from provided stream");
      if (count2 == 0)
      {
        for (; count > 0L; --count)
          this.OutStream.WriteByte((byte) 0);
      }
      else
        this.OutStream.Write(this.buffer, 0, count2);
    }

    protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode, EntryType entryType)
    {
      TarHeader tarHeader = new TarHeader()
      {
        FileName = name,
        LastModification = lastModificationTime,
        SizeInBytes = count,
        UserId = userId,
        GroupId = groupId,
        Mode = mode,
        EntryType = entryType
      };
      this.OutStream.Write(tarHeader.GetHeaderValue(), 0, tarHeader.HeaderSize);
    }

    public void AlignTo512(long size, bool acceptZero)
    {
      size %= 512L;
      if (size == 0L && !acceptZero)
        return;
      for (; size < 512L; ++size)
        this.OutStream.WriteByte((byte) 0);
    }

    public virtual void Close()
    {
      if (this.isClosed)
        return;
      this.AlignTo512(0L, true);
      this.AlignTo512(0L, true);
      this.isClosed = true;
    }
  }
}
