// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

// Interface which defines the methods that should be implemented by a detector.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calibration.Api.Models
{
    public class DetectorData : IDetectorData
    {
        private IEnumerable<byte> airData;
        private IEnumerable<byte> darkData;

        private int airDataLength;
        private int darkDataLength;

        public DetectorData()
        {
            ClearData();
        }

        public void AppendDarkData(byte[] data)
        {
            if (data != null)
            {
                if (darkData.Equals(Enumerable.Empty<byte>()))
                {
                    darkData = data;
                }
                else
                {
                    ushort nDataRow = BitConverter.ToUInt16(data, 0);
                    ushort nDarkRow = BitConverter.ToUInt16(darkData.ToArray(), 0);
                    ushort nRow = (ushort)(nDataRow + nDarkRow);
                    byte[] nRowBytes = BitConverter.GetBytes(nRow);

                    darkData = nRowBytes.Concat(darkData.Skip(DetectorDataConstants.DATA_BYTES))
                        .Concat(data.Skip(DetectorDataConstants.HEADER_BYTES));
                }

                darkDataLength++;
            }
        }

        public void AppendAirData(byte[] data)
        {
            if (data != null)
            {
                if (airData.Equals(Enumerable.Empty<byte>()))
                {
                    airData = data;
                }
                else
                {
                    ushort nDataRow = BitConverter.ToUInt16(data, 0);
                    ushort nAirRow = BitConverter.ToUInt16(airData.ToArray(), 0);
                    ushort nRow = (ushort)(nDataRow + nAirRow);
                    byte[] nRowBytes = BitConverter.GetBytes(nRow);

                    airData = nRowBytes.Concat(airData.Skip(DetectorDataConstants.DATA_BYTES))
                        .Concat(data.Skip(DetectorDataConstants.HEADER_BYTES));
                }

                airDataLength++;
            }
        }

        public int GetDarkDataLength()
        {
            return darkDataLength;
        }

        public int GetAirDataLength()
        {
            return airDataLength;
        }

        public byte[] GetData()
        {
            return darkData.Concat(airData).ToArray();
        }

        public void ClearData()
        {
            darkData = Enumerable.Empty<byte>();
            airData = Enumerable.Empty<byte>();

            darkDataLength = 0;
            airDataLength = 0;
        }

        /// <summary>
        /// Verifies if data length corresponds to header info.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool isValidData(byte[] data)
        {
            if (data != null && data.Length > DetectorDataConstants.HEADER_BYTES)
            {
                ushort nRow = BitConverter.ToUInt16(data, 0);
                ushort nCol = BitConverter.ToUInt16(data, 2);

                int expectedDataLength = (nRow * nCol * DetectorDataConstants.N_ENERGY * DetectorDataConstants.DATA_BYTES) + DetectorDataConstants.HEADER_BYTES;

                return expectedDataLength == data.Length;
            }
            return false;
        }
    }
}