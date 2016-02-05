using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;
using TSCoreLib.TechIndicator;

namespace TSCoreLib.Strategy
{
	public class MAStrategy : Strategy
	{
		private Dictionary<string, MovingAverage> movingAverage_dic = new Dictionary<string, MovingAverage>();
		private Dictionary<string, CrossSignal> crossSignal_dic = new Dictionary<string, CrossSignal>();
		private Dictionary<string, TradeSignal> tradingSignal_dic = new Dictionary<string, TradeSignal>();
		private Dictionary<string, double> notional_dic = new Dictionary<string, double>();
		private Dictionary<string, string> currencyPair_dic = new Dictionary<string, string>();
		private Dictionary<string, int> interval_dic = new Dictionary<string, int>();
		
		//private List<string> currencyPair_list = new List<string>();


		public MAStrategy() : base()
		{ 
			this.StrategyName = "SMA Strategy"; 
		}

		private void SetMovingAverageInterval(string label, string currencyPair, int interval, string color)
		{
			if (pricelist_dic_.ContainsKey(currencyPair))
			{
				if (!movingAverage_dic.ContainsKey(label))
				{
					MovingAverage sma = new MovingAverage(currencyPair, this.pricelist_dic_[currencyPair], interval, color);
					sma.CalculateIndicator();

					string key = currencyPair + interval.ToString();
					if (!movingAverage_dic.ContainsKey(key))
					{
						this.movingAverage_dic.Add(key, sma);
					}
					
					this.currencyPair_dic.Add(label, currencyPair);
					this.interval_dic.Add(label, interval);
				}			
			}			
		}

		public MovingAverage GetMovingAverage(string currencyPair, int interval)
		{
			return this.movingAverage_dic[currencyPair + interval.ToString()];
		}

		private void SetTradingSignal(string label, string currencyPair, TradeSignal LongOrShort)
		{
			if (!tradingSignal_dic.ContainsKey(label))
			{
				this.tradingSignal_dic.Add(label, LongOrShort);
			}
		}

		private void SetCrossSignal(string label, string currencyPair, CrossSignal crossSignal)
		{
			if (!crossSignal_dic.ContainsKey(label))
			{
				this.crossSignal_dic.Add(label, crossSignal);
			}
		}

		private void SetNotional(string label, string currencyPair, double Notional)
		{
			if (!notional_dic.ContainsKey(label))
			{
				this.notional_dic.Add(label, Notional);
			}
		}

		public override void SetTradingParametersImplements(StrategyView strategyView)
		{
			string label = strategyView.CurrencyPair + strategyView.Interval.ToString() 
							+ strategyView.Trading.ToString() + strategyView.Signal.ToString() 
							+ strategyView.Notional.ToString();

			this.AddStrategy(label);
			this.SetMovingAverageInterval(label, strategyView.CurrencyPair, strategyView.Interval, strategyView.Color);
			this.SetTradingSignal(label, strategyView.CurrencyPair, strategyView.Trading);
			this.SetCrossSignal(label, strategyView.CurrencyPair, strategyView.Signal);
			this.SetNotional(label, strategyView.CurrencyPair, strategyView.Notional);	
		}

		public override Dictionary<string, TechIndicator.TechIndicator> GetIndicatorDicImplements()
		{
			Dictionary<string, TechIndicator.TechIndicator> indicator_dic = new Dictionary<string, TechIndicator.TechIndicator>();

			foreach (var techIndicator in this.movingAverage_dic)
			{
				indicator_dic.Add(techIndicator.Key, (TechIndicator.TechIndicator)techIndicator.Value);
			}
			return indicator_dic;
		}

		public override void CheckParameters()
		{

		}

		public override void Run(ValidateMethod myMethod)
		{
			if (myMethod == ValidateMethod.BackTesting)
			{
				this.RunBackTesting();
			}
			else
			{
				
			}
		}

		private void RunBackTesting()
		{
			DateTime today;
			double todayPrice, yesterdayPrice;
			double todayValue, yesterdayValue;
			Dictionary<string, double> yesterdayPrice_dic = new Dictionary<string, double>();
			Dictionary<string, double> yesterdayValue_dic = new Dictionary<string, double>();

			foreach (var currencyPair in pricelist_dic_)
			{
				yesterdayPrice_dic.Add(currencyPair.Key, currencyPair.Value[0].Close);
			}

			DateTime LastDate = pricelist_dic_.Last().Value.Last().Datetime.Date;
			todayValue = 0;
			yesterdayValue = 0;

			for (int i = 0; i < pricelist_dic_.First().Value.Count; i++)
			{
				// start each day
				today = pricelist_dic_.First().Value[i].Datetime;

				foreach (var strategy in strategy_list)
				{
					string label = strategy;
					string currencyPair = currencyPair_dic[label];
					int interval = interval_dic[label];
					string smaName = currencyPair + interval.ToString();
					todayValue = this.movingAverage_dic[smaName].GetValue(today);

					if (!yesterdayValue_dic.ContainsKey(smaName))
						yesterdayValue_dic.Add(smaName, todayValue);

					yesterdayValue = yesterdayValue_dic[smaName];
					
					todayPrice = pricelist_dic_[currencyPair][i].Close;
					yesterdayPrice = yesterdayPrice_dic[currencyPair];

					CrossSignal crossSignal = this.checkCrossSignal(todayValue, yesterdayValue, todayPrice, yesterdayPrice);
					TradeSignal tradeSignal = this.checkTradeSignal(label, today, crossSignal, todayPrice);

					if (tradeSignal == TradeSignal.Long)
					{
						double longNotional = notional_dic[label];

						//用今天收盤價買
						this.portfolio_dic[today].BuyCurrencyPair(currencyPair, longNotional, todayPrice);
					}
					else if (tradeSignal == TradeSignal.Short)
					{
						double shortNotional = notional_dic[label];
						this.portfolio_dic[today].SellCurrencyPair(currencyPair, shortNotional, todayPrice);
					}
					else if (tradeSignal == TradeSignal.TakeProfit || tradeSignal == TradeSignal.StopLoss)
					{
						double OffsetNotional = notional_dic[label];
						double NetNotional = this.portfolio_dic[today].Position(currencyPair).NetNotional;
						if (NetNotional > 0)
						{
							this.portfolio_dic[today].SellCurrencyPair(currencyPair, OffsetNotional, todayPrice);
						}
						else
						{
							this.portfolio_dic[today].BuyCurrencyPair(currencyPair, OffsetNotional, todayPrice);
						}
					}
					else // none
					{

					}
				}

				if (today != LastDate)
				{
					foreach (var SMA in movingAverage_dic)
					{
						string smaName = SMA.Key;
						double value = SMA.Value.GetValue(today);
						yesterdayValue_dic[smaName] = value;
					}

					foreach (var currencyPair in currencyPair_dic)
					{
						double price = pricelist_dic_[currencyPair.Value][i].Close;
						yesterdayPrice_dic[currencyPair.Value] = price;
					}
					this.MoveNextDay();
				}
			}	
		}

		private TradeSignal checkTradeSignal(string label, DateTime today, CrossSignal crossSignal, double price)
		{
			CrossSignal targetSignal = crossSignal_dic[label];
			double position = notional_dic[label];
			string currencyPair = currencyPair_dic[label];

			if (targetSignal == crossSignal)
			{
				TradeSignal tradeSignal = tradingSignal_dic[label];
				if (tradeSignal == TradeSignal.Long)
				{
					double nowlongNotional = this.portfolio_dic[today].Position(currencyPair).LongNotional;
					if (nowlongNotional < MaxLongPosition)
					{
						return TradeSignal.Long;
					}
					else
					{
						return TradeSignal.None;
					}
				}
				else if (tradeSignal == TradeSignal.Short)
				{
					double nowshortNotional = this.portfolio_dic[today].Position(currencyPair).LongNotional;
					if (nowshortNotional < MaxShortPosition)
					{
						return TradeSignal.Short;
					}
					else
					{
						return TradeSignal.None;
					}
				}
				else if (tradeSignal == TradeSignal.Exit)
				{
					double nowNetNotional = this.portfolio_dic[today].Position(currencyPair).NetNotional;
					double costPrice = this.portfolio_dic[today].Position(currencyPair).CostPrice;
					if (nowNetNotional > 0)
					{
						if (costPrice > price) // long will loss
						{
							return TradeSignal.StopLoss;
						}
						else
						{
							return TradeSignal.TakeProfit;
						}
					}
					else if (nowNetNotional < 0)
					{
						if (costPrice < price)
						{
							return TradeSignal.StopLoss;
						}
						else
						{
							return TradeSignal.TakeProfit;
						}
					}
					else
					{
						return TradeSignal.None;
					}
				}
				else
				{
					return TradeSignal.None;
				}
			}
			else
			{
				return TradeSignal.None;
			}
		}

		private CrossSignal checkCrossSignal(double todayValue,double yesterdayValue, double todayprice, double yesterdayprice)
		{
			if (yesterdayprice >= yesterdayValue) // going down
			{
				if (todayprice < todayValue)
				{
					return CrossSignal.UpToDown;
				}
				else
				{
					return CrossSignal.None;
				}
			}
			else if (yesterdayprice < yesterdayValue) // going up
			{
				if (todayprice > todayValue)
				{
					return CrossSignal.DownToUp;
				}
				else
				{
					return CrossSignal.None;
				}
			}
			else
			{
				return CrossSignal.None;
			}
		}
	}
}
