using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamCraft.Container
{
	public class ModifiablePriorityQueue<T, U> : PriorityQueue<T, U>
	{
		public T Peek()
		{
			if (Count == 0) throw new System.InvalidOperationException();
			return list[list.Count - 1].item;
		}

		public void Insert(int index, T item, U priority)
		{
			list.Insert(index, (item, priority));
		}

		public void RemoveAt(int index)
		{
			list.RemoveAt(index);
		}

		public T Find(Predicate<T> match)
		{
			return list.First(x => match(x.item)).item;
		}

		public int FindIndex(Predicate<T> match)
		{
			return list.FindIndex(x => match(x.item));
		}

		public bool Exist(Predicate<T> match)
		{
			return list.Any(x => match(x.item));
		}

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= Count) throw new System.ArgumentOutOfRangeException();
				return list[index].item;
			}
			set
			{
				if (index < 0 || index >= Count) throw new System.ArgumentOutOfRangeException();
				list[index] = (value, list[index].priority);
			}
		}
	}
}
