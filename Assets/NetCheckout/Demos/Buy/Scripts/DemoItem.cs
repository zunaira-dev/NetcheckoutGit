using UnityEngine;

namespace NetCheckout.Demo
{
    /// <summary>
    /// Represents the data for an item that can be purchased.
    /// </summary>
    public class DemoItem : MonoBehaviour
    {
        public new string name;
        public string price;
        public int quantity;
        public Sprite icon;

        // Start is called before the first frame update
        void Start() { }
    }
}