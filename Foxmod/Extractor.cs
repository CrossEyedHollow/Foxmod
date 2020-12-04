using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foxmod
{
    class Extractor
    {
        public static int GetSingleRegister(int address, int[] source)
        {
            //Get the value
            int result = GetHoldingRegisters(address, 1, source)[0];
            //Get the bytes
            byte[] tmp = BitConverter.GetBytes(result);
            //Convert to int16 and return
            return BitConverter.ToInt16(tmp, 0);
        }

        public static int GetDoubleRegister(int address, int[] source)
        {
            int[] result = GetHoldingRegisters(address, 2, source);
            byte[] bytes = BitConverter.GetBytes((result[1] << 16) | result[0]);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static long GetQuadripleRegister(int address, int[] source)
        {
            int[] result = GetHoldingRegisters(address, 4, source);

            byte[] bytes = BitConverter.GetBytes(((((long)result[3] << 48) | ((long)result[2] << 32)) | (long)(result[1] << 16)) | (long)result[0]);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static bool GetSingleCoil(int address, bool[] source)
        {
            return GetCoils(address, 1, source)[0];
        }

        private static bool[] GetCoils(int address, int count, bool[] source)
        {
            List<bool> output = new List<bool>();

            for (int i = 0; i < count; i++)
            {
                output.Add(source[address + i]);
            }
            return output.ToArray();
        }

        private static int[] GetHoldingRegisters(int address, int count, int[] source)
        {
            List<int> output = new List<int>();

            for (int i = 0; i < count; i++)
            {
                output.Add(source[address + i]);
            }
            return output.ToArray();
        }
    }
}
