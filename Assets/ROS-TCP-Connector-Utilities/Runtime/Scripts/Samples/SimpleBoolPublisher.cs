using UnityEngine;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector.Utilities.Samples
{
    public class SimpleBoolPublisher : MonoBehaviour
    {
        [SerializeField]
        private ROSPublisher<BoolMsg> _publisher;
        [SerializeField]
        private BoolMsg _msg;

        private void Start()
        {
            _publisher.Advertise();
            _publisher.message = _msg;
        }
    }
}
