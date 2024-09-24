using System;
using iFactr.Core.Controls;
using Link = iFactr.UI.Link;

namespace iFactr.Core.Layers
{
    /// <summary>
    /// Represents a navigation element within an <see cref="iList"/> or <see cref="iMenu"/>.
    /// </summary>
    public class iItem
    {
        /// <summary>
        /// Gets or sets the primary display text for this instance.
        /// </summary>
        /// <value>The text as a <see cref="String"/> value.</value>
        public string Text { get; set; }
        /// <summary>
        /// Gets or sets the secondary display text for this instance.
        /// </summary>
        /// <value>The subtext as a <see cref="String"/> value.</value>
        public string Subtext { get; set; }

        // S-102769 - Combined Patient Search results page - June 19, 2023
        /// <summary>
        /// Gets or sets the sub secondary display text for this instance.
        /// </summary>
        /// <value>The subtext as a <see cref="String"/> value.</value>
        public string SubSourcetext { get; set; }

        /// <summary>
        /// Gets or sets an icon to display next to the text for this instance.
        /// </summary>
        /// <value>The icon as an <see cref="Icon"/>.</value>
        public Icon Icon { get; set; }
        /// <summary>
        /// Gets or sets the link to navigate to when this instance is selected.
        /// </summary>
        /// <value>The link as a <see cref="Link"/>.</value>
        public Link Link { get; set; }
        /// <summary>
        /// Gets or sets a secondary link, generally displayed as a button, that this instance can alternatively navigate to.
        /// </summary>
        /// <value>The button as a <see cref="Button"/>.</value>
        public Button Button { get; set; }

        /// <summary>
        /// Gets or sets the TextColor for this instance.
        /// </summary>
        /// <value>The text as a <see cref="String"/> value.</value>
        public string TextColor { get; set; }

        /// <summary>
        /// Gets or sets the SubTextColor for this instance.
        /// </summary>
        /// <value>The text as a <see cref="String"/> value.</value>
        public string SubTextColor { get; set; }

        /// <summary>
        /// Gets or sets the FontFormat for this instance.
        /// </summary>
        /// <value>The text as a <see cref="String"/> value.</value>
        public string FontFormat { get; set; }

        /// <summary>
        /// Creates a deep-copy clone of this instance.
        /// </summary>
        public iItem Clone()
        {
            iItem i = (iItem)MemberwiseClone();
            if (Icon != null)
                i.Icon = Icon.Clone();
            if (Link != null)
                i.Link = Link.Clone();
            if (Button != null)
                i.Button = Button.Clone();

            return i;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class.
        /// </summary>
        public iItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link and text provided.
        /// </summary>
        /// <param name="link">A <see cref="Link"/> representing the link to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        public iItem(Link link, string text)
        {
            Text = text;
            Link = link;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link, text, and async value provided.
        /// </summary>
        /// <param name="link">A <see cref="Link"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        /// <param name="async">If <c>true</c>, sets the link rev value to Async.</param>
        public iItem(Link link, string text, bool async)
        {
            Text = text;
            Link = link;
            Link.RequestType = async ? UI.RequestType.Async : UI.RequestType.ClearPaneHistory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link address and text provided.
        /// </summary>
        /// <param name="linkAddress">A <see cref="String"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        public iItem(string linkAddress, string text)
        {
            Text = text;
            Link = new Link { Address = linkAddress };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link address, text, and async value provided.
        /// </summary>
        /// <param name="linkAddress">A <see cref="String"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        /// <param name="async">If <c>true</c>, sets the link rev value to Async.</param>
        public iItem(string linkAddress, string text, bool async)
        {
            Text = text;
            Link = new Link { Address = linkAddress, RequestType = async ? UI.RequestType.Async : UI.RequestType.ClearPaneHistory };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link address, text, and subtext provided.
        /// </summary>
        /// <param name="linkAddress">A <see cref="String"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        /// <param name="subText">A <see cref="String"/> representing the subtext value.</param>
        public iItem(string linkAddress, string text, string subText)
        {
            Text = text;
            Subtext = subText;
            Link = new Link { Address = linkAddress };
        }

        // S-102769 - Combined Patient Search results page - June 19, 2023
        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link address, text, and subtext provided.
        /// </summary>
        /// <param name="linkAddress">A <see cref="String"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        /// <param name="subText">A <see cref="String"/> representing the subtext value.</param>s
        /// <param name="subSourceText">A <see cref="String"/> representing the subtext value.</param>
        public iItem(string linkAddress, string text, string subText, string subSourceText)
        {
            Text = text;
            Subtext = subText;
            SubSourcetext = subSourceText;
            Link = new Link { Address = linkAddress };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="iItem"/> class using the link address, text, sub text, and async value provided.
        /// </summary>
        /// <param name="linkAddress">A <see cref="String"/> representing the link address to navigate to when selected.</param>
        /// <param name="text">A <see cref="String"/> representing the text value.</param>
        /// <param name="subText">A <see cref="String"/> representing the subtext value.</param>
        /// <param name="async">If <c>true</c>, sets the link rev value to Async.</param>
        public iItem(string linkAddress, string text, string subText, bool async)
        {
            Text = text;
            Subtext = subText;
            Link = new Link { Address = linkAddress, RequestType = async ? UI.RequestType.Async : UI.RequestType.ClearPaneHistory };
        }
    }
}