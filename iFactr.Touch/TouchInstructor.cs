using System;
using System.Linq;

using CoreGraphics;
using UIKit;

using iFactr.UI;
using iFactr.UI.Controls;
using iFactr.UI.Instructions;

using Size = iFactr.UI.Size;

namespace iFactr.Touch
{
    public class TouchInstructor : UniversalInstructor
    {
        protected override void OnLayout(ILayoutInstruction element)
        {
            var headeredCell = element as HeaderedControlCell;
            if (headeredCell != null)
            {
                OnLayoutHeaderedCell(headeredCell);
                return;
            }

            var contentCell = element as ContentCell;
            if (contentCell != null)
            {
                base.OnLayout(element);
                OnLayoutContentCell(contentCell);
                return;
            }

            base.OnLayout(element);
        }

        // if controls.count == 1 then set beside
        // if controls.count < 3 && all are picker, image, selectlist, or switch then set beside
        private void OnLayoutHeaderedCell(HeaderedControlCell cell)
        {
            var grid = ((IPairable)cell).Pair as IGridBase;
            if (grid == null)
                return;

            grid.Columns.Clear();
            grid.Rows.Clear();

            var controls = grid.Children.Where(c => c != cell.Header).ToList();

            grid.Columns.Add(Column.OneStar);
            grid.Rows.Add(Row.AutoSized);

            cell.Header.Font = Font.PreferredHeaderFont.Size > 0 ? Font.PreferredHeaderFont : Font.PreferredLabelFont;
            cell.Header.Lines = 1;
            cell.Header.VerticalAlignment = VerticalAlignment.Center;
            cell.Header.HorizontalAlignment = HorizontalAlignment.Left;
            cell.Header.RowIndex = 0;
            cell.Header.ColumnIndex = 0;

            int firstLabel = controls.FindIndex(c => c is ILabel);
            if (firstLabel < 0)
            {
                firstLabel = controls.Count;
            }

            var first = controls.FirstOrDefault();
            if (firstLabel == 1 && !(first is ITextArea))
            {
                first.Margin = new Thickness((first is ISlider) ? Thickness.LeftMargin : Thickness.LargeHorizontalSpacing, 0, 0, 0);

                if (first is ITextBox || first is IPasswordBox || first is ISlider)
                {
                    grid.Columns[0] = Column.AutoSized;
                    grid.Columns.Insert(1, Column.OneStar);
                }
                else
                {
                    grid.Columns.Insert(1, Column.AutoSized);
                }

                first.VerticalAlignment = VerticalAlignment.Center;
                first.RowIndex = 0;
                first.ColumnIndex = 1;
            }
            else if (firstLabel == 2 && !(controls.Any(c => c is ITextBox || c is IPasswordBox || c is ITextArea || c is ISlider)))
            {
                grid.Columns.Insert(1, Column.AutoSized);
                grid.Columns.Insert(2, Column.AutoSized);

                first.Margin = new Thickness(Thickness.LargeHorizontalSpacing, 0, Thickness.SmallHorizontalSpacing, 0);
                first.VerticalAlignment = VerticalAlignment.Center;
                first.RowIndex = 0;
                first.ColumnIndex = 1;

                var second = controls[1];
                second.VerticalAlignment = VerticalAlignment.Center;
                second.RowIndex = 0;
                second.ColumnIndex = 2;
            }
            else
            {
                for (int i = 0; i < firstLabel; i++)
                {
                    var control = controls[i];
                    if (control is ITextArea)
                    {
                        grid.Rows.Add(Row.OneStar);
                        control.VerticalAlignment = VerticalAlignment.Stretch;
                    }
                    else
                    {
                        grid.Rows.Add(Row.AutoSized);
                        control.VerticalAlignment = VerticalAlignment.Center;
                    }

                    control.Margin = new Thickness(0, Thickness.SmallVerticalSpacing, 0, 0);
                    control.RowIndex = grid.Rows.Count - 1;
                    control.ColumnIndex = 0;
                }
            }

            for (int i = firstLabel; i < controls.Count; i++)
            {
                grid.Rows.Add(Row.AutoSized);

                var control = controls[i];
                control.Margin = new Thickness(0, Thickness.SmallVerticalSpacing, 0, 0);
                control.VerticalAlignment = VerticalAlignment.Center;
                control.RowIndex = grid.Rows.Count - 1;
                control.ColumnIndex = 0;
                control.ColumnSpan = grid.Columns.Count;
            }
        }

        private void OnLayoutContentCell(ContentCell cell)
        {
            if (!string.IsNullOrEmpty(cell.SubtextLabel.Text) && cell.MinHeight == Cell.StandardCellHeight)
            {
                cell.MinHeight = Cell.StandardCellHeight + Thickness.TopMargin + Thickness.BottomMargin;
            }
            else if (string.IsNullOrEmpty(cell.SubtextLabel.Text) && cell.MinHeight == (Cell.StandardCellHeight + Thickness.TopMargin + Thickness.BottomMargin))
            {
                cell.MinHeight = Cell.StandardCellHeight;
            }

            var tableCell = ((IPairable)cell).Pair as UITableViewCell;
            if (tableCell == null || !UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
            {
                return;
            }

            if (cell.Image.Dimensions == Size.Empty)
            {
                tableCell.SeparatorInset = new UIEdgeInsets(0, 15, 0, 0);
            }
            else
            {
                var margin = cell.Image.Margin;
                if (margin.Right < Thickness.LeftMargin)
                {
                    margin.Right = Thickness.LeftMargin;
                    cell.Image.Margin = margin;
                }

                double height = (double.IsInfinity(cell.MaxHeight) ? double.MaxValue : cell.MaxHeight) -
                    (cell.Padding.Top + cell.Padding.Bottom) - (margin.Top + margin.Bottom);

                float width = (float)cell.Image.Measure(new Size(tableCell.ContentView.Frame.Width - (cell.Padding.Left + cell.Padding.Right) - (margin.Left + margin.Right), height)).Width;
                tableCell.SeparatorInset = new UIEdgeInsets(0, (float)(cell.Padding.Left + margin.Left + margin.Right) + width, 0, 0);

                var tableView = tableCell.GetSuperview<TableView>();
                if (tableView == null)
                {
                    return;
                }

                var indexPath = tableView.IndexPathForCell(tableCell);
                if (tableView.Style == UITableViewStyle.Grouped && tableView.Source.RowsInSection(tableView, indexPath.Section) == indexPath.Row + 1)
                {
                    return;
                }

                var contentSuper = tableCell.ContentView.Superview;
                var separator = contentSuper.Subviews.FirstOrDefault(v => v is SeparatorView);
                if (separator != null)
                {
                    var contentFrame = tableCell.ContentView.Frame;
                    contentFrame.Height = contentSuper.Frame.Height - (1 / UIScreen.MainScreen.Scale);
                    tableCell.ContentView.Frame = contentFrame;

                    separator.Frame = new CGRect(tableCell.SeparatorInset.Left,
                        tableCell.ContentView.Frame.Bottom + tableCell.SeparatorInset.Top,
                        tableCell.Frame.Width - tableCell.SeparatorInset.Left - tableCell.SeparatorInset.Right,
                        contentSuper.Frame.Height - tableCell.ContentView.Frame.Height - tableCell.SeparatorInset.Bottom - tableCell.SeparatorInset.Top);
                }
            }
        }
    }
}

