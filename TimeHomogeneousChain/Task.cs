using QueueingTheoryLibrary;

using System;
using System.Linq;
using System.Text;

namespace TimeHomogeneousChain
{
    internal class Task : ITask
    {
        private readonly ushort IndexOfStartState;
        private readonly ushort CountOfStates;
        private readonly ushort NumberOfSteps;
        private readonly decimal[,] ProbabilityOfTransition;
        private readonly decimal[,] ProbabilityOfStates;
        public bool IsSolved { get; set; }

        public Task(ushort countOfStates, ushort indexOfStartState, ushort countOfSteps, decimal[,] probabilitiesOfTransition)
        {
            CountOfStates = countOfStates;
            IndexOfStartState = indexOfStartState;
            NumberOfSteps = countOfSteps;
            ProbabilityOfTransition = probabilitiesOfTransition;
            ProbabilityOfStates = new decimal[NumberOfSteps, CountOfStates];
        }

        public string GetResult()
        {
            StringBuilder resultText = new StringBuilder();
            resultText.AppendLine("Ймовірності перебування системи в станах:");
            for (int i = 0; i < CountOfStates; ++i)
            {
                resultText.AppendLine($"Стан №{i + 1}: {ProbabilityOfStates[NumberOfSteps - 1, i]}");
            }
            return resultText.ToString();
        }

        public void Solve()
        {
            for (int indexOfStep = 0; indexOfStep < NumberOfSteps; ++indexOfStep)
            {
                decimal[] row = CalculateProbabilitiesOfStates(indexOfStep);
                for (int indexOfState = 0; indexOfState < CountOfStates; ++indexOfState)
                {
                    ProbabilityOfStates[indexOfStep, indexOfState] = row[indexOfState];
                }
            }
        }

        private decimal[] CalculateProbabilitiesOfStates(int indexOfStep)
        {
            decimal[] array = new decimal[CountOfStates];
            if (indexOfStep == 0)
            {
                for (int i = 0; i < CountOfStates; ++i)
                {
                    array[i] = ProbabilityOfTransition[IndexOfStartState, i];
                }
            }
            else
            {
                for (int indexOfCurrentState = 0; indexOfCurrentState < CountOfStates; ++indexOfCurrentState)
                {
                    for (int indexOfPreviousState = 0; indexOfPreviousState < CountOfStates; indexOfPreviousState++)
                    {
                        array[indexOfCurrentState] += ProbabilityOfStates[indexOfStep - 1, indexOfPreviousState] * ProbabilityOfTransition[indexOfPreviousState, indexOfCurrentState];
                    }
                }
            }
            return array.Sum() == 1 ? array : throw new ArithmeticException($"Сума імовірностей станів на кроці {indexOfStep} не дорівнює 1!");
        }
    }
}
