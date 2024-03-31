using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Actionlib;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector
{
    using MessageGeneration;
    using Utilities;

    [System.Serializable]
    public class ROSActionServer<TAction, TActionGoal, TActionResult, TActionFeedback, TGoal, TResult, TFeedback>
        where TAction : Action<TActionGoal, TActionResult, TActionFeedback, TGoal, TResult, TFeedback>, new()
        where TActionGoal : ActionGoal<TGoal>, new()
        where TActionResult : ActionResult<TResult>, new()
        where TActionFeedback : ActionFeedback<TFeedback>, new()
        where TGoal : Message
        where TResult : Message
        where TFeedback : Message
    {
        private class ActionWithStateMachine
        {
            public TAction action;
            public ActionStateMachine stateMachine;
        }

        [SerializeField]
        private float _frequency = 20.0f;
        [SerializeField]
        private string _action;
        [SerializeField]
        private string _clock;

        private ROSConnection _ros;

        private ROSSubscriber<TimeMsg> _clockSubscriber;

        private ROSSubscriber<TActionGoal> _goalSubscriber;
        private ROSSubscriber<GoalIDMsg> _preemptSubscriber;

        private ROSPublisher<TActionFeedback> _feedbackPublisher;
        private ROSPublisher<TActionResult> _resultPublisher;
        private ROSPublisher<GoalStatusArrayMsg> _statusPublisher;

        private Dictionary<string, ActionWithStateMachine> _actions;
        private ActionWithStateMachine _currentAction;
        private ActionWithStateMachine _newAction;

        private System.Action _newGoalAvailableCallback;
        private System.Action _newPreemptRequestAvailableCallback;

        private Coroutine _coroutine;
        private float _lastTime;

        public void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();

            _clockSubscriber = new ROSSubscriber<TimeMsg>(_clock);

            _goalSubscriber = new ROSSubscriber<TActionGoal>(_action + "/goal", GoalCallback);
            _preemptSubscriber = new ROSSubscriber<GoalIDMsg>(_action + "/cancel", PreemptCallback);

            _feedbackPublisher = new ROSPublisher<TActionFeedback>(_action + "/feedback");
            _resultPublisher = new ROSPublisher<TActionResult>(_action + "/result");
            _statusPublisher = new ROSPublisher<GoalStatusArrayMsg>(_action + "/status");

            _actions = new Dictionary<string, ActionWithStateMachine>();

            if (_coroutine != null) CoroutineHandler.StopStaticCoroutine(ref _coroutine);
            _coroutine = CoroutineHandler.StartStaticCoroutine(PublishStatusCoroutine());
        }

        public void RegisterGoalCallback(System.Action callback)
        {
            _newGoalAvailableCallback = callback;
        }

        public void RegisterPreemptCallback(System.Action callback)
        {
            _newPreemptRequestAvailableCallback = callback;
        }

        private IEnumerator PublishStatusCoroutine()
        {
            while (true)
            {
                if (_frequency == 0.0f)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                yield return new WaitUntil(() =>
                {
                    return Time.time - _lastTime > 1.0f / _frequency;
                });

                GoalStatusArrayMsg msg = GenerateGoalStatusArrayMsg();
                _statusPublisher.Publish(msg);
            }
        }

        private GoalStatusArrayMsg GenerateGoalStatusArrayMsg()
        {
            GoalStatusArrayMsg msg = new GoalStatusArrayMsg()
            {
                header = new HeaderMsg()
                {
                    stamp = _clockSubscriber.message,
                    frame_id = ""
                }
            };

            List<GoalStatusMsg> statusListMsgs = new List<GoalStatusMsg>();
            foreach (ActionWithStateMachine action in _actions.Values)
            {
                statusListMsgs.Add(action.stateMachine.currentStatusMsg);
            }

            msg.status_list = statusListMsgs.ToArray();

            return msg;
        }

        private void GoalCallback(TActionGoal msg)
        {
            string id = msg.goal_id.id;

            if (_actions.ContainsKey(id))
            {
                _actions[id].action.action_goal = msg;
                _actions[id].stateMachine.ReceiveGoal();
                _newAction = _actions[id];
            }
            else
            {
                _newAction = new ActionWithStateMachine()
                {
                    action = new TAction()
                    {
                        action_goal = msg,
                        action_result = new TActionResult(),
                        action_feedback = new TActionFeedback()
                    },
                    stateMachine = new ActionStateMachine(id)
                };
                _actions.Add(id, _newAction);
            }

            _newGoalAvailableCallback?.Invoke();
        }

        private void PreemptCallback(GoalIDMsg msg)
        {
            _actions[msg.id]?.stateMachine.CancelRequest();
            _newPreemptRequestAvailableCallback?.Invoke();
        }

        public TGoal AcceptNewGoal()
        {
            _currentAction = _newAction;
            SetAccepted();
            _newAction = null;
            return _currentAction.action.action_goal.goal;
        }

        public void PublishFeedback(TFeedback feedback)
        {
            if (_currentAction == null) return;

            TActionFeedback feedbackMsg = new TActionFeedback();
            feedbackMsg.header = new HeaderMsg()
            {
                stamp = _clockSubscriber.message,
                frame_id = ""
            };
            feedbackMsg.status = _currentAction.stateMachine.currentStatusMsg;
            feedbackMsg.feedback = feedback;

            _feedbackPublisher.Publish(feedbackMsg);
        }

        private void PublishResult(TResult result, string text)
        {
            TActionResult resultMsg = new TActionResult();
            resultMsg.header = new HeaderMsg()
            {
                stamp = _clockSubscriber.message,
                frame_id = ""
            };
            resultMsg.status = _currentAction.stateMachine.currentStatusMsg;
            resultMsg.status.text = text;
            resultMsg.result = result;

            _resultPublisher.Publish(resultMsg);
        }

        public void SetAccepted()
        {
            _currentAction?.stateMachine.SetAccepted();
        }

        public void SetRejected()
        {
            _currentAction?.stateMachine.SetRejected();
        }

        public void SetSucceeded(TResult result, string text = "")
        {
            if (_currentAction == null) return;

            _currentAction.stateMachine.SetSucceeded();
            PublishResult(result, text);
        }

        public void SetAborted(TResult result, string text = "")
        {
            if (_currentAction == null) return;

            _currentAction.stateMachine.SetAborted();
            PublishResult(result, text);
        }

        public void SetPreempted(TResult result, string text = "")
        {
            if (_currentAction == null) return;

            _currentAction.stateMachine.SetPreempted();
            PublishResult(result, text);
        }

        public void SetCancelled()
        {
            _currentAction?.stateMachine.SetCancelled();
        }
        public bool IsNewGoalAvailable()
        {
            return _newAction != null;
        }

        public bool IsActive()
        {
            if (_currentAction == null) return false;
            return _currentAction.stateMachine.currentStatus == GoalStatusMsg.ACTIVE || _currentAction.stateMachine.currentStatus == GoalStatusMsg.PREEMPTING;
        }

        public bool IsPreemptRequested()
        {
            if (_currentAction == null) return false;
            return _currentAction.stateMachine.currentStatus == GoalStatusMsg.PREEMPTING;
        }
    }
}
