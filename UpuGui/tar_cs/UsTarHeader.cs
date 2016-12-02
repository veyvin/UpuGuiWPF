// Decompiled with JetBrains decompiler
// Type: tar_cs.UsTarHeader
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Net;
using System.Text;

namespace tar_cs
{
  internal class UsTarHeader : TarHeader
  {
    private string namePrefix = string.Empty;
    private const string magic = "ustar";
    private const string version = "  ";
    private string groupName;
    private string userName;

    public override string UserName
    {
      get
      {
        return this.userName.Replace("\0", string.Empty);
      }
      set
      {
        if (value.Length > 32)
          throw new TarException("user name can not be longer than 32 chars");
        this.userName = value;
      }
    }

    public override string GroupName
    {
      get
      {
        return this.groupName.Replace("\0", string.Empty);
      }
      set
      {
        if (value.Length > 32)
          throw new TarException("group name can not be longer than 32 chars");
        this.groupName = value;
      }
    }

    public override string FileName
    {
      get
      {
        return this.namePrefix.Replace("\0", string.Empty) + base.FileName.Replace("\0", string.Empty);
      }
      set
      {
        if (value.Length > 100)
        {
          if (value.Length > (int) byte.MaxValue)
            throw new TarException("UsTar fileName can not be longer thatn 255 chars");
          int index = value.Length - 100;
          while (!UsTarHeader.IsPathSeparator(value[index]))
          {
            ++index;
            if (index == value.Length)
              break;
          }
          if (index == value.Length)
            index = value.Length - 100;
          this.namePrefix = value.Substring(0, index);
          base.FileName = value.Substring(index, value.Length - index);
        }
        else
          base.FileName = value;
      }
    }

    public override bool UpdateHeaderFromBytes()
    {
      byte[] bytes = this.GetBytes();
      this.UserName = Encoding.ASCII.GetString(bytes, 265, 32);
      this.GroupName = Encoding.ASCII.GetString(bytes, 297, 32);
      this.namePrefix = Encoding.ASCII.GetString(bytes, 347, 157);
      return base.UpdateHeaderFromBytes();
    }

    internal static bool IsPathSeparator(char ch)
    {
      if ((int) ch != 92 && (int) ch != 47)
        return (int) ch == 124;
      return true;
    }

    public override byte[] GetHeaderValue()
    {
      byte[] headerValue = base.GetHeaderValue();
      Encoding.ASCII.GetBytes("ustar").CopyTo((Array) headerValue, 257);
      Encoding.ASCII.GetBytes("  ").CopyTo((Array) headerValue, 262);
      Encoding.ASCII.GetBytes(this.UserName).CopyTo((Array) headerValue, 265);
      Encoding.ASCII.GetBytes(this.GroupName).CopyTo((Array) headerValue, 297);
      Encoding.ASCII.GetBytes(this.namePrefix).CopyTo((Array) headerValue, 347);
      if (this.SizeInBytes >= 8589934591L)
        UsTarHeader.SetMarker(UsTarHeader.AlignTo12(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.SizeInBytes)))).CopyTo((Array) headerValue, 124);
      this.RecalculateChecksum(headerValue);
      Encoding.ASCII.GetBytes(this.HeaderChecksumString).CopyTo((Array) headerValue, 148);
      return headerValue;
    }

    private static byte[] SetMarker(byte[] bytes)
    {
      bytes[0] |= (byte) 128;
      return bytes;
    }

    private static byte[] AlignTo12(byte[] bytes)
    {
      byte[] numArray = new byte[12];
      bytes.CopyTo((Array) numArray, 12 - bytes.Length);
      return numArray;
    }
  }
}
