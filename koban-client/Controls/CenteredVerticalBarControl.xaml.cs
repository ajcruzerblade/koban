using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Koban.Controls
{
    public sealed partial class CenteredVerticalBarControl : UserControl
    {
        static SolidColorBrush BackgroundColor = new SolidColorBrush(Colors.Transparent);

        public CenteredVerticalBarControl()
        {
            this.InitializeComponent();
        }

        public void DrawDataPoint(double value, Brush barColor = null, Image toolTip = null)
        {
            topRowDefinition.Height = new GridLength((1 - value) / 2, GridUnitType.Star);
            centerRowDefinition.Height = new GridLength(value, GridUnitType.Star);
            bottomRowDefinition.Height = new GridLength((1 - value) / 2, GridUnitType.Star);

            Border slice = new Border { Background = BackgroundColor };
            Grid.SetRow(slice, 0);
            graph.Children.Add(slice);

            slice = new Border { Background = barColor };
            Grid.SetRow(slice, 1);
            graph.Children.Add(slice);

            slice = new Border { Background = BackgroundColor };
            Grid.SetRow(slice, 2);
            graph.Children.Add(slice);

            this.AddFlyoutToElement(graph, toolTip);
        }

        private void AddFlyoutToElement(FrameworkElement element, Image toolTip)
        {
            if (toolTip != null)
            {
                FlyoutBase.SetAttachedFlyout(element, new Flyout { Content = toolTip });

                element.PointerReleased += (s, e) =>
                {
                    FlyoutBase.ShowAttachedFlyout(element);
                };
            }
        }
    }
}
