using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.Intrinsics.Arm;
using Dapper;

public static class DBManager
{
	static string connStr = "Data Source=.\\biketrips.db;Version=3;";
	static string orderBy = " ORDER BY deptTime";
	public static List<Trip> OrderBy(List<string> columns)
	{
		if (columns == null || columns.Count == 0)
		{
			orderBy = " ORDER BY deptTime";
		}
		else {
			string str = " ORDER BY ";
			foreach (string column in columns)
			{
				str += column + ",";
			}
			orderBy = str.Substring(0, str.Length - 1);
		}
		return LoadTrips();
	}
	public static List<Trip> LoadTrips()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			IEnumerable<Trip> output = cnn.Query<Trip>("select * from TRIPS"+orderBy+" LIMIT 50");
			return output.ToList();
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
	public static void TrimDatabase()
	{
		using (IDbConnection cnn = new SQLiteConnection(connStr))
		{
			cnn.Execute("delete from TRIPS where distance < 10");
			cnn.Execute("delete from TRIPS where duration < 10");
		}
	}

}