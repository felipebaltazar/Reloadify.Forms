using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reloadify.Forms
{
	public class WeakList<T> : IList<T>
	{
		private readonly List<WeakReference> items = new List<WeakReference>();

		public T this[int index] { get => (T)items[index].Target; set => throw new NotImplementedException(); }

		public int Count => CleanseItems().Count;

		public bool IsReadOnly => false;

		public void Add(T item)
		{
			if (Contains(item))
				return;

			items.Add(new WeakReference(item));
		}

		public void AddRange(IEnumerable<T> items)
        {
			var itemsToAdd = items.Where(i => !Contains(i))
								  .Select(i=> new WeakReference(i));

			this.items.AddRange(itemsToAdd);
        }

		public void Clear()
		{
			_ = CleanseItems().ToList();
			items.Clear();
		}

		public bool Contains(T item) =>
			CleanseItems().Any(x => x.IsAlive && EqualityComparer<T>.Default.Equals(item, (T)x.Target));

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (arrayIndex < 0)
				return;

			if (arrayIndex >= Count)
				return;

			array[arrayIndex] = this[arrayIndex];
		}

		public IEnumerator<T> GetEnumerator() =>
			CleanseItems().Select(x => (T)x.Target).GetEnumerator();

		public int IndexOf(T item) =>
			CleanseItems().IndexOf(items.FirstOrDefault(x => x.IsAlive && EqualityComparer<T>.Default.Equals(item, (T)x.Target)));

        public void Insert(int index, T item) =>
			items[index] = new WeakReference(item);

        public bool Remove(T item) =>
			items.RemoveAll(x => EqualityComparer<T>.Default.Equals(item, (T)x.Target)) > 0;

        public void RemoveAt(int index) =>
			items.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		List<WeakReference> CleanseItems()
		{
			items.RemoveAll(x => !x.IsAlive);
			return items;
		}

		public void ForEach(Action<T> action)
		{
			var items = CleanseItems().ToList();
			foreach (var item in items)
			{
				if (item.IsAlive)
					action?.Invoke((T)item.Target);
			}
		}
	}
}
