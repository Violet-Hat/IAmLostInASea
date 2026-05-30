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
            int xRand = WorldGen.genRand.Next(55, 95);
            int entrancePosX = (GenVars.dungeonSide == -1) ? Main.maxTilesX - xRand : xRand;
            int entrancePosY = WorldGenTools.FindSurface(entrancePosX) - 5;

            Vector2 entrancePosition = new(entrancePosX, entrancePosY);

            int style = 4; // WorldGen.genRand.Next(5)
            GeneratePoints(entrancePosition, OceanicCaves[0].origin, style);

            for (int i = 0; i < (OceanicCaves.Count - 1); i++)
            {
                GeneratePoints(OceanicCaves[i].origin, OceanicCaves[i + 1].origin, style);
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

            Vector2 cave1 = GetCaveVector(DepthsCenterX + biomePadding, DepthsCenterY - biomePadding);
            Vector2 cave2 = GetCaveVector(DepthsCenterX - biomePadding, DepthsCenterY - biomePadding);
            Vector2 cave3 = GetCaveVector(DepthsCenterX, DepthsCenterY + biomePadding);

            //Make sure the first cave is the one away from the edge
            if (GenVars.dungeonSide == -1)
            {
                (cave1, cave2) = (cave2, cave1);
            }

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

        public static int GetPointsCenter(int x1, int x2) => Math.Min(x1, x2) + (Math.Abs(x1 - x2) / 2);

        private static void GeneratePoints(Vector2 start, Vector2 end, int style)
        {
            switch (style)
            {
                case (int)TunnelStylesEnum.Snake:
                    GenerateCubicPoints(start, end, false);
                    break;
                
                case (int)TunnelStylesEnum.OddBall:
                    GenerateCubicPoints(start, end, true);
                    break;
                
                case (int)TunnelStylesEnum.HighCurve:
                    GenerateQuadraticPoints(start, end, false);
                    break;
                
                case (int)TunnelStylesEnum.LowCurve:
                    GenerateQuadraticPoints(start, end, true);
                    break;
                
                case (int)TunnelStylesEnum.Straight:
                    GenerateLinearPoints(start, end);
                    break;
            }
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

        public static void GenerateQuadraticPoints(Vector2 p0, Vector2 p2, bool lowCurve)
        {
            //Get p1
            int cX = GetPointsCenter((int)p0.X, (int)p2.X);
            float control = lowCurve ? Math.Max(p0.Y, p2.Y) : Math.Min(p0.Y, p2.Y);

            Vector2 p1 = new(cX, (int)control);

            //Bezier curve
            int modulus = 10;
            int segments = 1000;

            for (int i = 0; i < segments; i++)
            {
                if (i % modulus == 0)
                {
                    float t = i / (float)segments;
                    Vector2 pos = BezierCurve.QuadraticBezier(t, p0, p1, p2);
                    TunnelPositions.Add(pos);
                }
            }
        }

        public static void GenerateCubicPoints(Vector2 p0, Vector2 p3, bool oddBall)
        {
            //Get p1 and p2
            int cX = GetPointsCenter((int)p0.X, (int)p3.X);
            float control1 = oddBall ? Math.Max(p0.Y, p3.Y) : Math.Min(p0.Y, p3.Y);
            float control2 = oddBall ? Math.Min(p0.Y, p3.Y) : Math.Max(p0.Y, p3.Y);

            Vector2 p1 = new(cX, (int)control1);
            Vector2 p2 = new(cX, (int)control2);

            //Bezier curve
            int modulus = 10;
            int segments = 1000;

            for (int i = 0; i < segments; i++)
            {
                if (i % modulus == 0)
                {
                    float t = i / (float)segments;
                    Vector2 pos = BezierCurve.CubicBezier(t, p0, p1, p2, p3);
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
                //Thick sand roof for the tunnels
                Point roofOrigin = new((int)position.X, (int)position.Y - 5);

                ShapeHelper.PlaceCircle(roofOrigin, TileID.HardenedSand, WallID.None, pathSize, 0, replaceOnly: true);
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
            
            int endY = DepthsCenterY + (DepthsLength / 2) + 10;

            for (int l = 0; l < 5; l++)
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