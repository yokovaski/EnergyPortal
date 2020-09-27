using System;

namespace EnergyPortal.Extensions
{
    public static class LongExtensions
    {
        public static double DivideByThousand(this long value) => Math.Round(value / 1000.0, 3);
    }
}