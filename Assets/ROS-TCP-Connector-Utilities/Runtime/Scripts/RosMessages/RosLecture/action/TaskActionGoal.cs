using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.RosLecture
{
    public class TaskActionGoal : ActionGoal<TaskGoal>
    {
        public const string k_RosMessageName = "ros_lecture_msgs/TaskActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public TaskActionGoal() : base()
        {
            this.goal = new TaskGoal();
        }

        public TaskActionGoal(HeaderMsg header, GoalIDMsg goal_id, TaskGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static TaskActionGoal Deserialize(MessageDeserializer deserializer) => new TaskActionGoal(deserializer);

        TaskActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = TaskGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
