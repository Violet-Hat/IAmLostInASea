using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace IAmLostInASea
{
    [BackgroundColor(27, 30, 64, 200)]
    public class AEWorldGenConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(45, 68, 110, 200)]
        [DefaultValue(true)]
        public bool GenerateOceanDepths { get; set; }

        [BackgroundColor(45, 68, 110, 200)]
        [DefaultValue(true)]
        public bool GenerateOceanicTrench { get; set; }
    }
}