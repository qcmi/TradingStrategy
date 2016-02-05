using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib.Strategy
{
	public enum CrossSignal
	{
		UpToDown,
		DownToUp,
		None
	}

	public enum TradeSignal
	{
		Long,
		Short,
		TakeProfit,
		StopLoss,
		Exit,
		None
	}
}
