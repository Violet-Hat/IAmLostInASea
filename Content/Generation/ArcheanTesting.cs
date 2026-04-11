using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IAmLostInASea.Content.Generation
{
    public class ArcheanTesting : ModSystem
    {
        private static int PlaceTestX;
        private static int PlaceTestY;
        private static int TestWidth;
        private static int TestHeight;
        private static int HoleMinAmount; //Maybe unused
        private static List<Vector2> Holes;

        private void ArcheanGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Testing testing!!!";

            PlaceTestX = Main.maxTilesX / 2;
            PlaceTestY = (int)(Main.maxTilesY * 0.55f);

			TestHeight = 140;
            TestWidth = (int)(TestHeight * 0.75f);

            Point origin = new Point(PlaceTestX, PlaceTestY);

            //Place the base of the biome
            WorldUtils.Gen(origin, new Shapes.Circle(TestWidth, TestHeight), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
				new Actions.SetTile(TileID.HardenedSand),
			}));

            /*
            //Place a flatish area that will not be destroyed
            WorldUtils.Gen(origin, new Shapes.Circle((int)(TestWidth * 0.5f), (int)(TestHeight * 0.1f)), Actions.Chain(new GenAction[]
			{
				new Actions.SetTile(TileID.Sandstone),
			}));
            */

            //List initiation
            Holes = new List<Vector2>();

            //Large empty spaces
			GenerateHoles(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 55, 15, 30, 40, 0.55f);

            //Medium empty spaces
            GenerateHoles(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 40, 15, 20, 25, 0.55f);

            //Small empty spaces
            GenerateHoles(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 25, 15, 10, 15, 0.55f);

            //Smoothing
            BiomeSmoothing(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 2);

            //Add some noise at the border of the biome
            NoiseGeneration(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 0.65f, 0.525f);

            //Smooth again
            BiomeSmoothing(PlaceTestX, PlaceTestY, TestWidth, TestHeight, 4);
        }

        public static void GenerateHoles(int centerX, int centerY, int width, int height, int caveDistance, int borderPadding, int minSizeY, int maxSizeY, float sizeMultiplier)
        {
            //Calculate a starting point in the X and Y axis
			int StartX = centerX - width * 2;
			int EndX = centerX + width * 2;
			
			int StartY = centerY - height * 2;
			int EndY = centerY + height * 2;

            //Values that will be used in the cycle
            int sizeX;
            int sizeY;

            int borderDistanceX;
            int borderDistanceY;

            for (int i = StartX; i <= EndX; i++)
            {
                for (int j = StartY; j <= EndY; j++)
                {
                    //Size and border padding
                    sizeY = WorldGen.genRand.Next(minSizeY, maxSizeY + 1);
                    sizeX = (int)(sizeY * sizeMultiplier);

                    borderDistanceX = sizeX + borderPadding;
                    borderDistanceY = sizeY + borderPadding;

                    //If it's not on a biome tile, skip
                    Tile tile = Framing.GetTileSafely(i, j);

					if(tile.TileType != TileID.HardenedSand)
					{
						continue;
					}

                    //If it's too close to the border, skip
                    bool DontPlace = false;
					for(int x = i - borderDistanceX; x <= i + borderDistanceX; x++)
					{
						if(Framing.GetTileSafely(x, j).TileType != TileID.HardenedSand)
						{
							DontPlace = true;
							break;
						}
					}
					
					for(int y = j - borderDistanceY; y <= j + borderDistanceY; y++)
					{
						if(Framing.GetTileSafely(i, y).TileType != TileID.HardenedSand)
						{
							DontPlace = true;
							break;
						}
					}
					
					if(DontPlace)
					{
						continue;
					}

                    //If there aren't many tiles, skip
                    int count = 0;
                    for(int x = i - sizeX; x <= i + sizeX; x++)
                    {
                        for(int y = j - sizeY; y <= j + sizeY; y++)
                        {
                            if (Main.tile[x, y].HasTile)
                            {
                                count++;
                            }
                        }
                    }

                    int area = sizeX * sizeY * 4;
                    int minArea = (int)(area * 0.65f);

                    if (count < minArea)
                    {
                        continue;
                    }

                    //If it's too close to other holes, skip
                    Vector2 PositionToCheck = new Vector2(i, j);
					bool tooClose = false;
					
					foreach(var ExistingPosition in Holes)
					{
						if(Vector2.DistanceSquared(PositionToCheck, ExistingPosition) < caveDistance * caveDistance)
						{
							tooClose = true;
							break;
						}
					}
					
					if(tooClose)
					{
						continue;
					}

                    //Place it with a 50/50 chance
                    if (WorldGen.genRand.NextBool())
                    {
                        int OffSetX = WorldGen.genRand.Next(-5, 6);
						int OffSetY = WorldGen.genRand.Next(-5, 6);
						Point holeOrigin = new(i + OffSetX, j + OffSetY);

                        WorldUtils.Gen(holeOrigin, new Shapes.Circle(sizeX, sizeY), Actions.Chain(new GenAction[]
						{
                            new Modifiers.Dither(0.425),
							new Actions.ClearTile(),
						}));

                        Holes.Add(PositionToCheck);
                    }
                }
            }
        }

        public static void NoiseGeneration(int X, int Y, int width, int height, float innerMult, float chance)
        {
            //Get a list of valid tiles with at least 24
            List <Point> validTiles = new List<Point>();

            int innerWidth = (int)(width * innerMult);
            int innerHeight = (int)(height * innerMult);

            for (int i = X - width; i <= X + width; i++)
            {
                for (int j = Y - height; j <= Y + height; j++)
                {
                    if (WorldgenTools.IsInEllipse(X, Y, width, height, i, j) && !WorldgenTools.IsInEllipse(X, Y, innerWidth, innerHeight, i, j))
                    {
                        int count = TileCount(i, j, 2);

                        if (count == 25)
                        {
                            validTiles.Add(new Point(i, j));
                        }
                    }
                }
            }

            //Kill the tile randomly
            foreach (Point tile in validTiles)
            {
                if (WorldGen.genRand.NextFloat() < chance)
                {
                    WorldGen.KillTile(tile.X, tile.Y, noItem: true);
                }
            }
        }

        public static void BiomeSmoothing(int X, int Y, int width, int height, int loop = 1)
        {
            for (int l = 0; l < loop; l++)
            {
                for (int i = -width; i <= width; i++)
                {
                    for (int j = -height; j <= height; j++)
                    {
                        if (WorldgenTools.IsInEllipse(X, Y, width, height, X + i, Y + j))
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

        public static int TileCount(int X, int Y, int padding)
        {
            int count = 0;

            for (int i = X - padding; i <= X + padding; i++)
            {
                for (int j = Y - padding; j <= Y + padding; j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);

                    if (tile.HasTile && tile.TileType == TileID.HardenedSand)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//Add the biome in the worldgen task
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 1, new PassLegacy("Trench Generation", ArcheanGen));
			}
		}
    }
}