using UnityEngine;
using System;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;
using System.Text;

public class TestDB : MonoBehaviour {

	public static TestDB Instance = null;

	/// <summary>
	/// DB actual file location
	/// </summary>
	private const string SQL_DB_NAME = "Cards";


	/// <summary> Tables </summary>
	private const string SQL_Card_Table = "Card";
	private const string SQL_Ability_Table = "Ability";
	private const string SQL_Card_Type_Table = "Card_Type";


	/// <summary>
	/// predefine columns here to there are no typos
	/// </summary>
	private const string COL_Card_ID = "card_id";
	private const string COL_Ab_ID = "ability_id";
	private const string COL_Ab1_ID = "ability1_id";
	private const string COL_Ab2_ID = "ability2_id";

	private const string COL_Name = "name";
	private const string COL_Image = "image_path";
	private const string COL_HP = "hp";
	private const string COL_AP = "ap";
	private const string COL_Type = "type";

	private const string COL_CT_ID = "card_type_id";
	private const string COL_Card_Type = "card_type";
	private const string COL_Ability = "ability";
	private const string COL_Ability_Desc = "ability_desc";

	/// <summary>
	/// Card Image Paths
	/// </summary>
	private static readonly string Card_IM_Path = "Card_Images" + Path.DirectorySeparatorChar;


	/// <summary>
	/// DB objects
	/// </summary>
	private IDbConnection mConnection = null;
	private IDbCommand mCommand = null;
	private IDataReader mReader = null;
	private string mSQLString;


	/// <summary>
	/// Location of Database
	/// </summary>
	private static readonly string SQL_DB_LOCATION = "URI=file:"
		+ Application.dataPath + Path.DirectorySeparatorChar
		+ "Databases" + Path.DirectorySeparatorChar
		+ SQL_DB_NAME + ".db";


	void Awake()
	{
		Debug.Log(SQL_DB_LOCATION);
		Instance = this;
		SQLiteInit();
	}

	void OnDestroy()
	{
		SQLiteClose();
	}

	/// <summary>
	/// Basic initialization of SQLite
	/// </summary>
	private void SQLiteInit(){
		//Open The Database
		Debug.Log("SQLiter - Opening SQLite Connection at " + SQL_DB_LOCATION);
		mConnection = new SqliteConnection(SQL_DB_LOCATION);
		mCommand = mConnection.CreateCommand();

		mConnection.Open();

		mCommand.CommandText = "PRAGMA foreign_keys = ON";
		mCommand.ExecuteNonQuery();

		Debug.Log ("Testing the Command:");
		getCards ();

		Debug.Log("SQLiter - Closing SQLite Connection");
		mConnection.Close();

	}

	/// <summary>
	/// Get Info for cards from database
	/// </summary>
	private void getCards(){

		mCommand.CommandText = "select * from card";
		mReader = mCommand.ExecuteReader();

		while (mReader.Read ()) {

			int id = Int32.Parse (mReader.GetString (0));			//card_id
			string card_name = mReader.GetString (1);				//name
			string fpath = Card_IM_Path + mReader.GetString (2);	//image_path
			int abId1 = Int32.Parse (mReader.GetString (3));		//ability1_id
			int abId2 = Int32.Parse (mReader.GetString (4));		//ability2_id
			int hp = Int32.Parse(mReader.GetString (5));			//hp
			int ap = Int32.Parse(mReader.GetString (6));			//ap
			int type = Int32.Parse(mReader.GetString (7));			//type

			Init.insertImage(id, fpath);
			Init.newCard (id, card_name, abId1, abId2, hp, ap, type);
		//	Debug.Log(sb.ToString());
		}
	}

	public string getCardType(bool ability, int typeID){
		string column, idCol, result;
		if (ability) {
			column = COL_Ability;
			idCol = COL_Ab_ID;
		} else {
			column = COL_Card_Type;
			idCol = COL_CT_ID;
		}

		mConnection.Open ();
		mCommand.CommandText = "select " + column
			+ " from " + SQL_Card_Type_Table + " natural join " + SQL_Ability_Table
			+ " where " + idCol + "=" + typeID;

		//Debug.Log (mCommand.CommandText);
		mReader = mCommand.ExecuteReader ();
		mReader.Read ();
	
		result = mReader.GetString(0);

		mConnection.Close ();
		return result;
	}


	public string getAbility(int abID){
		string ability;
		mConnection.Open ();
		mCommand.CommandText = "select " + COL_Ability_Desc + " from " + SQL_Ability_Table
			+ " where " + COL_Ab_ID + "=" + abID;
		Debug.Log (mCommand.CommandText);
		mReader = mCommand.ExecuteReader ();
		mReader.Read ();

		ability = mReader.GetString (0);
		mConnection.Close ();
		return ability;
	}

	public void searchDB(string feed, string limit){
		feed = "'%" + feed + "%'";
		int id;
		mConnection.Open ();
		mCommand.CommandText = "select " + COL_Card_ID
			+ " from " + SQL_Card_Table + " as c join " + SQL_Ability_Table + " as a on c."
			+ COL_Ab1_ID +"=a." + COL_Ab_ID + " or c." + COL_Ab2_ID + "=a." + COL_Ab_ID
			+ " where (" + COL_Name + " like " + feed + " or " + COL_Ability + " like " + feed
			+ ") and " + COL_CT_ID + limit;
		
		Debug.Log (mCommand.CommandText);
		mReader = mCommand.ExecuteReader();

		while (mReader.Read ()) {
			id = Int32.Parse (mReader.GetString (0));
			GameObject.Find ("CardGrid").GetComponent<GridScript> ().setActiveChild (id);
		}
		mConnection.Close ();
	}


	/// <summary>
	/// Basic execute command - open, create command, execute, close
	/// </summary>
	/// <param name="commandText"></param>
	public void ExecuteNonQuery(string commandText)
	{
		mConnection.Open();
		mCommand.CommandText = commandText;
		mCommand.ExecuteNonQuery();
		mConnection.Close();
	}

	/// <summary>
	/// Clean up everything for SQLite
	/// </summary>
	private void SQLiteClose()
	{
		if (mReader != null && !mReader.IsClosed)
			mReader.Close();
		mReader = null;

		if (mCommand != null)
			mCommand.Dispose();
		mCommand = null;

		if (mConnection != null && mConnection.State != ConnectionState.Closed)
			mConnection.Close();
		mConnection = null;
	}
}
