using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InventoryManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "The Swords of Ditto inventory manager";
            Console.WindowHeight = 60;
            WriteTitle();

            var savedataPath = GetSavePath();

            CopySaveToBackup(savedataPath);

            var saveData = LoadSave(savedataPath);
            var itemDefinitions = LoadItemDefinitions();

            Menu(saveData, itemDefinitions);

            WriteSave(savedataPath, saveData);

            Console.Clear();
        }

        private static void WriteLine(string value, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"\t{value}");
            Console.ForegroundColor = oldColor;
        }

        private static void Write(string value)
        {
            Console.Write($"\t{value}");
        }

        private static void WriteTitle()
        {
            Console.Clear();
            WriteLine("The Swords of Ditto inventory manager");
            WriteLine("-------------------------------------");

            WriteLine("################################################", ConsoleColor.DarkYellow);
            WriteLine("# The application does not work well           #", ConsoleColor.DarkYellow);
            WriteLine("# with Steam Cloud (or any other cloud saves). #", ConsoleColor.DarkYellow);
            WriteLine("# It is advised to deactivate it to minimize   #", ConsoleColor.DarkYellow);
            WriteLine("################################################", ConsoleColor.DarkYellow);
            WriteLine("# To save your modifications, exit the         #", ConsoleColor.DarkYellow);
            WriteLine("# application through the \"Exit\" (0) command.#", ConsoleColor.DarkYellow);
            WriteLine("################################################", ConsoleColor.DarkYellow);
        }

        private static void WriteSave(string savedataPath, JObject saveData)
        {
            File.WriteAllLines(savedataPath, new[] { "-1", JsonConvert.SerializeObject(saveData, new DoubleJsonConverter()) });
        }

        private static void Menu(JObject saveData, List<ItemDefinition> itemDefinitions)
        {
            var exit = false;
            do
            {
                int commandId = -1;
                var response = "";
                do
                {
                    WriteTitle();

                    DisplayInventory(saveData, itemDefinitions);

                    commandId = -1;
                    Console.WriteLine();
                    WriteLine("What do you want to do:");
                    Console.WriteLine();
                    WriteLine("1 - Add celestial fragments");
                    WriteLine("2 - Add gold");
                    WriteLine("3 - Add items");
                    WriteLine("0 - Exit");
                    Write("Selection: ");
                    response = Console.ReadLine();
                } while (string.IsNullOrWhiteSpace(response) || !int.TryParse(response, out commandId) || (commandId != 0 && commandId != 1 && commandId != 2 && commandId != 3));

                WriteTitle();

                Console.WriteLine();

                switch (commandId)
                {
                    case 0:
                        exit = true;
                        WriteLine("Exiting...");
                        break;

                    case 1:
                        AddFragments(saveData);
                        break;

                    case 2:
                        AddGold(saveData);
                        break;

                    case 3:
                        AddItem(saveData, itemDefinitions);
                        break;
                }

                WriteTitle();

                DisplayInventory(saveData, itemDefinitions);
            } while (!exit);
        }

        private static string GetSavePath()
        {
            var windowsGamePath = Path.Combine(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"), "The_Swords_of_Ditto");
            if (Directory.Exists(windowsGamePath))
            {
                var folders = Directory.GetDirectories(windowsGamePath);
                if (folders.Length > 0)
                {
                    var files = Directory.GetFiles(folders[0], "savedata");
                    if (files.Length > 0)
                    {
                        return files[0];
                    }
                }
            }

            string savedataPath;
            do
            {
                Write("Path to your savedata file: ");
                savedataPath = Console.ReadLine();
            } while (!File.Exists(savedataPath));
            return savedataPath;
        }

        private static void AddItem(JObject saveData, List<ItemDefinition> itemDefinitions)
        {
            var inventory = GetInventory(saveData);

            int itemId = -1;
            var response = "";
            do
            {
                itemId = -1;
                WriteLine("Please choose an item from the list:");
                Console.WriteLine();
                ListItems(itemDefinitions, inventory);
                Write("Item id:");
                response = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(response) || !int.TryParse(response, out itemId) || !itemDefinitions.Any(item => item.Id == itemId));

            var quantity = 1d;
            var itemDefinition = itemDefinitions.First(item => item.Id == itemId);
            if (!itemDefinition.StackMax.HasValue || itemDefinition.StackMax.Value > 1)
            {
                Write("Indicate the quantity (default: 1): ");
                var quantityResponse = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(quantityResponse))
                {
                    double.TryParse(quantityResponse, out quantity);
                }
            }
            else if (inventory.ContainsKey(itemId))
            {
                // avoid to add an already possessed item whith a stack-max of 1
                return;
            }

            // if already in inventory
            if (inventory.ContainsKey(itemId))
            {
                // only modify quantity
                inventory[itemId] += quantity;
            }
            else
            {
                inventory.Add(itemId, quantity);
            }

            SetInventory(saveData, inventory);
        }

        private static SortedDictionary<double, double> GetInventory(JObject saveData)
        {
            return new SortedDictionary<double, double>(
                saveData["inventory"][0].Select(t => (double)t)
                    .Zip(saveData["inventory"][1].Select(t => (double)t),
                            (id, quantity) => new { id, quantity }
                        ).ToDictionary(x => x.id, x => x.quantity)
                );
        }

        private static void SetInventory(JObject saveData, IDictionary<double, double> newInventory)
        {
            saveData["inventory"][0] = new JArray(newInventory.Keys.Select(i => new JValue(i)));
            saveData["inventory"][1] = new JArray(newInventory.Values.Select(i => new JValue(i)));
        }

        private static void AddFragments(JObject saveData)
        {
            WriteLine("Add celestial fragments");
            Write("Indicate the quantity (default: 100): ");
            var quantity = Console.ReadLine();

            var amount = 100d;
            if (!string.IsNullOrWhiteSpace(quantity))
            {
                double.TryParse(quantity, out amount);
            }

            WriteLine($"Add {amount} fragments");

            saveData["serial grid"][1][24] = (double)saveData["serial grid"][1][24] + amount;
        }

        private static void AddGold(JObject saveData)
        {
            WriteLine("Add gold");
            Write("Indicate the quantity (default: 1000): ");
            var quantity = Console.ReadLine();

            var amount = 1000d;
            if (!string.IsNullOrWhiteSpace(quantity))
            {
                double.TryParse(quantity, out amount);
            }

            WriteLine($"Add {amount} gold");

            saveData["serial grid"][1][0] = (double)saveData["serial grid"][1][0] + amount;
        }

        private static void ListItems(List<ItemDefinition> itemDefinitions, SortedDictionary<double, double> inventory)
        {
            WriteLine("List of known items (id: name):");
            itemDefinitions.ForEach(itemDef =>
            {
                var quantity = inventory.ContainsKey(itemDef.Id) ? $"(have {inventory[itemDef.Id]})" : "";
                var comment = string.IsNullOrWhiteSpace(itemDef.Comment) ? "" : $"\t~ {itemDef.Comment}";
                WriteLine($" - {itemDef.Id}:\t{itemDef.Name.PadRight(20, ' ')}{quantity.PadRight(10, ' ')} {comment}");
            });
        }

        private static void DisplayInventory(JObject saveData, List<ItemDefinition> itemDefinitions)
        {
            Console.WriteLine();
            WriteLine("Current inventory:");
            WriteLine("-----------------");

            WriteLine($"Celestial fragments: {saveData["serial grid"][1][24].Value<double>()}");
            WriteLine($"Gold: {saveData["serial grid"][1][0].Value<double>()}");
            Console.WriteLine();

            WriteLine("Items:");
            var inventory = GetInventory(saveData);

            foreach (var item in inventory)
            {
                var itemName = $"{item.Key} (unknown item)";
                var knownItem = itemDefinitions.Find(id => id.Id == item.Key);
                if (knownItem != null)
                {
                    itemName = knownItem.Name;
                }
                WriteLine($"{itemName} : {item.Value}");
            }
        }

        private static void CopySaveToBackup(string savedataPath)
        {
            // backup of the save
            File.Copy(savedataPath, savedataPath + ".backup", true);
        }

        private static JObject LoadSave(string savedataPath)
        {
            var saveLines = File.ReadAllLines(savedataPath);

            return JObject.Parse(saveLines[1]);
        }

        private static List<ItemDefinition> LoadItemDefinitions()
        {
            return JsonConvert.DeserializeObject<List<ItemDefinition>>(File.ReadAllText("items-definition.json"));
        }
    }
}
