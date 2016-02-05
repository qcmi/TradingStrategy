using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TSCoreLib;

namespace TSCoreLib.Strategy
{
	public class StrategyFactory
	{	
		private static class SingletonHolder
        {
            internal static readonly StrategyFactory Instance = new StrategyFactory();
 
            static SingletonHolder() { }
        }

		private StrategyFactory() { }

		public static StrategyFactory Instance { get { return SingletonHolder.Instance; } }

		public Strategy MakeStrategy(string strategyName)
		{
			Strategy newStrategy;
			switch (strategyName)
			{
				case "SMA Strategy":
					newStrategy = new MAStrategy();
					break;
				default:
					newStrategy = new Strategy();
					break;
			}

			return newStrategy;
		}
	}
}
