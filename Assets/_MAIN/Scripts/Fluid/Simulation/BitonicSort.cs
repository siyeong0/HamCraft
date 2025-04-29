using System;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace HamCraft
{
	public class BitonicSort
	{
		public static void Sort<T>(T[] arr) where T : IComparable<T>
		{
			int numElements = arr.Length;
			// array length must be power of 2.
			bool isPowerOfTwo(int n) => (n != 0) && ((n & (n - 1)) == 0);
			Assert.IsTrue(isPowerOfTwo(numElements));

			for (int k = 2; k <= numElements; k *= 2)
			{
				for (int j = k / 2; j > 0; j /= 2)
				{
					Parallel.For(0, numElements, i => 
					{
						int l = i ^ j;
						if (l > i)
						{
							if (((i & k) == 0) && (arr[i].CompareTo(arr[l]) > 0) ||
								((i & k) != 0) && (arr[i].CompareTo(arr[l]) < 0))
							{
								T t = arr[i];
								arr[i] = arr[l];
								arr[l] = t;
							}
						}
					});
				}
			}
		}
	}
}
