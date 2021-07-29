// Creation Date: 01/06/2021
// Author: Mariana Silva
// Author: Renato Lacerda
// Developed by: VMI Security System
// Copyright © 2021 VMI Security System
// All rights are reserved. Reproduction in whole or part is prohibited without the written consent of the copyright owner.

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EventBus.Models;
using EventBus.Mqtt;
using Extensions.ReactiveX;
using Spectrum.V1.Model.Data;

namespace Calibration.Api.Models
{
    public class RequestOperationParams
    {
        public Subject<MqttResponse<Command[]>> subject { get; set; }
        public Action RequestOperation { get; set; }
        public Action<MqttResponse<Command[]>> OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }

        public void Request()
        {
            IDisposable disposable = null;
            try
            {
                disposable = this.subject
                    .AsObservable()
					.SwitchMap(result =>
                    {
                        if (result.code == MqttStatusCodes.OK)
                        {
                            return Observable.Return(result);
                        }
                        return Observable.Throw<MqttResponse<Command[]>>(new MqttErrorResponseException(result.errorCause));
                    })
                    .WaitForEvents(5000)
                    .Subscribe(result => this.OnSuccess(result),
                               error => this.OnError(error));

                this.RequestOperation();
            }
            catch (Exception error)
            {
                disposable?.Dispose();
                this.OnError(error);
            }
        }
    }
}