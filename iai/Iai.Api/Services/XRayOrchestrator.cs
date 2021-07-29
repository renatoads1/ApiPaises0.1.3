// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Calibration.Api.Models;
using EventBus.Mqtt;
using Extensions.ReactiveX;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectrum.V1.Model.Data;
using Spectrum.V1.Model.Settings;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Calibration.Api.Services
{
    public class XRayOrchestrator : Msg
	{
		private readonly ILogger<XRayOrchestrator> logger;
		private readonly EventPublisher eventPublisher;
		private List<GeneratorSettings> xRaySettings;

		private string errorMessage;

		private bool isXRayOn;
		private IDisposable disposable;

		private Subject<MqttResponse<Command[]>> subStartXRay = new Subject<MqttResponse<Command[]>>();
		private Subject<MqttResponse<Command[]>> subStopXRay = new Subject<MqttResponse<Command[]>>();
		private Subject<MqttResponse<Command[]>> subXRayData = new Subject<MqttResponse<Command[]>>();

		/// <summary>
		/// builder method with dependency injection
		/// </summary>
		/// <param name="eventPublisher"></param>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public XRayOrchestrator(
			EventPublisher eventPublisher,
			SystemSettings settings,
			ILogger<XRayOrchestrator> logger
		)
		{
			this.eventPublisher = eventPublisher;
			this.logger = logger;
			this.xRaySettings = settings.equipmentSettings.generatorsSettings;
		}

		public bool IsXRayOn()
		{
			if (errorMessage != null)
			{
				throw new Exception(errorMessage);
			}
			return isXRayOn;
		}

		public void HandleTurnXRayOnResponse(byte[] payload) => EmitResponseEvent(payload, subStartXRay);

		public void TurnXRayOn()
		{
			var parameters = new RequestOperationParams()
			{
				subject = subStartXRay,
				RequestOperation = () =>
				{
					errorMessage = null;
					eventPublisher.Publish(TopicsConstants.START_XRAY_REQUEST);
				},
				OnSuccess = response =>
				{
					logger.LogInformation($"Emissão de raio x iniciada");
					WaitForRisingTime();
				},
				OnError = response =>
				{
					logger.LogError($"Falha ao iniciar emissão de raio x. {response}");
					errorMessage = "Failure during xray start";
				}
			};
			parameters.Request();
		}

		public void HandleTurnXRayOffResponse(byte[] payload) => EmitResponseEvent(payload, subStopXRay);

		public void TurnXRayOff()
		{
			var parameters = new RequestOperationParams()
			{
				subject = subStopXRay,
				RequestOperation = () =>
				{
					errorMessage = null;
					eventPublisher.Publish(TopicsConstants.CANCEL_XRAY_REQUEST);

				},
				OnSuccess = response =>
				{
					logger.LogInformation($"Emissão de raio x interrompida");
					isXRayOn = false;
					
				},
				OnError = response =>
				{
					logger.LogError($"Falha ao interromper emissão de raio x. {response}");
					errorMessage = "Failure during xray stop";
				}
			};
			parameters.Request();
		}

		/// <summary>
		/// Waits for x-ray rising time to indicate its status
		/// </summary>
		private void WaitForRisingTime()
		{
			var timeSpan = TimeSpan.FromSeconds(1);
			var timeout = 5000;
			disposable = QueryXRayData()
			.SwitchMap(_ => subXRayData.AsObservable().WaitForEvents(timeout))
			.Do(data => CheckXRayVoltage(data.result))
			.Delay(timeSpan)
			.Repeat(5)
			.Catch((Exception e) =>
			{
				logger.LogError($"Error during x-ray data polling: {e}");
				errorMessage = "Error during xray data polling";
				return Observable.Empty<MqttResponse<Command[]>>();
			})
			.SubscribeOn(Scheduler.Default)
			.Subscribe();
		}

		/// <summary>
		/// Checks if current x-ray voltage reached the default value
		/// </summary>
		/// <param name="data">Data array of result field from mqtt response</param>
		private void CheckXRayVoltage(Command[] data)
		{
			logger.LogInformation($"Checking x-ray voltage...");
			var firstGeneratorData = data[0];
			var secondGeneratorData = data[1];

			if (firstGeneratorData != null && secondGeneratorData != null)
			{
				var firstCurrentVoltage = firstGeneratorData.payload[8];
				var firstExpectedVoltage = xRaySettings[0].kV;
				var secondCurrentVoltage = secondGeneratorData.payload[8];
				var secondExpectedVoltage = xRaySettings[1].kV;

				if (isVoltageReached(firstCurrentVoltage, firstExpectedVoltage) &&
					isVoltageReached(secondCurrentVoltage, secondExpectedVoltage))
				{
					logger.LogInformation($"Expected first generator voltage reached:  {firstCurrentVoltage}/{firstExpectedVoltage}");
					logger.LogInformation($"Expected second generator voltage reached:  {secondCurrentVoltage}/{secondExpectedVoltage}");
					disposable?.Dispose();
					isXRayOn = true;
				}
				else
				{
					logger.LogInformation($"Expected first generator voltage not reached: {firstCurrentVoltage}/{firstExpectedVoltage}");
					logger.LogInformation($"Expected second generator voltage not reached: {secondCurrentVoltage}/{secondExpectedVoltage}");
				}
			}
			else if (firstGeneratorData != null)
			{
				var currentVoltage = firstGeneratorData.payload[8];
				var expectedVoltage = xRaySettings[0].kV;

				if (isVoltageReached(currentVoltage, expectedVoltage))
				{
					logger.LogInformation($"Expected generator voltage reached: {currentVoltage}/{expectedVoltage}");
					disposable?.Dispose();
					isXRayOn = true;
				}
				else
				{
					logger.LogInformation($"Expected first generator voltage not reached: {currentVoltage}/{expectedVoltage}");
				}
			}
			else if (secondGeneratorData != null)
			{
				var currentVoltage = secondGeneratorData.payload[8];
				var expectedVoltage = xRaySettings[1].kV;

				if (isVoltageReached(currentVoltage, expectedVoltage))
				{
					logger.LogInformation($"Expected generator voltage reached: {currentVoltage}/{expectedVoltage}");
					disposable?.Dispose();
					isXRayOn = true;
				}
				else
				{
					logger.LogInformation($"Expected first generator voltage not reached: {currentVoltage}/{expectedVoltage}");
				}
			}
			else
			{
				logger.LogError($"No generator data available");
			}
		}

		/// <summary>
		/// Verifies if current voltage is in aceptable range
		/// </summary>
		/// <param name="current">Voltage measured</param>
		/// <param name="expected">Default voltage in settings</param>
		/// <returns></returns>
		private bool isVoltageReached(int current, int expected)
		{
			return (current >= expected - 2) && (current <= expected + 2);
		}

		public void HandleQueryXRayDataResponse(byte[] payload) => EmitResponseEvent(payload, subXRayData);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private IObservable<bool> QueryXRayData()
		{
			return Observable.Create<bool>((IObserver<bool> observer) =>
            {
                try
                {
                    observer.OnNext(true);
                    observer.OnCompleted();
                    eventPublisher.Publish(TopicsConstants.QUERY_XRAY_DATA_REQUEST,
						new byte[] { 0xFF, 0xFF });
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                return Disposable.Empty;
            });
		}

		/// <summary>
		/// Emit the response event to its observer
		/// </summary>
		/// <param name="rawResponse">the byte array response</param>
		/// <param name="subject">the subject which will publish the event to a observer</param>
		private void EmitResponseEvent(byte[] rawResponse, Subject<MqttResponse<Command[]>> subject)
		{
			try
			{
				var stringPayload = System.Text.Encoding.UTF8.GetString(rawResponse);
				var mqttResponse = JsonConvert.DeserializeObject<MqttResponse<Command[]>>(stringPayload);
				if (mqttResponse == null)
				{
					throw new ArgumentNullException(string.Empty, CalibrationErrors.PAYLOAD_CONVERSION_FAILED);
				}
				subject.OnNext(mqttResponse);
			}
			catch (Exception e)
			{
				logger.LogError($"Error on handling response: {e.Message}");
			}
		}

	}
}