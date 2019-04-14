﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TimeInhomogeneousChain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ushort countOfStates = 1;
        private ushort numberOfSteps = 1;
        private ushort startIndex;
        private bool isTableProgrammableChanged;
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
                }
                else
                    MessageBox.Show("Будь ласка введіть кількість станів системи.\nЧисло повинно бути бути більше нуля!");
            }
        }

        private void AddTablesToGrid()
        {
            TransitionTables.Clear();
            for (int i = 0; i < NumberOfSteps; ++i)
            {
                TransitionTables.Add(new Label() { Content = $"Таблиця переходів на кроці №{i + 1}" });
                TransitionTables.Add(GenerateTransitionTables(i));
            }
        }

        public ushort NumberOfSteps
        {
            get => numberOfSteps;
            set
            {
                if (value > 0)
                {
                    numberOfSteps = value;
                    AddTablesToGrid();
                }
                else
                    MessageBox.Show("Будь ласка введіть кількість кроків системи.\nЧисло повинно бути більше нуля!");
            }
        }

        public bool EnableAutoFilling { get; set; }

        public ObservableCollection<UIElement> TransitionTables { get; set; } = new ObservableCollection<UIElement>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private DataGrid GenerateTransitionTables(int index)
        {
            DataGrid dataGrid = new DataGrid()
            {
                AutoGenerateColumns = false,
                CanUserDeleteRows = false,
                CanUserAddRows = false,
                Name = $"DataGrid{index + 1}"
            };

            List<State> stateList = new List<State>();
            for (int i = 0; i < CountOfStates; i++)
            {
                stateList.Add(new State(i + 1, countOfStates));
                var col = new DataGridTextColumn() { Header = i + 1 };
                var binding = new Binding($"Values[{i}]");
                col.Binding = binding;
                dataGrid.Columns.Add(col);
            }
            
            dataGrid.ItemsSource = index != 0 ? stateList : new List<State>() { stateList[startIndex] };
            
            return dataGrid;
        }

        private void BtnCalculateStatesProbabilities_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal[,,] parsedTables = ParseDataGridAndGetTable();
                if (isTableProgrammableChanged)
                {
                    UpdateTransitionTable(parsedTables);
                }

                Task task = new Task(countOfStates, startIndex, NumberOfSteps, parsedTables);
                task.Solve();
                TextBlockResult.Text = task.GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTransitionTable(decimal[,,] updatedTable)
        {
            for (int indexOfStep = 0, tableIndex = 1; tableIndex < TransitionTables.Count; ++indexOfStep, tableIndex += 2)
            {
                var dataGrid = TransitionTables[tableIndex] as DataGrid;
                IList<State> updatedStates = new List<State>();
                for (int i = 0; i < countOfStates; ++i)
                {
                    var rowOfTable = new decimal[countOfStates];
                    for (int j = 0; j < countOfStates; j++)
                    {
                        rowOfTable[j] = updatedTable[i, j, indexOfStep];
                    }
                    updatedStates.Add(new State(i + 1, rowOfTable));
                }
                dataGrid.ItemsSource = updatedStates;
            }
        }

        private decimal[,,] ParseDataGridAndGetTable()
        {
            decimal[,,] table = new decimal[countOfStates, countOfStates, numberOfSteps];
            for (int indexOfStep = 0, tablesIndex = 1; tablesIndex < TransitionTables.Count; tablesIndex += 2, indexOfStep++)
            {
                var dataGrid = TransitionTables[tablesIndex] as DataGrid;
                for (int i = 0; i < dataGrid.Items.Count; ++i)
                {
                    decimal sumOfRow = 0;
                    for (int j = 0; j < dataGrid.Columns.Count; ++j)
                    {
                        var cell = (dataGrid.Items[i] as State).Values[j];
                        if (cell < 0 || cell > 1)
                            throw new ArgumentException($"Значення переходу із стану {i + 1} в стан {j + 1} має невірне значення!\nЗначення повинно бути від 0 до 1!");
                        sumOfRow += cell;
                        table[i, j, indexOfStep] = cell;
                    }

                    if (sumOfRow != 1)
                    {
                        if (sumOfRow < 1 && EnableAutoFilling)
                        {
                            table[i, i, indexOfStep] = 1 - (sumOfRow - table[i, i, indexOfStep]);
                            isTableProgrammableChanged = true;
                        }
                        else if (sumOfRow > 1 && EnableAutoFilling)
                            throw new ArgumentException($"Для доповнення сума переходів із стану повинна бути менше 1!");
                        else
                            throw new ArithmeticException($"Сума ймовірностей переходу із стану №{i + 1} не дорівнює 1!\n Введено некоректні дані. Будь ласка спробуйте ще раз.\nЗверніть увагу, правильний формат запису - '0.25'");
                    }
                }
            }
            return table;
        }

        private void BtnClearTable_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i + 1 < TransitionTables.Count; i += 2)
            {
                TransitionTables[i + 1] = GenerateTransitionTables(i / 2);
            }
        }

        private void BtnRandomFillTable_Click(object sender, RoutedEventArgs e)
        {
            GenerateTransitionTableWithRandomValues(CountOfStates);
        }

        private void GenerateTransitionTableWithRandomValues(int n)
        {
            for (int tableIndex = 1; tableIndex < TransitionTables.Count; tableIndex += 2)
            {
                var dataGrid = TransitionTables[tableIndex] as DataGrid;
                IList<State> randomState = new List<State>();
                Random random = new Random();
                for (int i = 0; i < n; ++i)
                {
                    decimal[] values = new decimal[n];
                    for (int j = 0; j < values.Length && values.Sum() != 1; ++j)
                    {
                        values[j] = (decimal)random.Next(0, 100 - (int)(values.Sum() * 100)) / 100;
                    }
                    if (values.Sum() != 1)
                        values[i] = 1 - (values.Sum() - values[i]);
                    randomState.Add(new State(i + 1, values));
                }
                dataGrid.ItemsSource = randomState;
            }
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
