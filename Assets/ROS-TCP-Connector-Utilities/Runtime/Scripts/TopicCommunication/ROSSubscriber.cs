using System;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector
{
    using MessageGeneration;

    [Serializable]
    public class ROSSubscriber<T> where T : Message
    {
        [SerializeField]
        private string _topic;
        [SerializeField, Min(0.0f)]
        private float _timeout;

        private ROSConnection _ros;
        private Action<T> _callback;
        private T _msg;

        private float _lastTime;

        public T message { get => _msg; }

        public bool isSubscribed
        {
            get
            {
                return Time.time - _lastTime < _timeout;
            }
        }

        public ROSSubscriber(string topic, Action<T> callback = null)
        {
            _topic = topic;
            _callback = callback;
            Subscribe();
        }
            
        public void SetCallback(Action<T> callback)
        {
            _callback = callback;
        }

        public void Subscribe()
        {
            _ros = ROSConnection.GetOrCreateInstance();
            
            _ros.Subscribe<T>(_topic, InternalCallback);

            _lastTime = Time.time;
        }

        public void Subscribe(Action<T> callback)
        {
            SetCallback(callback);
            Subscribe();
        }

        public void Unsubscribe()
        {
            _ros.Unsubscribe(_topic);
        }

        private void InternalCallback(T msg)
        {
            _msg = msg;
            _lastTime = Time.time;

            if (_callback != null) _callback.Invoke(msg);
        }
    }
}
