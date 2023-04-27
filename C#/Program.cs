using System.Formats.Asn1;
using System.Globalization;
using System.Xml.Serialization;


namespace DevAcademy
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using System.Data.SQLite;
	using System.Text;
	using System.Threading.Tasks;
	using System.Net.Http;
	using System.Reflection;
	using static System.Runtime.InteropServices.JavaScript.JSType;
	using Microsoft.EntityFrameworkCore;
	using System.Diagnostics;

	internal class Program
	{
		public async static Task Main(string[] args)
		{
			string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName+"\\";
			List<Trip> trips = DBManager.LoadTrips();
			Console.WriteLine("Database contains a total of " + trips.Count + " trips.");

			DisplayTrips();
			void DisplayTrips()
			{
				while (true)
				{
					Console.WriteLine("Type index of trip to view info (0 - " + (trips.Count - 1) + ")");
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
		}


	}
}