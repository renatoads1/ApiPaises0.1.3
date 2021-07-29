// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.


// Interface to define the basic methods for a Warmup Orchestrator.
namespace Calibration.Api.Models
{
    public interface ICalibrationOrchestrator
    {

        ///<summary>
        /// Starts calibration process.
        ///</summary>
        void StartCalibration();

        ///<summary>
        /// Cancels calibration process.
        ///</summary>
        void CancelCalibration();

        /// <summary>
        /// Sends calibration status through MQTT
        /// </summary>p
        public void SendCalibrationStatus();

        /// <summary>
        /// Returns calibration data status
        /// </summary>
        /// <returns true>Calibration data is ready for use</returns>
        /// <returns false>There is no calibration data for use</returns>
        public bool GetCalibrationDataStatus();
    }
}