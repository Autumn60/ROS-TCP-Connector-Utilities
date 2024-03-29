using System;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector
{
    using Message = MessageGeneration.Message;

    public class ROSServiceServer<TRequest, TResponse>
        where TRequest : Message
        where TResponse : Message
    {
        [SerializeField]
        private string _service;

        private ROSConnection _ros;
        private Func<TRequest, TResponse> _callback;

        public void AdvertiseService(Func<TRequest, TResponse> callback)
        {
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.ImplementService<TRequest, TResponse>(_service, InternalCallback);
            _callback = callback;
        }

        public TResponse InternalCallback(TRequest req)
        {
            if (_callback == null) return null;
            return _callback(req);
        }
    }
}
