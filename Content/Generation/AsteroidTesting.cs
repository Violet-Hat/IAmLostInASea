using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Microsoft.Xna.Framework;

namespace IAmLostInASea.Content.Generation
{
    public class AsteroidTesting : ModSystem
    {
        private void AsteroidGen(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Sans";

            int placeX = Main.maxTilesX / 2;
            int placeY = (int)(Main.maxTilesY * 0.10f);

            int smallSize1 = 5;
            int smallSize2 = 8;

            int mediumSize1 = 15;
            int mediumSize2 = 20;

            int largeSize1 = 30;
            int largeSize2 = 40;

            //Small
            Point origin = new Point(placeX - mediumSize2 - smallSize2 - 5, placeY);
            WorldUtils.Gen(origin, new Shapes.Circle(smallSize2), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.RubyGemspark),
            }));
            WorldUtils.Gen(origin, new Shapes.Circle(smallSize1), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.EmeraldGemspark),
            }));

            //Medium
            origin = new Point(placeX, placeY);
            WorldUtils.Gen(origin, new Shapes.Circle(mediumSize2), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.RubyGemspark),
            }));
            WorldUtils.Gen(origin, new Shapes.Circle(mediumSize1), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.EmeraldGemspark),
            }));

            //Large
            origin = new Point(placeX + mediumSize2 + largeSize2 + 5, placeY);
            WorldUtils.Gen(origin, new Shapes.Circle(largeSize2), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.RubyGemspark),
            }));
            WorldUtils.Gen(origin, new Shapes.Circle(largeSize1), Actions.Chain(new GenAction[]
            {
                new Actions.SetTile(TileID.EmeraldGemspark),
            }));
        }

        /*
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//Add the biome in the worldgen task
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 2, new PassLegacy("Asteroid Gen", AsteroidGen));
			}
		}
        */
    }
}