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
	public static List<string> GetColumnNames()
	{
		return new List<string> {"id","Name","Address","City","Operator","Capacity","Location (xx.xx,yy.yy)" };
	}
	public List<string> GetDisplayedData()
	{
		return new List<string> { id.ToString(), name, address, city, operatr, capacity.ToString(), location.ToString("00.00") };
	}
	public string GetListName(bool swe = false)
	{
		return id + " " + (swe ? nameSwe : name);
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
}