using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;

namespace TSCoreLib
{
	public class Portfolio : ICloneable
	{
		Dictionary<string, Position> position_dic = new Dictionary<string, Position>();
		List<double> priceList = new List<double>();
		private string basecurrency;


		public Portfolio()
		{
			basecurrency = GlobalVariable.BaseCurrency;
		}

		public Portfolio(string currencyPair) : base()
		{
			Position newPosition = new Position(currencyPair);
			position_dic.Add(currencyPair, newPosition);
		}

		public Portfolio(List<string> currencyPair_list)
			: base()
		{
			foreach (var currencyPair in currencyPair_list)
			{
				Position newPosition = new Position(currencyPair);
				position_dic.Add(currencyPair, newPosition);
			}		
		}

		private void OpenPosition(string currencyPair, double amount, double price, PositionDirection pd)
		{
			if (position_dic.ContainsKey(currencyPair))
			{
				if (position_dic[currencyPair].Direction == PositionDirection.Long)
				{
					position_dic[currencyPair].LongNotional += amount;
					position_dic[currencyPair].NetNotional = position_dic[currencyPair].LongNotional;
					
				}
				else if (position_dic[currencyPair].Direction == PositionDirection.Short)
				{
					position_dic[currencyPair].ShortNotional += amount;
					position_dic[currencyPair].NetNotional = -position_dic[currencyPair].ShortNotional;
				}
				else
				{
					Position newPosition = new Position(pd, currencyPair, amount, price);
					position_dic[currencyPair] = newPosition;
				}
				position_dic[currencyPair].CostPrice = (position_dic[currencyPair].NetNotional
															* position_dic[currencyPair].CostPrice
															+ amount * price) / (position_dic[currencyPair].NetNotional
															+ amount);
				position_dic[currencyPair].CurrentPrice = price;
			}
			else
			{
				Position newPosition = new Position(pd, currencyPair, amount, price);
				position_dic.Add(currencyPair, newPosition);
			}
		}

		private void ClosePosition(string currencyPair, double amount, double price)
		{
			string[] currencies = currencyPair.Split('/');
			string CommCurrency = currencies[0];
			string TermCurrency = currencies[1];

			Position adjustPosition = position_dic[currencyPair];
			if (Math.Abs(adjustPosition.NetNotional) >= amount) // close position
			{
				adjustPosition.RealizedNetProfit = (price - adjustPosition.CostPrice) * amount
												* (int)adjustPosition.Direction;

				if (CommCurrency == basecurrency) // like USD/CNH
				{
					adjustPosition.RealizedNetProfit = adjustPosition.RealizedNetProfit / price;
				}

				if (adjustPosition.Direction == PositionDirection.Long)
				{
					adjustPosition.LongNotional = adjustPosition.LongNotional - amount;
					adjustPosition.NetNotional = adjustPosition.LongNotional;
				}
				else
				{
					adjustPosition.ShortNotional = adjustPosition.ShortNotional - amount;
					adjustPosition.NetNotional = -adjustPosition.ShortNotional;
				}
				adjustPosition.CurrentPrice = price;
			}
			else // close and open a oppsite position
			{
				adjustPosition.RealizedNetProfit = (price - adjustPosition.CostPrice) * adjustPosition.NetNotional;

				if (CommCurrency == basecurrency) // like USD/CNH
				{
					adjustPosition.RealizedNetProfit = adjustPosition.RealizedNetProfit / price;
				}

				if (adjustPosition.Direction == PositionDirection.Long)
				{
					adjustPosition.ShortNotional = amount - adjustPosition.LongNotional;
					adjustPosition.LongNotional = 0;
					adjustPosition.NetNotional = -adjustPosition.ShortNotional;
				}
				else
				{
					adjustPosition.LongNotional = amount - adjustPosition.ShortNotional;
					adjustPosition.ShortNotional = 0;
					adjustPosition.NetNotional = adjustPosition.LongNotional;
				}
				adjustPosition.CurrentPrice = price;
				adjustPosition.CostPrice = price;
			}
		}

		public void BuyCurrencyPair(string currencyPair, double amount, double price)
		{
			if (position_dic.ContainsKey(currencyPair))
			{
				if (position_dic[currencyPair].Direction == PositionDirection.Long
					|| position_dic[currencyPair].Direction == PositionDirection.None)
				{
					this.OpenPosition(currencyPair, amount, price, PositionDirection.Long);
				}
				else
				{
					this.ClosePosition(currencyPair, amount, price);
				}
			}
			else
			{
				this.OpenPosition(currencyPair, amount, price, PositionDirection.Long);
			}
		}

		public void SellCurrencyPair(string currencyPair, double amount, double price)
		{
			if (position_dic.ContainsKey(currencyPair))
			{
				if (position_dic[currencyPair].Direction == PositionDirection.Short
					|| position_dic[currencyPair].Direction == PositionDirection.None)
				{
					this.OpenPosition(currencyPair, amount, price, PositionDirection.Short);
				}
				else
				{
					this.ClosePosition(currencyPair, amount, price);
				}
			}
			else
			{
				this.OpenPosition(currencyPair, amount, price, PositionDirection.Short);
			}
		}

		public Position Position(string Currency)
		{
			if (!position_dic.ContainsKey(Currency))
			{
				return new Position(Currency);
			}
			else
			{
				return position_dic[Currency];
			}		
		}

		public double UnrealizedNetProfit(string currencyPair, double price)
		{
			double unrealizednp = 0;
			if (position_dic.ContainsKey(currencyPair))
			{
				position_dic[currencyPair].CurrentPrice = price;
				position_dic[currencyPair].UnrealizedNetProfit = this.CalculateMTM(currencyPair);
				unrealizednp = position_dic[currencyPair].UnrealizedNetProfit;
			}
				
			return unrealizednp;
		}

		private double CalculateMTM(string currencyPair)
		{
			double mtm = 0;
			Position myposition = position_dic[currencyPair];
			mtm = (myposition.CurrentPrice - myposition.CostPrice) * myposition.NetNotional;
			string[] currencies = currencyPair.Split('/');
			string CommCurrency = currencies[0];
			string TermCurrency = currencies[1];
			if (CommCurrency == "USD")
			{
				mtm = mtm / myposition.CurrentPrice;
			}
			return mtm;
		}

		public List<string> GetAllPositionCurrencyPair()
		{
			List<string> pair = new List<string>();
			foreach (var position in position_dic.Values)
			{
				pair.Add(position.CurrencyPair);
			}
			return pair;
		}

		public object Clone()
		{
			List<string> allccypair = this.GetAllPositionCurrencyPair();

			Portfolio newPortfolio = new Portfolio(allccypair);

			foreach (var ccyPair in allccypair)
			{
				Position position = newPortfolio.Position(ccyPair);
				Position oldposition = this.Position(ccyPair);

				position.CostPrice = oldposition.CostPrice;
				position.CurrencyPair = oldposition.CurrencyPair;
				position.CurrentPrice = oldposition.CurrentPrice;
				position.LongNotional = oldposition.LongNotional;
				position.NetNotional = oldposition.NetNotional;
				position.RealizedNetProfit = oldposition.RealizedNetProfit;
				position.ShortNotional = oldposition.ShortNotional;
				position.UnrealizedNetProfit = oldposition.UnrealizedNetProfit;

				if (oldposition.Direction == PositionDirection.Long)
					position.Direction = PositionDirection.Long;
				else if (oldposition.Direction == PositionDirection.Short)
					position.Direction = PositionDirection.Short;
				else
					position.Direction = PositionDirection.None;
			}

			return newPortfolio;
		}

		public Portfolio ClonePortfolio()
		{
			return (Portfolio)this.Clone();
		}
	}
}
