using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D/Tiles/Ham Rule Tile")]
public class CustomRuleTile : RuleTile
{
	public override bool RuleMatch(int neighbor, TileBase other)
	{
		if (other is RuleOverrideTile)
			other = (other as RuleOverrideTile).m_InstanceTile;

		switch (neighbor)
		{
			case TilingRule.Neighbor.This:
				return other is CustomRuleTile;
			case TilingRule.Neighbor.NotThis:
				return !(other is CustomRuleTile);
			default:
				break;
		}
		return base.RuleMatch(neighbor, other);
	}
}