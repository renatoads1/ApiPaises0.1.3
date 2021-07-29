// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Calibration.Api.Models;
using EventBus.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectrum.V1.Model.Data;
using System;
using System.IO;
using System.Reactive.Subjects;

namespace Calibration.Api.Services
{
    public class DetectionOrchestrator : Msg
	{

		private readonly ILogger<DetectionOrchestrator> logger;
		private readonly EventPublisher eventPublisher;

		private DetectorData firstDetector;
		private DetectorData secondDetector;

		private string errorMessage;

		private DetectionState detectionState;
		private bool isDetectorOn;
		private bool start = false;

		private long dataLength = 32;
		private Subject<MqttResponse<Command[]>> subStartDetect = new Subject<MqttResponse<Command[]>>();
		private Subject<MqttResponse<Command[]>> subStopDetect = new Subject<MqttResponse<Command[]>>();

		public DetectionOrchestrator(
			EventPublisher eventPublisher,
			ILogger<DetectionOrchestrator> logger,
			IConfiguration configuration
		)
		{
			this.eventPublisher = eventPublisher;
			this.logger = logger;
			this.dataLength = configuration.GetValue<int>("DataLength");

			logger.LogInformation($"Configured data length: {dataLength}");
		}

		public bool IsDetectorOn()
		{
			if (errorMessage != null)
			{
				throw new Exception(errorMessage);
			} 
			return isDetectorOn;
		}

		public void HandleStartDetectionResponse(byte[] payload) => EmitResponseEvent(payload, subStartDetect);

		public void StartDetection()
		{

			Mata();
			if (!EvolutionCalibration.Start) {
				 var r = Send(200,10, null);
				eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE,r);
				EvolutionCalibration.Start = true;
			}
            

			var parameters = new RequestOperationParams()
			{
				subject = subStartDetect,
				RequestOperation = () =>
				{
					errorMessage = null;
					eventPublisher.Publish(TopicsConstants.START_DETECTOR_REQUEST);
				},
				OnSuccess = response =>
				{
					logger.LogInformation($"Detecção iniciada");
					isDetectorOn = true;
					var r = Send(200, 10,null);
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
				},
				OnError = response =>
				{
					var r = Send(500, 0, $"Falha ao iniciar detecção. {response}");
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					start = false;

					logger.LogError($"Falha ao iniciar detecção. {response}");
					errorMessage = "Failure during detection start";
				}
			};
			parameters.Request();
		}

		public void HandleStopDetectionResponse(byte[] payload) => EmitResponseEvent(payload, subStopDetect);

		public void StopDetection()
		{
			detectionState = DetectionState.IDLE;
			start = false;
			var parameters = new RequestOperationParams()
			{
				subject = subStopDetect,
				RequestOperation = () =>
				{
					errorMessage = null;
					eventPublisher.Publish(TopicsConstants.CANCEL_DETECTOR_REQUEST);
				},
				OnSuccess = response =>
				{
					logger.LogInformation($"Detecção interrompida");
					isDetectorOn = false;
				},
				OnError = response =>
				{
					start = false;
					logger.LogError($"Falha ao interromper detecção. {response}");
					errorMessage = "Failure during detection stop";
				}
			};
			parameters.Request();
		}

		public void StartDarkDetection()
		{
			
			if (!EvolutionCalibration.DarkDetection)
			{
                try
                {
					var r = Send(200, 30,null);
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					EvolutionCalibration.DarkDetection = true;
				}
                catch 
                {
					var r = Send(200, 0, $"Erro no StartDarkDetection");
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					logger.LogInformation($"Erro no StartDarkDetection");
					eventPublisher.Publish(TopicsConstants.CANCEL_DETECTOR_REQUEST);
					start = false;
				}
				
			}

			if (isDetectorOn)
			{
				detectionState = DetectionState.DARK_DETECTION;
			}
		}

		public void StartAirDetection()
		{
			
			if (!EvolutionCalibration.AirDetection)
			{
                try
                {
					var r = Send(200, 70, null);
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					EvolutionCalibration.AirDetection = true;
				}
                catch (Exception)
                {
					logger.LogInformation($"Erro no StartAirDetection");
					eventPublisher.Publish(TopicsConstants.CANCEL_DETECTOR_REQUEST);
					var r = Send(200, 0, $"Erro no StartAirDetection");
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					start = false;
				}
				
			}

			if (isDetectorOn)
			{
				detectionState = DetectionState.AIR_DETECTION;
			}
		}

		public void AppendFirstDetectorData(byte[] payload)
		{

			if (firstDetector == null)
			{
				firstDetector = new DetectorData();
			}

			if (detectionState == DetectionState.DARK_DETECTION)
			{
				if (firstDetector.GetDarkDataLength() < dataLength)
				{
					firstDetector.AppendDarkData(payload);
				}
			}
			else if (detectionState == DetectionState.AIR_DETECTION)
			{
				if (firstDetector.GetAirDataLength() < dataLength)
				{
					firstDetector.AppendAirData(payload);
				}
			}
		}

		public void AppendSecondDetectorData(byte[] payload)
		{
			if (secondDetector == null)
			{
				secondDetector = new DetectorData();
			}

			if (detectionState == DetectionState.DARK_DETECTION)
			{
				if (secondDetector.GetDarkDataLength() < dataLength)
				{
					secondDetector.AppendDarkData(payload);
				}
			}
			else if (detectionState == DetectionState.AIR_DETECTION)
			{
				if (secondDetector.GetAirDataLength() < dataLength)
				{
					secondDetector.AppendAirData(payload);
				}
			}
		}

		public bool IsDarkDataReady()
		{

			if (firstDetector != null && secondDetector != null)
			{
				return firstDetector.GetDarkDataLength() == dataLength && secondDetector.GetDarkDataLength() == dataLength;
			}
			else if (firstDetector != null)
			{
				return firstDetector.GetDarkDataLength() == dataLength;
			}
			else if (secondDetector != null)
			{
				return secondDetector.GetDarkDataLength() == dataLength;
			}
			return false;
		}

		public bool IsAirDataReady()
		{
			if (firstDetector != null && secondDetector != null)
			{
				return firstDetector.GetAirDataLength() == dataLength && secondDetector.GetAirDataLength() == dataLength;
			}
			else if (firstDetector != null)
			{
				return firstDetector.GetAirDataLength() == dataLength;
			}
			else if (secondDetector != null)
			{
				return secondDetector.GetAirDataLength() == dataLength;
			}
			return false;
		}

		public void SaveData()
		{
			if (!EvolutionCalibration.Idle)
			{
                try
                {
					//var msg = _messageservice.Send(200,idle,null);
					var r = Send(200, 100, $"Dados Salvos");
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					EvolutionCalibration.Idle = true;
				}
                catch (Exception e)
                {

					logger.LogInformation($"Erro no SaveData");
					eventPublisher.Publish(TopicsConstants.CANCEL_DETECTOR_REQUEST);
					var r = Send(200, 0, $"Erro no SaveData");
					eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
					start = false;
				}

			}

			try
			{
				if (firstDetector != null && secondDetector != null)
				{
					File.WriteAllBytes("data0.bin", firstDetector.GetData());
					File.WriteAllBytes("data1.bin", secondDetector.GetData());
				}
				else if (firstDetector != null)
				{
					File.WriteAllBytes("data0.bin", firstDetector.GetData());
				}
				else if (secondDetector != null)
				{
					File.WriteAllBytes("data1.bin", secondDetector.GetData());
				}
				logger.LogInformation($"Sucesso ao persistir dados de calibração.");
			}
			catch (Exception e)
			{
				start = false;
				eventPublisher.Publish(TopicsConstants.CANCEL_DETECTOR_REQUEST);
				var r = Send(200, 0, $"Falha ao persistir dados de calibração. {e}");
				eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
				logger.LogError($"Falha ao persistir dados de calibração. {e}");
			}
		}

		public void PublishData()
		{
			try
			{
				if (firstDetector != null && secondDetector != null)
				{
					eventPublisher.Publish(TopicsConstants.CALIBRATION_FIRST_DETECTOR_DATA_RESPONSE, firstDetector.GetData());
					eventPublisher.Publish(TopicsConstants.CALIBRATION_SECOND_DETECTOR_DATA_RESPONSE, secondDetector.GetData());
				}
				else if (firstDetector != null)
				{
					eventPublisher.Publish(TopicsConstants.CALIBRATION_FIRST_DETECTOR_DATA_RESPONSE, firstDetector.GetData());
				}
				else if (secondDetector != null)
				{
					eventPublisher.Publish(TopicsConstants.CALIBRATION_SECOND_DETECTOR_DATA_RESPONSE, secondDetector.GetData());
				}
				logger.LogInformation($"Sucesso ao publicar dados de calibração.");
			}
			catch (Exception e)
			{
				start = false;
				var r = Send(400, 0, $"Falha ao publicar dados de calibração. {e}");
				eventPublisher.Publish(TopicsConstants.CALIBRATION_PROGRESS_RESPONSE, r);
				logger.LogError($"Falha ao publicar dados de calibração. {e}");
			}
		}

		public void InvalidateData()
		{
			if (firstDetector != null && secondDetector != null)
			{
				firstDetector.ClearData();
				secondDetector.ClearData();
				if (File.Exists("data0.bin")) File.Delete("data0.bin");
				if (File.Exists("data1.bin")) File.Delete("data1.bin");
			}
			else if (firstDetector != null)
			{
				firstDetector.ClearData();
				if (File.Exists("data0.bin")) File.Delete("data0.bin");
			}
			else if (secondDetector != null)
			{
				secondDetector.ClearData();
				if (File.Exists("data1.bin")) File.Delete("data1.bin");
			}
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