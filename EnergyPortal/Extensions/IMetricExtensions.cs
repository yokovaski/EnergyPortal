using System;
using DatabaseInterface.Entities;
using EnergyPortal.Models;

namespace EnergyPortal.Extensions
{
    public static class IMetricExtensions
    {
        public static OutMetricModel ToOutMetricModel(this IMetric current, IMetric next, Settings settings)
        {
            var highUsageDelta = next.UsageTotalHigh - current.UsageTotalHigh;
            var lowUsageDelta = next.UsageTotalLow - current.UsageTotalLow;
            var highRedeliveryDelta = next.RedeliveryTotalHigh - current.RedeliveryTotalHigh;
            var lowRedeliveryDelta = next.RedeliveryTotalLow - current.RedeliveryTotalLow;
            var gasDelta = next.UsageGasTotal - current.UsageGasTotal;

            var highUsagePrice = settings.HighUsagePricePerKwh;
            var lowUsagePrice = settings.LowUsagePricePerKwh;
            var highRedeliveryPrice = settings.HighRedeliveryPricePerKwh;
            var lowRedeliveryPrice = settings.LowRedeliveryPricePerKwh;
            var gasPrice = settings.GasPrice;

            var highUsageCost = highUsagePrice * highUsageDelta.DivideByThousand();
            var lowUsageCost = lowUsagePrice * lowUsageDelta.DivideByThousand();
            var highRedeliveryCost = highRedeliveryPrice * highRedeliveryDelta.DivideByThousand();
            var lowRedeliveryCost = lowRedeliveryPrice * lowRedeliveryDelta.DivideByThousand();

            var intake = next.UsageTotalHigh - current.UsageTotalHigh + next.UsageTotalLow - current.UsageTotalLow;
            var redelivery = next.RedeliveryTotalHigh - current.RedeliveryTotalHigh + next.RedeliveryTotalLow - current.RedeliveryTotalLow;
            var solar = next.SolarTotal - current.SolarTotal;

            var firstOfMonthDelta = new DateTime(current.Created.Year, current.Created.Month, 1);
            var monthDelta = firstOfMonthDelta.AddMonths(1) - firstOfMonthDelta;
            var fractionOfMonth = 0M;

            if (monthDelta.TotalMilliseconds > 0)
            {
                fractionOfMonth = Convert.ToDecimal((next.Created - current.Created).TotalMilliseconds) / Convert.ToDecimal(monthDelta.TotalMilliseconds);
            }

            var electricityFee = settings.ElectricityDeliveryPricePerMonth * fractionOfMonth;
            var gasFee = settings.GasDeliveryPricePerMonth * fractionOfMonth;
            
            var intakeCost = (highUsageCost + lowUsageCost + electricityFee).Round(2);
            var redeliveryCost = (highRedeliveryCost + lowRedeliveryCost).Round(2);
            var gasCost = ((gasPrice * gasDelta.DivideByThousand()) + gasFee).Round(2);
            
            var solarUsage = solar - redelivery;
            
            // solar value can't be trusted
            var usage = solarUsage > 0 ? intake + solarUsage : intake;

            return new OutMetricModel
            {
                DateTime = current.Created,
                Usage = usage,
                Intake = intake,
                Redelivery = redelivery,
                Solar = solar,
                Gas = next.UsageGasTotal - current.UsageGasTotal,
                UsageCost = intakeCost - redeliveryCost,
                IntakeCost = intakeCost,
                RedeliveryCost = -redeliveryCost,
                GasCost = gasCost
            };
        }
    }
}