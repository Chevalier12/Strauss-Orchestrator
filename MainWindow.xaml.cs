﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using MySqlConnector;

namespace StraussOrchestratorCSharp;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    //Editor Window
    public DummyWindow SubWindow;
    //===============Automation Buttons MainMenu======================
    public List<string> AutomationButtonsList = new()
    {
        "  Packages  ",
        "  Jobs  ",
        "  Machines  ",
        "  Scheduling  "
    };

    public StackPanel AutomationStackPanel;
    public List<Image> ImageMenuList;

    public List<StackPanel> MainMenuStackPanels = new();

    public List<string> SubMenuButtonNamesList = new()
    {
        "  Add  ",
        "  Edit  ",
        "  Delete  ",
        "  Run  ",
        "  Activate  ",
        "  Deactivate  ",
        "  Ok  ",
        "  Cancel  ",
        "  Stop  "
    };


    //===============All possible submenu items=========================
    public List<Button> SubMenuButtonsList = new();

    public StackPanel SubMenuStackPanel = new();
    public SqlTable Table = new();

    public MainWindow()
    {
        InitializeComponent();

        //Initialize the menu list made of pictures, then set the handlers by using the set_handlers method, see method
        //for more info.
        ImageMenuList = new List<Image>
            { Automation_Button, Home_Button, Monitoring_Button, Queues_Button, Assets_Button, Settings_Button };
        set_handlers(ImageMenuList, Image_Button_Click, Image_Button_MouseEnter, Image_Button_MouseLeave);

        //Create all sub-menu buttons
        foreach (var element in SubMenuButtonNamesList)
        {
            var newButton = new Button
            {
                Background = new SolidColorBrush(new Color { A = 100, R = 231, G = 233, B = 245 }),
                FontFamily = new FontFamily("Century Gothic"),
                Content = element,
                FontSize = 14,
                Height = 30,
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Name = element.Trim()
            };

            try
            {
                //This code adds handlers based on the name of the methods
                //I wrote this because I was lazy, it uses reflection so it's probably not a good idea
                //but since it only triggers once per run I don't think it can do that much damage.
                var method = typeof(MainWindow).GetMethod(newButton.Name + "_Method");
                var myDelegate = Delegate.CreateDelegate(typeof(RoutedEventHandler), this, method);
                newButton.Click += (RoutedEventHandler)myDelegate;
            }
            catch (Exception e)
            {
                //MessageBox.Show("error");
            }

            SubMenuButtonsList.Add(newButton);
        }

        //Create Automation main menu
        AutomationStackPanel = CreateMainMenu(AutomationButtonsList);
        AutomationStackPanel.Name = "Automation";

        //Contain Main Menu panels in here
        MainMenuStackPanels.Add(AutomationStackPanel);
    }
    //================================================

    public Boolean OKCancel_Pressed;
    public void Ok_Method(object sender, RoutedEventArgs e)
    {
        OKCancel_Pressed = true;
        IsEnabled = true;
        
        foreach (var item in ((Grid)SubWindow.DummyWindow_MainGrid.Children[0]).Children)
        {
            string columnName = null;
            string valueName = null;
            
            if (Grid.GetColumn((UIElement)item) == 0 && typeof(TextBlock) == item.GetType())
            {
                columnName = ((TextBlock)item).Text.Split(" ")[0];
            }   
            
            if (Grid.GetColumn((UIElement)item) == 0 && typeof(TextBox) == item.GetType())
            {
                valueName = ((TextBox)item).Text;
            }
            
            
        }
        SubWindow.Close();
        //Send all rows to SQL
    }

    public void Cancel_Method(object sender, RoutedEventArgs e)
    {
        OKCancel_Pressed = true;
        IsEnabled = true;
        SubWindow.Close();
        //Do nothing, user cancelled prompt
    }

    public void DummyWindow_Closed(object sender, EventArgs e)
    {
        if (!OKCancel_Pressed)
        {
            IsEnabled = true;
        }

        //User closed window forcefully.
    }
    
    public void Add_Method(object sender, RoutedEventArgs e)
    {
        OKCancel_Pressed = false;
        //This entire method is dedicated to the "Add" button.
        IsEnabled = false;

        //Create new window and add a grid
        SubWindow = new()
        {
            ShowActivated = true,
            Name = Table.Name,
            Title = "Editor",
            Owner = GetWindow(this),
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        SubWindow.Closed += DummyWindow_Closed!;
        
        Grid newGrid = new Grid
            { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        var textBlockColumnDefinition = new ColumnDefinition();
        var textBoxColumnDefinition = new ColumnDefinition { Width = new GridLength(205) };

        newGrid.ColumnDefinitions.Add(textBlockColumnDefinition);
        newGrid.ColumnDefinitions.Add(textBoxColumnDefinition);

        foreach (var col in Table.ColumnDefinitions)
        {
            //Grid will contains text blocks and textboxes so the user can insert new data rows in the data table, afterwards
            //the SQL table will be updated with this new data table.
            var tb = new TextBlock();
            ComboBox comboTextBox = new ComboBox();
            TextBox inputTextBox = new();
            
            var columnName = col.Name.Replace("_Header", "");

            if (columnName != "" && columnName != "ID")
            {
                var contentRowDefinition = new RowDefinition { Height = new GridLength(30) };
                newGrid.RowDefinitions.Add(contentRowDefinition);
                
                    tb.Text = col.Name.Replace("_Header", "").Replace("_", " ") + "  :  ";
                    tb.FontFamily = new FontFamily("Century Gothic");
                    //tb.Width = 200;
                    tb.FontSize = 14;
                    tb.Height = 23;
                    tb.HorizontalAlignment = HorizontalAlignment.Right;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(tb, 0);
                    
                    newGrid.Children.Add(tb);

                if (!columnName.Contains("Type"))
                {
                    inputTextBox.Width = 200;
                    inputTextBox.Height = 23;
                    inputTextBox.FontSize = 14;
                    inputTextBox.FontFamily = new FontFamily("Century Gothic");
                    inputTextBox.HorizontalAlignment = HorizontalAlignment.Center;
                    newGrid.Children.Add(inputTextBox);
                    Grid.SetRow(inputTextBox, newGrid.RowDefinitions.IndexOf(contentRowDefinition));
                    Grid.SetColumn(inputTextBox, 1);
                }
                else
                {
                    comboTextBox.Width = 200;
                    comboTextBox.Height = 23;
                    comboTextBox.FontSize = 14;
                    comboTextBox.FontFamily = new FontFamily("Century Gothic");
                    comboTextBox.HorizontalAlignment = HorizontalAlignment.Center;
                    comboTextBox.IsEditable = false;
                    comboTextBox.ItemsSource = CreateEnumFromDT(SQL_Load_SpecificColumn("Server=192.168.1.137;User ID=root;Database=straussdatabase",
                        tb.Text.Replace(":","").Trim(), "column_dropboxes"));
                    newGrid.Children.Add(comboTextBox);
                    Grid.SetRow(comboTextBox, newGrid.RowDefinitions.IndexOf(contentRowDefinition));
                    Grid.SetColumn(comboTextBox, 1);
                }
              
                
                //Several keywords are taken in consideration to avoid certain column values to be edited
                //as they are not meant to be edited
                if (columnName.Contains("Date"))
                {
                    inputTextBox.IsEnabled = false;
                    inputTextBox.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                }
                else if (columnName.Contains("Started") || columnName.Contains("Finished"))
                {
                    inputTextBox.IsEnabled = false;
                    inputTextBox.Text = "N/A";
                }

                Grid.SetRow(tb, newGrid.RowDefinitions.IndexOf(contentRowDefinition));
                
            }
        }


        //Add OK and Cancel buttons to the grid
        RowDefinition confirmationsRowDefinition = new();
        confirmationsRowDefinition.Height = new GridLength(30);
        newGrid.RowDefinitions.Add(confirmationsRowDefinition);
        var colIndex = 0;

        foreach (var subMenuControl in SubMenuButtonsList)
            if (subMenuControl.Name.Contains("Ok") || subMenuControl.Name.Contains("Cancel"))
            {
                try
                {
                    Grid ParentGrid = (Grid)subMenuControl.Parent;
                    ParentGrid.Children.Clear();
                }
                catch (Exception exception)
                {

                }
                newGrid.Children.Add(subMenuControl);
                Grid.SetRow(subMenuControl, newGrid.RowDefinitions.IndexOf(confirmationsRowDefinition));
                Grid.SetColumn(subMenuControl, colIndex);
                colIndex += 1;
            }

        SubWindow.DummyWindow_MainGrid.Children.Add(newGrid);
        SubWindow.Width = 400;
        SubWindow.Height = newGrid.Children.Count * 25;
        SubWindow.ResizeMode = ResizeMode.NoResize;
        SubWindow.Show();
    }

    private StackPanel CreateMainMenu(List<string> nameList)
    {
        var menuPanel = new StackPanel { Orientation = Orientation.Horizontal };


        foreach (var element in nameList)
        {
            var myButton = new Button
            {
                BorderBrush = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.DarkOrange),
                FontFamily = new FontFamily("Century Gothic"),
                Content = element,
                FontSize = 14,
                Height = 30,
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            myButton.Click -= Menu_Handlers;
            myButton.Click += Menu_Handlers;

            myButton.Name = element.Trim();
            menuPanel.Children.Add(myButton);
            menuPanel.Children.Add(new Separator
            {
                Width = 10, Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0)
            });
        }

        menuPanel.HorizontalAlignment = HorizontalAlignment.Left;
        menuPanel.VerticalAlignment = VerticalAlignment.Top;
        menuPanel.Margin = new Thickness(50, 50, 0, 0);
        return menuPanel;
    }

    private void Menu_Handlers(object sender, RoutedEventArgs e)
    {
        var senderName = ((Button)sender).Content.ToString().ToLower().Trim();

        var mySqlTables = SQL_Load_DatabaseTables("Server=192.168.1.137;User ID=root;Database=straussdatabase");
        var tableList = new List<string>();

        //Get all tables that match name with the sender_name
        foreach (DataRow item in mySqlTables.Rows)
            if (item.ItemArray[0].ToString().Contains(senderName))
                tableList.Add(item.ItemArray[0].ToString());

        //If any match, then create conditional tables
        if (tableList.Contains(senderName + "_list") && tableList.Contains(senderName + "_attachments"))
            CreateTable("Server=192.168.1.137;User ID=root;Database=straussdatabase",
                tableList[tableList.IndexOf(senderName + "_list")],
                tableList[tableList.IndexOf(senderName + "_attachments")], 0, 10);
        else if (tableList.Contains(senderName + "_list") && tableList.Contains(senderName + "_attachments") == false)
            CreateTable("Server=192.168.1.137;User ID=root;Database=straussdatabase",
                tableList[tableList.IndexOf(senderName + "_list")], null, 0, 10);

        //Create submenu for the table based on sender_name
        SubMenuStackPanel.Children.Clear();
        SubMenuStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        var subMenuItems =
            SQL_Load_SpecificColumn("Server=192.168.1.137;User ID=root;Database=straussdatabase", senderName, "category_submenu");

        MainGrid.Children.Remove(SubMenuStackPanel);

        foreach (var subMenuItem in SubMenuButtonsList)
        foreach (DataRow item in subMenuItems.Rows)
            if (item.ItemArray[0].ToString().Contains(subMenuItem.Name))
            {
                SubMenuStackPanel.Children.Add(subMenuItem);
                SubMenuStackPanel.Children.Add(new Separator
                    { Width = 10, Background = new SolidColorBrush(Colors.White) });
            }

        var tableThickness = Table.Margin;
        SubMenuStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        SubMenuStackPanel.VerticalAlignment = VerticalAlignment.Top;
        SubMenuStackPanel.Margin = new Thickness(tableThickness.Left, tableThickness.Top - 50, 0, 0);

        MainGrid.Children.Add(SubMenuStackPanel);
    }

    private void set_handlers(List<Image> myImages, MouseButtonEventHandler clickEventHandler,
        MouseEventHandler mouseEnterEventHandler, MouseEventHandler mouseLeaveEventHandler)
    {
        foreach (var element in myImages)
        {
            element.MouseLeftButtonDown += clickEventHandler;
            element.MouseEnter += mouseEnterEventHandler;
            element.MouseLeave += mouseLeaveEventHandler;
        }
    }

    private void Image_Button_MouseEnter(object sender, MouseEventArgs e)
    {
        //Uses naming conventions to dynamically assign bitmapimages based on event types
        var name = ((Image)sender).Name.Split("_")[0];

        ((Image)sender).Source =
            new BitmapImage(new Uri(name + "_Images/highlight_" + name + ".png", UriKind.Relative));

        if (((Image)sender).Tag == "IsSelected")
            ((Image)sender).Source =
                new BitmapImage(new Uri(name + "_Images/highlight_" + name + "_selected.png", UriKind.Relative));
    }

    private void Image_Button_MouseLeave(object sender, MouseEventArgs e)
    {
        //Uses naming conventions to dynamically assign bitmapimages based on event types

        var name = ((Image)sender).Name.Split("_")[0];

        ((Image)sender).Source =
            new BitmapImage(new Uri(name + "_Images/no_highlight_" + name + ".png", UriKind.Relative));

        if (((Image)sender).Tag == "IsSelected")
            ((Image)sender).Source =
                new BitmapImage(new Uri(name + "_Images/no_highlight_" + name + "_selected.png", UriKind.Relative));
    }

    private void Image_Button_Click(object sender, MouseButtonEventArgs e)
    {
        //Uses naming conventions to dynamically assign bitmapimages based on event types
        //It does get even more bizzare the more you look into this code.
        //Try not to stare into it because it will stare back.
        var name = ((Image)sender).Name;

        foreach (var image in ImageMenuList)
            if (image.Name == name)
            {
                //Add Main Menu for Category
                image.Source =
                    new BitmapImage(new Uri(
                        name.Split("_")[0] + "_Images/highlight_" + name.Split("_")[0] + "_selected.png",
                        UriKind.Relative));
                image.Tag = "IsSelected";

                //Dynamically assign grid based on button name by using naming conventions.
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(MainGrid); i++)
                {
                    var childVisual = (Visual)VisualTreeHelper.GetChild(MainGrid, i);

                    if (childVisual.GetType() == typeof(Grid))
                    {
                        var myGrid = (Grid)childVisual;
                        if (myGrid.Name.Contains(name.Split("_")[0] + "_Grid"))
                        {
                            foreach (var stackPanel in MainMenuStackPanels)
                                if (stackPanel.Name.Contains(name.Split("_")[0]))
                                {
                                    myGrid.Visibility = Visibility.Visible;
                                    if (myGrid.Children.Contains(stackPanel) == false)
                                    {
                                        myGrid.Children.Add(stackPanel);
                                        break;
                                    }
                                }
                        }
                        else if (myGrid.Name.Contains("_Grid"))
                        {
                            myGrid.Visibility = Visibility.Hidden;
                        }
                    }
                    //End adding Main Menu for Category
                }
            }
            else if (image.Name != name)
            {
                image.Source =
                    new BitmapImage(new Uri(
                        image.Name.Split("_")[0] + "_Images/no_highlight_" + image.Name.Split("_")[0] + ".png",
                        UriKind.Relative));
                image.Tag = "IsNotSelected";
            }
    }

    private void MainGrid_Loaded(object sender, RoutedEventArgs e)
    {
        //"Server=192.168.1.137;User ID=root;Database=straussdatabase"
    }

    public void CreateTable(string myConnectionString, string tableName, [Optional] string attachmentTableName,
        int startIndex, int endIndex)
    {
        MainGrid.Children.Remove(Table);
        Table = new SqlTable
        {
            SqlConnectionString = myConnectionString
        };

        if (attachmentTableName == null)
            Table.CreateGrid(Table.SQLTable_Load(tableName, startIndex, endIndex), startIndex, endIndex);
        else
            Table.CreateGrid(Table.SQLTable_Load(tableName, startIndex, endIndex), startIndex, endIndex,
                Table.SQLTable_Load(attachmentTableName, startIndex, endIndex));
        MainGrid.Children.Add(Table);
    }
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

    public static DataTable SQL_Load_SpecificColumn(string sqlConnectionString, string columnName, string tableName)
    {
        //This function is used to load settings for the MainMenu -> SubMenu items.
        var table = new DataTable();
        using (var connection = new MySqlConnection(sqlConnectionString))
        {
            using (var command = new MySqlCommand("SELECT `" + columnName + "` FROM `" + tableName + "`", connection))
            {
                connection.Open();
                table.Load(command.ExecuteReader());
                connection.Close();
            }
        }

        return table;
    }

    private IEnumerable<string> CreateEnumFromDT(DataTable dataTable)
    {
        List<string> list = new();
        foreach (DataRow dataTableRow in dataTable.Rows)
        {
            list.Add(dataTableRow.ItemArray[0].ToString());
        }

        return list.AsEnumerable();
    }
}