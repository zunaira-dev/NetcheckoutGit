#if NETCHECKOUT_PLAYMAKER
using UnityEngine;

namespace NetCheckout.PlayMaker
{
    /// <summary>
    /// Singleton manager class that gives easy access to NetCheckout's major functions.
    /// </summary>
    public class PlayMakerCheckout
    {
        private static PlayMakerCheckout instance;

        private Checkout checkout;

        private ICheckoutClient checkoutClient;

        public enum CheckoutClient
        {
            PayPal,
            Stripe
        }

        private PlayMakerCheckout() { }

        private PlayMakerCheckout(ICheckoutClient client)
        {
            SetClient(client);
        }

        /// <summary>
        /// Access checkout functionality via this singleton instance.
        /// </summary>
        public static PlayMakerCheckout Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayMakerCheckout(new PayPalClient());
                return instance;
            }
        }

        /// <summary>
        /// Stores UI messages displayed to the user.
        /// </summary>
        public MessageConfig MessageConfig
        {
            get => checkout.MessageConfig;
            set => checkout.MessageConfig = value;
        }

        /// <summary>
        /// Initializes a new checkout instance with the specified client interface.
        /// </summary>
        /// <param name="client">i.e. <see cref="PayPalClient"/></param>
        public void SetClient(ICheckoutClient client)
        {
            checkoutClient = client;
            checkout = new Checkout(checkoutClient);
        }

        /// <summary>
        /// Set the Stripe secret key associated with your developer account.
        /// In editor, it will set the test secret key. In a build, it will set the live secret key.
        /// </summary>
        public void SetClientSecret(string secret)
        {
            BaseClient client = (BaseClient)checkoutClient;
            client.SetClientSecret(secret);
        }

        /// <summary>
        /// Charge users a one-time fee.
        /// </summary>
        /// <param name="price">amount to charge (i.e. 4.99)</param>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public void Buy(string itemName, string price, int quantity, action onComplete)
        {
            checkout.Buy(itemName, price, quantity, onComplete);
        }

        /// <summary>
        /// Gets data in the <see cref="Checkout.OrderData" class/>.
        /// </summary>
        /// <param name="orderId">The id returned after making a successful purchase.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="Checkout.OrderData"/> object. 
        /// On failure, returns false and error response.</param>
        public void GetOrderData(string orderId, action onComplete)
        {
            checkout.GetOrderData(orderId, (success, data) => {
                onComplete?.Invoke(success, data);
            });
        }

        /// <summary>
        /// Charge users a recurring fee.
        /// For example, to charge a monthly fee, set the period to month and intervals to 1. 
        /// To charge every 2 weeks, set period to week and intervals to 2.
        /// </summary>
        /// <param name="planName">name of the subscription (i.e. Gold Plan)</param>
        /// <param name="price">amount to charge (i.e. 7.99)</param>
        /// <param name="period">day, week, month, year</param>
        /// <param name="intervals">number of periods. To bill every 3 months, set period to month and intervals to 3.</param>
        /// <param name="onComplete">Callback. If successful, returns true and the subscription id. If failed, returns false and the error response.</param>
        public void Subscribe(string planName, string price, PaymentPeriod period, int intervals, action onComplete)
        {
            checkout.Subscribe(planName, price, period, intervals, onComplete);
        }

        /// <summary>
        /// Gets data in the <see cref="Checkout.SubscriptionData"/> class.
        /// </summary>
        /// <param name="subscriptionId">Id received on initial subscription.</param>
        /// <param name = "onComplete" > Successful callback returns true value and a <see cref="Checkout.SubscriptionData"/> object.
        /// On failure, returns false and error response.</param>
        public void GetSubscriptionData(string subscriptionId, action onComplete)
        {
            checkout.GetSubscriptionData(subscriptionId, (success, data) => {
                onComplete?.Invoke(success, data);
            });
        }

        /// <summary>
        /// Unpauses the current subscription, so users will start to be charged again.
        /// </summary>
        /// <param name="subscriptionId">Id received on initial subscription.</param>
        /// <param name="onComplete">Successful callback returns true value and 
        /// either the subscription Id (Stripe) or an empty object (PayPal). 
        /// On failure, returns false and error response.</param>
        public void ActivateSubscription(string subscriptionId, action onComplete)
        {
            checkout.ActivateSubscription(subscriptionId, onComplete);
        }

        /// <summary>
        /// Pauses the current subscription, so users will not be charged anymore.
        /// </summary>
        /// <param name="subscriptionId">Id received on initial subscription.</param>
        /// <param name="onComplete">Successful callback returns true value and 
        /// either the subscription Id (Stripe) or an empty object (PayPal). 
        /// On failure, returns false and error response.</param>
        public void DeactivateSubscription(string subscriptionId, action onComplete)
        {
            checkout.DeactivateSubscription(subscriptionId, onComplete);
        }
    }
}
#endif