using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Autoservice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DetailsFactory detailsFactory = new DetailsFactory();
            Service service = new Service(detailsFactory);

            service.Work();

            Console.ReadKey();
        }
    }

    class Service
    {
        private const string ServeCommand = "1";

        private List<Stack> _stacks = new List<Stack>();
        private Queue<Client> _clients = new Queue<Client>();

        private int _serviceRevenue = 100;
        private int _penalties = 0;

        public Service(DetailsFactory stack)
        {
            GenerateDetailsFromStack(stack);
            GenerateClients(5);
        }

        public void Work()
        {
            while (_clients.Count > 0)
            {
                DisplayStorage();

                ConsoleHelper.PrintColor(ConsoleColor.DarkRed, $"\nTotal revenue for the day: {_serviceRevenue} money.");
                ConsoleHelper.PrintColor(ConsoleColor.DarkRed, $"\nTotal revenue for the day: {_penalties} money.");

                string option = Console.ReadLine();

                switch (option)
                {
                    case ServeCommand:
                        ServeClient();
                        break;

                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }

                Console.ReadLine();
                Console.Clear();
            }

            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "All clients have been served. Press any key to exit.");
            Console.ReadKey();
        }

        private void ServeClient()
        {
            Client client = _clients.Dequeue();
            ConsoleHelper.PrintColor(ConsoleColor.DarkYellow, $"A client approaches, needing to replace {client.Count} {client.TypeOfDetailToBeRepaired}. Choose the part number:");

            Stack selectedStack = GetStorage();

            if (selectedStack != null)
            {
                if (selectedStack.DetailCount >= _clients.Count)
                {
                    string answerYes = "yes";
                    string answerNo = "no";

                    int moneyEarned = CalculateMoneyEarned(client, selectedStack);
                    _serviceRevenue += moneyEarned;

                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, $"\nTotal revenue for the day: {moneyEarned} money.");

                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Can you replace it?");
                    string userAnswer = Console.ReadLine();

                    if (userAnswer == answerYes)
                    {
                        selectedStack.RemoveDetail(_clients.Count);
                        ReplaceBrokenDetails(selectedStack, _clients.Count);
                        ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Replacement successful!");
                    }
                    else if (userAnswer == answerNo)
                    {
                        ApplyPenalty();
                    }
                    else
                    {
                        ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Unable to recognize the answer. The client left without waiting for an answer.");
                        ApplyPenalty();
                    }
                }
                else
                {
                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Not enough details in stock to fulfill the request.");
                    ApplyPenalty();
                }
            }
            else
            {
                ApplyPenalty();
            }
        }

        private void GenerateDetailsFromStack(DetailsFactory stack)
        {
            List<Detail> details = stack.CreateDetails();
            int minDetailCount = 10;
            int maxDetailCount = 20;

            foreach (Detail detail in details)
            {
                _stacks.Add(new Stack(detail, RandomGenerate.Next(minDetailCount, maxDetailCount)));
            }
        }

        private int CalculateMoneyEarned(Client client, Stack stack)
        {
            int repairCost = stack.Detail.Price * client.Count;

            return repairCost;
        }

        private Stack GetStorage()
        {
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Enter the part number:");
            string userChoice = Console.ReadLine();

            if (int.TryParse(userChoice, out int userNumber))
            {
                Stack selectedStock = _stacks.ElementAtOrDefault(userNumber);

                if (selectedStock != null)
                {
                    return selectedStock;
                }
            }

            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "You entered an incorrect number or it refers to a non-existent part.");
            return null;
        }

        private void ApplyPenalty()
        {
            int fine = 500;
            ConsoleHelper.PrintColor(ConsoleColor.DarkYellow, $"\nPenalty applied: {fine} money.");
            _penalties += fine;
        }

        private void DisplayStorage()
        {
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Press: 1 to start");
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "The stock contains:");

            for (int index = 0; index < _stacks.Count; index++)
            {
                var storage = _stacks[index];
                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, $"{index} - part {storage.Detail.Type}, quantity {storage.DetailCount}, {storage.Detail.Price}");
            }
        }

        private void GenerateClients(int clientCount)
        {
            Enumerable.Range(1, clientCount)
                .ToList()
                .ForEach(_ =>
                {
                    _clients.Enqueue(new Client());
                });
        }

        private void ReplaceBrokenDetails(Stack selectedStack, int requestedCount)
        {
            for (int i = 0; i < requestedCount; i++)
            {
                selectedStack.RemoveDetail(_clients.Count);
            }

            int newDetailsCount = requestedCount;
            while (newDetailsCount > 0)
            {
                DetailType typeToReplace = selectedStack.Detail.Type;
                Stack newStack = _stacks.FirstOrDefault(s => s.Detail.Type == typeToReplace);
                if (newStack != null)
                {
                    newStack.AddDetail(_clients.Count);
                    newDetailsCount--;
                }
                else
                {
                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Ran out of new details for replacement.");
                }
            }
        }
    }

    enum DetailType
    {
        Wheel,
        AutoLight,
        Engine,
    }

    class Detail
    {
        public Detail(DetailType detailType, int price)
        {
            Type = detailType;
            Price = price;
        }

        public DetailType Type { get; private set; }
        public int Price { get; private set; }
    }

    class DetailsFactory
    {
        private List<Detail> _details = new List<Detail>();

        public DetailsFactory()
        {
            _details.Add(new Detail(DetailType.Wheel, price: 200));
            _details.Add(new Detail(DetailType.AutoLight, price: 500));
            _details.Add(new Detail(DetailType.Engine, price: 1000));
        }

        public List<Detail> CreateDetails() => _details.Select(details => new Detail(details.Type, details.Price)).ToList();
    }

    class Stack
    {
        public Stack() { }

        public Stack(Detail detail, int count)
        {
            Detail = detail;
            DetailCount = count;
        }

        public Detail Detail { get; private set; }
        public int DetailCount { get; private set; }

        public void RemoveDetail(int detailCount)
        {
            DetailCount -= detailCount;
        }

        public void AddDetail(int detailCount)
        {
            DetailCount += detailCount;
        }
    }

    class Client
    {
        public Client()
        {
            TypeOfDetailToBeRepaired = GenerateDetailToMend();
            Count = GenerateDetailCount();
        }

        public int Count { get; private set; }
        public DetailType TypeOfDetailToBeRepaired { get; private set; }

        private DetailType GenerateDetailToMend()
        {
            int enumCount = Enum.GetNames(typeof(DetailType)).Length;
            return (DetailType)RandomGenerate.Next(0, enumCount);
        }

        private int GenerateDetailCount()
        {
            int enumCount = Enum.GetNames(typeof(DetailType)).Length;
            return RandomGenerate.Next(1, enumCount);
        }
    }

    static class RandomGenerate
    {
        private static Random s_random = new Random();

        public static int Next(int minimum, int maximum) => s_random.Next(minimum, maximum);
    }

    static class ConsoleHelper
    {
        public static void PrintColor(ConsoleColor color, string message)
        {
            ConsoleColor detault = Console.ForegroundColor;
            Console.ForegroundColor = color;

            Console.WriteLine(message);

            Console.ForegroundColor = detault;
        }
    }
}