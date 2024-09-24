using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using CoreGraphics;
using Foundation;
using UIKit;

using iFactr.Core;
using iFactr.UI;
using iFactr.UI.Controls;

using Size = iFactr.UI.Size;
using System.Runtime.InteropServices;

namespace iFactr.Touch
{
    #region iOS8 and above
    public class ListView : UITableViewController, IListView, INotifyPropertyChanged
    {
        public static int CellId { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Activated;

        public event EventHandler Deactivated;

        public event EventHandler Rendering;

        public event SubmissionEventHandler Submitting;

        public ColumnMode ColumnMode
        {
            get { return columnMode; }
            set
            {
                if (value != columnMode)
                {
                    columnMode = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ColumnMode"));
                    }
                }
            }
        }
        private ColumnMode columnMode;

        public ListViewStyle Style { get; private set; }

        public IMenu Menu
        {
            get { return menu == null ? null : (menu.Pair as IMenu) ?? menu; }
            set
            {
                if (value != menu)
                {
                    menu = value;
                    this.SetMenu(menu);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Menu"));
                    }
                }
            }
        }
        private IMenu menu;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

        public PreferredOrientation PreferredOrientations
        {
            get { return preferredOrientations; }
            set
            {
                if (value != preferredOrientations)
                {
                    preferredOrientations = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PreferredOrientations"));
                    }
                }
            }
        }
        private PreferredOrientation preferredOrientations;

        public string StackID
        {
            get { return stackID; }
            set
            {
                if (value != stackID)
                {
                    stackID = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("StackID"));
                    }
                }
            }
        }
        private string stackID;

        public Link BackLink
        {
            get { return backLink; }
            set
            {
                if (value != backLink)
                {
                    backLink = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackLink"));
                    }
                }
            }
        }
        private Link backLink;

        public CellDelegate CellRequested
        {
            get { return cellRequested; }
            set
            {
                if (value != cellRequested)
                {
                    cellRequested = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("CellRequested"));
                    }
                }
            }
        }
        private CellDelegate cellRequested;

        public ItemIdDelegate ItemIdRequested
        {
            get { return itemIdRequested; }
            set
            {
                if (value != itemIdRequested)
                {
                    itemIdRequested = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ItemIdRequested"));
                    }
                }
            }
        }
        private ItemIdDelegate itemIdRequested;

        public ShouldNavigateDelegate ShouldNavigate
        {
            get { return shouldNavigate; }
            set
            {
                if (value != shouldNavigate)
                {
                    shouldNavigate = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ShouldNavigate"));
                    }
                }
            }
        }
        private ShouldNavigateDelegate shouldNavigate;

        public Pane OutputPane
        {
            get { return outputPane; }
            set
            {
                if (value != outputPane)
                {
                    outputPane = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("OutputPane"));
                    }
                }
            }
        }
        private Pane outputPane;

        public PopoverPresentationStyle PopoverPresentationStyle
        {
            get { return ModalPresentationStyle == UIModalPresentationStyle.FullScreen ? PopoverPresentationStyle.FullScreen : PopoverPresentationStyle.Normal; }
            set
            {
                if (value != PopoverPresentationStyle)
                {
                    ModalPresentationStyle = value == PopoverPresentationStyle.FullScreen ? UIModalPresentationStyle.FullScreen : UIModalPresentationStyle.FormSheet;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PopoverPresentationStyle"));
                    }
                }
            }
        }

        public IHistoryStack Stack
        {
            get
            {
                var stack = NavigationController as IHistoryStack;
                if (stack != null && PaneManager.Instance.Contains(stack))
                {
                    return stack;
                }

                return null;
            }
        }

        public Color HeaderColor
        {
            get { return headerColor.ToColor(); }
            set
            {
                if (value != headerColor.ToColor())
                {
                    headerColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        NavigationController.NavigationBar.BarTintColor = headerColor;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HeaderColor"));
                    }
                }
            }
        }
        private UIColor headerColor;

        public Color TitleColor
        {
            get { return titleColor.ToColor(); }
            set
            {
                if (value != titleColor.ToColor())
                {
                    titleColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        NavigationController.NavigationBar.TintColor = titleColor;
                        NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleColor"));
                    }
                }
            }
        }
        private UIColor titleColor;

        public new string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    
                    if (NavigationItem != null)
                    {
                        NavigationItem.Title = title ?? string.Empty;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }
        private string title;

        public Color SeparatorColor
        {
            get { return TableView.SeparatorColor.ToColor(); }
            set
            {
                if (value != TableView.SeparatorColor.ToColor())
                {
                    TableView.SeparatorColor = value.IsDefaultColor ? defaultSeparatorColor : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SeparatorColor"));
                    }
                }
            }
        }

        public double Height { get; private set; }

        public double Width { get; private set; }

        public SectionCollection Sections { get; private set; }

        public ValidationErrorCollection ValidationErrors { get; private set; }

        public ISearchBox SearchBox
        {
            get
            {
                var box = searchBar as ISearchBox;
                return box == null ? null : (box.Pair as ISearchBox) ?? box;
            }
            set
            {
                if (value != SearchBox)
                {
                    if (searchBar != null)
                    {
                        searchBar.RemoveFromSuperview();
                    }

                    searchBar = TouchFactory.GetNativeObject<UIView>(value, "searchBox");
                    if (searchBar != null)
                    {
                        TableView.TableHeaderView = searchBar;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SearchBox"));
                    }
                }
            }
        }
        private UIView searchBar;

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

        public Type ModelType
        {
            get { return model == null ? null : model.GetType(); }
        }

        private bool hasAppeared, keyboardVisible;
        private object model;
        private UIColor defaultSeparatorColor;
        private Dictionary<string, string> submitValues;
        private Dictionary<NSIndexPath, UITableViewCell> cells;
        private Dictionary<NSIndexPath, nfloat> cellHeights; // because scrolling to a row is way too complicated for iOS to do correctly
        private Dictionary<NSIndexPath, IRichContentCell> richContentCells;

        public ListView()
            : this(ListViewStyle.Default)
        {
        }

        public ListView(ListViewStyle style)
            : base((UITableViewStyle)style)
        {
            Style = style;

            Sections = new SectionCollection();
            ValidationErrors = new ValidationErrorCollection();
            
            submitValues = new Dictionary<string, string>();
            cells = new Dictionary<NSIndexPath, UITableViewCell>();
            cellHeights = new Dictionary<NSIndexPath, nfloat>(IndexPathComparer.Instance);
            richContentCells = new Dictionary<NSIndexPath, IRichContentCell>();
        }

        public IDictionary<string, string> GetSubmissionValues()
        {
            foreach (var cell in TableView.VisibleCells.OfType<IGridCell>())
            {
                SetSubmitValue(cell);
            }

            return new Dictionary<string, string>(submitValues);
        }

        public IEnumerable<ICell> GetVisibleCells()
        {
            foreach (var cell in TableView.VisibleCells.OfType<ICell>())
            {
                yield return (cell.Pair as ICell) ?? cell;
            }
        }

        public void ReloadSections()
        {
            richContentCells.Clear();
            cellHeights.Clear();
            TableView.ReloadData();
        }

        public void ScrollToCell(int section, int index, bool animated)
        {
            if (TableView.Superview == null)
            {
                Metadata["iosInitialScrollPosition"] = new object[] { section, index, animated };
                return;
            }

            if (!cellHeights.ContainsKey(NSIndexPath.FromRowSection(index, section)))
            {
                var nextField = Metadata["NextField"];

                NSIndexPath path = NSIndexPath.FromRowSection(0, 0);
                foreach (var key in cellHeights.Keys)
                {
                    if (key.Section > path.Section || (key.Section == path.Section && key.Row > path.Row))
                    {
                        path = key;
                    }
                }

                for (int i = path.Section; i <= section; i++)
                {
                    var sect = Sections[i];
                    for (int j = (i == path.Section ? path.Row : 0); j < (i == section ? index + 1 : sect.ItemCount); j++)
                    {
                        TableView.Source.GetCell(TableView, NSIndexPath.FromRowSection(j, i));
                    } 
                }

                if (nextField != null)
                {
                    Metadata["NextField"] = nextField;
                }

                TableView.ReloadData();
            }

            var rect = TableView.RectForRowAtIndexPath(NSIndexPath.FromRowSection(index, section));
            if (rect.Y < TableView.ContentOffset.Y + TableView.ContentInset.Top)
            {
                TableView.ScrollToRow(NSIndexPath.FromRowSection(index, section), UITableViewScrollPosition.Top, animated);
            }
            else if (rect.Bottom > TableView.ContentOffset.Y + TableView.ContentInset.Bottom + TableView.Frame.Height)
            {
                TableView.ScrollToRow(NSIndexPath.FromRowSection(index, section), UITableViewScrollPosition.Bottom, animated);
            }
        }

        public void ScrollToEnd(bool animated)
        {
            if (Sections.Count == 0)
            {
                return;
            }

            if (!cellHeights.ContainsKey(NSIndexPath.FromRowSection(Sections.Count - 1, Sections.Last().ItemCount - 1)))
            {
                NSIndexPath path = NSIndexPath.FromRowSection(0, 0);
                foreach (var key in cellHeights.Keys)
                {
                    if (key.Section > path.Section || (key.Section == path.Section && key.Row > path.Row))
                    {
                        path = key;
                    }
                }

                for (int i = path.Section; i < Sections.Count; i++)
                {
                    var section = Sections[i];
                    for (int j = (i == path.Section ? path.Row : 0); j < section.ItemCount; j++)
                    {
                        TableView.Source.GetCell(TableView, NSIndexPath.FromRowSection(j, i));
                    } 
                }

                TableView.ReloadData();
            }

            TableView.ScrollRectToVisible(new CGRect(0, TableView.ContentSize.Height - 1, 1, 1), animated);
        }

        public void ScrollToHome(bool animated)
        {
            TableView.ScrollRectToVisible(new CGRect(0, 0, 1, 1), animated);
        }

        public void SetBackground(Color color)
        {
            if (color.IsDefaultColor)
            {
                if (TableView.BackgroundView != null)
                {
                    TableView.BackgroundView.BackgroundColor = null;
                }
                return;
            }
            else if (TableView.BackgroundView == null || TableView.BackgroundView is UIImageView)
            {
                TableView.BackgroundView = new UIView();
            }

            TableView.BackgroundView.BackgroundColor = color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            UIImageView imageView = new UIImageView(UIImage.FromFile(imagePath));
            switch (stretch)
            {
                case iFactr.UI.ContentStretch.Fill:
                    imageView.ContentMode = UIViewContentMode.ScaleToFill;
                    break;
                case iFactr.UI.ContentStretch.None:
                    imageView.ContentMode = UIViewContentMode.Center;
                    break;
                case iFactr.UI.ContentStretch.Uniform:
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    break;
                case iFactr.UI.ContentStretch.UniformToFill:
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                    break;
            }

            TableView.BackgroundView = imageView;
        }

        public void Submit(string url)
        {
            Submit(new Link(url));
        }

        public void Submit(Link link)
        {
            if (link == null)
                return;

            if (link.Parameters == null)
            {
                link.Parameters = new Dictionary<string, string>();
            }

            foreach (var cell in TableView.VisibleCells.OfType<IGridCell>())
            {
                SetSubmitValue(cell);
            }

            SubmissionEventArgs args = new SubmissionEventArgs(link, ValidationErrors);

            var handler = Submitting;
            if (handler != null)
            {
                handler(Pair ?? this, args);
            }

            if (args.Cancel)
                return;

            foreach (string id in submitValues.Keys)
            {
                link.Parameters[id] = submitValues[id];
            }

            iApp.Navigate(link, this);
        }

        public object GetModel()
        {
            return model;
        }

        public void SetModel(object model)
        {
            this.model = model;
        }

        public void Render()
        {
            var handler = Rendering;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }

            submitValues.Clear();
            ReloadSections();
        }

        public bool Equals(IView other)
        {
            var view = other as View;
            if (view != null)
            {
                return view.Equals(this);
            }

            return base.Equals(other);
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown ?
                        UIInterfaceOrientation.PortraitUpsideDown : UIInterfaceOrientation.Portrait;
                case PreferredOrientation.Landscape:
                    return InterfaceOrientation == UIInterfaceOrientation.LandscapeRight ?
                        UIInterfaceOrientation.LandscapeRight : UIInterfaceOrientation.LandscapeLeft;
                default:
                    return base.PreferredInterfaceOrientationForPresentation();
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
                case PreferredOrientation.Landscape:
                    return UIInterfaceOrientationMask.Landscape;
                default:
                    return UIInterfaceOrientationMask.All;
            }
        }

        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                foreach (var cell in cells.Values)
                {
                    try
                    {
                        cell.RemoveFromSuperview();
                    }
                    catch (ObjectDisposedException) { }
                }

                cells.Clear();

                if (TableView.Source != null)
                {
                    TableView.Source.Dispose();
                    TableView.Source = null;
                }

                menu = null;
            });

            base.Dispose(disposing);
        }

        public override void ViewDidLoad()
        {
            defaultSeparatorColor = TableView.SeparatorColor;
            TableView.EstimatedRowHeight = 44;
            TableView.SectionHeaderHeight = UITableView.AutomaticDimension;
            TableView.SectionFooterHeight = UITableView.AutomaticDimension;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.Source = new TableViewDelegate(this);

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                TableView.CellLayoutMarginsFollowReadableWidth = false;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            this.SetMenu(menu);
            this.ConfigureBackButton(BackLink, OutputPane);

            if (TableView.IndexPathForSelectedRow != null)
            {
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);
            }

            if (NavigationItem != null)
            {
                NavigationItem.Title = title ?? string.Empty;
            }

            if (NavigationController != null && NavigationController.NavigationBar != null)
            {
                NavigationController.NavigationBar.BarTintColor = headerColor;
                NavigationController.NavigationBar.TintColor = titleColor;
                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
            }

            RemoveNotifications();

            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillShow:"), UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onKeyboardWillHide:"), UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onTextDidBeginEditing:"), UITextField.TextDidBeginEditingNotification, null);
            NSNotificationCenter.DefaultCenter.AddObserver(this, new ObjCRuntime.Selector("onTextDidBeginEditing:"), UITextView.TextDidBeginEditingNotification, null);

            UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
            {
                var scrollPosition = Metadata.Get<object[]>("iosInitialScrollPosition");
                if (scrollPosition != null)
                {
                    Metadata.Remove("iosInitialScrollPosition");
                    ScrollToCell((int)scrollPosition[0], (int)scrollPosition[1], (bool)scrollPosition[2]);
                }
            });
        }
        
        public override void ViewDidAppear(bool animated)
        {
            var stack = NavigationController as IHistoryStack;
            if (!hasAppeared && (!ModalManager.TransitionInProgress || (stack != null && stack.ID == "Popover")))
            {
                hasAppeared = true;
                var handler = Activated;
                if (handler != null)
                {
                    handler(Pair ?? this, EventArgs.Empty);
                }
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (NavigationItem != null)
            {
                NavigationItem.SetRightBarButtonItem(null, animated);
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            RemoveNotifications();

            var stack = NavigationController as IHistoryStack;
            if (hasAppeared && (!ModalManager.TransitionInProgress || stack == null || stack.ID == "Popover"))
            {
                hasAppeared = false;
                if (stack == null || stack.CurrentView != this)
                {
                    var handler = Deactivated;
                    if (handler != null)
                    {
                        handler(Pair ?? this, EventArgs.Empty);
                    }
                }
            }
        }

        private void RemoveNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextField.TextDidBeginEditingNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextView.TextDidBeginEditingNotification, null);
        }

        [Export("onKeyboardWillShow:")]
        private void OnKeyboardWillShow(NSNotification notification)
        {
            keyboardVisible = true;

            var value = notification.UserInfo.ValueForKey(UIKeyboard.FrameBeginUserInfoKey) as NSValue;
            if (value == null)
            {
                return;
            }

            var responder = TableView.GetSubview<UIView>(v => v.IsFirstResponder);
            if (responder == null)
            {
                return;
            }

            var kbFrame = value.CGRectValue;
            var insets = TableView.ContentInset;
            insets.Bottom = kbFrame.Height;

            TableView.ContentInset = insets;
            TableView.ScrollIndicatorInsets = TableView.ContentInset;

            nfloat y = TableView.Frame.Height - TableView.ContentInset.Bottom - responder.Frame.Height;
            var frame = TableView.ConvertRectFromView(responder.Bounds, responder);
            if (frame.Y + 10 > TableView.ContentOffset.Y + y)
            {
                TableView.SetContentOffset(new CGPoint(TableView.ContentOffset.X, frame.Y - y + 10), false); // true doesn't seem to work correctly
            }
            else if (frame.Y - 10 < TableView.ContentOffset.Y + TableView.ContentInset.Top)
            {
                TableView.SetContentOffset(new CGPoint(TableView.ContentOffset.X,
                    Math.Max(frame.Y - TableView.ContentInset.Top - 10, -TableView.ContentInset.Top)), false);
            }
        }

        [Export("onKeyboardWillHide:")]
        private void OnKeyboardWillHide(NSNotification notification)
        {
            keyboardVisible = false;

            var insets = TableView.ContentInset;
            insets.Bottom = 0;

            var tabController = TableView.GetSuperview<UITabBarController>();
            if (tabController != null)
            {
                insets.Bottom = tabController.TabBar.Frame.Height;
            }

            TableView.ContentInset = insets;
            TableView.ScrollIndicatorInsets = TableView.ContentInset;
        }

        [Export("onTextDidBeginEditing:")]
        private void OnTextDidBeginEditing(NSNotification notification)
        {
            if (keyboardVisible)
            {
                var responder = notification.Object as UIView;
                if (notification.Object == null)
                {
                    return;
                }

                nfloat y = TableView.Frame.Height - TableView.ContentInset.Bottom - responder.Frame.Height;
                var frame = TableView.ConvertRectFromView(responder.Bounds, responder);
                if (frame.Y + 10 > TableView.ContentOffset.Y + y)
                {
                    TableView.SetContentOffset(new CGPoint(TableView.ContentOffset.X, frame.Y - y + 10), true);
                }
                else if (frame.Y - 10 < TableView.ContentOffset.Y + TableView.ContentInset.Top)
                {
                    TableView.SetContentOffset(new CGPoint(TableView.ContentOffset.X,
                        Math.Max(frame.Y - TableView.ContentInset.Top - 10, -TableView.ContentInset.Top)), true);
                }
            }
        }

        private void SetSubmitValue(IGridCell cell)
        {
            foreach (var control in cell.Children.OfType<IControl>().Where(c => c.ShouldSubmit()))
            {
                string[] errors;
                if (!control.Validate(out errors))
                {
                    ValidationErrors[control.SubmitKey] = errors;
                }
                else
                {
                    ValidationErrors.Remove(control.SubmitKey);
                }

                submitValues[control.SubmitKey] = control.StringValue;
            }
        }

        private class TableViewDelegate : UITableViewSource
        {
            private ListView ListView { get { return reference.Target as ListView; } }

            private WeakReference reference;

            public TableViewDelegate(ListView listView)
            {
                reference = new WeakReference(listView);
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return ListView.Sections.Count;
            }

            public override nfloat GetHeightForHeader(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return 0;
                }

                var header = TouchFactory.GetNativeObject<UIView>(ListView.Sections[(int)section].Header, "Header");
                if (header == null)
                {
                    return 0;
                }

                var view = header as UITableViewHeaderFooterView;
                if (view == null)
                {
                    header.LayoutSubviews();
                    return header.Frame.Height;
                }

                if (tableView.Style == UITableViewStyle.Plain)
                {
                    return view.TextLabel.Font.LineHeight + 6;
                }

                view.TextLabel.Lines = 0;

                return (NFloat)Math.Max(section == 0 ? 56 : 38,
                    view.TextLabel.SizeThatFits(new CGSize(tableView.Frame.Width - 76, tableView.Frame.Height)).Height + 15);
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return null;
                }

                var s = ListView.Sections[(int)section];
                return s.Header == null ? null : s.Header.Text;
            }

            public override UIView GetViewForHeader(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return null;
                }
                
                var header = TouchFactory.GetNativeObject<UIView>(ListView.Sections[(int)section].Header, "Header");
                var view = header as UITableViewHeaderFooterView;
                if (view != null && view.TextLabel != null)
                {
                    view.TextLabel.Lines = 0;
                }

                if (header != null)
                {
                    header.LayoutSubviews();
                }

                return header;
            }

            public override nfloat GetHeightForFooter(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return 0;
                }

                var footer = TouchFactory.GetNativeObject<UIView>(ListView.Sections[(int)section].Footer, "Footer");
                if (footer == null)
                {
                    return 0;
                }

                var view = footer as UITableViewHeaderFooterView;
                if (view == null)
                {
                    footer.LayoutSubviews();
                    return footer.Frame.Height;
                }

                if (tableView.Style == UITableViewStyle.Plain)
                {
                    return view.TextLabel.Font.LineHeight + 6;
                }

                view.TextLabel.Lines = 0;

                return (NFloat)Math.Max(section == ListView.Sections.Count - 1 ? 48 : 30,
                    view.TextLabel.SizeThatFits(new CGSize(tableView.Frame.Width - 76, tableView.Frame.Height)).Height + 15);
            }

            public override string TitleForFooter(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return null;
                }

                var s = ListView.Sections[(int)section];
                return s.Footer == null ? null : s.Footer.Text;
            }
            
            public override UIView GetViewForFooter(UITableView tableView, nint section)
            {
                if (ListView.Sections.Count <= section)
                {
                    return null;
                }
                
                var footer = TouchFactory.GetNativeObject<UIView>(ListView.Sections[(int)section].Footer, "Footer");
                var view = footer as UITableViewHeaderFooterView;
                if (view != null && view.TextLabel != null)
                {
                    view.TextLabel.Lines = 0;
                }

                if (footer != null)
                {
                    footer.LayoutSubviews();
                }

                return footer;
            }
            
            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return ListView.Sections.Count > section ? ListView.Sections[(int)section].ItemCount : 0;
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return DictionaryExtensions.GetValueOrDefault(ListView.cellHeights, indexPath, tableView.EstimatedRowHeight);
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = DictionaryExtensions.GetValueOrDefault(ListView.cells, indexPath);
                if (cell == null)
                {
                    return DictionaryExtensions.GetValueOrDefault(ListView.cellHeights, indexPath, tableView.EstimatedRowHeight);
                }

                var rich = cell as IRichContentCell;
                if (rich != null)
                {
                    rich.Load();
                }

                return cell.Frame.Height;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                IRichContentCell richCell = null;
                if (ListView.richContentCells.TryGetValue(indexPath, out richCell))
                {
                    ListView.cells[indexPath] = (UITableViewCell)richCell;
                    return (UITableViewCell)richCell;
                }

                var section = ListView.Sections.Count > indexPath.Section ? ListView.Sections[indexPath.Section] : null;
                if (section != null && section.ItemIdRequested != null)
                {
                    CellId = section.ItemIdRequested(indexPath.Row);
                }
                else if (ListView.ItemIdRequested != null)
                {
                    CellId = ListView.ItemIdRequested(indexPath.Section, indexPath.Row);
                }

                // currently avoiding recycling rich content cells due to graphical problems
                ICell cell = tableView.DequeueReusableCell(CellId.ToString()) as IGridCell;
                if (section != null && section.CellRequested != null)
                {
                    cell = section.CellRequested(indexPath.Row, cell == null ? null : (cell.Pair as ICell) ?? cell);
                }
                else if (ListView.CellRequested != null)
                {
                    cell = ListView.CellRequested(indexPath.Section, indexPath.Row, cell == null ? null : (cell.Pair as ICell) ?? cell);
                }

                UITableViewCell tableCell = null;
                CustomItemContainer container = cell as CustomItemContainer;
                if (container != null)
                {
                    tableCell = container.CustomItem as UITableViewCell;
                }
                else
                {
                    tableCell = TouchFactory.GetNativeObject<UITableViewCell>(cell, "cell");
                }

                if (tableCell != null)
                {
                    // if we don't do this, the cell frame may not be correct during layout
                    var frame = tableCell.Frame;
                    frame.Width = tableView.Frame.Width;
                    tableCell.Frame = frame;

                    tableCell.LayoutSubviews();
                    ListView.cells[indexPath] = tableCell;
                    ListView.cellHeights[indexPath] = tableCell.Frame.Height;
                }
                
                richCell = tableCell as IRichContentCell;
                if (richCell != null)
                {
                    ListView.richContentCells[indexPath] = richCell;
                    richCell.Load();
                }
                else if (ListView.richContentCells.ContainsKey(indexPath))
                {
                    ListView.richContentCells.Remove(indexPath);
                }

                return tableCell ?? new UITableViewCell();
            }
            
            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as IGridCell;
                if (cell != null)
                {
                    cell.Select();
                }
            }

            public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cell.RemoveFromSuperview();

                var icell = cell as IGridCell;
                if (icell != null)
                {
                    ListView.SetSubmitValue(icell);
                }

                cell.EndEditing(true);

                if (indexPath == null)
                {
                    var key = ListView.cells.FirstOrDefault(kvp => kvp.Value == cell).Key;
                    if (key != null)
                    {
                        ListView.cells.Remove(key);
                    }
                }
                else
                {
                    ListView.cells.Remove(indexPath);
                }
            }
        }

        private class IndexPathComparer : IEqualityComparer<NSIndexPath>
        {
            public static IndexPathComparer Instance { get; private set; }

            static IndexPathComparer()
            {
                Instance = new IndexPathComparer();
            }

            public bool Equals(NSIndexPath x, NSIndexPath y)
            {
                if (x == null) return y == null;
                if (y == null) return false;
                return x.Section == y.Section && x.Row == y.Row;
            }

            public int GetHashCode(NSIndexPath obj)
            {
                return obj.Section ^ obj.Row;
            }
        }
    }
    #endregion

    #region iOS7 and below
    public class ListViewLegacy : UIViewController, IListView, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler Activated;

        public event EventHandler Deactivated;

        public event EventHandler Rendering;

        public event EventHandler Scrolled;

        public event SubmissionEventHandler Submitting;

        public ColumnMode ColumnMode
        {
            get { return columnMode; }
            set
            {
                if (value != columnMode)
                {
                    columnMode = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ColumnMode"));
                    }
                }
            }
        }
        private ColumnMode columnMode;

        public ListViewStyle Style { get; private set; }

        public IMenu Menu
        {
            get { return menu; }
            set
            {
                if (value != menu)
                {
                    menu = value;
                    this.SetMenu(menu);

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Menu"));
                    }
                }
            }
        }
        private IMenu menu;

        public MetadataCollection Metadata
        {
            get { return metadata ?? (metadata = new MetadataCollection()); }
        }
        private MetadataCollection metadata;

        public PreferredOrientation PreferredOrientations
        {
            get { return preferredOrientations; }
            set
            {
                if (value != preferredOrientations)
                {
                    preferredOrientations = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PreferredOrientations"));
                    }
                }
            }
        }
        private PreferredOrientation preferredOrientations;

        public string StackID
        {
            get { return stackID; }
            set
            {
                if (value != stackID)
                {
                    stackID = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("StackID"));
                    }
                }
            }
        }
        private string stackID;

        public Link BackLink
        {
            get { return backLink; }
            set
            {
                if (value != backLink)
                {
                    backLink = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("BackLink"));
                    }
                }
            }
        }
        private Link backLink;

        public CellDelegate CellRequested
        {
            get { return cellRequested; }
            set
            {
                if (value != cellRequested)
                {
                    cellRequested = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("CellRequested"));
                    }
                }
            }
        }
        private CellDelegate cellRequested;

        public ItemIdDelegate ItemIdRequested
        {
            get { return itemIdRequested; }
            set
            {
                if (value != itemIdRequested)
                {
                    itemIdRequested = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ItemIdRequested"));
                    }
                }
            }
        }
        private ItemIdDelegate itemIdRequested;

        public ShouldNavigateDelegate ShouldNavigate
        {
            get { return shouldNavigate; }
            set
            {
                if (value != shouldNavigate)
                {
                    shouldNavigate = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("ShouldNavigate"));
                    }
                }
            }
        }
        private ShouldNavigateDelegate shouldNavigate;

        public Pane OutputPane
        {
            get { return outputPane; }
            set
            {
                if (value != outputPane)
                {
                    outputPane = value;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("OutputPane"));
                    }
                }
            }
        }
        private Pane outputPane;

        public PopoverPresentationStyle PopoverPresentationStyle
        {
            get { return ModalPresentationStyle == UIModalPresentationStyle.FullScreen ? PopoverPresentationStyle.FullScreen : PopoverPresentationStyle.Normal; }
            set
            {
                if (value != PopoverPresentationStyle)
                {
                    ModalPresentationStyle = value == PopoverPresentationStyle.FullScreen ? UIModalPresentationStyle.FullScreen : UIModalPresentationStyle.FormSheet;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("PopoverPresentationStyle"));
                    }
                }
            }
        }

        public IHistoryStack Stack
        {
            get
            {
                var stack = NavigationController as IHistoryStack;
                if (stack != null && PaneManager.Instance.Contains(stack))
                {
                    return stack;
                }

                return null;
            }
        }

        public Color HeaderColor
        {
            get { return headerColor.ToColor(); }
            set
            {
                if (value != headerColor.ToColor())
                {
                    headerColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            NavigationController.NavigationBar.BarTintColor = headerColor;
                        }
                        else
                        {
                            NavigationController.NavigationBar.TintColor = headerColor ?? UIColor.Black;
                        }
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("HeaderColor"));
                    }
                }
            }
        }
        private UIColor headerColor;

        public Color TitleColor
        {
            get { return titleColor.ToColor(); }
            set
            {
                if (value != titleColor.ToColor())
                {
                    titleColor = value.IsDefaultColor ? null : value.ToUIColor();
                    if (NavigationController != null && NavigationController.NavigationBar != null && NavigationController.VisibleViewController == this)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            NavigationController.NavigationBar.TintColor = titleColor;
                        }

                        NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("TitleColor"));
                    }
                }
            }
        }
        private UIColor titleColor;

        public new string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    
                    if (NavigationItem != null)
                    {
                        NavigationItem.Title = title ?? string.Empty;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Title"));
                    }
                }
            }
        }
        private string title;

        public Color SeparatorColor
        {
            get { return TableView.SeparatorColor.ToColor(); }
            set
            {
                if (value != TableView.SeparatorColor.ToColor())
                {
                    TableView.SeparatorColor = value.IsDefaultColor ? null : value.ToUIColor();

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SeparatorColor"));
                    }
                }
            }
        }

        public double Height { get; private set; }

        public double Width { get; private set; }

        public SectionCollection Sections { get; private set; }

        public ValidationErrorCollection ValidationErrors { get; private set; }

        public ISearchBox SearchBox
        {
            get
            {
                var box = searchBar as ISearchBox;
                return box == null ? null : (box.Pair ?? box) as ISearchBox;
            }
            set
            {
                if (value != SearchBox)
                {
                    if (searchBar != null)
                    {
                        searchBar.RemoveFromSuperview();
                    }

                    searchBar = TouchFactory.GetNativeObject<UIView>(value, "searchBox");
                    if (searchBar != null)
                    {
                        TableView.TableHeaderView = searchBar;
                    }

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("SearchBox"));
                    }
                }
            }
        }
        private UIView searchBar;

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

        public Type ModelType
        {
            get { return model == null ? null : model.GetType(); }
        }
        
        public TableView TableView
        {
            get { return View as TableView; }
        }

        private bool hasAppeared;
        private object model;
        private Dictionary<string, string> submitValues;
        private Dictionary<NSIndexPath, IRichContentCell> richContentCells;

        public ListViewLegacy()
            : this(ListViewStyle.Default)
        {
        }

        public ListViewLegacy(ListViewStyle style)
        {
            Style = style;

            Sections = new SectionCollection();
            ValidationErrors = new ValidationErrorCollection();
            
            submitValues = new Dictionary<string, string>();
            richContentCells = new Dictionary<NSIndexPath, IRichContentCell>();
        }

        public IDictionary<string, string> GetSubmissionValues()
        {
            foreach (var cell in TableView.VisibleCells.OfType<IGridCell>())
            {
                SetSubmitValue(cell);
            }

            return new Dictionary<string, string>(submitValues);
        }

        public IEnumerable<ICell> GetVisibleCells()
        {
            foreach (var cell in TableView.VisibleCells.OfType<ICell>())
            {
                yield return (cell.Pair as ICell) ?? cell;
            }
        }

        public void ReloadSections()
        {
            richContentCells.Clear();
            TableView.ReloadData();
        }

        public void ScrollToCell(int section, int index, bool animated)
        {
            iApp.Log.Warn("ScrollToCell is only supported on iOS 8 and above.");
        }

        public void ScrollToEnd(bool animated)
        {
            iApp.Log.Warn("ScrollToEnd is only supported on iOS 8 and above.");
        }

        public void ScrollToHome(bool animated)
        {
            TableView.ScrollToTop(animated);
        }

        public void SetBackground(Color color)
        {
            if (color.IsDefaultColor)
            {
                if (TableView.BackgroundView != null)
                {
                    TableView.BackgroundView.BackgroundColor = null;
                }
                return;
            }
            else if (TableView.BackgroundView == null || TableView.BackgroundView is UIImageView)
            {
                TableView.BackgroundView = new UIView();
            }

            TableView.BackgroundView.BackgroundColor = color.ToUIColor();
        }

        public void SetBackground(string imagePath, ContentStretch stretch)
        {
            UIImageView imageView = new UIImageView(UIImage.FromFile(imagePath));
            switch (stretch)
            {
                case iFactr.UI.ContentStretch.Fill:
                    imageView.ContentMode = UIViewContentMode.ScaleToFill;
                    break;
                case iFactr.UI.ContentStretch.None:
                    imageView.ContentMode = UIViewContentMode.Center;
                    break;
                case iFactr.UI.ContentStretch.Uniform:
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    break;
                case iFactr.UI.ContentStretch.UniformToFill:
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                    break;
            }

            TableView.BackgroundView = imageView;
        }

        public void Submit(string url)
        {
            Submit(new Link(url));
        }

        public void Submit(Link link)
        {
            if (link == null)
                return;

            if (link.Parameters == null)
            {
                link.Parameters = new Dictionary<string, string>();
            }

            foreach (var cell in TableView.VisibleCells.OfType<IGridCell>())
            {
                SetSubmitValue(cell);
            }

            SubmissionEventArgs args = new SubmissionEventArgs(link, ValidationErrors);

            var handler = Submitting;
            if (handler != null)
            {
                handler(Pair ?? this, args);
            }

            if (args.Cancel)
                return;

            foreach (string id in submitValues.Keys)
            {
                link.Parameters[id] = submitValues[id];
            }

            iApp.Navigate(link, this);
        }

        public object GetModel()
        {
            return model;
        }

        public void SetModel(object model)
        {
            this.model = model;
        }

        public void Render()
        {
            foreach (var cell in TableView.VisibleCells)
            {
                var view = cell.GetSubview<UIView>(v => v.IsFirstResponder);
                if (view != null)
                {
                    view.ResignFirstResponder();
                }
            }

            var handler = Rendering;
            if (handler != null)
            {
                handler(Pair ?? this, EventArgs.Empty);
            }

            submitValues.Clear();
            ReloadSections();
        }

        public bool Equals(IView other)
        {
            var view = other as View;
            if (view != null)
            {
                return view.Equals(this);
            }

            return base.Equals(other);
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown ?
                        UIInterfaceOrientation.PortraitUpsideDown : UIInterfaceOrientation.Portrait;
                case PreferredOrientation.Landscape:
                    return InterfaceOrientation == UIInterfaceOrientation.LandscapeRight ?
                        UIInterfaceOrientation.LandscapeRight : UIInterfaceOrientation.LandscapeLeft;
                default:
                    return base.PreferredInterfaceOrientationForPresentation();
            }
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            switch (PreferredOrientations)
            {
                case PreferredOrientation.Portrait:
                    return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
                case PreferredOrientation.Landscape:
                    return UIInterfaceOrientationMask.Landscape;
                default:
                    return UIInterfaceOrientationMask.All;
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath.ToString() == "bounds")
            {
                var frame = ((NSValue)change.ObjectForKey(NSObject.ChangeNewKey)).CGRectValue;
                if (frame.Width != Width)
                {
                    Width = frame.Width;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Width"));
                    }
                }

                if (frame.Height != Height)
                {
                    Height = frame.Height;

                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Height"));
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                try
                {
                    View.Layer.RemoveObserver(this, new NSString("bounds"));
                }
                catch (Exception) { }
            });
        }

        public override void ViewDidLoad()
        {
            View = new TableView((UITableViewStyle)Style)
            {
                Frame = View.Frame,
                Source = new ListViewSource(this)
            };

            Width = View.Frame.Width;
            Height = View.Frame.Height;

            View.Layer.AddObserver(this, new NSString("bounds"), NSKeyValueObservingOptions.New, IntPtr.Zero);

            TableView.Scrolled += (sender, e) =>
            {
                var handler = Scrolled;
                if (handler != null)
                {
                    handler(pair ?? this, EventArgs.Empty);
                }
            };
       }

        public override void ViewWillAppear(bool animated)
        {
            this.SetMenu(menu);
            this.ConfigureBackButton(BackLink, OutputPane);

            if (TableView.IndexPathForSelectedRow != null)
            {
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);
            }
            
            if (NavigationItem != null)
            {
                NavigationItem.Title = title ?? string.Empty;
            }

            if (NavigationController != null && NavigationController.NavigationBar != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    NavigationController.NavigationBar.BarTintColor = headerColor;
                    NavigationController.NavigationBar.TintColor = titleColor;
                }
                else
                {
                    NavigationController.NavigationBar.TintColor = headerColor ?? UIColor.Black;
                }

                NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes() { ForegroundColor = titleColor };
            }
        }
        
        public override void ViewDidAppear(bool animated)
        {
            if (!hasAppeared && !ModalManager.TransitionInProgress)
            {
                hasAppeared = true;
                var handler = Activated;
                if (handler != null)
                {
                    handler(Pair ?? this, EventArgs.Empty);
                }
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            foreach (var cell in TableView.VisibleCells)
            {
                var view = cell.GetSubview<UIView>(v => v.IsFirstResponder);
                if (view != null)
                {
                    view.ResignFirstResponder();
                }
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (hasAppeared && !ModalManager.TransitionInProgress)
            {
                hasAppeared = false;
                var stack = NavigationController as IHistoryStack;
                if (stack == null || stack.CurrentView != this)
                {
                    var handler = Deactivated;
                    if (handler != null)
                    {
                        handler(Pair ?? this, EventArgs.Empty);
                    }
                }
            }
        }

        private void SetSubmitValue(IGridCell cell)
        {
            foreach (var control in cell.Children.OfType<IControl>().Where(c => c.ShouldSubmit()))
            {
                string[] errors;
                if (!control.Validate(out errors))
                {
                    ValidationErrors[control.SubmitKey] = errors;
                }
                else
                {
                    ValidationErrors.Remove(control.SubmitKey);
                }

                submitValues[control.SubmitKey] = control.StringValue;
            }
        }
        
        private class ListViewSource : TableViewSource
        {
            private ListViewLegacy listView;
            
            public ListViewSource(ListViewLegacy listView)
            {
                this.listView = listView;
            }
            
            public override int NumberOfSections(TableView tableView)
            {
                return listView.Sections.Count;
            }
            
            public override nfloat GetHeightForHeader(TableView tableView, int section)
            {
                if (listView.Sections.Count <= section)
                {
                    return 0;
                }
                
                var s = listView.Sections[section];
                return s.Header == null ? 0 : tableView.Style == UITableViewStyle.Grouped ? 35 : 20;
            }
            
            public override nfloat GetHeightForFooter(TableView tableView, int section)
            {
                if (listView.Sections.Count <= section)
                {
                    return 0;
                }
                
                var s = listView.Sections[section];
                return s.Footer == null ? 0 : tableView.Style == UITableViewStyle.Grouped ? 30 : 20;
            }
            
            public override UIView GetViewForHeader(TableView tableView, int section)
            {
                return listView.Sections.Count > section ?
                    TouchFactory.GetNativeObject<UIView>(listView.Sections[section].Header, "Header") : null;
            }
            
            public override UIView GetViewForFooter(TableView tableView, int section)
            {
                return listView.Sections.Count > section ?
                    TouchFactory.GetNativeObject<UIView>(listView.Sections[section].Footer, "Footer") : null;
            }
            
            public override int RowsInSection(TableView tableView, int section)
            {
                return listView.Sections.Count > section ? listView.Sections[section].ItemCount : 0;
            }
            
            public override UITableViewCell GetCell(TableView tableView, NSIndexPath indexPath)
            {
                IRichContentCell richCell = null;
                if (listView.richContentCells.TryGetValue(indexPath, out richCell))
                {
                    return (UITableViewCell)richCell;
                }
    
                Section section = null;
                if (listView.Sections.Count > indexPath.Section)
                {
                    section = listView.Sections[indexPath.Section];
                }
    
                int id = 0;
                if (section != null && section.ItemIdRequested != null)
                {
                    id = section.ItemIdRequested(indexPath.Row);
                }
                else if (listView.ItemIdRequested != null)
                {
                    id = listView.ItemIdRequested(indexPath.Section, indexPath.Row);
                }
    
                // currently avoiding recycling rich content cells due to graphical problems
                ICell cell = listView.TableView.DequeueReusableCell(id) as IGridCell;
                if (section != null && section.CellRequested != null)
                {
                    cell = section.CellRequested(indexPath.Row, cell == null ? null : (cell.Pair as ICell) ?? cell);
                }
                else if (listView.CellRequested != null)
                {
                    cell = listView.CellRequested(indexPath.Section, indexPath.Row, cell == null ? null : (cell.Pair as ICell) ?? cell);
                }
    
                UITableViewCell tableCell = null;
                CustomItemContainer container = cell as CustomItemContainer;
                if (container != null)
                {
                    tableCell = container.CustomItem as UITableViewCell;
                }
                else
                {
                    tableCell = TouchFactory.GetNativeObject<UITableViewCell>(cell, "cell");
                }

                if (tableCell != null)
                {
                    tableCell.Tag = id;
                    tableCell.LayoutSubviews();
                }
                
                richCell = tableCell as IRichContentCell;
                if (richCell != null)
                {
                    listView.richContentCells[indexPath] = richCell;
                    richCell.Load();
                }
                else if (listView.richContentCells.ContainsKey(indexPath))
                {
                    listView.richContentCells.Remove(indexPath);
                }
    
                return tableCell ?? new UITableViewCell();
            }
            
            public override void RowSelected(TableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath) as IGridCell;
                if (cell != null)
                {
                    cell.Select();
                }
            }
            
            public override void CellDisplayingEnded(TableView tableView, UITableViewCell cell)
            {
                var icell = cell as IGridCell;
                if (icell != null)
                {
                    listView.SetSubmitValue(icell);
                }
    
                cell.EndEditing(true);
            }
        }
    }
    #endregion
}