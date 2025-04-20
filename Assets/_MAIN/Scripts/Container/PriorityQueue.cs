using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using static UnityEngine.Rendering.DebugUI;

namespace HamCraft.Container
{
	public class PriorityQueue<T, U>
	{
		protected List<(T item, U priority)> list = new List<(T, U priority)>();	// 오름차순

		public int Count => list.Count;

		public void Enqueue(T item, U priority)
		{
			list.Add((item, priority));
			list = list.OrderBy(x => 
			{
				object priorityValue;

				if (x.priority is Delegate del)
				{
					var returnValue = del.DynamicInvoke();
					if (returnValue is IComparable comparable)
						priorityValue = comparable;
					else
						throw new InvalidOperationException("Priority Func must return IComparable type");
				}
				else if (x.priority is IComparable directComparable)
				{
					priorityValue = directComparable;
				}
				else
				{
					throw new InvalidOperationException("Priority must be either IComparable or Func returning IComparable");
				}

				return priorityValue;
			}).ToList();
		}

		public T Dequeue()
		{
			if (Count == 0) throw new System.InvalidOperationException();
			T item = list[Count - 1].item;
			list.RemoveAt(Count - 1);
			return item;
		}

		public void Clear()
		{
			list.Clear();
		}
	}
}
