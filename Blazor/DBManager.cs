using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.Intrinsics.Arm;
using Dapper;

public static class DBManager
{
	static string connStr = "Data Source=.\\biketrips.db;Version=3;";
	//static string distinct = "distinct";
	static string orderBy = "";
	static string where = "";
	static string limit = " LIMIT 30";
	public static int totalEntryCount = 0;
	public static void ResetFilters()
	{
		orderBy = "";
		where = "";
	}
	public static void AddWhere(string str)
	{
		if (where == "" || where == null)
			where = " WHERE ";
		else
			where += " AND ";
		where += str;

	}
	public static void Where(string str)
	{
		where = " WHERE " + str;
	}
	public static void OrderBy(List<string> columns)
	{
		if (columns == null || columns.Count == 0)
		{
			orderBy = "";
		}
		else {
			string str = " ORDER BY ";
			foreach (string column in columns)
			{
				str += column + ",";
			}
			string temp = str.Substring(0, str.Length - 1);
			if (!temp.Equals(orderBy))
				orderBy = temp;
			else
			{
				//if already using the same order, reverse the order
				str = " ORDER BY ";
				foreach (string column in columns)
				{
					str += column + " DESC,";
				}
				orderBy = str.Substring(0, str.Length - 1);
			}
		}
	}
	public static void Limit(int count = 30,int pageIndex = 0)
	{
		if (count <= 0)
			limit = "";
		else
			limit = " LIMIT "+(pageIndex*count) + ", "+count;
	}
	public static List<Trip> LoadTrips()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			if(totalEntryCount==0)Task.Run(() => LoadStats());
			IEnumerable<Trip> output = cnn.Query<Trip>("select * from TRIPS"+where+orderBy+limit);
			return output.ToList();
		}
	}
	public static int GetCount()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			IEnumerable<int> temp = cnn.Query<int>("SELECT COUNT(*) from TRIPS" + where + orderBy + limit);
			return temp.ToList()[0];

		}
	}
	public static void SaveTrip(Trip trip)
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			cnn.Execute("insert into TRIPS values (" + trip.GetData() + ")", trip);
		}
	}
	public static void SaveTrips(List<Trip> trips)
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
	public static async Task LoadStats()
	{
		totalEntryCount = -1;
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			IEnumerable<int> temp = cnn.Query<int>("SELECT COUNT(*) from TRIPS");
			totalEntryCount = temp.ToList()[0];
			
		}
	}
	public static void TrimDatabase()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			cnn.Execute("delete from TRIPS where distance < 10");
			cnn.Execute("delete from TRIPS where duration < 10");
		}
	}

}