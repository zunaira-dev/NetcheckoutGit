#if NETCHECKOUT_PLAYMAKER
using UnityEngine;
using NetCheckout.PlayMaker;

namespace HutongGames.PlayMaker.Actions.NetCheckout
{
    [ActionCategory("Net Checkout")]
    [Tooltip("The first window shown when subscribing to a plan. Customize various text/image properties of the window.")]
    public class SetSubscriptionWindow : FsmStateAction
    {
        [UIHint(UIHint.FsmString)]
        [Tooltip("Window's header text. If left blank, default values will be used.")]
        public FsmString headerText;

        [UIHint(UIHint.FsmString)]
        [Tooltip("Window's body text. If left blank, default values will be used.")]
        public FsmString bodyText;

        [UIHint(UIHint.FsmString)]
        [Tooltip("Text on the subscribe button. If left blank, default values will be used.")]
        public FsmString buttonText;

        [UIHint(UIHint.FsmInt)]
        [Tooltip("Array index of the Window prefab to use, set in WindowController prefab (see Resources folder). If left blank, default values will be used.")]
        public FsmInt prefabIndex;

        [Tooltip("Sprite to use in the window. Set prefabIndex to 1 for images (or set up your own prefabs in the WindowController prefab in the Resources folder). If left blank, default values will be used.")]
        public Sprite sprite;

        public override void Reset()
        {
            headerText = null;
            bodyText = null;
            buttonText = null;
            prefabIndex = null;
            sprite = null;
        }

        public override void OnEnter()
        {
            DoAction();

            Finish();
        }

        private void DoAction()
        {
            var orderConfig = PlayMakerCheckout.Instance.MessageConfig.subscribeWindow;
            if (headerText.Value != string.Empty)
                orderConfig.header = headerText.Value;
            if (bodyText.Value != string.Empty)
                orderConfig.body = bodyText.Value;
            if (buttonText.Value != string.Empty)
                orderConfig.buttonTitle = buttonText.Value;
            if (prefabIndex.Value != 0)
                orderConfig.prefabIndex = prefabIndex.Value;
            if (sprite)
                orderConfig.sprite = sprite;
        }
    }
}
#endif