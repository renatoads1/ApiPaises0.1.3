// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Xunit;
using Calibration.Api.Services;
using Moq;
using Microsoft.Extensions.Logging;
using EventBus.Mqtt;
using uPLibrary.Networking.M2Mqtt;
using System.IO;
using Microsoft.Extensions.Configuration;
using System;

namespace Tests
{
	public class DetectionOrchestratorTests
	{
		// Path to detector dataset
		readonly string DATASET_IN1 = "../../../Datasets/dataset_in1.bin";
		
		// Number of detector data to receive in order to complete detection stages
		readonly int dataLength = 10;

		/// <summary>
		/// Creates DetectorOrchestrator object to execute tests
		/// </summary>
		/// <returns>New DetectorOrchestrator object</returns>
		internal DetectionOrchestrator BuildObject()
		{
			var mqttClient = new Mock<MqttClient>("localhost");
			var loggerPub = new Mock<ILogger<EventPublisher>>();
			var loggerDetec = new Mock<ILogger<DetectionOrchestrator>>();
			var eventPublisher = new Mock<EventPublisher>(mqttClient.Object, loggerPub.Object);

			Mock<IConfigurationSection> section = new Mock<IConfigurationSection>();
			section.Setup(x => x.Value).Returns(dataLength.ToString());

			Mock<IConfiguration> configuration = new Mock<IConfiguration>();
			configuration.Setup(x => x.GetSection(It.Is<string>(k => k == "DataLength"))).Returns(section.Object);

			return new DetectionOrchestrator(eventPublisher.Object, loggerDetec.Object, configuration.Object);
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 })]
		public void TestStartDetection_SuccessResponse(byte[] response)
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(response);

			Assert.True(detectionOrchestrator.IsDetectorOn());
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 52, 48, 48, 125 })]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 53, 48, 48, 125 })]
		[InlineData(new byte[] { })]
		public void TestStartDetection_FailResponse(byte[] response)
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(response);

			try
			{
				Assert.False(detectionOrchestrator.IsDetectorOn());
			}
			catch (Exception e)
			{
				Assert.Equal("Failure during detection start", e.Message);
			}
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 })]
		public void TestStopDetection_SuccessResponse(byte[] response)
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(response);

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StopDetection();
			detectionOrchestrator.HandleStopDetectionResponse(response);

			Assert.False(detectionOrchestrator.IsDetectorOn());
		}

		[Theory]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 52, 48, 48, 125 })]
		[InlineData(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 53, 48, 48, 125 })]
		[InlineData(new byte[] { })]
		public void TestStopDetection_FailResponse(byte[] response)
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StopDetection();
			detectionOrchestrator.HandleStopDetectionResponse(response);

			try
			{
				Assert.True(detectionOrchestrator.IsDetectorOn());
			}
			catch (Exception e)
			{
				Assert.Equal("Failure during detection stop", e.Message);
			}
		}

		[Fact]
		public void TestAppendFirstDetectorDarkData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendFirstDetectorDarkData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendFirstDetectorAirData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendFirstDetectorAirData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendSecondDetectorDarkData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendSecondDetectorDarkData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendSecondDetectorAirData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendSecondDetectorAirData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendFirstAndSecondDetectorDarkData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendFirstAndSecondDetectorDarkData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartDarkDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsDarkDataReady());
		}

		[Fact]
		public void TestAppendFirstAndSecondDetectorAirData_DetectionRunning()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.True(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendFirstAndSecondDetectorAirData_DetectionNotRunning()
		{
			var detectionOrchestrator = BuildObject();

			Assert.False(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			Assert.False(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestAppendFirstAndSecondDetectorData_IncompleteData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}
			detectionOrchestrator.AppendSecondDetectorData(data);

			Assert.False(detectionOrchestrator.IsAirDataReady());
		}

		[Fact]
		public void TestSaveFirstDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			detectionOrchestrator.SaveData();

			Assert.True(File.Exists("data0.bin"));
		}

		[Fact]
		public void TestSaveSecondDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			detectionOrchestrator.SaveData();

			Assert.True(File.Exists("data1.bin"));
		}

		[Fact]
		public void TestSaveFirstAndSecondDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			detectionOrchestrator.SaveData();

			Assert.True(File.Exists("data0.bin"));
			Assert.True(File.Exists("data1.bin"));
		}

		[Fact]
		public void TestInvalidateFirstDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
			}

			detectionOrchestrator.InvalidateData();

			Assert.False(detectionOrchestrator.IsAirDataReady());
			Assert.False(File.Exists("data0.bin"));
		}

		[Fact]
		public void TestInvalidateSecondDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			detectionOrchestrator.InvalidateData();

			Assert.False(detectionOrchestrator.IsAirDataReady());
			Assert.False(File.Exists("data1.bin"));
		}

		[Fact]
		public void TestInvalidateFirstAndSecondDetectorData()
		{
			var detectionOrchestrator = BuildObject();

			detectionOrchestrator.StartDetection();
			detectionOrchestrator.HandleStartDetectionResponse(new byte[] { 123, 34, 99, 111, 100, 101, 34, 58, 50, 48, 48, 125 });

			Assert.True(detectionOrchestrator.IsDetectorOn());

			detectionOrchestrator.StartAirDetection();

			byte[] data = File.ReadAllBytes(DATASET_IN1);
			for (uint i = 0; i < dataLength; i++)
			{
				detectionOrchestrator.AppendFirstDetectorData(data);
				detectionOrchestrator.AppendSecondDetectorData(data);
			}

			detectionOrchestrator.InvalidateData();

			Assert.False(detectionOrchestrator.IsAirDataReady());
			Assert.False(File.Exists("data0.bin"));
			Assert.False(File.Exists("data1.bin"));
		}
	}
}
