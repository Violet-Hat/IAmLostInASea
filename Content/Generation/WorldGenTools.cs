using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.WorldBuilding;

using IAmLostInASea.Common.Tiles;

namespace IAmLostInASea.Content.Generation
{
	public class WorldGenTools
	{
		public static bool NoFloatingIslands(int x, int y)
		{
			//Check the houses positions
			for (int i = 0; i < GenVars.numIslandHouses; i++)
			{
				if (x > (GenVars.floatingIslandHouseX[i] - 100) && x < (GenVars.floatingIslandHouseX[i] + 100))
				{
					if (y > (GenVars.floatingIslandHouseY[i] - 50) && y < (GenVars.floatingIslandHouseY[i] + 50))
					{
						return false;
					}
				}
			}

			if (IsBiomeTile.IsFloatingIslandTile(x, y))
			{
				return false;
			}

			return true;
		}
		
		public static bool IsItPlaceable(Point origin, int r)
        {
			//Check if it is inside the world borders
			if (!WorldGen.InWorld(origin.X, origin.Y, r))
			{
				return false;
			}

			for (int i = origin.X - r; i <= origin.X + r; i++)
			{
				for (int j = origin.Y - r; j <= origin.Y + r; j++)
				{
					if (i < 41 || i > Main.maxTilesX - 42 || j < 41 || j > Main.maxTilesY)
					{
						return false;
					}
				}
			}
			
			//Check if it is far away from the islands
			for (int i = origin.X - r; i <= origin.X + r; i++)
			{
				for (int j = origin.Y - r; j <= origin.Y + r; j++)
				{
					if (!NoFloatingIslands(i, j))
					{
						return false;
					}
				}
			}
			
			//Check for solids
			int count = 0;
			
            for (int i = origin.X - r; i <= origin.X + r; i++)
			{
				for (int j = origin.Y - r; j <= origin.Y + r; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if(tile.HasTile && Main.tileSolid[tile.TileType])
					{
						count++;
					}
                }
            }
			
			if (count > 9)
            {
                return false;
            }
			
            return true;
        }

		static public int MooreTiles(int x, int y)
        {
            int count = 0;

            for (int nebX = x - 1; nebX <= x + 1; nebX++)
            {
                for (int nebY = y - 1; nebY <= y + 1; nebY++)
                {
                    if (nebX != x || nebY != y)
                    {
                        if (Framing.GetTileSafely(nebX, nebY).HasTile)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

		public static bool IsInEllipse(int h, int k, int a, int b, int x, int y)
        {
            double powXH = Math.Pow(x - h, 2);
            double powYK = Math.Pow(y - k, 2);
            double powA = Math.Pow(a, 2);
            double powB = Math.Pow(b, 2);

            return (powXH / powA) + (powYK / powB) <= 1;
        }

        public static int FindSurface(int x, int startY = -1)
        {
            bool FoundSurface = false;
            int attempts = 0;

            int y = (startY == -1) ? (int)(Main.maxTilesY * 0.15f) : startY;

            while (!FoundSurface && attempts++ < 100000)
            {
				while ((!WorldGen.SolidTile(x, y) || !NoFloatingIslands(x, y)) && y <= (Main.worldSurface + 100))
				{
					y++;
				}
				if (WorldGen.SolidTile(x, y) && NoFloatingIslands(x, y))
				{
					FoundSurface = true;
				}
			}

            return y;
        }

		internal static readonly List<Vector2> Directions =
        [
            new Vector2(-1f, -1f),
			new Vector2(1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, -1f),
			new Vector2(-1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
		];
		
		public static float PerlinNoise2D(float x, float y, int octaves, int seed)
		{
			float SmoothFunction(float n) => 3f * n * n - 2f * n * n * n;

			float NoiseGradient(int s, int noiseX, int noiseY, float xd, float yd)
			{
				int hash = s;
				hash ^= 1619 * noiseX;
				hash ^= 31337 * noiseY;

				hash = hash * hash * hash * 60493;
				hash = (hash >> 13) ^ hash;

				Vector2 g = Directions[hash & 7];

				return xd * g.X + yd * g.Y;
			}

			int frequency = (int)Math.Pow(2D, octaves);
			x *= frequency;
			y *= frequency;

			int flooredX = (int)x;
			int flooredY = (int)y;
			int ceilingX = flooredX + 1;
			int ceilingY = flooredY + 1;
			float interpolatedX = x - flooredX;
			float interpolatedY = y - flooredY;
			float interpolatedX2 = interpolatedX - 1;
			float interpolatedY2 = interpolatedY - 1;

			float fadeX = SmoothFunction(interpolatedX);
			float fadeY = SmoothFunction(interpolatedY);

			float smoothX = MathHelper.Lerp(NoiseGradient(seed, flooredX, flooredY, interpolatedX, interpolatedY), NoiseGradient(seed, ceilingX, flooredY, interpolatedX2, interpolatedY), fadeX);
			float smoothY = MathHelper.Lerp(NoiseGradient(seed, flooredX, ceilingY, interpolatedX, interpolatedY2), NoiseGradient(seed, ceilingX, ceilingY, interpolatedX2, interpolatedY2), fadeX);

			return MathHelper.Lerp(smoothX, smoothY, fadeY);
		}
	}
}