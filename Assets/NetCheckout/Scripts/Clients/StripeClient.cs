using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace NetCheckout
{
    /// <summary>
    /// Used for interacting with the Stripe APIs.
    /// </summary>
    public class StripeClient : BaseClient, ICheckoutClient
    {
        /// <summary>
        /// Stores UI messages displayed to the user.
        /// </summary>
        public MessageConfig MessageConfig { get; set; }

        /// <summary>
        /// How often to call Stripe API to check for user approval of the order.
        /// In seconds.
        /// </summary>
        public float approvalTimeInterval = 1f;

        private static bool IsValid(long code) => (int)code / 100 == 2;

        #region Object Classes
        [Serializable]
        public class Product
        {
            public string id;
            public bool active;
            public string[] attributes;
            public long created;
            public string description;
            public string images;
            public bool livemode;
            public object metadata;
            public string name;
            public string statement_descriptor;
            public string type;
            public string unit_label;
            public long updated;
        }

        [Serializable]
        public class Price
        {
            public string id;
            public string active;
            public string billing_scheme;
            public long created;
            public string currency;
            public bool livemode;
            public string lookup_key;
            public object metadata;
            public string nickname;
            public string product;
            public Recurring recurring;
            public string tiers_mode;
            public string type;
            /// <summary>In cents.</summary>
            public int unit_amount;
            public string unit_amount_decimal;

            [Serializable]
            public class Recurring
            {
                public string aggregate_usage;
                public string interval;
                public int interval_count;
                public string usage_type;
            }
        }

        [Serializable]
        public class Session
        {
            public string id;
            public string mode;
            public string currency;
            public string cancel_url;
            public string success_url;
            /// <summary>In cents.</summary>
            public int amount_total;
            public string client_reference_id;
            public string customer;
            public string customer_email;
            public CustomData metadata;
            public string subscription;
            public string payment_intent;
            public string url;

            [Serializable]
            public class CustomData // see CreateCheckoutSession()
            {
                public string product;
                public string quantity;
                public string unit_price;
            }
        }

        [Serializable]
        public class PaymentIntent
        {
            public string id;
            /// <summary>In cents.</summary>
            public int amount;
            public string client_secret;
            public string currency;
            public string customer;
            public string description;
            public string payment_method;
            public string[] payment_method_types;
            public string receipt_email;
            public string setup_future_usage;
            public string statement_descriptor;
            public string status;
            public int amount_received;
        }

        [Serializable]
        public class Subscription
        {
            public string id;
            public bool cancel_at_period_end;
            public string customer;
            public LineItems items;
            public string status;
            public string quantity;
            public PauseSubscription pause_collection;

            public bool IsActive => status == "active";
        }

        [Serializable]
        public class PauseSubscription
        {
            public string behavior;
            public string resumes_at;
        }

        [Serializable]
        public class Customer
        {
            public string id;
            public string email;
            public string name;
        }

        [Serializable]
        public class LineItems
        {
            public LineItem[] data;
            public bool has_more;
            public string url;

            [Serializable]
            public class LineItem
            {
                public string id;
                public int amount_subtotal;
                /// <summary>In cents.</summary>
                public int amount_total;
                public string description;
                public Price price;
                public int quantity;
            }
        }

        protected class RecurringPayment
        {
            public PaymentPeriod interval;
            public int count;
        }
        #endregion

        /// <summary>
        /// Initializes a new client instance. Loads MessageConfig field with default data.
        /// </summary>
        public StripeClient()
        {
            MessageConfig = MessageConfig.GetDefaultConfig();
        }

        /// <summary>
        /// Set the Stripe secret key associated with your developer account.
        /// In editor, it will set the test secret key. In a build, it will set the live secret key.
        /// </summary>
        public override void SetClientSecret(string secret)
        {
            settings.PayPalClientSecret = secret;
        }

        /// <summary>
        /// Shortcut function for making one-time purchases.
        /// </summary>
        /// <param name="price">i.e. 4.99</param>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public static void Buy(string itemName, string price, int quantity, action onComplete = null)
        {
            new StripeClient().CreateOrder(itemName, price, quantity, onComplete);
        }

        /// <summary>
        /// Shortcut function for making recurring purchases.
        /// </summary>
        /// <param name="itemName">name of subscription (i.e. Gold Plan)</param>
        /// <param name="price">amount to charge (i.e. 7.99)</param>
        /// <param name="period">day, week, month, year</param>
        /// <param name="intervals">number of periods. To bill every 3 months, set period to month and intervals to 3.</param>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public static void Subscribe(string itemName, string price, PaymentPeriod period, int intervals, action onComplete = null)
        {
            new StripeClient().CreateSubscription(itemName, price, period, intervals, onComplete);
        }

        /// <summary>
        /// First call to initiate the NetCheckout process of making a one-time purchase. 
        /// Creates an order on Stripe's servers that the user must confirm to complete the purchase.
        /// </summary>
        /// <param name="onComplete">Callback. If successful, returns true and the session id. If failed, returns false and the error response.</param>
        public void CreateOrder(string itemName, string price, int quantity, action onComplete)
        {
            WindowController.Instance.DisplayWindow(
                MessageConfig.orderWindow,
                () => CallApiToCreateOrder(itemName, price, quantity, onComplete)
            );
        }

        private void CallApiToCreateOrder(string itemName, string price, int amount, action onComplete, RecurringPayment subscription = null)
        {
            CallApiToCreateProduct(itemName, (success, productId) =>
            {
                if (success)
                {
                    CallApiToCreatePrice((string)productId, itemName, price, subscription, (createdPrice, data) =>
                    {
                        if (!createdPrice)
                            onComplete?.Invoke(false, data);
                        else
                        {
                            string mode = subscription != null ? "subscription" : "payment";
                            CreateCheckoutSession((Price)data, amount, mode, onComplete);
                        }
                    });
                }
                else
                    onComplete?.Invoke(false, productId);
            });
        }

        /// <summary>
        /// First call to initiate the NetCheckout process of making a recurring purchase. 
        /// Creates an order on Stripe's servers that the user must confirm to complete the purchase.
        /// </summary>
        /// <param name="itemName">name of subscription (i.e. Gold Plan)</param>
        /// <param name="price">amount to charge (i.e. 7.99)</param>
        /// <param name="period">day, week, month, year</param>
        /// <param name="intervals">number of periods. To bill every 3 months, set period to month and intervals to 3.</param>
        /// <param name="onComplete">Callback. If successful, returns true and the session id. If failed, returns false and the error response.</param>
        public void CreateSubscription(string itemName, string price, PaymentPeriod period, int intervals, action onComplete)
        {
            var subscription = new RecurringPayment() { interval = period, count = intervals };

            WindowController.Instance.DisplayWindow(
                MessageConfig.subscribeWindow,
                () => CallApiToCreateSubscription(itemName, price, 1, onComplete, subscription)
            );
        }

        private void CallApiToCreateSubscription(string itemName, string price, int amount, action onComplete, RecurringPayment subscription)
        {
            CallApiToCreateOrder(itemName, price, amount, onComplete, subscription);
        }

        private void CallApiToCreateProduct(string itemName, action onComplete)
        {
            var itemData = new List<(string, string)>() { ("name", itemName) };
            
            Request("POST", "v1/products", itemData, (status, res) =>
            {
                object id = IsValid(status) ? JsonUtility.FromJson<Product>(res).id : res;
                onComplete?.Invoke(IsValid(status), id);
            });
        }
        
        private void CallApiToCreatePrice(string productId, string itemName, string unitPrice, RecurringPayment subscription, action onComplete)
        {
            var data = new List<(string, string)>()
            {
                ("nickname", itemName),
                ("unit_amount", PriceToCents(unitPrice)),
                ("currency", settings.currencyCode),
                ("product", productId)
            };
            
            if (subscription != null)
            {
                data.Add(("recurring[interval]", subscription.interval.ToString()));
                data.Add(("recurring[interval_count]", subscription.count.ToString()));
            }

            Request("POST", "v1/prices", data, (status, res) =>
            {
                object price = IsValid(status) ? JsonUtility.FromJson<Price>(res) : (object)res;
                onComplete?.Invoke(IsValid(status), price);
            });
        }

        private void CreateCheckoutSession(Price price, int amount, string mode, action onComplete)
        {
            var data = new List<(string, string)>()
            {
                ("mode", mode),
                ("payment_method_types[0]", "card"),
                ("metadata[product]", price.nickname),
                ("metadata[quantity]", amount.ToString()),
                ("metadata[unit_price]", price.unit_amount.ToString()),
                ("line_items[0][price]", price.id),
                ("line_items[0][quantity]", amount.ToString()),
                ("success_url", settings.stripeSuccessUrl),
                ("cancel_url", settings.stripeCancelUrl)
            };

            Request("POST", "v1/checkout/sessions", data, (status, res) =>
            {
                if (!IsValid(status))
                    onComplete?.Invoke(false, res);
                else
                    LaunchStripeCheckout(JsonUtility.FromJson<Session>(res), price, onComplete);
            });
        }

        private void LaunchStripeCheckout(Session session, Price price, action onComplete)
        {
            WaitForApproval(session.id, onComplete);

#if UNITY_EDITOR || !UNITY_WEBGL 
            string redirectPath = GetCheckoutRedirectPath();
            byte[] htmlBytes = Resources.Load<TextAsset>("Redirect/redirect_template").bytes;
            
            WriteHtmlBytes(redirectPath, htmlBytes, session.id);
#if UNITY_IOS && !UNITY_EDITOR
            redirectPath = $"file:///{redirectPath}";
#endif
            Utility.OpenURL(redirectPath);
#else
            Utility.OpenURL(session.url);
#endif
        }

        private string GetCheckoutRedirectPath()
        {
            var dataPath = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
            return Path.Combine(dataPath, "stripe_redirect.html");
        }

        private void CallStripeRedirectUrl(string sessionId)
        {
            string relativeUrl = string.Format("stripe-redirect.html?session_id={0}&pkey={1}", sessionId, settings.StripePublishableKey);
            
            Utility.OpenURL(Path.Combine(Application.absoluteURL, relativeUrl));
        }

        private void WriteHtmlBytes(string path, byte[] bytes, string sessionId)
        {
            using (FileStream toStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                string html = Utility.GetUTF8String(bytes);
                var fields = GetHtmlFieldReplacements(sessionId);

                html = ReplaceAll(fields, html);
                byte[] htmlBytes = Utility.GetBytes(html);
                toStream.Write(htmlBytes, 0, htmlBytes.Length);
            }
        }

        private List<(string, string)> GetHtmlFieldReplacements(string sessionId)
        {
            List<(string, string)> fields = new List<(string, string)>()
            {
                ("STRIPE_SESSION_ID", sessionId),
                ("STRIPE_PKEY", settings.StripePublishableKey)
            };
            return fields;
        }

        private void DeleteCheckoutRedirectFile()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            File.Delete(GetCheckoutRedirectPath());
            
            if (Application.isEditor)
                File.Delete(GetCheckoutRedirectPath() + ".meta");
#endif
        }

        /// <summary>
        /// Call Stripe API periodically to check for user approval of order.
        /// </summary>
        /// <param name="sessionId">Received from Stripe API.</param>
        public void WaitForApproval(string sessionId, action onComplete)
        {
            // display UI waiting for approval
            WindowController.Instance.DisplayWindowWithoutButton(MessageConfig.waitWindow);

            // automatically close Android browser window upon payment success
#if UNITY_ANDROID
            string url = Settings.stripeApiUrl + string.Format("v1/checkout/sessions/{0}", sessionId);
            string key = settings.StripeSecretKey;
            (string, string) validator = ("customer", "not_null");
            AndroidHelper.SetupNativeUrlCloseEvent(url, key, approvalTimeInterval, validator, settings.androidAppUri);
#endif

            Coroutiner.Instance.StartCoroutine(WaitForApprovalCoroutine(sessionId, onComplete));
        }

        private IEnumerator WaitForApprovalCoroutine(string sessionId, action onComplete)
        {
            bool waitingOnApproval = true;
            bool gettingOrderDetails = false;
            Session session = null;

            while (waitingOnApproval && WindowController.Instance.Window.IsDisplayed)
            {
                yield return new WaitForSeconds(approvalTimeInterval);

                if (gettingOrderDetails)
                    continue;

                gettingOrderDetails = true;

                // get order details
                Request("GET", string.Format("v1/checkout/sessions/{0}", sessionId), null, (status, res) =>
                {
                    session = JsonUtility.FromJson<Session>(res);

                    if (IsValid(status) && !string.IsNullOrEmpty(session.customer))
                        waitingOnApproval = false;

                    gettingOrderDetails = false;
                });
            }

            DeleteCheckoutRedirectFile();

            if (!WindowController.Instance.Window.IsDisplayed)
                yield break;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // notify app window
            SetForegroundWindow(windowHandle);
#elif UNITY_IOS && !UNITY_EDITOR
            Utility.CloseNativeUrl();
#endif

            var window = string.IsNullOrEmpty(session?.subscription) ? MessageConfig.completeWindow.success : MessageConfig.subscribeCompleteWindow;
            WindowController.Instance.DisplayWindow(window, () => WindowController.Instance.HideWindow());
            onComplete?.Invoke(true, sessionId);
        }

        /// <summary>
        /// Capture payment from the user. <b>Note:</b> this is NOT used for the Stripe 
        /// client as purchases are automatically captured on Stripe's checkout page.
        /// </summary>
        /// <param name="orderId"></param>
        public void ConfirmPurchase(string orderId, action onComplete)
        {
            throw new NotImplementedException("Stripe client does not use this method. Purchase is completed automatically after approval.");
        }

        /// <summary>
        /// Make a raw API request to Stripe.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="endPoint">i.e. v1/products</param>
        /// <param name="postData">Tuple data array of fields to post/patch.</param>
        /// <param name="callback">Returns server response in JSON format (status code, response string).</param>
        public void Request(string method, string endPoint, List<(string, string)> postData, UnityAction<long, string> callback)
        {
            UnityWebRequest request = new UnityWebRequest(Settings.stripeApiUrl + endPoint, method);

            request.SetRequestHeader("Authorization", "Bearer " + settings.StripeSecretKey);
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            byte[] formData = CreateFormData(postData);
                
            if (postData != null && formData.Length > 0)
                request.uploadHandler = new UploadHandlerRaw(formData) { contentType = "application/x-www-form-urlencoded" };

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SendWebRequest().completed += _ =>
            {
                callback(request.responseCode, request.downloadHandler.text);
            };
        }

        private byte[] CreateFormData(List<(string, string)> data)
        {
            if (data == null)
                return new byte[0];
            
            var form = new WWWForm();
            foreach (var field in data)
                form.AddField(field.Item1, field.Item2);
            
            return form.data;
        }

        /// <summary>
        /// Gets data in the <see cref="Session" class/>.
        /// </summary>
        /// <param name="sessionId">The id returned after making a successful purchase.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="Session"/> instance. 
        /// On failure, returns false and error response.</param>
        public void GetOrderDetails(string sessionId, action onComplete)
        {
            Request("GET", string.Format("v1/checkout/sessions/{0}", sessionId), null, (status, res) =>
            {
                object session = IsValid(status) ? JsonUtility.FromJson<Session>(res) : (object)res;
                onComplete?.Invoke(IsValid(status), session);
            });
        }

        /// <summary>
        /// Unpauses the current subscription, so users will start to be charged again.
        /// Note: The subscription ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="subscriptionId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="onComplete">Successful callback returns true value and the subscription Id. On failure, returns false and error response.</param>
        public void ActivateSubscription(string subscriptionId, action onComplete)
        {
            SetSubscriptionStatus(subscriptionId, true, onComplete);
        }

        /// <summary>
        /// Pauses the current subscription, so users will not be charged anymore.
        /// Note: The subscription ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="subscriptionId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="onComplete">Successful callback returns true value and the subscription Id. On failure, returns false and error response.</param>
        public void DeactivateSubscription(string subscriptionId, action onComplete)
        {
            SetSubscriptionStatus(subscriptionId, false, onComplete);
        }

        private void SetSubscriptionStatus(string subscriptionId, bool active, action onComplete)
        {
            var planData = new List<(string, string)>()
            {
                active ?
                ("pause_collection", string.Empty) :
                ("pause_collection[behavior]", "void")
            };

            Request("POST", string.Format("v1/subscriptions/{0}", subscriptionId), planData, (status, res) =>
            {
                object id = IsValid(status) ? JsonUtility.FromJson<Subscription>(res).id : res;
                onComplete?.Invoke(IsValid(status), id);
            });
        }

        /// <summary>
        /// Note: The subscription ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="subscriptionId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="price">the new price (i.e. 12.99)</param>
        /// <param name="onComplete">Successful callback returns true value and the subscription Id. On failure, returns false and error response.</param>
        public void UpdateSubscriptionPricing(string subscriptionId, string price, action onComplete)
        {
            Request("GET", string.Format("v1/subscriptions/{0}", subscriptionId), null, (status, res) =>
            {
                if (!IsValid(status))
                    onComplete?.Invoke(false, res);
                else
                    PostApiToUpdatePricing(JsonUtility.FromJson<Subscription>(res), price, onComplete);
            });
        }

        private void PostApiToUpdatePricing(Subscription subscription, string price, action onComplete)
        {
            Price originalPrice = subscription.items.data[0].price;

            var planData = new List<(string, string)>()
            {
                ("items[0][id]", subscription.items.data[0].id),
                ("items[0][price_data][currency]", settings.currencyCode),
                ("items[0][price_data][product]", originalPrice.product),
                ("items[0][price_data][recurring][interval]", originalPrice.recurring.interval),
                ("items[0][price_data][unit_amount]", PriceToCents(price))
            };

            Request("POST", string.Format("v1/subscriptions/{0}", subscription.id), planData, (status, res) =>
            {
                object id = IsValid(status) ? subscription.id : res;
                onComplete?.Invoke(IsValid(status), id);
            });
        }

        /// <summary>
        /// Gets data in the <see cref="Subscription" class/>.
        /// </summary>
        /// <param name="sessionId">The id returned after creating a successful subscription.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="Subscription"/> instance. 
        /// On failure, returns false and error response.</param>
        public void GetSubscriptionDetails(string sessionId, action onComplete)
        {
            GetOrderDetails(sessionId, (success, data) =>
            {
                if (!success)
                {
                    onComplete?.Invoke(false, data);
                    return;
                }

                string subscriptionId = ((Session)data).subscription;
                Request("GET", string.Format("v1/subscriptions/{0}", subscriptionId), null, (status, res) =>
                {
                    object subscription = IsValid(status) ? JsonUtility.FromJson<Subscription>(res) : (object)res;
                    onComplete?.Invoke(IsValid(status), subscription);
                });
            });
            
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetOrderDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Session"/> data</param>
        /// <returns>currency code (i.e. USD)</returns>
        public static string GetCurrencyFromOrderDetails(object data)
        {
            return ((Session)data).currency;
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetOrderDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Session"/> data</param>
        /// <returns>price (i.e. 3.99)</returns>
        public static double GetPriceFromOrderDetails(object data)
        {
            return ((Session)data).amount_total / 100d;
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Subscription"/> data</param>
        /// <returns>currency code (i.e. USD)</returns>
        public static string GetCurrencyFromSubscriptionDetails(object data)
        {
            return ((Subscription)data).items.data[0].price.currency;
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Subscription"/> data</param>
        /// <returns>price (i.e. 3.99)</returns>
        public static double GetPriceFromSubscriptionDetails(object data)
        {
            return ((Subscription)data).items.data[0].price.unit_amount / 100d;
        }

        private static string PriceToCents(string price)
        {
            return (float.Parse(price) * 100).ToString();
        }

        private static string ReplaceAll(List<(string, string)> fields, string input)
        {
            foreach (var field in fields)
                input = input.Replace(field.Item1, field.Item2);
            return input;
        }
    }
}