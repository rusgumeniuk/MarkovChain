using System.Windows;

namespace TimeHomogeneousChain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private uint countOfStates = 4;
        private uint numberOfSteps = 3;
        public uint CountOfStates
        {
            get => countOfStates;
            set
            {
                if (value > 0)
                {
                    countOfStates = value;
                    GenerateTransitionTable(value);
                }
                else
                    MessageBox.Show("Будь ласка введіть кількість станів системи.\nЧисло повинно бути бути більше нуля!");
            }
        }
        public uint NumberOfSteps
        {
            get => numberOfSteps;
            set
            {
                if (value > 0)
                    numberOfSteps = value;
                else
                    MessageBox.Show("Будь ласка введіть кількість кроків системи.\nЧисло повинно бути більше нуля!");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void GenerateTransitionTable(uint countOfStates)
        {

        }

        private void BtnCalculateStatesProbabilities_Click(object sender, RoutedEventArgs e)
        {
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
