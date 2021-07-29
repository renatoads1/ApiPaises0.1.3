// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using System;
using System.Linq;
using System.Collections.Generic;
using EventBus.Mqtt;
using Microsoft.Extensions.Logging;
using uPLibrary.Networking.M2Mqtt;
using Calibration.Api.Models;
using Spectrum.V1.Model.Settings;
using Calibration.Api.Services;

// Class responsible to listen to warmup events.
namespace Calibration.Api.EventProcessors
{
    public class DetectorEventProcessor : EventProcessor
    {
        private DetectionOrchestrator detectionOrchestrator;
        private Dictionary<string, Action<byte[]>> delegates;

        public DetectorEventProcessor(
            MqttClient mqttClient,
            ILogger<EventProcessor> logger,
            SystemSettings settings,
            DetectionOrchestrator detectionOrchestrator
            ) : base(mqttClient,logger,settings)
        {
            this.detectionOrchestrator = detectionOrchestrator;
            InitDelegates();
            Subscribe(topicsToSubscribe, qosLevels);
        }

        ///<summary>
        /// Init the delegates which run the correspondent method to its respective topic.
        ///</summary>
        private void InitDelegates()
        {
            delegates = new Dictionary<string, Action<byte[]>>()
            {
                [TopicsConstants.START_DETECTOR_RESPONSE] = payload => detectionOrchestrator.HandleStartDetectionResponse(payload),
                [TopicsConstants.CANCEL_DETECTOR_RESPONSE] = payload => detectionOrchestrator.HandleStopDetectionResponse(payload),
                [TopicsConstants.FIRST_DETECTOR_DATA_RESPONSE] = payload => detectionOrchestrator.AppendFirstDetectorData(payload),
                [TopicsConstants.SECOND_DETECTOR_DATA_RESPONSE] = payload => detectionOrchestrator.AppendSecondDetectorData(payload)
            };
        }

        public override void Execute(string topic, byte[] payload)
        {
            Action<byte[]> runDelegate = delegates[topic];
            runDelegate(payload);
        }

        public override bool Validate(string topic, byte[] payload)
        {
            return delegates.ContainsKey(topic);
        }
        protected override string GetClassName()
        {
            return GetType().ToString();
        }
    }
}