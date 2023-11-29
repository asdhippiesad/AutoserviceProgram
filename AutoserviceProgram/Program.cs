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
            Stack stack = new Stack();
            Service service = new Service(stack);

            service.Work();

            Console.ReadKey();
        }
    }

    class Service
    {
        private const string ServeCommand = "1";

        private List<Storage> _storages = new List<Storage>();
        private Queue<Client> _clients = new Queue<Client>();

        private Storage _selectedStorage = new Storage();

        private int _serviceRevenue = 100;
        private int _penalties = 0;

        public Service(Stack stack)
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

            Storage selectedStorage = GetStorage();

            if (selectedStorage != null)
            {
                int moneyEarned = CalculateMoneyEarned(client, selectedStorage);
                _serviceRevenue += moneyEarned;

                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, $"\nTotal revenue for the day: {moneyEarned} money.");

                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Can you replace it?");
                string userAnswer = Console.ReadLine();

                if (userAnswer == "yes")
                {
                    _selectedStorage.ReplaceDetail();
                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Replacement successful!");
                }
                else if (userAnswer == "no")
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

        private void GenerateDetailsFromStack(Stack stack)
        {
            List<Detail> details = stack.CreateDetail();
            Dictionary<DetailType, int> detailDictionary = stack.GetServiceToDictionary();

            foreach (Detail detail in details)
            {
                int count = detailDictionary.TryGetValue(detail.Type, out int value) ? value : 0;
                _storages.Add(new Storage(detail, count));
            }
        }

        private int CalculateMoneyEarned(Client client, Storage storage)
        {
            int repairCost = storage.Detail.Price * client.Count;

            return repairCost;
        }

        private Storage GetStorage()
        {
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Enter the part number:");
            string userChoice = Console.ReadLine();

            if (int.TryParse(userChoice, out int userNumber))
            {
                Storage selectedStock = _storages.ElementAtOrDefault(userNumber);

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
            Count = count;
            Price = price;
        }

        public DetailType Type { get; private set; }
        public int Count { get; private set; }
        public int Price { get; private set; }
    }

    class Stack
    {
        List<Detail> _details = new List<Detail>
        {
            new Detail(DetailType.Wheel,count: 3, price: 200),
            new Detail(DetailType.AutoLight,count: 1, price: 500),
            new Detail(DetailType.Engine,count: 2, price: 1000),
        };

        public List<Detail> CreateDetail() => _details.Select(details => new Detail(details.Type, details.Count, details.Price)).ToList();

        public Dictionary<DetailType, int> GetServiceToDictionary() => _details.ToDictionary(detail => detail.Type, detail => detail.Count);
    }

    class Storage
    {
        public Storage() { }

        public Storage(Detail detail, int count)
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
        public int Count { get; private set; }
        public DetailType TypeOfDetailToBeRepaired { get; private set; }

        public Client()
        {
            TypeOfDetailToBeRepaired = GenerateDetailToMend();
            Count = GenerateDetailCount();
        }

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