using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.RosLecture;

namespace Unity.Robotics.ROSTCPConnector.Utilities.Samples
{
    using TaskServer = ROSActionServer<TaskAction, TaskActionGoal, TaskActionResult, TaskActionFeedback, TaskGoal, TaskResult, TaskFeedback>;

    public class AdvActionServer : MonoBehaviour
    {
        [SerializeField]
        private TaskServer _server;

        private TaskGoal _currentGoal;
        private float _startTime = 0.0f;

        private void Start()
        {
            _server.Start();
            _server.RegisterGoalCallback(GoalCallback);
        }

        private void Update()
        {
            if (_server.IsActive())
            {
                if (_server.IsPreemptRequested())
                {
                    _server.SetPreempted(new TaskResult(Time.time - _startTime > _currentGoal.duration));
                    Debug.Log("Preempt Goal");
                    return;
                }

                if (Time.time - _startTime > _currentGoal.duration)
                {
                    Debug.Log("Succeeded");
                    _server.SetSucceeded(new TaskResult(true));
                }
                else
                {
                    float rate = (Time.time - _startTime) / (float)_currentGoal.duration;
                    Debug.Log("Feedback: " + rate.ToString());
                    _server.PublishFeedback(new TaskFeedback(rate));
                }
            }
        }

        private void GoalCallback()
        {
            _currentGoal = _server.AcceptNewGoal();
            _startTime = Time.time;

            Debug.Log("Update Goal");
        }
    }
}