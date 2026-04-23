using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;

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

			//Add the biome in the worldgen task
			int DepthsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Remove Broken Traps"));
			if (DepthsIndex != -1)
			{
				tasks.Insert(DepthsIndex + 1, new PassLegacy("Ocean Depths Generation", OceanDepths.DepthsGen));
			}
		}
    }
}