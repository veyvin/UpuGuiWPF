// Decompiled with JetBrains decompiler
// Type: Mono.Options.OptionContext
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

namespace Mono.Options
{
  public class OptionContext
  {
    private Option option;
    private string name;
    private int index;
    private OptionSet set;
    private OptionValueCollection c;

    public Option Option
    {
      get
      {
        return this.option;
      }
      set
      {
        this.option = value;
      }
    }

    public string OptionName
    {
      get
      {
        return this.name;
      }
      set
      {
        this.name = value;
      }
    }

    public int OptionIndex
    {
      get
      {
        return this.index;
      }
      set
      {
        this.index = value;
      }
    }

    public OptionSet OptionSet
    {
      get
      {
        return this.set;
      }
    }

    public OptionValueCollection OptionValues
    {
      get
      {
        return this.c;
      }
    }

    public OptionContext(OptionSet set)
    {
      this.set = set;
      this.c = new OptionValueCollection(this);
    }
  }
}
