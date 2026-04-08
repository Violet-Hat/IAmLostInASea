using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.ID;

namespace IAmLostInASea.Content.Generation
{
	public class WorldgenTools
	{	
		public static bool NoFloatingIslands(int X, int Y)
		{
			//Check the houses positions
			for (int i = 0; i < GenVars.numIslandHouses; i++)
			{
				if (X > (GenVars.floatingIslandHouseX[i] - 100) && X < (GenVars.floatingIslandHouseX[i] + 100))
				{
					if (Y > (GenVars.floatingIslandHouseY[i] - 50) && Y < (GenVars.floatingIslandHouseY[i] + 50))
					{
						return false;
					}
				}
			}

			if (IsFloatingIslandTile(X, Y))
			{
				return false;
			}

			return true;
		}

		public static bool IsFloatingIslandTile(int X, int Y)
		{
			return Main.tile[X, Y].TileType == TileID.Cloud ||
				Main.tile[X, Y].TileType == TileID.RainCloud ||
				Main.tile[X, Y].TileType == TileID.SnowCloud ||
				Main.tile[X, Y].TileType == TileID.Sunplate;
		}

		public static bool IsBeachTile(int X, int Y)
		{
			return Main.tile[X, Y].TileType == TileID.Sand ||
				Main.tile[X, Y].TileType == TileID.HardenedSand ||
				Main.tile[X, Y].TileType == TileID.ShellPile;
		}
		
		public static bool IsItPlaceable(Point origin, int r, StructureMap structures)
        {
			//Generate the new radius with padding, the diameter and the area
			int radius = r + 15;
			int diameter = radius * 2;
			Rectangle area = new Rectangle(origin.X - radius, origin.Y - radius, diameter, diameter);

			//Check if it is inside the world borders
			if (!WorldGen.InWorld(origin.X, origin.Y, r))
			{
				return false;
			}

			for (int i = area.Left; i < area.Right; i++)
			{
				for (int j = area.Top; j < area.Bottom; j++)
				{
					if (i < 41 || i > Main.maxTilesX - 42 || j < 41 || j > Main.maxTilesY)
					{
						return false;
					}
				}
			}
			
			//Check if it is far away from the islands
			for (int i = area.Left; i < area.Right; i++)
			{
				for (int j = area.Top; j < area.Bottom; j++)
				{
					if (NoFloatingIslands(i, j))
					{
						return false;
					}
				}
			}
			
			//Check if it can be placed here using the structure map
            if (!structures.CanPlace(area))
            {
                return false;
            }
			
			//Check for solids
			int count = 0;
			
            for (int i = area.Left; i < area.Right; i++)
			{
				for (int j = area.Top; j < area.Bottom; j++)
				{
					if(Main.tile[i, j].HasTile && Main.tileSolid[Main.tile[i, j].TileType])
					{
						count++;
					}
                }
            }
			
			if (count > 4)
            {
                return false;
            }
			
            return true;
        }

		static public int CheckTiles(int x, int y)
        {
            int count = 0;

            for (int nebX = x - 1; nebX <= x + 1; nebX++)
            {
                for (int nebY = y - 1; nebY <= y + 1; nebY++)
                {
                    if (nebX != x || nebY != y)
                    {
                        if (Main.tile[nebX, nebY].HasTile)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

		internal static readonly List<Vector2> Directions = new List<Vector2>()
		{
			new Vector2(-1f, -1f),
			new Vector2(1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, 1f),
			new Vector2(0f, -1f),
			new Vector2(-1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 0f),
		};
		
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