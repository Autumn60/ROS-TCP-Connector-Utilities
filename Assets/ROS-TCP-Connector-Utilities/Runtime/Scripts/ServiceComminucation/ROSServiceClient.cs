using System;
using System.Collections;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector.Utilities;

namespace Unity.Robotics.ROSTCPConnector
{
    using Message = MessageGeneration.Message;

    public class ROSServiceClient<TRequest, TResponse>
        where TRequest : Message
        where TResponse : Message, new()
    {
        [SerializeField]
        private string _service;
        [SerializeField]
        private float _delay;
        [SerializeField, Min(0.0f)]
        private float _timeout;

        private ROSConnection _ros;

        private Action<TResponse> _responseCallback;
        private Action _timeoutCallback;

        private Coroutine _coroutine;

        public void RegisterService(Action<TResponse> responseCallback, Action timeoutCallback = null)
        {
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.RegisterRosService<TRequest, TResponse>(_service);
        }

        public void Call(TRequest request)
        {
            if (_coroutine != null) CoroutineHandler.StopStaticCoroutine(ref _coroutine);
            _coroutine = CoroutineHandler.StartStaticCoroutine(ServiceCallingCoroutine(request));
        }

        private IEnumerator ServiceCallingCoroutine(TRequest req)
        {
            yield return new WaitForSeconds(_delay);

            _ros.SendServiceMessage(_service, req, (TResponse res) =>
            {
                _responseCallback.Invoke(res);
                return;
            });

            yield return new WaitForSeconds(_timeout);
            _timeoutCallback.Invoke();
        }
    }
}
