// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

namespace Calibration.Api.Models
{
    public enum DetectionState
    {
        IDLE,
        DARK_DETECTION,
        AIR_DETECTION
    }

    public static class DetectorDataConstants
    {
        public const ushort HEADER_BYTES = 8;
        public const ushort DATA_BYTES = 2;
        public const ushort N_ENERGY = 2;
    }
}