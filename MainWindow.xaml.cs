using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StraussOrchestratorCSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SQLTable Table = new();
        public List<Image> ImageMenuList;

        //===============Automation Buttons MainMenu======================
        public List<string> Automation_ButtonsList = new()
        {
            "  Packages  ",
            "  Jobs  ",
            "  Machines  "
        };

        public StackPanel Automation_StackPanel;

        public List<StackPanel> MainMenuStackPanels = new();


        //===============All possible submenu items=========================
        public List<Button> SubMenuButtonsList = new();
        public List<String> SubMenuButtonNamesList = new()
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

        public StackPanel SubMenuStackPanel = new();
        //================================================

        public void Add_Method(object sender, RoutedEventArgs e)
        {
            //This entire method is dedicated to the "Add" button.
            //this.IsEnabled = false;

            //Create new window and add a grid
            DummyWindow addWindow = new() {ShowActivated = true, Title= "", Owner = Window.GetWindow(this), WindowStartupLocation = WindowStartupLocation.CenterOwner};
            addWindow.Show();

            Grid newGrid = new Grid() {HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center};

            ColumnDefinition textBlockColumnDefinition = new ColumnDefinition();
            ColumnDefinition textBoxColumnDefinition = new ColumnDefinition() { Width = new GridLength(205)};

            newGrid.ColumnDefinitions.Add(textBlockColumnDefinition);
            newGrid.ColumnDefinitions.Add(textBoxColumnDefinition);

            foreach (ColumnDefinition col in Table.ColumnDefinitions)
            {   //Grid will contains text blocks and textboxes so the user can insert new data rows in the data table, afterwards
                //the SQL table will be updated with this new data table.
                TextBlock tb = new TextBlock();
                TextBox inputTextBox = new();
                var columnName = col.Name.Replace("_Header", "");

                if (columnName != "" && columnName != "ID")
                {
                    RowDefinition contentRowDefinition = new RowDefinition() { Height = new GridLength(30) };
                    newGrid.RowDefinitions.Add(contentRowDefinition);

                    tb.Text = col.Name.Replace("_Header", "").Replace("_", " ") + "  :  ";
                    tb.FontFamily = new FontFamily("Century Gothic");
                    tb.FontSize = 14;
                    tb.Height = 23;
                    tb.HorizontalAlignment = HorizontalAlignment.Right;
                    Grid.SetColumn(tb, 0);
                    

                    inputTextBox.Width = 200;
                    inputTextBox.Height = 23;
                    inputTextBox.FontSize = 14;
                    inputTextBox.FontFamily = new FontFamily("Century Gothic");
                    inputTextBox.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(inputTextBox, 1);

                    newGrid.Children.Add(tb);
                    newGrid.Children.Add(inputTextBox);

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
                    Grid.SetRow(inputTextBox, newGrid.RowDefinitions.IndexOf(contentRowDefinition));
                }
                
            }

            
            addWindow.DummyWindow_MainGrid.Children.Add(newGrid);
            addWindow.SizeToContent = SizeToContent.Width;
            addWindow.SizeToContent = SizeToContent.Height;
            //addWindow.Width += 30;
            //addWindow.Height += 30;
            addWindow.ResizeMode = ResizeMode.NoResize;

        }

        public MainWindow()
        {
            InitializeComponent();

            //Initialize the menu list made of pictures, then set the handlers by using the set_handlers method, see method
            //for more info.
            ImageMenuList = new List<Image> { Automation_Button, Home_Button, Monitoring_Button, Queues_Button, Assets_Button, Settings_Button };
            set_handlers(ImageMenuList, Image_Button_Click, Image_Button_MouseEnter, Image_Button_MouseLeave);

            //Create all sub-menu buttons
            foreach (var element in SubMenuButtonNamesList)
            {
                Button NewButton = new Button
                {
                    Background = new SolidColorBrush(new Color { A = 100, R = 231, G = 233, B = 245 }),
                    FontFamily = new FontFamily("Century Gothic"),
                    Content = element,
                    FontSize = 14,
                    Height = 30,
                    Width = Double.NaN,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Name = element.Trim()
                };

                try
                {   //This code adds handlers based on the name of the methods
                    //I wrote this because I was lazy, it uses reflection so it's probably not a good idea
                    //but since it only triggers once per run I don't think it can do that much damage.
                    MethodInfo method = typeof(MainWindow).GetMethod(NewButton.Name + "_Method");
                    Delegate myDelegate = Delegate.CreateDelegate(typeof(RoutedEventHandler), this, method);
                    NewButton.Click += (RoutedEventHandler)myDelegate;
                }
                catch (Exception e)
                {
                    //MessageBox.Show("error");
                }

                SubMenuButtonsList.Add(NewButton);
            }

            //Create Automation main menu
            Automation_StackPanel = CreateMainMenu(Automation_ButtonsList);
            Automation_StackPanel.Name = "Automation";

            //Contain Main Menu panels in here
            MainMenuStackPanels.Add(Automation_StackPanel);
        }

        private StackPanel CreateMainMenu(List<String> nameList)
        {
            StackPanel menuPanel = new StackPanel() { Orientation = Orientation.Horizontal };


            foreach (var element in nameList)
            {
                Button myButton = new Button()
                {
                    BorderBrush = new SolidColorBrush(Colors.White),
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = new SolidColorBrush(Colors.DarkOrange),
                    FontFamily = new FontFamily("Century Gothic"),
                    Content = element,
                    FontSize = 14,
                    Height = 30,
                    Width = Double.NaN,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                myButton.Click -= Menu_Handlers;
                myButton.Click += Menu_Handlers;

                myButton.Name = element.Trim();
                menuPanel.Children.Add(myButton);
                menuPanel.Children.Add(new Separator() { Width = 10, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0) });
            }

            menuPanel.HorizontalAlignment = HorizontalAlignment.Left;
            menuPanel.VerticalAlignment = VerticalAlignment.Top;
            menuPanel.Margin = new Thickness(50, 50, 0, 0);
            return menuPanel;
        }

        private void Menu_Handlers(object sender, RoutedEventArgs e)
        {
            String Sender_Name = ((Button)sender).Content.ToString().ToLower().Trim();

            DataTable mySQLTables = SQL_Load_DatabaseTables("Server=localhost;User ID=root;Database=straussdatabase");
            List<String> tableList = new List<String>();

            //Get all tables that match name with the sender_name
            foreach (DataRow item in mySQLTables.Rows)
            {
                if (item.ItemArray[0].ToString().Contains(Sender_Name))
                {
                    tableList.Add(item.ItemArray[0].ToString());
                }
            }

            //If any match, then create conditional tables
            if (tableList.Contains(Sender_Name + "_list") && tableList.Contains(Sender_Name + "_attachment"))
            {
                CreateTable("Server=localhost;User ID=root;Database=straussdatabase", tableList[tableList.IndexOf(Sender_Name + "_list")], tableList[tableList.IndexOf(Sender_Name + "_attachment")], 0, 10);
            }
            else if (tableList.Contains(Sender_Name + "_list") && tableList.Contains(Sender_Name + "_attachment") == false)
            {
                CreateTable("Server=localhost;User ID=root;Database=straussdatabase", tableList[tableList.IndexOf(Sender_Name + "_list")], null, 0, 10);
            }

            //Create submenu for the table based on sender_name
            SubMenuStackPanel.Children.Clear();
            SubMenuStackPanel = new() { Orientation = Orientation.Horizontal };
            DataTable subMenuItems = SQL_Load_SubMenuItems("Server=localhost;User ID=root;Database=straussdatabase", Sender_Name);

            MainGrid.Children.Remove(SubMenuStackPanel);

            foreach (Button SubMenuItem in SubMenuButtonsList)
            {
                foreach (DataRow Item in subMenuItems.Rows)
                {
                    if (Item.ItemArray[0].ToString().Contains(SubMenuItem.Name))
                    {

                        SubMenuStackPanel.Children.Add(SubMenuItem);
                        SubMenuStackPanel.Children.Add(new Separator()
                            { Width = 10, Background = new SolidColorBrush(Colors.White) });
                    }
                }
            }

            Thickness tableThickness = Table.Margin;
            SubMenuStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            SubMenuStackPanel.VerticalAlignment = VerticalAlignment.Top;
            SubMenuStackPanel.Margin = new Thickness(tableThickness.Left, tableThickness.Top - 50, 0, 0);

            MainGrid.Children.Add(SubMenuStackPanel);



        }
        private void set_handlers(List<Image> myImages, MouseButtonEventHandler clickEventHandler, MouseEventHandler mouseEnterEventHandler, MouseEventHandler mouseLeaveEventHandler)
        {
            foreach (Image element in myImages)
            {
                element.MouseLeftButtonDown += clickEventHandler;
                element.MouseEnter += mouseEnterEventHandler;
                element.MouseLeave += mouseLeaveEventHandler;
            }

        }
        private void Image_Button_MouseEnter(object sender, MouseEventArgs e)
        {
            //Uses naming conventions to dynamically assign bitmapimages based on event types
            string name = ((Image)sender).Name.Split("_")[0];

            ((Image)sender).Source = new BitmapImage(new Uri(name + "_Images/highlight_" + name + ".png", UriKind.Relative));

            if (((Image)sender).Tag == "IsSelected")
            {
                ((Image)sender).Source = new BitmapImage(new Uri(name + "_Images/highlight_" + name + "_selected.png", UriKind.Relative));
            }
        }
        private void Image_Button_MouseLeave(object sender, MouseEventArgs e)
        {
            //Uses naming conventions to dynamically assign bitmapimages based on event types

            string name = ((Image)sender).Name.Split("_")[0];

            ((Image)sender).Source = new BitmapImage(new Uri(name + "_Images/no_highlight_" + name + ".png", UriKind.Relative));

            if (((Image)sender).Tag == "IsSelected")
            {
                ((Image)sender).Source = new BitmapImage(new Uri(name + "_Images/no_highlight_" + name + "_selected.png", UriKind.Relative));
            }

        }
        private void Image_Button_Click(object sender, MouseButtonEventArgs e)
        {
            //Uses naming conventions to dynamically assign bitmapimages based on event types
            //It does get even more bizzare the more you look into this code.
            //Try not to stare into it because it will stare back.
            string name = ((Image)sender).Name;

            foreach (var image in ImageMenuList)
            {
                if (image.Name == name)
                {

                    //Add Main Menu for Category
                    image.Source = new BitmapImage(new Uri(name.Split("_")[0] + "_Images/highlight_" + name.Split("_")[0] + "_selected.png", UriKind.Relative));
                    image.Tag = "IsSelected";

                    //Dynamically assign grid based on button name by using naming conventions.
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(MainGrid); i++)
                    {
                        Visual childVisual = (Visual)VisualTreeHelper.GetChild(MainGrid, i);

                        if (childVisual.GetType() == typeof(Grid))
                        {
                            Grid myGrid = (Grid)childVisual;
                            if (myGrid.Name.Contains(name.Split("_")[0] + "_Grid"))
                            {
                                foreach (StackPanel stackPanel in MainMenuStackPanels)
                                {
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
                    image.Source = new BitmapImage(new Uri(image.Name.Split("_")[0] + "_Images/no_highlight_" + image.Name.Split("_")[0] + ".png", UriKind.Relative));
                    image.Tag = "IsNotSelected";
                }
            }

        }
        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {

            //"Server=localhost;User ID=root;Database=straussdatabase"

        }

        public void CreateTable(String myConnectionString, String TableName, [Optional] String AttachmentTableName, int StartIndex, int EndIndex)
        {
            MainGrid.Children.Remove(Table);
            Table = new SQLTable
            {
                SqlConnectionString = myConnectionString
            };

            if (AttachmentTableName == null)
            {
                Table.CreateGrid(Table.SQLTable_Load(TableName, StartIndex, EndIndex), StartIndex, EndIndex,
                    null);
            }
            else
            {
                Table.CreateGrid(Table.SQLTable_Load(TableName, StartIndex, EndIndex), StartIndex, EndIndex,
                    Table.SQLTable_Load(AttachmentTableName, StartIndex, EndIndex));
            }
            MainGrid.Children.Add(Table);

        }
        public DataTable SQL_Load_DatabaseTables(string SqlConnectionString)
        {
            DataTable table = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(SqlConnectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SHOW TABLES;", connection))
                {
                    connection.Open();
                    table.Load(command.ExecuteReader());
                    connection.Close();
                }
            }

            return table;
        }

        public DataTable SQL_Load_SubMenuItems(string SqlConnectionString, string columnName)
        {

            //This function is used to load settings for the MainMenu -> SubMenu items.
            DataTable table = new DataTable();
            using (MySqlConnection connection = new MySqlConnection(SqlConnectionString))
            {
                using (MySqlCommand command = new MySqlCommand("SELECT " + columnName + " FROM category_submenu", connection))
                {
                    connection.Open();
                    table.Load(command.ExecuteReader());
                    connection.Close();
                }
            }

            return table;
        }

    }
}