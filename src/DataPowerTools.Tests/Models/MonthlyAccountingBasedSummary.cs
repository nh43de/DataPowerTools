using System;

namespace DataPowerTools.Tests.Models
{

    public class MonthlyAccountingBasedSummary
    {
        public string ClientId { get; set; } = "FP";// ClientId (Primary key)
        public DateTime RecordDate { get; set; } = new DateTime(2017, 6, 1); // RecordDate (Primary key)
        public decimal IrlcGainLoss { get; set; } = 206376.7782m; // IrlcGainLoss
        public decimal IrlcGainLossReversed { get; set; } = -133643.4679m; // IrlcGainLossReversed
        public decimal WarehouseGainLoss { get; set; } = 897302.5612m; // WarehouseGainLoss
        public decimal WarehouseGainLossReversed { get; set; } = -1008034.7845m; // WarehouseGainLossReversed
        public decimal TotalReverseEntryIncome { get; set; } = -37998.9130m; // TotalReverseEntryIncome
        public decimal DeliveredGainLoss { get; set; } = 150838.4909m; // DeliveredGainLoss
        public decimal MsrValue { get; set; } = 61364.4334m; // MsrValue
        public decimal OriginationFeeIncome { get; set; } = 0.0000m; // OriginationFeeIncome
        public decimal InterestIncome { get; set; } = 97556.0017m; // InterestIncome
        public decimal FundingExpense { get; set; } = -23334.9280m; // FundingExpense
        public decimal HedgeInstrumentGainLoss { get; set; } = -28184.7500m; // HedgeInstrumentGainLoss
        public decimal TotalRealizedIncome { get; set; } = 258239.2481m; // TotalRealizedIncome
        public decimal? TotalIncome { get; set; } = 220240.3351m; // TotalIncome
        public decimal SaleLoanBalance { get; set; } = 7867235.0600m; // SaleLoanBalance
        public decimal? TotalRealizedIncomeBasis { get; set; } = 0.0328m; // TotalRealizedIncomeBasis

        public MonthlyAccountingBasedSummary ShouldFail => throw new Exception("In EF when object context has been disposed and lazy loading is enabled, this will fail.");
    }
}