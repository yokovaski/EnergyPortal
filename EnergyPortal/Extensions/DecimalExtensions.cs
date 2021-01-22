using System;

namespace EnergyPortal.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Round(this decimal value, int decimals)
        {
            return Math.Round(value, decimals);
        }
    }
}