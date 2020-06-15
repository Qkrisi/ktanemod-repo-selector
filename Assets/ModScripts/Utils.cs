using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Questioner
{
    public class LoopingList<T> : IEnumerable<T>
    {
        private readonly List<T> items = new List<T>();

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        public LoopingList(List<T> b)
        {
            foreach (var i in b) Add(i);
        }

        public LoopingList(T[] b) : this(b.ToList()) { }

        public LoopingList() { }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void Add(T i)
        {
            items.Add(i);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(getIndex(index));
        }

        public T[] ToArray()
        {
            return items.ToArray();
        }

        public void AddRange(IEnumerable<T> _items)
        {
            items.AddRange(_items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        private int getIndex(int index)
        {
            if (items.Count == 0) throw new IndexOutOfRangeException("List count is 0");
            if (index >= 0) return index % items.Count;
            return items.Count - ((-index) % items.Count);
        }

        public T this[int index]
        {
            get
            {
                return items[getIndex(index)];
            }
            set
            {
                items[getIndex(index)] = value;
            }
        }
    }
}