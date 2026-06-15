using Terraria;
using Terraria.ID;

namespace IAmLostInASea.Content.Generation.Objects
{
    public class Cell(int x, int y, int size)
    {
        public readonly int x = x;
        public readonly int y = y;
        public readonly int size = (size - 1) / 2;

        public void Place()
        {
            for (int i = x - size; i <= x + size; i++)
            {
                for (int j = y - size; j <= y + size; j++)
                {
                    if (i != x || j != y)
                    {
                        WorldGen.KillTile(i, j, noItem: true);
                        WorldGen.KillWall(i, j);

                        WorldGen.PlaceTile(i, j, TileID.EmeraldGemspark);
                    }
                }
            }

            WorldGen.KillTile(x, y, noItem: true);
            WorldGen.KillWall(x, y);

            WorldGen.PlaceTile(x, y, TileID.RubyGemspark);
        }
    }
}