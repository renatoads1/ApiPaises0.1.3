// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

// Interface which defines the methods that should be implemented by a detector.
namespace Calibration.Api.Models
{
    public interface IDetectorData
    {
        /// <summary>
        /// Appends incoming data to existing one.
        /// </summary>
        /// <param name="data">Data to append</param>
        void AppendAirData(byte[] data);

        /// <summary>
        /// Appends incoming data to existing one.
        /// </summary>
        /// <param name="data">Data to append</param>
        void AppendDarkData(byte[] data);

        /// <summary>
        /// Returns existing detector data.
        /// </summary>
        /// <returns></returns>
        byte[] GetData();
    }
}