using System;
using UnityEngine;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Rosgraph;

namespace Unity.Robotics.ROSTCPConnector
{
    public class ROSClockPublisher : MonoBehaviour
    {
        private ROSPublisher<ClockMsg> _publisher;

        private void Start()
        {
            _publisher = new ROSPublisher<ClockMsg>("/clock", 1, true);
        }

        private void Update()
        {
            float time = Time.time;
#if ROS2
            int sec = (int)Math.Truncate(Time.time);
#else
            uint sec = (uint)Math.Truncate(Time.time);
#endif

            ClockMsg msg = new ClockMsg()
            {
                clock = new TimeMsg()
                {
                    sec = sec,
                    nanosec = (uint)((time - sec) * 1e+9)
                },

            };
            _publisher.Publish(msg);
        }
    }
}
