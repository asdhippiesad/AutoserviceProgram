using System;
using System.Collections.Generic;
using System.Linq;

namespace Autoservice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Service service = new Service();

            service.Work();

            Console.ReadKey();
        }
    }

    class Service
    {
        private const string ServeCommand = "1";

        private List<Storage> _storages = new List<Storage>();
        private Queue<Client> _clients = new Queue<Client>();
        private Random _random = new Random();
        private int _serviceRevenue = 0;
        private int _penalties = 0;

        public Service()
        {
            GenerateStorage();
            GenerateClients(20);
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

                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Can you repair it?");
                string userAnswer = Console.ReadLine();

                if (userAnswer == "yes")
                {
                    selectedStorage.RemoveDetail();
                    ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Repair successful!");
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

        private int CalculateMoneyEarned(Client client, Storage storage)
        {
            int repairCost = storage.Detail.Count * client.Count;
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
            ConsoleHelper.PrintColor(ConsoleColor.DarkYellow, $"\nTotal revenue for the day: {fine} money.");
            _penalties += fine;
        }

        private void DisplayStorage()
        {
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "Press: 1 to start");
            ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, "The stock contains:");

            for (int index = 0; index < _storages.Count; index++)
            {
                var storage = _storages[index];
                ConsoleHelper.PrintColor(ConsoleColor.DarkBlue, $"{index} - part {storage.Detail.Type}, quantity {storage.DetailCount}");
            }
        }

        private void GenerateStorage()
        {
            int minDetailCount = 5;
            int maxDetailCount = 50;

            Enum.GetValues(typeof(DetailType))
                .Cast<DetailType>()
                .ToList()
                .ForEach(type =>
                {
                    _storages.Add(new Storage(new Detail(type, _random.Next(minDetailCount, maxDetailCount)), _random.Next(minDetailCount, maxDetailCount)));
                });
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
        public Detail(DetailType detailType, int count)
        {
            Type = detailType;
            Count = count;
        }

        public DetailType Type { get; private set; }
        public int Count { get; private set; }
    }

    class Storage
    {
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
    }

    class Client
    {
        public int Count { get; private set; }
        public DetailType TypeOfDetailToBeRepaired { get; private set; }
        private Random _random = new Random();

        public Client()
        {
            TypeOfDetailToBeRepaired = GenerateDetailToMend();
            Count = GenerateDetailCount();
        }

        private DetailType GenerateDetailToMend()
        {
            int enumCount = Enum.GetNames(typeof(DetailType)).Length;
            return (DetailType)_random.Next(0, enumCount);
        }

        private int GenerateDetailCount()
        {
            int enumCount = Enum.GetNames(typeof(DetailType)).Length;
            return _random.Next(1, enumCount);
        }
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