using Modbus;
using Plc2DatabaseTool;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxmod
{
    class PLC : PlcBase
    {
        #region Constructors
        public PLC(ModbusCommunicator modbusCommunicator, int id)
        {
            this.Id = id;
            this.Ip = modbusCommunicator.IP_Address;
            this.Port = modbusCommunicator.IP_Port;
            this.CommType = modbusCommunicator.Type;
            this.modbusCommunicator = modbusCommunicator;
        }
        public PLC(ModbusCommunicator.ModbusType CommType, string Ip, int Port, int Id)
        {
            this.CommType = CommType;
            this.Ip = Ip;
            this.Port = Port;
            this.Id = Id;
            Init();
        }
        public PLC(string SerialPort, int Id, int BaudRate, int DataBits, Parity Parity, StopBits StopBits)
        {
            CommType = ModbusCommunicator.ModbusType.ModbusRTU;
            this.SerialPort = SerialPort;
            this.Id = Id;
            this.BaudRate = BaudRate;
            this.DataBits = DataBits;
            this.Parity = Parity;
            this.StopBits = StopBits;
            Init();
        }
        public PLC(ModbusCommunicator.ModbusType CommType, string Ip, int Port, int Id, string SerialPort, int BaudRate, int DataBits, Parity Parity, StopBits StopBits)
        {
            this.CommType = CommType;
            this.SerialPort = SerialPort;
            this.Id = Id;
            this.BaudRate = BaudRate;
            this.DataBits = DataBits;
            this.Parity = Parity;
            this.StopBits = StopBits;
            this.Ip = Ip;
            this.Port = Port;
            Init();
        }
        #endregion

        public int[] Result { get; set; }

        public void Read()
        {
            Result = ReadHoldingRegisters(PlcManager.StartReg, PlcManager.AmoutToRead);
        }
        public int[] ReadHoldingRegisters(int address, int count)
        {
            int[] output = new int[count];
            //Make 3 tries
            for (int i = 0; i < 3; i++)
            {
                //Connect to the plc
                if (modbusCommunicator.Connected == false) modbusCommunicator.Connect();
                //Read the register
                ModbusCommunicator.ActionResult result = modbusCommunicator.ReadHoldingRegister((byte)Id, address, count, ref output);
                //If the reading was successful exit cycle
                if (result == ModbusCommunicator.ActionResult.OK) break;
                else if (i == 2) //On the last iteration disconect and throw an error
                {
                    if (modbusCommunicator.Connected == true) modbusCommunicator.Disconnect();
                    throw new Exception($"Unable to read from the plc, action result: '{result.ToString()}'");
                }
                Disconnect();
            }

            return output;
        }

        public bool[] ReadCoils(int address, int count)
        {
            bool[] output = new bool[count];
            for (int i = 0; i < 3; i++)
            {
                //Connect to the plc
                if (modbusCommunicator.Connected == false) modbusCommunicator.Connect();
                //Read the register
                ModbusCommunicator.ActionResult result = modbusCommunicator.ReadCoils((byte)Id, address, count, ref output);
                //If the reading was successful exit cycle
                if (result == ModbusCommunicator.ActionResult.OK) break;
                else if (i == 2) //On the last iteration disconect and throw an error
                {
                    if (modbusCommunicator.Connected == true) modbusCommunicator.Disconnect();
                    throw new Exception($"Unable to read from the plc, action result: '{result.ToString()}'.");
                }
            }
            return output;
        }
        public void ClearResult()
        {
            Result = null;
        }
        public void Disconnect()
        {
            try
            {
                modbusCommunicator.Disconnect();
            }
            catch { }
        }
    }
}