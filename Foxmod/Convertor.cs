using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxmod
{
    public static class Convertor
    {
        public static Modbus.ModbusCommunicator.ModbusType StringToConnType(string input)
        {
            switch (input.ToLower())
            {
                case "mot":
                    return Modbus.ModbusCommunicator.ModbusType.Modbus_RTU_Over_TCP;
                case "mt":
                    return Modbus.ModbusCommunicator.ModbusType.ModbusTCP;
                default:
                    throw new NotImplementedException($"Type {input} not implemented.");
            }
        }
    }
}
