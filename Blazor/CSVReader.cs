using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class CSVReader
{
	public static List<Trip> trips;
	public static async Task ReadLocal(StreamReader reader)
	{
		trips = new List<Trip>();
		await reader.ReadLineAsync(); // skip header row
										//int id = 0;
		while (true)
		{
			string? s = await reader.ReadLineAsync();
			if (s != null)
			{
				Trip bt = new Trip(s);
				if (bt.Validate())
				{
					trips.Add(bt);
				}
			}
			else
				break;

		}
	}
	public static async Task ReadOnline(string url = "https://dev.hsl.fi/citybikes/od-trips-2021/2021-05.csv")
	{
		HttpClient httpClient = new HttpClient();
		HttpResponseMessage response = await httpClient.GetAsync(url);

		if (response.IsSuccessStatusCode)
		{
			Stream stream = await response.Content.ReadAsStreamAsync();
			Task t = Task.Run(() => CSVReader.ReadLocal(new StreamReader(stream)));
		}
		else
		{
			Console.WriteLine("Error downloading file: " + response.ReasonPhrase);
		}


	}

	public static List<string> LineToList(string line, char separator = ',')
	{
		List<string> list = new List<string>();
		while (line.Contains(separator))
		{
			string s = line.Substring(0, line.IndexOf(separator));
			list.Add(s);
			line = line.Substring(line.IndexOf(separator) + 1);
		}
		list.Add(line);
		return list;
	}
}
