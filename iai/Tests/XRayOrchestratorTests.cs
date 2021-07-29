// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Calibration.Api.Services;
using EventBus.Mqtt;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.V1.Model.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using Xunit;

namespace Tests
{
	public class XRayOrchestratorTests
	{
		private readonly SystemSettings settings = new SystemSettings
		{
			equipmentSettings = new EquipmentSettings
			{
				generatorsSettings = new List<GeneratorSettings>
					{
						new GeneratorSettings {
							kV = 170
						},
						new GeneratorSettings {
							kV = 170
						}
					}
			}
		};

		private readonly byte[] xRayOffResponse = new byte[] {
			0x7B, 0x22, 0x63, 0x6F, 0x64, 0x65, 0x22, 0x3A, 0x32, 0x30, 0x30, 0x2C, 0x22, 0x72, 0x65, 0x73,
			0x75, 0x6C, 0x74, 0x22, 0x3A, 0x5B, 0x7B, 0x22, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64, 0x54,
			0x79, 0x70, 0x65, 0x22, 0x3A, 0x31, 0x32, 0x38, 0x2C, 0x22, 0x70, 0x61, 0x79, 0x6C, 0x6F, 0x61,
			0x64, 0x22, 0x3A, 0x22, 0x41, 0x73, 0x67, 0x54, 0x41, 0x51, 0x55, 0x4A, 0x4F, 0x41, 0x59, 0x41,
			0x41, 0x44, 0x77, 0x41, 0x2F, 0x79, 0x73, 0x44, 0x22, 0x7D, 0x2C, 0x6E, 0x75, 0x6C, 0x6C, 0x5D,
			0x2C, 0x22, 0x65, 0x72, 0x72, 0x6F, 0x72, 0x43, 0x61, 0x75, 0x73, 0x65, 0x22, 0x3A, 0x6E, 0x75,
			0x6C, 0x6C, 0x7D
		};

		private readonly byte[] xRayOnResponse = new byte[] {
			0x7B, 0x22, 0x63, 0x6F, 0x64, 0x65, 0x22, 0x3A, 0x32, 0x30, 0x30, 0x2C, 0x22, 0x72, 0x65, 0x73,
			0x75, 0x6C, 0x74, 0x22, 0x3A, 0x5B, 0x7B, 0x22, 0x63, 0x6F, 0x6D, 0x6D, 0x61, 0x6E, 0x64, 0x54,
			0x79, 0x70, 0x65, 0x22, 0x3A, 0x31, 0x32, 0x38, 0x2C, 0x22, 0x70, 0x61, 0x79, 0x6C, 0x6F, 0x61,
			0x64, 0x22, 0x3A, 0x22, 0x41, 0x73, 0x67, 0x54, 0x41, 0x51, 0x55, 0x4A, 0x53, 0x51, 0x61, 0x71,
			0x44, 0x48, 0x77, 0x41, 0x2F, 0x37, 0x77, 0x44, 0x22, 0x7D, 0x2C, 0x6E, 0x75, 0x6C, 0x6C, 0x5D,
			0x2C, 0x22, 0x65, 0x72, 0x72, 0x6F, 0x72, 0x43, 0x61, 0x75, 0x73, 0x65, 0x22, 0x3A, 0x6E, 0x75,
			0x6C, 0x6C, 0x7D
		};

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 })]
		public void TestTurnXRayOn_SuccessResponse(byte[] response)
		{
			var mqttClient = new Mock<MqttClient>("localhost");
			var loggerPub = new Mock<ILogger<EventPublisher>>();
			var loggerXRay = new Mock<ILogger<XRayOrchestrator>>();
			var eventPublisher = new Mock<EventPublisher>(mqttClient.Object, loggerPub.Object);

			XRayOrchestrator xRayOrchestrator = new XRayOrchestrator(eventPublisher.Object, settings, loggerXRay.Object);

			xRayOrchestrator.TurnXRayOn();
			xRayOrchestrator.HandleTurnXRayOnResponse(response);

			Assert.False(xRayOrchestrator.IsXRayOn());

			Thread.Sleep(3000);
			xRayOrchestrator.HandleQueryXRayDataResponse(xRayOnResponse);
			
			Assert.True(xRayOrchestrator.IsXRayOn());
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 52, 48, 48, 125 })]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 53, 48, 48, 125 })]
		[InlineData(new byte[] { })]
		public void TestTurnXRayOn_FailResponse(byte[] response)
		{
			var mqttClient = new Mock<MqttClient>("localhost");
			var loggerPub = new Mock<ILogger<EventPublisher>>();
			var loggerXRay = new Mock<ILogger<XRayOrchestrator>>();
			var eventPublisher = new Mock<EventPublisher>(mqttClient.Object, loggerPub.Object);

			XRayOrchestrator xRayOrchestrator = new XRayOrchestrator(eventPublisher.Object, settings, loggerXRay.Object);

			xRayOrchestrator.TurnXRayOn();
			xRayOrchestrator.HandleTurnXRayOnResponse(response);

			try
			{
				Assert.False(xRayOrchestrator.IsXRayOn());
			}
			catch (Exception e)
			{
				Assert.Equal("Failure during xray start", e.Message);
			}
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 })]
		public void TestTurnXRayOff_SuccessResponse(byte[] response)
		{
			var mqttClient = new Mock<MqttClient>("localhost");
			var loggerPub = new Mock<ILogger<EventPublisher>>();
			var loggerXRay = new Mock<ILogger<XRayOrchestrator>>();
			var eventPublisher = new Mock<EventPublisher>(mqttClient.Object, loggerPub.Object);

			XRayOrchestrator xRayOrchestrator = new XRayOrchestrator(eventPublisher.Object, settings, loggerXRay.Object);

			xRayOrchestrator.TurnXRayOn();
			xRayOrchestrator.HandleTurnXRayOnResponse(response);
			
			Assert.False(xRayOrchestrator.IsXRayOn());

			Thread.Sleep(3000);
			xRayOrchestrator.HandleQueryXRayDataResponse(xRayOnResponse);

			Assert.True(xRayOrchestrator.IsXRayOn());

			xRayOrchestrator.TurnXRayOff();
			xRayOrchestrator.HandleTurnXRayOffResponse(response);

			Thread.Sleep(1000);
			Assert.False(xRayOrchestrator.IsXRayOn());
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 52, 48, 48, 125 })]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 53, 48, 48, 125 })]
		[InlineData(new byte[] { })]
		public void TestTurnXRayOff_FailResponse(byte[] response)
		{
			var mqttClient = new Mock<MqttClient>("localhost");
			var loggerPub = new Mock<ILogger<EventPublisher>>();
			var loggerXRay = new Mock<ILogger<XRayOrchestrator>>();
			var eventPublisher = new Mock<EventPublisher>(mqttClient.Object, loggerPub.Object);

			XRayOrchestrator xRayOrchestrator = new XRayOrchestrator(eventPublisher.Object, settings, loggerXRay.Object);

			xRayOrchestrator.TurnXRayOn();
			xRayOrchestrator.HandleTurnXRayOnResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.False(xRayOrchestrator.IsXRayOn());

			Thread.Sleep(3000);
			xRayOrchestrator.HandleQueryXRayDataResponse(xRayOnResponse);

			Assert.True(xRayOrchestrator.IsXRayOn());

			xRayOrchestrator.TurnXRayOff();
			xRayOrchestrator.HandleTurnXRayOffResponse(response);

			try
			{
				Assert.True(xRayOrchestrator.IsXRayOn());
			}
			catch (Exception e)
			{
				Assert.Equal("Failure during xray stop", e.Message);
			}
		}
	}
}
