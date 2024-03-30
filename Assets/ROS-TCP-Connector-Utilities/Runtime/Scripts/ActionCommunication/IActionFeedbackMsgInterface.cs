using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Actionlib;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector
{
    public interface IActionFeedbackMsgInterface<TFeedbackMessage>
        where TFeedbackMessage : Message
    {
        public HeaderMsg header { set; get; }
        public GoalStatusMsg status { set; get; }
        public TFeedbackMessage feedback { set; get; }
    }
}
