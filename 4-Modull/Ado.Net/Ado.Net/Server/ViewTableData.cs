using Npgsql;
using Dapper;

namespace Ado.Net.Server;

public static class DataViewer
{
    public static List<string> ViewTable(NpgsqlConnection connection)
    {
        // Jadvallarni olish
        var tables = connection.Query<string>("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");

        Console.WriteLine("Jadvallar:");
        List<string> tableList = new List<string>();
        int index = 1;
        foreach (var table in tables)
        {
            tableList.Add(table);
            Console.WriteLine($"{index}. {table}"); // Jadval nomlari raqamlangan holda ko'rsatiladi
            index++;
        }
        return tableList;
    }

    public static List<string> ViewTableData(NpgsqlConnection connection)
    {
        // Jadvallarni ko'rsatish uchun ViewTable funksiyasini chaqiramiz
        var tableList = ViewTable(connection);

        // Foydalanuvchidan jadval raqamini tanlashni so'raymiz
        Console.WriteLine("Jadval raqamini tanlang:");
        if (int.TryParse(Console.ReadLine(), out int selectedIndex) && selectedIndex > 0 && selectedIndex <= tableList.Count)
        {
            // Foydalanuvchi tanlagan jadval nomini olamiz
            string table = tableList[selectedIndex - 1];

            // Tanlangan jadvalning ustunlarini olish
            var columns = connection.Query<string>($"SELECT column_name FROM information_schema.columns WHERE table_name = '{table}'");
            var columnNames = string.Join(", ", columns);

            // Tanlangan jadvaldagi ma'lumotlarni olish
            string query = $"SELECT {columnNames} FROM {table}";
            var data = connection.Query(query);

            // Ustunlarni bosib chiqarish
            Console.WriteLine($"\nJadvaldagi ma'lumotlar ({table}):");
            List<string> strings = new List<string>();

            // Ustun nomlarini chiroyli ko'rinishda chiqaramiz
            Console.WriteLine(new string('-', 50)); // Jadval boshlanishi uchun chiziq
            foreach (var column in columns)
            {
                Console.Write($"{column,-15}"); // Har bir ustun nomini 15 belgidan joy ajratib chiqarish
            }
            Console.WriteLine();
            Console.WriteLine(new string('-', 50)); // Ustunlardan keyin ajratuvchi chiziq

            // Har bir qatorni chiqaramiz
            foreach (var row in data)
            {
                var rowValues = ((IDictionary<string, object>)row).Values;
                foreach (var value in rowValues)
                {
                    // Har bir ustun qiymatini ham 15 belgidan joy ajratib chiqarish
                    Console.Write($"{value,-15}");
                }
                Console.WriteLine();
                strings.Add(string.Join(", ", rowValues)); // Ma'lumotlar string ko'rinishda saqlanadi
            }
            Console.WriteLine(new string('-', 50)); // Jadval tugashi uchun chiziq
            return strings;
        }
        else
        {
            Console.WriteLine("Noto'g'ri tanlov, qayta urinib ko'ring.");
            return new List<string>();
        }
    }
    public static List<string> ViewTableColumns(NpgsqlConnection connection)
    {
        Console.WriteLine("Jadval nomini kiriting:");
        string table = Console.ReadLine();

        // Ustunlar ro'yxatini olish
        var columns = connection.Query("SELECT column_name, data_type FROM information_schema.columns WHERE table_name = @Table", new { Table = table });

        Console.WriteLine($"\nJadval ustunlari ({table}):\n");

        List<string> columnList = new List<string>();

        // Har bir ustunni chiroyli ko'rinishda chop etish
        foreach (var column in columns)
        {
            string columnInfo = $"{column.column_name} - {column.data_type}";
            Console.WriteLine(columnInfo);
            columnList.Add(column.column_name); // Har bir ustunni ro'yxatga qo'shish
        }

        return columnList; // Ro'yxatni qaytarish
    }
    public static void InsertData(NpgsqlConnection connection)
    {
        // Barcha jadvallarni olish
        var tables = connection.Query<string>("SELECT table_name FROM information_schema.tables WHERE table_schema='public'");

        // Jadvallarni son ketma-ketligida chiqarish
        Console.WriteLine("Jadvallar ro'yxati:");
        int tableIndex = 1;
        foreach (var tableName in tables)
        {
            Console.WriteLine($"{tableIndex}. {tableName}");
            tableIndex++;
        }

        // Foydalanuvchidan jadvalni son orqali tanlash
        Console.WriteLine("\nJadvalni tanlash uchun uning raqamini kiriting:");
        int selectedTableIndex;
        while (!int.TryParse(Console.ReadLine(), out selectedTableIndex) || selectedTableIndex < 1 || selectedTableIndex > tables.Count())
        {
            Console.WriteLine("Noto'g'ri raqam kiritildi. Iltimos, to'g'ri raqamni kiriting:");
        }

        // Tanlangan jadval nomini olish
        string table = tables.ElementAt(selectedTableIndex - 1);
        Console.WriteLine($"\nSiz tanlagan jadval: {table}");

        // Tanlangan jadvalning ustunlarini olish
        var columns = connection.Query("SELECT column_name, data_type FROM information_schema.columns WHERE table_name = @Table", new { Table = table });

        // Ustunlar ro'yxatini chiqarish
        Console.WriteLine($"\n{table} jadvali ustunlari:");
        foreach (var column in columns)
        {
            Console.WriteLine($"- {column.column_name} ({column.data_type})");
        }

        List<string> selectedColumns = new List<string>();
        List<string> values = new List<string>();
        var parameters = new DynamicParameters();

        // Har bir ustun uchun ma'lumot kiritish jarayoni
        foreach (var column in columns)
        {
            Console.WriteLine($"\n{column.column_name} uchun qiymat kiriting (hech narsa kiritmasangiz, ushbu ustun o'tkazib yuboriladi):");
            string value = Console.ReadLine().Trim();

            if (!string.IsNullOrEmpty(value))
            {
                selectedColumns.Add(column.column_name);
                values.Add($"@{column.column_name}");

                // Ustun turiga mos ravishda qiymatni qo'shish
                if (column.data_type == "integer")
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        parameters.Add($"@{column.column_name}", intValue);
                    }
                    else
                    {
                        Console.WriteLine($"Xatolik: {column.column_name} uchun integer qiymat kiritishingiz kerak.");
                        return;
                    }
                }
                else if (column.data_type == "numeric")
                {
                    if (decimal.TryParse(value, out decimal decimalValue))
                    {
                        parameters.Add($"@{column.column_name}", decimalValue);
                    }
                    else
                    {
                        Console.WriteLine($"Xatolik: {column.column_name} uchun numeric qiymat kiritishingiz kerak.");
                        return;
                    }
                }
                else if (column.data_type == "boolean")
                {
                    if (bool.TryParse(value, out bool boolValue))
                    {
                        parameters.Add($"@{column.column_name}", boolValue);
                    }
                    else
                    {
                        Console.WriteLine($"Xatolik: {column.column_name} uchun boolean qiymat kiritishingiz kerak (true/false).");
                        return;
                    }
                }
                else
                {
                    // Boshqa turlarga string sifatida kiritish
                    parameters.Add($"@{column.column_name}", value);
                }
            }
        }

        // Kiritilgan ustunlar va qiymatlarni tekshirish
        if (selectedColumns.Count == 0)
        {
            Console.WriteLine("Hech qanday ma'lumot kiritilmadi.");
            return;
        }

        // SQL INSERT so'rovini yaratish
        string columnList = string.Join(", ", selectedColumns);
        string valueList = string.Join(", ", values);
        string query = $"INSERT INTO {table} ({columnList}) VALUES ({valueList})";

        // So'rovni bajarish
        connection.Execute(query, parameters);

        // Tasdiqlash
        Console.WriteLine("\nMa'lumot muvaffaqiyatli qo'shildi!");
    }

    public static void UpdateData(NpgsqlConnection connection)
    {
        try
        {
            // Jadvallar va ustunlarni chiqarish
            var tables = connection.Query<string>("SELECT table_name FROM information_schema.tables WHERE table_schema='public'").ToList();
            Console.WriteLine("Jadvallar ro'yxati:");
            for (int i = 0; i < tables.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {tables[i]}");
            }

            // Jadvalni tanlash
            Console.WriteLine("Jadval raqamini kiriting:");
            int tableIndex = Convert.ToInt32(Console.ReadLine()) - 1;
            string table = tables[tableIndex];

            // Ustunlarni chiqarish
            var columns = connection.Query($"SELECT column_name, data_type FROM information_schema.columns WHERE table_name = '{table}'").ToList();
            Console.WriteLine($"{table} jadvali ustunlari:");
            foreach (var column in columns)
            {
                Console.WriteLine($"- {column.column_name} ({column.data_type})");
            }

            // Ustunlarga qiymatlar kiritish
            var updateSet = new List<string>();
            var parameters = new DynamicParameters();
            foreach (var column in columns)
            {
                if (column.column_name.ToLower() != "id") // ID ustunini chiqarmaymiz
                {
                    Console.WriteLine($"{column.column_name} uchun yangi qiymat kiriting:");
                    string value = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        updateSet.Add($"{column.column_name} = @{column.column_name}");
                        parameters.Add($"@{column.column_name}", value);
                    }
                }
            }

            // O'zgartirilishi kerak bo'lgan ID raqamini so'rash
            Console.WriteLine("O'zgartirilishi kerak bo'lgan ID raqamini kiriting:");
            int oldId = Convert.ToInt32(Console.ReadLine());
            parameters.Add("@oldId", oldId);

            // Yangi ID ni so'rash
            Console.WriteLine("Yangi ID raqamini kiriting:");
            int newId = Convert.ToInt32(Console.ReadLine());
            parameters.Add("@newId", newId);

            // SQL so'rovini tuzish va bajarish
            string setClause = string.Join(", ", updateSet);
            string query = $"UPDATE {table} SET {setClause}, id = @newId WHERE id = @oldId";
            connection.Execute(query, parameters);

            Console.WriteLine("Ma'lumot muvaffaqiyatli yangilandi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Xatolik: {ex.Message}");
        }
    }
    //public static void DeleteData(NpgsqlConnection connection)
    //{
    //    try
    //    {
    //        var tables1 = connection.Query<string>("SELECT table_name FROM information_schema.tables WHERE table_schema='public'");

    //        // Jadvallarni son ketma-ketligida chiqarish
    //        Console.WriteLine("Jadvallar ro'yxati:");
    //        int tableIndex1 = 1;
    //        foreach (var tableName in tables1)
    //        {
    //            Console.WriteLine($"{tableIndex1}. {tableName}");
    //            tableIndex1++;
    //        }

    //        //var tables = ViewTable(connection);
    //        //// Oldindan belgilangan jadval nomlari


    //        //Console.WriteLine("O'chirilishi kerak bo'lgan jadvalni tanlang:");
    //        //for (int i = 0; i < tables.Count; i++)
    //        //{
    //        //    Console.WriteLine($"{i + 1}. {tables[i]}");
    //        //}



    //        int tableIndex = Convert.ToInt32(Console.ReadLine()) - 1;
    //        if (tableIndex < 0 || tableIndex >= tables1.Count)
    //        {
    //            Console.WriteLine("Noto'g'ri tanlov.");
    //            return;
    //        }

    //        string table = tables1[tableIndex];

    //        Console.WriteLine("O'chirilishi kerak bo'lgan ma'lumotni ko'rsatuvchi so'rov (masalan, 'WHERE id=@id'): ");
    //        string deleteQuery = Console.ReadLine();

    //        Console.WriteLine("O'chirish uchun parametrlarni kiriting (masalan, '1'): ");
    //        var parameters = Console.ReadLine().Split(',');

    //        string query = $"DELETE FROM {table} {deleteQuery}";
    //        connection.Execute(query, parameters);
    //        Console.WriteLine("Ma'lumot o'chirildi!");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Xatolik: {ex.Message}");
    //    }
    //}
    public static void DeleteData(NpgsqlConnection connection)
    {
        try
        {
            var tableList = ViewTable(connection);
            Console.WriteLine("Jadval nomini kiriting:");
            string table = Console.ReadLine();
            Console.WriteLine("O'chirilishi kerak bo'lgan ma'lumotni ko'rsatuvchi so'rov (masalan, 'WHERE id=@id'): ");
            string deleteQuery = Console.ReadLine();

            
            Console.WriteLine("O'chirish uchun parametrlarni kiriting (masalan, '1'):");
            var parameters = Console.ReadLine().Split(',');

            string query = $"DELETE FROM {table} {deleteQuery}";
            connection.Execute(query, parameters);
            Console.WriteLine("Ma'lumot o'chirildi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Xatolik: {ex.Message}");
        }
    }

}
