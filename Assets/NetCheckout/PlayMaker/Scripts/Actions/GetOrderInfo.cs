#if NETCHECKOUT_PLAYMAKER
using NetCheckout;
using NetCheckout.PlayMaker;
using System.Diagnostics;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Gets info on an order after user has bought an item(s).")]
    public class GetOrderInfo : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("The id returned after making a successful purchase.")]
        public FsmString orderId;

        [ActionSection("Result")]

        [Tooltip("Event to send when the info has been retrieved or the request failed.")]
        [EventTarget(FsmEventTarget.EventTarget.Self)]
        public FsmEvent infoEvent;

        [Tooltip("Set a bool variable to true if order info was successfully retrieved, otherwise false.")]
        [UIHint(UIHint.Variable)]
        public FsmBool storeSuccess;

        [Tooltip("Set a string variable to the product name if info was retrieved. Otherwise, it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeProduct;

        [Tooltip("Set a float variable to the item price if info was retrieved. Otherwise, it's zero.")]
        [UIHint(UIHint.Variable)]
        public FsmFloat storePrice;

        [Tooltip("Set a float variable to the total order price if info was retrieved. Otherwise, it's zero.")]
        [UIHint(UIHint.Variable)]
        public FsmFloat storeTotal;

        [Tooltip("Set an int variable to the item quantity if info was retrieved. Otherwise, it's zero.")]
        [UIHint(UIHint.Variable)]
        public FsmInt storeQuantity;

        [Tooltip("Set a string variable to the purchase currency if info was retrieved. Otherwise, it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeCurrency;

        [Tooltip("Set a string variable to the error message if info retrieval failed, otherwise it's an empty string.")]
        [UIHint(UIHint.Variable)]
        public FsmString storeErrorMessage;

        public override void Reset()
        {
            orderId = string.Empty;
            infoEvent = null;
            storeSuccess = null;
            storeProduct = null;
            storePrice = null;
            storeTotal = null;
            storeQuantity = null;
            storeCurrency = null;
            storeErrorMessage = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.GetOrderData(
                orderId.Value,
                OnComplete);
        }

        private void OnComplete(bool success, object data)
        {
            storeSuccess.Value = success;
            
            if (success)
            {
                var orderData = (Checkout.OrderData)data;
                storeProduct.Value = orderData.product;
                storePrice.Value = (float)orderData.price;
                storeTotal.Value = (float)orderData.total;
                storeQuantity.Value = orderData.quantity;
                storeCurrency.Value = orderData.currency;
            }
            else
                storeErrorMessage.Value = data.ToString();

            Fsm.Event(infoEvent);
        }
    }
}
#endif