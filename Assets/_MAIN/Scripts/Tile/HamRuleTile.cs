using UnityEngine;
using UnityEngine.Tilemaps;

namespace HamCraft
{
	[CreateAssetMenu(menuName = "2D/Tiles/Ham Rule Tile")]
	public class HamRuleTile : RuleTile
	{
		public override bool RuleMatch(int neighbor, TileBase other)
		{
			if (other is RuleOverrideTile)
				other = (other as RuleOverrideTile).m_InstanceTile;

			switch (neighbor)
			{
				case TilingRule.Neighbor.This:
					return other is HamRuleTile;
				case TilingRule.Neighbor.NotThis:
					return !(other is HamRuleTile);
				default:
					break;
			}
			return base.RuleMatch(neighbor, other);
		}
	}
}