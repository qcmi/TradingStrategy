using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib
{
	public enum PositionDirection
	{
		Long = 1,
		Short = -1,
		None = 0
	}

	public class Position
	{
		public string CurrencyPair { get; set; }
		public double LongNotional { get; set; }
		public double ShortNotional { get; set; }
		public double NetNotional { get; set; }
		public double CostPrice { get; set; }
		public double CurrentPrice { get; set; }
		public double RealizedNetProfit { get; set; }
		public double UnrealizedNetProfit { get; set; }
		
		
		public PositionDirection Direction { get; set; }

		public Position(string currencyPair_)
		{
			this.Direction = PositionDirection.None;
			this.CurrencyPair = currencyPair_;
			this.LongNotional = this.NetNotional = this.ShortNotional = 0;
		}

		public Position(PositionDirection direction_ ,string currencyPair_, double notional_,  double price_)
		{
			this.CurrencyPair = currencyPair_;
			this.Direction = direction_;
			this.CostPrice = price_;
			this.RealizedNetProfit = 0;
			this.CurrentPrice = CostPrice;

			if (direction_ == PositionDirection.Long)
			{
				this.LongNotional = notional_;
				this.ShortNotional = 0;
				this.NetNotional = LongNotional;
			}
			else
			{
				this.LongNotional = 0;
				this.ShortNotional = notional_;
				this.NetNotional = ShortNotional;
			}

		}

		//public static Position operator +(Position position1, Position position2)
		//{
		//    Position newPosition = new Position();
		//    if (position1.CurrencyPair != position2.CurrencyPair)
		//    {
		//         System.ArgumentException argEx = new System.ArgumentException("Position currency pair are different, can't add.", "Position Error");
		//         throw argEx; 
		//    }
		//    else
		//    {
		//        newPosition.CurrencyPair = position1.CurrencyPair;
		//        if (position1.Direction != position2.Direction)
		//        {
		//            if (position1.Notional >= position2.Notional)
		//            {
		//                newPosition.Notional = position1.Notional - position2.Notional;
		//                newPosition.Direction = position1.Direction;
		//            }
		//            else
		//            {
		//                newPosition.Notional = position2.Notional - position1.Notional;
		//                newPosition.Direction = position2.Direction;
		//            }
		//        }
		//        else
		//        {
		//            newPosition.Direction = position1.Direction;
		//            newPosition.Notional = position1.Notional + position2.Notional;
		//        }
		//    }

		//    return newPosition;
		//}

		//public static Position operator -(Position position1, Position position2)
		//{
		//    Position newPosition = new Position(position1.Currency, position1.NetPosition - position2.NetPosition);

		//    return newPosition;
		//}
	}
}
