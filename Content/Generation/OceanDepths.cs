using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

using IAmLostInASea.Common;

namespace IAmLostInASea.Content.Generation
{
    public class OceanDepths : ModSystem
    {
        //Generation values
        private static int DepthsLeftX;
        private static int DepthsRightX;
        private static int PlaceDepthsY;
        private static int DepthsLimit;
        private static List<Vector2> ZeroToUlt; //From p0 to the last point
        private static List<Vector2> Positions; //Positions to cave the tunnels

        //Main: Ocean Depths Generation
        public static void DepthsGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Waterlurkers";

            //Initialize lists
            ZeroToUlt = new List<Vector2>();
            Positions = new List<Vector2>();

            //Place it on the jungle side
            bool LeftClosestToCenter;

            if (GenVars.dungeonSide == -1)
            {
                DepthsRightX = Main.maxTilesX - 90;
                DepthsLeftX = Main.maxTilesX - 320;

                LeftClosestToCenter = true;
            }
            else
            {
                DepthsRightX = 320;
                DepthsLeftX = 90;

                LeftClosestToCenter = false;
            }

            //Get the Y origin and Depth limit
            PlaceDepthsY = (int)Main.worldSurface + 50;
            DepthsLimit = (int)(Main.maxTilesY * 0.55);

            //Set points
            int controlAmount = 2;
            int distance = (DepthsLimit - PlaceDepthsY) / controlAmount;

            Vector2 p0; //Start

            if (LeftClosestToCenter)
            {
                p0 = new Vector2(Main.maxTilesX - 120, WorldgenTools.FindSurface(Main.maxTilesX - 120) - 10);
                ZeroToUlt.Add(p0);

                for (int i = 0; i < controlAmount; i++)
                {
                    //Close to center
                    Vector2 v1 = GetVector2(DepthsLeftX + (10 * i), PlaceDepthsY + (distance * i));
                    ZeroToUlt.Add(v1);

                    //Far from center
                    Vector2 v2 = GetVector2(DepthsRightX - (10 * i), PlaceDepthsY + (distance / 2) + (distance * i));
                    ZeroToUlt.Add(v2);
                }
            }
            else
            {
                p0 = new Vector2(120, WorldgenTools.FindSurface(120) - 10);
                ZeroToUlt.Add(p0);

                for (int i = 0; i < controlAmount; i++)
                {
                    //Close to center
                    Vector2 v1 = GetVector2(DepthsRightX - (10 * i), PlaceDepthsY + (distance * i));
                    ZeroToUlt.Add(v1);

                    //Far from center
                    Vector2 v2 = GetVector2(DepthsLeftX + (10 * i), PlaceDepthsY + (distance / 2) + (distance * i));
                    ZeroToUlt.Add(v2);
                }
            }

            //Generate points
            for (int i = 0; i < (ZeroToUlt.Count - 1); i++)
            {
                GeneratePoints(ZeroToUlt[i], ZeroToUlt[i + 1]);
            }
            progress.Set(0.25);

            //Place sand base
            SandBase((int)p0.Y + 20, 15, 40, 20);
            progress.Set(0.5);

            //Cave tunnels
            CaveTunnels(6, 25, 12);
            progress.Set(0.75);

            //Fill the tunnels
            FloodTunnels(8, 30, 15);
        }

        //Get ocean water height
        public static int FindWaterSurface(int X, int startY = 10)
        {
            bool FoundSurface = false;
            int attempts = 0;

            int Y = startY;

            while (!FoundSurface && attempts++ < 100000)
            {
                Tile tile = Framing.GetTileSafely(X, Y);

				while ((!tile.CheckingLiquid || !WorldgenTools.NoFloatingIslands(X, Y)) && Y <= (Main.worldSurface + 100))
				{
					Y++;
				}
				if (tile.CheckingLiquid && WorldgenTools.NoFloatingIslands(X, Y))
				{
					FoundSurface = true;
				}
			}

            return Y;
        }

        //Get a vector2 with a slight randomized Y value
        public static Vector2 GetVector2(int X, int Y)
        {
            int newX = X;
            int newY = Y + WorldGen.genRand.Next(-10, 11);

            return new Vector2(newX, newY);
        }

        //Fill the list of positions
        public static void GeneratePoints(Vector2 p0, Vector2 p3)
        {
            //Get p1 and p2
            int cX = (int)(Math.Min(p0.X, p3.X) + (Math.Abs(p0.X - p3.X) / 2));

            Vector2 p1 = new Vector2(cX, p0.Y);
            Vector2 p2 = new Vector2(cX, p3.Y);

            //Bezier curve
            int segments = 1000;

            for (int i = 0; i < segments; i++)
            {
                if (i % 10 == 0)
                {
                    float t = i / (float)segments;
                    Vector2 pos = BezierCurve.CubicBezier(t, p0, p1, p2, p3);
                    Positions.Add(pos);
                }
            }
        }

        //Sand base for the tunnels
        public static void SandBase(int limit, int s1, int s2, int s3)
        {
            foreach (Vector2 position in Positions)
            {
                if (position.Y > limit)
                {
                    WorldUtils.Gen(position.ToPoint(), new Shapes.Circle(s1), Actions.Chain(new GenAction[]
                    {
                        new Modifiers.Blotches(2, 0.4),
                        new Actions.SetTile(TileID.HardenedSand),
                    }));
                }
            }

            for (int i = 1; i < ZeroToUlt.Count; i++)
            {
                WorldUtils.Gen(ZeroToUlt[i].ToPoint(), new Shapes.Circle(s2, s3), Actions.Chain(new GenAction[]
                {
                    new Modifiers.Blotches(2, 0.4),
                    new Actions.SetTile(TileID.HardenedSand),
                }));
            }
        }

        //The tunnels
        public static void CaveTunnels(int s1, int s2, int s3)
        {
            foreach (Vector2 position in Positions)
            {
                WorldUtils.Gen(position.ToPoint(), new Shapes.Circle(s1), Actions.Chain(new GenAction[]
                {
                    new Actions.ClearTile(),
                }));
            }

            for (int i = 1; i < ZeroToUlt.Count; i++)
            {
                WorldUtils.Gen(ZeroToUlt[i].ToPoint(), new Shapes.Circle(s2, s3), Actions.Chain(new GenAction[]
                {
                    new Actions.ClearTile(),
                }));
            }
        }

        //Flood the tunnels
        public static void FloodTunnels(int s1, int s2, int s3)
        {
            foreach (Vector2 position in Positions)
            {
                WorldUtils.Gen(position.ToPoint(), new Shapes.Circle(s1), Actions.Chain(new GenAction[]
                {
                    new Modifiers.IsNotSolid(),
                    new Actions.SetLiquid(),
                }));
            }

            for (int i = 1; i < ZeroToUlt.Count; i++)
            {
                WorldUtils.Gen(ZeroToUlt[i].ToPoint(), new Shapes.Circle(s2, s3), Actions.Chain(new GenAction[]
                {
                    new Modifiers.IsNotSolid(),
                    new Actions.SetLiquid(),
                }));
            }
        }
    }
}