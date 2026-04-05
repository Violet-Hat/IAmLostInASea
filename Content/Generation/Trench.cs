using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using ABMod.Generation;
using ReLogic.Utilities;

namespace IAmLostInASea.Content.Generation
{
    public class Trench : ModSystem
    {
        public static int PlaceTrenchX;
        public static int PlaceTrenchY;
        public static int TrenchDepthLimit;
        public static int MaxWidth;
        //public static int MinWidth;

        private void TrenchGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Into the abyss you go";

            MaxWidth = Main.maxTilesX >= 8400 ? 44 : Main.maxTilesX >= 6400 ? 40 : 36;

            //Get the X spawn point of the trench
            if (GenVars.dungeonSide == -1)
            {
                PlaceTrenchX = (int)(Main.maxTilesX - 75 - (MaxWidth * 1.25f));
            }
            else
            {
                PlaceTrenchX = (int)(75 + (MaxWidth * 1.25f));
            }

            //Get the Y spawn point of the trench
            bool FoundSurface = false;
            int attempts = 0;

            while (!FoundSurface && attempts++ < 100000)
            {
				int Y = 10;

				while (!WorldGen.SolidTile(PlaceTrenchX, Y) && Y <= Main.worldSurface)
				{
					Y++;
				}
				if (WorldGen.SolidTile(PlaceTrenchX, Y))
				{
					FoundSurface = true;
                    PlaceTrenchY = Y - 25;
				}
			}

            //Origin point of the trench
            Point origin = new Point(PlaceTrenchX, PlaceTrenchY);

            //How deep it should be
            int depthLimit = Main.maxTilesY - 500;
            TrenchDepthLimit = Math.Abs(PlaceTrenchY - depthLimit);

            //Place a solid base of hardened sand and regular sand for the trench
            //Make the regular sand base smaller
            Point baseOrigin = new Point(PlaceTrenchX, PlaceTrenchY + 40);
            int BaseDepthLimit = TrenchDepthLimit + 40;
            int baseWidth = (int)(MaxWidth * 1.5f);

            WorldUtils.Gen(baseOrigin, new ReverseMound(baseWidth, BaseDepthLimit), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearWall(), //Hage walls
                new Actions.SetTile(TileID.HardenedSand),
			}));

            BaseDepthLimit = TrenchDepthLimit + 20;
            baseWidth = (int)(MaxWidth * 1.25f);

            WorldUtils.Gen(baseOrigin, new ReverseMound(baseWidth, BaseDepthLimit), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.SetTile(TileID.Sand),
			}));

            //Clear the tiles, set the water
            WorldUtils.Gen(origin, new ReverseMound(MaxWidth, TrenchDepthLimit), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearTile(),
                new Actions.SetLiquid(),
			}));

            WorldGen.PlaceTile(PlaceTrenchX, PlaceTrenchY, TileID.EmeraldGemspark, forced: true);

            /*
            Vector2D offSet = new Vector2D(0, TrenchDepthLimit);

            //Place a solid base of hardened sand and regular sand for the trench
            //Make the regular sand base smaller
            Point baseOrigin = new Point(PlaceTrenchX, PlaceTrenchY + 40);
            Vector2D baseOffSet = new Vector2D(0, TrenchDepthLimit + 50);
            int baseWidth = (int)(MaxWidth * 1.25f);

            WorldUtils.Gen(baseOrigin, new Shapes.Tail(baseWidth, baseOffSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearWall(), //Hage walls
                new Actions.SetTile(TileID.HardenedSand),
			}));

            baseOffSet = new Vector2D(0, TrenchDepthLimit + 25);
            baseWidth = (int)(MaxWidth * 1.10f);

            WorldUtils.Gen(baseOrigin, new Shapes.Tail(baseWidth, baseOffSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.SetTile(TileID.Sand),
			}));

            //Clear the tiles, set the water
            WorldUtils.Gen(origin, new Shapes.Tail(MaxWidth, offSet), Actions.Chain(new GenAction[]
			{
                new Modifiers.Blotches(2, 0.4),
                new Actions.ClearTile(),
                new Actions.SetLiquid(),
			}));

            WorldGen.PlaceTile(PlaceTrenchX, PlaceTrenchY, TileID.EmeraldGemspark, forced: true);
            */
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//Add the biome in the worldgen task
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 1, new PassLegacy("Trench Generation", TrenchGen));
			}
		}
    }
}