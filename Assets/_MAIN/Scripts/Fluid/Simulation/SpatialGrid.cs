using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace HamCraft
{
	public class SpatialGrid
	{
		class Entry : IComparable<Entry>
		{
			public uint Key;
			public int Index;

			public int CompareTo(Entry other)
			{
				return Key.CompareTo(other.Key);
			}
		};
		Entry[] spatialLookup;
		int[] startIndices;

		float2[] points;
		float radius;
		int maxPoints;

		(int, int)[] cellOffsets =
			{
				(-1, 1),
				(0, 1),
				(1, 1),
				(-1, 0),
				(0, 0),
				(1, 0),
				(-1, -1),
				(0, -1),
				(1, -1)
			};

		public SpatialGrid(int maxPoints)
		{
			this.maxPoints = maxPoints;

			// init spatial look up table
			int padded = getNextPow2(maxPoints); // length must be 2^n
			spatialLookup = new Entry[padded];
			for (int i = 0; i < padded; i++) spatialLookup[i] = new Entry() {Key = uint.MaxValue, Index = -1 };

			// init start indices buffer
			startIndices = new int[maxPoints];
		}

		public void UpdateSpatialLookup(float2[] points, float radius)
		{
			this.points = points;
			this.radius = radius;

			Parallel.For(0, points.Length, i =>
			{
				(int cellX, int cellY) = cvtPositionToCellCoord(points[i], radius);
				uint cellKey = getKeyFromHash(hashCellPos(cellX, cellY));
				spatialLookup[i] = new Entry() { Key = cellKey, Index = i };
				startIndices[i] = int.MaxValue;
			});

			// Array.Sort(spatialLookup);
			BitonicSort.Sort(spatialLookup);

			Parallel.For(0, points.Length, i =>
			{
				uint key = spatialLookup[i].Key;
				uint prevKey = i == 0 ? uint.MaxValue : spatialLookup[i - 1].Key;
				if (key != prevKey)
				{
					startIndices[key] = i;
				}
			});
		}

		public void ForeachPointWithinRadius(float2 samplePoint, Action<int> callback)
		{
			(int centerX, int centerY) = cvtPositionToCellCoord(samplePoint, radius);

			foreach ((int offsetX, int offsetY) in cellOffsets)
			{
				uint key = getKeyFromHash(hashCellPos(centerX + offsetX, centerY + offsetY));
				int cellStartIndex = startIndices[key];
				for (int i = cellStartIndex; i < spatialLookup.Length; i++)
				{
					if (spatialLookup[i].Key != key) break;

					int index = spatialLookup[i].Index;
					float dist = math.length(points[index] - samplePoint);

					if (dist <= radius)
					{
						callback(index);
					}
				}
			}
		}

		public List<int> GetNeighbors(float2 samplePoint)
		{
			List<int> neighbors = new List<int>();

			(int centerX, int centerY) = cvtPositionToCellCoord(samplePoint, radius);
			foreach ((int offsetX, int offsetY) in cellOffsets)
			{
				uint key = getKeyFromHash(hashCellPos(centerX + offsetX, centerY + offsetY));
				int cellStartIndex = startIndices[key];
				for (int i = cellStartIndex; i < spatialLookup.Length; i++)
				{
					if (spatialLookup[i].Key != key) break;

					int index = spatialLookup[i].Index;
					float dist = math.length(points[index] - samplePoint);

					if (dist <= radius)
					{
						neighbors.Add(index);
					}
				}
			}

			return neighbors;
		}

		(int, int) cvtPositionToCellCoord(float2 position, float radius)
		{
			float2 cellPos = position / radius;
			return ((int)cellPos.x, (int)cellPos.y);
		}

		uint hashCellPos(int cellX, int cellY)
		{
			const uint hashK1 = 15823;
			const uint hashK2 = 9737333;
			return ((uint)cellX * hashK1 + (uint)cellY * hashK2);
		}

		uint getKeyFromHash(uint hash)
		{
			return hash % (uint)maxPoints;
		}

		int getNextPow2(int n)
		{
			int power = 1;
			while (power < n)
				power *= 2;
			return power;
		}
	}
}
