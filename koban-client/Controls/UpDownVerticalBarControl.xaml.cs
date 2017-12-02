using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Koban.Controls
{
    public sealed partial class UpDownVerticalBarControl : UserControl
    {
        static SolidColorBrush BackgroundColor = new SolidColorBrush(Colors.Transparent);

        public UpDownVerticalBarControl()
        {
            this.InitializeComponent();
        }

        public void DrawDataPoint(double value, Brush barColor, Image toolTip = null)
        {
            if (value > 0)
            {
                value *= 0.5;
                topRowDefinition.Height = new GridLength(0.5 - value, GridUnitType.Star);
                upBarRowDefinition.Height = new GridLength(value, GridUnitType.Star);
                downBarRowDefinition.Height = new GridLength(0);
                bottomRowDefinition.Height = new GridLength(0.5, GridUnitType.Star);
            }
            else
            {
                value *= -0.5;
                topRowDefinition.Height = new GridLength(0.5, GridUnitType.Star);
                upBarRowDefinition.Height = new GridLength(0);
                downBarRowDefinition.Height = new GridLength(value, GridUnitType.Star);
                bottomRowDefinition.Height = new GridLength(0.5 - value, GridUnitType.Star);
            }

            Border slice = new Border { Background = BackgroundColor };
            Grid.SetRow(slice, 0);
            graph.Children.Add(slice);

            slice = new Border { Background = barColor };
            Grid.SetRow(slice, 1);
            graph.Children.Add(slice);

            slice = new Border { Background = barColor };
            Grid.SetRow(slice, 2);
            graph.Children.Add(slice);

            slice = new Border { Background = BackgroundColor };
            Grid.SetRow(slice, 3);
            graph.Children.Add(slice);

            AddFlyoutToElement(graph, toolTip);
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
