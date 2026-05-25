using Terraria.ModLoader.Config;
using System.ComponentModel;

using IAmLostInASea.Enums;

namespace IAmLostInASea
{
    [BackgroundColor(27, 30, 64, 200)]
    public class AEWorldGenConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [BackgroundColor(45, 68, 110, 200)]
        [DefaultValue(JungleOceanStyleEnum.Random)]
        [DrawTicks]
        public JungleOceanStyleEnum JungleOceanStyle;

        [BackgroundColor(45, 68, 110, 200)]
        [DefaultValue(true)]
        public bool GuaranteedOceanCave { get; set; }
    }
}