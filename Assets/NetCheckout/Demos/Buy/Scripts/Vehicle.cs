using UnityEngine;
using UnityEngine.EventSystems;

namespace NetCheckout.Demo
{
    /// <summary>
    /// Represents an item that could be purchased if necessary.
    /// </summary>
    public class Vehicle : MonoBehaviour
    {
        public GameObject raceButton;
        public bool locked;
        public DemoCheckout checkout;
        public DemoItem item;

        public static Vehicle selected;

        private bool isRotating;
        private readonly float rotationSpeed = 2f;

        private Vector3 originalPosition;

        // Start is called before the first frame update
        void Start()
        {
            originalPosition = transform.position;
        }

        private void OnMouseEnter()
        {
            // zoom in
            Vector3 dir = Camera.main.transform.position - transform.position;
            transform.position += dir.normalized * 1.3f;
        }

        private void OnMouseExit()
        {
            // reset zoom
            transform.position = originalPosition;
        }

        private void OnMouseUpAsButton()
        {
            // ignore UI clicks
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (locked)
                Buy();
            else
                Select();
        }

        private void Buy()
        {
            string header = string.Format("Buy {0} for ${1}?", item.name, item.price);
            checkout.SetOrderWindowHeader(header);
            checkout.SetOrderWindowImage(1, item.icon);
            checkout.BuyItem(item, OnUnlock);
        }

        public void Select()
        {
            if (selected)
                selected.Unselect();

            selected = this;
            isRotating = true;
            raceButton.SetActive(true);
        }

        public void Unselect()
        {
            selected = null;
            isRotating = false;
            raceButton.SetActive(false);
        }

        private void OnUnlock(bool success, object data)
        {
            if (success)
            {
                Debug.Log("Order ID: " + data.ToString());
                locked = false;
                Select();
            }
            else
                Debug.LogError(data.ToString());
        }

        // Update is called once per frame
        void Update()
        {
            if (isRotating)
                transform.Rotate(Vector3.up * rotationSpeed);
        }
    }
}