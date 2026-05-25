using System;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;

using IAmLostInASea.Enums;

namespace IAmLostInASea.Content.Generation
{
    public class Trench : ModSystem
    {
        //Generation values
        private static int PlaceTrenchX;
        private static int PlaceTrenchY;
        private static int TrenchDepthLimit;

        private static readonly int MaxWidth = Main.maxTilesX >= 8400 ? 85 : Main.maxTilesX >= 6400 ? 80 : 75;
        private static int HighestPoint;

        //Main method
        public static void TrenchGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Into the abyss you go";

            //Get the X spawn point of the trench
            if (GenVars.dungeonSide == -1)
            {
                PlaceTrenchX = 50 + MaxWidth;
            }
            else
            {
                PlaceTrenchX = Main.maxTilesX - 50 - MaxWidth;
            }

            //Get the Y spawn point of the trench
            PlaceTrenchY = WorldGenTools.FindSurface(PlaceTrenchX) - 25;

            //Get the highest point
            int leftY = WorldGenTools.FindSurface(PlaceTrenchX - MaxWidth);
            int rightY = WorldGenTools.FindSurface(PlaceTrenchX + MaxWidth);

            HighestPoint = Math.Min(leftY, rightY) - 25;

            //How deep it should be
            int depthLimit = Main.maxTilesY - 370;
            TrenchDepthLimit = Math.Abs(PlaceTrenchY - depthLimit);

            //Place a solid base of hardened sand
            TrenchBase(PlaceTrenchX, PlaceTrenchY, MaxWidth, TrenchDepthLimit);
            progress.Set(0.25);

            //Noise
            TrenchHole(PlaceTrenchX, PlaceTrenchY, MaxWidth - 15, TrenchDepthLimit - 20);
            progress.Set(0.5);

            //Smooth the noise
            TrenchSmoothing(PlaceTrenchX, PlaceTrenchY, MaxWidth, TrenchDepthLimit, 5);
            progress.Set(0.75);

            //Fill with water
            TrenchFilling(PlaceTrenchX, PlaceTrenchY, MaxWidth, TrenchDepthLimit);
        }

        //Helper method : Generate the base of the trench
        private static void TrenchBase(int x, int y, int width, int depth)
        {
            int limit;

            for (int i = x - width; i <= x + width; i++)
            {
                //Limit to avoid tiles being placed above the ocean
                limit = WorldGenTools.FindSurface(i, HighestPoint) + 15;

                for (int j = y; j <= y + depth; j++)
                {
                    if (WorldGenTools.IsInEllipse(x, y, width, depth, i, j))
                    {
                        Tile tile = Framing.GetTileSafely(i, j);

                        //Replace or place tile
                        if (tile.HasTile)
                        {
                            tile.TileType = TileID.HardenedSand;
                        }
                        else if (j > limit)
                        {
                            WorldGen.PlaceTile(i, j, TileID.HardenedSand);
                        }
                        
                        //Slope and kill wall
                        WorldGen.SlopeTile(i, j);
                        WorldGen.KillWall(i, j);
                    }
                }
            }
        }

        //Helper method : Generate caves in the trench
        private static void TrenchHole(int x, int y, int width, int depth)
        {
            //Randomize a style for the caves
            int style = 2;

            int seed = WorldGen.genRand.Next();
            int octaves = 5;

            float smooth = 80f;
            float clearChance = 0.65f;

            float caveDivX = GetSmootherX(style);
            float caveDivY = GetSmootherY(style);

            for (int i = x - width; i <= x + width; i++)
            {
                for (int j = y; j <= y + depth; j++)
                {
                    if (WorldGenTools.IsInEllipse(x, y, width, depth, i, j))
                    {
                        //Perlin noise values
                        float horizontalOffsetNoise = WorldGenTools.PerlinNoise2D(i / smooth, j / smooth, octaves, unchecked(seed + 1)) * 0.01f;
                        float cavePerlinValue = WorldGenTools.PerlinNoise2D(i / caveDivX, j / caveDivY, octaves, seed) + 0.5f + horizontalOffsetNoise;
                        float cavePerlinValue2 = WorldGenTools.PerlinNoise2D(i / caveDivX, j / caveDivY, octaves, unchecked(seed - 1)) + 0.5f;
                        float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                        float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.2f;

                        //Remove tiles based on the noise and a float value
                        bool noiseCheck = caveNoiseMap * caveNoiseMap > caveCreationThreshold;
                        bool floatCheck = WorldGen.genRand.NextFloat() < clearChance;

                        if (noiseCheck && floatCheck)
                        {
                            WorldGen.KillTile(i, j, noItem: true);
                        }
                    }
                }
            }
        }

        //Helper method : Smooth the area
        private static void TrenchSmoothing(int x, int y, int width, int depth, int loop = 1)
        {
            for (int l = 0; l < loop; l++)
            {
                for (int i = x - width; i <= x + width; i++)
                {
                    for (int j = y; j <= y + depth; j++)
                    {
                        int tileCount = WorldGenTools.MooreTiles(i, j);

                        if (tileCount < 4)
                        {
                            WorldGen.KillTile(i, j, noItem: true);
                        }
                        else if (tileCount > 4)
                        {
                            WorldGen.PlaceTile(i, j, TileID.HardenedSand);
                        }
                    }
                }
            }
        }

        //Helper method : Fill the area with water
        private static void TrenchFilling(int x, int y, int width, int depth)
        {
            for (int i = x - width; i <= x + width; i++)
            {
                for (int j = y; j <= y + depth; j++)
                {
                    Tile tile = Framing.GetTileSafely(i, j);

                    tile.LiquidType = LiquidID.Water;
                    tile.LiquidAmount = byte.MaxValue;
                }
            }
        }

        //Helper methods : Get a "smoother" for X and Y
        private static float GetSmootherX(int style)
        {
            if (style == (int)TrenchStyle.Vertical)
            {
                return 450f;
            }
            else if (style == (int)TrenchStyle.Horizontal)
            {
                return 850f;
            }
            else
            {
                return 650f;
            }
        }

        private static float GetSmootherY(int style)
        {
            if (style == (int)TrenchStyle.Vertical)
            {
                return 850f;
            }
            else if (style == (int)TrenchStyle.Horizontal)
            {
                return 450f;
            }
            else
            {
                return 650f;
            }
        }
    }
}