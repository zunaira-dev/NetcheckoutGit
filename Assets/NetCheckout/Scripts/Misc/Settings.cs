using UnityEngine;

namespace NetCheckout
{
    /// <summary>
    /// Stores NetCheckout environment settings.
    /// </summary>
    public class Settings : ScriptableObject
    {
        #region PayPal Configuration

#if UNITY_EDITOR || NETCHECKOUT_DEBUG
        public static string payPalApiUrl =>  "https://api.sandbox.paypal.com/";
        /// <summary>
        /// Uses proper live or test id based on environment.
        /// </summary>
        public string PayPalClientId
        {
            get => payPalSandboxClientId;
            set => payPalSandboxClientId = value;
        }
        
        /// <summary>
        /// Uses proper live or test value based on environment.
        /// </summary>
        public string PayPalClientSecret
        {
            get => payPalSandboxClientSecret;
            set => payPalSandboxClientSecret = value;
        }
#else
        public static readonly string payPalApiUrl = "https://api.paypal.com/";
        
        public string PayPalClientId
        {
            get => payPalClientId;
            set => payPalClientId = value;
        }

        public string PayPalClientSecret
        {
            get => payPalClientSecret;
            set => payPalClientSecret = value;
        }
#endif
        [Space(5)]
        [Header("PayPal Configuration")]

        /// <summary>
        /// Used for testing in editor only.
        /// </summary>
        [Tooltip("Used for testing in editor only.")]
        public string payPalSandboxClientId;

        /// <summary>
        /// Used for testing in editor only.
        /// </summary>
        [Tooltip("Used for testing in editor only.")]
        public string payPalSandboxClientSecret;

        /// <summary>
        /// Used for live transactions in builds.
        /// </summary>
        [Tooltip("Used for live transactions in builds.")]
        public string payPalClientId;

        /// <summary>
        ///  Used for live transactions in builds.
        /// </summary>
        [Tooltip("Used for live transactions in builds.")]
        public string payPalClientSecret;

        /// <summary>
        /// URL that PayPal calls after user approves the order.
        /// </summary>
        [Tooltip("URL that PayPal calls after user approves the order.")]
        public string payPalSuccessUrl = "https://semanticgamesllc.wordpress.com/payment-approved/";

        /// <summary>
        /// URL that PayPal calls if user cancels the order.
        /// </summary>
        [Tooltip("URL that PayPal calls if user cancels the order.")]
        public string payPalCancelUrl = "https://semanticgamesllc.wordpress.com/payment-canceled/";
        #endregion

        #region Stripe Configuration
        public static readonly string stripeApiUrl = "https://api.stripe.com/";

#if UNITY_EDITOR || NETCHECKOUT_DEBUG
        /// <summary>
        /// Uses proper live or test key based on environment.
        /// </summary>
        public string StripePublishableKey
        {
            get => stripeTestPublishableKey;
            set => stripeTestPublishableKey = value;
        }

        /// <summary>
        /// Uses proper live or test key based on environment.
        /// </summary>
        public string StripeSecretKey
        {
            get => stripeTestSecretKey;
            set => stripeTestSecretKey = value;
        }
#else
        public string StripePublishableKey
        {
            get => stripePublishableKey;
            set => stripePublishableKey = value;
        }

        public string StripeSecretKey
        {
            get => stripeSecretKey;
            set => stripeSecretKey = value;
        }
#endif
        [Header("Stripe Configuration")]

        /// <summary>
        ///  Used for testing in editor only.
        /// </summary>
        [Tooltip("Used for testing in editor only.")]
        public string stripeTestPublishableKey = "pk_test_51HAQJ8Ei7aoiR9LTqIx2dUA2N1lHrvN1iyOsuqdEB4AzRJDCvfjSKWbQgrUPrYV9A5dsgo77Rq2TsLKA5KMzD9C300MFdDo7KC";

        /// <summary>
        ///  Used for testing in editor only.
        /// </summary>
        [Tooltip("Used for testing in editor only.")]
        public string stripeTestSecretKey = "sk_test_51HAQJ8Ei7aoiR9LTjHMiOa5nC1glfDitVjqjNAR6naLErWBxT5BnhR1WfEy5rlXm3Kaf3fobcglgmld2VoEJ1oo600LyJsSe4A";

        /// <summary>
        /// Used for live transactions in builds.
        /// </summary>
        [Tooltip("Used for live transactions in builds.")]
        public string stripePublishableKey;

        /// <summary>
        /// Used for live transactions in builds.
        /// </summary>
        [Tooltip("Used for live transactions in builds.")]
        public string stripeSecretKey;

        /// <summary>
        /// URL that Stripe calls after user completes the order.
        /// </summary>
        [Tooltip("URL that Stripe calls after user completes the order.")]
        public string stripeSuccessUrl = "https://semanticgamesllc.wordpress.com/payment-approved/";

        /// <summary>
        /// URL that Stripe calls if user cancels the order.
        /// </summary>
        [Tooltip("URL that Stripe calls if user cancels the order.")]
        public string stripeCancelUrl = "https://semanticgamesllc.wordpress.com/payment-canceled/";
        #endregion

        #region Other Settings
        [Header("Other Settings")]
        [Tooltip("Type of currency to use for purchases. In three-letter ISO-4217 format.")]
        public string currencyCode = "USD";

        /// <summary>
        /// Custom URI to link back to your app after user completes the order.
        /// </summary>
        [Tooltip("Custom URI to link back to your app after user completes the order.")]
        public string androidAppUri = "netcheckout://semanticgamesllc.payment_callback";
        #endregion
    }
}