using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] request = new byte[12];

            ModbusReadCommandParameters readF = this.CommandParameters as ModbusReadCommandParameters;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, request, 4, 2);
            request[6] = readF.UnitId;
            request[7] = readF.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readF.StartAddress)), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)readF.Quantity)), 0, request, 10, 2);
            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> dictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters readF = this.CommandParameters as ModbusReadCommandParameters;
            ushort address = readF.StartAddress; // izvucena adresa
            ushort value; // ta vrednost
            int count = 0; // potrebno za uslov prekida petlje
            for (int i = 0; i < response[8]; i++) // 1.petlja po bajtu
            {
                byte dataPart = response[9 + i];
                for (int j = 0; j < 8; j++) // sad idemo bit po bit
                {
                    value = (ushort)(dataPart & 1); // izvlacimo 1 bit, to je vrednost i to ide u recnik
                    dataPart >>= 1;
                    dictionary.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, address++), value);
                    count++;
                    if (count == readF.Quantity) // treba nam raniji izlazak iz petlje
                        break;
                }

            }
            return dictionary;
        }
    }
}