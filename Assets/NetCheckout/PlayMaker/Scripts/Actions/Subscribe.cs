#if NETCHECKOUT_PLAYMAKER
using NetCheckout;
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Charge users a recurring fee.")]
    public class Subscribe : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Name of the subscription (i.e. Gold Plan)")]
        public FsmString planName = "My Plan";

        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Amount to charge (i.e. 4.99)")]
        public FsmString price = "2.99";

        [RequiredField]
        [UIHint(UIHint.FsmEnum)]
        [Tooltip("Time period to charge - day, week, month, or year")]
        public PaymentPeriod period = PaymentPeriod.month;

        [RequiredField]
        [UIHint(UIHint.FsmInt)]
        [Tooltip("Number of periods. To bill every 3 months, set period to month and intervals to 3.")]
        public FsmInt intervals = 1;

        [ActionSection("Result")]

        [Tooltip("Event to send when the subscription has been approved or the request failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent subscribeEvent;

        [Tooltip("Set a bool variable to true if subscription was approved, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the subscription Id if subscription was approved, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeSubscriptionId;

        [Tooltip("Set a string variable to the error message if purchase failed, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            planName = "My Plan";
            price = "2.99";
            period = PaymentPeriod.month;
            intervals = 1;
            subscribeEvent = null;
            storeSuccess = null;
            storeSubscriptionId = null;
            storeErrorMessage = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.Subscribe(
                planName.Value,
                price.Value,
                period,
                intervals.Value,
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;

            if (success)
                storeSubscriptionId.Value = data.ToString();
            else
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(subscribeEvent);
        }
    }
}
#endif