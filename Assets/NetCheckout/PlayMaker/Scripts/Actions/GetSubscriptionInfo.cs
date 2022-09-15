#if NETCHECKOUT_PLAYMAKER
using NetCheckout;
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Gets info on a subscription after user has purchased a plan.")]
    public class GetSubscriptionInfo : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("The id returned after the initial subscription.")]
        public FsmString subscriptionId;

        [ActionSection("Result")]

        [Tooltip("Event to send when the info has been retrieved or the request failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent infoEvent;

        [Tooltip("Set a bool variable to true if subscription info was successfully retrieved, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the plan name if info was retrieved. Otherwise, it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storePlan;

        [Tooltip("Set a float variable to the plan price if info was retrieved. Otherwise, it's zero.")]
        [UIHint(UIHint.Variable)]
        public FsmFloat storePrice;

        [Tooltip("Set a string variable to the plan period if info was retrieved. Otherwise, it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storePeriod;

        [Tooltip("Set an int variable to the period intervals if info was retrieved. Otherwise, it's zero.")]
        [UIHint(UIHint.Variable)]
        public FsmInt storeIntervals;

        [Tooltip("Set a bool variable to the plan's active state if info was retrieved. Otherwise, it's false by default.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeActive;

        [Tooltip("Set a string variable to the purchase currency if info was retrieved. Otherwise, it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeStatus;

        [Tooltip("Set a string variable to the error message if info retrieval failed, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            subscriptionId = string.Empty;
            infoEvent = null;
            storeSuccess = null;
            storePlan = null;
            storePrice = null;
            storePeriod = null;
            storeIntervals = null;
            storeActive = null;
            storeStatus = null;
            storeErrorMessage = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.GetSubscriptionData(
                subscriptionId.Value,
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;

            if (success)
            {
                var subscriptionData = (Checkout.SubscriptionData)data;
                storePlan.Value = subscriptionData.plan;
                storePrice.Value = (float)subscriptionData.price;
                storePeriod.Value = subscriptionData.period;
                storeIntervals.Value = subscriptionData.intervals;
                storeActive.Value = subscriptionData.active;
                storeStatus.Value = subscriptionData.status;
            }
            else
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(infoEvent);
        }
    }
}
#endif