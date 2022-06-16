using System.Data;
using MySqlConnector;

namespace StraussOrchestratorCSharp;

public class Library
{
    public static DataTable SQL_Load_DatabaseTables(string sqlConnectionString)
    {
        var table = new DataTable();
        using (var connection = new MySqlConnection(sqlConnectionString))
        {
            using (var command = new MySqlCommand("SHOW TABLES;", connection))
            {
                connection.Open();
                table.Load(command.ExecuteReader());
                connection.Close();
            }
        }

        return table;
    }

    public static DataTable SQL_Load_SubMenuItems(string sqlConnectionString, string columnName)
    {
        //This function is used to load settings for the MainMenu -> SubMenu items.
        var table = new DataTable();
        using (var connection = new MySqlConnection(sqlConnectionString))
        {
            using (var command = new MySqlCommand("SELECT " + columnName + " FROM category_submenu", connection))
            {
                connection.Open();
                table.Load(command.ExecuteReader());
                connection.Close();
            }
        }

        return table;
    }
}