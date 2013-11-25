using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComputerAlgebra.Plotting
{
    public class SeriesEventArgs : EventArgs
    {
        private Series s;
        public Series Series { get { return s; } }

        public SeriesEventArgs(Series S) { s = S; }
    }

    /// <summary>
    /// Collection of Series.
    /// </summary>
    public class SeriesCollection : ICollection<Series>, IEnumerable<Series>
    {
        protected List<Series> x = new List<Series>();

        public Series this[int index] { get { return x[index]; } }

        public delegate void SeriesEventHandler(object sender, SeriesEventArgs e);

        private List<SeriesEventHandler> itemAdded = new List<SeriesEventHandler>();
        protected void OnItemAdded(SeriesEventArgs e) { foreach (SeriesEventHandler i in itemAdded) i(this, e); }
        public event SeriesEventHandler ItemAdded
        {
            add { itemAdded.Add(value); }
            remove { itemAdded.Remove(value); }
        }

        private List<SeriesEventHandler> itemRemoved = new List<SeriesEventHandler>();
        protected void OnItemRemoved(SeriesEventArgs e) { foreach (SeriesEventHandler i in itemRemoved) i(this, e); }
        public event SeriesEventHandler ItemRemoved
        {
            add { itemRemoved.Add(value); }
            remove { itemRemoved.Remove(value); }
        }

        // ICollection<Series>
        public int Count { get { lock(x) return x.Count; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(Series item)
        {
            lock (x) x.Add(item);
            OnItemAdded(new SeriesEventArgs(item));
        }
        public void AddRange(IEnumerable<Series> items)
        {
            lock (x) foreach (Series i in items)
                Add(i);
        }
        public void Clear()
        {
            Series[] removed;
            lock (x)
            {
                removed = x.ToArray();
                x.Clear();
            }

            foreach (Series i in removed)
                OnItemRemoved(new SeriesEventArgs(i));
        }
        public bool Contains(Series item) { lock(x) return x.Contains(item); }
        public void CopyTo(Series[] array, int arrayIndex) { lock (x) x.CopyTo(array, arrayIndex); }
        public bool Remove(Series item)
        {
            bool ret;
            lock(x) ret = x.Remove(item);
            if (ret)
                OnItemRemoved(new SeriesEventArgs(item));
            return ret;
        }
        public void RemoveRange(IEnumerable<Series> items)
        {
            foreach (Series i in items)
                Remove(i);
        }

        public void ForEach(Action<Series> f)
        {
            lock (x) foreach (Series i in x)
                f(i);
        }

        // IEnumerable<Series>
        public IEnumerator<Series> GetEnumerator() { return x.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }
}
