using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TimeHomogeneousChain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ushort countOfStates = 1;
        private ushort numberOfSteps = 1;
        private ushort startIndex;
        public ushort StartIndex
        {
            get => (ushort)(startIndex + 1);
            set
            {
                if (value < 1 || value > countOfStates)
                    MessageBox.Show("Будь ласка виберіть коректний номер початкового стану!");
                else
                    startIndex = (ushort)(value - 1);
            }
        }
        public ushort CountOfStates
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
        public ushort NumberOfSteps
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

        public bool EnableAutoFilling { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void GenerateTransitionTable(ushort countOfStates)
        {
            TransitionTable.Columns.Clear();

            List<State> stateList = new List<State>();
            for (int i = 0; i < countOfStates; i++)
            {
                stateList.Add(new State(i + 1, countOfStates));
                var col = new DataGridTextColumn() { Header = i + 1 };
                var binding = new Binding("Values[" + i + "]");
                col.Binding = binding;
                TransitionTable.Columns.Add(col);
            }

            TransitionTable.ItemsSource = stateList;
        }

        private void BtnCalculateStatesProbabilities_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task task = new Task(countOfStates, startIndex, NumberOfSteps, GetTable());
                task.Solve();
                MessageBox.Show(task.GetResult());
                //    {0.1m, 0.2m, 0.3m, 0.4m},
                //    { 0, 0.3m,0.2m,0.5m},
                //    {0,0,0.4m,0.6m},
                //    {0,0,0,1}                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private decimal[,] GetTable()
        {
            decimal[,] table = new decimal[countOfStates, countOfStates];
            for (int i = 0; i < TransitionTable.Items.Count; ++i)
            {
                decimal rowValue = 0;
                for (int j = 0; j < TransitionTable.Columns.Count; ++j)
                {
                    var cell = (TransitionTable.Items[i] as State).Values[j];
                    if (cell < 0 || cell > 1)
                        throw new ArgumentException($"Значення переходу із стану {i + 1} в стан {j + 1} має невірне значення!\nЗначення повинно бути від 0 до 1!");
                    rowValue += cell;
                    table[i, j] = cell;
                }

                if (rowValue != 1)
                {
                    if (rowValue < 1 && EnableAutoFilling)
                    {
                        table[i, i] = 1 - (rowValue - table[i, i]);
                    }
                    else if (rowValue > 1 && EnableAutoFilling)
                        throw new ArgumentException($"Для доповнення сума переходів із стану повинна бути менше 1!");
                    else
                        throw new ArithmeticException($"Сума ймовірностей переходу із стану №{i + 1} не дорівнює 1!\n Введено некоректні дані. Будь ласка спробуйте ще раз.\nЗверніть увагу, правильний формат запису - '0.25'");
                }
            }
            return table;
        }

        private void BtnClearTable_Click(object sender, RoutedEventArgs e)
        {
            GenerateTransitionTable(CountOfStates);
        }

        private void BtnRandomFillTable_Click(object sender, RoutedEventArgs e)
        {
            TransitionTable.ItemsSource = GenerateTransitionTableWithRandomValues();
        }

        private IEnumerable GenerateTransitionTableWithRandomValues()
        {
            IList<State> randomState = new List<State>();
            Random random = new Random();
            for (int i = 0; i < CountOfStates; ++i)
            {
                decimal[] values = new decimal[countOfStates];
                for (int j = 0; j < values.Length && values.Sum() != 1; ++j)
                {
                    values[j] = (decimal)random.Next(0, 100 - (int)(values.Sum() * 100)) / 100;
                }
                if (values.Sum() != 1)
                    values[i] = 1 - (values.Sum() - values[i]);
                randomState.Add(new State(i, values));
            }
            return randomState;
        }
    }

    internal class State
    {
        public int Index { get; set; }
        public decimal[] Values { get; set; }
        public State(int index, uint countOfStates)
        {
            this.Index = index;
            Values = new decimal[countOfStates];
        }
        public State(int index, decimal[] values)
        {
            this.Index = index;
            this.Values = values;
        }
    }
}
