using UnityEngine;

namespace NetCheckout.Demo
{
    /// <summary>
    /// Example manager class that gives easy access to NetCheckout's major functions.
    /// Feel free to use as is or tweak it. Or write your own.
    /// </summary>
    public class DemoCheckout : MonoBehaviour
    {
        public CheckoutClient checkoutClient;

        private Checkout checkout;

        public enum CheckoutClient
        {
            PayPal,
            Stripe
        }

        private void Awake()
        {
            ICheckoutClient client;
            
            switch (checkoutClient)
            {
                case CheckoutClient.PayPal: 
                    client = new PayPalClient();
                    break;
                default:
                    client = new StripeClient();
                    break;
            };

            checkout = new Checkout(client);
        }

        public void BuyItem(DemoItem item, action onComplete)
        {
            checkout.Buy(item.name, item.price, item.quantity, onComplete);
        }

        public void GetOrderData(string orderId, action onComplete)
        {
            checkout.GetOrderData(orderId, (success, data) => {
                onComplete?.Invoke(success, data);
            });
        }

        public void SubscribeToPlan(DemoPlan plan, action onComplete)
        {
            checkout.Subscribe(plan.name, plan.price, plan.period, plan.intervals, onComplete);
        }

        public void GetSubscriptionData(string subscriptionId, action onComplete)
        {
            checkout.GetSubscriptionData(subscriptionId, (success, data) => {
                onComplete?.Invoke(success, data);
            });
        }

        public void ActivateSubscription(string subscriptionId, action onComplete)
        {
            checkout.ActivateSubscription(subscriptionId, onComplete);
        }

        public void DeactivateSubscription(string subscriptionId, action onComplete)
        {
            checkout.DeactivateSubscription(subscriptionId, onComplete);
        }

        public void SetOrderWindowHeader(string text)
        {
            var msg = checkout.MessageConfig;
            msg.orderWindow.header = text;
            checkout.MessageConfig = msg;
        }

        public void SetOrderWindowImage(int windowIndex, Sprite sprite)
        {
            var msg = checkout.MessageConfig;
            msg.orderWindow.prefabIndex = windowIndex;
            msg.orderWindow.sprite = sprite;
        }

        public void SetSubscribeWindowHeader(string text)
        {
            var msg = checkout.MessageConfig;
            msg.subscribeWindow.header = text;
            checkout.MessageConfig = msg;
        }

        public void SetSubscribeWindowImage(int windowIndex, Sprite sprite)
        {
            var msg = checkout.MessageConfig;
            msg.subscribeWindow.prefabIndex = windowIndex;
            msg.subscribeWindow.sprite = sprite;
            checkout.MessageConfig = msg;
        }
    }
}