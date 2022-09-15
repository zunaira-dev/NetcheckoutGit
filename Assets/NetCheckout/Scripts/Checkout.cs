using UnityEngine;
using UnityEngine.Events;

namespace NetCheckout
{
    /// <summary>
    ///  Wrapper around the checkout client interface, but also contains 
    ///  some convenience functions that simplify accessing payment data.
    /// </summary>
    public class Checkout
    {
        public readonly ICheckoutClient client;

        #region Object Classes
        public class SubscriptionData
        {
            public SubscriptionData(string plan, double price, string period, int intervals, string status, bool active)
            {
                this.plan = plan;
                this.price = price;
                this.period = period;
                this.intervals = intervals;
                this.active = active;
                this.status = status;
            }

            public string plan;
            public double price;
            public string period;
            public int intervals;
            public bool active;
            public string status;
        }

        public class OrderData
        {
            public OrderData(string product, double price, double total, int quantity, string currency)
            {
                this.product = product;
                this.price = price;
                this.total = total;
                this.quantity = quantity;
                this.currency = currency;
            }

            public string product;
            public double price;
            public double total;
            public int quantity;
            public string currency;
        }
        #endregion

        /// <summary>
        /// Stores UI messages displayed to the user.
        /// </summary>
        public MessageConfig MessageConfig
        {
            get => client.MessageConfig;
            set => client.MessageConfig = value;
        }

        /// <summary>
        /// Initializes a new instance with a checkout client (i.e. <see cref="PayPalClient"/> or <see cref="StripeClient"/>).
        /// </summary>
        public Checkout(ICheckoutClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Charge users a one-time fee.
        /// </summary>
        /// <param name="price">amount to charge (i.e. 4.99)</param>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public void Buy(string itemName, string price, int quantity, action onComplete)
        {
            client.CreateOrder(itemName, price, quantity, onComplete);
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
            client.CreateSubscription(planName, price, period, intervals, onComplete);
        }

        /// <summary>
        /// Gets data in the <see cref="OrderData" class/>.
        /// </summary>
        /// <param name="orderId">The id returned after making a successful purchase.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="OrderData"/> object. 
        /// On failure, returns false and error response.</param>
        public void GetOrderData(string orderId, action onComplete)
        {
            client.GetOrderDetails(orderId, (success, data) =>
            {
                if (!success)
                    onComplete?.Invoke(false, data);
                else if (client is PayPalClient)
                    onComplete(true, CreatePayPalOrderData(data));
                else if (client is StripeClient)
                    onComplete(true, CreateStripeOrderData(data));
            });
        }

        /// <summary>
        /// Gets data in the <see cref="SubscriptionData"/> class.
        /// </summary>
        /// <param name="subscriptionId">Id received on initial subscription.</param>
        /// <param name = "onComplete" > Successful callback returns true value and a <see cref="SubscriptionData"/> object.
        /// On failure, returns false and error response.</param>
        public void GetSubscriptionData(string subscriptionId, action onComplete)
        {
            client.GetSubscriptionDetails(subscriptionId, (success, data) =>
            {
                if (!success)
                    onComplete?.Invoke(false, data);
                else if (client is PayPalClient)
                    onComplete(true, CreatePayPalSubscriptionData(data));
                else if (client is StripeClient)
                    onComplete(true, CreateStripeSubscriptionData(data));
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
            SetSubscriptionStatus(subscriptionId, id =>
            {
                client.ActivateSubscription(id, (success, res) => onComplete?.Invoke(success, res));
            });
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
            SetSubscriptionStatus(subscriptionId, id =>
            {
                client.DeactivateSubscription(id, (success, res) => onComplete?.Invoke(success, res));
            });
        }

        private void SetSubscriptionStatus(string subscriptionId, UnityAction<string> statusAction)
        {
            client.GetSubscriptionDetails(subscriptionId, (success, plan) =>
            {
                string id = string.Empty;

                if (client is PayPalClient)
                    id = ((PayPalClient.Plan)plan).id;
                else if (client is StripeClient)
                    id = ((StripeClient.Subscription)plan).id;

                statusAction.Invoke(id);
            });
        }

        private OrderData CreatePayPalOrderData(object data)
        {
            var total = PayPalClient.GetPriceFromOrderDetails(data);
            var currency = PayPalClient.GetCurrencyFromOrderDetails(data);
            var item = ((PayPalClient.Order)data).purchase_units[0].items[0];
            var product = item.name;
            var price = double.Parse(item.unit_amount.value);
            var quantity = int.Parse(item.quantity);

            return new OrderData(product, price, total, quantity, currency);
        }

        private OrderData CreateStripeOrderData(object data)
        {
            var total = StripeClient.GetPriceFromOrderDetails(data);
            var currency = StripeClient.GetCurrencyFromOrderDetails(data);
            var item = (StripeClient.Session)data;
            var product = item.metadata.product;
            var price = double.Parse(item.metadata.unit_price) / 100d;
            var quantity = int.Parse(item.metadata.quantity);

            return new OrderData(product, price, total, quantity, currency);
        }

        private SubscriptionData CreatePayPalSubscriptionData(object data)
        {
            var price = PayPalClient.GetPriceFromSubscriptionDetails(data);
            var planData = (PayPalClient.Plan)data;
            string plan = planData.name;
            string status = planData.status;
            bool active = status.ToLower() == "active";
            int intervals = planData.billing_cycles[0].frequency.interval_count;
            string period = planData.billing_cycles[0].frequency.interval_unit;
            
            return new SubscriptionData(plan, price, period, intervals, status, active);
        }

        private SubscriptionData CreateStripeSubscriptionData(object data)
        {
            var planData = ((StripeClient.Subscription)data);
            var priceData = planData.items.data[0].price;
            var price = StripeClient.GetPriceFromSubscriptionDetails(data);
            var plan = priceData.nickname;
            bool active = string.IsNullOrEmpty(planData.pause_collection.behavior);
            string status = active ? "active" : "inactive";
            var intervals = priceData.recurring.interval_count;
            var period = priceData.recurring.interval;

            return new SubscriptionData(plan, price, period, intervals, status, active);
        }
    }
}