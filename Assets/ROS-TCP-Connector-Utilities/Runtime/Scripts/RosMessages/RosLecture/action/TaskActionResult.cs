using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.RosLecture
{
    public class TaskActionResult : ActionResult<TaskResult>
    {
        public const string k_RosMessageName = "ros_lecture_msgs/TaskActionResult";
        public override string RosMessageName => k_RosMessageName;


        public TaskActionResult() : base()
        {
            this.result = new TaskResult();
        }

        public TaskActionResult(HeaderMsg header, GoalStatusMsg status, TaskResult result) : base(header, status)
        {
            this.result = result;
        }
        public static TaskActionResult Deserialize(MessageDeserializer deserializer) => new TaskActionResult(deserializer);

        TaskActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = TaskResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
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
