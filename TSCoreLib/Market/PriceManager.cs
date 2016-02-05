using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bloomberg;


namespace TSCoreLib.Market
{
	public class PriceManager
	{
		private Dictionary<string, List<Price>> pricelist_dic = new Dictionary<string, List<Price>>();
		
		private static class SingletonHolder
        {
            internal static readonly PriceManager Instance = new PriceManager();
 
            static SingletonHolder() { }
        }

		private PriceManager() { }

		public static PriceManager Instance { get { return SingletonHolder.Instance; } }

		//private void AddPriceList(string currencyPair, List<Price> pricelist)
		//{
		//    if (pricelist_dic.ContainsKey(currencyPair))
		//    {
		//        List<Price> opriceList = pricelist_dic[currencyPair];
		//        foreach (var price in pricelist)
		//        {
		//            Price targetPrice = (from p in opriceList
		//                                 where price.Datetime.Date == p.Datetime.Date
		//                                 select price).FirstOrDefault();
		//            return targetPrice;
		//        }
		//        this.ClearCurrencyPairPrice(currencyPair);
		//    }
		//    this.pricelist_dic.Add(currencyPair, pricelist);
		//}

		public void SetMarketPriceInterval(string currencyPair, DateTime startDate, DateTime endDate)
		{
			List<Price> listprice = this.GetBloombergPriceList(startDate, endDate, currencyPair);
			if (pricelist_dic.ContainsKey(currencyPair))
			{
				pricelist_dic.Remove(currencyPair);
			}
			pricelist_dic.Add(currencyPair, listprice);
		}

		public List<Price> GetPriceList(string currencyPair, DateTime startDate, DateTime endDate)
		{
			return this.GetBloombergPriceList(startDate, endDate, currencyPair);
		}

		public Price GetPrice(string currencyPair, DateTime datetime)
		{
			Price targetPrice;
			// find in private list first
			if (pricelist_dic.ContainsKey(currencyPair))
			{
				List<Price> targetPriceList 
					= pricelist_dic[currencyPair].Where(price => price.Datetime.Date <= datetime.Date
																&& price.Datetime.Date >= datetime.AddDays(-2).Date).ToList();
				if (targetPriceList.Count() == 0)
				{
					// if there are no data, get from bloomberg
					List<Price> listprice = this.GetBloombergPriceList(datetime, datetime, currencyPair);
					targetPrice = listprice[0];
				}
				else
				{
					targetPrice = targetPriceList.Last();
				}
			}
			else // find in bloomberg directly
			{
				List<Price> listprice = this.GetBloombergPriceList(datetime, datetime, currencyPair);
				targetPrice = listprice[0];
			}

			//if (targetPrice == null)
			//{
			//    string[] currencys = currencyPair.Split('/');
			//    string CommCurrency = currencys[0];
			//    string TermCurrency = currencys[1];
			//    string newUnderlying = TermCurrency + "/" + CommCurrency;

			//    targetPrice = this.queryPrice(newUnderlying, datetime);
			//    targetPrice.Close = 1 / targetPrice.Close;
			//    targetPrice.Open = 1 / targetPrice.Open;
			//    targetPrice.High = 1 / targetPrice.High;
			//    targetPrice.Low = 1 / targetPrice.Low;
			//}
			return targetPrice;
		}

		public double GetClosePrice(string currencyPair, DateTime datetime)
		{
			return this.GetPrice(currencyPair, datetime).Close;
		}

		public double GetOpenPrice(string currencyPair, DateTime datetime)
		{
			return this.GetPrice(currencyPair, datetime).Open;
		}

		public double GetHighPrice(string currencyPair, DateTime datetime)
		{
			return this.GetPrice(currencyPair, datetime).High;
		}

		public double GetLowPrice(string currencyPair, DateTime datetime)
		{
			return this.GetPrice(currencyPair, datetime).Low;
		}

		//public void ClearAllPrice()
		//{
		//    this.pricelist_dic.Clear();
		//}

		//public void ClearCurrencyPairPrice(string currencyPair)
		//{
		//    this.pricelist_dic.Remove(currencyPair);
		//}

		//private Price queryPrice(string currencyPair, DateTime datetime)
		//{
		//    Price targetPrice = (from price in pricelist_dic[currencyPair]
		//                         where price.Datetime <= datetime
		//                         orderby price.Datetime descending
		//                         select price).FirstOrDefault();
		//    return targetPrice;
		//}

		private List<Price> GetBloombergPriceList(DateTime startDate, DateTime endDate, string currencyPair)
		{
			string startDate_s = startDate.ToString("yyyyMMdd");
			string endDate_s = endDate.ToString("yyyyMMdd");
			string BloombergTicker = currencyPair.Replace("/","") + " Curncy"; 
			
			List<Bloomberg.Bloomberg.BloombergPrice> pricelist =
			Bloomberg.Bloomberg.GetHistoricalPrice(startDate_s, endDate_s, BloombergTicker, PriceFormat.OHLC);

			List<Price> listPrice = new List<Price>();

			foreach (var price in pricelist)
			{
				Price newprice = new Price();
				newprice.Datetime = price.Datetime;
				newprice.Close = price.Close;
				newprice.High = price.High;
				newprice.Low = price.Low;
				newprice.Open = price.Open;
				listPrice.Add(newprice);
			}

			return listPrice;
		}
	}
}
