#if NETCHECKOUT_PLAYMAKER
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Unpauses the current subscription, so users will start to be charged again.")]
    public class ActivateSubscription : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("The id returned after the initial subscription.")]
        public FsmString subscriptionId;

        [ActionSection("Result")]

        [Tooltip("Event to send when the subscription has been activated or the request has failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent activateEvent;

        [Tooltip("Set a bool variable to true if subscription was activated, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the error message if subscription could not be activated, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            subscriptionId = string.Empty;
            activateEvent = null;
            storeSuccess = null;
            storeErrorMessage = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.ActivateSubscription(
                subscriptionId.Value,
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;

            if (!success)
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(activateEvent);
        }
    }
}
#endif