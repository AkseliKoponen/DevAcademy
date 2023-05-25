using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Stations are one of the values stored in the database.
/// </summary>
public class Station
{
	/// <summary>
	/// fid of the station(Database value)
	/// </summary>
	public int fid{get;set;}

	/// <summary>
	/// id of the station(Database value)
	/// </summary>
	public int id{get;set;}

	/// <summary>
	/// name of the station (Database value)
	/// </summary>
	public string name{get;set; }

	/// <summary>
	/// name of the station in swedish (Database value)
	/// </summary>
	public string nameSwe{get;set; }

	/// <summary>
	/// address of the station. (Database value)
	/// <br>Note: contains a comma and usually follow the format of (station name, street address)</br>
	/// </summary>
	public string address{get;set; }

	/// <summary>
	/// address of the station in swedish. (Database value)
	/// <br>Note: contains a comma and usually follow the format of (station name, street address)</br>
	/// </summary>
	public string addressSwe{get;set; }

	/// <summary>
	/// The name of the city where the station exists (Database value)
	/// </summary>
	public string city{get;set; }

	/// <summary>
	/// The swedish name of the city where the station exists (Database value)
	/// </summary>
	public string citySwe{get;set; }

	/// <summary>
	/// The operator of the station (Database value)
	/// <br>Note:Most records do not have an operator</br>
	/// </summary>
	public string operatr{get;set; }

	/// <summary>
	/// How many bikes can the station hold (Database value)
	/// </summary>
	public int capacity{get;set; }

	/// <summary>
	/// Latitude of the geocoordinates of the station (Database value)
	/// </summary>
	public float x{get;set; }

	/// <summary>
	/// Longitude of the geocoordinates of the station (Database value)
	/// </summary>
	public float y{get;set; }

	#region Additional Info for StationView

	/// <summary>
	/// The amount of trips departing from this station
	/// </summary>
	public int totalTripsFrom = 0;

	/// <summary>
	/// The amount of trips arriving to this station
	/// </summary>
	public int totalTripsTo = 0;

	/// <summary>
	/// The average distance traveled when departing from this station
	/// </summary>
	public int averageDistanceFrom=0;

	/// <summary>
	/// The average distance traveled when returning to this station
	/// </summary>
	public int averageDistanceTo=0;

	/// <summary>
	/// Used by StationView to see if CalculateInfo() is still running
	/// </summary>
	public bool loadingInfo = true;

	/// <summary>
	/// Most common destinations when departing from this station
	/// </summary>
	public List<Station> topStationsTo = new List<Station>();

	/// <summary>
	/// Most common origin stations when arriving to this station
	/// </summary>
	public List<Station> topStationsFrom = new List<Station>();

	/// <summary>
	/// if there are less than 5 trips linked to this station, mark this station as "unused"
	/// </summary>
	public bool unusedStation = false;
	#endregion

	/// <summary>
	/// 
	/// </summary>
	public Station()
	{

	}
	/// <summary>
	/// Creates a station by slicing the .csv <paramref name="line"/> into values
	/// </summary>
	public Station(string line)
	{
		List<string> data = DBManager.LineToList(line);
		PropertyInfo[] pis = GetType().GetProperties();
		data = RemoveIrregs(data);
		//Irregularities in station CSV file: the finnish address field contains a comma -> merge data[4] and [5]
		//Most of the stations are missing operator and capacity
		
		for (int i = 0; i < data.Count && i < pis.Length; i++)
		{
			string ptn = pis[i].PropertyType.Name.ToLower();
			switch (ptn)
			{
				default:
					Console.WriteLine("Unimplemented propertyType (" + ptn + ") on " + pis[i].Name);
					break;
				case "string":
					pis[i].SetValue(this, data[i]);
					break;
				case "int32":
					if (data[i] == "")
						pis[i].SetValue(this, 0);
					else
					{
						try { pis[i].SetValue(this, int.Parse(data[i])); }
						catch { Console.WriteLine("Could not (int)parse '" + data[i] + "' on " + pis[i].Name); }
					}
					break;
				case "single":
					if (data[i] == "")
						pis[i].SetValue(this, 0);
					else
					{
						try { pis[i].SetValue(this, float.Parse(data[i])); }
						catch { Console.WriteLine("Could not (float)parse '" + data[i] + "' on " + pis[i].Name); }
					}
					break;
			}
		}
		List<string> RemoveIrregs(List<string> originalData)
		{
			List<string> temp = new List<string>();
			temp.AddRange(originalData.GetRange(0, 4));
			temp.Add(originalData[4] + ", " + originalData[5]);
			temp.Add(originalData[4] + ", " + originalData[6]);
			temp.AddRange(originalData.GetRange(7, originalData.Count - 7));
			return temp;
		}
	}


	/// <summary>
	/// Get distance from this station to the <paramref name="other"/> station
	/// </summary>
	public double GetDistanceTo(Station other)
	{

		double cdMultip = 0.0174532925199433;

		double latitude = y * cdMultip;
		double longitude = x * cdMultip;
		double num = other.y * cdMultip;
		double longitude1 = other.x * cdMultip;
		double num1 = longitude1 - longitude;
		double num2 = num - latitude;
		double num3 = Math.Pow(Math.Sin(num2 / 2), 2) + Math.Cos(latitude) * Math.Cos(num) * Math.Pow(Math.Sin(num1 / 2), 2);
		double num4 = 2 * Math.Atan2(Math.Sqrt(num3), Math.Sqrt(1 - num3));
		double num5 = 6376500 * num4;
		return num5;
		
	}

	/// <summary>
	/// Get name with id
	/// </summary>
	public string GetListName(bool swe = false)
	{
		return (swe ? nameSwe : name)+ " – ("+id + ")";
	}
	/// <summary>
	/// Get all data in one string
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
	/// Calculate the additional info for StationView Page.
	/// <br>Specify <paramref name="filterMonth"/> to get data only from a specific month.</br>
	/// </summary>
	public void CalculateInfo(int filterMonth = default)
	{
		if (loadingInfo == false && filterMonth==default)
			return;
		loadingInfo = true;
		DBManager.trips.Limit(0);
		List<Trip> tripsTo = GetJourneysTo();
		List<Trip> tripsFrom = GetJourneysFrom();
		if (tripsTo.Count<5 && tripsFrom.Count < 5)
		{
			loadingInfo = false;
			unusedStation = true;
		}
		if (filterMonth != default)
		{
			//if filtering by month, delete trips that did not depart at filterMonth
			for (int i = tripsTo.Count - 1; i >= 0; i--)
			{

				if (tripsTo[i].GetDeptTime().Month != filterMonth)
					tripsTo.RemoveAt(i);
			}

			for (int i = tripsFrom.Count - 1; i >= 0; i--)
			{

				if (tripsFrom[i].GetDeptTime().Month != filterMonth)
					tripsFrom.RemoveAt(i);
			}
		}
		
		totalTripsFrom =  tripsFrom.Count;
		totalTripsTo = tripsTo.Count;
		averageDistanceFrom = GetAverageDistance(tripsFrom);
		averageDistanceTo = GetAverageDistance(tripsTo);
		topStationsFrom = GetTopStations(tripsFrom, false);
		topStationsTo = GetTopStations(tripsTo, true);

		loadingInfo = false;
		int GetAverageDistance(List<Trip> trips)
		{

			int total = 0;
			int validTrips = 0;
			foreach (Trip t in trips)
			{
				if (t.distance == null)
				{
					continue;
				}
				total += (int)t.distance;
				validTrips++;
			}
			return (int)((float)total / (float)validTrips);
		}
		List<Station> GetTopStations(List<Trip> trips, bool from)
		{
			List<IDCounter> idcounters = new List<IDCounter>();
			DBManager.stations.ResetFilters();
			DBManager.stations.Limit(0);
			
			foreach (Station st in DBManager.LoadStations())
			{
				int matchcount = 0;
				for (int i = trips.Count - 1; i >= 0; i--)
				{
					if (from)
					{
						if (trips[i].deptStationId == st.id)
						{
							matchcount++;
							trips.RemoveAt(i);
						}
					}
					else
					{
						if (trips[i].retStationId == st.id)
						{
							matchcount++;
							trips.RemoveAt(i);
						}
					}
				}
				idcounters.Add(new IDCounter(st.id, matchcount));
				//idcounters.Add(new IDCounter(st.id, trips.Where(t => (from?t.deptStationId:t.retStationId) == st.id).ToList().Count));
			}
			idcounters.Sort(IDCounter.CompareByCount);
			List<Station> sts = new List<Station>();
			DBManager.stations.Limit(1);
			for (int i = 0; i < 5 && i<idcounters.Count; i++)
			{
				if (idcounters[i].id == id) idcounters.RemoveAt(i);	//Disregard trips that start and end at the same station
				sts.Add(GetStationByID(idcounters[i].id));
			}
			return sts;
		}
	}
	
	/// <summary>
	/// Get a station whose id matches the given <paramref name="id"/>.
	/// </summary>
	public static Station GetStationByID(int id)
	{
		DBManager.stations.SetWhere("id = " + id);
		List<Station> stations = DBManager.LoadStations();
		if (stations.Count != null && stations.Count > 0)
			return stations[0];
		return null;
	}

	/// <summary>
	/// Struct for counting how many trips are connected to a station with a specific ID
	/// </summary>
	public struct IDCounter
	{
		/// <summary>
		/// ID of the station
		/// </summary>
		public int id;

		/// <summary>
		/// How many stations match the ID
		/// </summary>
		public int count;

		/// <summary>
		/// Constructor for IDCounter
		/// </summary>
		/// <param name="_id">id</param>
		/// <param name="_count">count</param>
		public IDCounter(int _id,int _count)
		{
			id = _id;
			count = _count;
		}

		/// <summary>
		/// Used for sorting a list of IDCounters by their count
		/// </summary>
		public static int CompareByCount(IDCounter idc1, IDCounter idc2)
		{
			return idc2.count.CompareTo(idc1.count);
		}
	}

	/// <summary>
	/// Returns all trips departing from this station
	/// </summary>
	List<Trip> GetJourneysFrom()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("deptStationId = " + id);
		return DBManager.LoadTrips();
	}

	/// <summary>
	/// Get the amount of trips departing from this station.
	/// <br>Faster than GetJourneysFrom().count</br>
	/// </summary>
	public int GetCountFrom()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("deptStationId = " + id);
		return DBManager.trips.GetCount();
	}

	/// <summary>
	/// Get the amount of trips ending at this station
	/// <br>Faster than GetJourneysTo().count</br>
	/// </summary>
	public int GetCountTo()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("retStationId = " + id);
		return DBManager.trips.GetCount();
	} 
	/// <summary>
	/// Returns all trips ending at this station
	/// </summary>
	List<Trip> GetJourneysTo()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("retStationId = " + id);
		return DBManager.LoadTrips();
	}

	/// <summary>
	///	Get the data to be displayed in ListView
	/// </summary>
	public List<string> GetListData()
	{
		return new List<string> { id.ToString(), name, address, capacity.ToString() };
	}

	/// <summary>
	///	Get the names of columns displayed in ListView.
	///	<br>Set <paramref name="displayName"/> as false to get the names of the corresponding properties</br>
	/// </summary>
	public static List<string> GetColumnNames(bool displayName = true)
	{
		if (displayName)
			return new List<string> { "ID", "Name", "Address", "Capacity" };
		else
			return new List<string> { "id", "name", "address", "capacity" };
	}
	/// <summary>
	///	Get the names of the fields assigned in InsertForm page
	/// </summary>
	public static List<string> GetInputFieldNames()
	{
		return new List<string> { "Name", "Address", "City", "Capacity", "Position (X)", "Position (Y)" };
	}
	/// <summary>
	/// Used when sorting a list of Stations
	/// </summary>
	public static int CompareByID(Station s1, Station s2)
	{
		return s1.id.CompareTo(s2.id);
	}
	/// <summary>
	///	Get ALL stations in DBManager
	/// </summary>
	public static List<Station> GetAllStations()
	{
		DBManager.stations.Limit(0);
		DBManager.stations.ResetFilters();
		DBManager.stations.OrderBy("name");
		return DBManager.LoadStations();
	}
}