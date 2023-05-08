using System;
using System.Collections.Generic;
using System.Reflection;

public partial class Trip
{

	public string? deptTime { get; set; }

	public string? retTime { get; set; }

	public int? deptStationId { get; set; }

	public string? deptStationName { get; set; }

	public int? retStationId { get; set; }

	public string? retStationName { get; set; }

	public int? distance { get; set; }

	public int? duration { get; set; }
	public Trip() { }
	public Trip(string line)
	{
		List<string> data = CSVReader.LineToList(line);
		PropertyInfo[] fis = GetType().GetProperties();
		for (int i = 0; i < data.Count && i < fis.Length; i++)
		{
			if (fis[i].PropertyType == typeof(string))
				fis[i].SetValue(this, data[i]);
			else
			{
				try
				{
					fis[i].SetValue(this, int.Parse(data[i]));
				}
				catch
				{
				}
			}
		}
	}
	public bool Validate()
	{
		return distance >= 10 && duration >= 10;

	}
	static DateTime GetDateTimeFromString(string str)
	{
		DateTime dt;
		List<int> dates = GetSeparatedValues(str.Substring(0, str.IndexOf('T')));
		int year = dates[0];
		int month = dates[1];
		int day = dates[2];
		List<int> times = GetSeparatedValues(str.Substring(str.IndexOf('T') + 1));
		int hour = times[0];
		int minute = times[1];
		int second = times[2];
		dt = new DateTime(year, month, day, hour, minute, second);
		return dt;

		List<int> GetSeparatedValues(string s)
		{
			char separator = ' ';
			foreach (char c in s.ToCharArray())
			{
				if (!Char.IsDigit(c))
				{
					separator = c;
					break;
				}
			}
			if (s.EndsWith(separator)) s = s.Substring(0, s.Length - 1);
			if (s.StartsWith(separator)) s = s.Substring(1);
			List<int> values = new List<int>();
			while (s.Contains(separator))
			{
				AddValue(s.Substring(0, s.IndexOf(separator)));
				s = s.Substring(s.IndexOf(separator) + 1);
			}
			AddValue(s);
			return values;


			void AddValue(string val)
			{

				int i = -1;
				int.TryParse(val, out i);
				values.Add(i);
			}
		}
	}
	public string GetDistanceKm()
	{
		float f = (float)distance / (float)1000;
		return f.ToString("0.00") + " km";
	}
	public string GetDurationMin()
	{
		return ((float)duration / (float)60).ToString("0.0") + " min";
	}
	public string GetData()
	{
		string s = "";
		foreach (PropertyInfo fi in GetType().GetProperties())
		{
			object obj = fi.GetValue(this);
			s += (obj != null ? "'" + obj.ToString() + "'" : "NULL") + ", ";
		}
		return s.Substring(0, s.Length - 2);
	}
	public DateTime GetDeptTime()
	{
		return GetDateTimeFromString(deptTime);
	}
	public DateTime GetRetTime()
	{
		return GetDateTimeFromString(retTime);
	}
	public static string GetColumns()
	{

		return "(deptTime, retTime, deptStationId, deptStationName, retStationId, retStationName, distance, duration)";
	}
	public void Debug()
	{
		foreach (PropertyInfo fi in GetType().GetProperties())
		{
			object obj = fi.GetValue(this);
			string str = fi.Name + ": " + (obj != null ? obj.ToString() : "NULL");
			Console.WriteLine(str);
		}
	}
}
