using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class CSVReader
{
	public static List<Station> stations;
	public static async Task ReadLocal(StreamReader reader)
	{
		stations = new List<Station>();
		await reader.ReadLineAsync(); // skip header row
		while (true)
		{
			string? s = await reader.ReadLineAsync();
			if (s != null)
			{
				stations.Add(new Station(s));
			}
			else
				break;

		}
	}
	public static List<string> LineToList(string line)
	{
		List<string> list = new List<string>();
		char separator = ',';

		while (line.Contains(separator))
		{
			string s = line.Substring(0, line.IndexOf(separator));
			bool quotes = (s.StartsWith('"') && line.Substring(1).Contains('"'));
			if(quotes)
			{
				s =  '"' + line.Substring(1, line.Substring(1).IndexOf('"')+1);
			}
			line = line.Substring(s.Length+1);
			if(quotes) s= s.Substring(1,s.Length-2);
			list.Add(s);
		}
		list.Add(line);
		return list;
	}
}
