using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;

namespace TSCoreLib.TechIndicator
{
	public class MovingAverage : TechIndicator
	{
		public int Interval { get; set; }
		private DateTime startDate_;
		private DateTime endDate_;
		private List<Price> enoughPrice_list;

		public MovingAverage(string currencyPair, List<Price> pricelist, int interval, string color)
		{
			this.Color = color;
			this.Label = currencyPair;
			this.Interval = interval;
			this.price_list = pricelist;
			this.setStartEndDate();
		}

		public override void CalculateIndicator()
		{
			if (this.indicator_list != null)
			{
				this.indicator_list.Clear();
			}		
			this.getEnoughPrice();
			List<double> averagePrice = new List<double>();

			int interval_ = Interval;
			for (int i = 0; i < interval_; i++)
			{
				Price price = (from p in enoughPrice_list
							   where p.Datetime == startDate_.AddDays(-(i + 1))
							   select p).FirstOrDefault();

				if (price == null)
				{
					interval_++;
				}
				else
				{
					averagePrice.Insert(0, price.Close); // 第一個是最舊的price
				}
			}

			for (int i = 0; i < price_list.Count; i++)
			{
				Indicator newIndicator = new Indicator();
				newIndicator.Date = price_list[i].Datetime;
				newIndicator.Value = averagePrice.Average();

				this.indicator_list.Add(newIndicator);
				averagePrice.RemoveAt(0); //移除最舊的
				averagePrice.Add(price_list[i].Close); //加上最新的
			}
		}

		private void getEnoughPrice() 
		{
			DateTime EarlystartDate_ = startDate_;
			int weeks = Interval / 5 + 1;
			EarlystartDate_ = EarlystartDate_.AddDays(-7 * weeks);

			enoughPrice_list = PriceManager.Instance.GetPriceList(this.Label, EarlystartDate_, this.endDate_);
		}

		private void setStartEndDate()
		{
			this.startDate_ = (from p in price_list
								  orderby p.Datetime ascending
								  select p.Datetime).First();
			this.endDate_ = (from p in price_list
								orderby p.Datetime descending
								select p.Datetime).First();

		}

	}
}
