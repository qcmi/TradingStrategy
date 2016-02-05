using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib.Market;

namespace TSCoreLib.TechIndicator
{
	public class Indicator
	{
		public DateTime Date { get; set; }
		public double Value { get; set; }
	}

	public interface ITechIndicator
	{
		void CalculateIndicator();
	}

	public class TechIndicator : ITechIndicator 
	{
		public string Color { get; set; }
		protected string Label { get; set; }
		protected List<Price> price_list = new List<Price>();
		protected List<Indicator> indicator_list = new List<Indicator>();	
		public virtual void CalculateIndicator() {}

		public double GetValue(DateTime datetime)
		{
			double value = (from p in indicator_list
							where p.Date == datetime.Date
							select p.Value).First();

			return value;
		}

		public List<Indicator> GetIndicatorList()
		{
			return this.indicator_list;
		}
	}
}
