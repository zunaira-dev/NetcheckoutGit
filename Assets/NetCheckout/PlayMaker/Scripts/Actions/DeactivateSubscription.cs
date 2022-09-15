#if NETCHECKOUT_PLAYMAKER
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Pauses the current subscription, so users will not be charged anymore.")]
    public class DeactivateSubscription : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("The id returned after the initial subscription.")]
        public FsmString subscriptionId;

        [ActionSection("Result")]

        [Tooltip("Event to send when the subscription has been deactivated or the request has failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent deactivateEvent;

        [Tooltip("Set a bool variable to true if subscription was deactivated, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the error message if subscription could not be deactivated, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            subscriptionId = string.Empty;
            deactivateEvent = null;
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
            PlayMakerCheckout.Instance.DeactivateSubscription(
                subscriptionId.Value,
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;

            if (!success)
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(deactivateEvent);
        }
    }
}
#endif