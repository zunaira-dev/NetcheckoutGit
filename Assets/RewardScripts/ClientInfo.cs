using NetCheckout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientInfo : MonoBehaviour
{
    //[SerializeField]
    //private Text text;
    #region Client
    public void SentMoneyToClient() {
      Application.OpenURL("https://www.paypal.com/us/digital-wallet/send-receive-money/send-money");
    }
    public void Paypal() {
        NetCheckout.PayPalClient.Buy("My Item", "4.99", 1);
    }
    public void Stripe() {
        NetCheckout.StripeClient.Buy("My Item", "4.99", 1);
    }
    #endregion Client
}
