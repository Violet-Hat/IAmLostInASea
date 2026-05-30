using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

using IAmLostInASea.Common;
using IAmLostInASea.Enums;
using IAmLostInASea.Content.Generation.Objects;

namespace IAmLostInASea.Content.Generation
{
    public class OceanicDepths
    {
        //Generation values
        static int DepthsCenterX;
        static int DepthsCenterY;

        static readonly int DepthsLength = 300;
        static readonly int CaveDoubleRadius = (int)(DepthsLength / 2 * 0.9f);

        static readonly List<OceanicCave> OceanicCaves = []; //From the first to the last cave
        static readonly List<Vector2> TunnelPositions = [];

        public static void DepthsGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Waterlurkers";

            //Depths center
            int padding = 30 + (DepthsLength / 2);
            DepthsCenterX = (GenVars.dungeonSide == -1) ? Main.maxTilesX - padding : padding;
            DepthsCenterY = (int)Main.worldSurface + (DepthsLength / 2) + 30;

            //Get origins for each cave and create the objects
            Vector2[] caveOrigin = GetOrigins();
            for (int i = 0; i < caveOrigin.Length; i++)
            {
                OceanicCaves.Add(new(CaveDoubleRadius / 2, caveOrigin[i]));
            }

            //Generate tunnel points
            padding = 120;
            int entrancePosX = (GenVars.dungeonSide == -1) ? Main.maxTilesX - padding : padding;
            int entrancePosY = WorldGenTools.FindSurface(entrancePosX) - 5;

            Vector2 entrancePosition = new(entrancePosX, entrancePosY);
            GenerateLinearPoints(entrancePosition, OceanicCaves[0].origin);

            //int style = WorldGen.genRand.Next(5);
            for (int i = 0; i < (OceanicCaves.Count - 1); i++)
            {
                GenerateLinearPoints(OceanicCaves[i].origin, OceanicCaves[i + 1].origin);
            }
            progress.Set(0.25);

            //Tasks
            GenerateSandBase(20);
            progress.Set(0.5);

            GenerateTunnels(5);
            progress.Set(0.75);

            FloodTunnels(10);
            progress.Set(0.95);

            Cleaning(entrancePosY);
        }

        private static Vector2[] GetOrigins()
        {
            int biomePadding = DepthsLength / 4;

            Vector2 cave1 = GetCaveVector(DepthsCenterX - biomePadding, DepthsCenterY - biomePadding);
            Vector2 cave2 = GetCaveVector(DepthsCenterX + biomePadding, DepthsCenterY - biomePadding);
            Vector2 cave3 = GetCaveVector(DepthsCenterX, DepthsCenterY + biomePadding);

            //Randomize the second and third cave
            if (WorldGen.genRand.NextBool())
            {
                (cave3, cave2) = (cave2, cave3);
            }

            return [cave1, cave2, cave3];
        }

        private static Vector2 GetCaveVector(int centerX, int centerY, int maxOffsetX = 10, int maxOffsetY = 10)
        {
            int randX = WorldGen.genRand.Next(-maxOffsetX, maxOffsetX + 1);
            int randY = WorldGen.genRand.Next(-maxOffsetY, maxOffsetY + 1);

            return new Vector2(centerX + randX, centerY + randY);
        }

        private static void GenerateLinearPoints(Vector2 p0, Vector2 p1)
        {
            //Bezier curve
            int modulus = 10;
            int segments = 1000;

            for (int i = 0; i < segments; i++)
            {
                if (i % modulus == 0)
                {
                    float t = i / (float)segments;
                    Vector2 pos = BezierCurve.LinearBezier(t, p0, p1);
                    TunnelPositions.Add(pos);
                }
            }
        }

        private static void GenerateSandBase(int pathSize)
        {
            //Place sand base for the paths
            int surfaceLimit;
            int padding = 10;

            foreach (Vector2 position in TunnelPositions)
            {
                surfaceLimit = WorldGenTools.FindSurface((int)position.X) + pathSize + padding;

                //If deep enough, place a solid base, otherwise replace tiles
                if (position.Y > surfaceLimit)
                {
                    ShapeHelper.PlaceCircle(position.ToPoint(), TileID.Sand, WallID.None, pathSize);
                }
            }

            //Place the caves sand base
            foreach (OceanicCave cave in OceanicCaves)
            {
                cave.PlaceBase();
            }
        }

        private static void GenerateTunnels(int pathSize)
        {
            //Create paths
            foreach (Vector2 position in TunnelPositions)
            {
                ShapeHelper.PlaceCircle(position.ToPoint(), -1, WallID.None, pathSize, 0);
            }

            //Create caves
            foreach (OceanicCave cave in OceanicCaves)
            {
                cave.PlaceCave();
            }
        }

        private static void FloodTunnels(int pathSize)
        {
            //Flood paths
            foreach (Vector2 position in TunnelPositions)
            {
                ShapeHelper.PlaceLiquidInCircle(position.ToPoint(), LiquidID.Water, pathSize);
            }

            //Flood caves
            foreach (OceanicCave cave in OceanicCaves)
            {
                cave.FloodCave();
            }
        }

        private static void Cleaning(int startY)
        {
            //Beginning and end of the depths
            int startX = DepthsCenterX - (DepthsLength / 2);
            int endX = DepthsCenterX + (DepthsLength / 2);
            
            int endY = DepthsCenterY + (DepthsLength / 2);

            for (int l = 0; l < 3; l++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        Tile tile = Framing.GetTileSafely(x, y);
                        Tile tileBelow = Framing.GetTileSafely(x, y + 1);

                        if (tile.TileType == TileID.Sand && (!tileBelow.HasTile || tileBelow.TileType == TileID.HardenedSand))
                        {
                            tile.TileType = TileID.HardenedSand;
                        }
                    }
                }
            }
        }
    }
}