namespace NLogWindowsPhoneApplication
{
    using System.Windows;
    using Microsoft.Phone.Controls;

    using NLog;

    public partial class MainPage : PhoneApplicationPage
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("Hello <> 'foo' \"bar\" !");
        }
    }
}