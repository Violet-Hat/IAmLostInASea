using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;

using IAmLostInASea.Content.Generation.Objects;

namespace IAmLostInASea.Content.Generation
{
    public class Maze
    {
        //Generation values
        static int MazeCenterX;
        static int MazeCenterY;

        static readonly int Width = 21;
        static readonly int Height = 21;

        static readonly int cellSize = 5;
        static readonly int cellPadding = 3;
        static readonly int cellDistance = cellSize + cellPadding;

        static readonly Cell[,] Grid = new Cell[Width, Height];
        static readonly Stack<Cell> Stack = new();

        static Cell Current;

        public static void MazeGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Bull of Hell";

            //Maze center
            MazeCenterX = Main.maxTilesX / 2;
            MazeCenterY = (int)(Main.maxTilesY * 0.6f);

            //Add cells to the grid, i and j are the positions in the grid
            int topCornerX = MazeCenterX - ((Width - 1) / 2 * cellDistance);
            int topCornerY = MazeCenterY - ((Height - 1) / 2 * cellDistance);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int offsetX = i * cellDistance;
                    int offsetY = j * cellDistance;

                    Cell cell = new(i, j, topCornerX + offsetX, topCornerY + offsetY);
                    Grid[i, j] = cell;
                }
            }

            //Place base
            int baseOriginX = topCornerX - cellDistance;
            int baseOriginY = topCornerY - cellDistance;
            Point baseOrigin = new(baseOriginX, baseOriginY);

            int baseWidth = (Width * cellDistance) + cellDistance;
            int baseHeight = (Height * cellDistance) + cellDistance;

            ShapeHelper.PlaceRectangle(baseOrigin, TileID.BlueDungeonBrick, WallID.BlueDungeon, baseWidth, baseHeight, 0);
            progress.Set(0.5);

            //Time to generate the maze
            List<Cell> neighbors;

            Current = Grid[0, 0];
            Current.visited = true;
            Stack.Push(Current);

            while (Stack.Count != 0)
            {
                //Pop the cell from the stack and make it the current cell
                Current = Stack.Pop();

                //Check if it has unvisited neighbors
                neighbors = HasNeighbors(Current);

                if (neighbors.Count > 0)
                {
                    //Push the current cell to the stack
                    Stack.Push(Current);

                    //Choose one of the unvisited neighbors
                    Cell chosen = WorldGen.genRand.Next(neighbors);

                    //Remove the wall between the current cell and the chosen cell
                    ConnectCells(Current, chosen);

                    //Mark the chosen cell as visited and push it to the stack
                    chosen.visited = true;
                    Stack.Push(chosen);
                }
            }
        }

        private static List<Cell> HasNeighbors(Cell cell)
        {
            //List of neighbors
            List<Cell> neighbors = [];

            //Add the unvisited neighbors to the neighbor list
            if (cell.i - 1 > -1)
            {
                Cell left = Grid[cell.i - 1, cell.j];

                if (!left.visited)
                {
                    neighbors.Add(left);
                }
            }
            if (cell.i + 1 < Width)
            {
                Cell right = Grid[cell.i + 1, cell.j];

                if (!right.visited)
                {
                    neighbors.Add(right);
                }    
            }
            if (cell.j - 1 > -1)
            {
                Cell top = Grid[cell.i, cell.j - 1];

                if (!top.visited)
                {
                    neighbors.Add(top);
                }
            }
            if (cell.j + 1 < Height)
            {
                Cell bottom = Grid[cell.i, cell.j + 1];

                if (!bottom.visited)
                {
                    neighbors.Add(bottom);
                }
            }
            
            return neighbors;
        }

        private static void ConnectCells(Cell origin, Cell destiny)
        {
            int clearPadding = cellSize - cellPadding;

            //Get the distances
            int distanceX = origin.x - destiny.x;
            int distanceY = origin.y - destiny.y;

            //If the distance on the X axis is not 0, make an horizontal path
            if (distanceX != 0)
            {
                //Make sure to start from the smaller point
                int startX = (origin.x < destiny.x) ? origin.x : destiny.x;
                int endX = (startX == origin.x) ? destiny.x : origin.x;

                for (int y = origin.y - clearPadding; y <= origin.y + clearPadding; y++)
                {
                    for (int x = startX - clearPadding; x <= endX + clearPadding; x++)
                    {
                        if (Framing.GetTileSafely(x, y).HasTile)
                        {
                            WorldGen.KillTile(x, y);
                        }
                    }
                }
            }

            //If the distance on the Y axis is not 0, make an vertical path
            if (distanceY != 0)
            {
                //Make sure to start from the smaller point
                int startY = (origin.y < destiny.y) ? origin.y : destiny.y;
                int endY = (startY == origin.y) ? destiny.y : origin.y;

                for (int x = origin.x - clearPadding; x <= origin.x + clearPadding; x++)
                {
                    for (int y = startY - clearPadding; y <= endY + clearPadding; y++)
                    {
                        if (Framing.GetTileSafely(x, y).HasTile)
                        {
                            WorldGen.KillTile(x, y);
                        }
                    }
                }
            }
        }
    }
}