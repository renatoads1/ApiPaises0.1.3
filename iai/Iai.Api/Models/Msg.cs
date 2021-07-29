// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.


using Calibration.Api.Services;
using System;

namespace Calibration.Api.Models
{
    /// <summary>
    /// class responsible for sending messages
    /// </summary>
    public class Msg
    {
        public int code { get; set; }
        public int result { get; set; }
        public string errorCause { get; set; }

        public Msg()
        {

        }

        /// <summary>
        /// method that sends message
        /// </summary>
        /// <param name="status">int</param>
        /// <param name="code">int</param>
        /// <param name="msg">string</param>
        /// <returns></returns>
        public Msg Send(int code, int result, string msg)
        {
            this.code = code;
            this.result = result;
            errorCause = msg;

            try
            {
                var obj = new Msg{ code = code, result = result, errorCause = msg };
               //var str = JsonConvert.SerializeObject(obj).ToString();
                var str = obj;
                return str;
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
        /// <summary>
        /// kills the calibration flow
        /// </summary>
        public void Mata() {
            EvolutionCalibration.Start = false;
            EvolutionCalibration.DarkDetection = false;
            EvolutionCalibration.AirDetection = false;
            EvolutionCalibration.Idle = false;
        }

    }

}
