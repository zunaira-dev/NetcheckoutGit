using UnityEngine;

namespace NetCheckout.Demo
{
    public class DemoSimple : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            NetCheckout.StripeClient.Buy("My Item", "4.99", 1);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}