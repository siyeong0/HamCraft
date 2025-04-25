using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace HamCraft
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Parcel
	{
		public float2 Position;
		public float2 Velocity;
	}
}
