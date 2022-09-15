using UnityEngine;

namespace NetCheckout
{
    /// <summary>
    /// Creates ad hoc coroutines. Useful for object 
    /// instances that don't inherit from monobehaviour.
    /// </summary>
    public class Coroutiner : MonoBehaviour
    {
        private static Coroutiner instance;
        
        public static Coroutiner Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject("Coroutine Handler").AddComponent<Coroutiner>();
                return instance;
            }
        }
    }
}