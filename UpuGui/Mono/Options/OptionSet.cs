// Decompiled with JetBrains decompiler
// Type: Mono.Options.OptionSet
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.Options
{
  public class OptionSet : KeyedCollection<string, Option>
  {
    private List<ArgumentSource> sources = new List<ArgumentSource>();
    private readonly Regex ValueOption = new Regex("^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");
    private const int OptionWidth = 29;
    private const int Description_FirstWidth = 51;
    private const int Description_RemWidth = 49;
    private Converter<string, string> localizer;
    private ReadOnlyCollection<ArgumentSource> roSources;

    public Converter<string, string> MessageLocalizer
    {
      get
      {
        return this.localizer;
      }
    }

    public ReadOnlyCollection<ArgumentSource> ArgumentSources
    {
      get
      {
        return this.roSources;
      }
    }

    public OptionSet()
      : this((Converter<string, string>) (f => f))
    {
    }

    public OptionSet(Converter<string, string> localizer)
    {
      this.localizer = localizer;
      this.roSources = new ReadOnlyCollection<ArgumentSource>((IList<ArgumentSource>) this.sources);
    }

    protected override string GetKeyForItem(Option item)
    {
      if (item == null)
        throw new ArgumentNullException("option");
      if (item.Names != null && item.Names.Length > 0)
        return item.Names[0];
      throw new InvalidOperationException("Option has no names!");
    }

    [Obsolete("Use KeyedCollection.this[string]")]
    protected Option GetOptionForName(string option)
    {
      if (option == null)
        throw new ArgumentNullException("option");
      try
      {
        return this[option];
      }
      catch (KeyNotFoundException ex)
      {
        return (Option) null;
      }
    }

    protected override void InsertItem(int index, Option item)
    {
      this.InsertItem(index, item);
      this.AddImpl(item);
    }

    protected override void RemoveItem(int index)
    {
      Option option = this.Items[index];
      base.RemoveItem(index);
      for (int index1 = 1; index1 < option.Names.Length; ++index1)
        this.Dictionary.Remove(option.Names[index1]);
    }

    protected override void SetItem(int index, Option item)
    {
      this.SetItem(index, item);
      this.AddImpl(item);
    }

    private void AddImpl(Option option)
    {
      if (option == null)
        throw new ArgumentNullException("option");
      List<string> list = new List<string>(option.Names.Length);
      try
      {
        for (int index = 1; index < option.Names.Length; ++index)
        {
          this.Dictionary.Add(option.Names[index], option);
          list.Add(option.Names[index]);
        }
      }
      catch (Exception ex)
      {
        foreach (string key in list)
          this.Dictionary.Remove(key);
        throw;
      }
    }

    public OptionSet Add(string header)
    {
      if (header == null)
        throw new ArgumentNullException("header");
      this.Add((Option) new OptionSet.Category(header));
      return this;
    }

    public OptionSet Add(Option option)
    {
      this.Add(option);
      return this;
    }

    public OptionSet Add(string prototype, Action<string> action)
    {
      return this.Add(prototype, (string) null, action);
    }

    public OptionSet Add(string prototype, string description, Action<string> action)
    {
      return this.Add(prototype, description, action, false);
    }

    public OptionSet Add(string prototype, string description, Action<string> action, bool hidden)
    {
      if (action == null)
        throw new ArgumentNullException("action");
      this.Add((Option) new OptionSet.ActionOption(prototype, description, 1, (Action<OptionValueCollection>) (v => action(v[0])), hidden));
      return this;
    }

    public OptionSet Add(string prototype, OptionAction<string, string> action)
    {
      return this.Add(prototype, (string) null, action);
    }

    public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
    {
      return this.Add(prototype, description, action, false);
    }

    public OptionSet Add(string prototype, string description, OptionAction<string, string> action, bool hidden)
    {
      if (action == null)
        throw new ArgumentNullException("action");
      this.Add((Option) new OptionSet.ActionOption(prototype, description, 2, (Action<OptionValueCollection>) (v => action(v[0], v[1])), hidden));
      return this;
    }

    public OptionSet Add<T>(string prototype, Action<T> action)
    {
      return this.Add<T>(prototype, (string) null, action);
    }

    public OptionSet Add<T>(string prototype, string description, Action<T> action)
    {
      return this.Add((Option) new OptionSet.ActionOption<T>(prototype, description, action));
    }

    public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
    {
      return this.Add<TKey, TValue>(prototype, (string) null, action);
    }

    public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
    {
      return this.Add((Option) new OptionSet.ActionOption<TKey, TValue>(prototype, description, action));
    }

    public OptionSet Add(ArgumentSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      this.sources.Add(source);
      return this;
    }

    protected virtual OptionContext CreateOptionContext()
    {
      return new OptionContext(this);
    }

    public List<string> Parse(IEnumerable<string> arguments)
    {
      if (arguments == null)
        throw new ArgumentNullException("arguments");
      OptionContext optionContext = this.CreateOptionContext();
      optionContext.OptionIndex = -1;
      bool flag = true;
      List<string> list = new List<string>();
      Option def = this.Contains("<>") ? this["<>"] : (Option) null;
      OptionSet.ArgumentEnumerator ae = new OptionSet.ArgumentEnumerator(arguments);
      foreach (string str in ae)
      {
        ++optionContext.OptionIndex;
        if (str == "--")
          flag = false;
        else if (!flag)
          OptionSet.Unprocessed((ICollection<string>) list, def, optionContext, str);
        else if (!this.AddSource(ae, str) && !this.Parse(str, optionContext))
          OptionSet.Unprocessed((ICollection<string>) list, def, optionContext, str);
      }
      if (optionContext.Option != null)
        optionContext.Option.Invoke(optionContext);
      return list;
    }

    private bool AddSource(OptionSet.ArgumentEnumerator ae, string argument)
    {
      foreach (ArgumentSource argumentSource in this.sources)
      {
        IEnumerable<string> replacement;
        if (argumentSource.GetArguments(argument, out replacement))
        {
          ae.Add(replacement);
          return true;
        }
      }
      return false;
    }

    private static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
    {
      if (def == null)
      {
        extra.Add(argument);
        return false;
      }
      c.OptionValues.Add(argument);
      c.Option = def;
      c.Option.Invoke(c);
      return false;
    }

    protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
    {
      if (argument == null)
        throw new ArgumentNullException("argument");
      flag = name = sep = value = (string) null;
      Match match = this.ValueOption.Match(argument);
      if (!match.Success)
        return false;
      flag = match.Groups["flag"].Value;
      name = match.Groups["name"].Value;
      if (match.Groups["sep"].Success && match.Groups["value"].Success)
      {
        sep = match.Groups["sep"].Value;
        value = match.Groups["value"].Value;
      }
      return true;
    }

    protected virtual bool Parse(string argument, OptionContext c)
    {
      if (c.Option != null)
      {
        this.ParseValue(argument, c);
        return true;
      }
      string flag;
      string name;
      string sep;
      string option1;
      if (!this.GetOptionParts(argument, out flag, out name, out sep, out option1))
        return false;
      if (this.Contains(name))
      {
        Option option2 = this[name];
        c.OptionName = flag + name;
        c.Option = option2;
        switch (option2.OptionValueType)
        {
          case OptionValueType.None:
            c.OptionValues.Add(name);
            c.Option.Invoke(c);
            break;
          case OptionValueType.Optional:
          case OptionValueType.Required:
            this.ParseValue(option1, c);
            break;
        }
        return true;
      }
      if (this.ParseBool(argument, name, c))
        return true;
      return this.ParseBundledValue(flag, string.Concat(new string[1]
      {
        name + sep + option1
      }), c);
    }

    private void ParseValue(string option, OptionContext c)
    {
      if (option != null)
      {
        string[] strArray;
        if (c.Option.ValueSeparators == null)
          strArray = new string[1]
          {
            option
          };
        else
          strArray = option.Split(c.Option.ValueSeparators, c.Option.MaxValueCount - c.OptionValues.Count, StringSplitOptions.None);
        foreach (string str in strArray)
          c.OptionValues.Add(str);
      }
      if (c.OptionValues.Count == c.Option.MaxValueCount || c.Option.OptionValueType == OptionValueType.Optional)
        c.Option.Invoke(c);
      else if (c.OptionValues.Count > c.Option.MaxValueCount)
        throw new OptionException(this.localizer(string.Format("Error: Found {0} option values when expecting {1}.", (object) c.OptionValues.Count, (object) c.Option.MaxValueCount)), c.OptionName);
    }

    private bool ParseBool(string option, string n, OptionContext c)
    {
      string index;
      if (n.Length < 1 || (int) n[n.Length - 1] != 43 && (int) n[n.Length - 1] != 45 || !this.Contains(index = n.Substring(0, n.Length - 1)))
        return false;
      Option option1 = this[index];
      string str = (int) n[n.Length - 1] == 43 ? option : (string) null;
      c.OptionName = option;
      c.Option = option1;
      c.OptionValues.Add(str);
      option1.Invoke(c);
      return true;
    }

    private bool ParseBundledValue(string f, string n, OptionContext c)
    {
      if (f != "-")
        return false;
      for (int index = 0; index < n.Length; ++index)
      {
        string name = f + n[index].ToString();
        string key = n[index].ToString();
        if (!this.Contains(key))
        {
          if (index == 0)
            return false;
          throw new OptionException(string.Format(this.localizer("Cannot use unregistered option '{0}' in bundle '{1}'."), (object) key, (object) (f + n)), (string) null);
        }
        Option option = this[key];
        switch (option.OptionValueType)
        {
          case OptionValueType.None:
            OptionSet.Invoke(c, name, n, option);
            continue;
          case OptionValueType.Optional:
          case OptionValueType.Required:
            string str = n.Substring(index + 1);
            c.Option = option;
            c.OptionName = name;
            this.ParseValue(str.Length != 0 ? str : (string) null, c);
            return true;
          default:
            throw new InvalidOperationException("Unknown OptionValueType: " + (object) option.OptionValueType);
        }
      }
      return true;
    }

    private static void Invoke(OptionContext c, string name, string value, Option option)
    {
      c.OptionName = name;
      c.Option = option;
      c.OptionValues.Add(value);
      option.Invoke(c);
    }

    public void WriteOptionDescriptions(TextWriter o)
    {
      foreach (Option p in (Collection<Option>) this)
      {
        int written = 0;
        if (!p.Hidden)
        {
          if (p is OptionSet.Category)
            this.WriteDescription(o, p.Description, "", 80, 80);
          else if (this.WriteOptionPrototype(o, p, ref written))
          {
            if (written < 29)
            {
              o.Write(new string(' ', 29 - written));
            }
            else
            {
              o.WriteLine();
              o.Write(new string(' ', 29));
            }
            this.WriteDescription(o, p.Description, new string(' ', 31), 51, 49);
          }
        }
      }
      foreach (ArgumentSource argumentSource in this.sources)
      {
        string[] names = argumentSource.GetNames();
        if (names != null && names.Length != 0)
        {
          int n = 0;
          OptionSet.Write(o, ref n, "  ");
          OptionSet.Write(o, ref n, names[0]);
          for (int index = 1; index < names.Length; ++index)
          {
            OptionSet.Write(o, ref n, ", ");
            OptionSet.Write(o, ref n, names[index]);
          }
          if (n < 29)
          {
            o.Write(new string(' ', 29 - n));
          }
          else
          {
            o.WriteLine();
            o.Write(new string(' ', 29));
          }
          this.WriteDescription(o, argumentSource.Description, new string(' ', 31), 51, 49);
        }
      }
    }

    private void WriteDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth)
    {
      bool flag = false;
      foreach (string str in OptionSet.GetLines(this.localizer(OptionSet.GetDescription(value)), firstWidth, remWidth))
      {
        if (flag)
          o.Write(prefix);
        o.WriteLine(str);
        flag = true;
      }
    }

    private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
    {
      string[] names = p.Names;
      int nextOptionIndex1 = OptionSet.GetNextOptionIndex(names, 0);
      if (nextOptionIndex1 == names.Length)
        return false;
      if (names[nextOptionIndex1].Length == 1)
      {
        OptionSet.Write(o, ref written, "  -");
        OptionSet.Write(o, ref written, names[0]);
      }
      else
      {
        OptionSet.Write(o, ref written, "      --");
        OptionSet.Write(o, ref written, names[0]);
      }
      for (int nextOptionIndex2 = OptionSet.GetNextOptionIndex(names, nextOptionIndex1 + 1); nextOptionIndex2 < names.Length; nextOptionIndex2 = OptionSet.GetNextOptionIndex(names, nextOptionIndex2 + 1))
      {
        OptionSet.Write(o, ref written, ", ");
        OptionSet.Write(o, ref written, names[nextOptionIndex2].Length == 1 ? "-" : "--");
        OptionSet.Write(o, ref written, names[nextOptionIndex2]);
      }
      if (p.OptionValueType == OptionValueType.Optional || p.OptionValueType == OptionValueType.Required)
      {
        if (p.OptionValueType == OptionValueType.Optional)
          OptionSet.Write(o, ref written, this.localizer("["));
        OptionSet.Write(o, ref written, this.localizer("=" + OptionSet.GetArgumentName(0, p.MaxValueCount, p.Description)));
        string str = p.ValueSeparators == null || p.ValueSeparators.Length <= 0 ? " " : p.ValueSeparators[0];
        for (int index = 1; index < p.MaxValueCount; ++index)
          OptionSet.Write(o, ref written, this.localizer(str + OptionSet.GetArgumentName(index, p.MaxValueCount, p.Description)));
        if (p.OptionValueType == OptionValueType.Optional)
          OptionSet.Write(o, ref written, this.localizer("]"));
      }
      return true;
    }

    private static int GetNextOptionIndex(string[] names, int i)
    {
      while (i < names.Length && names[i] == "<>")
        ++i;
      return i;
    }

    private static void Write(TextWriter o, ref int n, string s)
    {
      n += s.Length;
      o.Write(s);
    }

    private static string GetArgumentName(int index, int maxIndex, string description)
    {
      if (description == null)
      {
        if (maxIndex != 1)
          return "VALUE" + (object) (index + 1);
        return "VALUE";
      }
      string[] strArray;
      if (maxIndex == 1)
        strArray = new string[2]
        {
          "{0:",
          "{"
        };
      else
        strArray = new string[1]
        {
          "{" + (object) index + ":"
        };
      for (int index1 = 0; index1 < strArray.Length; ++index1)
      {
        int startIndex1 = 0;
        int startIndex2;
        do
        {
          startIndex2 = description.IndexOf(strArray[index1], startIndex1);
        }
        while ((startIndex2 < 0 || startIndex1 == 0 ? 0 : ((int) description[startIndex1++ - 1] == 123 ? 1 : 0)) != 0);
        if (startIndex2 != -1)
        {
          int num = description.IndexOf("}", startIndex2);
          if (num != -1)
            return description.Substring(startIndex2 + strArray[index1].Length, num - startIndex2 - strArray[index1].Length);
        }
      }
      if (maxIndex != 1)
        return "VALUE" + (object) (index + 1);
      return "VALUE";
    }

    private static string GetDescription(string description)
    {
      if (description == null)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder(description.Length);
      int startIndex = -1;
      for (int index = 0; index < description.Length; ++index)
      {
        switch (description[index])
        {
          case ':':
            if (startIndex >= 0)
            {
              startIndex = index + 1;
              break;
            }
            goto default;
          case '{':
            if (index == startIndex)
            {
              stringBuilder.Append('{');
              startIndex = -1;
              break;
            }
            if (startIndex < 0)
            {
              startIndex = index + 1;
              break;
            }
            break;
          case '}':
            if (startIndex < 0)
            {
              if (index + 1 == description.Length || (int) description[index + 1] != 125)
                throw new InvalidOperationException("Invalid option description: " + description);
              ++index;
              stringBuilder.Append("}");
              break;
            }
            stringBuilder.Append(description.Substring(startIndex, index - startIndex));
            startIndex = -1;
            break;
          default:
            if (startIndex < 0)
            {
              stringBuilder.Append(description[index]);
              break;
            }
            break;
        }
      }
      return stringBuilder.ToString();
    }

    private static IEnumerable<string> GetLines(string description, int firstWidth, int remWidth)
    {
      return StringCoda.WrappedLines(description, firstWidth, remWidth);
    }

    internal sealed class Category : Option
    {
      public Category(string description)
        : base("=:Category:= " + description, description)
      {
      }

      protected override void OnParseComplete(OptionContext c)
      {
        throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
      }
    }

    private sealed class ActionOption : Option
    {
      private Action<OptionValueCollection> action;

      public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
        : this(prototype, description, count, action, false)
      {
      }

      public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action, bool hidden)
        : base(prototype, description, count, hidden)
      {
        if (action == null)
          throw new ArgumentNullException("action");
        this.action = action;
      }

      protected override void OnParseComplete(OptionContext c)
      {
        this.action(c.OptionValues);
      }
    }

    private sealed class ActionOption<T> : Option
    {
      private Action<T> action;

      public ActionOption(string prototype, string description, Action<T> action)
        : base(prototype, description, 1)
      {
        if (action == null)
          throw new ArgumentNullException("action");
        this.action = action;
      }

      protected override void OnParseComplete(OptionContext c)
      {
        this.action(Option.Parse<T>(c.OptionValues[0], c));
      }
    }

    private sealed class ActionOption<TKey, TValue> : Option
    {
      private OptionAction<TKey, TValue> action;

      public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
        : base(prototype, description, 2)
      {
        if (action == null)
          throw new ArgumentNullException("action");
        this.action = action;
      }

      protected override void OnParseComplete(OptionContext c)
      {
        this.action(Option.Parse<TKey>(c.OptionValues[0], c), Option.Parse<TValue>(c.OptionValues[1], c));
      }
    }

    private class ArgumentEnumerator : IEnumerable<string>, IEnumerable
    {
      private List<IEnumerator<string>> sources = new List<IEnumerator<string>>();

      public ArgumentEnumerator(IEnumerable<string> arguments)
      {
        this.sources.Add(arguments.GetEnumerator());
      }

      public void Add(IEnumerable<string> arguments)
      {
        this.sources.Add(arguments.GetEnumerator());
      }

      public IEnumerator<string> GetEnumerator()
      {
        do
        {
          IEnumerator<string> c = this.sources[this.sources.Count - 1];
          if (c.MoveNext())
          {
            yield return c.Current;
          }
          else
          {
            c.Dispose();
            this.sources.RemoveAt(this.sources.Count - 1);
          }
        }
        while (this.sources.Count > 0);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return (IEnumerator) this.GetEnumerator();
      }
    }
  }
}
