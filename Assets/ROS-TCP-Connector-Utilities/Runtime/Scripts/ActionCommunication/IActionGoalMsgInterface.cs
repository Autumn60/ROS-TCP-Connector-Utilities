using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Actionlib;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector
{
    public interface IActionGoalMsgInterface<TGoalMessage>
        where TGoalMessage : Message
    {
        public HeaderMsg header { set; get; }
        public GoalIDMsg goal_id { set; get; }
        public TGoalMessage goal { set; get; }
    }
}
