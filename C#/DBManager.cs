using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DevAcademy
{

	public static class DBManager
	{
		static string connStr = "Data Source=.\\biketrips.db;Version=3;";
		public static List<Trip> LoadTrips(DynamicParameters dpm =default)
		{
			if (dpm == default)
				dpm = new DynamicParameters();
			using (IDbConnection cnn = new SQLiteConnection(connStr))
			{
				IEnumerable<Trip> output = cnn.Query<Trip>("select * from TRIPS", dpm);
				return output.ToList();
			}
		}
		public static void SaveTrip(Trip trip)
		{
			using (IDbConnection cnn = new SQLiteConnection(connStr))
			{
				cnn.Execute("insert into TRIPS " /*+ Trip.GetColumns()*/ + " values (" + trip.GetData() + ")", trip);
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
						cnn.Execute("insert into TRIPS " /*+ Trip.GetColumns()*/ + " values (" + trip.GetData() + ")", trip);
						trips.RemoveAt(0);
						counter++;
					}
				}
				totalCounter += counter;
				counter = 0;
				Console.WriteLine("--"+totalCounter+" of "+totalTripCount +" saved ("+(float)totalCounter/(float)totalTripCount*100+"%)!");
				
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
}
