using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NetCheckout
{
    /// <summary>
    /// Manager for ad hoc <see cref="Window"/> creation and display.
    /// Can contain multiple window prefab references. Set prefabIndex in 
    /// a <see cref="MessageConfig"/> instance before displaying the window. 
    /// Or, set an external prefab with <see cref="SetWindow(Window)"/>
    /// </summary>
    public class WindowController : MonoBehaviour
    {
        public Window[] windowPrefabs;
        
        private Window windowHandle;

        private static WindowController instance;

        public static WindowController Instance
        {
            get
            {
                if (instance == null)
                {
                    var prefab = Resources.Load<WindowController>("NetCheckout Window");
                    instance = Instantiate(prefab);
                }
                return instance;
            }
        }

        public Window Window
        {
            get
            {
                if (windowHandle == null)
                {
                    windowHandle = Instantiate(windowPrefabs[0], Instance.transform);
                    windowHandle.Hide();
                }
                return windowHandle;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (EventSystem.current == null)
                CreateEventSystem();
        }

        private void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public void DisplayWindow(MessageConfig.Window config, UnityAction buttonAction)
        {
            DisplayCustomWindow(config, buttonAction);
        }

        public void DisplayWindow(string header, string body, string buttonTitle, UnityAction buttonAction)
        {
            SetWindowButton(buttonTitle, buttonAction);

            DisplayWindow(header, body);
        }
        
        public void DisplayWindow(string header, string body)
        {
            SetWindowHeader(header);
            SetWindowBody(body);
            Window.Display();
        }

        public void DisplayWindowWithoutButton(MessageConfig.Window config)
        {
            DisplayCustomWindow(config, null);
        }

        public void DisplayWindowWithoutButton(string header, string body)
        {
            SetWindowButtonActive(false);

            DisplayWindow(header, body);
        }

        public void HideWindow()
        {
            Window.Hide();
        }

        private void DisplayCustomWindow(MessageConfig.Window config, UnityAction action)
        {
            if (windowHandle != windowPrefabs[config.prefabIndex])
                SetWindow(windowPrefabs[config.prefabIndex]);

            if (action != null)
                SetWindowButton(config.buttonTitle, action);
            else
                SetWindowButtonActive(false);

            SetWindowHeader(config.header);
            SetWindowImage(config.sprite);
            SetWindowBody(config.body);

            // Trigger callback for any custom window updates
            config.onDisplay?.Invoke(Window);

            Window.Display();
        }

        private void SetWindow(Window prefab)
        {
            if (windowHandle != null)
                Destroy(windowHandle.gameObject);

            windowHandle = Instantiate(prefab, Instance.transform);
            windowHandle.Hide();
        }

        private void SetWindowHeader(string text)
        {
            if (Window.header)
                Window.SetHeader(text);
        }

        private void SetWindowBody(string text)
        {
            if (Window.body)
                Window.SetBody(text);
        }

        private void SetWindowButton(string text, UnityAction action)
        {
            if (Window.button)
                Window.SetButton(text, action);
        }

        private void SetWindowButtonActive(bool active)
        {
            if (Window.button)
                Window.SetButtonActive(active);
        }

        private void SetWindowImage(Sprite sprite)
        {
            if (Window.image)
                Window.SetImage(sprite);
        }
    }
}