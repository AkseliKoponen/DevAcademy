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
	/// <summary>
	/// Creates a trip by slicing the .csv <paramref name="line"/> into values
	/// </summary>
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
	/// <summary>
	/// Converts <paramref name="dt"/> to a string matching the format in database
	/// </summary>
	public static string DateTimeToString(DateTime dt)
	{
		//Format for Database is 2021-05-31T23:57:25
		return dt.Year + "-" + dt.Month + "-" + dt.Day + "T" + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
	}
	/// <summary>
	/// Gets a datetime from a <paramref name="str"/> with the format used in the database
	/// </summary>
	static DateTime GetDateTimeFromString(string str)
	{
		DateTime dt = new DateTime(0);
		try
		{
			if (str.Contains('T'))
			{
				List<int> dates = GetSeparatedValues(str.Substring(0, str.IndexOf('T')));
				int year = dates[0];
				int month = dates[1];
				int day = dates[2];
				List<int> times = GetSeparatedValues(str.Substring(str.IndexOf('T') + 1));
				int hour = times[0];
				int minute = times[1];
				int second = times[2];
				dt = new DateTime(year, month, day, hour, minute, second);
			}
			else
			{
				List<int> dates = GetSeparatedValues(str);
				int year = dates[0];
				int month = dates[1];
				int day = dates[2];
				dt = new DateTime(year, month, day);
			}
		}
		catch { Console.WriteLine(str + " could not be converted to DateTime!!"); }
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
	/// <summary>
	/// Gets distance in a readable kilometer format
	/// </summary>
	public string GetDistanceKm()
	{
		float f = (float)distance / (float)1000;
		return f.ToString("0.00") + " km";
	}

	/// <summary>
	/// Gets duration in a readable minute format
	/// </summary>
	public string GetDurationMin()
	{
		return ((float)duration / (float)60).ToString("0.0") + " min";
	}

	/// <summary>
	/// Get all data in one .csv style line
	/// </summary>
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
	/// <summary>
	/// Get departure time in DateTime
	/// </summary>
	public DateTime GetDeptTime()
	{
		return GetDateTimeFromString(deptTime);
	}
	/// <summary>
	/// Get return time in DateTime
	/// </summary>
	public DateTime GetRetTime()
	{
		return GetDateTimeFromString(retTime);
	}
	/// <summary>
	///	Get the names of columns displayed in ListView.
	///	<br>Set <paramref name="displayName"/> as false to get the names of the corresponding properties</br>
	/// </summary>
	public static List<string> GetColumns(bool displayName = true)
	{
		if(displayName)
			return new List<string> { "Departure Station", "Return Station", "Distance (km)", "Duration (min)" };
		else
			return new List<string> { "deptStationName", "retStationName", "distance", "duration"};
	}

	/// <summary>
	///	Get the names of the properties assigned in InsertForm
	/// </summary>
	public static List<string> GetInputNames()
	{
		return new List<string> { "Departure Time","Departure Station", "Return Time","Return Station", "Distance (m)", "Duration (s)" };
	}

}
