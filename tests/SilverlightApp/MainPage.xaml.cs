using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApp
{
    using NLog;

    public partial class MainPage : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainPage()
        {
            InitializeComponent();
            AutoLog(this);
        }

        private void AutoLog(UIElement uiElement)
        {
            HookUpEvents(uiElement);

            int count = VisualTreeHelper.GetChildrenCount(uiElement);
            for (int i = 0; i < count; ++i)
            {
                var child = VisualTreeHelper.GetChild(uiElement, i) as UIElement;
                if (child != null)
                {
                    this.AutoLog(child);
                }
            }
        }

        private void HookUpEvents(UIElement uiElement)
        {
            // VisualTreeHelper.GetParent(uiElement) as UIElement;

            uiElement.MouseEnter += (sender, o) => { logger.Trace("OnMouseEnter {0}", uiElement.GetValue(NameProperty)); };
            uiElement.MouseLeave += (sender, o) => { logger.Trace("OnMouseLeave {0}", uiElement.GetValue(NameProperty)); };
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Hello world!");
        }
    }
}
