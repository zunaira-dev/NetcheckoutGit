#if NETCHECKOUT_PLAYMAKER
using NetCheckout;
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("First action to call for NetCheckout. Specify the client to use (i.e. PayPal or Stripe)")]
    public class SetClient : FsmStateAction
    {
        [UIHint(UIHint.FsmEnum)] 
        [Tooltip("The NetCheckout client to use.")]
        public PlayMakerCheckout.CheckoutClient client;

        public override void Reset()
        {
            client = 0;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }
        
        private void DoAction()
        {
            ICheckoutClient clientInterface;
            switch (client)
            {
                case PlayMakerCheckout.CheckoutClient.PayPal:
                    clientInterface = new PayPalClient();
                    break;
                default:
                    clientInterface = new StripeClient();
                    break;
            }
            PlayMakerCheckout.Instance.SetClient(clientInterface);
        }
    }
}
#endif