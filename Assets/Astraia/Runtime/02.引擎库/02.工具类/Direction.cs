using UnityEngine;

namespace Astraia
{
    public static class Direction
    {
        private static readonly Vector2Int[] D32 = new Vector2Int[4];
        private static readonly Vector2Int[] D64 = new Vector2Int[8];

        static Direction()
        {
            D32[(int)Input.L] = new Vector2Int(+1, 0);
            D32[(int)Input.R] = new Vector2Int(-1, 0);
            D32[(int)Input.U] = new Vector2Int(0, +1);
            D32[(int)Input.D] = new Vector2Int(0, -1);
            D64[(int)Input.L] = new Vector2Int(+1, 0);
            D64[(int)Input.R] = new Vector2Int(-1, 0);
            D64[(int)Input.U] = new Vector2Int(0, +1);
            D64[(int)Input.D] = new Vector2Int(0, -1);
            D64[(int)Input.LU] = new Vector2Int(-1, +1);
            D64[(int)Input.LD] = new Vector2Int(-1, -1);
            D64[(int)Input.RU] = new Vector2Int(+1, +1);
            D64[(int)Input.RD] = new Vector2Int(+1, -1);
        }

        public static Vector2Int[] GetD4()
        {
            return D32;
        }

        public static Vector2Int[] GetD8()
        {
            return D64;
        }

        public static Vector2Int Get(Input input)
        {
            return D64[(int)input];
        }

        public enum Input
        {
            L,
            R,
            U,
            D,
            LU,
            LD,
            RU,
            RD
        }
    }
}