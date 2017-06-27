using System;
using System.ComponentModel;
using System.Linq;

using UIKit;

using iFactr.UI;

namespace iFactr.Touch
{
	public class SearchBox : UISearchBar, ISearchBox, INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

		public event SearchEventHandler SearchPerformed;
        
        public new Color BackgroundColor
        {
            get { return textField == null ? new Color() : textField.BackgroundColor.ToColor(); }
            set
            {
                if (value != BackgroundColor)
                {
                    GetTextField();
                    if (textField != null)
                    {
                        textField.BackgroundColor = value.IsDefaultColor ? null : value.ToUIColor();

                        var handler = PropertyChanged;
                        if (handler != null)
                        {
                            handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                        }
                    }
                }
            }
        }

		public Color BorderColor
		{
            get { return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? BarTintColor.ToColor() : TintColor.ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? defaultTintColor.ToColor() : value;
                if (color != BorderColor)
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        BarTintColor = color.ToUIColor();
                    }
                    else
                    {
                        TintColor = color.ToUIColor();
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BorderColor"));
                    }
                }
            }
		}
        
        public Color ForegroundColor
        {
            get { return textField == null ? UIColor.Black.ToColor() : textField.TextColor.ToColor(); }
            set
            {
                var color = value.IsDefaultColor ? UIColor.Black : value.ToUIColor();
                if (value != ForegroundColor)
                {
                    GetTextField();
                    if (textField != null)
                    {
                        textField.TextColor = color;

                        var handler = PropertyChanged;
                        if (handler != null)
                        {
                            handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                        }
                    }
                }
            }
        }
        
        public TextCompletion TextCompletion
        {
            get
            {
                return (TextCompletion)((AutocorrectionType == UITextAutocorrectionType.No ? 0 : 1) +
                    (AutocapitalizationType == UITextAutocapitalizationType.None ? 0 : 2));
            }
            set
            {
                if (value != TextCompletion)
                {
                    AutocorrectionType = ((value & TextCompletion.OfferSuggestions) == 0) ?
                        UITextAutocorrectionType.No : UITextAutocorrectionType.Default;
                    
                    AutocapitalizationType = ((value & TextCompletion.AutoCapitalize) == 0) ?
                        UITextAutocapitalizationType.None : UITextAutocapitalizationType.Sentences;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TextCompletion"));
                    }
                }
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                value = value ?? string.Empty;
                if (base.Text != value)
                {
                    base.Text = value;

                    var phandler = PropertyChanged;
                    if (phandler != null)
                    {
                        phandler(this, new PropertyChangedEventArgs("Text"));
                    }

                    if (Superview == null)
                    {
                        // TextChanged doesn't seem to fire if the super view is null
                        var handler = SearchPerformed;
                        if (handler != null)
                        {
                            handler(Pair ?? this, new SearchEventArgs(value));
                        }
                    }
                }
            }
        }

        public override string Placeholder
        {
            get { return base.Placeholder; }
            set
            {
                if (value != base.Placeholder)
                {
                    base.Placeholder = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Placeholder"));
                    }
                }
            }
        }

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
        
        private UITextField textField;
        private UIColor defaultTintColor;

		public SearchBox ()
		{
            defaultTintColor = UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? BarTintColor : TintColor;
            
			AutocorrectionType = UITextAutocorrectionType.No;
            AutocapitalizationType = UITextAutocapitalizationType.None;
			
			TextChanged += (sender, e) =>
			{
				var handler = SearchPerformed;
				if (handler != null)
				{
					handler(Pair ?? this, new SearchEventArgs(e.SearchText));
				}
			};

            SearchButtonClicked += (sender, e) =>
            {
                ResignFirstResponder();
            };

			SizeToFit();
		}

		public void Focus()
		{
			base.BecomeFirstResponder();
		}

        public override void WillMoveToWindow(UIWindow window)
        {
            base.WillMoveToWindow(window);
            if (window != null)
            {
                GetTextField();
            }
        }
        
        private void GetTextField()
        {
            if (textField == null)
            {
                textField = this.GetSubview<UITextField>();
                if (textField != null)
                {
                    textField.EnablesReturnKeyAutomatically = false;
                    textField.ReturnKeyType = UIReturnKeyType.Done;
                }
            }
        }
	}
}

