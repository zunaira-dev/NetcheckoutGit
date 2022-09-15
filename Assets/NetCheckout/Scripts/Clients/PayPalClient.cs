using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace NetCheckout
{
    /// <summary>
    /// Used for interacting with the PayPal APIs.
    /// </summary>
    public class PayPalClient : BaseClient, ICheckoutClient
    {
        /// <summary>
        /// Stores UI messages displayed to the user.
        /// </summary>
        public MessageConfig MessageConfig { get; set; }

        /// <summary>
        /// How often to call PayPal API to check for user approval of the order.
        /// In seconds.
        /// </summary>
        public float approvalTimeInterval = 1f;

        private bool HasAccessToken => !string.IsNullOrEmpty(accessToken);

        private static bool IsValid(long code) => (int)code / 100 == 2;

        private static string accessToken;

        #region Object Classes
        [Serializable]
        public class Order
        {
            public string create_time;
            public string update_time;
            public string id;
            public string intent;
            public Payer payer;
            public PurchaseUnit[] purchase_units;
            public string status;
            public Link[] links;
        }

        [Serializable]
        public class OrderRequest
        {
            public string note;
            public string capture_type;
            public Amount amount;
        }

        [Serializable]
        public class Link
        {
            public string href;
            public string rel;
            public string method;
        }

        [Serializable]
        public class Payer
        {
            public Name name;
            public string email_address;
            public string payer_id;
        }

        [Serializable]
        public class Name
        {
            public string given_name;
            public string surname;
        }

        [Serializable]
        public class Item
        {
            public string intent = "CAPTURE";
            public ApplicationContext application_context = new ApplicationContext();
            public PurchaseUnit[] purchase_units = new PurchaseUnit[1];

            public string ReturnUrl
            {
                get => application_context.return_url;
                set => application_context.return_url = value;
            }

            public string CancelUrl
            {
                get => application_context.cancel_url;
                set => application_context.cancel_url = value;
            }

            public Item(string name, string price, int amount, string currencyCode)
            {
                purchase_units[0] = new PurchaseUnit(price, amount, name, currencyCode);
            }
        }

        [Serializable]
        public class ApplicationContext
        {
            public string return_url;
            public string cancel_url;
        }

        [Serializable]
        public class PurchaseUnit
        {
            public AmountWithBreakdown amount;
            public string description;
            public Item[] items = new Item[1];

            public PurchaseUnit(string value, int quantity, string name, string currencyCode)
            {
                Amount total = new Amount()
                {
                    value = (float.Parse(value) * quantity).ToString(),
                    currency_code = currencyCode
                };

                amount = new AmountWithBreakdown(total.value, currencyCode);
                amount.breakdown = new AmountBreakdown(total);

                items[0] = new Item()
                {
                    name = name,
                    unit_amount = new Amount()
                    {
                        value = value,
                        currency_code = currencyCode
                    },
                    quantity = quantity.ToString()
                };
            }

            [Serializable]
            public class Item
            {
                public string name;
                public Amount unit_amount;
                public string quantity;
            }
        }
        
        [Serializable]
        public class Amount
        {
            public string currency_code;
            public string value;
        }

        [Serializable]
        public class AmountBreakdown
        {
            public AmountBreakdown(Amount total)
            {
                item_total = total;
            }
            public Amount item_total;
        }

        [Serializable]
        public class AmountWithBreakdown : Amount
        {
            public AmountWithBreakdown(string value, string currencyCode)
            {
                this.value = value;
                currency_code = currencyCode;
            }
            public AmountBreakdown breakdown;
        }

        [Serializable]
        public class BaseProduct
        {
            public string name;
            public string type;
        }

        [Serializable]
        public class Product : BaseProduct
        {
            public string id;
            public string description;
            public string category;
            public string image_url;
            public string home_url;
        }

        [Serializable]
        public class BasePlan
        {
            public string product_id;
            public string name;
            public BillingCycle[] billing_cycles;
            public PaymentPreferences payment_preferences;

            public string GetTotalPrice()
            {
                float total = 0;
                foreach (BillingCycle cycle in billing_cycles)
                    total += float.Parse(cycle.pricing_scheme.fixed_price.value);
                return total.ToString();
            }

            [Serializable]
            public class BillingCycle
            {
                public PricingScheme pricing_scheme;
                public Frequency frequency;
                public string tenure_type;
                public int sequence;
                public int total_cycles;
            }

            [Serializable]
            public class PricingSchemeWrapper
            {
                public PricingSchemeRequest[] pricing_schemes;
            }

            [Serializable]
            public class PricingSchemeRequest
            {
                public int billing_cycle_sequence;
                public PricingScheme pricing_scheme;
            }

            [Serializable]
            public class PricingScheme
            {
                public int version;
                public Money fixed_price;
                public string create_time;
                public string update_time;
            }

            [Serializable]
            public class Frequency
            {
                public string interval_unit;
                public int interval_count;
            }

            [Serializable]
            public class PaymentPreferences
            {
                public bool auto_bill_outstanding;
                public Money setup_fee;
                public string setup_fee_failure_action;
                public int payment_failure_threshold;
            }

            [Serializable]
            public class Money
            {
                public string currency_code;
                public string value;
            }
        }

        [Serializable]
        public class Plan : BasePlan
        {
            public string id;
            public string status;
            public string description;
            public Taxes taxes;
            public bool quantity_supported;

            [Serializable]
            public class Taxes
            {
                public string percentage;
                public bool inclusive;
            }

            public static PricingSchemeRequest CreatePricingSchemeRequest(string price, string currencyCode, int sequence)
            {
                return new PricingSchemeRequest()
                {
                    billing_cycle_sequence = sequence,
                    pricing_scheme = new PricingScheme()
                    {
                        fixed_price = new Money()
                        {
                            currency_code = currencyCode,
                            value = price
                        }
                    }
                };
            }
        }

        [Serializable]
        public class BaseSubscription
        {
            public string plan_id;
            public ApplicationContext application_context;
        }

        [Serializable]
        public class Subscription : BaseSubscription
        {
            public string id;
            public string status;
            public string start_time;
            public string quantity;
            public SubscriberRequest subscriber;
            public Link[] links;

            [Serializable]
            public class SubscriberRequest
            {
                public Name name;
                public string email_address;
                public string payer_id;
                public object shipping_address;
                public object payment_source;
            }
        }

        [Serializable]
        public class AccessToken
        {
            public string scope;
            public string access_token;
            public string app_id;
            public int expires_in;
            public string nonce;
        }
        #endregion

        /// <summary>
        /// Initializes a new client instance. Loads MessageConfig field with default data.
        /// </summary>
        public PayPalClient()
        {
            MessageConfig = MessageConfig.GetDefaultConfig();
        }

        /// <summary>
        /// Set the PayPal client secret associated with your developer account.
        /// In editor, it will set the test client secret. In a build, it will set the live client secret.
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
            new PayPalClient().CreateOrder(itemName, price, quantity, onComplete);
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
            new PayPalClient().CreateSubscription(itemName, price, period, intervals, onComplete);
        }

        /// <summary>
        /// First call to initiate the NetCheckout process of making a recurring purchase. 
        /// Creates an order on PayPal's servers that the user must approve to complete the purchase.
        /// </summary>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public void CreateOrder(string itemName, string price, int amount, action onComplete)
        {
            WindowController.Instance.DisplayWindow(
                MessageConfig.orderWindow,
                () => CallApiToCreateOrder(itemName, price, amount, onComplete)
            );
        }

        private void CallApiToCreateOrder(string itemName, string price, int amount, action onComplete)
        {
            Item item = new Item(itemName, price, amount, settings.currencyCode)
            {
                ReturnUrl = settings.payPalSuccessUrl,
                CancelUrl = settings.payPalCancelUrl
            };

            Request("POST", "v2/checkout/orders", JsonUtility.ToJson(item), (status, res) =>
            {
                var response = JsonUtility.FromJson<Order>(res);

                if (!IsValid(status) || response?.status != "CREATED")
                {
                    onComplete?.Invoke(false, res);
                    return;
                }

                string approvalLink = string.Empty;

                foreach (Link link in response.links)
                    if (link.rel == "approve")
                        approvalLink = link.href;

                Utility.OpenURL(approvalLink);

                WaitForApproval(response.id, onComplete);
            });
        }

        /// <summary>
        /// Call PayPal API periodically to check for user approval of order.
        /// </summary>
        /// <param name="orderId">Received from PayPal API.</param>
        public void WaitForApproval(string orderId, action onComplete)
        {
            // display UI waiting for approval
            WindowController.Instance.DisplayWindowWithoutButton(MessageConfig.waitWindow);

            // automatically close Android browser window upon order approval
#if UNITY_ANDROID
            string url = Settings.payPalApiUrl + GetOrderEndpoint(orderId);
            string key = accessToken;
            (string, string) validator = ("status", "APPROVED");
            AndroidHelper.SetupNativeUrlCloseEvent(url, key, approvalTimeInterval, validator, settings.androidAppUri);
#endif

            Coroutiner.Instance.StartCoroutine(WaitForApprovalCoroutine(orderId, onComplete));
        }

        private IEnumerator WaitForApprovalCoroutine(string orderId, action onComplete)
        {
            bool waitingForApproval = true;
            bool gettingOrderDetails = false;

            while (waitingForApproval && WindowController.Instance.Window.IsDisplayed)
            {
                yield return new WaitForSeconds(approvalTimeInterval);

                if (gettingOrderDetails)
                    continue;

                gettingOrderDetails = true;

                // get order details
                Request("GET", GetOrderEndpoint(orderId), string.Empty, (status, res) =>
                {
                    Order order = JsonUtility.FromJson<Order>(res);

                    if (IsValid(status) && order.status == "APPROVED")
                        waitingForApproval = false;

                    gettingOrderDetails = false;
                });
            }

            if (!WindowController.Instance.Window.IsDisplayed)
                yield break;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // notify app window
            SetForegroundWindow(windowHandle);
#elif UNITY_IOS && !UNITY_EDITOR
            Utility.CloseNativeUrl();
#endif
            if (OrderIsSubscription(orderId))
            {
                WindowController.Instance.DisplayWindow(MessageConfig.subscribeCompleteWindow, () => WindowController.Instance.HideWindow());
                onComplete?.Invoke(true, orderId);
                yield break;
            }

            WindowController.Instance.DisplayWindow(
                MessageConfig.confirmWindow,
                () => ConfirmPurchase(orderId, onComplete)
            );
        }

        private string GetOrderEndpoint(string orderId)
        {
            if (OrderIsSubscription(orderId))
                return string.Format("v1/billing/subscriptions/{0}", orderId);
            return string.Format("v2/checkout/orders/{0}", orderId);
        }

        private bool OrderIsSubscription(string orderId)
        {
            return orderId.Substring(0, 2) == "I-";
        }

        /// <summary>
        /// Capture payment from the user.
        /// </summary>
        /// <param name="onComplete">Callback. If successful, returns true and the order id. If failed, returns false and the error response.</param>
        public void ConfirmPurchase(string orderId, action onComplete)
        {
            Request("POST", string.Format("v2/checkout/orders/{0}/capture", orderId), string.Empty, (status, res) =>
            {
                Order order = JsonUtility.FromJson<Order>(res);

                bool success = IsValid(status) && order.status == "COMPLETED";
                MessageConfig.Window window = success ? MessageConfig.completeWindow.success : MessageConfig.completeWindow.failure;

                WindowController.Instance.DisplayWindow(window, () => WindowController.Instance.HideWindow());
                onComplete?.Invoke(success, success ? order.id : res);
            });
        }

        /// <summary>
        /// First call to initiate the NetCheckout process of making a recurring purchase.
        /// Creates an order on PayPal's servers that the user must approve before completing the purchase.
        /// </summary>
        /// <param name="itemName">name of subscription (i.e. Gold Plan)</param>
        /// <param name="price">amount to charge (i.e. 7.99)</param>
        /// <param name="period">day, week, month, year</param>
        /// <param name="intervals">number of periods. To bill every 3 months, set period to month and intervals to 3.</param>
        /// <param name="onComplete">Callback. If successful, returns true and the subscription id. If failed, returns false and the error response.</param>
        public void CreateSubscription(string itemName, string price, PaymentPeriod period, int intervals, action onComplete)
        {
            WindowController.Instance.DisplayWindow(
                MessageConfig.subscribeWindow,
                () => CreateSubscriptionForCheckout(itemName, price, period, intervals, onComplete)
            );
        }

        private void CreateSubscriptionForCheckout(string productName, string price, PaymentPeriod period, int intervals, action onComplete)
        {
            // create Product, Plan, then Subscription
            BaseProduct product = CreateProduct(productName);
            CallApiToCreateProduct(product, (productCreated, data) =>
            {
                if (!productCreated)
                    onComplete?.Invoke(false, data);
                else
                    createPlan((Product)data);
            });

            void createPlan(Product prod)
            {
                BasePlan plan = CreatePlan(prod, price, period, intervals);
                CallApiToCreatePlan(plan, (planCreated, data) =>
                {
                    if (!planCreated)
                        onComplete?.Invoke(false, data);
                    else
                    {
                        BaseSubscription subscription = CreateSubscriptionObject((Plan)data);
                        CallApiToCreateSubscription(subscription, onComplete);
                    }
                });
            }
        }

        private BaseProduct CreateProduct(string name)
        {
            BaseProduct product = new BaseProduct()
            {
                name = name,
                type = "SERVICE"
            };

            return product;
        }

        private void CallApiToCreateProduct(BaseProduct product, action onComplete)
        {
            Request("POST", "v1/catalogs/products", JsonUtility.ToJson(product), (status, res) =>
            {
                object prod = IsValid(status) ? JsonUtility.FromJson<Product>(res) : (object)res;
                onComplete?.Invoke(IsValid(status), prod);
            });
        }

        private BasePlan CreatePlan(Product product, string price, PaymentPeriod period, int intervals)
        {
            return new BasePlan()
            {
                product_id = product.id,
                name = product.name,
                billing_cycles = new Plan.BillingCycle[1]
                {
                    new Plan.BillingCycle()
                    {
                        frequency = new Plan.Frequency()
                        {
                            interval_unit = period.ToString().ToUpper(),
                            interval_count = intervals
                        },
                        tenure_type = "REGULAR",
                        pricing_scheme = new Plan.PricingScheme()
                        {
                            fixed_price = new Plan.Money()
                            {
                                currency_code = settings.currencyCode,
                                value = price
                            },
                            create_time = Utility.GetISO8601Time(),
                            update_time = Utility.GetISO8601Time()
                        },
                        sequence = 1,
                        total_cycles = 0
                    }
                },
                payment_preferences = new Plan.PaymentPreferences()
                {
                    setup_fee = new BasePlan.Money()
                    {
                        value = "0",
                        currency_code = settings.currencyCode
                    },
                    payment_failure_threshold = 2
                }
            };
        }
        
        private void CallApiToCreatePlan(BasePlan basePlan, action onComplete)
        {
            Request("POST", "v1/billing/plans", JsonUtility.ToJson(basePlan), (status, res) =>
            {
                object plan = IsValid(status) ? JsonUtility.FromJson<Plan>(res) : (object)res;
                onComplete?.Invoke(IsValid(status), plan);
            });
        }

        private BaseSubscription CreateSubscriptionObject(Plan plan)
        {
            return new BaseSubscription()
            {
                plan_id = plan.id,
                application_context = new ApplicationContext()
                {
                    return_url = settings.payPalSuccessUrl,
                    cancel_url = settings.payPalCancelUrl
                }
            };
        }

        private void CallApiToCreateSubscription(BaseSubscription subscription, action onComplete)
        {
            string postData = JsonUtility.ToJson(subscription);
            
            Request("POST", "v1/billing/subscriptions", postData, (status, res) =>
            {
                var response = JsonUtility.FromJson<Subscription>(res);

                if (!IsValid(status) || response.status != "APPROVAL_PENDING")
                {
                    onComplete?.Invoke(false, res);
                    return;
                }

                string approvalLink = string.Empty;

                foreach (Link link in response.links)
                    if (link.rel == "approve")
                        approvalLink = link.href;

                Utility.OpenURL(approvalLink);

                WaitForApproval(response.id, onComplete);
            });
        }

        /// <summary>
        /// Make a raw API request to PayPal.
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="endPoint">i.e. v2/checkout/orders</param>
        /// <param name="jsonData">Data to post/patch.</param>
        /// <param name="callback">Returns server response in JSON format (status code, response string).</param>
        public void Request(string method, string endPoint, string jsonData, UnityAction<long, string> callback)
        {
            GetAccessToken(MakeApiCall);

            void MakeApiCall()
            {
                UnityWebRequest request = new UnityWebRequest(Settings.payPalApiUrl + endPoint, method);
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(jsonData))
                    request.uploadHandler = new UploadHandlerRaw(Utility.GetBytes(jsonData)) { contentType = "application/json" };

                request.downloadHandler = new DownloadHandlerBuffer();

                request.SendWebRequest().completed += _ =>
                {
                    callback(request.responseCode, request.downloadHandler.text);
                };
            };
        }

        private void GetAccessToken(UnityAction callback)
        {
            if (HasAccessToken)
            {
                callback();
                return;
            }

            UnityWebRequest request = new UnityWebRequest(Settings.payPalApiUrl + "v1/oauth2/token", "POST");

            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Accept-Language", "en_US");
            request.SetRequestHeader("Authorization", "Basic " + Utility.GetBase64(settings.PayPalClientId + ":" + settings.PayPalClientSecret));

            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(Utility.GetBytes("grant_type=client_credentials"));

            request.SendWebRequest().completed += _ =>
            {
                var response = JsonUtility.FromJson<AccessToken>(request.downloadHandler.text);

                accessToken = response?.access_token;
                callback();
            };
        }

        /// <summary>
        /// Gets data in the <see cref="Order" class/>.
        /// </summary>
        /// <param name="orderId">The id returned after making a successful purchase.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="Order"/> instance. 
        /// On failure, returns false and error response.</param>
        public void GetOrderDetails(string orderId, action onComplete)
        {
            Request("GET", string.Format("v2/checkout/orders/{0}", orderId), string.Empty, (status, res) =>
            {
                object order = IsValid(status) ? JsonUtility.FromJson<Order>(res) : (object)res;
                onComplete?.Invoke(IsValid(status), order);
            });
        }

        /// <summary>
        /// Unpauses the current subscription, so users will start to be charged again.
        /// Note: The plan ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="planId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="onComplete">Successful callback returns true value and an empty object. On failure, returns false and error response.</param>
        public void ActivateSubscription(string planId, action onComplete)
        {
            Request("POST", string.Format("v1/billing/plans/{0}/activate", planId), string.Empty, (status, res) =>
            {
                onComplete?.Invoke(IsValid(status), res);
            });
        }

        /// <summary>
        /// Pauses the current subscription, so users will not be charged anymore.
        /// Note: The plan ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="planId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="onComplete">Successful callback returns true value and an empty object. On failure, returns false and error response.</param>
        public void DeactivateSubscription(string planId, action onComplete)
        {
            Request("POST", string.Format("v1/billing/plans/{0}/deactivate", planId), string.Empty, (status, res) =>
            {
                onComplete?.Invoke(IsValid(status), res);
            });
        }

        /// <summary>
        /// Note: The plan ID is NOT returned after a successful purchase. 
        /// You will need to get that first by calling <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="planId">received after call to <see cref="GetSubscriptionDetails(string, action)"/></param>
        /// <param name="price">the new price (i.e. 12.99)</param>
        /// <param name="onComplete">Successful callback returns true value and an empty object. On failure, returns false and error response.</param>
        public void UpdateSubscriptionPricing(string planId, string price, action onComplete)
        {
            BasePlan.PricingSchemeWrapper pricing = new BasePlan.PricingSchemeWrapper
            {
                pricing_schemes = new BasePlan.PricingSchemeRequest[1] { Plan.CreatePricingSchemeRequest(price, settings.currencyCode, 1) }
            };

            Request("POST", string.Format("v1/billing/plans/{0}/update-pricing-schemes", planId), JsonUtility.ToJson(pricing), (status, res) =>
            {
                onComplete?.Invoke(IsValid(status), res);
            });
        }

        /// <summary>
        /// Gets data in the <see cref="Plan"/> class.
        /// </summary>
        /// <param name="subscriptionId">The id returned after creating a successful subscription.</param>
        /// <param name="onComplete">Successful callback returns true value and a <see cref="Plan"/> instance. 
        /// On failure, returns false and error response.</param>
        public void GetSubscriptionDetails(string subscriptionId, action onComplete)
        {
            Request("GET", string.Format("v1/billing/subscriptions/{0}", subscriptionId), string.Empty, (status, res) =>
            {
                if (!IsValid(status))
                { 
                    onComplete?.Invoke(false, res);
                    return;
                }
                
                string planId = JsonUtility.FromJson<Subscription>(res).plan_id;

                Request("GET", string.Format("v1/billing/plans/{0}", planId), string.Empty, (code, response) =>
                {
                    object plan = IsValid(code) ? JsonUtility.FromJson<Plan>(response) : (object)response;
                    onComplete?.Invoke(IsValid(code), plan);
                });
            });
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetOrderDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Order"/> data</param>
        /// <returns>price (i.e. 3.99)</returns>
        public static double GetPriceFromOrderDetails(object data)
        {
            return double.Parse(((Order)data).purchase_units[0].amount.value);
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Plan"/> data</param>
        /// <returns>price (i.e. 3.99)</returns>
        public static double GetPriceFromSubscriptionDetails(object data)
        {
            return double.Parse(((Plan)data).billing_cycles[0].pricing_scheme.fixed_price.value);
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetOrderDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Order"/> data</param>
        /// <returns>currency code (i.e. USD)</returns>
        public static string GetCurrencyFromOrderDetails(object data)
        {
            return ((Order)data).purchase_units[0].amount.currency_code;
        }

        /// <summary>
        /// Convenience function to be used after <see cref="GetSubscriptionDetails(string, action)"/>
        /// </summary>
        /// <param name="data"><see cref="Plan"/> data</param>
        /// <returns>currency code (i.e. USD)</returns>
        public static string GetCurrencyFromSubscriptionDetails(object data)
        {
            return ((Plan)data).billing_cycles[0].pricing_scheme.fixed_price.currency_code;
        }
    }
}