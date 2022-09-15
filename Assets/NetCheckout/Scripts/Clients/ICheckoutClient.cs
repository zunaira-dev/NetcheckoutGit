
namespace NetCheckout
{
    public delegate void action(bool success, object data);
    
    public interface ICheckoutClient
    {
        MessageConfig MessageConfig { get; set; }

        void CreateOrder(string itemName, string price, int amount, action onComplete);

        void WaitForApproval(string orderId, action onComplete);

        void ConfirmPurchase(string orderId, action onComplete);

        void GetOrderDetails(string orderId, action onComplete);

        void CreateSubscription(string itemName, string price, PaymentPeriod period, int intervals, action onComplete);

        void ActivateSubscription(string planId, action onComplete);

        void DeactivateSubscription(string planId, action onComplete);

        void UpdateSubscriptionPricing(string planId, string price, action onComplete);

        void GetSubscriptionDetails(string subscriptionId, action onComplete);
    }

    public enum PaymentPeriod
    {
        day,
        week,
        month,
        year
    }
}
