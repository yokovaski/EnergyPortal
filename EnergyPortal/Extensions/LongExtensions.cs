using System;

namespace EnergyPortal.Extensions
{
    public static class LongExtensions
    {
        public static decimal DivideByThousand(this long value) => decimal.Round(value / 1000.0M, 3);
    }
}