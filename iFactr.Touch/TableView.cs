using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace iFactr.Touch
{
    public class TableView : UIScrollView
    {
        public TableViewSource Source
        {
            get { return source; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                source = value;
            }
        }
        private TableViewSource source;

        public override CGSize ContentSize
        {
            get
            {
                return base.ContentSize;
            }
            set
            {
                trueContentSize = value;

                nfloat minHeight = (Frame.Height - (ContentInset.Top + ContentInset.Bottom)) + 1;
                if (value.Height < minHeight)
                {
                    value.Height = minHeight;
                }
                base.ContentSize = value;
            }
        }
        private CGSize trueContentSize;

        public override UIEdgeInsets ContentInset
        {
            get
            {
                return base.ContentInset;
            }
            set
            {
                base.ContentInset = value;
                nfloat minHeight = (Frame.Height - (base.ContentInset.Top + base.ContentInset.Bottom)) + 1;
                if (base.ContentSize.Height > trueContentSize.Height || trueContentSize.Height < minHeight)
                {
                    base.ContentSize = new CGSize(ContentSize.Width, NMath.Max(trueContentSize.Height, minHeight));
                }
            }
        }

        public UIView BackgroundView
        {
            get { return Subviews.FirstOrDefault(); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (value is UITableViewCell)
                {
                    throw new ArgumentException("Invalid view type");
                }

                if (Subviews.Length > 0)
                {
                    Subviews.First().RemoveFromSuperview();
                }

                base.InsertSubview(value, 0);
            }
        }

        public UIView TableHeaderView
        {
            get { return tableHeaderView; }
            set
            {
                if (tableHeaderView != null)
                {
                    var inset = ContentInset;
                    inset.Top -= tableHeaderView.Frame.Height;
                    ContentInset = inset;

                    tableHeaderView.RemoveFromSuperview();
                }

                tableHeaderView = value;
                if (tableHeaderView != null)
                {
                    Add(tableHeaderView);
                }
            }
        }
        private UIView tableHeaderView;
        
        public nfloat RowHeight { get; private set; }
        
        public nfloat SectionHeaderHeight { get; private set; }
        
        public nfloat SectionFooterHeight { get; private set; }
        
        public NSIndexPath IndexPathForSelectedRow { get; private set; }
        
        public UIColor SeparatorColor { get; set; }
        
        public UITableViewStyle Style { get; private set; }
        
        public UITableViewCell[] VisibleCells
        {
            get { return Subviews.OfType<UITableViewCell>().ToArray(); }
        }
        
        private nfloat Top
        {
            get { return ContentOffset.Y + ContentInset.Top; }
        }
        
        private nfloat Bottom
        {
            get { return ContentOffset.Y + Frame.Height - ContentInset.Bottom; }
        }

        private bool isRounded;
        private bool keyboardVisible;
        private bool scrollingDown;
        private nfloat lastYOffset;
        private CGSize currentSize;
        private Section[] sections;
        private List<Section> visibleSections;
        private List<UITableViewCell> offScreenCells;
        private NSIndexPath currentCellIndex;
        private UIColor defaultSeparatorColor;
        private UIColor defaultHeaderFooterBackground;
        private UIColor defaultHeaderFooterForeground;
        private UIColor defaultCellBackground;
        
        public TableView(UITableViewStyle style)
        {
            isRounded = (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && style == UITableViewStyle.Grouped);
            BackgroundView = new UITableView(Frame, style) { SeparatorStyle = UITableViewCellSeparatorStyle.None, BackgroundColor = null };
            Source = new TableViewSource();
            Style = style;
            
            visibleSections = new List<Section>();
            offScreenCells = new List<UITableViewCell>();
            scrollingDown = true;
            
            // creating a dummy table view to get a few default values
            var table = new UITableView(new CGRect(), style);
            BackgroundColor = table.BackgroundColor;
            RowHeight = 44;//table.RowHeight; // because iOS 8 doesn't provide defaults anymore, we're making our own
            SectionHeaderHeight = 23;//table.SectionHeaderHeight;
            SectionFooterHeight = 23;//table.SectionFooterHeight;
            defaultSeparatorColor = table.SeparatorColor;
            defaultCellBackground = UIColor.White;

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                if (Style == UITableViewStyle.Grouped)
                {
                    defaultHeaderFooterBackground = BackgroundColor;
                    defaultHeaderFooterForeground = new UIColor(0.43f, 0.43f, 0.45f, 1);
                }
                else
                {
                    defaultHeaderFooterBackground = new UIColor(0.97f, 0.97f, 0.97f, 1);
                    defaultHeaderFooterForeground = new UIColor(0.14f, 0.14f, 0.14f, 1);
                }
            }
            else
            {
                if (Style == UITableViewStyle.Grouped)
                {
                    defaultHeaderFooterBackground = BackgroundColor;
                    defaultHeaderFooterForeground = new UIColor(0.3f, 0.34f, 0.42f, 1);
                    defaultCellBackground = new UIColor(0.97f, 0.97f, 0.97f, 1);
                }
                else
                {
                    defaultHeaderFooterBackground = new UIColor(0.69f, 0.73f, 0.76f, 1);
                    defaultHeaderFooterForeground = UIColor.White;
                }
            }
            
            ScrolledToTop += (sender, e) =>
            {
                if (Window != null)
                {
                    foreach (var section in visibleSections)
                    {
                        for (int i = section.VisibleCells.Count - 1; i >= 0; i--)
                        {
                            OnCellEndDisplay(section.VisibleCells[i], section);
                        }
                    }

                    lastYOffset = 0;
                    currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
                    visibleSections.Clear();
                    GetCells();
                }
            };
            
            AddGestureRecognizer(new UITapGestureRecognizer((tap) =>
            {
                var location = tap.LocationInView(this);
                for (int i = 0; i < visibleSections.Count; i++)
                {
                    var section = visibleSections[i];
                    for (int j = 0; j < section.VisibleCells.Count; j++)
                    {
                        var cell = section.VisibleCells[j];
                        var frame = cell.Frame;
                        if (frame.Contains(location))
                        {
                            DeselectRow(IndexPathForSelectedRow, false);
                            IndexPathForSelectedRow = IndexPathForCell(cell);
                            cell.SetSelected(true, false);
                            Source.RowSelected(this, IndexPathForSelectedRow);
                            return;
                        }
                    }
                }
            }));
        }

        public void ScrollToTop(bool animated)
        {
            if (ContentOffset.Y != -ContentInset.Top)
            {
                SetContentOffset(new CGPoint(ContentOffset.X, -ContentInset.Top), animated);
            }
        }
        
        public UITableViewCell DequeueReusableCell(int identifier)
        {
            return offScreenCells.FirstOrDefault(c => c.Tag == identifier);
        }
        
        public UITableViewCell CellAt(NSIndexPath indexPath)
        {
            return GetCellAtPath(indexPath) ?? Source.GetCell(this, indexPath);
        }
        
        public NSIndexPath IndexPathForCell(UITableViewCell cell)
        {
            var section = visibleSections.FirstOrDefault();
            if (section == null)
            {
                return null;
            }
            
            int index = section.VisibleCells.IndexOf(cell);
            if (visibleSections.Count > 1)
            {
                if (index >= 0)
                {
                    return NSIndexPath.FromRowSection(section.Cells.Length - (section.VisibleCells.Count - index), sections.IndexOf(section));
                }
                
                for (int i = 1; i < visibleSections.Count; i++)
                {
                    section = visibleSections[i];
                    index = section.VisibleCells.IndexOf(cell);
                    if (index >= 0)
                    {
                        return NSIndexPath.FromRowSection(index, sections.IndexOf(section));
                    }
                }
            }
            else if (index >= 0)
            {
                int row = Math.Max(Math.Min(currentCellIndex.Row, section.Cells.Length - 1), 0);
                return scrollingDown ? NSIndexPath.FromRowSection((row + 1) - (section.VisibleCells.Count - index), currentCellIndex.Section) :
                    NSIndexPath.FromRowSection(row + index, currentCellIndex.Section);
            }
            
            return null;
        }
        
        public void SelectRow(NSIndexPath indexPath, bool animated)
        {
            DeselectRow(IndexPathForSelectedRow, false);
            IndexPathForSelectedRow = indexPath;
            
            var cell = GetCellAtPath(indexPath);
            if (cell != null)
            {
                cell.SetSelected(true, animated);
            }
            
            Source.RowSelected(this, indexPath);
        }
        
        public void DeselectRow(NSIndexPath indexPath, bool animated)
        {
            if (indexPath == null || IndexPathForSelectedRow == null ||
                indexPath.Section != IndexPathForSelectedRow.Section || indexPath.Row != IndexPathForSelectedRow.Row)
            {
                return;
            }
            
            IndexPathForSelectedRow = null;
            var cell = GetCellAtPath(indexPath);
            if (cell != null)
            {
                cell.SetSelected(false, animated);
            }
        }
        
        public void ReloadData()
        {
            sections = new Section[Source.NumberOfSections(this)];
            for (int i = 0; i < sections.Length; i++)
            {
                var section = new Section(this, Source.RowsInSection(this, i));
                section.Height = (section.Cells.Length * RowHeight);
                
                nfloat height = Source.GetHeightForHeader(this, i);
                if (height == 0 && Style == UITableViewStyle.Grouped)
                {
                    height = SectionHeaderHeight + (i == 0 ? SectionFooterHeight : 0);
                }
                section.Height += height;
                
                height = Source.GetHeightForFooter(this, i);
                if (height == 0 && Style == UITableViewStyle.Grouped)
                {
                    height = SectionFooterHeight + (i == sections.Length - 1 ? SectionHeaderHeight : 0);
                }
                section.Height += height;
                
                sections[i] = section;
            }

            foreach (var section in visibleSections)
            {
                for (int i = section.VisibleCells.Count -1; i >= 0; i--)
                {
                    OnCellEndDisplay(section.VisibleCells[i], section);
                }

                if (section.Header != null)
                {
                    section.Header.RemoveFromSuperview();
                }

                if (section.Footer != null)
                {
                    section.Footer.RemoveFromSuperview();
                }
            }

            visibleSections.Clear();
            offScreenCells.Clear();
            currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
            
            ContentSize = new CGSize(ContentSize.Width, (nfloat)sections.Sum(s => s.Height) +
                (tableHeaderView == null ? 0 : tableHeaderView.Frame.Height));

            ScrollRectToVisible(new CGRect(0, 0, 1, 1), false);
            
            if (Window != null)
            {
                GetCells();
            }
        }

        public void ResizeRow(NSIndexPath indexPath)
        {
            if (indexPath == null)
            {
                throw new ArgumentNullException("indexPath");
            }

            var cell = GetCellAtPath(indexPath);
            if (cell != null)
            {
                nfloat delta = 0;
                for (int i = 0; i < visibleSections.Count; i++)
                {
                    var section = visibleSections[i];

                    if (delta != 0)
                    {
                        section.Y += delta;
                        section.ClampHeader(this);
                        section.ClampFooter(this);
                    }

                    for (int j = 0; j < section.VisibleCells.Count; j++)
                    {
                        var visibleCell = section.VisibleCells[j];
                        if (visibleCell == cell)
                        {
                            delta = visibleCell.Frame.Height - section.Cells[indexPath.Row];
                            if (delta == 0)
                            {
                                return;
                            }

                            ContentSize = new CGSize(ContentSize.Width, trueContentSize.Height + delta);
                            SetSeparator(visibleCell, indexPath.Row == section.Cells.Length - 1);
                            section.Cells[indexPath.Row] = visibleCell.Frame.Height;
                            section.Height += delta;
                            section.ClampFooter(this);
                        }
                        else
                        {
                            var frame = visibleCell.Frame;
                            frame.Y += delta;
                            visibleCell.Frame = frame;
                        }
                    }
                }
            }
        }
        
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var frame = Frame;
            frame.Y = ContentOffset.Y;
            BackgroundView.Frame = frame;

            if (Window != null)
            {
                if (Frame.Size != currentSize)
                {
                    if (tableHeaderView != null)
                    {
                        frame = tableHeaderView.Frame;
                        frame.Width = Frame.Width;
                        tableHeaderView.Frame = frame;
                    }

                    if (VisibleCells.Length > 0)
                    {
                        nfloat totalDelta = 0;
                        foreach (var section in visibleSections)
                        {
                            nfloat sectionDelta = 0;
                            section.Y += totalDelta;

                            for (int i = 0; i < section.VisibleCells.Count; i++)
                            {
                                var cell = section.VisibleCells[i];
                                int row = IndexPathForCell(cell).Row;

                                frame = cell.Frame;
                                frame.Y += totalDelta;
                                frame.X = isRounded ? 10 : 0;
                                frame.Width = Frame.Width - (frame.X * 2);
                                cell.Frame = frame;

                                cell.LayoutSubviews();
                                SetSeparator(cell, row == section.Cells.Length - 1);

                                nfloat delta = (cell.Frame.Height - section.Cells[row]);
                                section.Cells[row] = cell.Frame.Height;

                                totalDelta += delta;
                                sectionDelta += delta;

                                if (isRounded)
                                {
                                    var indexPath = IndexPathForCell(cell);
                                    var sectionCount = sections[indexPath.Section].Cells.Length;
                                    CAShapeLayer shape = null;
                                    if (indexPath.Row == 0 && sectionCount == 1)
                                    {
                                        shape = new CAShapeLayer();
                                        shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                                            UIRectCorner.AllCorners, new CGSize(10, 10)).CGPath;
                                    }
                                    else if (indexPath.Row == 0)
                                    {
                                        shape = new CAShapeLayer();
                                        shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                                            UIRectCorner.TopLeft | UIRectCorner.TopRight, new CGSize(10, 10)).CGPath;
                                    }
                                    else if (indexPath.Row == sectionCount - 1)
                                    {
                                        shape = new CAShapeLayer();
                                        shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                                            UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new CGSize(10, 10)).CGPath;
                                    }

                                    cell.Layer.Mask = shape;
                                    cell.Layer.MasksToBounds = true;
                                }
                            }

                            section.Height += sectionDelta;
                        }

                        if (totalDelta != 0)
                        {
                            ContentSize = new CGSize(ContentSize.Width, trueContentSize.Height + totalDelta);
                        }
                    }

                    currentSize = Frame.Size;

                    // iOS 7 has a problem with floating headers and footers that result in them being misplaced.
                    // to compensate for it, we need to tell the table to re-layout again.
                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        SetNeedsLayout();
                    }
                }

                GetCells();
            }
        }

        public override void InsertSubview(UIView view, nint atIndex)
        {
            base.InsertSubview(view, NMath.Max(atIndex, 1));
        }

        public override void SendSubviewToBack(UIView view)
        {
            base.InsertSubviewAbove(view, BackgroundView);
        }
        
        public override void WillMoveToWindow(UIWindow window)
        {
            if (window == null)
            {
                base.WillMoveToWindow(window);
                return;
            }

            RemoveNotifications();

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardWillShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHide);
            NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextDidBeginEditingNotification, OnTextDidBeginEditing);
            NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidBeginEditingNotification, OnTextDidBeginEditing);
            
            if (visibleSections.Count > 0)
            {
                ContentOffset = new CGPoint(ContentOffset.X, lastYOffset);
            }
            else
            {
                lastYOffset = ContentOffset.Y;
            }
            
            base.WillMoveToWindow(window);
        }

        public override void MovedToWindow()
        {
            base.MovedToWindow();

            if (Window == null)
            {
                RemoveNotifications();
            }
        }

        protected override void Dispose(bool disposing)
        {
            RemoveNotifications();
            base.Dispose(disposing);
        }

        private void OnKeyboardWillShow(NSNotification notification)
        {
            keyboardVisible = true;
            
            var value = notification.UserInfo.ValueForKey(UIKeyboard.BoundsUserInfoKey) as NSValue;
            if (value == null)
            {
                return;
            }

            var responder = this.GetSubview<UIView>(v => v.IsFirstResponder);
            if (responder == null)
            {
                return;
            }

            var kbFrame = value.CGRectValue;
            var insets = ContentInset;
            insets.Bottom = kbFrame.Height;

            if (!UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                var tabController = this.GetSuperview<UITabBarController>();
                if (tabController != null)
                {
                    insets.Bottom -= tabController.TabBar.Frame.Height;
                }
            }

            ContentInset = insets;
            ScrollIndicatorInsets = ContentInset;

            nfloat y = Frame.Height - ContentInset.Bottom - responder.Frame.Height;
            var frame = ConvertRectFromView(responder.Bounds, responder);
            if (frame.Y + 10 > ContentOffset.Y + y)
            {
                SetContentOffset(new CGPoint(ContentOffset.X, frame.Y - y + 10), true);
            }
            else if (frame.Y - 10 < ContentOffset.Y + ContentInset.Top)
            {
                SetContentOffset(new CGPoint(ContentOffset.X, NMath.Max(frame.Y - ContentInset.Top - 10, -ContentInset.Top)), true);
            }
        }

        private void OnKeyboardWillHide(NSNotification notification)
        {
            keyboardVisible = false;

            var insets = ContentInset;
            insets.Bottom = 0;

            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
              var tabController = this.GetSuperview<UITabBarController>();
              if (tabController != null)
              {
                  insets.Bottom = tabController.TabBar.Frame.Height;
              }
            }

            ContentInset = insets;
            ScrollIndicatorInsets = ContentInset;
        }

        private void OnTextDidBeginEditing(NSNotification notification)
        {
            if (keyboardVisible)
            {
                var responder = notification.Object as UIView;
                if (notification.Object == null)
                {
                    return;
                }

                nfloat y = Frame.Height - ContentInset.Bottom - responder.Frame.Height;
                var frame = ConvertRectFromView(responder.Bounds, responder);
                if (frame.Y + 10 > ContentOffset.Y + y)
                {
                    SetContentOffset(new CGPoint(ContentOffset.X, frame.Y - y + 10), true);
                }
                else if (frame.Y - 10 < ContentOffset.Y + ContentInset.Top)
                {
                    SetContentOffset(new CGPoint(ContentOffset.X, NMath.Max(frame.Y - ContentInset.Top - 10, -ContentInset.Top)), true);
                }
            }
        }
        
        private void GetCells()
        {
            if (sections == null || sections.Length == 0)
                return;
            
            bool goingDown = ContentOffset.Y >= lastYOffset;
            lastYOffset = ContentOffset.Y;
            
            if (currentCellIndex == null)
            {
                currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
            }
            else if (scrollingDown != goingDown)
            {
                scrollingDown = goingDown;
                if (goingDown)
                {
                    if (visibleSections.Count > 1)
                    {
                        var section = visibleSections.Last();
                        currentCellIndex = NSIndexPath.FromRowSection(section.VisibleCells.Count - 1, currentCellIndex.Section + visibleSections.Count - 1);
                    }
                    else
                    {
                        var section = visibleSections.FirstOrDefault();
                        if (section == null)
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
                        }
                        else
                        {
                            int row = Math.Min(Math.Max(currentCellIndex.Row, 0), sections[currentCellIndex.Section].Cells.Length - 1);
                            currentCellIndex = NSIndexPath.FromRowSection(Math.Max(row + section.VisibleCells.Count - 1, -1), currentCellIndex.Section);
                        }
                    }
                }
                else if (visibleSections.Count > 1)
                {
                    var section = visibleSections.First();
                    currentCellIndex = NSIndexPath.FromRowSection(section.Cells.Length - section.VisibleCells.Count, currentCellIndex.Section - (visibleSections.Count - 1));
                }
                else
                {
                    var section = visibleSections.FirstOrDefault();
                    if (section == null)
                    {
                        currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
                    }
                    else
                    {
                        int row = Math.Min(Math.Max(currentCellIndex.Row, 0), sections[currentCellIndex.Section].Cells.Length - 1);
                        currentCellIndex = NSIndexPath.FromRowSection(row - section.VisibleCells.Count + 1, currentCellIndex.Section);
                    }
                }
            }

            foreach (var section in visibleSections)
            {
                section.ClampHeader(this);
                section.ClampFooter(this);

                SetHeaderFooterDefaults(section.Header as UITableViewHeaderFooterView);
                SetHeaderFooterDefaults(section.Footer as UITableViewHeaderFooterView);
            }
            
            if (goingDown)
            {
                nfloat lastY = tableHeaderView == null ? 0 : tableHeaderView.Frame.Height;
                if (visibleSections.Count > 0)
                {
                    lastY = visibleSections.Last().LastVisibleY;
                    
                    var firstSection = visibleSections.First();
                    for (int i = 0; i < firstSection.VisibleCells.Count; i++)
                    {
                        var cell = firstSection.VisibleCells[i];
                        if (cell.Frame.Bottom < ContentOffset.Y)
                        {
                            OnCellEndDisplay(cell, firstSection);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    if (!firstSection.IsVisible)
                    {
                        visibleSections.Remove(firstSection);
                    }
                }
                
                if (lastY < ContentOffset.Y + Frame.Height)
                {
                    for (int sectionIndex = currentCellIndex.Section; sectionIndex < sections.Length; sectionIndex++)
                    {
                        var section = sections[sectionIndex];
                        if (visibleSections.LastOrDefault() != section)
                        {
                            visibleSections.Add(section);
                            if (sectionIndex > 0)
                            {
                                var previous = sections[sectionIndex - 1];
                                section.Y = previous.Y + previous.Height;
                            }
                            else
                            {
                                section.Y = tableHeaderView == null ? 0 : tableHeaderView.Frame.Height;
                            }
                        }
                        
                        if (section.Header == null)
                        {
                            GetHeaderAndFooter(sectionIndex);
                            lastY = NMath.Max(lastY, section.Header.Frame.Bottom);
                        }
                        
                        if (lastY > ContentOffset.Y + Frame.Height)
                        {
                            return;
                        }
                        
                        for (int rowIndex = currentCellIndex.Row + 1; rowIndex < section.Cells.Length; rowIndex++)
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(rowIndex, sectionIndex);
                            var cell = Source.GetCell(this, currentCellIndex);

                            float x = isRounded ? 10 : 0;
                            cell.Frame = new CGRect(x, section.LastCellY, Frame.Width - (x * 2), cell.Frame.Height);
                            lastY = cell.Frame.Bottom;

                            cell.LayoutSubviews();
                            SetSeparator(cell, rowIndex == section.Cells.Length - 1);
                            SetSelected(cell);

                            nfloat delta = cell.Frame.Height - section.Cells[rowIndex];
                            if (delta != 0)
                            {
                                ContentSize = new CGSize(ContentSize.Width, trueContentSize.Height + delta);
                                section.Height += delta;
                                section.Cells[rowIndex] = cell.Frame.Height;
                            }
                            
                            InsertSubview(cell, 0);
                            OnCellBeginDisplay(cell, currentCellIndex, false);
                            
                            if (lastY > ContentOffset.Y + Frame.Height)
                            {
                                return;
                            }
                        }
                        
                        if (sectionIndex < sections.Length - 1)
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(-1, sectionIndex + 1);
                        }
                        else
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(section.Cells.Length, sectionIndex);
                        }
                    }
                }
            }
            else
            {
                nfloat firstY = tableHeaderView == null ? 0 : tableHeaderView.Frame.Height;
                if (visibleSections.Count > 0)
                {
                    firstY = visibleSections.First().FirstVisibleY;
                    
                    var lastSection = visibleSections.Last();
                    for (int i = lastSection.VisibleCells.Count - 1; i >= 0; i++)
                    {
                        if (i >= lastSection.VisibleCells.Count)
                        {
                            break;
                        }
                        
                        var cell = lastSection.VisibleCells[i];
                        if (cell.Frame.Y > ContentOffset.Y + Frame.Height)
                        {
                            OnCellEndDisplay(cell, lastSection);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    if (!lastSection.IsVisible)
                    {
                        visibleSections.Remove(lastSection);
                    }
                }
                
                if (firstY > ContentOffset.Y)
                {
                    for (int sectionIndex = currentCellIndex.Section; sectionIndex >= 0; sectionIndex--)
                    {
                        var section = sections[sectionIndex];
                        if (section.Footer == null)
                        {
                            GetHeaderAndFooter(sectionIndex);
                            firstY = NMath.Min(firstY, section.Footer.Frame.Bottom);
                        }
                        
                        if (visibleSections.FirstOrDefault() != section)
                        {
                            visibleSections.Insert(0, section);
                        }
                        
                        if (firstY < ContentOffset.Y)
                        {
                            return;
                        }
                        
                        for (int rowIndex = currentCellIndex.Row - 1; rowIndex >= 0; rowIndex--)
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(rowIndex, sectionIndex);
                            var cell = Source.GetCell(this, currentCellIndex);
                            float x = isRounded ? 10 : 0;
                            cell.Frame = new CGRect(x, section.FirstCellY - cell.Frame.Height, Frame.Width - (x * 2), cell.Frame.Height);

                            cell.LayoutSubviews();
                            SetSeparator(cell, rowIndex == section.Cells.Length - 1);
                            SetSelected(cell);

                            nfloat delta = cell.Frame.Height - section.Cells[rowIndex];
                            cell.Frame = new CGRect(x, section.FirstCellY - cell.Frame.Height + delta, Frame.Width - (x * 2), cell.Frame.Height);
                            firstY = cell.Frame.Y;

                            if (delta != 0)
                            {
                                ContentSize = new CGSize(ContentSize.Width, trueContentSize.Height + delta);
                                ContentOffset = new CGPoint(ContentOffset.X, ContentOffset.Y + delta);
                                section.Height += delta;
                                section.Cells[rowIndex] = cell.Frame.Height;

                                foreach (var visibleCell in section.VisibleCells)
                                {
                                    var frame = visibleCell.Frame;
                                    frame.Y += delta;
                                    visibleCell.Frame = frame;
                                }

                                for (int visibleSectionIndex = 1; visibleSectionIndex < visibleSections.Count; visibleSectionIndex++)
                                {
                                    var visibleSection = visibleSections[visibleSectionIndex];
                                    visibleSection.Y += delta;

                                    foreach (var visibleCell in visibleSection.VisibleCells)
                                    {
                                        var frame = visibleCell.Frame;
                                        frame.Y += delta;
                                        visibleCell.Frame = frame;
                                    }
                                }
                            }
                            
                            InsertSubview(cell, 0);
                            OnCellBeginDisplay(cell, currentCellIndex, true);
                            
                            if (firstY < ContentOffset.Y)
                            {
                                return;
                            }
                        }
                        
                        if (sectionIndex > 0)
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(sections[sectionIndex - 1].Cells.Length, sectionIndex - 1);
                        }
                        else
                        {
                            currentCellIndex = NSIndexPath.FromRowSection(-1, 0);
                        }
                    }
                }
            }
        }
        
        private void OnCellBeginDisplay(UITableViewCell cell, NSIndexPath indexPath, bool insertAtTop)
        {
            offScreenCells.Remove(cell);
            if (insertAtTop)
            {
                sections[indexPath.Section].VisibleCells.Insert(0, cell);
            }
            else
            {
                sections[indexPath.Section].VisibleCells.Add(cell);
            }

            if (cell.BackgroundColor == null)
            {
                cell.BackgroundColor = defaultCellBackground;
            }

            if (isRounded)
            {
                var sectionCount = sections[indexPath.Section].Cells.Length;
                CAShapeLayer shape = null;
                if (indexPath.Row == 0 && sectionCount == 1)
                {
                    shape = new CAShapeLayer();
                    shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                        UIRectCorner.AllCorners, new CGSize(10, 10)).CGPath;
                }
                else if (indexPath.Row == 0)
                {
                    shape = new CAShapeLayer();
                    shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                        UIRectCorner.TopLeft | UIRectCorner.TopRight, new CGSize(10, 10)).CGPath;
                }
                else if (indexPath.Row == sectionCount - 1)
                {
                    shape = new CAShapeLayer();
                    shape.Path = UIBezierPath.FromRoundedRect(new CGRect(0, 0, cell.Bounds.Width, cell.Bounds.Height),
                        UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new CGSize(10, 10)).CGPath;
                }

                cell.Layer.Mask = shape;
                cell.Layer.MasksToBounds = true;
            }
        }
        
        private void OnCellEndDisplay(UITableViewCell cell, Section section)
        {
            cell.RemoveFromSuperview();
            offScreenCells.Add(cell);
            section.VisibleCells.Remove(cell);
            Source.CellDisplayingEnded(this, cell);
            
            // even if cells aren't being recycled, we should try to keep memory down
            if (offScreenCells.Count > 100)
            {
                offScreenCells.RemoveAt(0);
            }
        }
        
        private UITableViewCell GetCellAtPath(NSIndexPath path)
        {
            if (path.Section < 0 || path.Section >= sections.Length)
            {
                return null;
            }
            
            var section = sections[path.Section];
            if (section.VisibleCells.Count > 0)
            {
                if (visibleSections.Count == 1)
                {
                    try
                    {
                        int row = Math.Max(Math.Min(currentCellIndex.Row, section.Cells.Length - 1), 0);
                        if (scrollingDown)
                        {
                            return section.VisibleCells[path.Row - (row - section.VisibleCells.Count + 1)];
                        }
                        else
                        {
                            return section.VisibleCells[path.Row - row];
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return null;
                    }
                }
                
                var firstCell = section.VisibleCells.First();
                if (firstCell.Frame.Y > ContentOffset.Y && path.Row < section.VisibleCells.Count)
                {
                    return section.VisibleCells[path.Row];
                }
                
                var lastCell = section.VisibleCells.Last();
                if (lastCell.Frame.Bottom < ContentOffset.Y + Frame.Height &&
                    path.Row - (section.Cells.Length - section.VisibleCells.Count) < section.VisibleCells.Count)
                {
                    return section.VisibleCells[path.Row - (section.Cells.Length - section.VisibleCells.Count)];
                }
            }
            
            return null;
        }
        
        private void SetSeparator(UITableViewCell cell, bool atBottom)
        {
            var contentSuper = cell.ContentView.Superview;
            var separator = contentSuper.Subviews.FirstOrDefault(v => v is SeparatorView);
            if (separator == null)
            {
                separator = new SeparatorView();
                contentSuper.Add(separator);
            }
            
            var contentFrame = cell.ContentView.Frame;
            contentFrame.Height = contentSuper.Frame.Height - (1 / UIScreen.MainScreen.Scale);
            cell.ContentView.Frame = contentFrame;
            
            separator.BackgroundColor = SeparatorColor == null ? defaultSeparatorColor : SeparatorColor;
            
            if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && !(atBottom && Style == UITableViewStyle.Grouped))
            {
                separator.Frame = new CGRect(cell.SeparatorInset.Left,
                    cell.ContentView.Frame.Bottom + cell.SeparatorInset.Top,
                    cell.Frame.Width - cell.SeparatorInset.Left - cell.SeparatorInset.Right,
                    contentSuper.Frame.Height - cell.ContentView.Frame.Height - cell.SeparatorInset.Bottom - cell.SeparatorInset.Top);
            }
            else
            {
                separator.Frame = new CGRect(0, cell.ContentView.Frame.Bottom,
                    cell.Frame.Width, contentSuper.Frame.Height - cell.ContentView.Frame.Height);
            }
        }
        
        private void SetSeparator(UIView view)
        {
            if (isRounded)
            {
                return;
            }

            var separator = view.Subviews.FirstOrDefault(v => v is SeparatorView);
            if (separator == null)
            {
                separator = new SeparatorView();
                view.Add(separator);
            }

            nfloat height = (1 / UIScreen.MainScreen.Scale);
            separator.BackgroundColor = SeparatorColor == null ? defaultSeparatorColor : SeparatorColor;
            separator.Frame = new CGRect(0, view.Frame.Height - height, view.Frame.Width, height);
            
            var headerFooter = view as UITableViewHeaderFooterView;
            if (headerFooter != null)
            {
                var frame = headerFooter.ContentView.Frame;
                frame.Height -= height;
                headerFooter.ContentView.Frame = frame;
            }
        }
        
        private void SetSelected(UITableViewCell cell)
        {
            if (IndexPathForSelectedRow != null && currentCellIndex.Section == IndexPathForSelectedRow.Section &&
                currentCellIndex.Row == IndexPathForSelectedRow.Row)
            {
                cell.SetSelected(true, false);
            }
            else if (cell.Selected)
            {
                cell.SetSelected(false, false);
            }
        }
        
        private void GetHeaderAndFooter(int sectionIndex)
        {
            var section = sections[sectionIndex];
            if (section.Header == null)
            {
                section.Header = Source.GetViewForHeader(this, sectionIndex) ?? new UIView();
                var header = section.Header as UITableViewHeaderFooterView;
                
                var headerFrame = section.Header.Frame;
                headerFrame.Y = section.Y;
                headerFrame.Width = Frame.Width;
                headerFrame.Height = Source.GetHeightForHeader(this, sectionIndex);
                
                if (Style == UITableViewStyle.Grouped)
                {
                    if (headerFrame.Height == 0)
                    {
                        headerFrame.Height = SectionHeaderHeight + (sectionIndex == 0 ? SectionFooterHeight : 0);
                    }

                    section.Header.Frame = headerFrame;
                    SetSeparator(section.Header);

                    if (header != null)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            header.TextLabel.Frame = new CGRect(new CGPoint(15, 11.5f), header.TextLabel.Frame.Size);
                        }
                        else
                        {
                            header.TextLabel.Frame = new CGRect(new CGPoint(19, 7), header.TextLabel.Frame.Size);
                            header.TextLabel.ShadowColor = UIColor.White;
                        }
                    }
                }
                else
                {
                    section.Header.Frame = headerFrame;

                    if (header != null)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            header.TextLabel.Frame = new CGRect(new CGPoint(15, 2), header.TextLabel.Frame.Size);
                        }
                        else
                        {
                            header.TextLabel.Frame = new CGRect(new CGPoint(12, -1), header.TextLabel.Frame.Size);
                            header.TextLabel.ShadowColor = new UIColor(0, 0, 0, 0.44f);
                        }
                    }
                }

                section.ClampHeader(this);

                if (header != null)
                {
                    SetHeaderFooterDefaults(header);
                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0) && Style == UITableViewStyle.Grouped && header.TextLabel.Text != null)
                    {
                        header.TextLabel.Text = header.TextLabel.Text.ToUpper();
                        header.TextLabel.SizeToFit();
                    }
                }
            }
        
            if (section.Footer == null)
            {
                section.Footer = Source.GetViewForFooter(this, sectionIndex) ?? new UIView();
                var footer = section.Footer as UITableViewHeaderFooterView;

                var footerFrame = section.Footer.Frame;
                footerFrame.Width = Frame.Width;
                footerFrame.Height = Source.GetHeightForFooter(this, sectionIndex);
                
                if (Style == UITableViewStyle.Grouped)
                {
                    if (footerFrame.Height == 0)
                    {
                        footerFrame.Height = SectionFooterHeight + (sectionIndex == sections.Length - 1 ? SectionHeaderHeight : 0);
                    }

                    footerFrame.Y = section.Y + section.Height - footerFrame.Height;
                    section.Footer.Frame = footerFrame;

                    if (footer != null)
                    {
                        if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                        {
                            footer.TextLabel.Frame = new CGRect(new CGPoint(15, 6.5f), footer.TextLabel.Frame.Size);
                        }
                        else
                        {
                            footer.TextLabel.Frame = new CGRect(19, 7, Frame.Width - 38, footer.TextLabel.Frame.Height);
                            footer.TextLabel.ShadowColor = UIColor.White;
                            footer.TextLabel.TextAlignment = UITextAlignment.Center;
                            if (footer.TextLabel.Font.ToFont().Equals(iFactr.UI.Font.PreferredSectionFooterFont))
                            {
                                footer.TextLabel.Font = UIFont.SystemFontOfSize(15);
                            }
                        }
                    }
                }
                else if (footer != null)
                {
                    footerFrame.Y = section.Y + section.Height - footerFrame.Height;
                    section.Footer.Frame = footerFrame;

                    if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                    {
                        footer.TextLabel.Frame = new CGRect(new CGPoint(15, 2), footer.TextLabel.Frame.Size);
                    }
                    else
                    {
                        footer.TextLabel.Frame = new CGRect(new CGPoint(12, -1), footer.TextLabel.Frame.Size);
                        footer.TextLabel.ShadowColor = new UIColor(0, 0, 0, 0.44f);
                    }
                }

                section.ClampFooter(this);

                if (footer != null)
                {
                    SetHeaderFooterDefaults(footer);
                }
            }
        }

        private void SetHeaderFooterDefaults(UITableViewHeaderFooterView view)
        {
            if (view == null)
            {
                return;
            }

            if (view.BackgroundView == null)
            {
                view.BackgroundView = new UIView();
            }

            if (view.BackgroundView.BackgroundColor.IsDefaultColor() && BackgroundView.BackgroundColor == null)
            {
                view.BackgroundView.BackgroundColor = defaultHeaderFooterBackground;
            }

            if (view.TextLabel.TextColor.IsDefaultColor())
            {
                view.TextLabel.TextColor = defaultHeaderFooterForeground;
            }
        }

        private void RemoveNotifications()
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillShowNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UIKeyboard.WillHideNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextField.TextDidBeginEditingNotification, null);
            NSNotificationCenter.DefaultCenter.RemoveObserver(this, UITextView.TextDidBeginEditingNotification, null);
        }
        
        private class Section
        {
            public nfloat[] Cells;
            public List<UITableViewCell> VisibleCells;
            public UIView Header;
            public UIView Footer;
            public nfloat Y;
            public nfloat Height;
            
            public nfloat FirstVisibleY
            {
                get
                {
                    nfloat y = Y + Height;
                    if (VisibleCells.Count > 0)
                    {
                        y = VisibleCells.First().Frame.Y;
                    }
                    
                    if (Header != null && Header.Superview != null && Header.Frame.Y < y && Header.Frame.Y == Y)
                    {
                        y = Header.Frame.Y;
                    }
                    
                    if (Footer != null && Footer.Superview != null && Footer.Frame.Y < y && Footer.Frame.Bottom == Y + Height)
                    {
                        y = Footer.Frame.Y;
                    }
                    
                    return y;
                }
            }
            
            public nfloat LastVisibleY
            {
                get
                {
                    nfloat y = Y;
                    if (VisibleCells.Count > 0)
                    {
                        y = VisibleCells.Last().Frame.Bottom;
                    }
                    
                    if (Header != null && Header.Superview != null && Header.Frame.Bottom > y && Header.Frame.Y == Y)
                    {
                        y = Header.Frame.Bottom;
                    }
                    
                    if (Footer != null && Footer.Superview != null && Footer.Frame.Bottom > y && Footer.Frame.Bottom == Y + Height)
                    {
                        y = Footer.Frame.Bottom;
                    }
                    
                    return y;
                }
            }
            
            public nfloat FirstCellY
            {
                get
                {
                    if (VisibleCells.Count > 0)
                    {
                        return VisibleCells.First().Frame.Y;
                    }
                    
                    if (Footer != null)
                    {
                        return Y + Height - Footer.Frame.Height;
                    }
                    
                    return Y + Height;
                }
            }
            
            public nfloat LastCellY
            {
                get
                {
                    if (VisibleCells.Count > 0)
                    {
                        return VisibleCells.Last().Frame.Bottom;
                    }
                    
                    if (Header != null)
                    {
                        return Y + Header.Frame.Height;
                    }
                    
                    return Y;
                }
            }
            
            public bool IsVisible
            {
                get
                {
                    return VisibleCells.Count > 0 ||
                        (Header != null && Header.Superview != null) ||
                        (Footer != null && Footer.Superview != null);
                }
            }
            
            public Section(TableView tableView, int cellCount)
            {
                Cells = new nfloat[cellCount];
                for (int i = 0; i < cellCount; i++)
                {
                    Cells[i] = tableView.RowHeight;
                }

                VisibleCells = new List<UITableViewCell>();
            }
            
            public void ClampHeader(TableView tableView)
            {
                if (Header == null)
                {
                    return;
                }
                
                var frame = Header.Frame;
                if (tableView.Style == UITableViewStyle.Grouped)
                {
                    frame.Y = Y;
                }
                else
                {
                    frame.Y = NMath.Max(tableView.Top, Y);
                    if (Footer == null)
                    {
                        frame.Y = NMath.Min(frame.Y, Y + Height - frame.Height);
                    }
                    else
                    {
                        frame.Y = NMath.Min(frame.Y, Footer.Frame.Y - frame.Height);
                    }
                }

                frame.Width = tableView.Frame.Width;
                
                if (frame.Bottom < tableView.ContentOffset.Y || frame.Y > tableView.ContentOffset.Y + tableView.Frame.Height)
                {
                    Header.RemoveFromSuperview();
                }
                else
                {
                    if (Header.Superview == null)
                    {
                        tableView.InsertSubview(Header, tableView.Subviews.Count(v => v is UITableViewCell) + 1);
                    }
                    
                    Header.Frame = frame;
                }
            }
            
            public void ClampFooter(TableView tableView)
            {
                if (Footer == null)
                {
                    return;
                }
                
                var frame = Footer.Frame;
                if (tableView.Style == UITableViewStyle.Grouped)
                {
                    frame.Y = Y + Height - frame.Height;
                }
                else
                {
                    frame.Y = NMath.Min(tableView.Bottom - frame.Height, Y + Height - frame.Height);
                    if (Header == null)
                    {
                        frame.Y = NMath.Max(frame.Y, Y);
                    }
                    else
                    {
                        frame.Y = NMath.Max(frame.Y, Header.Frame.Bottom);
                    }
                }

                frame.Width = tableView.Frame.Width;
                
                if (frame.Bottom < tableView.ContentOffset.Y || frame.Y > tableView.ContentOffset.Y + tableView.Frame.Height)
                {
                    Footer.RemoveFromSuperview();
                }
                else
                {
                    if (Footer.Superview == null)
                    {
                        tableView.InsertSubview(Footer, tableView.Subviews.Count(v => v is UITableViewCell) + 1);
                    }
                    
                    Footer.Frame = frame;
                }
            }
        }
    }
    
    public class TableViewSource : UIScrollViewDelegate
    {
        public virtual int NumberOfSections(TableView tableView)
        {
            return 0;
        }
        
        public virtual int RowsInSection(TableView tableView, int section)
        {
            return 0;
        }
        
        public virtual UITableViewCell GetCell(TableView tableView, NSIndexPath indexPath)
        {
            return null;
        }
        
        public virtual void RowSelected(TableView tableView, NSIndexPath indexPath)
        {
        }
        
        public virtual void CellDisplayingEnded(TableView tableView, UITableViewCell cell)
        {
        }
        
        public virtual nfloat GetHeightForHeader(TableView tableView, int section)
        {
            return tableView.SectionHeaderHeight;
        }
        
        public virtual nfloat GetHeightForFooter(TableView tableView, int section)
        {
            return tableView.SectionFooterHeight;
        }
        
        public virtual UIView GetViewForHeader(TableView tableView, int section)
        {
            return null;
        }
        
        public virtual UIView GetViewForFooter(TableView tableView, int section)
        {
            return null;
        }
    }
    
    internal class SeparatorView : UIView
    {
    }
}

