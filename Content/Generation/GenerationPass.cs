using System.Collections.Generic;
using Terraria.WorldBuilding;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Terraria;
using ReLogic.Utilities;
using Microsoft.Xna.Framework;
using IAmLostInASea.Enums;

namespace IAmLostInASea.Content.Generation
{
    public class GenerationPass : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
            bool dungeonOceanCave = ModContent.GetInstance<AEWorldGenConfig>().GuaranteedOceanCave;
            int JungleOceanCave = (int)ModContent.GetInstance<AEWorldGenConfig>().JungleOceanStyle;

            if (JungleOceanCave == (int)JungleOceanStyleEnum.Random)
            {
                JungleOceanCave = WorldGen.genRand.NextBool() ? (int)JungleOceanStyleEnum.Vanilla : (int)JungleOceanStyleEnum.Custom;
            }

            //Vanilla ocean cave tweaking
            int oceanCaveIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Create Ocean Caves"));
            tasks[oceanCaveIndex] = new PassLegacy("Create Ocean Caves", (progress, config) =>
            {
                int chance = 3;
                int xRand;
                int posX;
                
                if (WorldGen.remixWorldGen)
                {
                    chance = 2;
                }
                
                for (int i = 0; i < 2; i++)
                {
                    bool check1 = (i != 0) || (GenVars.dungeonSide <= 0);
                    bool check2 = (i != 1) || (GenVars.dungeonSide >= 0);
                    bool check3 = WorldGen.genRand.NextBool(chance) || dungeonOceanCave || WorldGen.drunkWorldGen || WorldGen.tenthAnniversaryWorldGen;

                    if (check1 && check2 && check3)
                    {
                        progress.Message = Lang.gen[90].Value;
                        
                        xRand = WorldGen.genRand.Next(55, 95);
                        posX = (i == 1) ? Main.maxTilesX - xRand : xRand;
                        
                        int posY = 0;
                        while (!WorldGen.SolidTile(posX, posY))
                        {
                            posY++;
                        }
                        
                        WorldGen.oceanCave(posX, posY);
                    }

                    bool check4 = JungleOceanCave == (int)JungleOceanStyleEnum.Vanilla;

                    if (check1 && check2 && check4)
                    {
                        xRand = WorldGen.genRand.Next(55, 95);
                        posX = (i == 0) ? Main.maxTilesX - xRand : xRand;

                        int posY = 0;
                        while (!WorldGen.SolidTile(posX, posY))
                        {
                            posY++;
                        }
                        
                        WorldGen.oceanCave(posX, posY);
                    }
                }
            });

            //Re-locate the Aether to avoid the ocean caves
            int shimmerIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shimmer"));
            tasks[shimmerIndex] = new PassLegacy("Shimmer", (progress, config) =>
            {
                double mult = JungleOceanCave == (int)JungleOceanStyleEnum.Vanilla ? 0.45 : 0.65;

                int MinY = (int)(Main.maxTilesY * mult);
                int MaxY = (int)(Main.maxTilesY * 0.7);

                if (MaxY > Main.maxTilesY - 200)
                {
                    MaxY = Main.maxTilesY - 200;
                }
                if (MaxY <= MinY)
                {
                    MaxY = MinY + 50;
                }
                
                int xRand = WorldGen.genRand.Next(150, 250);
                int AetherX = GenVars.dungeonSide < 0 ? Main.maxTilesX - xRand : xRand;
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
                    xRand = WorldGen.genRand.Next(150, 250);
                    AetherX = GenVars.dungeonSide < 0 ? Main.maxTilesX - xRand : xRand;
                    AetherY = WorldGen.genRand.Next(MinY, MaxY);
                }
                
                GenVars.shimmerPosition = new Vector2D(AetherX, AetherY);
                
                //This protects the shimmer from other structures
                int ProtectionSize = 200;
                GenVars.structures.AddProtectedStructure(new Rectangle(AetherX - ProtectionSize / 2, AetherY - ProtectionSize / 2, ProtectionSize, ProtectionSize));
            });

            //Generate Ocean Depths
            int OceanBiomeIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
            if (OceanBiomeIndex != -1 && JungleOceanCave == (int)JungleOceanStyleEnum.Custom)
            {
                tasks.Insert(OceanBiomeIndex + 1, new PassLegacy("Oceanic Depths Generation", OceanicDepths.DepthsGen));
            }
		}
    }
}