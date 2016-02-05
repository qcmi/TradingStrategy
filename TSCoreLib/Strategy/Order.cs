using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib.Strategy
{
	public class Order
	{
		enum Type
		{
			Limit,
			Market,
			StopLoss
		}

		enum Duration
		{
			GTD, // good til date
			GTC, // good til cancel
			GAT // good after time/date
		}


	}
}
