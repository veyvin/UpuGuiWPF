// Decompiled with JetBrains decompiler
// Type: Mono.Options.OptionException
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Mono.Options
{
  [Serializable]
  public class OptionException : Exception
  {
    private string option;

    public string OptionName
    {
      get
      {
        return this.option;
      }
    }

    public OptionException()
    {
    }

    public OptionException(string message, string optionName)
      : base(message)
    {
      this.option = optionName;
    }

    public OptionException(string message, string optionName, Exception innerException)
      : base(message, innerException)
    {
      this.option = optionName;
    }

    protected OptionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.option = info.GetString("OptionName");
    }

    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("OptionName", (object) this.option);
    }
  }
}
