// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:33
// # Recently: 2026-08-13 22:26:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia
{
    public static class AsyncExtensions
    {
        public static Watch Render(this Watch item)
        {
            return TimeManager.Render(item);
        }

        public static Watch Physic(this Watch item)
        {
            return TimeManager.Physic(item);
        }

        public static Watch Accept(this Watch item)
        {
            return TimeManager.Accept(item);
        }

        public static Tween Render(this Tween item)
        {
            return TimeManager.Render(item);
        }

        public static Tween Physic(this Tween item)
        {
            return TimeManager.Physic(item);
        }

        public static Tween Accept(this Tween item)
        {
            return TimeManager.Accept(item);
        }
    }
}