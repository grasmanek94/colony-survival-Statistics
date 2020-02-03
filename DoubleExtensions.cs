using System;

namespace grasmanek94.Statistics
{
    public static class DoubleExtensions
    {
        const double _3 = 0.001;
        const double _4 = 0.0001;
        const double _5 = 0.00001;
        const double _6 = 0.000001;
        const double _7 = 0.0000001;

        public static bool E3DP(this double left, double right)
        {
            return Math.Abs(left - right) < _3;
        }

        public static bool E4DP(this double left, double right)
        {
            return Math.Abs(left - right) < _4;
        }

        public static bool E5DP(this double left, double right)
        {
            return Math.Abs(left - right) < _5;
        }

        public static bool E6DP(this double left, double right)
        {
            return Math.Abs(left - right) < _6;
        }

        public static bool E7DP(this double left, double right)
        {
            return Math.Abs(left - right) < _7;
        }
    }
}
