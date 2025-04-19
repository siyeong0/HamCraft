using System;
using System.Collections.Generic;

namespace HamCraft.Container
{
	public class ModifiableQueue<T>
	{
		private List<T> list = new List<T>();

		public int Count => list.Count;

		public void Enqueue(T item)
		{
			list.Insert(0, item);
		}

		public T Dequeue()
		{
			if (Count == 0) throw new System.InvalidOperationException();
			T item = list[list.Count -1];
			list.RemoveAt(list.Count - 1);
			return item;
		}

		public T Peek()
		{
			if (Count == 0) throw new System.InvalidOperationException();
			return list[list.Count - 1];
		}

		public void Insert(int index, T item)
		{
			list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		public T Find(Predicate<T> match)
		{
			return list.Find(match);
		}

		public int FindIndex(Predicate<T> match)
		{
			return list.FindIndex(match);
		}

		public bool Exist(Predicate<T> match)
		{
			return list.Exists(match);
		}

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count) throw new System.ArgumentOutOfRangeException();
				return list[index];
			}
			set
			{
				if (index < 0 || index >= Count) throw new System.ArgumentOutOfRangeException();
				list[index] = value;
			}
		}
	}
}
