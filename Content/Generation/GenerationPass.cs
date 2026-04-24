using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Terraria;
using ReLogic.Utilities;
using Microsoft.Xna.Framework;

namespace IAmLostInASea.Content.Generation
{
    public class GenerationPass : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
            /*
			//Add the biome in the worldgen task
			int TrenchIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			if (TrenchIndex != -1)
			{
				tasks.Insert(TrenchIndex + 1, new PassLegacy("Trench Generation", TrenchTrenchGen));
			}
            */

			//Re-locate the Aether to avoid the Ancient Swamps
            int shimmerIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shimmer"));
            tasks[shimmerIndex] = new PassLegacy("Shimmer", (progress, config) =>
            {
                int MinY = (int)(Main.maxTilesY * 0.65);
                int MaxY = (int)(Main.maxTilesY * 0.75);

                if (MaxY > Main.maxTilesY - 200)
                {
                    MaxY = Main.maxTilesY - 200;
                }
                if (MaxY <= MinY)
                {
                    MaxY = MinY + 50;
                }

                int AetherX = GenVars.dungeonSide < 0 ? Main.maxTilesX - 150 : 150;
                int AetherY = WorldGen.genRand.Next(MinY, MaxY);
                
                //anniversary lobotomy
				int AnniversaryMinY = MinY;
				int AnniversaryMaxY = MaxY;

                if (AnniversaryMaxY <= AnniversaryMinY)
                {
                    AnniversaryMaxY = AnniversaryMinY + 50;
                }
				
                if (WorldGen.tenthAnniversaryWorldGen)
                {
                    AetherY = WorldGen.genRand.Next(AnniversaryMinY, AnniversaryMaxY);
                }
                
                //Fail-safe
                while (!WorldGen.ShimmerMakeBiome(AetherX, AetherY))
                {
                    AetherX = (GenVars.dungeonSide < 0) ? (int)(Main.maxTilesX * 0.95f) : (int)(Main.maxTilesX * 0.05f);
                    AetherY = WorldGen.genRand.Next(MinY, MaxY);
                }

                GenVars.shimmerPosition = new Vector2D(AetherX, AetherY);

                //This protects the shimmer from other structures
                int ProtectionSize = 200;
                GenVars.structures.AddProtectedStructure(new Rectangle(AetherX - ProtectionSize / 2, AetherY - ProtectionSize / 2, ProtectionSize, ProtectionSize));
            });

			int DepthsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			if (DepthsIndex != -1)
			{
				tasks.Insert(DepthsIndex + 1, new PassLegacy("Ocean Depths Generation", OceanDepths.DepthsGen));
			}
		}
    }
}