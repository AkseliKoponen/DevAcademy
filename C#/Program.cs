using System.Formats.Asn1;
using System.Globalization;

namespace DevAcademy
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Reflection;
	internal class Program
	{
		static void Main(string[] args)
		{
			Task t = Task.Run(() => CSVReader.Read());
			t.Wait();
		}

		static class CSVReader
		{
			public static async Task Read()
			{
				string url = "https://dev.hsl.fi/citybikes/od-trips-2021/2021-05.csv";
				HttpClient httpClient = new HttpClient();
				HttpResponseMessage response = await httpClient.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					
					Stream stream = await response.Content.ReadAsStreamAsync();
					List<BikeTrip> trips = new List<BikeTrip>();
					StreamReader reader = new StreamReader(stream);
					await reader.ReadLineAsync(); // skip header row
					int i = 500;
					while (true && i >0)
					{
						i--;
						string? s = reader.ReadLine();
						if (s != null)
						{
							trips.Add(new BikeTrip(LineToList(reader.ReadLine())));
						}
						else
							break;
						
					}
					if (i == 0)
						Console.WriteLine("INFINITY ACHIEVED");
					Console.WriteLine("Type index of trip to view info (0 - "+(trips.Count-1)+")");
					while (true) {
						string input = "";
						do
						{
							input = Console.ReadLine();
						} while (input == null || input == "");
						int index;
						try
						{
							index = int.Parse(input);
						}
						catch
						{
							Console.WriteLine("Invalid Input!");
							continue;
						}
						trips[index].Debug();
					}
				}
				else
				{
					Console.WriteLine("Error downloading file: "+response.ReasonPhrase);
				}

				List<string> LineToList(string line)
				{
					List<string> list = new List<string>();
					while (line.Contains(','))
					{
						string s = line.Substring(0, line.IndexOf(','));
						list.Add(s);
						line = line.Substring(line.IndexOf(',') + 1);
					}
					list.Add(line);
					return list;
				}
			}
		}

		class BikeTrip
		{
			public string StartedAt;
			public string EndedAt;
			public int FromStationId;
			public string FromStationName;
			public int ToStationId;
			public string ToStationName;
			public string BikeId;
			public string PlanDurationInSeconds;
			public BikeTrip(List<string> data)
			{
				FieldInfo[] fis = GetType().GetFields();
				for (int i = 0;i<data.Count && i < fis.Length; i++)
				{
					if (fis[i].FieldType == typeof(string))
						fis[i].SetValue(this, data[i]);
					else if(fis[i].FieldType == typeof(int))
					{
						try {
							fis[i].SetValue(this, int.Parse(data[i]));
						} catch { }
					}
				}
			}
			public void Debug()
			{
				foreach(FieldInfo fi in GetType().GetFields())
				{
					object obj = fi.GetValue(this);
					string str = fi.Name + ": " + (obj != null ? obj.ToString() : "NULL");
					Console.WriteLine(str);
				}
			}
		}
	}
}