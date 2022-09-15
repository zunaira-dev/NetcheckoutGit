#if NETCHECKOUT_PLAYMAKER
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Charge users a one-time fee.")]
    public class Buy : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Name of the item the user will purchase.")]
        public FsmString itemName = "My Item";
        
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Amount to charge (i.e. 4.99)")]
        public FsmString price = "1.99";

        [RequiredField]
        [UIHint(UIHint.FsmInt)]
        [Tooltip("Quantity of the item the user will purchase.")]
        public FsmInt quantity = 1;

        [ActionSection("Result")]

        [Tooltip("Event to send when the purchase has been approved or the request failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent buyEvent;

        [Tooltip("Set a bool variable to true if purchase was approved, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the order Id if purchase was approved, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeOrderId;

        [Tooltip("Set a string variable to the error message if purchase failed, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            itemName = "My Item";
            price = "1.99";
            quantity = 1;
            buyEvent = null;
            storeSuccess = null;
            storeOrderId = null;
            storeErrorMessage = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.Buy(
                itemName.Value, 
                price.Value, 
                quantity.Value, 
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;

            if (success)
                storeOrderId.Value = data.ToString();
            else
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(buyEvent);
        }
    }
}
#endif