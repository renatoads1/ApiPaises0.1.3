// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using EventBus.Mqtt;

namespace Calibration.Api.Services
{
    public static class EvolutionCalibration
    {

        public static bool Start = false;
        public static bool DarkDetection = false;
        public static bool TurnXrayOn = false;
        public static bool AirDetection = false;
        public static bool Idle = false;

        public static void Sendmsg() {
            System.Console.WriteLine("rodou");
        }
    }
}
