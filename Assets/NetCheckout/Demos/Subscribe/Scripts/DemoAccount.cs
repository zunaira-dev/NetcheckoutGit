using UnityEngine;
using UnityEngine.UI;

namespace NetCheckout.Demo
{
    /// <summary>
    /// Example for handling user account information when creating and managing their subscription.
    /// </summary>
    public class DemoAccount : MonoBehaviour
    {
        public DemoCheckout checkout;

        public Text planText;
        public Text priceText;
        public Text periodText;
        public Text statusText;
        public Button activateButton;
        public Button deactivateButton;

        public GameObject accountPanel;
        public GameObject plansPanel;

        private string subscriptionId;

        // Start is called before the first frame update
        void Start() { }

        public void SelectPlan(DemoPlan plan)
        {
            string header = string.Format("Subscribe for ${0}?", plan.price);
            checkout.SetSubscribeWindowHeader(header);
            checkout.SubscribeToPlan(plan, OnSubscribe);
        }

        private void OnSubscribe(bool success, object data)
        {
            if (!success)
                Debug.LogError("Failed to subscribe. Error details: " + data.ToString());
            else
                DisplayAccount(data.ToString());
        }

        public void DisplayAccount(string subscriptionId)
        {
            this.subscriptionId = subscriptionId;

            PopulateData();

            plansPanel.SetActive(false);
            accountPanel.SetActive(true);
        }

        private void PopulateData()
        {
            checkout.GetSubscriptionData(subscriptionId, (success, data) =>
            {
                if (!success)
                {
                    Debug.LogError("Failed to get account data. Error details: " + data.ToString());
                    Debug.Log("New orders can take a second or two to propagate. Trying again in 3 seconds...");
                    Invoke("PopulateData", 3f);
                }
                else
                {
                    var subscription = (Checkout.SubscriptionData)data;
                    planText.text = subscription.plan;
                    priceText.text = subscription.price.ToString();
                    periodText.text = string.Format("Every {0} {1}{2}", subscription.intervals, subscription.period,
                        (subscription.intervals != 1) ? "s" : "");
                    statusText.text = subscription.status;

                    EnableActivationButton(!subscription.active);
                }
            });
        }

        public void ActivateSubscription()
        {
            activateButton.interactable = false;
            checkout.ActivateSubscription(subscriptionId, (success, data) =>
            {
                activateButton.interactable = true;

                if (!success)
                    Debug.LogError("Failed to activate subscription. Error Details: " + data.ToString());
                else
                    EnableActivationButton(false);

                UpdateDisplay();
            });
        }

        public void DeactivateSubscription()
        {
            deactivateButton.interactable = false;
            checkout.DeactivateSubscription(subscriptionId, (success, data) =>
            {
                deactivateButton.interactable = true;

                if (!success)
                    Debug.LogError("Failed to deactivate subscription. Error Details: " + data.ToString());
                else
                    EnableActivationButton(true);

                UpdateDisplay();
            });
        }

        private void EnableActivationButton(bool enabled)
        {
            activateButton.gameObject.SetActive(enabled);
            deactivateButton.gameObject.SetActive(!enabled);
        }

        private void UpdateDisplay()
        {
            DisplayAccount(subscriptionId);
        }
    }
}