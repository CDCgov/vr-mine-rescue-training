using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/* TODO:
 * CSV Export Function
 * */

/* //comment
 * code      
 * 
 * This comment is for the next block of code
 * 
 * code //comment
 * 
 * This comment is for the current line of code
 * 
 * */

public class GenerateConnectionTable
{
	public static Dictionary<string, List<TableInfoContainer>> MatchMap;
	public static Dictionary<TableInfoContainer, List<TableInfoContainer>> ExportMap;

	[MenuItem("Tools/Generate Connection Table")] //menu option


	private static void NewMenuOption()
	{
		string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		string filename = "\\MineSegmentLogicTable.csv";
		string writepath = desktop + filename; //CSV write path
		CreateMatchMap();
		CSVExport(ExportMap, writepath);
	}

	public static void CreateMatchMap()
	{
		string path = @"Assets/Tilesets/UGCoalAdvTiles2/"; //Prefab path - D, Problem, we may need to address how it will look for other prefab paths (i.e. a UGCoal 3, etc) -B
		string[] files = Directory.GetFiles(path, "*.prefab*"); //getting paths of all the files and storing them in an array
		//List<MineSegment> mineSegmentList = new List<MineSegment>();
		List<GameObject> gameObjList = new List<GameObject>(); 
		for (int i = 0; i < files.Length; i++)
		{
			/*
			UnityEngine.Object[] tempArr = AssetDatabase.LoadAllAssetsAtPath(files[i]); //load all objects at path into object array
			foreach (UnityEngine.Object o in tempArr)
			{

				MineSegment obj = o as MineSegment; //pick out all the MineSegment objects
				if (obj)
				{

					mineSegmentList.Add(obj); //add them to list
				}
			}*/
			GameObject gobj = AssetDatabase.LoadMainAssetAtPath(files[i]) as GameObject;
			if (gobj != null)
			{
				if (gobj.GetComponent<MineSegment>() != null)
				{
					gameObjList.Add(gobj);
				}
			}
		}

		MatchMap = new Dictionary<string, List<TableInfoContainer>>(); //map to store matches
		ExportMap = new Dictionary<TableInfoContainer, List<TableInfoContainer>>(); //map to export to CSV

		//iterating through every possible segment connection
		foreach (GameObject gameObj in gameObjList)
		{
			MineSegment o = gameObj.GetComponent<MineSegment>();
			//GameObject segmentGeometryPrefab = o.SegmentGeometryPrefab;
			//SegmentGeometry geometry = segmentGeometryPrefab.GetComponent<SegmentGeometry>();
			SegmentGeometry geometry = o.SegmentGeometry;
			if (geometry != null)
			{
				for (int i = 0; i < geometry.SegmentConnections.Length; i++) //iterating through each segment connection of the prefab
				{
					string[] tokens = geometry.SegmentConnections[i].ConnectionID.Split('_');
					TableInfoContainer curContainer = new TableInfoContainer(gameObj, geometry.SegmentConnections[i]); //generating key using current Segment Connection and Mine Segment
					List<TableInfoContainer> matchedContainer = Matches(gameObjList, tokens[1], tokens[2]); //generating value using Matches function
					if (!MatchMap.ContainsKey(geometry.SegmentConnections[i].ConnectionID))
					{
						MatchMap.Add(geometry.SegmentConnections[i].ConnectionID, matchedContainer); //adding key,value pair to map
					}
					ExportMap.Add(curContainer, matchedContainer);
				}
			}
		}
		//toString();
	}

	public static void toString()
	{
		//debugging purposes
		foreach (KeyValuePair<string, List<TableInfoContainer>> entry in MatchMap)
		{
			Debug.Log("Connection ID: " + entry.Key + " Matches with: ");
			foreach (TableInfoContainer container in entry.Value)
			{
				Debug.Log(container.GameObj.GetComponent<MineSegment>().name + ", " + container.SegmentConnection.ConnectionID);
			}
		}
	}

	//Get the MatchMap dictionary
	public static Dictionary<string, List<TableInfoContainer>> GetMatchMap ()
	{
		if (MatchMap == null)
		{
			CreateMatchMap();
		}
		return MatchMap;
	}
	//Matches a segment connection ID with all possible segment connections
	//Returns a list of all Mine Segment + Segment Connection ID pairs (see class tableInfoContainer for details)
	public static List<TableInfoContainer> Matches(List<GameObject> gameObjectList, string rockdustlevel, string orientation)
	{
		TableInfoContainer returnTableInfo = new TableInfoContainer(); 
		List<TableInfoContainer> matchedConnections = new List<TableInfoContainer>(); //initializing return value

		//Just like before, iterating through every segment connection
		foreach (GameObject gameObj in gameObjectList)
		{
			MineSegment o = gameObj.GetComponent<MineSegment>();
			//GameObject segmentGeometryPrefab = o.SegmentGeometryPrefab;
			//SegmentGeometry geometry = segmentGeometryPrefab.GetComponent<SegmentGeometry>();
			SegmentGeometry geometry = o.SegmentGeometry;
			if (geometry != null)
			{
				for (int i = 0; i < geometry.SegmentConnections.Length; i++)
				{
					bool matched = false;
					
					string[] tokens = geometry.SegmentConnections[i].ConnectionID.Split('_'); //tokenizing connection ID

					//checks if these IDs match, this will need to be expanded in the future
					if (orientation.Equals("EW1A") && tokens[2].Equals("EW1B")) 
					{
						matched = true;
					}
					else if (orientation.Equals("EW1B") && tokens[2].Equals("EW1A"))
					{
						matched = true;
					}
					else if (orientation.Equals("NS1A") && tokens[2].Equals("NS1B"))
					{
						matched = true;
					}
					else if (orientation.Equals("NS1B") && tokens[2].Equals("NS1A"))
					{
						matched = true;
					}

					if (rockdustlevel.Equals(tokens[1]) && matched) //checks if rockdust matches
					{
						returnTableInfo = new TableInfoContainer(gameObj, geometry.SegmentConnections[i]);
						matchedConnections.Add(returnTableInfo); //adding to list of matching segment connections
					}
				}
			}
		}
		return matchedConnections;
	}

	//Exports logic table to CSV file
	public static void CSVExport(Dictionary<TableInfoContainer, List<TableInfoContainer>> matchMap, string path)
	{
		System.IO.StreamWriter objWriter;
		objWriter = new System.IO.StreamWriter(path,false);
		objWriter.Write("Segment Prefab,");
		objWriter.Write("Type,");
		objWriter.Write("Rock Dust Level,");
		objWriter.Write("Connection ID,");
		objWriter.Write("Conn. Centroid,");
		objWriter.Write("Conn. Normal,");
		objWriter.Write("Matching Connection ID,");
		objWriter.Write("\n");
		foreach (KeyValuePair<TableInfoContainer, List<TableInfoContainer>> entry in matchMap)
		{
			CSVContainer rowToExport = new CSVContainer(entry.Key);
			objWriter.Write(rowToExport.segmentPrefab + ',');
			objWriter.Write(rowToExport.type + ',');
			objWriter.Write(rowToExport.rockDustLevel + ',');
			objWriter.Write(rowToExport.connectionID + ',');
			objWriter.Write(rowToExport.connCentroid + ',');
			objWriter.Write(rowToExport.connNormal + ',');
			objWriter.Write(rowToExport.matchingID + ',');
			objWriter.Write("\n");
		}
		objWriter.Close();
	}

	//Class that stores a Mine Segment and Segment Connection
	public class TableInfoContainer
	{
		public GameObject GameObj;
		public SegmentConnection SegmentConnection;
		public TableInfoContainer()
		{
			this.GameObj = null;
			this.SegmentConnection = null;
		}
		public TableInfoContainer(GameObject gameObj, SegmentConnection SegmentConnection)
		{
			this.GameObj = gameObj;
			this.SegmentConnection = SegmentConnection;
		}
		public string toString()
		{
			return this.GameObj.GetComponent<MineSegment>().name + " " +  SegmentConnection.ConnectionID;
		}
	}
	
	//Class that stores all the information needed to assemble the logic table
	public class CSVContainer
	{
		public string segmentPrefab;
		public string type;
		public string rockDustLevel;
		public string connectionID;
		public string connCentroid;
		public string connNormal;
		public string matchingID;

		//processing information into strings
		public CSVContainer (TableInfoContainer tableInfo)
		{
			this.segmentPrefab = tableInfo.GameObj.GetComponent<MineSegment>().name;
			if (tableInfo.GameObj.GetComponent<MineSegment>().name.Contains("4Way"))
			{
				type = "4 Way Intersection";
			} else if (tableInfo.GameObj.GetComponent<MineSegment>().name.Contains("Corner")) {
				type = "Corner";
			} else if (tableInfo.GameObj.GetComponent<MineSegment>().name.Contains("End"))
			{
				type = "Endcap";
			} else if (tableInfo.GameObj.GetComponent<MineSegment>().name.Contains("Straight"))
			{
				type = "Straight";
			} else if (tableInfo.GameObj.GetComponent<MineSegment>().name.Contains("T_"))
			{
				type = "T section";
			}
			//string[] tokens = tableInfo.GameObj.GetComponent<MineSegment>().name.Split('-');
			//this.rockDustLevel = tokens[tokens.Length - 2];  
			string[] tokens = tableInfo.SegmentConnection.ConnectionID.Split('_');
			string dust = "";
			switch (tokens[1])
			{
				case "N":
					dust = "None";
					break;
				case "M":
					dust = "Medium";
					break;
				case "F":
					dust = "Full";
					break;
				default:
					break;
			}
			this.rockDustLevel = dust;
			this.connectionID = tableInfo.SegmentConnection.ConnectionID;
			this.connCentroid = ("\"" + tableInfo.SegmentConnection.Centroid.ToString() + "\"");
			this.connNormal = ("\"" + tableInfo.SegmentConnection.Normal.ToString() + "\"");
			string temp = tableInfo.SegmentConnection.ConnectionID;
			if (tableInfo.SegmentConnection.ConnectionID.EndsWith("A"))
			{
				temp = temp.TrimEnd('A');
				temp = temp + "B";
			} else if (tableInfo.SegmentConnection.ConnectionID.EndsWith("B"))
			{
				temp = temp.TrimEnd('B');
				temp = temp + "A";
			}
			this.matchingID = temp;
		}
	}
}
