using System;
using System.Linq;
using ReportTools;
using MySql.Data;
using System.Data;
using Plc2DatabaseTool;
using System.Timers;
using System.Threading;

namespace Foxmod
{
    class Program
    {
        public static DataSet settings;
        private static System.Timers.Timer timer;
        private static int lastHour;

        static void Main(string[] args)
        {
            Initialize();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private static void Initialize()
        {
            settings = new DataSet();
            settings.ReadXml($"{AppDomain.CurrentDomain.BaseDirectory}Settings.xml");

            //Read the database 
            DBManager db = new DBManager();
            PlcManager.Controllers = db.GetPlcList();
            PlcManager.DistinctIPs = PlcManager.Controllers.Select(i => i.Ip).Distinct().ToList();
            PlcManager.DeviceSetting = db.GetDeviceSettings();

            //Disconnect from the database
            db.Disconnect();

            //Get general settings
            DataRow generalSettings = settings.Tables["tblGeneral"].Rows[0];
            PlcManager.StartReg = Convert.ToInt32(generalSettings["fldStartRegister"]);
            PlcManager.AmoutToRead = Convert.ToInt32(generalSettings["fldAmountToRead"]);

            //Set the timer
            timer = new System.Timers.Timer(5000);
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            timer.AutoReset = true;

            //Set to -1 to avoid reading skip, if the app is started at midnight
            lastHour = -1;

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int hour = DateTime.Now.Hour;
            if (hour != lastHour)
            {
                lastHour = hour;
                ReadingType rType = hour == 0 ? ReadingType.Daily : ReadingType.Hourly;

                //Try 3 times
                Output.Report("New Reading Started");

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        if (i > 0) Output.Report($"Retry atempt: {i}");
                        //Create tasks
                        PlcManager.CreateTasks();
                        //Start the tasks
                        PlcManager.StartReading(rType);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Output.Report($"Reading failed: {ex.Message}");
                        //Wait a bit and try again
                        Thread.Sleep(2000);
                    }
                }
            }
        }
    }
}
