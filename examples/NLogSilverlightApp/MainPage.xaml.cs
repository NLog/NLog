namespace NLogSilverlightApp
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using NLog;

    public partial class MainPage : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Button clicked.");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Form Loaded...");
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            logger.Info("Key down: {0}", e.Key);
        }
    }
}
