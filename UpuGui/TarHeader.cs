// Decompiled with JetBrains decompiler
// Type: tar_cs.TarHeader
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Net;
using System.Text;

namespace tar_cs
{
  internal class TarHeader : ITarHeader
  {
    private static byte[] spaces = Encoding.ASCII.GetBytes("        ");
    private readonly byte[] buffer = new byte[512];
    protected readonly DateTime TheEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
    private long headerChecksum;
    private string fileName;

    public EntryType EntryType { get; set; }

    public virtual string FileName
    {
      get
      {
        return this.fileName.Replace("\0", string.Empty);
      }
      set
      {
        if (value.Length > 100)
          throw new TarException("A file name can not be more than 100 chars long");
        this.fileName = value;
      }
    }

    public int Mode { get; set; }

    public string ModeString
    {
      get
      {
        return Convert.ToString(this.Mode, 8).PadLeft(7, '0');
      }
    }

    public int UserId { get; set; }

    public virtual string UserName
    {
      get
      {
        return this.UserId.ToString();
      }
      set
      {
        this.UserId = int.Parse(value);
      }
    }

    public string UserIdString
    {
      get
      {
        return Convert.ToString(this.UserId, 8).PadLeft(7, '0');
      }
    }

    public int GroupId { get; set; }

    public virtual string GroupName
    {
      get
      {
        return this.GroupId.ToString();
      }
      set
      {
        this.GroupId = int.Parse(value);
      }
    }

    public string GroupIdString
    {
      get
      {
        return Convert.ToString(this.GroupId, 8).PadLeft(7, '0');
      }
    }

    public long SizeInBytes { get; set; }

    public string SizeString
    {
      get
      {
        return Convert.ToString(this.SizeInBytes, 8).PadLeft(11, '0');
      }
    }

    public DateTime LastModification { get; set; }

    public string LastModificationString
    {
      get
      {
        return Convert.ToString((long) (this.LastModification - this.TheEpoch).TotalSeconds, 8).PadLeft(11, '0');
      }
    }

    public string HeaderChecksumString
    {
      get
      {
        return Convert.ToString(this.headerChecksum, 8).PadLeft(6, '0');
      }
    }

    public virtual int HeaderSize
    {
      get
      {
        return 512;
      }
    }

    public TarHeader()
    {
      this.Mode = 511;
      this.UserId = 61;
      this.GroupId = 61;
    }

    public byte[] GetBytes()
    {
      return this.buffer;
    }

    private string TrimNulls(string input)
    {
      return input.Trim().Replace("\0", "");
    }

    public virtual bool UpdateHeaderFromBytes()
    {
      this.FileName = Encoding.ASCII.GetString(this.buffer, 0, 100);
      this.Mode = Convert.ToInt32(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 100, 7)), 8);
      this.UserId = Convert.ToInt32(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 108, 7)), 8);
      this.GroupId = Convert.ToInt32(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 116, 7)), 8);
      this.EntryType = (EntryType) this.buffer[156];
      this.SizeInBytes = ((int) this.buffer[124] & 128) != 128 ? Convert.ToInt64(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 124, 11)), 8) : IPAddress.NetworkToHostOrder(BitConverter.ToInt64(this.buffer, 128));
      this.LastModification = this.TheEpoch.AddSeconds((double) Convert.ToInt64(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 136, 11)), 8));
      int num = Convert.ToInt32(this.TrimNulls(Encoding.ASCII.GetString(this.buffer, 148, 6)));
      this.RecalculateChecksum(this.buffer);
      if ((long) num == this.headerChecksum)
        return true;
      this.RecalculateAltChecksum(this.buffer);
      return (long) num == this.headerChecksum;
    }

    private void RecalculateAltChecksum(byte[] buf)
    {
      TarHeader.spaces.CopyTo((Array) buf, 148);
      this.headerChecksum = 0L;
      foreach (byte num in buf)
      {
        if (((int) num & 128) == 128)
          this.headerChecksum -= (long) ((int) num ^ 128);
        else
          this.headerChecksum += (long) num;
      }
    }

    public virtual byte[] GetHeaderValue()
    {
      Array.Clear((Array) this.buffer, 0, this.buffer.Length);
      if (string.IsNullOrEmpty(this.FileName))
        throw new TarException("FileName can not be empty.");
      if (this.FileName.Length >= 100)
        throw new TarException("FileName is too long. It must be less than 100 bytes.");
      Encoding.ASCII.GetBytes(this.FileName.PadRight(100, char.MinValue)).CopyTo((Array) this.buffer, 0);
      Encoding.ASCII.GetBytes(this.ModeString).CopyTo((Array) this.buffer, 100);
      Encoding.ASCII.GetBytes(this.UserIdString).CopyTo((Array) this.buffer, 108);
      Encoding.ASCII.GetBytes(this.GroupIdString).CopyTo((Array) this.buffer, 116);
      Encoding.ASCII.GetBytes(this.SizeString).CopyTo((Array) this.buffer, 124);
      Encoding.ASCII.GetBytes(this.LastModificationString).CopyTo((Array) this.buffer, 136);
      this.buffer[156] = (byte) this.EntryType;
      this.RecalculateChecksum(this.buffer);
      Encoding.ASCII.GetBytes(this.HeaderChecksumString).CopyTo((Array) this.buffer, 148);
      return this.buffer;
    }

    protected virtual void RecalculateChecksum(byte[] buf)
    {
      TarHeader.spaces.CopyTo((Array) buf, 148);
      this.headerChecksum = 0L;
      foreach (long num in buf)
        this.headerChecksum += num;
    }
  }
}
