// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using System;
using EventBus.Mqtt;
using Calibration.Api.Models;
using Calibration.Api.Services;
using Spectrum.V1.Model.Settings;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt;
using Microsoft.Extensions.Logging;

namespace Calibration.Api.EventProcessors
{
    public class XRayEventProcessor : EventProcessor
    {
        /// <summary>
        /// type variable XRayOrchestrator
        /// </summary>
        private XRayOrchestrator xRayOrchestrator;
        private Dictionary<string, Action<byte[]>> delegates;

        /// <summary>
        /// class constructor with dependency injection
        /// </summary>
        public XRayEventProcessor(
            MqttClient mqttClient,
            ILogger<EventProcessor> logger,
            SystemSettings settings,
            XRayOrchestrator xRayOrchestrator) 
            : base(mqttClient, logger, settings)
        {
            this.xRayOrchestrator = xRayOrchestrator;
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
                [TopicsConstants.START_XRAY_RESPONSE] = payload => xRayOrchestrator.HandleTurnXRayOnResponse(payload),
                [TopicsConstants.CANCEL_XRAY_RESPONSE] = payload => xRayOrchestrator.HandleTurnXRayOffResponse(payload),
                [TopicsConstants.QUERY_XRAY_DATA_RESPONSE] = payload => xRayOrchestrator.HandleQueryXRayDataResponse(payload)
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
