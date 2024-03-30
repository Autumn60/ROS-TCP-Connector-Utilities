using System;
using System.Collections;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector
{
    using MessageGeneration;
    using Utilities;

    [Serializable]
    public class ROSPublisher<T> where T : Message
    {
        [SerializeField]
        private string _topic;
        [SerializeField, Min(1)]
        private int _queueSize = 1;
        [SerializeField]
        private bool _latch = false;

        [SerializeField, Min(0.0f), Tooltip("Set 0 to disable.")]
        private float _autoPublishingFrequency = 0.0f;

        private ROSConnection _ros;
        private T _msg;
        private Coroutine _coroutine;

        private float _lastTime;

        public T message { get => _msg; set => _msg = value; }

        public ROSPublisher(string topic, int queueSize = 1, bool latch = false, float autoPublishingFrequency = 0.0f)
        {
            _topic = topic;
            _queueSize = queueSize;
            _latch = latch;
            _autoPublishingFrequency = autoPublishingFrequency;
        }

        public void Advertise()
        {
            _ros = ROSConnection.GetOrCreateInstance();

            _ros.RegisterPublisher<T>(_topic, _queueSize, _latch);

            _lastTime = Time.time;

            if (_coroutine != null) CoroutineHandler.StopStaticCoroutine(ref _coroutine);
            _coroutine = CoroutineHandler.StartStaticCoroutine(AutoPublishCoroutine());
        }

        public void Publish(T msg)
        {
            _msg = msg;
            _ros.Publish(_topic, _msg);
            _lastTime = Time.time;
        }

        private IEnumerator AutoPublishCoroutine()
        {
            while (true)
            {
                if (_autoPublishingFrequency == 0.0f || _msg == null)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                yield return new WaitUntil(() =>
                {
                    return Time.time - _lastTime > 1.0f / _autoPublishingFrequency;
                });

                _ros.Publish(_topic, _msg);
            }
        }
    }
}