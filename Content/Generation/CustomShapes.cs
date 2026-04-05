using System;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;

namespace IAmLostInASea.Content.Generation
{
    public class ReverseMound : GenShape
    {
        private int halfWidth;
        private int depth;

        public ReverseMound(int halfWidth, int depth)
        {
            this.halfWidth = halfWidth;
            this.depth = depth;
        }

        public override bool Perform(Point origin, GenAction action)
        {
			double num = halfWidth;
			for (int i = -halfWidth; i <= halfWidth; i++)
			{
				int num2 = Math.Min(depth, (int)((0.0 - (depth + 1) / (num * num)) * (i + num) * (i - num)));
				for (int j = 0; j < num2; j++)
				{
					if (!UnitApply(action, origin, i + origin.X, origin.Y + j) && _quitOnFail)
					{
						return false;
					}
				}
			}
			return true;
        }
    }
}