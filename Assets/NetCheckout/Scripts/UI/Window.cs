using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace NetCheckout
{
    /// <summary>
    /// Main message window for NetCheckout and used by <see cref="WindowController"/>. 
    /// Populated with <see cref="MessageConfig"/> data. May be subclassed for custom use cases.
    /// </summary>
    public class Window : MonoBehaviour
    {
        public Text header;
        public Text body;
        public Image image;
        public Button button;

        public bool IsDisplayed => gameObject.activeInHierarchy;

        private void Awake()
        {
            SetButton("OK", Hide);
        }

        public void SetHeader(string text)
        {
            header.text = text;
        }

        public void SetImage(Sprite sprite)
        {
            image.sprite = sprite;
        }

        public void SetBody(string text)
        {
            body.text = text;
        }

        public void SetButton(string text, UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action ?? Hide);
            button.onClick.AddListener(HideButton);

            button.GetComponentInChildren<Text>().text = text;
            SetButtonActive(true);
        }

        public void SetButtonActive(bool active)
        {
            button.gameObject.SetActive(active);
        }

        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HideButton()
        {
            button.gameObject.SetActive(false);
        }
    }
}