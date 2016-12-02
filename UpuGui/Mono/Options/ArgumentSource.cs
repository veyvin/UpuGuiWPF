// Decompiled with JetBrains decompiler
// Type: Mono.Options.ArgumentSource
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mono.Options
{
  public abstract class ArgumentSource
  {
    public abstract string Description { get; }

    public abstract string[] GetNames();

    public abstract bool GetArguments(string value, out IEnumerable<string> replacement);

    public static IEnumerable<string> GetArgumentsFromFile(string file)
    {
      return ArgumentSource.GetArguments((TextReader) File.OpenText(file), true);
    }

    public static IEnumerable<string> GetArguments(TextReader reader)
    {
      return ArgumentSource.GetArguments(reader, false);
    }

    private static IEnumerable<string> GetArguments(TextReader reader, bool close)
    {
      try
      {
        StringBuilder arg = new StringBuilder();
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          int t = line.Length;
          for (int i = 0; i < t; ++i)
          {
            char c = line[i];
            if ((int) c == 34 || (int) c == 39)
            {
              char ch = c;
              for (++i; i < t; ++i)
              {
                c = line[i];
                if ((int) c != (int) ch)
                  arg.Append(c);
                else
                  break;
              }
            }
            else if ((int) c == 32)
            {
              if (arg.Length > 0)
              {
                yield return arg.ToString();
                arg.Length = 0;
              }
            }
            else
              arg.Append(c);
          }
          if (arg.Length > 0)
          {
            yield return arg.ToString();
            arg.Length = 0;
          }
        }
      }
      finally
      {
        if (close)
          reader.Close();
      }
    }
  }
}
