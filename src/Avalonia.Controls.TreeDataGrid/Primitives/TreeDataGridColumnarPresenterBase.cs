using System;
using System.Collections.Generic;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    /// <summary>
    /// Base class for presenters which display data in virtualized columns.
    /// </summary>
    /// <typeparam name="TItem">The item type.</typeparam>
    /// <remarks>
    /// Implements common layout functionality between <see cref="TreeDataGridCellsPresenter"/>
    /// and <see cref="TreeDataGridColumnHeadersPresenter"/>.
    /// </remarks>
    public abstract class TreeDataGridColumnarPresenterBase<TItem> : TreeDataGridPresenterBase<TItem> where TItem : ITreeDataGridPresentable
    {
        protected IColumns? Columns => Items as IColumns;

        protected sealed override Size GetInitialConstraint(Control element, int index, Size availableSize)
        {
            var column = (IUpdateColumnLayout)Columns![index];
            return new Size(Math.Min(availableSize.Width, column.MaxActualWidth), availableSize.Height);
        }

        protected override (int index, double position) GetOrEstimateAnchorElementForViewport(
            double viewportStart,
            double viewportEnd,
            int itemCount)
        {
            if (Columns?.GetColumnAt(viewportStart) is var (index, position) && index >= 0)
                return (index, position);
            return base.GetOrEstimateAnchorElementForViewport(viewportStart, viewportEnd, itemCount);
        }

        protected sealed override bool NeedsFinalMeasurePass(int firstIndex, IReadOnlyList<Control?> elements)
        {
            var columns = Columns!;

            columns.CommitActualWidths();

            // We need to do a second measure pass if any of the controls were measured with a width
            // that is greater than the final column width.
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                if (e is not null)
                {
                    var previous = LayoutInformation.GetPreviousMeasureConstraint(e)!.Value;
                    if (previous.Width > columns[i + firstIndex].ActualWidth)
                        return true;
                }
            }

            return false;
        }

        protected sealed override (int index, double position) GetElementAt(double position)
        {
            return ((IColumns)Items!).GetColumnAt(position);
        }

        protected sealed override Size GetFinalConstraint(Control element, int index, Size availableSize)
        {
            var column = Columns![index];
            return new(column.ActualWidth, double.PositiveInfinity);
        }

        protected sealed override double CalculateSizeU(Size availableSize)
        {
            return Columns?.GetEstimatedWidth(availableSize.Width) ?? 0;
        }

        protected override (int anchorIndex, double anchorU) GetAnchorElementForDisjunctScroll(double viewportStart, double viewportEnd, int itemCount)
        {
            double position = 0;
            double totalSize = 0;
            int anchorIndex = 0;
            double anchorU = 0;
            for (var thisIndex = 0; thisIndex<itemCount; thisIndex++)
            {
                totalSize += Items![thisIndex].GetSizeU();
                anchorIndex = thisIndex;
                anchorU = position;
                if (totalSize >= viewportStart)
                {
                    break;
                }
                position = totalSize;
            }

            return (anchorIndex, anchorU);
        }
    }
}
