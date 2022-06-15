using Microsoft.WindowsAPICodePack.Dialogs;
using MySqlConnector;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StraussOrchestratorCSharp;

public class SQLTable : Grid
{
    public string SqlConnectionString;
    public SQLBrowseControls InteractiveBrowseControls;

    public void CreateGrid(DataTable dataTable, int startIndex, int endIndex, DataTable fileAttachmentsDataTable = null)
    {

        this.HorizontalAlignment = HorizontalAlignment.Center;
        this.VerticalAlignment = VerticalAlignment.Top;
        this.Margin = new Thickness(0, 200, 0, 0);

        // Create Row Definition for Header
        RowDefinition headerRowDefinition = new RowDefinition()
        {
            Height = new GridLength(35),
            Name = "Row_Header"
        };
        this.RowDefinitions.Add(headerRowDefinition);
        //===================================

        //Create columns from SQL Datatable
        foreach (DataColumn column in dataTable.Columns)
        {
            ColumnDefinition headerColumnDefinition = new ColumnDefinition()
            {
                Name = column.ColumnName.Replace(" ", "_") + "_Header"
            };

            TextBlock columnTextBlock = new TextBlock()
            {
                Text = column.ColumnName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Height = 23,
                Width = Double.NaN,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Century Gothic")
            };
            headerColumnDefinition.Width = new GridLength(columnTextBlock.Text.Length * 10 + 25);

            //Create new class to enable filtering from a button inside of the header like in Excel

            //==========================

            this.ColumnDefinitions.Add(headerColumnDefinition);
            this.Children.Add(columnTextBlock);
            Grid.SetRow(columnTextBlock, 0);
            Grid.SetColumn(columnTextBlock, this.ColumnDefinitions.IndexOf(headerColumnDefinition));
        }
        //================================

        //Create GUI Table from SQL DataTable -> Row fragment
        int columnIndex = 0;
        foreach (DataRow row in dataTable.Rows)
        {
            if (dataTable.Rows.IndexOf(row) >= startIndex && dataTable.Rows.IndexOf(row) <= endIndex)
            {
                RowDefinition contentRowDefinition = new()
                {
                    Height = new GridLength(30),
                    Tag = row
                };
                this.RowDefinitions.Add(contentRowDefinition);

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
                            Width = Double.NaN,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };


                        idCheckBox.Click += RowDefinition_Clicked;

                        Grid.SetRow(idCheckBox, this.RowDefinitions.IndexOf((contentRowDefinition)));
                        Grid.SetColumn(idCheckBox, columnIndex);
                        this.Children.Add(idCheckBox);
                    }
                    else
                    {
                        TextBlock contentTextBlock = new()
                        {
                            Text = element.ToString(),
                            FontSize = 14,
                            Height = 23,
                            Width = Double.NaN,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontFamily = new FontFamily("Century Gothic")
                        };
                        Grid.SetRow(contentTextBlock, this.RowDefinitions.IndexOf((contentRowDefinition)));
                        Grid.SetColumn(contentTextBlock, columnIndex);
                        this.Children.Add(contentTextBlock);
                    }
                    columnIndex++;
                }

                columnIndex = 0; //Test
            }
        }
        //=======================================

        //If attachments DataTable is provided, then create it without storing it, just add event...
        if (fileAttachmentsDataTable != null)
        {
            ColumnDefinition attachmentsColumnDefinition = new() { Width = new GridLength(100) };
            this.ColumnDefinitions.Add(attachmentsColumnDefinition);


            int attachRowIndex = 1; //row 0 is header, row 1+ is content
            int attachColIndex = this.ColumnDefinitions.IndexOf((attachmentsColumnDefinition));

            foreach (DataRow row in fileAttachmentsDataTable.Rows)  //test
            {
                Button downloadButton = new() { Content = "Download", Height = 23 };
                downloadButton.Tag = attachRowIndex;
                downloadButton.Click += Download_Button_Click;

                Grid.SetRow(downloadButton, attachRowIndex);
                Grid.SetColumn(downloadButton, attachColIndex);
                this.Children.Add(downloadButton);

                attachRowIndex += 1;
            }


        }
        //============================================

    }

    private void RowDefinition_Clicked(object sender, RoutedEventArgs e)
    {
        CheckBox rowCheckBox = (CheckBox)sender;

        int row_index = Int32.Parse(rowCheckBox.Content.ToString());
    }
    private void Download_Button_Click(object sender, RoutedEventArgs e)
    {
        int index = (int)((Button)sender).Tag;

        DataTable attachmentsDataTable = SQLTable_Load("package_attachment", index, index);

        DataRow fileDataRow = attachmentsDataTable.Rows[index];

        int attachmentsDataTableColumnCount = attachmentsDataTable.Columns.Count - 1;

        Byte[] fileArray = (Byte[])fileDataRow.ItemArray[attachmentsDataTableColumnCount];

        string name = (string)fileDataRow.ItemArray[1];

        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
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
        DataTable table = new DataTable();
        using (MySqlConnection connection = new MySqlConnection(SqlConnectionString))
        {
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM " + tableName + " WHERE ID >= " + startIndex + " AND ID <= " + endIndex, connection))
            {
                connection.Open();
                table.Load(command.ExecuteReader());
                connection.Close();
            }
        }

        return table;
    }

}