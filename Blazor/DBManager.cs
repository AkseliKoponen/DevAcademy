using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using Dapper;

/// <summary>
/// DBManager is the interface for connecting to the database.
/// <br>Use <b>DBManager.stations</b> to access stations and <b>DBManager.trips</b> to access trips.</br>
/// </summary>
public class DBManager
{
	/// <summary>
	/// Changes the currentTable to either trips or stations
	/// </summary>
	/// <param name="trip">If true set to trips. Else set to stations.</param>
	public static void ToggleTable(bool trip)
	{
		currentTable = trip?trips:stations;
	}
	/// <summary>
	/// Keeps track of which table is displayed in ListView page
	/// </summary>
	public static DBManager currentTable;
	/// <summary>
	/// The TRIPS table in the database
	/// </summary>
	public static DBManager trips { get; private set; } = new DBManager("TRIPS");
	/// <summary>
	/// The STATIONS table in the database
	/// </summary>
	public static DBManager stations { get; private set; } = new DBManager("STATIONS");

	/// <summary>
	/// Which table to connect to
	/// </summary>
	public string table { get; private set; }

	/// <summary>
	/// Constructor for the DBManager, only sets the table
	/// </summary>
	/// <param name="_table">Which table should be accessed</param>
	DBManager(string _table)
	{
		table = _table;
	}
	static string connStr = "Data Source=.\\biketrips.db;Version=3;";
	string orderBy = "";
	List<string> whereString = new List<string>(); 
	string limit = " LIMIT 30";

	/// <summary>
	/// The amount of records matching the set criteria.
	/// <br>Using this is faster than reloading all the records from the database.</br>
	/// </summary>
	public int matchCount = 0;

	/// <summary>
	/// Clear order and where.
	/// </summary>
	public  void ResetFilters()
	{
		orderBy = "";
		whereString = new List<string>();
	}
	/// <summary>
	/// Returns the list of conditions in whereString as a single string.
	/// </summary>
	/// <param name="column">Removes a whereString containing this string if it exists</param>
	public void RemoveFilterColumn(string column)
	{
		for(int i = 0; i < whereString.Count; i++)
		{
			if (whereString[i].Contains(column))
			{
				whereString.RemoveAt(i);
				return;
			}
		}
	}
	/// <summary>
	/// Returns the list of conditions in whereString as a single string.
	/// </summary>
	/// <param name="and">If true combine the strings with AND, else combine them with OR.</param>
	string GetWhereString(bool and = true)
	{
		if (whereString.Count > 0)
		{
			string str = " WHERE ";
			for(int i = 0; i < whereString.Count; i++)
			{
				str += whereString[i];
				if (i != whereString.Count - 1)
				{
					str += " "+(and?"AND":"OR")+" ";
				}
			}
			return str;
		}
		else return "";
	}
	/// <summary>
	/// Add a conditional to an existing WHERE
	/// </summary>
	public  void AddWhere(string str)
	{
		//Only one filter allowed for one column. When adding a new filter, make sure to remove the old one.
		string column = str.Substring(0, str.IndexOf(" "));
		RemoveFilterColumn(column);
		whereString.Add(str);
		Console.WriteLine(str);

	}
	/// <summary>
	/// Set a WHERE conditional to a future LoadTrip()
	/// </summary>
	public  void SetWhere(string str)
	{
		whereString = new List<string> { str };
	}
	/// <summary>
	/// Same as OrderBy, but takes a single column as parameter
	/// </summary>
	public bool OrderBy(string column)
	{
		return OrderBy(new List<string> { column });
	}

	/// <summary>
	/// Set the ordering according to columns
	/// <br>If the order is already the same, reverse the order (from descending to ascending) and return false</br>
	/// <br>Return true if not the same order or columns is null</br>
	/// </summary>
	public bool OrderBy(List<string> columns)
	{
		if (columns == null || columns.Count == 0)
		{
			orderBy = "";
			return true;
		}
		else {
			string str = " ORDER BY ";
			foreach (string column in columns)
			{
				str += column + ",";
			}
			string temp = str.Substring(0, str.Length - 1);
			if (!temp.Equals(orderBy))
			{
				orderBy = temp;
				return true;
			}
			else
			{
				//if already using the same order, reverse the order
				str = " ORDER BY ";
				foreach (string column in columns)
				{
					str += column + " DESC,";
				}
				orderBy = str.Substring(0, str.Length - 1);
				return false;
			}
		}
	}  
	/// <summary>
	/// Sets the limit of retrieved rows from database.
	/// </summary>
	public  void Limit(int count = 30,int pageIndex = 0)
	{
		if (count <= 0)
			limit = "";
		else
			limit = " LIMIT "+(pageIndex*count) + ", "+count;
	}
	/// <summary>
	/// Load entries from the database with conditions that have been set by Where() and OrderBy().
	/// <br>The size of the returned list is limited by Limit()</br>
	/// </summary>
	public static List<Trip> LoadTrips()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			DBManager dbm = trips;
			dbm.matchCount = dbm.GetCount(false);
			return cnn.Query<Trip>("select * from " + dbm.table + dbm.GetWhereString() + dbm.orderBy + dbm.limit).ToList();
		}
	}   /// <summary>
		/// Load entries from the database with conditions that have been set by Where() and OrderBy().
		/// <br>The size of the returned list is limited by Limit()</br>
		/// </summary>
	public static List<Station> LoadStations()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			DBManager dbm = stations;
			dbm.matchCount = dbm.GetCount(false);
			string query = "select * from " + dbm.table + dbm.GetWhereString() + dbm.orderBy + dbm.limit;
			return cnn.Query<Station>("select * from " + dbm.table + dbm.GetWhereString() + dbm.orderBy + dbm.limit).ToList();
		}
	}
	/// <summary>
	/// Get the total count of entries matching the criteria
	/// </summary>
	/// <param name="_limit">If true, limit the count by the assigned limiter. Else get an unlimited count.</param>
	public int GetCount(bool _limit = true)
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			IEnumerable<int> temp = cnn.Query<int>("SELECT COUNT(*) from "+table + GetWhereString() + orderBy + (_limit?limit:""));
			return temp.ToList()[0];

		}
	}
	/// <summary>
	/// Saves a new entry into the database
	/// </summary>
	public void Save(object obj)
	{
		if (new List<Type> { typeof(Trip), typeof(Station) }.Contains(obj.GetType()) == false)
		{
			Console.WriteLine("No table for " + obj.GetType().Name);
			return;
		}
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			if (obj.GetType() == typeof(Trip))
			{
				Trip t = (Trip)obj;
				cnn.Execute("insert into "+table+" values (" + t.GetData() + ")", t);
			}
			else if (obj.GetType() == typeof(Station))
			{
				Station s = (Station)obj;
				cnn.Execute("insert into " + table + " values (" + s.GetData() + ")", s);
			}
		}
	}

	/// <summary>
	///	Converts a .csv <paramref name="line"/> into a list of values
	/// </summary>
	public static List<string> LineToList(string line)
	{
		List<string> list = new List<string>();
		char separator = ',';

		while (line.Contains(separator))
		{
			string s = line.Substring(0, line.IndexOf(separator));
			bool quotes = (s.StartsWith('"') && line.Substring(1).Contains('"'));
			if (quotes)
			{
				s = '"' + line.Substring(1, line.Substring(1).IndexOf('"') + 1);
			}
			line = line.Substring(s.Length + 1);
			if (quotes) s = s.Substring(1, s.Length - 2);
			list.Add(s);
		}
		list.Add(line);
		return list;
	}
	#region Old and Unused
	/// <summary>
	/// Saves a large amount of entries to the database
	/// </summary>
	public void SaveTrips(List<Trip> trips)
	{
		int connectionReset = 1000;
		int totalTripCount = trips.Count;
		Console.WriteLine("-------Saving a total of " + totalTripCount + " to database--------");
		int counter = 0;
		int totalCounter = 0;
		while (trips.Count > 0)
		{
			using (IDbConnection cnn = new SQLiteConnection(connStr))
			{
				while (counter < connectionReset && (trips.Count > 0))
				{
					Trip trip = trips[0];
					cnn.Execute("insert into TRIPS values (" + trip.GetData() + ")", trip);
					trips.RemoveAt(0);
					counter++;
				}
			}
			totalCounter += counter;
			counter = 0;
			Console.WriteLine("--" + totalCounter + " of " + totalTripCount + " saved (" + (float)totalCounter / (float)totalTripCount * 100 + "%)!");

		}
	}

	/// <summary>
	/// Delete bad entries from the database.
	/// <br>This method was used once to trim the database and is no longer required.</br>
	/// </summary>
	public void TrimDatabase()
	{
		if (table == "TRIPS")
		{
			using (IDbConnection cnn = new SQLiteConnection(connStr))
			{
				cnn.Execute("delete from " + table + " where distance < 10");
				cnn.Execute("delete from " + table + " where duration < 10");
			}
		}
	}
	#endregion

}