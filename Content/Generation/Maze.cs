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
        static readonly int mazeWidth = 21;
        static readonly int mazeHeight = 21;

        static readonly int cellSize = 5;
        static readonly int cellPadding = 3;
        static readonly int cellDistance = cellSize + cellPadding;

        static readonly Cell[,] grid = new Cell[mazeWidth, mazeHeight];
        static readonly Stack<Cell> stack = new();

        static Cell current;

        public static void MazeGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Bull of Hell";
        
            //Maze center
            mazeCenterX = Main.maxTilesX / 2;
            mazeCenterY = (int)(Main.maxTilesY * 0.6f);

            //Center room width, height and edges
            int centerRoomW = 5;
            int centerRoomH = 5;

            int centerEdgeX = (mazeWidth - centerRoomW) / 2;
            int centerEdgeY = (mazeHeight - centerRoomH) / 2;

            //Maze top corners
            int mazeTopCornerX = mazeCenterX - ((mazeWidth - 1) / 2 * cellDistance);
            int mazeTopCornerY = mazeCenterY - ((mazeHeight - 1) / 2 * cellDistance);

            //Add cells to the grid
            for (int i = 0; i < mazeWidth; i++)
            {
                for (int j = 0; j < mazeHeight; j++)
                {
                    //The dead space is the center part of the maze
                    if (i < centerEdgeX || j < centerEdgeY || i >= centerEdgeX + centerRoomW || j >= centerEdgeY + centerRoomH)
                    {
                        int offsetX = i * cellDistance;
                        int offsetY = j * cellDistance;
                        
                        Cell cell = new(i, j, mazeTopCornerX + offsetX, mazeTopCornerY + offsetY);
                        grid[i, j] = cell;
                    }
                }
            }

            int clearPadding = cellSize - cellPadding;

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
                    ConnectCells(current, chosen, clearPadding);

                    //Mark the chosen cell as visited and push it to the stack
                    chosen.visited = true;
                    stack.Push(chosen);
                }
            }

            //Dig entrances to the exterior
            DigExteriorEntrances(clearPadding);

            //Dig a single entrance to the center
            DigInternalEntrance(centerRoomW, centerRoomH, centerEdgeX, centerEdgeY, clearPadding);

            //Place platforms
            PlacePlatforms(clearPadding);

            //Dig the center area
            DigCenterArea(centerRoomW, centerRoomH, clearPadding);
        }

        private static void PlaceMazeRectangle(int topCornerX, int topCornerY)
        {
            int originX = topCornerX - cellDistance;
            int originY = topCornerY - cellDistance;
            Point origin = new(originX, originY);

            int rectWidth = (mazeWidth * cellDistance) + cellDistance + 1;
            int rectHeight = (mazeWidth * cellDistance) + cellDistance + 1;

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

        private static void ConnectCells(Cell origin, Cell destiny, int padding)
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

                for (int y = origin.y - padding; y <= origin.y + padding; y++)
                {
                    for (int x = startX - padding; x <= endX + padding; x++)
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

                for (int x = origin.x - padding; x <= origin.x + padding; x++)
                {
                    for (int y = startY - padding; y <= endY + padding; y++)
                    {
                        if (Framing.GetTileSafely(x, y).HasTile)
                        {
                            WorldGen.KillTile(x, y);
                        }
                    }
                }
            }
        }

        private static void DigExteriorEntrances(int padding)
        {
            //Cells in the corner
            Cell topLeft = grid[0, 0];
            Cell topRight = grid[mazeWidth - 1, 0];
            Cell bottomLeft = grid[0, mazeHeight - 1];
            Cell bottomRight = grid[mazeWidth - 1, mazeHeight - 1];

            //l = tunnel length, w = tunnel width
            for (int l = 0; l <= cellDistance; l++)
            {
                for (int w = -padding; w <= padding; w++)
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
        }

        private static void DigInternalEntrance(int width, int height, int edgeX, int edgeY, int padding)
        {
            //Possible entrances
            Cell leftEdge = grid[edgeX - 1, (mazeHeight - 1) / 2];
            Cell rightEdge = grid[edgeX + width, (mazeHeight - 1) / 2];
            Cell topEdge = grid[(mazeWidth - 1) / 2, edgeY - 1];
            Cell bottomEdge = grid[(mazeWidth - 1) / 2, edgeX + height];

            //List
            Cell[] entrances = [leftEdge, rightEdge, topEdge, bottomEdge];

            //Choose one at random
            Cell chosen = bottomEdge; //WorldGen.genRand.Next(entrances)

            //l = tunnel length, w = tunnel width
            for (int l = 0; l <= cellDistance; l++)
            {
                for (int w = -padding; w <= padding; w++)
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

        private static void PlacePlatforms(int padding)
        {
            //For each valid cell, place a platform beneath it
            foreach (Cell cell in grid)
            {
                if (cell != null)
                {
                    for (int i = -padding; i <= padding; i++)
                    {
                        int x = cell.x + i;
                        int y = cell.y + padding + 1;

                        Tile tile = Framing.GetTileSafely(x, y);

                        if (!tile.HasTile)
                        {
                            WorldGen.PlaceTile(x, y, TileID.Platforms, style: 6);
                        }
                    }
                }
            }
        }

        private static void DigCenterArea(int gridWidth, int gridHeight, int padding)
        {
            //Center width and height
            int width = ((gridWidth - 1) / 2 * cellDistance) + padding;
            int height = ((gridHeight - 1) / 2 * cellDistance) + padding;

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