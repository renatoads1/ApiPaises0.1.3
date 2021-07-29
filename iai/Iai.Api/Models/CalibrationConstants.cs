// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

namespace Calibration.Api.Models
{
   public static class TopicsConstants
    {
        
        public const string CALIBRATION_FIRST_DETECTOR_DATA_RESPONSE = "vmi/spectrum/v1/pipeline/calibration/0/in";
        public const string CALIBRATION_SECOND_DETECTOR_DATA_RESPONSE = "vmi/spectrum/v1/pipeline/calibration/1/in";
        public const string CALIBRATION_ERROR_RESPONSE = "vmi/spectrum/v1/calibration/status/error";
        public const string CALIBRATION_STATUS_RESPONSE = "vmi/spectrum/v1/calibration/status/out";
        public const string CALIBRATION_DATA_STATUS_RESPONSE = "vmi/spectrum/v1/calibration/data/status/out";
        public const string FIRST_DETECTOR_DATA_RESPONSE = "vmi/spectrum/v1/pipeline/preprocess/0/in";
        public const string SECOND_DETECTOR_DATA_RESPONSE = "vmi/spectrum/v1/pipeline/preprocess/1/in";
        public const string START_DETECTOR_REQUEST = "vmi/spectrum/v1/detector/request/start_capture";
        public const string START_DETECTOR_RESPONSE = "vmi/spectrum/v1/detector/response/start_capture";
        public const string CANCEL_DETECTOR_REQUEST = "vmi/spectrum/v1/detector/request/stop_capture";
        public const string CANCEL_DETECTOR_RESPONSE = "vmi/spectrum/v1/detector/response/stop_capture";
        public const string START_XRAY_REQUEST = "vmi/spectrum/v1/communication/mcb/generator/request/x_ray_on";
        public const string START_XRAY_RESPONSE = "vmi/spectrum/v1/communication/mcb/generator/response/x_ray_on";
        public const string CANCEL_XRAY_REQUEST = "vmi/spectrum/v1/communication/mcb/generator/request/x_ray_off";
        public const string CANCEL_XRAY_RESPONSE = "vmi/spectrum/v1/communication/mcb/generator/response/x_ray_off";
        public const string QUERY_XRAY_DATA_REQUEST = "vmi/spectrum/v1/communication/mcb/generator/request/status_or_clear_alarm";
        public const string QUERY_XRAY_DATA_RESPONSE = "vmi/spectrum/v1/communication/mcb/generator/response/status_or_clear_alarm";
        /// <summary>
        /// progress bar topics, 
        /// start calibration response, 
        /// cancel calibration response
        /// </summary>
        public const string CALIBRATION_PROGRESS_RESPONSE = "vmi/spectrum/v1/calibration/response/calibration_progress";
        public const string CALIBRATION_START_CALIBRATION_RESPONSE = "vmi/spectrum/v1/calibration/response/start_calibration";
        public const string CALIBRATION_CANCEL_CALIBRATION_RESPONSE = "vmi/spectrum/v1/calibration/response/cancel_calibration";
        
    }

    public enum CalibrationState
    {
        IDLE,
        START_DETECTION,
        DARK_DETECTION,
        TURN_XRAY_ON,
        AIR_DETECTION,
        STOP_PROCESSES,
        SAVE_CALIB_DATA,
        SEND_CALIB_DATA
    }
}