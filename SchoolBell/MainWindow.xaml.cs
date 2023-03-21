using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;
using System.Windows.Documents;
using System.Media;
using System.Windows.Shapes;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace SchoolBell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string filePath = Assembly.GetExecutingAssembly().Location;
        private static string projectPath = filePath.Replace(@"SchoolBellWPF\SchoolBell\SchoolBell\bin\Debug\net6.0-windows\SchoolBell.dll", "");
        private static Dictionary<int, string[]> listSortedContent; // 0 - playtime 1-song 2-duracation
        private bool doesWork;
        private AddWindow addWin;
        private SettingsWindow setWindow;
        private SongWindow songsWindow;
        private ListBox listBox;
        private Button btnWork;
        private Button btnAdd;
        private Button btnSongs;
        private Button btnDelete;
        private Button btnRandom;
        private Button btnSettings;
        private TextBlock btnWorkTitle = new TextBlock
        {
            HorizontalAlignment= HorizontalAlignment.Center,
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
        private TextBlock secondLineW = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        private TextBlock firstLineRNDM = new TextBlock 
        {
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        private TextBlock secondLineRNDM = new TextBlock 
        {
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        public List<string> lbContentList = new List<string>();
        private string lng = "Eng";

        public MainWindow()
        {
            InitializeComponent();

            btnWork = (Button)FindName("buttonWork");
            listBox = lBox;
            listBox.ItemsSource = lbContentList;
            btnWork = buttonWork;
            btnAdd = buttonAdd;
            btnDelete = buttonDelete;
            btnRandom = buttonRandom;
            btnSettings = buttonSettings;
            btnWorkTitle = buttonWorkTitle;
            btnSongs = buttonSongs;
            onLoad();
            SetFont();
        }
        private void SetFont()
        {
            PrivateFontCollection privateFontCollection = new PrivateFontCollection();
            byte[] fontData = Properties.Resources.Inconsolata; // replace with the name of your font resource
            IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            privateFontCollection.AddMemoryFont(fontPtr, fontData.Length);
            listBox.FontFamily = new System.Windows.Media.FontFamily(privateFontCollection.Families[0].Name);
            listBox.FontSize = 15;
        }
        private static void CheckAutoStart()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string[] valueNames = key.GetValueNames();
            bool isSet = false;
            foreach (string valueName in valueNames)
            {
                if (valueName == "SchoolBellRunner")
                {
                    isSet= true;
                }
            }
            if (isSet == false)
            {
                Process process = new Process();
                process.StartInfo.FileName = projectPath + @"Autostart\Autostart\bin\Debug\net6.0-windows\Autostart.exe";
                process.Start();
                Process process2 = new Process();
                process2.StartInfo.FileName = projectPath + @"Runner\Runner\bin\Debug\net6.0-windows\Runner.exe";
                process2.Start();
                MessageBox.Show("Hi!\nIt's first time when you open SchoolBell app.\nIf you want to know how to work with it\nthen go to the project folder and there\nyou will see a documentation.");
            }
        }

        private void onLoad()
        {
            CheckAutoStart();
            listSortedContent = new Dictionary<int, string[]>();
            string musicFileTxt = projectPath + "MusicDictionary.txt";
            string[] lineContant;
            int itemPlace = 0;
            string nline;
            try
            {
                foreach (string line in System.IO.File.ReadLines(musicFileTxt))
                {
                    nline = line.Replace("\n", "");
                    lineContant = nline.Split(','); // lineContant[0] - playtime,lineContant[1] - song, lineContant[2] - play duracation
                    listSortedContent[itemPlace] = new string[] { lineContant[0], lineContant[1], lineContant[2] };
                    itemPlace++;
                }
            }
            catch (IndexOutOfRangeException ex) { MessageBox.Show(Convert.ToString(ex)); }
            SetListBox();
            SetStartButton();
            switch (lng)
            {
                case "Eng":
                    SetEng();
                    break;
                case "Est":
                    SetEst();
                    break;
                case "Rus":
                    SetRus();
                    break;
            }
        }

        ////// Button events
        private void btnWork_Click(object sender, RoutedEventArgs e)
        {
            string[] content = System.IO.File.ReadAllText(projectPath + "config.txt").Trim().Split(",");
            bool isTrue = Convert.ToBoolean(content[0]);
            doesWork = isTrue ? false:true;
            switch (lng)
            {
                case "Eng":
                    btnWorkTitle.Text = isTrue ? "Programm is stopped": "Programm is working";
                    break;
                case "Est":
                    btnWorkTitle.Text = isTrue ? "Programm ootab": "Programm töötab";
                    break;
                case "Rus":
                    btnWorkTitle.Text = isTrue ? "Программа остановленна" : "Программа работает";
                    break;
            }
            System.IO.File.WriteAllText(projectPath + "config.txt", isTrue ? "false," + lng: "true," + lng);
        }
        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pair in listSortedContent)
            {
                listSortedContent[pair.Key][1] = "Random";
            }
            writeToFile();
            SetListBox();
        }
        private void btnDel_Click(object sender, EventArgs e)
        {
            int curItemIndex = lBox.SelectedIndex;
            if (curItemIndex > 1)
            {
                listSortedContent.Remove(curItemIndex - 2);
                writeToFile();
                SetListBox();
            }
            else if (curItemIndex != -1) { MessageBox.Show("Don't delete that please"); }
            else { MessageBox.Show("Please choose an object to delete"); }
        }

        public void btnAdd_Click(object sender, EventArgs e)
        {
            Button addBtn= new Button();
            addBtn.Click += addWindowAdd_Click;
            addWin = new AddWindow(addBtn,lng);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Button btnApply = new Button();
            btnApply.Click += btnApply_Settings;
            setWindow = new SettingsWindow(btnApply, lng);
        }

        private void btnSongs_Click(object sender, RoutedEventArgs e)
        {
            songsWindow = new SongWindow(lng);
        }

        private void btnApply_Settings(object sender, RoutedEventArgs e)
        {
            switch (setWindow.GetLng())
            {
                case "Eng":
                    SetEng();
                    setWindow.SetEng();
                    break;
                case "Est":
                    SetEst();
                    setWindow.SetEst();
                    break;
                case "Rus":
                    SetRus();
                    setWindow.SetRus();
                    break;
            }
            writeLng();
            SetListBox();
        }

        private void SetEng()
        {
            lng = "Eng";
            btnAdd.Content = new TextBlock
            {
                Text = "Add",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnDelete.Content = new TextBlock
            {
                Text = "Delete",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnSongs.Content = new TextBlock 
            {
                Text = "Songs",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            firstLineRNDM.Text = "Make all";
            secondLineRNDM.Text = "Random";
            ((StackPanel)buttonRandom.Content).Children.Clear();
            ((StackPanel)buttonRandom.Content).Children.Add(firstLineRNDM);
            ((StackPanel)buttonRandom.Content).Children.Add(secondLineRNDM);
            btnSettings.Content = new TextBlock
            {
                Text = "Settings",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            if(doesWork) { btnWorkTitle.Text = "Programm is working"; }
            else { btnWorkTitle.Text = "Programm is stopped"; }
            secondLineW.Text = "(press to change)";
            ((StackPanel)buttonWork.Content).Children.Clear();
            ((StackPanel)buttonWork.Content).Children.Add(btnWorkTitle);
            ((StackPanel)buttonWork.Content).Children.Add(secondLineW);
        }
        private void SetEst()
        {
            lng = "Est";
            btnAdd.Content = new TextBlock
            {
                Text = "Lisada",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnDelete.Content = new TextBlock
            {
                Text = "Eemalda",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnSongs.Content = new TextBlock
            {
                Text = "Laulud",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            firstLineRNDM.Text = "Teha kõik";
            secondLineRNDM.Text = "juhuslikuks";
            ((StackPanel)buttonRandom.Content).Children.Clear();
            ((StackPanel)buttonRandom.Content).Children.Add(firstLineRNDM);
            ((StackPanel)buttonRandom.Content).Children.Add(secondLineRNDM);
            btnSettings.Content = new TextBlock
            {
                Text = "Seaded",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            if (doesWork) { btnWorkTitle.Text = "Programm töötab"; }
            else { btnWorkTitle.Text = "Programm ootab"; }
            secondLineW.Text = "(vajata, et vahetada)";
            ((StackPanel)buttonWork.Content).Children.Clear();
            ((StackPanel)buttonWork.Content).Children.Add(btnWorkTitle);
            ((StackPanel)buttonWork.Content).Children.Add(secondLineW);
        }
        private void SetRus()
        {
            lng = "Rus";
            btnAdd.Content = new TextBlock
            {
                Text = "Добавить" ,
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnDelete.Content = new TextBlock 
            { 
                Text = "Удалить" ,
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            btnSongs.Content = new TextBlock
            {
                Text = "Музыка",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            firstLineRNDM.Text = "Сделать все";
            secondLineRNDM.Text = "рандомно";
            ((StackPanel)buttonRandom.Content).Children.Clear();
            ((StackPanel)buttonRandom.Content).Children.Add(firstLineRNDM);
            ((StackPanel)buttonRandom.Content).Children.Add(secondLineRNDM);
            btnSettings.Content = new TextBlock
            {
                Text = "Настройки",
                FontWeight = FontWeights.Bold,
                FontSize = 15
            };
            if (doesWork) { btnWorkTitle.Text = "Программа работает"; }
            else { btnWorkTitle.Text = "Программа остановленна"; }
            secondLineW.Text = "(нажмите для изменения)";
            ((StackPanel)buttonWork.Content).Children.Clear();
            ((StackPanel)buttonWork.Content).Children.Add(btnWorkTitle);
            ((StackPanel)buttonWork.Content).Children.Add(secondLineW);
        }
        

        private void SetStartButton()
        {
            StreamReader sr = new StreamReader(projectPath + "config.txt");
            string line = sr.ReadLine();
            string[] config = line.Split(",");
            lng = config[1];
            doesWork = Convert.ToBoolean(config[0]);
            if (doesWork == true) { btnWorkTitle.Text = "Programm is working"; }
            else { btnWorkTitle.Text = "Programm is stopped";}
            sr.Close();
        }

        public void SetListBox()
        {
            lbContentList.Clear();
            switch (lng)
            {
                case "Eng":
                    lbContentList.Add("Time  |\t\t           Music\t\t|  Duration");
                    lbContentList.Add("-------------------------------------------------------------");
                    break;
                case "Est":
                    lbContentList.Add("Aeg   |\t\t          Muusika\t\t|  Pikkus");
                    lbContentList.Add("-------------------------------------------------------------");
                    break;
                case "Rus":
                    lbContentList.Add("Время |\t\t          Музыка\t\t|  Длинна");
                    lbContentList.Add("-------------------------------------------------------------");
                    break;
            }


            foreach (var pair in listSortedContent) 
            {
                lbContentList.Add(listBoxLine(pair.Value)); 
            }

            listBox.ItemsSource = null;
            listBox.ItemsSource = lbContentList;
        }

        public void writeToFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter(projectPath + "MusicDictionary.txt");
                foreach (var pair in listSortedContent)
                {
                    sw.Write(getHourMinute(DateTime.Parse(pair.Value[0])) + "," + pair.Value[1] + "," + pair.Value[2].ToString() + "\n");
                }
                sw.Close();
            }
            catch (Exception ds) { MessageBox.Show(ds.ToString()); }
        }
        private void writeLng()
        {
            StreamWriter sw = new StreamWriter(projectPath + "config.txt");
            sw.Write(doesWork.ToString().ToLower()+ "," + lng);
            sw.Close();
        }

        public string listBoxLine(string[] arr)
        {
            string songInscription = arr[1].Length < 39 ? arr[1].PadRight(39) : arr[1].Substring(39);
            return arr[0] + " |  " + songInscription + "|  " + arr[2] + "s";
        }
        public string getHourMinute(DateTime time)
        {
            string hour = time.Hour.ToString().Length == 1 ? "0" + time.Hour.ToString() : time.Hour.ToString();
            string minute = time.Minute.ToString().Length == 1 ? "0" + time.Minute.ToString() : time.Minute.ToString();
            return hour + ":" + minute;
        }

        Dictionary<int, string[]> OrderListBoxDict(Dictionary<int, string[]> dict, int newKey, string[] data)
        {
            Dictionary<int, string[]> newDict = new Dictionary<int, string[]>();
            if (dict.Keys.Count - 1 < newKey)
            {
                newDict = dict;
                newDict[newKey] = data;
            }
            else
            {
                foreach (var kvp in dict)
                {
                    if (kvp.Key == newKey)
                    {
                        newDict[newKey] = data;
                        newDict[newKey + 1] = kvp.Value;
                    }
                    else if (kvp.Key > newKey)
                    {
                        newDict[kvp.Key + 1] = kvp.Value;
                    }
                    else if (kvp.Key < newKey)
                    {
                        newDict[kvp.Key] = kvp.Value;
                    }
                }
            }
            return newDict;
        }
        private void AddToListBox(string path, DateTime time, int duracation)
        {
            /// Changing values in musicDict,listConten + Adding to listbox1
            string strTime = getHourMinute(time);
            if (listSortedContent.Keys.Count != 0)
            {
                foreach (var pair in listSortedContent)
                {
                    if (DateTime.Parse(pair.Value[0]) == time)
                    {
                        listSortedContent[pair.Key] = new string[] { strTime, path, duracation.ToString() };
                        break;
                    }
                    else if (DateTime.Parse(pair.Value[0]) < time)
                    {
                        try
                        {
                            if (DateTime.Parse(listSortedContent[(pair.Key + 1)][0]) > time)
                            {
                                listSortedContent = OrderListBoxDict(listSortedContent, pair.Key + 1, new string[] { strTime, path, duracation.ToString() });
                                break;  
                            }
                        }
                        catch (KeyNotFoundException kexp)
                        {
                            listSortedContent = OrderListBoxDict(listSortedContent, pair.Key + 1, new string[] { strTime, path, duracation.ToString() });
                            break;
                        }
                    }
                    else if (DateTime.Parse(pair.Value[0]) > time)
                    {
                        listSortedContent = OrderListBoxDict(listSortedContent, 0, new string[] { strTime, path, duracation.ToString() });
                        break;
                    }
                }
            }
            else
            {
                listSortedContent[0] = new string[] { strTime, path, duracation.ToString() };
            }
            SetListBox();
            writeToFile();
        }


        public void addWindowAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddToListBox(addWin.GetMusic(), DateTime.Parse(addWin.GetTime()), Convert.ToInt32(addWin.GetDuracation()));
            }
            catch (FormatException fex) { MessageBox.Show("Please check your time or duracation again!"); }
            catch (NullReferenceException nrex) { MessageBox.Show("Please choose a song!"); }
            catch (Exception a) { MessageBox.Show(Convert.ToString(a)); }

            addWin.FormClose();
        }

    }

    public class AddWindow
    {
        public static Window addWindow;
        public static Grid grid;
        public static TextBox ptimeBox;
        public static TextBox durBox;
        public static Label lbSong;
        public static Label lbPlayTime;
        public static Label lbDuracation;
        public static Label lbS;
        public static Button btnAdd;
        public static Button btnSongAdd;
        public static ComboBox comboBox;
        private string lang;
        public AddWindow(Button _btnAdd, string _lng)
        {
            SetView(_btnAdd);
            switch (_lng)
            {
                case "Eng":
                    SetEng();
                    break;
                case "Est":
                    SetEst();
                    break;
                case "Rus":
                    SetRus();
                    break;
            }
        }
        public void SetView(Button _btnAdd)
        {
            addWindow = new Window
            {
                Height = 200,
                Width = 450,
            };

            grid = new Grid();
            // Define the column and row definitions
            for (int i = 0; i < 8; i++) { grid.ColumnDefinitions.Add(new ColumnDefinition()); }
            for (int i = 0; i < 4; i++) { grid.RowDefinitions.Add(new RowDefinition()); }


            comboBox = new ComboBox() { Margin = new Thickness(40, 29, 44, 28) };
            Grid.SetColumn(comboBox, 1);
            Grid.SetColumnSpan(comboBox, 5);
            Grid.SetRowSpan(comboBox, 2);

            lbSong = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 34,
                FontSize = 16
            };
            Grid.SetRowSpan(lbSong, 2);
            Grid.SetColumnSpan(lbSong, 2);

            lbPlayTime = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = 16
            };


            lbDuracation = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 33,
                FontSize = 16
            };
            Grid.SetColumn(lbDuracation, 4);
            Grid.SetColumnSpan(lbDuracation, 5);
            Grid.SetRowSpan(lbDuracation, 2);
            Grid.SetRow(lbDuracation, 1);

            lbS = new Label()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 25,
                Height = 33,
                FontSize = 16
            };
            Grid.SetColumn(lbS, 6);
            Grid.SetRowSpan(lbS, 3);
            Grid.SetRow(lbS, 1);
            Grid.SetColumnSpan(lbS, 2);

            durBox = new TextBox()
            {
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 36,
                Height = 24,
                TextWrapping = TextWrapping.Wrap,
                Text = "15",
                FontSize = 15
            };
            Grid.SetColumn(durBox, 6);
            Grid.SetRow(durBox, 2);

            ptimeBox = new TextBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 128,
                Height = 24,
                TextWrapping = TextWrapping.Wrap,
                Text = DateTime.Now.ToString("HH:mm"),
                FontSize = 15,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(ptimeBox, 1);
            Grid.SetColumnSpan(ptimeBox, 4);
            Grid.SetRow(ptimeBox, 2);

            btnAdd = _btnAdd;
            btnAdd.HorizontalAlignment = HorizontalAlignment.Center;
            btnAdd.VerticalAlignment = VerticalAlignment.Top;
            btnAdd.Width = 128;
            btnAdd.Height = 30;
            btnAdd.Margin = new Thickness(0, 38, 0, 0);
            btnAdd.FontSize = 16;
            Grid.SetColumn(btnAdd, 2);
            Grid.SetColumnSpan(btnAdd, 4);
            Grid.SetRow(btnAdd, 2);
            Grid.SetRowSpan(btnAdd, 2);
            
            grid.Children.Add(comboBox);
            grid.Children.Add(ptimeBox);
            grid.Children.Add(durBox);
            grid.Children.Add(lbSong);
            grid.Children.Add(lbS);
            grid.Children.Add(lbDuracation);
            grid.Children.Add(lbPlayTime);
            grid.Children.Add(btnAdd);
            
            addWindow.Content = grid;
            addWindow.Show();

            SetComboBox();
        }

        public string GetMusic() => comboBox.SelectedItem.ToString();
        public string GetTime() => ptimeBox.Text.Replace(" ", "");
        public string GetDuracation() => durBox.Text.Replace(" ", "");
        public void FormClose() => addWindow.Close();

        public static void SetComboBox()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            string musicFolder = filePath.Replace(@"SchoolBellWPF\SchoolBell\SchoolBell\bin\Debug\net6.0-windows\SchoolBell.dll", "") + @"MusicFiles";
            string[] musicFiles = Directory.GetFiles(musicFolder);
            comboBox.Items.Add("Random");
            foreach (var path in musicFiles)
            {
                comboBox.Items.Add(System.IO.Path.GetFileName(path).Replace(".wav", ""));
            }
        }

        private void SetEng()
        {
            addWindow.Title = "Add Time";
            lbSong.Content = "Music -";
            lbSong.Margin = new Thickness(30, 24, 0, 0);
            lbPlayTime.Content = "PlayTime -";
            lbPlayTime.Margin = new Thickness(10, 4, 0, 13);
            lbPlayTime.Width = 85;
            lbPlayTime.Height = 33;
            Grid.SetColumnSpan(lbPlayTime, 2);
            Grid.SetRowSpan(lbPlayTime, 2);
            Grid.SetRow(lbPlayTime, 1);
            lbDuracation.Content = "Duracation -";
            lbDuracation.Width = 98;
            lbDuracation.Margin = new Thickness(20, 33, 0, 0);
            durBox.Margin = new Thickness(5, 0, 12, 0);
            Grid.SetColumnSpan(durBox, 1);
            lbS.Content = "s";
            lbS.Margin = new Thickness(38, 33, 0, 0);
            ptimeBox.Margin = new Thickness(40, 0, 0, 0);
            btnAdd.Content = "Add";
        }
        private void SetEst()
        {
            addWindow.Title = "Lisa aega";
            lbSong.Content = "Muusika -";
            lbSong.Margin = new Thickness(10, 24, 0, 0);
            lbPlayTime.Content = "Mänguaeg -";
            lbPlayTime.Margin = new Thickness(10, 4, 0, 13);
            lbPlayTime.Width = 95;
            lbPlayTime.Height = 33;
            Grid.SetColumnSpan(lbPlayTime, 2);
            Grid.SetRowSpan(lbPlayTime, 2);
            Grid.SetRow(lbPlayTime, 1);
            lbDuracation.Content = "Kestus -";
            lbDuracation.Width = 98;
            lbDuracation.Margin = new Thickness(45, 33, 0, 0);
            durBox.Margin = new Thickness(5, 0, 12, 0);
            Grid.SetColumnSpan(durBox, 1);
            lbS.Content = "s";
            lbS.Margin = new Thickness(40, 33, 0, 0);
            ptimeBox.Margin = new Thickness(60, 0, 0, 0);
            btnAdd.Content = "Lisada";
        }
        private void SetRus()
        {
            addWindow.Title = "Добавить время";
            lbSong.Content = "Музыка -";
            lbSong.Margin = new Thickness(10, 24, 0, 0);
            lbPlayTime.Content = "Время -\nпроигрывания";
            lbPlayTime.Margin = new Thickness(10, 4, 0, 33);
            lbPlayTime.Width = 120;
            lbPlayTime.Height = 53;
            Grid.SetColumnSpan(lbPlayTime, 3);
            Grid.SetRowSpan(lbPlayTime, 2);
            Grid.SetRow(lbPlayTime, 2);
            lbDuracation.Content = "Длительность -";
            lbDuracation.Width = 168;
            lbDuracation.Margin = new Thickness(20, 35, 0, 0);
            durBox.Margin = new Thickness(30, 0, 0, 0);
            Grid.SetColumnSpan(durBox, 2);
            lbS.Content = "с";
            lbS.Margin = new Thickness(65, 35, 0, 0);
            ptimeBox.Margin = new Thickness(40, 0, 0, 0);
            btnAdd.Content = "Добавить";
        }
    }  
    
    public class SettingsWindow
    {
        public Window settingsWindow;
        public ComboBox comboBox;
        public Label label;
        public TextBlock textBlock;
        public Button btnApply;
        public Hyperlink hyperlink;

        public SettingsWindow(Button _btn, string _lng)
        {
            SetView(_btn);
            switch (_lng)
            {
                case "Eng":
                    SetEng();
                    break;
                case "Est":
                    SetEst();
                    break;
                case "Rus":
                    SetRus();
                    break;
            }
        }

        public void SetView(Button _btnApply)
        {
            settingsWindow = new Window();
            Grid grid = new Grid();

            label = new Label()
            {
                Margin = new Thickness(10, 10, 0, 0),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };

            comboBox = new ComboBox() 
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 54,
                Height = 26
            };
            ComboBoxItem cbitemEng = new ComboBoxItem() { Content = "Eng" };
            ComboBoxItem cbitemEst = new ComboBoxItem() { Content = "Est" };
            ComboBoxItem cbitemRus = new ComboBoxItem() {Content = "Rus"};
            comboBox.Items.Add(cbitemEng);
            comboBox.Items.Add(cbitemEst);
            comboBox.Items.Add(cbitemRus);

            textBlock = new TextBlock() 
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(15, 53, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap,

            };
            hyperlink = new Hyperlink()
            {
                NavigateUri = new System.Uri("https://github.com/VoiDd423/SchoolBell")
            };
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            textBlock.Inlines.Add(hyperlink);

            btnApply = _btnApply;
            btnApply.HorizontalAlignment = HorizontalAlignment.Left;
            btnApply.VerticalAlignment = VerticalAlignment.Top;
            btnApply.Height = 20;
            btnApply.Width = 43;
            
            grid.Children.Add(label);
            grid.Children.Add(textBlock);
            grid.Children.Add(comboBox);
            grid.Children.Add(btnApply);

            settingsWindow.Content = grid;
            settingsWindow.Show();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe", e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch(Exception a) { MessageBox.Show(a.ToString()); }
        }

        public string GetLng() => comboBox.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");

        public void SetEng()
        {
            settingsWindow.Height = 130;
            settingsWindow.Width = 240;
            settingsWindow.Title = "Settings";
            hyperlink.Inlines.Clear();
            hyperlink.Inlines.Add("Project on GitHub");
            btnApply.Content = "Apply";
            btnApply.Margin = new Thickness(176, 18, 0, 0);
            label.Content = "Language -";
            comboBox.Margin = new Thickness(109, 15, 0, 0);
        }

        public void SetEst()
        {
            settingsWindow.Height = 130;
            settingsWindow.Width = 200;
            settingsWindow.Title = "Seaded";
            hyperlink.Inlines.Clear();
            hyperlink.Inlines.Add("Proekt GitHub-is");
            btnApply.Content = "Säästa";
            btnApply.Margin = new Thickness(130, 18, 0, 0);
            label.Content = "Keel -";
            comboBox.Margin = new Thickness(65, 15, 0, 0);
        }

        public void SetRus()
        {
            settingsWindow.Height = 130;
            settingsWindow.Width = 210;
            settingsWindow.Title = "Настройки";
            hyperlink.Inlines.Clear();
            hyperlink.Inlines.Add("Проект на GitHub");
            btnApply.Content = "Сохр";
            btnApply.Margin = new Thickness(136, 18, 0, 0);
            label.Content = "Язык -";
            comboBox.Margin = new Thickness(75, 15, 0, 0);
        }
    }
    class SongWindow
    {
        private static string filePath = Assembly.GetExecutingAssembly().Location;
        private static string projectPath = filePath.Replace(@"SchoolBellWPF\SchoolBell\SchoolBell\bin\Debug\net6.0-windows\SchoolBell.dll", "");
        private static Window songsWindow;
        private static Button btnListen;
        private static ListBox lbSongs;
        private static Button btnAddSong;
        private static Button btnStop;
        private SoundPlayer sp;

        public SongWindow(string _lng)
        {
            SetView();
            switch (_lng)
            {
                case "Eng":
                    SetEng();
                    break;
                case "Est":
                    SetEst();
                    break;
                case "Rus":
                    SetRus();
                    break;
            }
            SetListBox();
        }

        private void SetView()
        {
            songsWindow = new Window
            {
                Height = 411,
                Width = 362
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(4, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(17, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(21, GridUnitType.Star) });

            // Create the list box
            lbSongs = new ListBox();
            lbSongs.Name = "lbSongs";
            Grid.SetColumnSpan(lbSongs, 4);
            lbSongs.Margin = new Thickness(10, 10, 10, 29);
            Grid.SetRowSpan(lbSongs, 7);
            grid.Children.Add(lbSongs);

            // Create the "Listen" button
            btnListen = new Button();
            btnListen.HorizontalAlignment = HorizontalAlignment.Left;
            btnListen.VerticalAlignment = VerticalAlignment.Top;
            btnListen.Margin = new Thickness(10, 44, 0, 0);
            btnListen.Height = 59;
            btnListen.Width = 105;
            btnListen.Click += btnListen_Click;
            btnListen.FontWeight = FontWeights.Bold;
            Grid.SetColumnSpan(btnListen, 2);
            Grid.SetRow(btnListen, 6);
            Grid.SetRowSpan(btnListen, 2);
            grid.Children.Add(btnListen);

            // Create the "Add song" button
            btnAddSong = new Button();
            btnAddSong.HorizontalAlignment = HorizontalAlignment.Left;
            btnAddSong.VerticalAlignment = VerticalAlignment.Top;
            btnAddSong.Margin = new Thickness(66, 44, 0, 0);
            btnAddSong.Height = 59;
            btnAddSong.Width = 105;
            btnAddSong.Click += btnAddSong_Click;
            btnAddSong.FontWeight = FontWeights.Bold;
            Grid.SetColumnSpan(btnAddSong, 2);
            Grid.SetRow(btnAddSong, 6);
            Grid.SetRowSpan(btnAddSong, 2);
            Grid.SetColumn(btnAddSong, 2);
            grid.Children.Add(btnAddSong);

            // Create the "Stop" button
            btnStop = new Button();
            btnStop.HorizontalAlignment = HorizontalAlignment.Center;
            btnStop.VerticalAlignment = VerticalAlignment.Top;
            btnStop.Margin = new Thickness(8, 44, 0, 0);
            btnStop.Height = 59;
            btnStop.Width = 106;
            btnStop.Click += btnStop_Click;
            btnStop.FontWeight= FontWeights.Bold;
            Grid.SetColumnSpan(btnStop, 2);
            Grid.SetRow(btnStop, 6);
            Grid.SetRowSpan(btnStop, 2);
            Grid.SetColumn(btnStop, 1);
            grid.Children.Add(btnStop);

            songsWindow.Content = grid;
            songsWindow.Show();
        }
        private static void btnAddSong_Click(object sender, RoutedEventArgs e)
        {
            string savePath;
            OpenFileDialog file = new OpenFileDialog();
            file.ShowDialog();
            string extencion = System.IO.Path.GetExtension(file.FileName);
            if (extencion == ".mp3" || extencion == ".wav")
            {
                try
                {
                    savePath = projectPath + @"MusicFiles\" + System.IO.Path.GetFileName(file.FileName);
                    if (extencion == ".wav")
                    {
                        System.IO.File.Copy(file.FileName, savePath);
                        AddToListBox(System.IO.Path.GetFileName(file.FileName));
                    }
                    else if (extencion == ".mp3")
                    {
                        using (var reader = new Mp3FileReader(file.FileName))
                        {
                            WaveFileWriter.CreateWaveFile(savePath.Replace(".mp3", ".wav"), reader);
                        }
                        AddToListBox(System.IO.Path.GetFileName(file.FileName).Replace(".mp3", ".wav"));
                    }
                }
                catch (IOException ex ) { MessageBox.Show(ex.ToString()); }
            }
            else { MessageBox.Show("Please choose a file with the extention .mp3 or .wav"); }
            SetListBox();
        }
        private static void AddToListBox(string line) => lbSongs.Items.Add(line);

        public static void SetListBox()
        {
            lbSongs.Items.Clear();
            string filePath = Assembly.GetExecutingAssembly().Location;
            string musicFolder = filePath.Replace(@"SchoolBellWPF\SchoolBell\SchoolBell\bin\Debug\net6.0-windows\SchoolBell.dll", "") + @"MusicFiles";
            string[] musicFiles = Directory.GetFiles(musicFolder);
            foreach (var path in musicFiles)
            {
                lbSongs.Items.Add(System.IO.Path.GetFileName(path).Replace(".wav", ""));
            }
        }

        private void btnListen_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (lbSongs.SelectedItem.ToString() != null)
                {
                    sp = new SoundPlayer(projectPath + @"\MusicFiles\"+ lbSongs.SelectedItem.ToString()+".wav");
                    sp.Play();
                }
                else { MessageBox.Show("Pleace chose something"); }
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e) { sp.Stop(); }

        private void SetEng()
        {
            songsWindow.Title = "Songs";
            btnStop.Content = "Stop";
            btnAddSong.Content = "Add song";
            btnListen.Content = "Listen";
        }
        private void SetEst()
        {
            songsWindow.Title = "Laulud";
            btnStop.Content = "Stop";
            btnAddSong.Content = "Lisada laulu";
            btnListen.Content = "Kuulata";
        }
        private void SetRus()
        {
            songsWindow.Title = "Музыка";
            btnStop.Content = "Остановить";
            btnAddSong.Content = "Добавить музыку";
            btnListen.Content = "Прослушать";
        }
    }
}
