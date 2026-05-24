using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace IAmLostInASea.Common.Tiles
{
    public class IsBiomeTile
    {
        public static bool IsFloatingIslandTile(int x, int y)
		{
			return Framing.GetTileSafely(x, y).TileType == TileID.Cloud ||
				Framing.GetTileSafely(x, y).TileType == TileID.RainCloud ||
				Framing.GetTileSafely(x, y).TileType == TileID.SnowCloud ||
				Framing.GetTileSafely(x, y).TileType == TileID.Sunplate;
		}

		public static bool IsBeachTile(int x, int y)
		{
			return Framing.GetTileSafely(x, y).TileType == TileID.Sand ||
				Framing.GetTileSafely(x, y).TileType == TileID.HardenedSand ||
				Framing.GetTileSafely(x, y).TileType == TileID.ShellPile;
		}
    }
}