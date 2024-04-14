using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            ushort address = readF.StartAddress;
            ushort value;
            for (int i = 0; i < response[8]; i += 2)
            {
                value = BitConverter.ToUInt16(response, (i + 9));
                value = (ushort)IPAddress.HostToNetworkOrder((short)value);
                dictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address++), value);
            }
            return dictionary;
        }
    }
}