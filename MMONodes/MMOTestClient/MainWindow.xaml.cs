namespace MMOTestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        ClientManager manager;

        public MainWindow()
        {
            InitializeComponent();
            SetProgress(50);
            manager = new ClientManager(this);
            manager.Start();
        }

        public void Log(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                logbox.Text += "\n" + text;
            });
        }

        public void SetStatus(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.statusLabel.Content = text;
            });
        }
        public void SetProgress(int percent)
        {
            this.Dispatcher.Invoke(() =>
                                       {
                                           this.statusProgress.Value = percent;
                                       });

        }

        public void SetLaunchReady()
        {
            this.Dispatcher.Invoke(() =>
                                       { });
        }

    }
}
