using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.RosLecture
{
    public class TaskActionFeedback : ActionFeedback<TaskFeedback>
    {
        public const string k_RosMessageName = "ros_lecture_msgs/TaskActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public TaskActionFeedback() : base()
        {
            this.feedback = new TaskFeedback();
        }

        public TaskActionFeedback(HeaderMsg header, GoalStatusMsg status, TaskFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static TaskActionFeedback Deserialize(MessageDeserializer deserializer) => new TaskActionFeedback(deserializer);

        TaskActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = TaskFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
