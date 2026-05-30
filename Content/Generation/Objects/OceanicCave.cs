using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace IAmLostInASea.Content.Generation.Objects
{
    public class OceanicCave(int radius, Vector2 origin)
    {
        public readonly int radius = radius;
        public readonly Vector2 origin = origin;

        public void PlaceBase()
        {
            ShapeHelper.PlaceCircle(origin.ToPoint(), TileID.Sand, WallID.None, radius);
        }

        public void PlaceCave()
        {
            int caveRadius = (int)(radius * 0.9f);

            //Beginning and end of the cave
            int startX = (int)origin.X - radius;
            int endX = (int)origin.X + radius;

            int startY = (int)origin.Y - radius;
            int endY = (int)origin.Y + radius;

            //Noise values
            int seed = WorldGen.genRand.Next();
            int octaves = 5;

            float clearChance = 0.675f;

            float caveDivX = 1000f;
            float caveDivY = 500f;

            //Dig caves
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (WorldGenTools.IsInCircle(x, y, (int)origin.X, (int)origin.Y, caveRadius))
                    {
                        //Perlin noise values
                        float horizontalOffsetNoise = WorldGenTools.PerlinNoise2D(x / caveDivX, y / caveDivY, octaves, unchecked(seed + 1)) * 0.01f;
                        float cavePerlinValue = WorldGenTools.PerlinNoise2D(x / caveDivX, y / caveDivY, octaves, seed) + 0.5f + horizontalOffsetNoise;
                        float cavePerlinValue2 = WorldGenTools.PerlinNoise2D(x / caveDivX, y / caveDivY, octaves, unchecked(seed - 1)) + 0.5f;
                        float caveNoiseMap = (cavePerlinValue + cavePerlinValue2) * 0.5f;
                        float caveCreationThreshold = horizontalOffsetNoise * 3.5f + 0.2f;

                        //Remove tiles based on the noise and a float value
                        bool noiseCheck = caveNoiseMap * caveNoiseMap > caveCreationThreshold;
                        bool floatCheck = WorldGen.genRand.NextFloat() < clearChance;
                        
                        if (noiseCheck && floatCheck)
                        {
                            WorldGen.KillTile(x, y, noItem: true);
                        }
                    }
                }
            }

            //Smooth the noise
            for (int l = 0; l < 10; l++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        int tileCount = WorldGenTools.MooreTiles(x, y);

                        if (tileCount < 4)
                        {
                            WorldGen.KillTile(x, y, noItem: true);
                        }
                        else if (tileCount > 4)
                        {
                            WorldGen.PlaceTile(x, y, TileID.Sand);
                        }
                    }
                }
            }
        }

        public void FloodCave()
        {
            ShapeHelper.PlaceLiquidInCircle(origin.ToPoint(), LiquidID.Water, radius);
        }
    }
}