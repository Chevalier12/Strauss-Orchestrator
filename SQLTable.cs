using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using MySqlConnector;

namespace StraussOrchestratorCSharp;

public class SqlTable : Grid
{
    public SqlBrowseControls InteractiveBrowseControls;
    public string SqlConnectionString;

    public void CreateGrid(DataTable dataTable, int startIndex, int endIndex, DataTable fileAttachmentsDataTable = null)
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Top;
        Margin = new Thickness(0, 200, 0, 0);

        // Create Row Definition for Header
        var headerRowDefinition = new RowDefinition
        {
            Height = new GridLength(35),
            Name = "Row_Header"
        };
        RowDefinitions.Add(headerRowDefinition);
        //===================================

        //Create columns from SQL Datatable
        foreach (DataColumn column in dataTable.Columns)
        {
            var headerColumnDefinition = new ColumnDefinition
            {
                Name = column.ColumnName.Replace(" ", "_") + "_Header"
            };

            var columnTextBlock = new TextBlock
            {
                Text = column.ColumnName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Height = 23,
                Width = double.NaN,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Century Gothic")
            };
            headerColumnDefinition.Width = new GridLength(columnTextBlock.Text.Length * 10 + 25);

            //Create new class to enable filtering from a button inside of the header like in Excel

            //==========================

            ColumnDefinitions.Add(headerColumnDefinition);
            Children.Add(columnTextBlock);
            SetRow(columnTextBlock, 0);
            SetColumn(columnTextBlock, ColumnDefinitions.IndexOf(headerColumnDefinition));
        }
        //================================

        //Create GUI Table from SQL DataTable -> Row fragment
        var columnIndex = 0;
        foreach (DataRow row in dataTable.Rows)
            if (dataTable.Rows.IndexOf(row) >= startIndex && dataTable.Rows.IndexOf(row) <= endIndex)
            {
                RowDefinition contentRowDefinition = new()
                {
                    Height = new GridLength(30),
                    Tag = row
                };
                RowDefinitions.Add(contentRowDefinition);

                foreach (var element in row.ItemArray)
                {
                    if (columnIndex == 0)
                    {
                        CheckBox idCheckBox = new()
                        {
                            FontFamily = new FontFamily("Century Gothic"),
                            Content = row[0],
                            FontSize = 14,
                            Height = 23,
                            Width = double.NaN,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };


                        idCheckBox.Click += RowDefinition_Clicked;

                        SetRow(idCheckBox, RowDefinitions.IndexOf(contentRowDefinition));
                        SetColumn(idCheckBox, columnIndex);
                        Children.Add(idCheckBox);
                    }
                    else
                    {
                        TextBlock contentTextBlock = new()
                        {
                            Text = element.ToString(),
                            FontSize = 14,
                            Height = 23,
                            Width = double.NaN,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontFamily = new FontFamily("Century Gothic")
                        };
                        SetRow(contentTextBlock, RowDefinitions.IndexOf(contentRowDefinition));
                        SetColumn(contentTextBlock, columnIndex);
                        Children.Add(contentTextBlock);
                    }

                    columnIndex++;
                }

                columnIndex = 0;
            }
        //=======================================

        //If attachments DataTable is provided, then create it without storing it, just add event...
        if (fileAttachmentsDataTable != null)
        {
            ColumnDefinition attachmentsColumnDefinition = new() { Width = new GridLength(100) };
            ColumnDefinitions.Add(attachmentsColumnDefinition);


            var attachRowIndex = 1; //row 0 is header, row 1+ is content
            var attachColIndex = ColumnDefinitions.IndexOf(attachmentsColumnDefinition);

            foreach (DataRow row in fileAttachmentsDataTable.Rows) //test
            {
                Button downloadButton = new()
                {
                    Content = "Download", Height = 23,
                    Tag = attachRowIndex
                };
                downloadButton.Click += Download_Button_Click;

                SetRow(downloadButton, attachRowIndex);
                SetColumn(downloadButton, attachColIndex);
                Children.Add(downloadButton);

                attachRowIndex += 1;
            }
        }
        //============================================
    }

    private void RowDefinition_Clicked(object sender, RoutedEventArgs e)
    {
        var rowCheckBox = (CheckBox)sender;

        var rowIndex = int.Parse(rowCheckBox.Content.ToString());
    }

    private void Download_Button_Click(object sender, RoutedEventArgs e)
    {
        var index = (int)((Button)sender).Tag;

        var attachmentsDataTable = SQLTable_Load("package_attachment", index, index);

        var fileDataRow = attachmentsDataTable.Rows[index];

        var attachmentsDataTableColumnCount = attachmentsDataTable.Columns.Count - 1;

        var fileArray = (byte[])fileDataRow.ItemArray[attachmentsDataTableColumnCount];

        var name = (string)fileDataRow.ItemArray[1];

        var dialog = new CommonOpenFileDialog();
        dialog.InitialDirectory = "C:\\Users";
        dialog.IsFolderPicker = true;
        dialog.ShowDialog();

        using (var fs = new FileStream(dialog.FileName + "\\" + name + ".nupkg", FileMode.Create, FileAccess.Write))
        {
            fs.Write(fileArray, 0, fileArray.Length);
        }
    }

    public DataTable SQLTable_Load(string tableName, int startIndex, int endIndex)
    {
        var table = new DataTable();
        using (var connection = new MySqlConnection(SqlConnectionString))
        {
            using (var command =
                   new MySqlCommand(
                       "SELECT * FROM " + tableName + " WHERE ID >= " + startIndex + " AND ID <= " + endIndex,
                       connection))
            {
                connection.Open();
                table.Load(command.ExecuteReader());
                table.TableName = tableName;
                connection.Close();
            }
        }

        return table;
    }
}