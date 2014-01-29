using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Text;


namespace FinnAutoSearcher
{
	class Program
	{
		static HtmlDocument doc;
		static HtmlWeb web;

		static void Main(string[] args)
		{
			web = new HtmlWeb();
			checkWeb();
		}

		private static void checkWeb()
		{
			HtmlDocument doc = web.Load("http://www.finn.no/finn/torget/resultat?keyword=radeon&SEGMENT=0&ITEM_CONDITION=0&SEARCHKEYNAV=SEARCH_ID_BAP_ALL&sort=1&periode=");

			HtmlNode resultList = doc.GetElementbyId("resultList");
			HtmlNodeCollection items = resultList.SelectNodes("//div[@class='man phl pbm ptl gridview r-object media']");

			int[] ids = new int[items.Count];
			for (int i = 0; i < items.Count; i++)
			{
				ids[i] = Convert.ToInt32(items[i].Id);
			}
			List<int> uniqueIDs = addItems(ids);





				while (true) { }
		}


		private static List<int> addItems(int[] ids)
		{
			


			FileStream fileStream = new FileStream("idLog", FileMode.OpenOrCreate, FileAccess.Read);
			FileInfo f = new FileInfo("idLog");


			byte[] lines = new byte[2048];
			List<int> uniqueIDs = new List<int>();
			string output = "";

			UTF8Encoding temp = new UTF8Encoding(true);
			fileStream.Read(lines, 0, lines.Length);
			string[] file = temp.GetString(lines).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			int[] storedIDs = new int[file.Length];
			fileStream.Close();
			for(int i = 0; i<file.Length; i++)
				storedIDs[i] = Convert.ToInt32(file[i]);

			

			foreach (int id in ids)
			{
				bool newID = true;
				foreach (int storedID in storedIDs)
				{
					if (id == storedID)
					{
						newID = false;
						break;
					}
				}
				if (newID)
				{
					uniqueIDs.Add(id);
					output += id + "\r\n";
				}
			}

			fileStream = new FileStream("idLog", FileMode.);
			byte[] outputData = null;
			outputData = Encoding.UTF8.GetBytes(output);
			fil




			return uniqueIDs;
		}
	}


}
