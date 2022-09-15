using UnityEngine;

namespace NetCheckout.Demo
{
    /// <summary>
    /// Represents the data for a subscription plan.
    /// </summary>
    public class DemoPlan : MonoBehaviour
    {
        public new string name;
        public string price;
        public int quantity;

        public PaymentPeriod period = PaymentPeriod.month;
        public int intervals = 1;

        // Start is called before the first frame update
        void Start() { }
    }
}