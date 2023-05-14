using Avalonia;
using Avalonia.Controls;
using SPTInstaller.Behaviors;
using System;
using System.Linq;

namespace SPTInstaller.CustomControls
{
    public class DistributedSpacePanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var children = Children;

            for (int i = 0; i < children.Count; i++)
            {
                // measure child objects so we can use their desired size in the arrange override
                var child = children[i];
                child.Measure(availableSize);
            }

            // we want to use all available space
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var children = Children;
            Rect rcChild = new Rect(finalSize);
            double previousChildSize = 0.0;

            // get child objects that don't want to span the entire control
            var nonSpanningChildren = children.Where(x => x.GetValue(SpanBehavior.SpanProperty) == false).ToList();

            // get the total height off all non-spanning child objects
            var totalChildHeight = nonSpanningChildren.Select(x => x.DesiredSize.Height).Sum();

            // remove the total child height from our controls final size and divide it by the total non-spanning child objects
            // except the last one, since it needs no space after it
            var spacing = (finalSize.Height - totalChildHeight) / (nonSpanningChildren.Count - 1);

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                var spanChild = child.GetValue(SpanBehavior.SpanProperty);

                if (spanChild)
                {
                    // move any spanning children to the top of the array to push them behind the other controls (visually)
                    children.Move(i, 0);

                    rcChild = rcChild.WithY(0)
                                     .WithX(0)
                                     .WithHeight(finalSize.Height)
                                     .WithWidth(finalSize.Width);



                    child.Arrange(rcChild);
                    continue;
                };

                rcChild = rcChild.WithY(rcChild.Y + previousChildSize);
                previousChildSize = child.DesiredSize.Height;
                rcChild = rcChild.WithHeight(previousChildSize)
                                 .WithWidth(Math.Max(finalSize.Width, child.DesiredSize.Width));

                previousChildSize += spacing;

                child.Arrange(rcChild);
            }

            return finalSize;
        }
    }
}
