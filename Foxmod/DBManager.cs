using Plc2DatabaseTool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportTools;

namespace Foxmod
{
    class DBManager : DBBase
    {
        #region Constructors
        public DBManager()
        {
            DataRow dr = Program.settings.Tables["tblDBSettings"].Rows[0];
            DBBase.DBName = (string)dr["fldDBName"];
            DBBase.DBIP = (string)dr["fldServer"];
            DBBase.DBUser = (string)dr["fldAccount"];
            DBBase.DBPass = (string)dr["fldPassword"];
            Init();
        }
        #endregion

        #region Functions
        public List<PLC> GetPlcList()
        {
            //Create a list
            List<PLC> output = new List<PLC>();
            //Get the select query
            string query = GetTablePlcQuery();
            //Read the db
            DataTable qResult = ReadDatabase(query);

            //Create a distinct IPs table
            DataView view = qResult.DefaultView;
            DataTable distinctTable = view.ToTable(true, "fldIP");

            //Create a list of communicators from the table
            PlcManager.communicators = new List<Modbus.ModbusCommunicator>();
            foreach (DataRow row in distinctTable.Rows)
            {
                Modbus.ModbusCommunicator communicator = new Modbus.ModbusCommunicator()
                {
                    IP_Address = (string)row["fldIP"]
                };
                PlcManager.communicators.Add(communicator);
            }

            //For each returned row create a plc object and add it to the list
            foreach (DataRow row in qResult.Rows)
            {
                string ip = (string)row["fldIP"];
                string connType = (string)row["fldConnType"];
                int port = Convert.ToInt32(row["fldPort"]);
                int id = Convert.ToInt32(row["fldID"]);

                //Find the modbus communicator
                Modbus.ModbusCommunicator communicator = PlcManager.communicators.Find(x => x.IP_Address == ip);
                communicator.Type = Convertor.StringToConnType(connType);
                communicator.IP_Port = port;
                
                //Create new device (flowmeter)
                PLC plc = new PLC(communicator, id);
                //Add it to the list
                output.Add(plc);
            }
            return output;
        }

        internal DataTable GetDeviceSettings()
        {
            return ReadDatabase(GetTableSettingsQuery());
        }

        public static string GetInsertIntoQuery(string table)
        {
            return $"INSERT INTO `{DBName}`.`{table}` (fldID, fldDate, fldValue, fldSN) VALUES ";
        }

        public static string GetValues(string serial, int deviceID, int register, PLC plc)
        {
            string output = "";
            int value = Extractor.GetDoubleRegister((register - PlcManager.StartReg), plc.Result);
            output += $"({deviceID}, NOW(), {value}, '{serial}'), ";
            return output;
        }

        public static string AssembleTableQuery(string table)
        {
            string output = DBManager.GetInsertIntoQuery(table);

            //For each row in the settings table add values to the query
            foreach (DataRow row in PlcManager.DeviceSetting.Rows)
            {
                //TODO Optimize
                //Declare variables
                string serial = (string)row["fldSN"];
                int register = Convert.ToInt32(row["fldReg"]);
                int deviceID = Convert.ToInt32(row["fldIndex"]);
                int plcID = (Convert.ToInt32(row["fldPLC"])) - 1;
                PLC plc = PlcManager.Controllers[plcID];

                //If the reading was now successful skip
                if (plc.Result == null) continue;

                //Get values
                output += GetValues(serial, deviceID, register, plc);
            }
            output = output.Remove(output.Length - 2, 2);
            output += ";";
            return output;
        }

        private string GetTableSettingsQuery()
        {
            return $"SELECT * FROM `{DBName}`.`tblwatermetersettings`;";
        }
        private string GetTablePlcQuery()
        {
            return $"SELECT * FROM `{DBName}`.`tblplc`;";
        }
        #endregion

        #region Direct access
        public DataTable ReadDatabase(string query)
        {
            cmd.CommandText = query;
            adapter.SelectCommand = cmd;
            DataTable output = new DataTable();
            try
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                adapter.Fill(output);
            }
            catch (Exception ex)
            {
                Output.Report($"Exception occured while reading from database: '{ex.Message}'");
            }
            return output;
        }

        public bool Execute(string query)
        {
            if (query == string.Empty) return false;
            bool output = false;
            //Execute query            
            cmd.CommandText = query;
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                output = true;
            }
            catch (Exception ex)
            {
                Output.Report($"Exception occured while writing to Database: '{ex.Message}'; {Environment.NewLine}Query: {query}");
            }
            //finally
            //{
            //    Disconnect();
            //}
            return output;
        }

        public void Disconnect()
        {
            try
            {
                conn.Close();
            }
            catch { }
        }
        #endregion
    }
}
