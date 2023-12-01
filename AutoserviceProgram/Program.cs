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

        private List<Stack> _storages = new List<Stack>();
        private Queue<Client> _clients = new Queue<Client>();

        private Stack _selectedStorage = new Stack();

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
        }

        private void ServeClient()
        {
            Client client = _clients.Dequeue();
            ConsoleHelper.PrintColor(ConsoleColor.DarkYellow, $"A client approaches, needing to replace {client.Count} {client.TypeOfDetailToBeRepaired}. Choose the part number:");

            Stack selectedStorage = GetStorage();

            if (selectedStorage != null)
            {
                string answerYes = "yes";
                string answerNo = "no";

                int moneyEarned = CalculateMoneyEarned(client, selectedStorage);
                _serviceRevenue += moneyEarned;

                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, $"\nTotal revenue for the day: {moneyEarned} money.");

                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Can you replace it?");
                string userAnswer = Console.ReadLine();

                if (userAnswer == answerYes)
                {
                    _selectedStorage.ReplaceDetail();
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
                ApplyPenalty();
            }
        }

        private void GenerateDetailsFromStack(DetailsFactory stack)
        {
            List<Detail> details = stack.CreateDetails();
            Dictionary<DetailType, int> detailDictionary = stack.GetServiceToDictionary();

            foreach (Detail detail in details)
            {
                int count = detailDictionary.TryGetValue(detail.Type, out int value) ? value : 0;
                _storages.Add(new Stack(detail, count));
            }
        }

        private int CalculateMoneyEarned(Client client, Stack storage)
        {
            int repairCost = storage.Detail.Price * client.Count;

            return repairCost;
        }

        private Stack GetStorage()
        {
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Enter the part number:");
            string userChoice = Console.ReadLine();

            if (int.TryParse(userChoice, out int userNumber))
            {
                Stack selectedStock = _storages.ElementAtOrDefault(userNumber);

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

            for (int index = 0; index < _storages.Count; index++)
            {
                var storage = _storages[index];
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
    }

    enum DetailType
    {
        Wheel,
        AutoLight,
        Engine,
    }

    class Detail
    {
        public Detail(DetailType detailType, int count, int price)
        {
            Type = detailType;
            Price = price;
            Count = count;
        }

        public DetailType Type { get; private set; }
        public int Count { get; private set; }
        public int Price { get; private set; }
    }

    class DetailsFactory
    {
        private List<Detail> _details = new List<Detail>
        {
            new Detail(DetailType.Wheel,count: 3, price: 200),
            new Detail(DetailType.AutoLight,count: 1, price: 500),
            new Detail(DetailType.Engine,count: 2, price: 1000),
        };

        public List<Detail> CreateDetails() => _details.Select(details => new Detail(details.Type, details.Count, details.Price)).ToList();

        public Dictionary<DetailType, int> GetServiceToDictionary() => _details.ToDictionary(detail => detail.Type, detail => detail.Count);
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

        public void RemoveDetail()
        {
            if (DetailCount > 0)
                DetailCount--;
        }

        public void ReplaceDetail()
        {
            DetailCount++;
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