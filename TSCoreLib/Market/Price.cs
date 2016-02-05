using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib.Market
{
	public interface IPrice
	{
		DateTime Datetime { get; set; }
		double Open { get; set; }
		double High { get; set; }
		double Low { get; set; }
		double Close { get; set; }
	}

	public class Price : IPrice
	{
		public DateTime Datetime { get; set; }
		public double Low { get { return this.lhoc[0]; } set { this.lhoc[0] = value; } }
		public double High { get { return this.lhoc[1]; } set { this.lhoc[1] = value; } }
		public double Open { get { return this.lhoc[2]; } set { this.lhoc[2] = value; } }
		public double Close { get { return this.lhoc[3]; } set { this.lhoc[3] = value; } }

		double[] lhoc = new double[4]; // comply with Chart series candle stick format
		public double[] LHOC { get { return this.lhoc; } }

		public Price()
		{

		}

		//double spread;

		//public Price(DateTime dateTime, double open, double high, double low, double close)
		//{
		//    this.dateTime = dateTime;
		//    this.open = open;
		//    this.high = high;
		//    this.low = low;
		//    this.close = close;
		//}

		//public DateTime Time { get { return this.dateTime; } }
	}
}
