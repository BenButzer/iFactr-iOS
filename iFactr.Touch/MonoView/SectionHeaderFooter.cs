using System;
using System.ComponentModel;

using iFactr.UI;

using CoreGraphics;
using UIKit;

namespace iFactr.Touch
{
    public class SectionHeaderFooter : UITableViewHeaderFooterView, ISectionHeader, ISectionFooter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value != backgroundColor)
                {
                    backgroundColor = value;
                    if (BackgroundView == null)
                    {
                        BackgroundView = new UIView();
                    }

                    if (value.IsDefaultColor)
                    {
                        var table = Superview as UITableView;
                        if (table != null)
                        {
                            BackgroundView.BackgroundColor = (table.Style == UITableViewStyle.Grouped ?
                                null : new UIColor(0.97f, 0.97f, 0.97f, 1));
                        }
                        else
                        {
                            BackgroundView.BackgroundColor = null;
                        }
                    }
                    else
                    {
                        BackgroundView.BackgroundColor = value.ToUIColor();
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackgroundColor"));
                    }
                }
            }
        }
        private Color backgroundColor;
        
        public Color ForegroundColor
        {
            get { return foregroundColor; }
            set
            {
                if (value != foregroundColor)
                {
                    foregroundColor = value;
                    if (value.IsDefaultColor)
                    {
                        var table = Superview as UITableView;
                        if (table != null)
                        {
                            TextLabel.TextColor = (table.Style == UITableViewStyle.Grouped ?
                                new UIColor(0.43f, 0.43f, 0.45f, 1) : new UIColor(0.14f, 0.14f, 0.14f, 1));
                        }
                        else
                        {
                            TextLabel.TextColor = null;
                        }
                    }
                    else
                    {
                        TextLabel.TextColor = value.ToUIColor();
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }
        private Color foregroundColor;

        public Font Font
        {
            get { return font.Name == null ? TextLabel.Font.ToFont() : font; }
            set
            {
                if (value != font)
                {
                    font = value;
                    TextLabel.Font = value.ToUIFont();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Font"));
                    }
                }
            }
        }
        private Font font;
        
        public string Text
        {
            get { return TextLabel.Text; }
            set
            {
                if (value != TextLabel.Text)
                {
                    TextLabel.Text = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Text"));
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
        
        public SectionHeaderFooter()
        {
        }

        public override void LayoutSubviews()
        {
            TextLabel.Font = Font.ToUIFont();
            base.LayoutSubviews();
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            base.WillMoveToSuperview(newsuper);

            if (newsuper != null)
            {
                var table = newsuper as UITableView;
                if (table != null)
                {
                    if (table.Style == UITableViewStyle.Grouped)
                    {
                        TextLabel.TextColor = foregroundColor.IsDefaultColor ? new UIColor(0.43f, 0.43f, 0.45f, 1) : foregroundColor.ToUIColor();
                    }
                    else
                    {
                        TextLabel.TextColor = foregroundColor.IsDefaultColor ? new UIColor(0.14f, 0.14f, 0.14f, 1) : foregroundColor.ToUIColor();
                        if (BackgroundView != null)
                        {
                            BackgroundView.BackgroundColor = backgroundColor.IsDefaultColor ? new UIColor(0.97f, 0.97f, 0.97f, 1) : backgroundColor.ToUIColor();
                        }
                    }
                }
            }
        }
    }

    public class SectionHeaderFooterLegacy : UITableViewHeaderFooterView, ISectionHeader, ISectionFooter, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new Color BackgroundColor
        {
            get { return BackgroundView == null ? new Color() : BackgroundView.BackgroundColor.ToColor(); }
            set
            {
                if (value != BackgroundColor)
                {
                    if (BackgroundView == null)
                    {
                        BackgroundView = new UIView();
                    }
                    BackgroundView.BackgroundColor = value.IsDefaultColor ? null : value.ToUIColor();

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
            get { return textLabel.TextColor.ToColor(); }
            set
            {
                if (value != textLabel.TextColor.ToColor())
                {
                    textLabel.TextColor = value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ForegroundColor"));
                    }
                }
            }
        }

        public Font Font
        {
            get { return textLabel.Font.ToFont(); }
            set
            {
                if (value != textLabel.Font.ToFont())
                {
                    textLabel.Font = value.ToUIFont();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Font"));
                    }
                }
            }
        }
        
        public string Text
        {
            get { return textLabel.Text; }
            set
            {
                if (value != textLabel.Text)
                {
                    textLabel.Text = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Text"));
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

        public override UILabel TextLabel
        {
            get { return textLabel; }
        }
        private UILabel textLabel;
        
        public SectionHeaderFooterLegacy()
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            ContentView.Add((textLabel = new UILabel()
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
                LineBreakMode = UILineBreakMode.TailTruncation,
                TextColor = new UIColor(0, 0, 0, 0)
            }));
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (textLabel != null)
            {
                if (textLabel.Text != null)
                {
                    if (textLabel.Frame.Height == 0)
                    {
                        textLabel.SizeToFit();
                    }

                    var frame = textLabel.Frame;
                    frame.Width = ContentView.Frame.Width - (frame.X * 2);
                    textLabel.Frame = frame;
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    textLabel.Center = new CGPoint(textLabel.Center.X, Frame.Height / 2);
                }
            }
        }
    }
}

