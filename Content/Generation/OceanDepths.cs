using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Microsoft.Xna.Framework;

using IAmLostInASea.Common;

namespace IAmLostInASea.Content.Generation
{
    public class OceanDepths : ModSystem
    {
        //Generation values
        private static int DepthsCenterX;
        private static int DepthsLeftX;
        private static int DepthsRightX;
        private static int PlaceDepthsY;
        private static int DepthsLimit;
        private static List<Vector2> Positions;

        //Main
        public static void DepthsGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Waterlurkers";

            //Initialize list
            Positions = new List<Vector2>();

            //Place it on the jungle side
            bool LeftClosestToCenter;

            if (GenVars.dungeonSide == -1)
            {
                DepthsRightX = Main.maxTilesX - 102;
                DepthsLeftX = Main.maxTilesX - 400;

                LeftClosestToCenter = true;
            }
            else
            {
                DepthsRightX = 400;
                DepthsLeftX = 102;

                LeftClosestToCenter = false;
            }

            //Get the center and Y origin
            DepthsCenterX = DepthsLeftX + (Math.Abs(DepthsLeftX - DepthsRightX) / 2);
            PlaceDepthsY = (int)Main.worldSurface + 50;

            //Depth limit that scales with world size
            DepthsLimit = (int)(Main.maxTilesY * 0.75);

            //Set points
            int distance = (DepthsLimit - PlaceDepthsY) / 3;

            Vector2 p1, p3, p5; //Odd closest to center
            Vector2 p2, p4, p6;

            if (LeftClosestToCenter)
            {
                p1 = GetVector2(DepthsLeftX, PlaceDepthsY);
                p3 = GetVector2(DepthsLeftX, PlaceDepthsY + distance);
                p5 = GetVector2(DepthsLeftX, PlaceDepthsY + (distance * 2));

                p2 = GetVector2(DepthsRightX, PlaceDepthsY + (distance / 2));
                p4 = GetVector2(DepthsRightX, PlaceDepthsY + (distance / 2) + distance);
                p6 = GetVector2(DepthsRightX, PlaceDepthsY + (distance / 2) + distance * 2);
            }
            else
            {
                p1 = GetVector2(DepthsRightX, PlaceDepthsY);
                p3 = GetVector2(DepthsRightX, PlaceDepthsY + distance);
                p5 = GetVector2(DepthsRightX, PlaceDepthsY + (distance * 2));

                p2 = GetVector2(DepthsLeftX, PlaceDepthsY + (distance / 2));
                p4 = GetVector2(DepthsLeftX, PlaceDepthsY + (distance / 2) + distance);
                p6 = GetVector2(DepthsLeftX, PlaceDepthsY + (distance / 2) + distance * 2);
            }

            //Generate points
            GeneratePoints(p1, p2);
            GeneratePoints(p2, p3);
            GeneratePoints(p3, p4);
            GeneratePoints(p4, p5);
            GeneratePoints(p5, p6);

            //Test
            foreach (Vector2 position in Positions)
            {
                WorldUtils.Gen(position.ToPoint(), new Shapes.Circle(5), Actions.Chain(new GenAction[]
                {
                    new Actions.SetTile(TileID.RubyGemspark),
                }));
            }
        }

        public static Vector2 GetVector2(int X, int Y)
        {
            int newX = X + WorldGen.genRand.Next(-5, 5);
            int newY = Y + WorldGen.genRand.Next(-10, 10);

            return new Vector2(newX, newY);
        }

        public static void GeneratePoints(Vector2 p0, Vector2 p3)
        {
            //Get p1 and p2
            Vector2 p1 = new Vector2(DepthsCenterX, p0.Y);
            Vector2 p2 = new Vector2(DepthsCenterX, p3.Y);

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
    }
}