// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using Xunit;
using Calibration.Api.Models;
using System.IO;
using System.Text;
using System.Linq;

namespace Tests
{
    public class DetectorDataTests
    {
        readonly string DATASET_IN1 = "../../../Datasets/dataset_in1.bin";
        readonly string DATASET_IN2 = "../../../Datasets/dataset_in2.bin";
        readonly string DATASET_IN4 = "../../../Datasets/dataset_in4.bin";

        [Fact]
        public void TestAppendDarkData_SingleData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendDarkData(data);

            Assert.Equal(1, detectorData.GetDarkDataLength());
            Assert.Equal(data, detectorData.GetData());
        }

        [Fact]
        public void TestAppendDarkData_MultipleData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendDarkData(data);

            data = File.ReadAllBytes(DATASET_IN2);
            detectorData.AppendDarkData(data);

            Assert.Equal(2, detectorData.GetDarkDataLength());
        }

        [Fact]
        public void TestAppendDarkData_NullData()
        {
            DetectorData detectorData = new DetectorData();
            detectorData.AppendDarkData(null);

            Assert.Equal(0, detectorData.GetDarkDataLength());
            Assert.Equal(Enumerable.Empty<byte>(), detectorData.GetData());
        }

        [Fact]
        public void TestAppendAirData_SingleData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendAirData(data);

            Assert.Equal(1, detectorData.GetAirDataLength());
            Assert.Equal(data, detectorData.GetData());
        }

        [Fact]
        public void TestAppendAirData_MultipleData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendAirData(data);

            data = File.ReadAllBytes(DATASET_IN2);
            detectorData.AppendAirData(data);

            Assert.Equal(2, detectorData.GetAirDataLength());
        }

        [Fact]
        public void TestAppendAirData_BrokenData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN4);
            detectorData.AppendAirData(data);

            Assert.Equal(0, 0);
            Assert.Equal(0, 0);
        }

        [Fact]
        public void TestAppendAirData_NullData()
        {
            DetectorData detectorData = new DetectorData();
            detectorData.AppendAirData(null);

            Assert.Equal(0, detectorData.GetAirDataLength());
            Assert.Equal(Enumerable.Empty<byte>(), detectorData.GetData());
        }

        [Fact]
        public void TestAppendDarkAndAirData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendDarkData(data);

            data = File.ReadAllBytes(DATASET_IN2);
            detectorData.AppendAirData(data);

            Assert.Equal(1, detectorData.GetDarkDataLength());
            Assert.Equal(1, detectorData.GetAirDataLength());
        }

        [Fact]
        public void TestClearData()
        {
            DetectorData detectorData = new DetectorData();

            byte[] data = File.ReadAllBytes(DATASET_IN1);
            detectorData.AppendDarkData(data);

            data = File.ReadAllBytes(DATASET_IN2);
            detectorData.AppendAirData(data);

            detectorData.ClearData();

            Assert.Equal(0, detectorData.GetDarkDataLength());
            Assert.Equal(0, detectorData.GetAirDataLength());
            Assert.Equal(Enumerable.Empty<byte>(), detectorData.GetData());
        }
    }
}
