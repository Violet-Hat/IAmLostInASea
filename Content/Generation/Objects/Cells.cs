namespace IAmLostInASea.Content.Generation.Objects
{
    public class Cell(int i, int j, int x, int y)
    {
        //Position in the 2D grid
        public int i = i;
        public int j = j;

        //Position ingame
        public int x = x;
        public int y = y;
        public bool visited = false;
    }
}