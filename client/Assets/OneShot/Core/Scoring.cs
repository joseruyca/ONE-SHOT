using System;
namespace OneShot.Core
{
    // Client prediction only; server result remains authoritative.
    public static class Scoring
    {
        public static int Precision(int actual, int target, int tolerance)
        {
            if (actual < 0 || actual > 10000 || target < 0 || target > 10000 || tolerance < 1 || tolerance > 10000)
                throw new ArgumentOutOfRangeException();
            int error = Math.Min(Math.Abs(actual - target), tolerance);
            return (int)(((long)(tolerance - error) * 10000 + tolerance / 2) / tolerance);
        }
    }
}
