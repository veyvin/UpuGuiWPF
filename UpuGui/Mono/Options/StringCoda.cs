// Decompiled with JetBrains decompiler
// Type: Mono.Options.StringCoda
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Collections.Generic;

namespace Mono.Options
{
  internal static class StringCoda
  {
    public static IEnumerable<string> WrappedLines(string self, params int[] widths)
    {
      IEnumerable<int> widths1 = (IEnumerable<int>) widths;
      return StringCoda.WrappedLines(self, widths1);
    }

    public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
    {
      if (widths == null)
        throw new ArgumentNullException("widths");
      return StringCoda.CreateWrappedLinesIterator(self, widths);
    }

    private static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
    {
      if (string.IsNullOrEmpty(self))
      {
        yield return string.Empty;
      }
      else
      {
        using (IEnumerator<int> enumerator = widths.GetEnumerator())
        {
          bool? hw = new bool?();
          int width = StringCoda.GetNextWidth(enumerator, int.MaxValue, ref hw);
          int start = 0;
          do
          {
            int end = StringCoda.GetLineEnd(start, width, self);
            char c = self[end - 1];
            if (char.IsWhiteSpace(c))
              --end;
            bool needContinuation = end != self.Length && !StringCoda.IsEolChar(c);
            string continuation = "";
            if (needContinuation)
            {
              --end;
              continuation = "-";
            }
            string line = self.Substring(start, end - start) + continuation;
            yield return line;
            start = end;
            if (char.IsWhiteSpace(c))
              ++start;
            width = StringCoda.GetNextWidth(enumerator, width, ref hw);
          }
          while (start < self.Length);
        }
      }
    }

    private static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
    {
      if (eValid.HasValue && (!eValid.HasValue || !eValid.Value))
        return curWidth;
      curWidth = (eValid = new bool?(ewidths.MoveNext())).Value ? ewidths.Current : curWidth;
      if (curWidth < ".-".Length)
        throw new ArgumentOutOfRangeException("widths", string.Format("Element must be >= {0}, was {1}.", (object) ".-".Length, (object) curWidth));
      return curWidth;
    }

    private static bool IsEolChar(char c)
    {
      return !char.IsLetterOrDigit(c);
    }

    private static int GetLineEnd(int start, int length, string description)
    {
      int num1 = Math.Min(start + length, description.Length);
      int num2 = -1;
      for (int index = start; index < num1; ++index)
      {
        if ((int) description[index] == 10)
          return index + 1;
        if (StringCoda.IsEolChar(description[index]))
          num2 = index + 1;
      }
      if (num2 == -1 || num1 == description.Length)
        return num1;
      return num2;
    }
  }
}
