#if NETCHECKOUT_PLAYMAKER
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("Manually set the client secret (PayPal) or secret key (Stripe).")]
    public class SetClientSecret : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        [Tooltip("The client secret to use.")]
        public FsmString secret;

        public override void Reset()
        {
            secret = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            PlayMakerCheckout.Instance.SetClientSecret(secret.Value);
        }
    }
}
#endif