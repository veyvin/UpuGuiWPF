// Decompiled with JetBrains decompiler
// Type: Mono.Options.Option
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Mono.Options
{
  public abstract class Option
  {
    private static readonly char[] NameTerminator = new char[2]
    {
      '=',
      ':'
    };
    private string prototype;
    private string description;
    private string[] names;
    private OptionValueType type;
    private int count;
    private string[] separators;
    private bool hidden;

    public string Prototype
    {
      get
      {
        return this.prototype;
      }
    }

    public string Description
    {
      get
      {
        return this.description;
      }
    }

    public OptionValueType OptionValueType
    {
      get
      {
        return this.type;
      }
    }

    public int MaxValueCount
    {
      get
      {
        return this.count;
      }
    }

    public bool Hidden
    {
      get
      {
        return this.hidden;
      }
    }

    internal string[] Names
    {
      get
      {
        return this.names;
      }
    }

    internal string[] ValueSeparators
    {
      get
      {
        return this.separators;
      }
    }

    protected Option(string prototype, string description)
      : this(prototype, description, 1, false)
    {
    }

    protected Option(string prototype, string description, int maxValueCount)
      : this(prototype, description, maxValueCount, false)
    {
    }

    protected Option(string prototype, string description, int maxValueCount, bool hidden)
    {
      if (prototype == null)
        throw new ArgumentNullException("prototype");
      if (prototype.Length == 0)
        throw new ArgumentException("Cannot be the empty string.", "prototype");
      if (maxValueCount < 0)
        throw new ArgumentOutOfRangeException("maxValueCount");
      this.prototype = prototype;
      this.description = description;
      this.count = maxValueCount;
      string[] strArray;
      if (!(this is OptionSet.Category))
        strArray = prototype.Split('|');
      else
        strArray = new string[1]
        {
          prototype + (object) this.GetHashCode()
        };
      this.names = strArray;
      if (this is OptionSet.Category)
        return;
      this.type = this.ParsePrototype();
      this.hidden = hidden;
      if (this.count == 0 && this.type != OptionValueType.None)
        throw new ArgumentException("Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.", "maxValueCount");
      if (this.type == OptionValueType.None && maxValueCount > 1)
        throw new ArgumentException(string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", (object) maxValueCount), "maxValueCount");
      if (Array.IndexOf<string>(this.names, "<>") >= 0 && (this.names.Length == 1 && this.type != OptionValueType.None || this.names.Length > 1 && this.MaxValueCount > 1))
        throw new ArgumentException("The default option handler '<>' cannot require values.", "prototype");
    }

    public string[] GetNames()
    {
      return (string[]) this.names.Clone();
    }

    public string[] GetValueSeparators()
    {
      if (this.separators == null)
        return new string[0];
      return (string[]) this.separators.Clone();
    }

    protected static T Parse<T>(string value, OptionContext c)
    {
      Type type1 = typeof (T);
      Type type2 = type1.IsValueType && type1.IsGenericType && !type1.IsGenericTypeDefinition && type1.GetGenericTypeDefinition() == typeof (Nullable<>) ? type1.GetGenericArguments()[0] : typeof (T);
      TypeConverter converter = TypeDescriptor.GetConverter(type2);
      T obj = default (T);
      try
      {
        if (value != null)
          obj = (T) converter.ConvertFromString(value);
      }
      catch (Exception ex)
      {
        throw new OptionException(string.Format(c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."), (object) value, (object) type2.Name, (object) c.OptionName), c.OptionName, ex);
      }
      return obj;
    }

    private OptionValueType ParsePrototype()
    {
      char ch = char.MinValue;
      List<string> list = new List<string>();
      for (int index1 = 0; index1 < this.names.Length; ++index1)
      {
        string name = this.names[index1];
        if (name.Length == 0)
          throw new ArgumentException("Empty option names are not supported.", "prototype");
        int index2 = name.IndexOfAny(Option.NameTerminator);
        if (index2 != -1)
        {
          this.names[index1] = name.Substring(0, index2);
          if ((int) ch != 0 && (int) ch != (int) name[index2])
            throw new ArgumentException(string.Format("Conflicting option types: '{0}' vs. '{1}'.", (object) ch, (object) name[index2]), "prototype");
          ch = name[index2];
          Option.AddSeparators(name, index2, (ICollection<string>) list);
        }
      }
      if ((int) ch == 0)
        return OptionValueType.None;
      if (this.count <= 1 && list.Count != 0)
        throw new ArgumentException(string.Format("Cannot provide key/value separators for Options taking {0} value(s).", (object) this.count), "prototype");
      if (this.count > 1)
      {
        if (list.Count == 0)
          this.separators = new string[2]
          {
            ":",
            "="
          };
        else
          this.separators = list.Count != 1 || list[0].Length != 0 ? list.ToArray() : (string[]) null;
      }
      return (int) ch != 61 ? OptionValueType.Optional : OptionValueType.Required;
    }

    private static void AddSeparators(string name, int end, ICollection<string> seps)
    {
      int startIndex = -1;
      for (int index = end + 1; index < name.Length; ++index)
      {
        switch (name[index])
        {
          case '{':
            if (startIndex != -1)
              throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", (object) name), "prototype");
            startIndex = index + 1;
            break;
          case '}':
            if (startIndex == -1)
              throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", (object) name), "prototype");
            seps.Add(name.Substring(startIndex, index - startIndex));
            startIndex = -1;
            break;
          default:
            if (startIndex == -1)
            {
              seps.Add(name[index].ToString());
              break;
            }
            break;
        }
      }
      if (startIndex != -1)
        throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", (object) name), "prototype");
    }

    public void Invoke(OptionContext c)
    {
      this.OnParseComplete(c);
      c.OptionName = (string) null;
      c.Option = (Option) null;
      c.OptionValues.Clear();
    }

    protected abstract void OnParseComplete(OptionContext c);

    public override string ToString()
    {
      return this.Prototype;
    }
  }
}
