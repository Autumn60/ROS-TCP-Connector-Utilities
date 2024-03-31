using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.RosLecture
{
    public class TaskAction : Action<TaskActionGoal, TaskActionResult, TaskActionFeedback, TaskGoal, TaskResult, TaskFeedback>
    {
        public const string k_RosMessageName = "ros_lecture_msgs/TaskAction";
        public override string RosMessageName => k_RosMessageName;


        public TaskAction() : base()
        {
            this.action_goal = new TaskActionGoal();
            this.action_result = new TaskActionResult();
            this.action_feedback = new TaskActionFeedback();
        }

        public static TaskAction Deserialize(MessageDeserializer deserializer) => new TaskAction(deserializer);

        TaskAction(MessageDeserializer deserializer)
        {
            this.action_goal = TaskActionGoal.Deserialize(deserializer);
            this.action_result = TaskActionResult.Deserialize(deserializer);
            this.action_feedback = TaskActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
