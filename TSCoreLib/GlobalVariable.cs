using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSCoreLib
{
	public static class GlobalVariable
	{
		static string basecurrency_;

		public static string BaseCurrency
		{
			get
			{
				return basecurrency_;
			}
			set
			{
				basecurrency_ = value;
			}		
		}
	}
}
