using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Actionlib;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector
{
    public interface IActionResultMsgInterface<TResultMessage>
        where TResultMessage : Message
    {
        public HeaderMsg header { set; get; }
        public GoalStatusMsg status { set; get; }
        public TResultMessage result { set; get; }
    }
}
