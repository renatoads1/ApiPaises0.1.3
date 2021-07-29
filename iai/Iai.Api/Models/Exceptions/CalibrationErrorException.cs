// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using System;
using System.Runtime.Serialization;

// Exception which is thrown when there is an error on initializing the generators.
namespace Calibration.Api.Models
{
    [Serializable]
    public class CalibrationErrorException : Exception
    {
        public CalibrationErrorException() { }

        public CalibrationErrorException(string message)
            : base(message) { }

        public CalibrationErrorException(string message, Exception inner)
            : base(message, inner) { }

        protected CalibrationErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}