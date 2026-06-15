using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

using IAmLostInASea.Common;
using IAmLostInASea.Enums;
using IAmLostInASea.Content.Generation.Objects;

namespace IAmLostInASea.Content.Generation
{
    public class Maze
    {
        //Generation values
        static int MazeCenterX;
        static int MazeCenterY;
        static int MazeSize;

        static readonly int cellSize = 5;
        static readonly int cellDistance = cellSize + 3;

        static readonly List<Cell> Grid = [];

        public static void MazeGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Bull of Hell";

            //Maze center
            MazeCenterX = Main.maxTilesX / 2;
            MazeCenterY = (int)(Main.maxTilesY * 0.6f);

            MazeSize = 10;

            //Add cells to the grid
            for (int x = -MazeSize; x <= MazeSize; x++)
            {
                for (int y = -MazeSize; y <= MazeSize; y++)
                {
                    int offsetX = x * cellDistance;
                    int offsetY = y * cellDistance;

                    Cell cell = new(MazeCenterX - offsetX, MazeCenterY - offsetY, cellSize);
                    Grid.Add(cell);
                }
            }

            //Place base
            int startX = MazeCenterX - (MazeSize * cellDistance) - (cellDistance / 2);
            int startY = MazeCenterY - (MazeSize * cellDistance) - (cellDistance / 2);

            int endX = MazeCenterX + (MazeSize * cellDistance) + (cellDistance / 2);
            int endY = MazeCenterY + (MazeSize * cellDistance) + (cellDistance / 2);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    WorldGen.KillTile(x, y, noItem: true);
                    WorldGen.KillWall(x, y);

                    WorldGen.PlaceTile(x, y, TileID.BlueDungeonBrick);
                }
            }

            //Testing
            foreach (Cell cell in Grid)
            {
                cell.Place();
            }
        }
    }
}