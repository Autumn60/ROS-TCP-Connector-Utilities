using UnityEngine;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector.Utilities.Samples
{
    public class SimpleBoolSubscriber : MonoBehaviour
    {
        [SerializeField]
        private ROSSubscriber<BoolMsg> _subscriber;
        [SerializeField]
        private bool _data;

        private void Start()
        {
            _subscriber.Subscribe(Callback);
        }

        private void Callback(BoolMsg msg)
        {
            _data = msg.data;
        }
    }
}
