// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Calibration.Api.Models;
using EventBus.Models;
using EventBus.Mqtt;
using Microsoft.Extensions.Logging;
using Spectrum.V1.Model.Data;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
// Class responsible to handle Sandt Warmup Process.
namespace Calibration.Api.Services
{
    public class CalibrationOrchestrator : ICalibrationOrchestrator 
    {
        private readonly ILogger<CalibrationOrchestrator> logger;
        private readonly EventPublisher eventPublisher;
        private DetectionOrchestrator detectionOrchestrator;
        /// <summary>
        /// private variable of type XRayOrchestrator
        /// </summary>
        private XRayOrchestrator xRayOrchestrator;

        private Dictionary<CalibrationState, Action> handlers;
        private Dictionary<CalibrationState, Action> transitions;

        private CalibrationState calibrationState;
        private Timer handlerTimer;

        private Subject<MqttResponse<Command[]>> subCalibrationStatus = new Subject<MqttResponse<Command[]>>();

        /// <summary>
        /// builder method with dependency injection
        /// </summary>
        /// <param name="eventPublisher"></param>
        /// <param name="logger"></param>
        /// <param name="detectionOrchestrator"></param>
        /// <param name="xRayOrchestrator"></param>
        public CalibrationOrchestrator(
            EventPublisher eventPublisher,
            ILogger<CalibrationOrchestrator> logger,
            DetectionOrchestrator detectionOrchestrator,
            XRayOrchestrator xRayOrchestrator
        )
        {
            this.eventPublisher = eventPublisher;
            this.logger = logger;
            this.detectionOrchestrator = detectionOrchestrator;
            this.xRayOrchestrator = xRayOrchestrator;

            InitHandlers();
            InitTransitions();
        }

        /// <summary>
        /// Associate each calibration state to its respective handler
        /// </summary>
        private void InitHandlers()
        {
            handlers = new Dictionary<CalibrationState, Action>()
            {
                [CalibrationState.IDLE] = HandleIdleState,
                [CalibrationState.START_DETECTION] = HandleStartDetectionState,
                [CalibrationState.DARK_DETECTION] = HandleDarkDetectionState,
                [CalibrationState.TURN_XRAY_ON] = HandleTurnXRayOnState,
                [CalibrationState.AIR_DETECTION] = HandleAirDetectionState,
                [CalibrationState.STOP_PROCESSES] = HandleStopProcessesState,
                [CalibrationState.SEND_CALIB_DATA] = HandleSendCalibrationDataState
            };
        }

        /// <summary>
        /// Associate each state transition based on current calibration state
        /// </summary>
        private void InitTransitions()
        {
            transitions = new Dictionary<CalibrationState, Action>()
            {
                [CalibrationState.IDLE] = () =>
                {
                    detectionOrchestrator.StartDetection();
                    calibrationState = CalibrationState.START_DETECTION;
                },
                [CalibrationState.START_DETECTION] = () =>
                {
                    detectionOrchestrator.StartDarkDetection();
                    calibrationState = CalibrationState.DARK_DETECTION;
                },
                [CalibrationState.DARK_DETECTION] = () =>
                {
                    xRayOrchestrator.TurnXRayOn();
                    calibrationState = CalibrationState.TURN_XRAY_ON;
                },
                [CalibrationState.TURN_XRAY_ON] = () =>
                {
                    detectionOrchestrator.StartAirDetection();
                    calibrationState = CalibrationState.AIR_DETECTION;
                },
                [CalibrationState.AIR_DETECTION] = () =>
                {
                    xRayOrchestrator.TurnXRayOff();
                    detectionOrchestrator.StopDetection();
                    calibrationState = CalibrationState.STOP_PROCESSES;
                },
                [CalibrationState.STOP_PROCESSES] = () =>
                {
                    calibrationState = CalibrationState.SEND_CALIB_DATA;
                    detectionOrchestrator.SaveData();
                    detectionOrchestrator.PublishData();
                },
                [CalibrationState.SEND_CALIB_DATA] = () =>
                {
                    calibrationState = CalibrationState.IDLE;
                }
            };
        }

        public void StartCalibration()
        {
            if (calibrationState != CalibrationState.IDLE)
            {
                logger.LogWarning($"Comando de start ignorado. Processo de calibração já está em execução");
                eventPublisher.Publish(TopicsConstants.CALIBRATION_START_CALIBRATION_RESPONSE, new MqttResponse<bool>(MqttStatusCodes.INTERNAL_SERVER_ERROR, 
                    false, "The calibration process is already running"));
            }
            else
            {
                logger.LogInformation($"Iniciando processo de calibração...");
                detectionOrchestrator.InvalidateData();
                SwitchState();
                InitHandlerTimer();

                eventPublisher.Publish(TopicsConstants.CALIBRATION_START_CALIBRATION_RESPONSE, new MqttResponse<bool>(MqttStatusCodes.OK, true));
            }
        }

        public void CancelCalibration()
        {
            logger.LogInformation($"Cancelando processo de calibração...");
            handlerTimer?.Dispose();
            detectionOrchestrator.StopDetection();
            xRayOrchestrator.TurnXRayOff();
            detectionOrchestrator.InvalidateData();
            calibrationState = CalibrationState.IDLE;

            eventPublisher.Publish(TopicsConstants.CALIBRATION_CANCEL_CALIBRATION_RESPONSE, new MqttResponse<bool>(MqttStatusCodes.OK, true));
        }

        public void SendCalibrationStatus()
        {
            var parameters = new RequestOperationParams()
            {
                subject = subCalibrationStatus,
                RequestOperation = () =>
                {
                    string response = "";
                    eventPublisher.Publish(TopicsConstants.CALIBRATION_STATUS_RESPONSE, new MqttResponse<string>(MqttStatusCodes.OK, response));
                },
                OnSuccess = response =>
                {
                    logger.LogInformation($"Enviando status do processo de calibração para o cliente...");
                },
                OnError = response =>
                {
                    logger.LogError($"Erro ao enviar status da calibração: {response}");
                }
            };
            parameters.Request();
        }

        public bool GetCalibrationDataStatus()
        {
            bool isDataReady = detectionOrchestrator.IsDarkDataReady() && detectionOrchestrator.IsAirDataReady();
            if (isDataReady)
            {
                detectionOrchestrator.PublishData();
            }
            return isDataReady;
        }

        /// <summary>
        /// Initializes timer for state machine handlers call
        /// </summary>
        private void InitHandlerTimer()
        {
            handlerTimer = new Timer(obj =>
            {
                HandleCalibrationState();
                logger.LogInformation($"Chamada do handler {calibrationState}");
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Calls handler associated to current calibration state
        /// </summary>
        private void HandleCalibrationState()
        {
            try
            {
                Action executeHandler = handlers[calibrationState];
                executeHandler();
            }
            catch (Exception e)
            {
                eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, new MqttResponse<int>(MqttStatusCodes.BAD_REQUEST, 0, e.Message));
                CancelCalibration();
            }
        }

        /// <summary>
        /// Calls transition state associated to current calibration state
        /// </summary>
        private void SwitchState()
        {
            try
            {
                Action executeTransition = transitions[calibrationState];
                executeTransition();
            }
            catch (Exception e)
            {
                eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, new MqttResponse<int>(MqttStatusCodes.BAD_REQUEST, 0, e.Message));
                CancelCalibration();
            }
        }

        private void HandleIdleState()
        {

        }

        private void HandleStartDetectionState()
        {
            if (detectionOrchestrator.IsDetectorOn())
            {
                SwitchState();
            }
        }

        private void HandleDarkDetectionState()
        {
            if (detectionOrchestrator.IsDarkDataReady())
            {
                SwitchState();
            }
        }

        private void HandleTurnXRayOnState()
        {
            if (xRayOrchestrator.IsXRayOn())
            {
                SwitchState();
            }
        }

        private void HandleAirDetectionState()
        {
            if (detectionOrchestrator.IsAirDataReady())
            {
                SwitchState();
            }
        }

        private void HandleStopProcessesState() 
        {
            if (!xRayOrchestrator.IsXRayOn() && !detectionOrchestrator.IsDetectorOn())
            {
                SwitchState();
            }
        }

        private void HandleSendCalibrationDataState()
        {
            handlerTimer?.Dispose();
            SwitchState();
        }
    }
}