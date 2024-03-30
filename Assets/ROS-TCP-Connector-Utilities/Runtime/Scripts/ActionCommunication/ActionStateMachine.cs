using RosMessageTypes.Actionlib;

namespace Unity.Robotics.ROSTCPConnector
{
    public class ActionStateMachine
    {
        private GoalStatusMsg _status;

        public byte currentStatus { get => _status.status; }
        public GoalStatusMsg currentStatusMsg { get => _status; }

        public ActionStateMachine(string id, byte status = GoalStatusMsg.PENDING)
        {
            _status = new GoalStatusMsg()
            {
                goal_id = new GoalIDMsg()
                {
                    id = id
                },
                status = status
            };
        }

        public void ReceiveGoal()
        {
            _status.status = GoalStatusMsg.PENDING;
        }

        public void CancelRequest()
        {
            switch (_status.status)
            {
                case GoalStatusMsg.PENDING:
                    _status.status = GoalStatusMsg.RECALLING;
                    break;

                case GoalStatusMsg.ACTIVE:
                    _status.status = GoalStatusMsg.PREEMPTING;
                    break;
                default:
                    break;
            }
        }

        public void SetAccepted()
        {
            switch (_status.status)
            {
                case GoalStatusMsg.PENDING:
                    _status.status = GoalStatusMsg.ACTIVE;
                    break;

                case GoalStatusMsg.RECALLING:
                    _status.status = GoalStatusMsg.PREEMPTING;
                    break;
                default:
                    break;
            }
        }

        public void SetRejected()
        {
            switch (_status.status)
            {
                case GoalStatusMsg.PENDING:
                    _status.status = GoalStatusMsg.REJECTED;
                    break;

                case GoalStatusMsg.RECALLING:
                    _status.status = GoalStatusMsg.REJECTED;
                    break;
                default:
                    break;
            }
        }

        public void SetSucceeded()
        {
            byte status = _status.status;
            if (status != GoalStatusMsg.ACTIVE && status != GoalStatusMsg.PREEMPTING) return;
            _status.status = GoalStatusMsg.SUCCEEDED;
        }

        public void SetAborted()
        {
            byte status = _status.status;
            if (status != GoalStatusMsg.ACTIVE && status != GoalStatusMsg.PREEMPTING) return;
            _status.status = GoalStatusMsg.ABORTED;
        }

        public void SetPreempted()
        {
            _status.status = GoalStatusMsg.PREEMPTED;
        }

        public void SetCancelled()
        {
            switch (_status.status)
            {
                case GoalStatusMsg.RECALLING:
                    _status.status = GoalStatusMsg.RECALLED;
                    break;

                case GoalStatusMsg.PREEMPTING:
                    _status.status = GoalStatusMsg.PREEMPTED;
                    break;
                default:
                    break;
            }
        }
    }
}
