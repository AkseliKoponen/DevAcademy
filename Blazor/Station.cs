using System.Collections.Immutable;
using System.Numerics;
using System.Reflection;

public class Station
{
	
	public int fid{get;set;}
	public int id{get;set;}
	public string name{get;set;}
	public string nameSwe{get;set;}
	public string address{get;set;}
	public string addressSwe{get;set;}
	public string city{get;set;}
	public string citySwe{get;set;}
	public string operatr{get;set;}
	public int capacity{get;set;}
	public float x{get;set;}
	public float y{get;set;}

	public Vector2 location;
	public int totalTripsFrom = 0;
	public int totalTripsTo = 0;
	public int averageDistanceFrom=0;
	public int averageDistanceTo=0;
	public bool loadingInfo = true;
	public List<Station> topStationsTo = new List<Station>();
	public List<Station> topStationsFrom = new List<Station>();
	public Station()
	{

	}
	public Station(string line)
	{
		List<string> data = CSVReader.LineToList(line);
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
		location = new Vector2(x, y);
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
	public Vector2 GetLocation()
	{
		return new Vector2(x,y);
	}

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
	public List<string> GetDisplayedData()
	{
		return new List<string> { id.ToString(), name, address, city, operatr, capacity.ToString(), location.ToString("00.00") };
	}
	public string GetListName(bool swe = false)
	{
		return (swe ? nameSwe : name)+ " – ("+id + ")";
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
	public bool unusedStation = false;
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
	public static Station GetStationByID(int id)
	{
		DBManager.stations.SetWhere("id = " + id);
		List<Station> stations = DBManager.LoadStations();
		if (stations.Count != null && stations.Count > 0)
			return stations[0];
		return null;
	}
	public struct IDCounter
	{
		public int id;
		public int count;
		public IDCounter()
		{
			id = 0;
			count = 0;
		}
		public IDCounter(int _id,int _count)
		{
			id = _id;
			count = _count;
		}
		public static int CompareByCount(IDCounter idc1, IDCounter idc2)
		{
			return idc1.count.CompareTo(idc2.count);
		}
		public void Log()
		{
			Console.WriteLine("StationID "+id+" count is "+count);
		}
	}
	List<Trip> GetJourneysFrom()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("deptStationId = " + id);
		return DBManager.LoadTrips();
	}
	public int GetCountFrom()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("deptStationId = " + id);
		return DBManager.trips.GetCount();
	}
	public int GetCountTo()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("retStationId = " + id);
		return DBManager.trips.GetCount();
	}
	List<Trip> GetJourneysTo()
	{
		DBManager.trips.ResetFilters();
		DBManager.trips.SetWhere("retStationId = " + id);
		return DBManager.LoadTrips();
	}
	public List<string> GetListData()
	{
		return new List<string> { id.ToString(), name, address, capacity.ToString() };
	}
	public static List<string> GetColumnNames(bool displayName = true)
	{
		if (displayName)
			return new List<string> { "ID", "Name", "Address", "Capacity" };
		else
			return new List<string> { "id", "name", "address", "capacity" };
	}
	public static List<string> GetInputFieldNames()
	{
		return new List<string> { "Name", "Address", "City", "Capacity", "Position (X)", "Position (Y)" };
	}
	public static int CompareByID(Station s1, Station s2)
	{
		return s1.id.CompareTo(s2.id);
	}
	public static List<Station> GetAllStations()
	{
		DBManager.stations.Limit(0);
		DBManager.stations.ResetFilters();
		DBManager.stations.OrderBy("name");
		return DBManager.LoadStations();
	}
}