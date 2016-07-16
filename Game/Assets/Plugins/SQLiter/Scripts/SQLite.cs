//----------------------------------------------
// SQLiter
// Copyright © 2014 OuijaPaw Games LLC
//----------------------------------------------

using UnityEngine;
using System.Data;
using Mono.Data.SqliteClient;
using System.IO;

namespace SQLiter
{
    /// <summary>
    /// The idea is that here is a bunch of the basics on using SQLite
    /// Nothing is some advanced course on doing joins and unions and trying to make your infinitely normalized schema work
    /// SQLite is simple.  Very simple.  
    /// Pros:
    /// - Very simple to use
    /// - Very small memory footprint
    /// 
    /// Cons:
    /// - It is a flat file database.  You can change the settings to make it run completely in memory, which will make it even
    /// faster; however, you cannot have separate threads interact with it -ever-, so if you plan on using SQLite for any sort
    /// of multiplayer game and want different Unity instances to interact/read data... they absolutely cannot.
    /// - Doesn't offer as many bells and whistles as other DB systems
    /// - It is awfully slow.  I mean dreadfully slow.  I know "slow" is a relative term, but unless the DB is all in memory, every
    /// time you do a write/delete/update/replace, it has to write to a physical file - since SQLite is just a file based DB.
    /// If you ever do a write and then need to read it shortly after, like .5 to 1 second after... there's a chance it hasn't been
    /// updated yet... and this is local.  So, just make sure you use a coroutine or whatever to make sure data is written before
    /// using it.
    /// 
    /// SQLite is nice for small games, high scores, simple saved, etc.  It is not very secure and not very fast, but it's cheap,
    /// simple, and useful at times.
    /// 
    /// Here are some starting tools and information.  Go explore.
    /// </summary>
    public class SQLite : MonoBehaviour
    {
        public static SQLite Instance = null;
        public bool DebugMode = false;

        /// <summary>
        /// Table name and DB actual file location
        /// </summary>
        private const string SQL_DB_NAME = "PlayersSQLite";

        // feel free to change where the DBs are stored
        // this file will show up in the Unity inspector after a few seconds of running it the first time
        private static readonly string SQL_DB_LOCATION = "URI=file:" 
            + Application.dataPath + Path.DirectorySeparatorChar
            + "Plugins" + Path.DirectorySeparatorChar
            + "SQLiter" + Path.DirectorySeparatorChar
            + "Databases" + Path.DirectorySeparatorChar
            + SQL_DB_NAME + ".db";

        // table name
        private const string SQL_TABLE_NAME = "GenericTableName";

        /// <summary>
        /// predefine columns here to there are no typos
        /// </summary>
        private const string COL_NAME = "name";  // using name as example of primary, unique, key
        private const string COL_LOGIN_LAST = "login";
        private const string COL_RACE = "race";
        private const string COL_CLASS = "class";
        private const string COL_LEVEL = "level";
        private const string COL_XP = "xp";
        private const string COL_GOLD = "gold";

        /// <summary>
        /// DB objects
        /// </summary>
        private IDbConnection mConnection = null;
        private IDbCommand mCommand = null;
        private IDataReader mReader = null;
        private string mSQLString;

        private bool mCreateNewTable = false;

        /// <summary>
        /// Awake will initialize the connection.  
        /// RunAsyncInit is just for show.  You can do the normal SQLiteInit to ensure that it is
        /// initialized during the AWake() phase and everything is ready during the Start() phase
        /// </summary>
        void Awake()
        {
            Debug.Log(SQL_DB_LOCATION);
            Instance = this;
            SQLiteInit();
        }

        void Start()
        {
            // just for testing, uncomment to play with it
            Invoke("Test", 1);
        }

        /// <summary>
        /// Just for testing, but you can see that GetAllPlayers is called -before- the insert player methods,
        /// and returns the data afterwards.
        /// </summary>
        void Test()
        {
            LoomManager.Loom.QueueOnMainThread(() =>
            {
                GetAllPlayers();
            });

            InsertPlayer("Alex", 3, 2, 3, 2, 4, 12);
            InsertPlayer("Bob", 3, 2, 3, 2, 4, 12);
            InsertPlayer("Frank", 3, 2, 3, 2, 4, 12);
            InsertPlayer("Joe", 3, 2, 3, 2, 4, 12);
        }

        /// <summary>
        /// Uncomment if you want to see the time it takes to do things
        /// </summary>
        //void Update()
        //{
        //    Debug.Log(Time.time);
        //}

        /// <summary>
        /// Clean up SQLite Connections, anything else
        /// </summary>
        void OnDestroy()
        {
            SQLiteClose();
        }

        /// <summary>
        /// Example using the Loom to run an asynchronous method on another thread so SQLite lookups
        /// do not block the main Unity thread
        /// </summary>
        public void RunAsyncInit()
        {
            LoomManager.Loom.QueueOnMainThread(() =>
            {
                SQLiteInit();
            });
        }

        /// <summary>
        /// Basic initialization of SQLite
        /// </summary>
        private void SQLiteInit()
        {
            Debug.Log("SQLiter - Opening SQLite Connection");
            mConnection = new SqliteConnection(SQL_DB_LOCATION);
            mCommand = mConnection.CreateCommand();
            mConnection.Open();

            // WAL = write ahead logging, very huge speed increase
            mCommand.CommandText = "PRAGMA journal_mode = WAL;";
            mCommand.ExecuteNonQuery();

            // journal mode = look it up on google, I don't remember
            mCommand.CommandText = "PRAGMA journal_mode";
            mReader = mCommand.ExecuteReader();
            if (DebugMode && mReader.Read())
                Debug.Log("SQLiter - WAL value is: " + mReader.GetString(0));
            mReader.Close();

            // more speed increases
            mCommand.CommandText = "PRAGMA synchronous = OFF";
            mCommand.ExecuteNonQuery();

            // and some more
            mCommand.CommandText = "PRAGMA synchronous";
            mReader = mCommand.ExecuteReader();
            if (DebugMode && mReader.Read())
                Debug.Log("SQLiter - synchronous value is: " + mReader.GetInt32(0));
            mReader.Close();

            // here we check if the table you want to use exists or not.  If it doesn't exist we create it.
            mCommand.CommandText = "SELECT name FROM sqlite_master WHERE name='" + SQL_TABLE_NAME + "'";
            mReader = mCommand.ExecuteReader();
            if (!mReader.Read())
            {
                Debug.Log("SQLiter - Could not find SQLite table " + SQL_TABLE_NAME);
                mCreateNewTable = true;
            }
            mReader.Close();

            // create new table if it wasn't found
            if (mCreateNewTable)
            {
                Debug.Log("SQLiter - Creating new SQLite table " + SQL_TABLE_NAME);

                // insurance policy, drop table
                mCommand.CommandText = "DROP TABLE IF EXISTS " + SQL_TABLE_NAME;
                mCommand.ExecuteNonQuery();

                // create new - SQLite recommendation is to drop table, not clear it
                mSQLString = "CREATE TABLE IF NOT EXISTS " + SQL_TABLE_NAME + " (" +
                    COL_NAME + " TEXT UNIQUE, " +
                    COL_RACE + " INTEGER, " +
                    COL_CLASS + " INTEGER, " +
                    COL_GOLD + " INTEGER, " +
                    COL_LOGIN_LAST + " INTEGER, " +
                    COL_LEVEL + " INTEGER, " +
                    COL_XP + " INTEGER)";
                mCommand.CommandText = mSQLString;
                mCommand.ExecuteNonQuery();
            }
            else
            {
                if (DebugMode)
                    Debug.Log("SQLiter - SQLite table " + SQL_TABLE_NAME + " was found");
            }

            // close connection
            mConnection.Close();
        }

        #region Insert
        /// <summary>
        /// Inserts a player into the database
        /// http://www.sqlite.org/lang_insert.html
        /// name must be unique, it's our primary key
        /// </summary>
        /// <param name="name"></param>
        /// <param name="raceType"></param>
        /// <param name="classType"></param>
        /// <param name="gold"></param>
        /// <param name="login"></param>
        /// <param name="level"></param>
        /// <param name="xp"></param>
        public void InsertPlayer(string name, int raceType, int classType, int gold, int login, int level, int xp)
        {
            name = name.ToLower();

            // note - this will replace any item that already exists, overwriting them.  
            // normal INSERT without the REPLACE will throw an error if an item already exists
            mSQLString = "INSERT OR REPLACE INTO " + SQL_TABLE_NAME
                + " ("
                + COL_NAME + ","
                + COL_RACE + ","
                + COL_CLASS + ","
                + COL_GOLD + ","
                + COL_LOGIN_LAST + ","
                + COL_LEVEL + ","
                + COL_XP
                + ") VALUES ('"
                + name + "',"  // note that string values need quote or double-quote delimiters
                + raceType + ","
                + classType + ","
                + gold + ","
                + login + ","
                + level + ","
                + xp
                + ");";

            if (DebugMode)
                Debug.Log(mSQLString);
            ExecuteNonQuery(mSQLString);
        }

        #endregion

        #region Query Values

        /// <summary>
        /// Quick method to show how you can query everything.  Expland on the query parameters to limit what you're looking for, etc.
        /// </summary>
        public void GetAllPlayers()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            mConnection.Open();

            // if you have a bunch of stuff, this is going to be inefficient and a pain.  it's just for testing/show
            mCommand.CommandText = "SELECT * FROM " + SQL_TABLE_NAME;
            mReader = mCommand.ExecuteReader();
            while (mReader.Read())
            {
                // reuse same stringbuilder
                sb.Length = 0;
                sb.Append(mReader.GetString(0)).Append(" ");
                sb.Append(mReader.GetInt32(1)).Append(" ");
                sb.Append(mReader.GetInt32(2)).Append(" ");
                sb.Append(mReader.GetInt32(3)).Append(" ");
                sb.Append(mReader.GetInt32(4)).Append(" ");
                sb.Append(mReader.GetInt32(5)).Append(" ");
                sb.AppendLine();

                // view our output
                if (DebugMode)
                    Debug.Log(sb.ToString());
            }
            mReader.Close();
            mConnection.Close();
        }

        /// <summary>
        /// Basic get, returning a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetRace(string value)
        {
            return QueryInt(COL_RACE, value);
        }

        /// <summary>
        /// Supply the column and the value you're trying to find, and it will use the primary key to query the result
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string QueryString(string column, string value)
        {
            string text = "Not Found";
            mConnection.Open();
            mCommand.CommandText = "SELECT " + column + " FROM " + SQL_TABLE_NAME + " WHERE " + COL_NAME + "='" + value + "'";
            mReader = mCommand.ExecuteReader();
            if (mReader.Read())
                text = mReader.GetString(0);
            else
                Debug.Log("QueryString - nothing to read...");
            mReader.Close();
            mConnection.Close();
            return text;
        }

        /// <summary>
        /// Supply the column and the value you're trying to find, and it will use the primary key to query the result
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int QueryInt(string column, string value)
        {
            int sel = -1;
            mConnection.Open();
            mCommand.CommandText = "SELECT " + column + " FROM " + SQL_TABLE_NAME + " WHERE " + COL_NAME + "='" + value + "'";
            mReader = mCommand.ExecuteReader();
            if (mReader.Read())
                sel = mReader.GetInt32(0);
            else
                Debug.Log("QueryInt - nothing to read...");
            mReader.Close();
            mConnection.Close();
            return sel;
        }

        /// <summary>
        /// Supply the column and the value you're trying to find, and it will use the primary key to query the result
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public short QueryShort(string column, string value)
        {
            short sel = -1;
            mConnection.Open();
            mCommand.CommandText = "SELECT " + column + " FROM " + SQL_TABLE_NAME + " WHERE " + COL_NAME + "='" + value + "'";
            mReader = mCommand.ExecuteReader();
            if (mReader.Read())
                sel = mReader.GetInt16(0);
            else
                Debug.Log("QueryShort - nothing to read...");
            mReader.Close();
            mConnection.Close();
            return sel;
        }
        #endregion

        #region Update / Replace Values
        /// <summary>
        /// A 'Set' method that will set a column value for a specific player, using their name as the unique primary key
        /// to some value.  This currently just uses 'int' types, but you could modify this to use/do most anything.
        /// Remember strings need single/double quotes around their values
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string column, int value, string name)
        {
            ExecuteNonQuery("UPDATE OR REPLACE " + SQL_TABLE_NAME + " SET " + column + "=" + value + " WHERE " + COL_NAME + "='" + name + "'");
        }

        #endregion

        #region Delete

        /// <summary>
        /// Basic delete, using the name primary key for the 
        /// </summary>
        /// <param name="nameKey"></param>
        public void DeletePlayer(string nameKey)
        {
            ExecuteNonQuery("DELETE FROM " + SQL_TABLE_NAME + " WHERE " + COL_NAME + "='" + nameKey + "'");
        }
        #endregion

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
}
