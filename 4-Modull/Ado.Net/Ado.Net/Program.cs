using System.Data;
using Ado.Net.Server;
using Dapper;
using Npgsql;

namespace Ado.Net;

internal class Program
{
    static void Main(string[] args)
    {
        bool connectionSuccessful = false;
        string connectionString = string.Empty;

        while (!connectionSuccessful)
        {
            // Ma'lumotlarni kiritish jarayoni
            Console.WriteLine("Ma'lumotlar bazasi servir manzili (masalan: localhost) [default: localhost]:");
            string servir = Console.ReadLine();
            if (string.IsNullOrEmpty(servir))
            {
                servir = "localhost"; // Default qiymat
            }

            Console.WriteLine("Ma'lumotlar bazasi porti (masalan: 5432) [default: 5432]:");
            string port = Console.ReadLine();
            if (string.IsNullOrEmpty(port))
            {
                port = "5432"; // Default qiymat
            }

            Console.WriteLine("Ma'lumotlar bazasi nomi (masalan: person): ");
            string database = Console.ReadLine();
            while (string.IsNullOrEmpty(database))
            {
                Console.WriteLine("Ma'lumotlar bazasi nomi majburiy. Iltimos, kiriting:");
                database = Console.ReadLine();
            }

            Console.WriteLine("Foydalanuvchi nomi (masalan: postgres) [default: postgres]:");
            string user = Console.ReadLine();
            if (string.IsNullOrEmpty(user))
            {
                user = "postgres"; // Default qiymat
            }

            Console.WriteLine("Password (masalan: ****):");
            string password = Console.ReadLine();
            while (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Parolni kiriting:");
                password = Console.ReadLine();
            }

            connectionString = $"Host={servir}; Port={port}; Database={database}; Username={user}; Password={password};";

            Console.WriteLine("Ma'lumotlar bazasiga ulanishga urinish...");

            try
            {
                // Ulanish jarayoni
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Ma'lumotlar bazasiga muvaffaqiyatli ulandingiz!");

                    connectionSuccessful = true; // Ulanish muvaffaqiyatli bo'lsa, loopdan chiqiladi

                    // Asosiy menu jarayoni
                    bool isRunning = true;
                    while (isRunning)
                    {
                        List<string> mainMenu = new List<string>()
                        {
                            "Schema"
                        };

                        int selectedMenu = ArrowIndex(mainMenu, "Main Menu");

                        switch (selectedMenu)
                        {
                            case 0: // "Schema" tugmasi
                                bool schemaMenuRunning = true;
                                while (schemaMenuRunning)
                                {
                                    List<string> schemaMenu = new List<string>()
                                    {
                                        "1: Jadval ko'rish",
                                        "2: Jadvaldagi ma'lumotlarni ko'rish",
                                        "3: Jadvaldagi ustunlari haqida ma'lumot olish",
                                        "4: Ma'lumotlar qo'shish",
                                        "5: Ma'lumotlarni yangilash",
                                        "6: Ma'lumotlarni o'chirish",
                                        "7: Chiqish"
                                    };

                                    int schemaChoice = ArrowIndex(schemaMenu, "Schema Menu");

                                    switch (schemaChoice)
                                    {
                                        case 0:
                                            ArrowIndex(DataViewer.ViewTable(connection), "Tables");
                                            break;
                                        case 1:
                                            ArrowIndex(DataViewer.ViewTableData(connection), "Table Data");
                                            break;
                                        case 2:
                                            // Jadvaldagi ustunlar haqida ma'lumot olish
                                            ArrowIndex(DataViewer.ViewTableColumns(connection), "Table columin");
                                            break;
                                        case 3:
                                            // Ma'lumot qo'shish
                                            DataViewer.InsertData(connection);
                                            break;
                                        case 4:
                                            // Ma'lumotlarni yangilash
                                            DataViewer.UpdateData(connection);
                                            break;
                                        case 5:
                                            // Ma'lumotlarni o'chirish
                                            DataViewer.DeleteData(connection);
                                            break;
                                        case 6:
                                            schemaMenuRunning = false; // Schema menyusidan chiqish
                                            break;
                                        default:
                                            Console.WriteLine("Noto'g'ri buyruq!");
                                            break;
                                    }
                                }
                                break;
                            default:
                                isRunning = false;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Xatolik: {ex.Message}");
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Ma'lumotlaringizda xatolik borligi sababli bazaga ulanolmadingiz.");
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine(" Iltimos, qayta urinib ko'ring.");
                Console.ResetColor();
            }
        }
    }

    public static int ArrowIndex(List<string> list, string name)
    {
        int selectIndex = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("\t\t\t" + name);

            for (int i = 0; i < list.Count; i++)
            {
                if (i == selectIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                Console.WriteLine(list[i]);
                Console.ResetColor();
            }

            ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            if (consoleKeyInfo.Key == ConsoleKey.UpArrow) selectIndex = (selectIndex - 1 + list.Count) % list.Count;
            else if (consoleKeyInfo.Key == ConsoleKey.DownArrow) selectIndex = (selectIndex + 1) % list.Count;
            else if (consoleKeyInfo.Key == ConsoleKey.Enter) return selectIndex;
        }
    }
}

