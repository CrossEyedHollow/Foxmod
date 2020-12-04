using System.IO.Ports;
using Modbus;

namespace Plc2DatabaseTool
{
    public class PlcBase
    {
        public PlcBase() { }

        protected ModbusCommunicator modbusCommunicator;

        public ModbusCommunicator.ModbusType CommType { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public int Id { get; set; }
        public string SerialPort { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }

        /// <summary>
        /// Initializes the necessary objects
        /// </summary>
        public void Init()
        {
            modbusCommunicator = new ModbusCommunicator()
            {
                IP_Address = Ip,
                IP_Port = Port,
                SerialPort_PortName = SerialPort,
                SerialPort_BaudRate = BaudRate,
                SerialPort_DataBits = DataBits,
                SerialPort_Parity = Parity,
                SerialPort_StopBits = StopBits,
                Type = CommType,
                ResponceTimeout = 1000
            };
        }
        public void Init(ModbusCommunicator modbusCommunicator)
        {
            this.modbusCommunicator = modbusCommunicator;
        }
    }
}
