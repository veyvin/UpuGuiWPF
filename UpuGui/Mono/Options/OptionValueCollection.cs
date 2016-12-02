// Decompiled with JetBrains decompiler
// Type: Mono.Options.OptionValueCollection
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Options
{
  public class OptionValueCollection : IList, ICollection, IList<string>, ICollection<string>, IEnumerable<string>, IEnumerable
  {
    private List<string> values = new List<string>();
    private OptionContext c;

    bool ICollection.IsSynchronized
    {
      get
      {
        return ((ICollection) this.values).IsSynchronized;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return ((ICollection) this.values).SyncRoot;
      }
    }

    public int Count
    {
      get
      {
        return this.values.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return false;
      }
    }

    bool IList.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    object IList.this[int index]
    {
      get
      {
        return (object) this[index];
      }
      set
      {
        ((IList) this.values)[index] = value;
      }
    }

    public string this[int index]
    {
      get
      {
        this.AssertValid(index);
        if (index < this.values.Count)
          return this.values[index];
        return (string) null;
      }
      set
      {
        this.values[index] = value;
      }
    }

    internal OptionValueCollection(OptionContext c)
    {
      this.c = c;
    }

    void ICollection.CopyTo(Array array, int index)
    {
      ((ICollection) this.values).CopyTo(array, index);
    }

    public void Add(string item)
    {
      this.values.Add(item);
    }

    public void Clear()
    {
      this.values.Clear();
    }

    public bool Contains(string item)
    {
      return this.values.Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
      this.values.CopyTo(array, arrayIndex);
    }

    public bool Remove(string item)
    {
      return this.values.Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.values.GetEnumerator();
    }

    public IEnumerator<string> GetEnumerator()
    {
      return (IEnumerator<string>) this.values.GetEnumerator();
    }

    int IList.Add(object value)
    {
      return ((IList) this.values).Add(value);
    }

    bool IList.Contains(object value)
    {
      return ((IList) this.values).Contains(value);
    }

    int IList.IndexOf(object value)
    {
      return ((IList) this.values).IndexOf(value);
    }

    void IList.Insert(int index, object value)
    {
      ((IList) this.values).Insert(index, value);
    }

    void IList.Remove(object value)
    {
      ((IList) this.values).Remove(value);
    }

    void IList.RemoveAt(int index)
    {
      this.values.RemoveAt(index);
    }

    public int IndexOf(string item)
    {
      return this.values.IndexOf(item);
    }

    public void Insert(int index, string item)
    {
      this.values.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      this.values.RemoveAt(index);
    }

    private void AssertValid(int index)
    {
      if (this.c.Option == null)
        throw new InvalidOperationException("OptionContext.Option is null.");
      if (index >= this.c.Option.MaxValueCount)
        throw new ArgumentOutOfRangeException("index");
      if (this.c.Option.OptionValueType == OptionValueType.Required && index >= this.values.Count)
        throw new OptionException(string.Format(this.c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), (object) this.c.OptionName), this.c.OptionName);
    }

    public List<string> ToList()
    {
      return new List<string>((IEnumerable<string>) this.values);
    }

    public string[] ToArray()
    {
      return this.values.ToArray();
    }

    public override string ToString()
    {
      return string.Join(", ", this.values.ToArray());
    }
  }
}
