using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;

namespace IAmLostInASea.Content.Generation
{
    public class Trench : ModSystem
    {
        public static int PlaceTrenchX;
        public static int PlaceTrenchY;
        public static int TrenchDepthLimit;
        public static int MaxWidth;

        private void TrenchGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Into the abyss you go";

            MaxWidth = Main.maxTilesX >= 8400 ? 60 : Main.maxTilesX >= 6400 ? 55 : 50;

            //Get the X spawn point of the trench
            if (GenVars.dungeonSide == -1)
            {
                PlaceTrenchX = (int)(Main.maxTilesX - 75 - (MaxWidth * 1.25f));
            }
            else
            {
                PlaceTrenchX = (int)(75 + (MaxWidth * 1.25f));
            }

            //Get the Y spawn point of the trench
            PlaceTrenchY = FindSurface(PlaceTrenchX) - 25;

            //How deep it should be
            int depthLimit = Main.maxTilesY - 400;
            TrenchDepthLimit = Math.Abs(PlaceTrenchY - depthLimit);

            //Place a solid base of hardened sand
            TrenchBase(PlaceTrenchX, PlaceTrenchY, MaxWidth + 20, TrenchDepthLimit + 30);
            progress.Set(0.25);

            //Noise
            TrenchHole(PlaceTrenchX, PlaceTrenchY, MaxWidth, TrenchDepthLimit);
            progress.Set(0.5);

            //Smooth the noise
            TrenchSmoothing(PlaceTrenchX, PlaceTrenchY, MaxWidth + 5, TrenchDepthLimit + 10, 4);
            progress.Set(0.75);

            //Fill with water
            TrenchFilling(PlaceTrenchX, PlaceTrenchY, MaxWidth + 5, TrenchDepthLimit + 10);

            WorldGen.PlaceTile(PlaceTrenchX, PlaceTrenchY, TileID.EmeraldGemspark, forced: true);
        }

        public static void TrenchBase(int X, int Y, int width, int depth)
        {
            int limit;

            for (int i = -width; i <= width; i++)
            {
                limit = FindSurface(X + i, Y - 25) + 15;

                for (int j = 0; j <= depth; j++)
                {
                    if (IsInEllipse(X, Y, width, depth, X + i, Y + j))
                    {
                        if (Y + j < limit)
                        {
                            continue;
                        }

                        WorldGen.PlaceTile(X + i, Y + j, TileID.HardenedSand, forced: true);
                        WorldGen.SlopeTile(X + i, Y + j);

                        WorldGen.KillWall(X + i, Y + j); //I hage wall
                    }
                }
            }
        }

        public static void TrenchHole(int X, int Y, int width, int depth)
        {
            for (int i = -width; i <= width; i++)
            {
                for (int j = 0; j <= depth; j++)
                {
                    if (IsInEllipse(X, Y, width, depth, X + i, Y + j))
                    {
                        if (Main.tile[X + i, Y + j].TileType == TileID.Sand)
                        {
                            WorldGen.KillTile(X + i, Y + j, noItem: true);
                            continue;
                        }

                        if (WorldGen.genRand.NextFloat() < 0.525f)
                        {
                            WorldGen.KillTile(X + i, Y + j, noItem: true);
                        }
                    }
                }
            }
        }

        public static void TrenchSmoothing(int X, int Y, int width, int depth, int loop = 1)
        {
            for (int l = 0; l < loop; l++)
            {
                for (int i = -width; i <= width; i++)
                {
                    for (int j = 0; j <= depth; j++)
                    {
                        if (IsInEllipse(X, Y, width, depth, X + i, Y + j))
                        {
                            int tileCount = WorldgenTools.CheckTiles(X + i, Y + j);

                            if (tileCount < 4)
                            {
                                WorldGen.KillTile(X + i, Y + j, noItem: true);
                            }
                            else if (tileCount > 4)
                            {
                                WorldGen.PlaceTile(X + i, Y + j, TileID.HardenedSand);
                            }
                        }
                    }
                }
            }
        }

        public static void TrenchFilling(int X, int Y, int width, int depth)
        {
            for (int i = X - width; i <= X + width; i++)
            {
                for (int j = Y; j <= Y + depth; j++)
                {
                    if (IsInEllipse(X, Y, width, depth, i, j))
                    {
                        Tile tile = Main.tile[i, j];

                        tile.LiquidType = LiquidID.Water;
						tile.LiquidAmount = byte.MaxValue;
                    }
                }
            }
        }

        public static int FindSurface(int X, int startY = 10)
        {
            bool FoundSurface = false;
            int attempts = 0;

            int Y = startY;

            while (!FoundSurface && attempts++ < 100000)
            {
				while (!WorldGen.SolidTile(X, Y) && Y <= Main.worldSurface)
				{
					Y++;
				}
				if (WorldGen.SolidTile(X, Y))
				{
					FoundSurface = true;
				}
			}

            return Y;
        }

        public static bool IsInEllipse(int H, int K, int A, int B, int X, int Y)
        {
            double powXH = Math.Pow(X - H, 2);
            double powYK = Math.Pow(Y - K, 2);
            double powA = Math.Pow(A, 2);
            double powB = Math.Pow(B, 2);

            return (powXH / powA) + (powYK / powB) <= 1;
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//Add the biome in the worldgen task
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 1, new PassLegacy("Trench Generation", TrenchGen));
			}
		}
    }
}