#region Includes
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using System.Net.Mail;
#endregion

namespace FinnAutoSearcher
{
	class Program
	{
		#region Declarations
		static HtmlWeb finn;
		static HtmlWeb HW;
		static string fileName = "idLog";
		static string fileNameHW = "hwIDLog";
		static string configName = "config";
		static List<int> storedIDs;
		static List<int> storedHWIDs;
		static int maxLength = 100;
		static string mailBody = "";
		static string mailTitle = "";
		static List<string> toEmails;
		static bool changed = false;
		static bool changedHW = false;
		#endregion

		static void Main(string[] args)
		{
			Console.WriteLine("-----------------------------------------------------------------------------");
			Console.WriteLine("FinnAutoSearcher - v1.1, 2014-01-31");
			Console.WriteLine("Author: Ole Kristian Nakken, ok.nakken@gmail.com");
			Console.WriteLine("FinnAutoSearcher searches finn.no for the searchterm 'RADEON', and sends all new auctions to the specified emails.");
			Console.WriteLine("In addition to searchin finn, FinnAutoSearcher now also searches Hardware.no's 'bruktmarked' for new graphicscards for sale");
			Console.WriteLine("You can enter your emails the first time you start the program, or by visiting your config file, in the program dir.");
			Console.WriteLine("-----------------------------------------------------------------------------");


			finn = new HtmlWeb();
			HW = new HtmlWeb();
			storedIDs = new List<int>();
			storedHWIDs = new List<int>();
			toEmails = new List<string>();
			loadConfig();
			readFinnFile();
			readHWFile();

			while (true)
			{
				if (!changed && !changedHW)
					Console.SetCursorPosition(0, Console.CursorTop);

				DateTime time = DateTime.Now;
				Console.Write("Waiting for page update. Last checked: " + time.Hour + ":" + time.Minute + ":" + time.Second + "   ");


				mailTitle = "New items: ";
				mailBody = "";
				checkFinn();
				checkHW();
				if (!mailBody.Equals(""))
				{
					sendMail();
				}

				System.Threading.Thread.Sleep(10000);
			}
		}

		private static void checkFinn()
		{

			HtmlDocument doc = finn.Load("http://www.finn.no/finn/torget/resultat?keyword=radeon&SEGMENT=0&ITEM_CONDITION=0&SEARCHKEYNAV=SEARCH_ID_BAP_ALL&sort=1&periode=");
			//HtmlDocument doc = web.Load("http://www.finn.no/finn/torget/resultat?SEGMENT=0&ITEM_CONDITION=0&SEARCHKEYNAV=SEARCH_ID_BAP_ALL&sort=1&periode=1");

			HtmlNode resultList = doc.GetElementbyId("resultList");
			HtmlNodeCollection items = resultList.SelectNodes("//div[@class='man phl pbm ptl gridview r-object media']");

			List<int> idsToAdd = new List<int>();


			for (int i = 0; i < items.Count; i++)
			{
				int id = Convert.ToInt32(items[i].Id);
				if (!storedIDs.Contains(id))
				{
					addFinnToMail(items[i]);
					storedIDs.Add(id);
				}
			}
			if (!mailBody.Equals(""))
			{
				changed = true;
				writeFinnIdsToFile();
			}
			else
			{
				changed = false;
			}
		}

		private static void checkHW()
		{
			HtmlDocument doc = HW.Load("http://www.hardware.no/bruktmarked/grafikkort_skjermkort?&type=selling");
			HtmlNode list = doc.GetElementbyId("main");
			HtmlNodeCollection annonse = list.SelectNodes("//li[@class='ad']");
			HtmlNodeCollection items = list.SelectNodes("//li[@class='ad']/div[@class='description']/h3/a");
			List<int> idsToAdd = new List<int>();


			for (int i = 0; i < items.Count; i++)
			{
				string link = items[i].GetAttributeValue("href", "tissefant");
				string[] d = link.Split('/');

				int id = Convert.ToInt32(d[d.Length - 1]);
				if (!storedHWIDs.Contains(id))
				{
					mailTitle += items[i].WriteContentTo();
					addHWToMail(annonse[i]);
					storedHWIDs.Add(id);
				}
			}
			if (!mailBody.Equals(""))
			{
				changedHW= true;
				writeHWIDsToFile();
			}
			else
			{
				changedHW = false;
			}

			/*
			foreach (HtmlNode node in items)
			{
				string link = node.GetAttributeValue("href","tissefant");
				string[] d = link.Split('/');
				Console.WriteLine(d[d.Length - 1]);
			}*/
		}


		#region Read/Write file
		private static void readFinnFile()
		{
			string line;
			if (File.Exists(fileName))
			{
				StreamReader file = null;
				try
				{
					file = new StreamReader(fileName);
					while ((line = file.ReadLine()) != null)
					{
						storedIDs.Add(Convert.ToInt32(line));
					}
				}
				finally
				{
					if (file != null)
						file.Close();
					Console.WriteLine("Read and processed file: " + fileName);
				}
			}
			else
			{
				Console.WriteLine("No Finn log file found. A new log file will be created.");
			}
		}

		private static void readHWFile()
		{
			string line;
			if (File.Exists(fileNameHW))
			{
				StreamReader file = null;
				try
				{
					file = new StreamReader(fileNameHW);
					while ((line = file.ReadLine()) != null)
					{
						storedHWIDs.Add(Convert.ToInt32(line));
					}
				}
				finally
				{
					if (file != null)
						file.Close();
					Console.WriteLine("Read and processed file: " + fileNameHW);
				}
			}
			else
			{
				Console.WriteLine("No HW log file found. A new log file will be created.");
			}
		}

		private static void loadConfig()
		{
			string line;
			if (File.Exists(configName))
			{
				StreamReader file = null;
				try
				{
					file = new StreamReader(configName);
					while ((line = file.ReadLine()) != null)
					{
						if (!line.StartsWith("#"))
							toEmails.Add(line);
					}
				}
				finally
				{
					if (file != null)
						file.Close();
					Console.WriteLine("Read and processed file: " + configName);
				}
			}
			else
			{
				Console.WriteLine(" ");
				Console.WriteLine("------------------------------------ERROR------------------------------------");
				Console.WriteLine("No config file found. A new file has been created created.");
				Console.WriteLine("Please enter your email address to automaticly recieve updates from finn.");
				Console.WriteLine("To insert multiple addresses separate them with a white space.");
				Console.WriteLine("------------------------------------ERROR------------------------------------");
				Console.Write("Email: ");
				string email = Console.ReadLine();
				string[] emails = email.Split(' ');

				TextWriter tw = new StreamWriter(configName);
				tw.WriteLine("#Finn Auto Searcher. Author: Ole Kristian Nakken. Contact: ok.nakken@gmail.com");
				tw.WriteLine("#This is the config file for FinnAutoSearcher");
				tw.WriteLine("#All lines starting with # will not be interpreted by the program, and are counted as comments.");
				tw.WriteLine("#Please enter all email addresses to recieve updates from this program on separate lines below.");
				foreach (string e in emails)
				{
					tw.WriteLine(e);
					toEmails.Add(e);
				}
				tw.Close();
			}
		}

		private static void writeFinnIdsToFile()
		{
			if (storedIDs.Count > maxLength)
				storedIDs.RemoveRange(0, storedIDs.Count - maxLength);

			try
			{
				File.Delete(fileName);
			}
			catch (Exception e)
			{

			}
			TextWriter tw = new StreamWriter(fileName);
			foreach (int id in storedIDs)
			{
				tw.WriteLine(id);
			}
			tw.Close();
			Console.WriteLine("Wrote new IDs to file: " + fileName);
		}

		private static void writeHWIDsToFile()
		{
			if (storedHWIDs.Count > maxLength)
				storedHWIDs.RemoveRange(0, storedHWIDs.Count - maxLength);

			try
			{
				File.Delete(fileNameHW);
			}
			catch (Exception e)
			{

			}
			TextWriter tw = new StreamWriter(fileNameHW);
			foreach (int id in storedHWIDs)
			{
				tw.WriteLine(id);
			}
			tw.Close();
			Console.WriteLine("Wrote new IDs to file: " + fileNameHW);
		}
		#endregion

		#region Mail handling
		private static void sendMail()
		{
			Console.WriteLine(mailTitle);

			try
			{
				MailMessage mail = new MailMessage();
				SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");


				mail.From = new MailAddress("ok.nakken2@gmail.com");
				foreach (string email in toEmails)
					mail.To.Add(email);

				mail.Subject = mailTitle;
				mail.Body = mailBody;
				mail.IsBodyHtml = true;

				SmtpServer.Port = 587;
				SmtpServer.Credentials = new System.Net.NetworkCredential("ok.nakken2@gmail.com", "rasor123");
				SmtpServer.EnableSsl = true;


				SmtpServer.Send(mail);
				Console.WriteLine("Mail Sent");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to send mail " + ex);
			}
		}

		private static void addFinnToMail(HtmlNode node)
		{
			HtmlNode textNode = node.SelectSingleNode("//h2[@class='mtn mln']/a[@href='annonse?finnkode=" + node.Id + "']");
			mailTitle += textNode.WriteContentTo();
			// http://www.finn.no/finn/torget/annonse?finnkode=46428558

			string body = node.WriteTo();
			body = body.Replace("annonse?finnkode=", "http://www.finn.no/finn/torget/annonse?finnkode=");
			mailBody += body;
		}

		private static void addHWToMail(HtmlNode node)
		{
			//Console.Write(node.WriteTo());
			//ADD TITLE


			string body = node.WriteTo();
			body = body.Replace("/bruktmarked/annonse/", "http://www.hardware.no/bruktmarked/annonse/");
			mailBody += body;
		}
		#endregion
	}
}
