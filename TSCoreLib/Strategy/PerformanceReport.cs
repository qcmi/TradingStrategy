using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib.Strategy
{
	public class TradingPerformace
	{
		public DateTime Date { get; set; }
		public double UnrealizedNetProfit { get; set; }
		public double RealizedNetProfit { get; set; }
		public double TotalNetProfit { get; set; }
		public double RefSpot { get; set; }
	}

	public class PerformanceReport
	{
		public Dictionary<DateTime, TradingPerformace> performance_dic = new Dictionary<DateTime,TradingPerformace>();
		
		public double TotalNetProfit { get; set; }
		public double ProfitFactor { get; set; }
		public double AverageTradeNetProfit { get; set; }
		public double TharpExpectancy { get; set; }
		public double SlippageAndCommission { get; set; }
		public double MaxDrawDown { get; set; }
		public int NumOfDealsDone { get; set; }

		public void Add(TradingPerformace tradingPerformance)
		{
			this.performance_dic.Add(tradingPerformance.Date, tradingPerformance);
		}

		public TradingPerformace GetPerfomanceReport(DateTime date)
		{
			return this.performance_dic[date];
		}

		public Dictionary<DateTime, TradingPerformace> GetPerformanceReportDictionary()
		{
			return this.performance_dic;
		}
	}
}
