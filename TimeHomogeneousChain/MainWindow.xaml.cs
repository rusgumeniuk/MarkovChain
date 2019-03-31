using System.Windows;

namespace TimeHomogeneousChain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Task task = new Task(4, 0, 3, new decimal[,]
            {
                {0.1m, 0.2m, 0.3m, 0.4m},
                { 0, 0.3m,0.2m,0.5m},
                {0,0,0.4m,0.6m},
                {0,0,0,1}
            });
            task.Solve();
            MessageBox.Show(task.GetResult());
        }
    }
}
