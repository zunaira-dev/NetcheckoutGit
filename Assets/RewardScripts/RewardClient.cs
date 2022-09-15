using NetCheckout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardClient : MonoBehaviour
{
    [SerializeField]
    private Session session;
    [SerializeField]
    private GameObject AdminPanel;
    [SerializeField]
    private GameObject ClientRequestPanel;
    [SerializeField]
    private GameObject PurchasePanel;
    [SerializeField]
    private Transform _content;
    [SerializeField]
    private Transform _instance;
    
    #region Admin
    public void OpenPanel() {
        AdminPanel.SetActive(false);
        ClientRequestPanel.SetActive(true);
    }
    public void LoadClient() {
        for (int i = 0; i < session.clientNum; i++) {
        Transform client= Instantiate(_instance, _content)as Transform;
        client.transform.GetChild(0).gameObject.GetComponent<Text>().text = session.ids[i];
        }
    }
    public void Paypal(string amount) {
        NetCheckout.PayPalClient.Buy("My Item", amount, 1);
    }
    public void Stripe() {
        NetCheckout.StripeClient.Buy("My Item", "4.99", 1);
    }
    #endregion Admin
    #region Client
    public void OpenPurchasePanel() {
        AdminPanel.SetActive(false);
        PurchasePanel.SetActive(true);
    }
    public void ClosePurchasePanel() {
        AdminPanel.SetActive(true);
        PurchasePanel.SetActive(false);
    }
    #endregion Client
}
