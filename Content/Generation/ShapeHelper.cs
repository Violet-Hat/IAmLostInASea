using Terraria.ID;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

namespace IAmLostInASea.Content.Generation
{
    public class ShapeHelper
    {
        public static void PlaceCircle(Point origin, int tileType, int wallType, int radius, double blotchChance = 0.4, bool clearTiles = true, bool clearWalls = true, bool replaceOnly = false)
        {
            //Tile placement
            ShapeData circle = new();
            GenAction blotches = new Modifiers.Blotches(2, blotchChance);

            WorldUtils.Gen(origin, new Shapes.Circle(radius), Actions.Chain(
            [
                blotches.Output(circle)
            ]));

            //Clear tiles
            if (clearTiles && !replaceOnly)
            {
                WorldUtils.Gen(origin, new ModShapes.All(circle), Actions.Chain(
                [
                    new Actions.ClearTile(), new Actions.SetLiquid(0, 0)
                ]));
            }

            //Place tiles
            if (tileType > -1)
            {
                if (replaceOnly)
                {
                    WorldUtils.Gen(origin, new ModShapes.All(circle), Actions.Chain(
                    [
                        new Modifiers.IsSolid(), new Actions.SetTile((ushort)tileType)
                    ]));
                }
                else
                {
                    WorldUtils.Gen(origin, new ModShapes.All(circle), Actions.Chain(
                    [
                        new Actions.PlaceTile((ushort)tileType)
                    ]));
                }
            }

            //Wall placement
            ShapeData wallCircle = new();
            GenAction wallBlotches = new Modifiers.Blotches(2, 0.4);

            WorldUtils.Gen(origin, new Shapes.Circle(radius - 1), Actions.Chain(
            [
                wallBlotches.Output(wallCircle)
            ]));

            //Clear walls
            if (clearWalls)
            {
                WorldUtils.Gen(origin, new ModShapes.All(wallCircle), Actions.Chain(
                [
                    new Actions.ClearWall()
                ]));
            }

            //Place walls
            if (wallType > WallID.None)
            {
                WorldUtils.Gen(origin, new ModShapes.All(wallCircle), Actions.Chain(
                [
                    new Actions.PlaceWall((ushort)wallType)
                ]));
            }
        }

        public static void PlaceLiquidInCircle(Point origin, int liquidType, int radius)
        {
            WorldUtils.Gen(origin, new Shapes.Circle(radius), Actions.Chain(
            [
                new Modifiers.IsNotSolid(), new Actions.SetLiquid(liquidType, byte.MaxValue)
            ]));
        }
    }
}