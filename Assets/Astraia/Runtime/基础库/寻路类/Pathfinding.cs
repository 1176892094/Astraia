using System;

namespace Astraia
{
    [Serializable]
    public abstract class Pathfinding
    {
        protected const int INF = int.MaxValue;
        protected readonly int width;
        protected readonly int height;
        protected readonly int[] costs;

        protected Pathfinding(int width, int height, int[] costs)
        {
            this.width = width;
            this.height = height;
            this.costs = costs;
        }

        protected int Index(int x, int y)
        {
            return y * width + x;
        }

        protected bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        protected bool CanMove(int x, int y, Neighbors d)
        {
            var nx = x + d.x;
            var ny = y + d.y;

            if (!Contains(nx, ny))
            {
                return false;
            }

            if (costs[Index(nx, ny)] >= INF)
            {
                return false;
            }

            if (d.cost == 14)
            {
                if (costs[Index(x, ny)] >= INF || costs[Index(nx, y)] >= INF)
                {
                    return false;
                }
            }

            return true;
        }

        public void SetCost(int x, int y, int cost)
        {
            if (Contains(x, y))
            {
                costs[Index(x, y)] = Math.Max(1, cost);
            }
        }

        public void SetObstacle(int x, int y, bool walkable)
        {
            if (Contains(x, y))
            {
                costs[Index(x, y)] = walkable ? 1 : INF;
            }
        }
    }
}