// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using System;
using System.Runtime.Serialization;

// Exception which is thrown when there is a mqtt error response with code such as 5xx or 4xx.
namespace Calibration.Api.Models
{
    [Serializable]
    public class MqttErrorResponseException : Exception
    {
        public MqttErrorResponseException() { }

        public MqttErrorResponseException(string message)
            : base(message) { }

        public MqttErrorResponseException(string message, Exception inner)
            : base(message, inner) { }

        protected MqttErrorResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}