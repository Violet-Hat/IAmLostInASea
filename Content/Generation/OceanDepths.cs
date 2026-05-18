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
        //Enums of generation styles and tasks
        public enum Styles
        {
            Snake,
            HighCurve,
            DeepCurve,
            Straight
        }

        public enum Tasks
        {
            SandBase,
            CaveTunnels,
            FloodTunnels
        }

        //Generation values
        private static int DepthsLeftX;
        private static int DepthsRightX;
        private static int PlaceDepthsY;
        private static int DepthsLimit;
        
        readonly static int Modulus = 15;
        readonly static int MaxRandSize = 4;

        private static List<Vector2> ZeroToUlt; //From p0 to the last point
        private static List<Vector2> Positions; //Positions to cave the tunnels

        //Main: Ocean Depths Generation
        public static void DepthsGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Waterlurkers";

            //Initialize lists
            ZeroToUlt = [];
            Positions = [];

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
                GenerateCubicPoints(ZeroToUlt[i], ZeroToUlt[i + 1]);
            }
            progress.Set(0.25);

            //Tasks
            GenerationTask(18, 40, -1, (int)Tasks.SandBase, (int)p0.Y + 20);
            progress.Set(0.5);

            GenerationTask(6, 25, -1, (int)Tasks.CaveTunnels, randSize: true);
            progress.Set(0.75);

            GenerationTask(12, 30, -1, (int)Tasks.FloodTunnels);
            progress.Set(0.75);
        }

        //Get a vector2 with a slight randomized Y value
        public static Vector2 GetVector2(int X, int Y)
        {
            int newX = X;
            int newY = Y + WorldGen.genRand.Next(-10, 11);

            return new Vector2(newX, newY);
        }

        //Fill the list of positions
        public static void GenerateCubicPoints(Vector2 p0, Vector2 p3)
        {
            //Get p1 and p2
            int cX = (int)(Math.Min(p0.X, p3.X) + (Math.Abs(p0.X - p3.X) / 2));

            Vector2 p1 = new(cX, p0.Y);
            Vector2 p2 = new(cX, p3.Y);

            //Bezier curve
            int segments = 1000;

            for (int i = 0; i < segments; i++)
            {
                if (i % Modulus == 0)
                {
                    float t = i / (float)segments;
                    Vector2 pos = BezierCurve.CubicBezier(t, p0, p1, p2, p3);
                    Positions.Add(pos);
                }
            }
        }

        public static void GenerationTask(int s1, int s2, int s3, int task, int limit = -1, bool randSize = false)
        {
            //Shapes
            Shapes.Circle tunnelShape;
            Shapes.Circle caveShape = (s3 == -1) ? new Shapes.Circle(s2) : new Shapes.Circle(s2, s3);

            //Tasks
            if (task == (int)Tasks.SandBase)
            {
                //Place sand base for tunnels
                foreach (Vector2 position in Positions)
                {
                    if (position.Y > limit)
                    {
                        int extraSize = randSize ? WorldGen.genRand.Next(MaxRandSize) : 0;
                        tunnelShape = new Shapes.Circle(s1 + extraSize);
                        
                        WorldUtils.Gen(position.ToPoint(), tunnelShape, Actions.Chain(
                        [
                            new Modifiers.Blotches(2, 0.4),
                            new Actions.SetTile(TileID.HardenedSand),
                        ]));
                    }
                }

                //Place sand base for "caves"
                for (int i = 1; i < ZeroToUlt.Count; i++)
                {
                    WorldUtils.Gen(ZeroToUlt[i].ToPoint(), caveShape, Actions.Chain(
                    [
                        new Modifiers.Blotches(2, 0.4),
                        new Actions.SetTile(TileID.HardenedSand),
                    ]));
                }
            }
            else if (task == (int)Tasks.CaveTunnels)
            {
                //Create tunnels
                foreach (Vector2 position in Positions)
                {
                    int extraSize = randSize ? WorldGen.genRand.Next(MaxRandSize) : 0;
                    tunnelShape = new Shapes.Circle(s1 + extraSize);

                    WorldUtils.Gen(position.ToPoint(), tunnelShape, Actions.Chain(
                    [
                        new Actions.ClearTile(),
                    ]));
                }

                //Create "caves"
                for (int i = 1; i < ZeroToUlt.Count; i++)
                {
                    WorldUtils.Gen(ZeroToUlt[i].ToPoint(), caveShape, Actions.Chain(
                    [
                        new Actions.ClearTile(),
                    ]));
                }
            }
            else if (task == (int)Tasks.FloodTunnels)
            {
                //Flood tunnels
                foreach (Vector2 position in Positions)
                {
                    int extraSize = randSize ? WorldGen.genRand.Next(MaxRandSize) : 0;
                    tunnelShape = new Shapes.Circle(s1 + extraSize);

                    WorldUtils.Gen(position.ToPoint(), tunnelShape, Actions.Chain(
                    [
                        new Modifiers.IsNotSolid(),
                        new Actions.SetLiquid(),
                    ]));
                }

                //Flood "caves"
                for (int i = 1; i < ZeroToUlt.Count; i++)
                {
                    WorldUtils.Gen(ZeroToUlt[i].ToPoint(), caveShape, Actions.Chain(
                    [
                        new Modifiers.IsNotSolid(),
                        new Actions.SetLiquid(),
                    ]));
                }
            }
        }
    }
}