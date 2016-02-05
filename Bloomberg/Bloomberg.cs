using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using BloombergLib.BloombergLib;
using System.Net;

namespace Bloomberg
{
	public enum PriceFormat
	{
		UniPrice,
		OHLC
	}

	public class Bloomberg
	{
		private static bool IsRegisterChannel = false;  // 靜態屬性

		public static List<Dictionary<string, string>>
			GetBloombergData(List<string> lstSecurities, List<string> lstField)
		{

			List<Dictionary<string, string>> DataTable = new List<Dictionary<string, string>>();
			List<string> tmp = new List<string>();

			BloombergLib.BloombergLib.CBloomberg objBloomberg;
			CBloombergRemoteFactory objRemoteFactory;

			//取得本機的 IP
			string strLocalIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].ToString();
			string strServerIP = "10.5.48.46";  // Bloomberg PC

			if (strLocalIP != strServerIP)
			{
				if (IsRegisterChannel == false)  // 連線設定、只要一次、程式就可重複使用
				{
					string path1 = Directory.GetCurrentDirectory();
					string path2 = Directory.GetParent(path1).FullName;
					string path3 = Directory.GetParent(path2).FullName;
					string path4 = Directory.GetParent(path3).FullName;
					string AppConfig = path4 + "\\UtilityLibrary\\App.Config";

					//RemotingConfiguration.Configure(@"D:\Work\20110819_BloombergAPI\AutoGetData\App.Config", false);
					RemotingConfiguration.Configure(AppConfig, false);
					RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
					RemotingConfiguration.CustomErrorsEnabled(true);
					IsRegisterChannel = true;
				}
			}

			//產生 object (若有 Remoting, 則object由Server端產生,否則由Client端產生)
			objRemoteFactory = new CBloombergRemoteFactory();
			objBloomberg = objRemoteFactory.GetNewInstance();

			try
			{

				//'清除資料
				objBloomberg.ClearData();

				//'讀取今天的資料
				objBloomberg.RequestType = RequestType.ReferenceDataRequest;

				foreach (string Security in lstSecurities)
				{
					objBloomberg.AddSecurities(Security);
				}

				foreach (string Field in lstField)
				{
					objBloomberg.AddFields(Field);
				}

				//'讀取Bloomberg資料
				objBloomberg.SendRequest();

				foreach (BloombergLib.CBloombergData objBloomberData in objBloomberg.GetData())
				{
					Dictionary<string, string> OHLC = new Dictionary<string, string>();

					OHLC.Add("IssueCode", objBloomberData.Security.ToUpper());

					foreach (string Field in lstField)
					{
						OHLC.Add(Field, objBloomberData.get_Field(Field));
					}

					DataTable.Add(OHLC);
				}

			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}

			return DataTable;

		}

		private double getCSVtime()
		{
			string strline = "";
			string[] valueStr = { "1.0" };
			int rows;

			StreamReader csvfile = new StreamReader(@"R:\RealTime.txt");

			rows = 0;

			while (!csvfile.EndOfStream)
			{
				rows = rows + 1;
				strline = csvfile.ReadLine();
				valueStr = strline.Split(',');
			}
			csvfile.Close();

			return Convert.ToDouble(valueStr.First());
		}


		// 一次取得許多商品的歷史價格
		public static void
			GetHistoricalData(string date1, string date2, string opfile, List<string> lstSecurities)
		{

			BloombergLib.BloombergLib.CBloomberg objBloomberg;
			CBloombergRemoteFactory objRemoteFactory;

			//取得本機的 IP
			string strLocalIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].ToString();
			string strServerIP = "10.5.48.46";  // Bloomberg PC

			if (strLocalIP != strServerIP)
			{
				if (IsRegisterChannel == false)  // 連線設定、只要一次、程式就可重複使用
				{
					//string path1 = Directory.GetCurrentDirectory();
					//string path2 = Directory.GetParent(path1).FullName;
					//string path3 = Directory.GetParent(path2).FullName;
					//string path4 = Directory.GetParent(path3).FullName;
					//string AppConfig = path4 + "\\Bloomberg\\App.Config";
					//RemotingConfiguration.Configure(AppConfig, false);

					RemotingConfiguration.Configure(@"C:\Bloomberg.Config", false);
					RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
					RemotingConfiguration.CustomErrorsEnabled(true);
					IsRegisterChannel = true;
				}
			}

			//產生 object (若有 Remoting, 則object由Server端產生,否則由Client端產生)
			objRemoteFactory = new CBloombergRemoteFactory();
			objBloomberg = objRemoteFactory.GetNewInstance();

			try
			{
				//'清除資料
				objBloomberg.ClearData();

				//'讀取今天的資料
				objBloomberg.RequestType = RequestType.HistoricalDataRequest;


				List<string> lstField = new List<string>();
				lstField.Add("PX_OPEN");
				lstField.Add("PX_HIGH");
				lstField.Add("PX_LOW");
				lstField.Add("PX_LAST");

				foreach (string Security in lstSecurities)
					objBloomberg.AddSecurities(Security);

				foreach (string Field in lstField)
					objBloomberg.AddFields(Field);

				//string mktdate = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

				BloombergLib.BloombergLib.CBloombergHistoricalOption objHistoricalOption;
				objHistoricalOption = objBloomberg.GetHistoricalOption();

				objHistoricalOption.periodicityAdjustment = periodicityAdjustment.ACTUAL;
				objHistoricalOption.periodicitySelection = periodicitySelection.DAILY;

				//'設定資料開始日期
				objHistoricalOption.StartDate = date1; // mktdate; // ' "20110901" 'txtStartDate.Text
				//'設定資料結束日期
				objHistoricalOption.EndDate = date2; // mktdate; // ' "20110901" 'txtEndDate.Text

				objHistoricalOption.nonTradingDayFillOption = nonTradingDayFillOption.ACTIVE_DAYS_ONLY;

				objHistoricalOption.nonTradingDayFillMethod = nonTradingDayFillMethod.PREVIOUS_VALUE;

				//'讀取Bloomberg資料
				objBloomberg.SendRequest();



				if (File.Exists(opfile))
					File.Delete(opfile);

				StreamWriter sw = File.AppendText(opfile);

				foreach (BloombergLib.CBloombergData objBloomberData in objBloomberg.GetData())
				{
					string tempMessage = objBloomberData.HistoryDate.Replace("-", "") + ",";

					foreach (string Field in lstField)
					{
						tempMessage += objBloomberData.get_Field(Field) + ",";
					}
					sw.WriteLine("{0}", tempMessage);

				}
				sw.WriteLine(DateTime.Now.ToString("yyyyMMdd.hhmmss"));
				sw.Close();
				Console.WriteLine("Done!");

			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}




		}


		// 取得單一商品的歷史價格
		public static void
		GetPriceIndividual(string date1, string date2, string opfile, string BloombergTicker, PriceFormat priceFormat)
		{
			BloombergLib.BloombergLib.CBloomberg objBloomberg;
			CBloombergRemoteFactory objRemoteFactory;

			//取得本機的 IP
			string strLocalIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].ToString();
			string strServerIP = "10.5.48.46";  // Bloomberg PC

			if (strLocalIP != strServerIP)
			{
				if (IsRegisterChannel == false)  // 連線設定、只要一次、程式就可重複使用
				{
					//string path1 = Directory.GetCurrentDirectory();
					//string path2 = Directory.GetParent(path1).FullName;
					//string path3 = Directory.GetParent(path2).FullName;
					//string path4 = Directory.GetParent(path3).FullName;
					//string AppConfig = path4 + "\\Bloomberg\\App.Config";
					//RemotingConfiguration.Configure(AppConfig, false);

					RemotingConfiguration.Configure(@"C:\Bloomberg.Config", false);
					RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
					RemotingConfiguration.CustomErrorsEnabled(true);
					IsRegisterChannel = true;
				}
			}

			//產生 object (若有 Remoting, 則object由Server端產生,否則由Client端產生)
			objRemoteFactory = new CBloombergRemoteFactory();
			objBloomberg = objRemoteFactory.GetNewInstance();

			try
			{
				//'清除資料
				objBloomberg.ClearData();

				//'讀取今天的資料
				objBloomberg.RequestType = RequestType.HistoricalDataRequest;  // <======= IMPORTANT!!!

				List<string> lstField = new List<string>();

				if (priceFormat == PriceFormat.UniPrice)
				{
					lstField.Add("PX_LAST"); lstField.Add("PX_LAST"); lstField.Add("PX_LAST"); lstField.Add("PX_LAST");
				}
				else
				{
					lstField.Add("PX_OPEN"); lstField.Add("PX_HIGH"); lstField.Add("PX_LOW"); lstField.Add("PX_LAST");
				}

				objBloomberg.AddSecurities(BloombergTicker);

				foreach (string Field in lstField) objBloomberg.AddFields(Field);

				//string mktdate = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

				BloombergLib.BloombergLib.CBloombergHistoricalOption objHistoricalOption;
				objHistoricalOption = objBloomberg.GetHistoricalOption();

				objHistoricalOption.periodicityAdjustment = periodicityAdjustment.ACTUAL;
				objHistoricalOption.periodicitySelection = periodicitySelection.DAILY;

				//'設定資料開始日期
				objHistoricalOption.StartDate = date1; // mktdate; // ' "20110901" 'txtStartDate.Text
				//'設定資料結束日期
				objHistoricalOption.EndDate = date2; // mktdate; // ' "20110901" 'txtEndDate.Text

				objHistoricalOption.nonTradingDayFillOption = nonTradingDayFillOption.ACTIVE_DAYS_ONLY;

				objHistoricalOption.nonTradingDayFillMethod = nonTradingDayFillMethod.PREVIOUS_VALUE;

				//'讀取Bloomberg資料
				objBloomberg.SendRequest();

				List<string> tmpout = new List<string>();

				foreach (BloombergLib.CBloombergData objBloomberData in objBloomberg.GetData())
				{
					string tempMessage = objBloomberData.HistoryDate.Replace("-", "");// +",";

					foreach (string Field in lstField)
					{
						tempMessage += "," + objBloomberData.get_Field(Field);
					}
					//sw.WriteLine("{0}", tempMessage);
					tmpout.Add(tempMessage);
				}

				if (tmpout.Count > 0)
				{
					StreamWriter sw = new StreamWriter(opfile, false, Encoding.Default);

					foreach (string strout in tmpout)
					{
						Console.WriteLine(strout);
						sw.WriteLine("{0}", strout);
					}
					sw.Close();
				}
				else
					Console.WriteLine("{0} No data from Bloomberg!!!", BloombergTicker);

				Console.WriteLine("Done!");

			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}


		}

		// 取得單一商品的歷史價格 for MarketData
		public static List<BloombergPrice>
		GetHistoricalPrice(string startDate, string endDate, string BloombergTicker, PriceFormat priceFormat)
		{
			BloombergLib.BloombergLib.CBloomberg objBloomberg;
			CBloombergRemoteFactory objRemoteFactory;
			List<BloombergPrice> HistoricalPrice = new List<BloombergPrice>();

			//取得本機的 IP
			string strLocalIP = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())[0].ToString();
			string strServerIP = "10.5.48.46";  // Bloomberg PC

			if (strLocalIP != strServerIP)
			{
				if (IsRegisterChannel == false)  // 連線設定、只要一次、程式就可重複使用
				{
					//string path1 = Directory.GetCurrentDirectory();
					//string path2 = Directory.GetParent(path1).FullName;
					//string path3 = Directory.GetParent(path2).FullName;
					//string path4 = Directory.GetParent(path3).FullName;
					//string AppConfig = path4 + "\\Bloomberg\\App.Config";
					//RemotingConfiguration.Configure(AppConfig, false);

					RemotingConfiguration.Configure(@"C:\Bloomberg.Config", false);
					RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
					RemotingConfiguration.CustomErrorsEnabled(true);
					IsRegisterChannel = true;
				}
			}

			//產生 object (若有 Remoting, 則object由Server端產生,否則由Client端產生)
			objRemoteFactory = new CBloombergRemoteFactory();
			objBloomberg = objRemoteFactory.GetNewInstance();

			try
			{
				//'清除資料
				objBloomberg.ClearData();

				//'讀取今天的資料
				objBloomberg.RequestType = RequestType.HistoricalDataRequest;  // <======= IMPORTANT!!!

				List<string> lstField = new List<string>();

				if (priceFormat == PriceFormat.UniPrice)
				{
					lstField.Add("PX_LAST"); lstField.Add("PX_LAST"); lstField.Add("PX_LAST"); lstField.Add("PX_LAST");
				}
				else
				{
					lstField.Add("PX_OPEN"); lstField.Add("PX_HIGH"); lstField.Add("PX_LOW"); lstField.Add("PX_LAST");
				}

				objBloomberg.AddSecurities(BloombergTicker);

				foreach (string Field in lstField) objBloomberg.AddFields(Field);

				//string mktdate = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

				BloombergLib.BloombergLib.CBloombergHistoricalOption objHistoricalOption;
				objHistoricalOption = objBloomberg.GetHistoricalOption();

				objHistoricalOption.periodicityAdjustment = periodicityAdjustment.ACTUAL;
				objHistoricalOption.periodicitySelection = periodicitySelection.DAILY;

				//'設定資料開始日期
				objHistoricalOption.StartDate = startDate; // mktdate; // ' "20110901" 'txtStartDate.Text
				//'設定資料結束日期
				objHistoricalOption.EndDate = endDate; // mktdate; // ' "20110901" 'txtEndDate.Text

				objHistoricalOption.nonTradingDayFillOption = nonTradingDayFillOption.ACTIVE_DAYS_ONLY;

				objHistoricalOption.nonTradingDayFillMethod = nonTradingDayFillMethod.PREVIOUS_VALUE;

				//'讀取Bloomberg資料
				objBloomberg.SendRequest();

				List<string> tmpout = new List<string>();

				foreach (BloombergLib.CBloombergData objBloomberData in objBloomberg.GetData())
				{
					string tempMessage = objBloomberData.HistoryDate.Replace("-", "");// +",";

					foreach (string Field in lstField)
					{
						tempMessage += "," + objBloomberData.get_Field(Field);
					}
					//sw.WriteLine("{0}", tempMessage);
					tmpout.Add(tempMessage);
				}

				if (tmpout.Count > 0)
				{
					foreach (var tmpdata in tmpout)
					{
						BloombergPrice tmpOHLC = new BloombergPrice();
						char[] delimiterChars = { ',' };
						string[] tmpstring = tmpdata.Split(delimiterChars);

						tmpOHLC.Datetime = DateTime.ParseExact(tmpstring[0], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
						tmpOHLC.Open = double.Parse(tmpstring[1]);
						tmpOHLC.High = double.Parse(tmpstring[2]);
						tmpOHLC.Low = double.Parse(tmpstring[3]);
						tmpOHLC.Close = double.Parse(tmpstring[4]);

						HistoricalPrice.Add(tmpOHLC);
					}
				}


			}
			catch (System.Exception ex)
			{
				throw ex;
			}

			return HistoricalPrice;
		}

		public class BloombergPrice
		{
			public DateTime Datetime { get; set; }
			public double Open { get; set; }
			public double High { get; set; }
			public double Low { get; set; }
			public double Close { get; set; }
		}
	}
}
