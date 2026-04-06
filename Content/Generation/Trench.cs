using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using ABMod.Generation;
using ReLogic.Utilities;

namespace IAmLostInASea.Content.Generation
{
    public class Trench : ModSystem
    {
        public static int PlaceTrenchX;
        public static int PlaceTrenchY;
        public static int TrenchDepthLimit;
        public static int MaxWidth;
        //public static int MinWidth;

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

            //Origin point of the trench
            Point origin = new Point(PlaceTrenchX, PlaceTrenchY);

            //How deep it should be
            int depthLimit = Main.maxTilesY - 400;
            TrenchDepthLimit = Math.Abs(PlaceTrenchY - depthLimit);

            //Place a solid base of hardened sand and regular sand for the trench
            TrenchBase(PlaceTrenchX, PlaceTrenchY, MaxWidth + 20, TrenchDepthLimit + 30);
            TrenchHole(PlaceTrenchX, PlaceTrenchY, MaxWidth, TrenchDepthLimit);

            WorldGen.PlaceTile(PlaceTrenchX, PlaceTrenchY, TileID.EmeraldGemspark, forced: true);

            /*
            Vector2D offSet = new Vector2D(0, TrenchDepthLimit);

            //Place a solid base of hardened sand and regular sand for the trench
            //Make the regular sand base smaller
            Point baseOrigin = new Point(PlaceTrenchX, PlaceTrenchY + 40);
            Vector2D baseOffSet = new Vector2D(0, TrenchDepthLimit + 50);
            int baseWidth = (int)(MaxWidth * 1.25f);

            WorldUtils.Gen(baseOrigin, new Shapes.Tail(baseWidth, baseOffSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearWall(), //Hage walls
                new Actions.SetTile(TileID.HardenedSand),
			}));

            baseOffSet = new Vector2D(0, TrenchDepthLimit + 25);
            baseWidth = (int)(MaxWidth * 1.10f);

            WorldUtils.Gen(baseOrigin, new Shapes.Tail(baseWidth, baseOffSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.SetTile(TileID.Sand),
			}));

            //Clear the tiles, set the water
            WorldUtils.Gen(origin, new Shapes.Tail(MaxWidth, offSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearTile(),
                new Actions.SetLiquid(),
			}));

            WorldGen.PlaceTile(PlaceTrenchX, PlaceTrenchY, TileID.EmeraldGemspark, forced: true);
            */
        }

        public static void TrenchBase(int X, int Y, int width, int depth)
        {
            int limit;

            for (int i = -width; i <= width; i++)
            {
                limit = FindSurface(X + i, Y - 25) + 20;

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
                        WorldGen.KillTile(X + i, Y + j, noItem: true);
                        WorldGen.PlaceLiquid(X + i, Y + j, Byte.MinValue, Byte.MaxValue);
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
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 1, new PassLegacy("Trench Generation", TrenchGen));
			}
		}
    }
}