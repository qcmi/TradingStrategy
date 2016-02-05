using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;
using TSCoreLib.Strategy;

namespace TSCoreLib
{
	public enum ValidateMethod
	{
		BackTesting,
		Simulation
	}

	public class PortfolioManager
	{
		private Dictionary<string,List<Price>> price_dic = new Dictionary<string,List<Price>>();
		private List<string> currencyPair_list = new List<string>();

		private DateTime startDate_;
		private DateTime endDate_;

		private TSCoreLib.Strategy.Strategy myStrategy_;
		private ValidateMethod validMethod_;

		public PortfolioManager(DateTime startDate, DateTime endDate)
		{
			this.SetDateInterval(startDate, endDate);
		}

		public void SetDateInterval(DateTime startDate, DateTime endDate)
		{
			this.startDate_ = startDate.Date;
			this.endDate_ = endDate.Date;
		}

		public void SetValidateMethod(ValidateMethod myMethod)
		{
			validMethod_ = myMethod;
		}

		public void SetTradingStrategy(TSCoreLib.Strategy.Strategy myStrategy)
		{
			myStrategy_ = myStrategy;
			myStrategy_.pricelist_dic_ = this.price_dic;
			myStrategy_.baseDate_ = this.startDate_;
			myStrategy_.InitializedPortfolio(this.currencyPair_list);
		}

		public void SetCurrencyPair(string currencyPair)
		{
			if (!this.currencyPair_list.Contains(currencyPair))
			{
				this.currencyPair_list.Add(currencyPair);
			}
		}

		public void SetCurrencyPairList(List<string> currencyPair)
		{
			this.currencyPair_list = currencyPair;
		}

		public List<string> GetCurrencyPairList()
		{
			return this.currencyPair_list;
		}

		public void GetAllMarketData()
		{
			price_dic.Clear();
			foreach (var currencyPair in this.currencyPair_list)
			{
				List<Price> listprice = PriceManager.Instance.GetPriceList(currencyPair, this.startDate_, this.endDate_);
				price_dic.Add(currencyPair, listprice);
			}
		}

		public void RunStrategy()
		{
			if (this.validMethod_ != null && this.myStrategy_ != null)
			{
				this.myStrategy_.CheckParameters();
				this.myStrategy_.Run(this.validMethod_);
			}		
		}

		public PerformanceReport ShowPerformance()
		{
			PerformanceReport myPerformance = this.myStrategy_.ExportPerformaceReport();
			return myPerformance;
		}
	}
}
