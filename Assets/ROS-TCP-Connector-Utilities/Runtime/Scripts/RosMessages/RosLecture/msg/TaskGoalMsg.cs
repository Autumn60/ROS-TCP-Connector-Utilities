//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.RosLecture
{
    [Serializable]
    public class TaskGoalMsg : Message
    {
        public const string k_RosMessageName = "ros_lecture_msgs/TaskGoal";
        public override string RosMessageName => k_RosMessageName;

        //  Define the goal
        public uint task_id;
        //  Specify which dishwasher we want to use
        public double duration;

        public TaskGoalMsg()
        {
            this.task_id = 0;
            this.duration = 0.0;
        }

        public TaskGoalMsg(uint task_id, double duration)
        {
            this.task_id = task_id;
            this.duration = duration;
        }

        public static TaskGoalMsg Deserialize(MessageDeserializer deserializer) => new TaskGoalMsg(deserializer);

        private TaskGoalMsg(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.task_id);
            deserializer.Read(out this.duration);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.task_id);
            serializer.Write(this.duration);
        }

        public override string ToString()
        {
            return "TaskGoalMsg: " +
            "\ntask_id: " + task_id.ToString() +
            "\nduration: " + duration.ToString();
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
