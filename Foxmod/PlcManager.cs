using ReportTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Foxmod
{
    class PlcManager
    {
        public static List<PLC> Controllers { get; set; }
        public static DataTable DeviceSetting { get; set; }
        public static List<string> DistinctIPs { get; set; }
        public static int StartReg { get; set; }
        public static int AmoutToRead { get; set; }
        public static List<Modbus.ModbusCommunicator> communicators { get; set; }

        private static List<Task> tasks;

        public static void CreateTasks()
        {
            tasks = new List<Task>();
            foreach (string ip in DistinctIPs)
            {
                Task task = new Task(() => ReadControllers(ip));
                tasks.Add(task);
            }
        }

        public static void StartReading(ReadingType rType)
        {
            foreach (Task task in tasks)
            {
                //Start the thread
                task.Start();
                try
                {
                    task.Wait();
                }
                catch { }
                Thread.Sleep(100);
            }

            Task t = Task.WhenAll(tasks);
            try
            {
                t.Wait();
            }
            catch { }

            //TODO Move this code to higher level
            if (t.IsCompleted)
            {
                string query = "";
                switch (rType)
                {
                    case ReadingType.Hourly:
                        {
                            query += DBManager.AssembleTableQuery("tblwatermeters1h");

                        }
                        break;
                    case ReadingType.Daily:
                        {
                            query += DBManager.AssembleTableQuery("tblwatermeters1h");
                            query += DBManager.AssembleTableQuery("tblwatermeters");
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Type '{rType.ToString()}' not implemented.");
                }

                //Execute the query
                DBManager db = new DBManager();
                db.Execute(query);
                Output.Report("Database updated.");

                //TODO Add database cleanup
                db.Disconnect();

                //Release resources
                foreach (Task task in tasks)
                {
                    task.Dispose();
                }
                t.Dispose();
            }
        }

        private static void ReadControllers(string ip)
        {
            //For each device on the this IP
            List<PLC> plcs = Controllers.FindAll(i => i.Ip == ip);
            //Modbus.ModbusCommunicator communicator = PlcManager.communicators.Find(x => x.IP_Address == ip);

            foreach (PLC plc in plcs)
            {
                //Clear the last result
                plc.ClearResult();

                //Get the new reading
                try
                {
                    plc.Read();
                    Console.WriteLine($"ip:{plc.Ip}, id:{plc.Id} responded.");
                }
                catch (Exception ex)
                {
                    Output.Report($"Ip:{plc.Ip}, id[{plc.Id}] didn't respond: {ex.Message}");
                }

                //Sleep
                Thread.Sleep(200);
            }
            //Disconnect from the device
            //communicator.Disconnect();

        }
    }
}
