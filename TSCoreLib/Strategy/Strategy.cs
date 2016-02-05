using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;
using TSCoreLib.TechIndicator;

namespace TSCoreLib.Strategy
{
	public interface IStrategy
	{
		void Run(ValidateMethod myMethod);
		void CheckParameters();
		void SetTradingParametersImplements(StrategyView strategyView);
		Dictionary<string, TechIndicator.TechIndicator> GetIndicatorDicImplements();
		PerformanceReport ExportPerformaceReport();
	}


	public class Strategy : IStrategy
	{
		protected Dictionary<DateTime, Portfolio> portfolio_dic = new Dictionary<DateTime, Portfolio>();
		public Dictionary<string, List<Price>> pricelist_dic_ = new Dictionary<string, List<Price>>();
		protected List<string> strategy_list = new List<string>();

		//protected TradeSignal tradeSignal_;
		//protected CrossSignal crossSignal_;
		protected double MaxLongPosition;
		protected double MaxShortPosition;
		public DateTime baseDate_;
		protected ValidateMethod validateMethod_;
		protected PerformanceReport performanceReport_ = new PerformanceReport();
		public string StrategyName;

		public Strategy()
		{
			//tradeSignal_ = TradeSignal.None;
			//crossSignal_ = CrossSignal.None;
		}

		public virtual void Run(ValidateMethod myMethod)
		{
		}

		public PerformanceReport ExportPerformaceReport() 
		{
			this.CalculatePortfolioPerformance();
			return this.performanceReport_; 
		}

		protected void CalculatePortfolioPerformance()
		{
			PerformanceReport report = new PerformanceReport();
			foreach (var portfolio in portfolio_dic)
			{
				TradingPerformace performance = new TradingPerformace();
				performance.Date = portfolio.Key;
				performance.UnrealizedNetProfit = this.TotalUnrealizedNetProfit(portfolio.Key);
				performance.RealizedNetProfit = this.TotalRealizedNetProfit(portfolio.Key);
				performance.TotalNetProfit = performance.UnrealizedNetProfit + performance.RealizedNetProfit;

				report.Add(performance);
			}
			this.performanceReport_ = report;
		}

		public virtual void CheckParameters() { }

		protected void MoveNextDay()
		{
			if (this.portfolio_dic.Count == 0) // first date
			{
				this.portfolio_dic.Add(baseDate_, new Portfolio());
			}

			Portfolio newPortfolio = this.portfolio_dic[this.baseDate_].ClonePortfolio();
			this.baseDate_ = this.baseDate_.AddDays(1);
			if (this.baseDate_.DayOfWeek == DayOfWeek.Saturday)
			{
				this.baseDate_ = this.baseDate_.AddDays(2);
			}
			this.portfolio_dic.Add(this.baseDate_, newPortfolio);
		}

		protected void MoveToDate(DateTime date)
		{
			while (this.baseDate_ < date)
			{
				this.MoveNextDay();
			}
		}

		protected DateTime Today()
		{
			return this.baseDate_.Date;
		}

		protected double TotalNetProfit(DateTime endDate)
		{
			return this.TotalRealizedNetProfit(endDate) + this.TotalUnrealizedNetProfit(endDate);
		}

		protected double RealizedNetProfit(DateTime endDate, string currencyPair)
		{
			return portfolio_dic[endDate].Position(currencyPair).RealizedNetProfit;
		}

		protected double TotalRealizedNetProfit(DateTime endDate)
		{
			double totalRealizedPnL = 0;
			List<string> allcurrencypair = portfolio_dic[endDate].GetAllPositionCurrencyPair();
			foreach (var currencyPair in allcurrencypair)
			{
				totalRealizedPnL += RealizedNetProfit(endDate, currencyPair);
			}

			return totalRealizedPnL;
		}

		protected double UnrealizedNetProfit(DateTime date, string currencyPair)
		{
			double price = (from p in this.pricelist_dic_[currencyPair]
							where p.Datetime.Date == date.Date
							select p.Close).FirstOrDefault();
			if (price == null)
			{
				price = PriceManager.Instance.GetClosePrice(currencyPair, date); 
			}
			
			return portfolio_dic[date].UnrealizedNetProfit(currencyPair, price);
		}

		protected double TotalUnrealizedNetProfit(DateTime date)
		{
			double totalUnrealizedPnL = 0;
			List<string> allcurrencypair = portfolio_dic[date].GetAllPositionCurrencyPair();
			foreach (var currencyPair in allcurrencypair)
			{
				totalUnrealizedPnL += this.UnrealizedNetProfit(date, currencyPair);
			}

			return totalUnrealizedPnL;
		}

		protected void BuyCurrencyPair(DateTime date, string currencyPair, double amount, double price)
		{
			this.portfolio_dic[date.Date].BuyCurrencyPair(currencyPair, amount, price);
		}

		protected void SellCurrencyPair(DateTime date, string currencyPair, double amount, double price)
		{
			this.portfolio_dic[date.Date].SellCurrencyPair(currencyPair, amount, price);
		}

		public void InitializedPortfolio(List<string> currencyPair_list)
		{
			this.portfolio_dic.Add(this.baseDate_, new Portfolio(currencyPair_list));
		}

		protected void AddStrategy(string strategyName)
		{
			if (!this.strategy_list.Contains(strategyName))
			{
				this.strategy_list.Add(strategyName);
			}
		}

		public void SetPositionLimit(double maxLongPosition, double maxShortPosition)
		{
			this.MaxLongPosition = maxLongPosition;
			this.MaxShortPosition = maxShortPosition;
		}

		public void SetTradingParameters(StrategyView strategyView)
		{
			this.SetTradingParametersImplements(strategyView);
		}

		public virtual void SetTradingParametersImplements(StrategyView strategyView) { }

		public Dictionary<string, TechIndicator.TechIndicator> GetIndicatorDic()
		{
			return this.GetIndicatorDicImplements();
		}

		public virtual Dictionary<string, TechIndicator.TechIndicator> GetIndicatorDicImplements() 
		{
			return new Dictionary<string, TechIndicator.TechIndicator>();
		}
	}

	public class StrategyView
	{
		public string Indicator { get; set; }
		public string CurrencyPair { get; set; }
		public int Interval { get; set; }
		public CrossSignal Signal { get; set; }
		public TradeSignal Trading { get; set; }
		public double Notional { get; set; }
		public string Color { get; set; }
	}
}
