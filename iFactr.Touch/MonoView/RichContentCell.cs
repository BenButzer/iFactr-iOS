using System;
using System.ComponentModel;
using CoreGraphics;using Foundation;
using iFactr.Core;
using iFactr.UI;
using UIKit;

namespace iFactr.Touch
{
    public class RichContentCell : UITableViewCell, IRichContentCell, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new Color BackgroundColor
        {
            get { return base.ContentView.BackgroundColor.ToColor(); }
            set
            {
                if (value != base.ContentView.BackgroundColor.ToColor())
                {
                    base.ContentView.BackgroundColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }

        public Color ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value != foregroundColor)
                {
                    foregroundColor = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }
        private Color foregroundColor;

        public double MaxHeight
        {
            get { return maxHeight; }
            set
            {
                if (value != maxHeight)
                {
                    maxHeight = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MaxHeight"));
                    }
                }
            }
        }
        private double maxHeight;

        public double MinHeight
        {
            get { return minHeight; }
            set
            {
                if (value != minHeight)
                {
                    minHeight = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("MinHeight"));
                    }
                }
            }
        }
        private double minHeight;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Text"));
                    }
                }
            }
        }
        private string text;

		public IPairable Pair
		{
			get { return pair; }
			set
			{
				if (pair == null)
				{
					pair = value;
					pair.Pair = this;
				}
			}
		}
		private IPairable pair;

        System.Collections.Generic.List<iFactr.Core.Controls.PanelItem> iFactr.Core.Layers.IHtmlText.Items
        {
            get { throw new NotSupportedException(); }
            set { }
        }

        private UIWebView webView;
        
        public RichContentCell() : base(UITableViewCellStyle.Default, ListView.CellId.ToString())
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            SelectionStyle = UITableViewCellSelectionStyle.None;

            webView = new UIWebView(ContentView.Frame)
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                BackgroundColor = null,
                DataDetectorTypes = UIDataDetectorType.Link,
                Opaque = false
            };
            
            webView.ShouldStartLoad += (sender, request, type) =>
            {
                if (type == UIWebViewNavigationType.LinkClicked && request?.Url != null)
                {
                    iApp.Navigate(request.Url.ToString());
                    return false;
                }
                
                return true;
            };

            webView.LoadFinished += (sender, e) =>
            {
                webView.Frame = ContentView.Frame;

                if (webView.ScrollView != null)
                {
                    webView.ScrollView.ScrollEnabled = false;
                }

                webView.InvokeOnMainThread(() =>
                {
                    webView.Frame = new CGRect(webView.Frame.X, webView.Frame.Y, webView.Frame.Width, webView.GetDocumentHeight());

                    {
                        var tableView = this.GetSuperview<TableView>();
                        if (tableView != null)
                        {
                            NSIndexPath path = tableView.IndexPathForCell(this);
                            if (path != null)
                            {
                                var frame = Frame;
                                frame.Height = (float)Math.Min(Math.Max(Math.Min(webView.Frame.Height, MaxHeight), MinHeight), float.MaxValue);
                                Frame = frame;

                                tableView.ResizeRow(path);
                            }
                            return;
                        }
                    }

                    {
                        var tableView = this.GetSuperview<UITableView>();
                        if (tableView != null)
                        {
                            NSIndexPath path = tableView.IndexPathForCell(this);
                            if (path != null)
                            {
                                var frame = tableView.RectForRowAtIndexPath(path);
                                nfloat height = frame.Height;
                                frame.Height = (nfloat)Math.Min(Math.Max(Math.Min((float)webView.Frame.Height, MaxHeight), MinHeight), float.MaxValue);
                                if (height == frame.Height)
                                {
                                    return;
                                }

                                Frame = frame;

                                tableView.BeginUpdates();
                                tableView.ReloadRows(new[] { path }, UITableViewRowAnimation.None);
                                tableView.EndUpdates();
                            }
                        }
                    }

                });
            };

            ContentView.Add(webView);
        }

        public void Load()
        {
            NSUrl newUrl = new NSUrl(Environment.CurrentDirectory, true);
            webView.LoadHtmlString(Text.StartsWith("<html>") && Text.EndsWith("</html>") ? Text :
                string.Format("<html><body style=\"-webkit-text-size-adjust:none;font-family:{2};color:#{0};margin:15px\">{1}</body></html>", foregroundColor.HexCode.Substring(3), Text, UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? "helvetica neue" : "helvetica"), newUrl);
        }

		public bool Equals (ICell other)
		{
			var cell = other as Cell;
			if (cell != null)
			{
				return cell.Equals(this);
			}
			
			return base.Equals(other);
		}
    }
}

