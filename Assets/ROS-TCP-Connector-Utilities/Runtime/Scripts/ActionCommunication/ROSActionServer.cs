using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using RosMessageTypes.Actionlib;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Std;

namespace Unity.Robotics.ROSTCPConnector
{
    using MessageGeneration;
    using Utilities;

    [Serializable]
    public class ROSActionServer<TActionGoalMessage, TGoalMessage,
                                    TActionResultMessage, TResultMessage,
                                    TActionFeedbackMessage, TFeedbackMessage>
        where TActionGoalMessage : Message, IActionGoalMsgInterface<TGoalMessage>, new()
        where TGoalMessage : Message
        where TActionResultMessage : Message, IActionResultMsgInterface<TResultMessage>, new()
        where TResultMessage : Message
        where TActionFeedbackMessage : Message, IActionFeedbackMsgInterface<TFeedbackMessage>, new()
        where TFeedbackMessage : Message
    {
        private class ROSAction
        {
            public TActionGoalMessage goal;
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

        private ROSSubscriber<TActionGoalMessage> _goalSubscriber;
        private ROSSubscriber<GoalIDMsg> _preemptSubscriber;

        private ROSPublisher<TActionFeedbackMessage> _feedbackPublisher;
        private ROSPublisher<TActionResultMessage> _resultPublisher;
        private ROSPublisher<GoalStatusArrayMsg> _statusPublisher;

        private Dictionary<string, ROSAction> _actions;
        private ROSAction _currentAction;
        private ROSAction _newAction;

        private Action _newGoalAvailableCallback;
        private Action _newPreemptRequestAvailableCallback;

        private Coroutine _coroutine;
        private float _lastTime;

        public void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();

            _clockSubscriber = new ROSSubscriber<TimeMsg>(_clock);

            _goalSubscriber = new ROSSubscriber<TActionGoalMessage>(_action + "/goal", GoalCallback);
            _preemptSubscriber = new ROSSubscriber<GoalIDMsg>(_action + "/cancel", PreemptCallback);

            _feedbackPublisher = new ROSPublisher<TActionFeedbackMessage>(_action + "/feedback");
            _resultPublisher = new ROSPublisher<TActionResultMessage>(_action + "/result");
            _statusPublisher = new ROSPublisher<GoalStatusArrayMsg>(_action + "/status");

            _actions = new Dictionary<string, ROSAction>();

            if (_coroutine != null) CoroutineHandler.StopStaticCoroutine(ref _coroutine);
            _coroutine = CoroutineHandler.StartStaticCoroutine(PublishStatusCoroutine());
        }

        public void RegisterGoalCallback(Action callback)
        {
            _newGoalAvailableCallback = callback;
        }

        public void RegisterPreemptCallback(Action callback)
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
            foreach (ROSAction action in _actions.Values)
            {
                statusListMsgs.Add(action.stateMachine.currentStatusMsg);
            }

            msg.status_list = statusListMsgs.ToArray();

            return msg;
        }

        private void GoalCallback(TActionGoalMessage msg)
        {
            string id = msg.goal_id.id;

            if (_actions.ContainsKey(id))
            {
                _actions[id].goal = msg;
                _actions[id].stateMachine.ReceiveGoal();
                _newAction = _actions[id];
            }
            else
            {
                _newAction = new ROSAction()
                {
                    goal = msg,
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

        public TGoalMessage AcceptNewGoal()
        {
            _currentAction = _newAction;
            SetAccepted();
            _newAction = null;
            return _currentAction.goal.goal;
        }

        public void PublishFeedback(TFeedbackMessage feedback)
        {
            if (_currentAction == null) return;

            TActionFeedbackMessage feedbackMsg = new TActionFeedbackMessage();
            feedbackMsg.header = new HeaderMsg()
            {
                stamp = _clockSubscriber.message,
                frame_id = ""
            };
            feedbackMsg.status = _currentAction.stateMachine.currentStatusMsg;
            feedbackMsg.feedback = feedback;

            _feedbackPublisher.Publish(feedbackMsg);
        }

        private void PublishResult(TResultMessage result, string text)
        {
            TActionResultMessage resultMsg = new TActionResultMessage();
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

        public void SetSucceeded(TResultMessage result, string text = "")
        {
            if (_currentAction == null) return;

            _currentAction.stateMachine.SetSucceeded();
            PublishResult(result, text);
        }

        public void SetAborted(TResultMessage result, string text = "")
        {
            if (_currentAction == null) return;

            _currentAction.stateMachine.SetAborted();
            PublishResult(result, text);
        }

        public void SetPreempted(TResultMessage result, string text = "")
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
