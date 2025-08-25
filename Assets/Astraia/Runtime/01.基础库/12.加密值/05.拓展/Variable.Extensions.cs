// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 18:04:15
// // # Recently: 2025-04-09 18:04:15
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Runtime.CompilerServices;
using Astraia.Common;

namespace Astraia
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBit(this Safe.Int data, int shift, int mask)
        {
            return (data >> shift) & (1 << mask) - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Safe.Int SetBit(this Safe.Int data, int shift, int mask, int value)
        {
            return (data & ~((1 << mask) - 1 << shift)) | ((value & (1 << mask) - 1) << shift);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBit(this Safe.Long data, int shift, int mask)
        {
            return (int)((data >> shift) & (1L << mask) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Safe.Long SetBit(this Safe.Long data, int shift, int mask, int value)
        {
            return (data & ~((1L << mask) - 1 << shift)) | ((value & (1L << mask) - 1) << shift);
        }
    }
}