using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using Dapper;

public class DBManager
{
	public static void SetTable(bool trip)
	{
		currentTable = trip?trips:stations;
	}
	public static DBManager currentTable;
	public static DBManager trips { get; private set; } = new DBManager("TRIPS");
	public static DBManager stations { get; private set; } = new DBManager("STATIONS");
	public string table { get; private set; }
	DBManager(string _table)
	{
		table = _table;
	}
	static string connStr = "Data Source=.\\biketrips.db;Version=3;";
	string orderBy = "";
	List<string> whereString = new List<string>(); 
	string limit = " LIMIT 30";
	public int matchCount = 0;
	/// <summary>
	/// Clear order and where.
	/// </summary>
	public  void ResetFilters()
	{
		orderBy = "";
		whereString = new List<string>();
	}
	public  void RemoveFilterColumn(string column)
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
	/// If the order is already the same, reverse the order (from descending to ascending) and return false
	/// Return true if not the same order or columns is null
	/// </summary>
	public  bool OrderBy(List<string> columns)
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
	/// The size of the returned list is limited by Limit()
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
		/// The size of the returned list is limited by Limit()
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
	public  int GetCount(bool _limit = true)
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			IEnumerable<int> temp = cnn.Query<int>("SELECT COUNT(*) from "+table + GetWhereString() + orderBy + (_limit?limit:""));
			return temp.ToList()[0];

		}
	}
	/// <summary>
	/// Saves a new trip into the database
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
	public void TrimDatabase()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			cnn.Execute("delete from "+table+" where distance < 10");
			cnn.Execute("delete from "+table+" where duration < 10");
		}
	}
	#endregion

}