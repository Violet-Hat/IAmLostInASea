using System;
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
        static int mazeCenterX;
        static int mazeCenterY;
        static int mazeWidth;
        static int mazeHeight;

        static readonly int minoRoomWidth = 5;
        static readonly int minoRoomHeight = 5;

        static readonly int cellSize = 5;
        static readonly int cellDistance = cellSize + 3;
        static readonly int cellClearing = (int)Math.Floor(cellSize / 2f);

        static Cell[,] grid;
        static readonly Stack<Cell> stack = new();

        static Cell current;

        public static void MazeGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Bull of Hell";
        
            //Maze center and sizes
            mazeCenterX = Main.maxTilesX / 2;
            mazeCenterY = (int)(Main.maxTilesY * 0.6f);

            mazeWidth = (Main.maxTilesX >= 8400) ? 29 : (Main.maxTilesX == 6400) ? 25 : 21;
            mazeHeight = (Main.maxTilesX >= 8400) ? 29 : (Main.maxTilesX == 6400) ? 25 : 21;

            //Start the grid
            grid = new Cell[mazeWidth, mazeHeight];

            //Add cells to the grid
            int mazeTopCornerX = (int)(mazeCenterX - (Math.Floor(mazeWidth / 2f) * cellDistance));
            int mazeTopCornerY = (int)(mazeCenterY - (Math.Floor(mazeHeight / 2f) * cellDistance));

            int centerEdgeX = (mazeWidth - minoRoomWidth) / 2;
            int centerEdgeY = (mazeHeight - minoRoomHeight) / 2;

            for (int i = 0; i < mazeWidth; i++)
            {
                for (int j = 0; j < mazeHeight; j++)
                {
                    //The dead space is the center part of the maze
                    if (i < centerEdgeX || j < centerEdgeY || i >= centerEdgeX + minoRoomWidth || j >= centerEdgeY + minoRoomHeight)
                    {
                        int offsetX = i * cellDistance;
                        int offsetY = j * cellDistance;
                        
                        Cell cell = new(i, j, mazeTopCornerX + offsetX, mazeTopCornerY + offsetY);
                        grid[i, j] = cell;
                    }
                }
            }

            //Place base
            PlaceMazeRectangle(mazeTopCornerX, mazeTopCornerY);
            progress.Set(0.5);

            //Time to generate the maze
            List<Cell> neighbors;

            current = grid[0, 0];
            current.visited = true;
            stack.Push(current);

            while (stack.Count != 0)
            {
                //Pop the cell from the stack and make it the current cell
                current = stack.Pop();

                //Check if it has unvisited neighbors
                neighbors = HasNeighbors(current);

                if (neighbors.Count > 0)
                {
                    //Push the current cell to the stack
                    stack.Push(current);

                    //Choose one of the unvisited neighbors
                    Cell chosen = WorldGen.genRand.Next(neighbors);

                    //Remove the wall between the current cell and the chosen cell
                    ConnectCells(current, chosen);

                    //Mark the chosen cell as visited and push it to the stack
                    chosen.visited = true;
                    stack.Push(chosen);
                }
            }

            //Dig entrances to the exterior and center
            DigEntrances(centerEdgeX, centerEdgeY);

            //Place platforms
            PlacePlatforms();

            //Dig the center area
            DigCenterArea();
        }

        private static void PlaceMazeRectangle(int topCornerX, int topCornerY)
        {
            int originX = topCornerX - cellDistance;
            int originY = topCornerY - cellDistance;
            Point origin = new(originX, originY);

            int rectWidth = (mazeWidth * cellDistance) + cellDistance + 1;
            int rectHeight = (mazeHeight * cellDistance) + cellDistance + 1;

            ShapeHelper.PlaceRectangle(origin, TileID.BlueDungeonBrick, WallID.BlueDungeon, rectWidth, rectHeight, 0);
        }

        private static List<Cell> HasNeighbors(Cell cell)
        {
            //List of neighbors
            List<Cell> neighbors = [];

            //Add the unvisited neighbors to the neighbor list
            if (cell.i - 1 > -1)
            {
                Cell left = grid[cell.i - 1, cell.j];

                if (left != null && !left.visited)
                {
                    neighbors.Add(left);
                }
            }
            if (cell.i + 1 < mazeWidth)
            {
                Cell right = grid[cell.i + 1, cell.j];

                if (right != null && !right.visited)
                {
                    neighbors.Add(right);
                }    
            }
            if (cell.j - 1 > -1)
            {
                Cell top = grid[cell.i, cell.j - 1];

                if (top != null && !top.visited)
                {
                    neighbors.Add(top);
                }
            }
            if (cell.j + 1 < mazeHeight)
            {
                Cell bottom = grid[cell.i, cell.j + 1];

                if (bottom != null && !bottom.visited)
                {
                    neighbors.Add(bottom);
                }
            }
            
            return neighbors;
        }

        private static void ConnectCells(Cell origin, Cell destiny)
        {
            //Get the distances
            int distanceX = origin.x - destiny.x;
            int distanceY = origin.y - destiny.y;

            //If the distance on the X axis is not 0, make an horizontal path
            if (distanceX != 0)
            {
                //Make sure to start from the smaller point
                int startX = (origin.x < destiny.x) ? origin.x : destiny.x;
                int endX = (startX == origin.x) ? destiny.x : origin.x;

                for (int y = origin.y - cellClearing; y <= origin.y + cellClearing; y++)
                {
                    for (int x = startX - cellClearing; x <= endX + cellClearing; x++)
                    {
                        if (Framing.GetTileSafely(x, y).HasTile)
                        {
                            WorldGen.KillTile(x, y, noItem:true);
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

                for (int x = origin.x - cellClearing; x <= origin.x + cellClearing; x++)
                {
                    for (int y = startY - cellClearing; y <= endY + cellClearing; y++)
                    {
                        if (Framing.GetTileSafely(x, y).HasTile)
                        {
                            WorldGen.KillTile(x, y);
                        }
                    }
                }
            }
        }

        private static void DigEntrances(int edgeX, int edgeY)
        {
            //Cells in the corners
            Cell topLeft = grid[0, 0];
            Cell topRight = grid[mazeWidth - 1, 0];
            Cell bottomLeft = grid[0, mazeHeight - 1];
            Cell bottomRight = grid[mazeWidth - 1, mazeHeight - 1];

            //l = tunnel length, w = tunnel width
            for (int l = 0; l <= cellDistance; l++)
            {
                for (int w = -cellClearing; w <= cellClearing; w++)
                {
                    //Top left tunnel, goes left
                    WorldGen.KillTile(topLeft.x - l, topLeft.y + w, noItem:true);

                    //Top right tunnel, goes up
                    WorldGen.KillTile(topRight.x + w, topRight.y - l, noItem:true);

                    //Bottom left tunnel, goes down
                    WorldGen.KillTile(bottomLeft.x + w, bottomLeft.y + l, noItem:true);

                    //Bottom right tunnel, goes right
                    WorldGen.KillTile(bottomRight.x + l, bottomRight.y + w, noItem:true);
                }
            }

            //Possible entrances to the center
            int mazeHalfWidth = (int)Math.Floor(mazeWidth / 2f);
            int mazeHalfHeigth = (int)Math.Floor(mazeHeight / 2f);

            Cell leftEdge = grid[edgeX - 1, mazeHalfHeigth];
            Cell rightEdge = grid[edgeX + minoRoomWidth, mazeHalfHeigth];
            Cell topEdge = grid[mazeHalfWidth, edgeY - 1];
            Cell bottomEdge = grid[mazeHalfWidth, edgeY + minoRoomHeight];

            //List
            Cell[] entrances = [leftEdge, rightEdge, topEdge, bottomEdge];

            //Choose one at random
            Cell chosen = WorldGen.genRand.Next(entrances);

            //l = tunnel length, w = tunnel width
            for (int l = 0; l <= cellDistance; l++)
            {
                for (int w = -cellClearing; w <= cellClearing; w++)
                {
                    //Left tunnel, goes right
                    if (chosen == leftEdge)
                    {
                        WorldGen.KillTile(chosen.x + l, chosen.y + w, noItem:true);
                    }

                    //Right tunnel, goes left
                    if (chosen == rightEdge)
                    {
                        WorldGen.KillTile(chosen.x - l, chosen.y + w, noItem:true);
                    }

                    //Top tunnel, goes down
                    if (chosen == topEdge)
                    {
                        WorldGen.KillTile(chosen.x + w, chosen.y + l, noItem:true);
                    }

                    //Bottom tunnel, goes up
                    if (chosen == bottomEdge)
                    {
                        WorldGen.KillTile(chosen.x + w, chosen.y - l, noItem:true);
                    }
                }
            }
        }

        private static void PlacePlatforms()
        {
            Tile tileTop;
            Tile tileBottom;

            int x;
            int topY;
            int bottomY;

            //For each valid cell, place a platform beneath it
            foreach (Cell cell in grid)
            {
                if (cell != null)
                {
                    for (int i = -cellClearing; i <= cellClearing; i++)
                    {
                        x = cell.x + i;
                        topY = cell.y - cellClearing - 1;
                        bottomY = cell.y + cellClearing + 1;

                        //Place platform above the cell
                        tileTop = Framing.GetTileSafely(x, topY);

                        if (!tileTop.HasTile)
                        {
                            WorldGen.PlaceTile(x, topY, TileID.Platforms, style: 6);
                        }
                        
                        //Place platform below the cell
                        tileBottom = Framing.GetTileSafely(x, bottomY);

                        if (!tileBottom.HasTile)
                        {
                            WorldGen.PlaceTile(x, bottomY, TileID.Platforms, style: 6);
                        }
                    }
                }
            }
        }

        private static void DigCenterArea()
        {
            //Center width and height
            int width = (int)((Math.Floor(minoRoomWidth / 2f) * cellDistance) + cellClearing);
            int height = (int)((Math.Floor(minoRoomHeight / 2f) * cellDistance) + cellClearing);

            //Dig the center area
            for (int x = mazeCenterX - width; x <= mazeCenterX + width; x++)
            {
                for (int y = mazeCenterY - height; y <= mazeCenterY + height; y++)
                {
                    WorldGen.KillTile(x, y, noItem:true);
                }
            }

            //Try placing platforms below it in case the entrance is at the bottom
            int j = mazeCenterY + height + 1;

            for (int i = mazeCenterX - width; i <= mazeCenterX + width; i++)
            {
                Tile tile = Framing.GetTileSafely(i, j);

                if (!tile.HasTile)
                {
                    WorldGen.PlaceTile(i, j, TileID.Platforms, style: 6);
                }
            }
        }
    }
}