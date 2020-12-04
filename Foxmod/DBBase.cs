using ConnectionTools;
using MySql.Data.MySqlClient;

namespace Plc2DatabaseTool
{
    /// <summary>
    /// Manages the database, call Init() after any parameter change to apply changes
    /// </summary>
    public class DBBase
    {
        protected DBBase() { }

        public static string DBName { get; set; }

        public static string DBIP { get; set; }

        public static string DBUser { get; set; }

        public static string DBPass { get; set; }

        protected MySqlConnection conn;
        protected MySqlCommand cmd;
        protected MySqlDataAdapter adapter;

        /// <summary>
        /// Call this function to Initialize the MySql objects or apply new settings
        /// </summary>
        public void Init()
        {
            string connString = DataBaseTools.GenerateConnectionString(DBIP, DBUser, DBPass);
            conn = new MySqlConnection(connString);
            cmd = new MySqlCommand() { Connection = conn };
            adapter = new MySqlDataAdapter();
        }
    }
}
