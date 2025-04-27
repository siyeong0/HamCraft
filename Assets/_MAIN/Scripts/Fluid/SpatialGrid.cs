using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace HamCraft
{
	public class SpatialGrid
	{
		class Entry : IComparable<Entry>
		{
			public int Index;
			public uint Key;

			public int CompareTo(Entry other)
			{
				return Key.CompareTo(other.Key);
			}
		};
		Entry[] spatialLookup;
		int[] startIndices;

		float2[] points;
		float radius;

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
			spatialLookup = new Entry[maxPoints];
			startIndices = new int[maxPoints];
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

		public void UpdateSpatialLookup(float2[] points, float radius)
		{
			this.points = points;
			this.radius = radius;

			Parallel.For(0, points.Length, i =>
			{
				(int cellX, int cellY) = cvtPositionToCellCoord(points[i], radius);
				uint cellKey = getKeyFromHash(hashCellPos(cellX, cellY));
				spatialLookup[i] = new Entry() { Index = i, Key = cellKey };
				startIndices[i] = int.MaxValue;
			});

			Array.Sort(spatialLookup);

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
			return hash % (uint)spatialLookup.Length;
		}
	}
}
