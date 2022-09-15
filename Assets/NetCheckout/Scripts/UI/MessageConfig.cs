using System.IO;
using System.Xml;
using System.Xml.Serialization;

using UnityEngine;
using UnityEngine.Events;

namespace NetCheckout
{
	/// <summary>
	/// Container for the messages displayed throughout the NetCheckout process.
	/// Default messages are loaded from MessageConfig.xml when an instance is created.
	/// The XML file is located in NetCheckout/Resources/Config.
	/// </summary>
	[XmlRoot("MessageConfig")]
	public class MessageConfig
	{
		/// <summary>
		/// First window in the checkout process. When the button is clicked, it will load the payment URL in the browser.
		/// </summary>
		[XmlElement("OrderWindow")]
		public Window orderWindow;

		/// <summary>
		/// Window is displayed while waiting for the user to authorize payment from the payment service.
		/// </summary>
		[XmlElement("WaitWindow")]
		public Window waitWindow;

		/// <summary>
		/// Confirm payment of the product.
		/// <b>Note:</b> Stripe automatically confirms payment, so it does not use this window.
		/// </summary>
		[XmlElement("ConfirmWindow")]
		public Window confirmWindow;

		/// <summary>
		/// If the payment was successful, window will display the success messages. Otherwise, it displays the failure messages.
		/// <b>Note:</b> Stripe only displays the success message.
		/// </summary>
		[XmlElement("CompleteWindow")]
		public StateWindow completeWindow;

		/// <summary>
		/// First window in the subscription checkout process. When the button is clicked, it will load the payment URL in the browser.
		/// </summary>
		[XmlElement("SubscribeWindow")]
		public Window subscribeWindow;

		/// <summary>
		/// Displays the success message for a new subscription.
		/// </summary>
		[XmlElement("SubscribeCompleteWindow")]
		public Window subscribeCompleteWindow;

		/// <summary>
		/// Message data for a window.
		/// </summary>
		public class Window
		{
			[XmlElement("Header")]
			public string header;

			[XmlElement("Body")]
			public string body;

			[XmlElement("ButtonTitle")]
			public string buttonTitle;

			/// <summary>
			/// Tells the WindowController which window prefab to instantiate.
			/// </summary>
			[XmlElement("PrefabIndex")]
			public int prefabIndex;

			/// <summary>
			/// The sprite to use if the window has an image.
			/// </summary>
			[XmlIgnore]
			public Sprite sprite;

			/// <summary>
			/// Triggers just before the window is displayed.
			/// </summary>
			[XmlIgnore]
			public UnityAction<NetCheckout.Window> onDisplay;
		}

		/// <summary>
		/// Window for either success or failure message states.
		/// </summary>
		public class StateWindow
		{
			[XmlElement("SuccessState")]
			public Window success;

			[XmlElement("FailureState")]
			public Window failure;
		}

		/// <summary>
		/// Loaded from MessageConfig.xml in the Resources/Config folder.
		/// </summary>
		/// <returns></returns>
		public static MessageConfig GetDefaultConfig()
		{
			TextAsset defaultData = Resources.Load("Config/XML/MessageConfig") as TextAsset;
			MemoryStream assetStream = new MemoryStream(defaultData.bytes);

			XmlTextReader xmlReader = new XmlTextReader(assetStream);
			XmlSerializer serializer = new XmlSerializer(typeof(MessageConfig));

			return (MessageConfig)serializer.Deserialize(xmlReader);
		}
	}
}