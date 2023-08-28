using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FluxAPI;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using RBLXex.Helpers;
using System.Threading;
using RBLXex_Rework.helpers;
using System.Net;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using FluxAPI.Classes;

namespace RBLXex_Rework {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // Happy exploiting
        private protected readonly Flux Fluxus = new Flux();
       
        public static string roblox_name = "Windows10Universal"; //Microsoft Store Roblox
        
        public static string versionNumber = "0.0.1";
        public static string versionStage = "alpha";
        
        //Paths
        public static string monacoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"monaco_editor");
        
        public static string moduleDLLPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Module.dll");

        public static string savedPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"saved");
        public static string exploitsDatabasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"saved\exploits\db");
        public static string savedExploitsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"saved\exploits\personal");
        public static string settingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"saved\settings.txt");
        public static string favouritesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"saved\favourites.txt");

        public static int windowNumber = 0; //0 = Executor, 1 = Script Hub, 2 = Settings

        private static WebClient client = new WebClient();
        private static string versionFormat = $"{versionNumber}-{versionStage}";

        private ListBox lastBox;

        private bool isIntellisensed = false;
        private bool autoInjectionCounterInitialized = false;

        private Array[] settingsDefaults =
        {
            new string[]
            {
                "hubversion",
                "uninstalled"
            },

            new string[]
            {
                "version",
                versionFormat
            },

            new string[]
            {
                "autoexecute",
                "false"
            },

            new string[]
            {
                "opensavesfiles",
                "true"
            },

            new string[]
            {
                "autoinject",
                "false"
            }
        };
        
        private bool showScriptDB = true;
        private bool showExploitDirScript = true;

        private OpenFileDialog openDialog = new OpenFileDialog();
        private SaveFileDialog saveDialog = new SaveFileDialog { Filter = "Lua File (.lua)|*.lua|Text File (.txt)|*.txt|Any File|*" };

        private Point lastPoint;

        private bool IsInjected() {
            try {
                bool b = FluxusAPI.is_injected(FluxusAPI.pid);

                return b;
            } catch (Exception e) {
                return false;
            }
        }

        private void OpenPopup(string title, string body) {
            Popup popup = new Popup(title, body);
            popup.Show();
        }
        
        private string GetFilePathFromListBoxItem(string item) {
            string[] splitted = item.Split('-');

            if (splitted.Length > 1) {
                string name = splitted[0];
                string dirType = splitted[1];

                bool containedByYou = dirType == "p";

                string fullDir = "";

                if (containedByYou) {
                    fullDir = savedExploitsPath + "\\" + name;
                }
                else {
                    fullDir = exploitsDatabasePath + "\\" + name;
                }

                return fullDir;
            }

            return "";
        }

        private void UpdateScriptList() {
            favourite_list_box.Items.Clear();
            storage_list_box.Items.Clear();

            void ScanNAdd(string dir) {
                //add the faves first girlie!!!

                foreach (string fileDir in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                    bool isFav = false;
                    
                    foreach (string favPath in File.ReadLines(favouritesPath)) {
                        if (favPath == fileDir) {
                            isFav = true; break;
                        }
                    }

                    string fileName = System.IO.Path.GetFileName(fileDir);
                    string endAttachment = "-p";

                    if (dir == exploitsDatabasePath) {
                        endAttachment = "-db";
                    }

                    fileName = fileName + endAttachment;

                    if (isFav) {
                        favourite_list_box.Items.Add(fileName);
                    }
                    else {
                        storage_list_box.Items.Add(fileName);
                    }
                }
            }

            if (showScriptDB) {
                ScanNAdd(exploitsDatabasePath);
            }

            if (showExploitDirScript) {
                ScanNAdd(savedExploitsPath);
            }
        }

        private bool StringToBool(string bool_) {
            if (bool_ == "true" || bool_ == "True") {
                return true;
            } else {
                return false;
            }
        }

        private void UpdateCheckbox(Button box, string value) {
            if (StringToBool(value) == true) {
                box.Content = "✔";
            } else {
                box.Content = "";
            }
        }

        private void ToggleCheckbox(Button box, string settingsKey) {
            string value = OtherAssists.GetSettingValue(settingsKey);

            if (value == "true" || value == "True") {
                OtherAssists.AssignSettingValue(settingsKey, "false");
            } else {
                OtherAssists.AssignSettingValue(settingsKey, "true");
            }

            UpdateCheckbox(box, OtherAssists.GetSettingValue(settingsKey));
        }

        private void OpenWindow() {
            if (windowNumber == 0) {
                executor_tab.Visibility = Visibility.Visible;
                script_hub_tab.Visibility = Visibility.Hidden;
                settings_tab.Visibility = Visibility.Hidden;
            }
            else if (windowNumber == 1) {
                executor_tab.Visibility = Visibility.Hidden;
                script_hub_tab.Visibility = Visibility.Visible;
                settings_tab.Visibility = Visibility.Hidden;
            }
            else if (windowNumber == 2) {
                executor_tab.Visibility = Visibility.Hidden;
                script_hub_tab.Visibility = Visibility.Hidden;
                settings_tab.Visibility = Visibility.Visible;
            }
        }

        private void SetupIntellisense() {
            void RepetitiveAdding(string fileName, string kind) {
                foreach (string text in File.ReadLines(monacoPath + $"/{fileName}")) {
                    MonacoHelper.AddIntellisense(monaco_browser, text, kind);
                }
            }

            RepetitiveAdding("globalf.txt", "Function");
            RepetitiveAdding("globalv.txt", "Variable");
            RepetitiveAdding("globalns.txt", "Class");
            RepetitiveAdding("classfunc.txt", "Method");
            RepetitiveAdding("base.txt", "Keyword");
        }
        
        public void Execute(string code) {
            if (IsInjected()) {
                Fluxus.Execute(code);
            }
            else {
                OpenPopup("Inject first", "You must inject into ROBLOX Windows Store Version first before execution.");
            }
        }

        public MainWindow() {
            InitializeComponent();
        }

        private void top_bar_MouseDown(object sender, MouseButtonEventArgs e) {
            lastPoint = new Point(e.GetPosition(top_bar).X, e.GetPosition(top_bar).Y);
        }

        private void top_bar_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                Top = Top + (e.GetPosition(top_bar).Y - lastPoint.Y);
                Left = Left + (e.GetPosition(top_bar).X - lastPoint.X);
            }
        }

        private void minimize_button_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void close_button_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void clear_button_Click(object sender, RoutedEventArgs e) {
            MonacoHelper.SetCode(monaco_browser, "");
        }

        private void execute_Click(object sender, RoutedEventArgs e) {
            Execute(MonacoHelper.GetCode(monaco_browser));
        }

        private void inject_button_Click(object sender, RoutedEventArgs e) {
            if (Process.GetProcessesByName(roblox_name).Length > 0) {
                if (!IsInjected()) {
                    Fluxus.Inject();
                    
                    //OpenPopup("Inject success", "Sucessfully injected dll's into Windows10Universal.exe");
                }
                else {
                    OpenPopup("Already injected", "ROBLOX has already been injected by the exploit, you can execute now.");
                }
            }
            else {
                OpenPopup("Open ROBLOX", "Open ROBLOX windows store version and press inject.");
            }
        }

        private void monaco_browser_KeyDown(object sender, KeyEventArgs e) {
            if (!isIntellisensed) {
                SetupIntellisense();

                isIntellisensed = true;
            }
        }

        private void main_body_Loaded(object sender, RoutedEventArgs e) {
            // Turns out to get the monaco editor to load in a wpf application you need to combine several things such as the registry changes, the "mark of the web" and to start it with "file:///"
            Fluxus.CreateDirectories();
            
            if (!Directory.Exists(savedPath)) { Directory.CreateDirectory(savedPath); }
            if (!Directory.Exists(savedExploitsPath)) { Directory.CreateDirectory(savedExploitsPath); }
            if (!Directory.Exists(exploitsDatabasePath)) { Directory.CreateDirectory(exploitsDatabasePath); }

            if (!File.Exists(favouritesPath)) { new StreamWriter(File.Open(favouritesPath, FileMode.CreateNew)).Close(); }
            if (!File.Exists(settingsPath)) {
                StreamWriter writer = new StreamWriter(File.Open(settingsPath, FileMode.CreateNew));

                string fullDefaults = "";

                foreach (string[] setting in settingsDefaults) {
                    string key = setting[0];
                    string value = setting[1];

                    fullDefaults = fullDefaults + $"{key}={value}\n";
                }

                writer.Write(fullDefaults);

                writer.Close();
            }

            //Update settings and restore known values
            if (versionFormat != OtherAssists.GetSettingValue("version")) {
                string[] old = File.ReadAllLines(settingsPath);

                string fullDefaults = "";

                foreach (string[] setting in settingsDefaults) {
                    string key = setting[0];
                    string value = setting[1];

                    fullDefaults = fullDefaults + $"{key}={value}\n";
                }

                MessageBox.Show(fullDefaults);

                File.WriteAllText(settingsPath, fullDefaults);

                foreach (string oldLine in old) {
                    MessageBox.Show(oldLine);
                    string[] seperated = oldLine.Split('=');

                    if (seperated.Length > 1) {
                        string key = seperated[0];
                        string value = seperated[1];

                        foreach (string line in File.ReadAllLines(settingsPath)) {
                            string[] seperated_ = line.Split('=');

                            if (seperated_.Length > 1) {
                                string key_ = seperated_[0];

                                if (key_ == key) {
                                    OtherAssists.AssignSettingValue(key, value);
                                }
                            }
                        }
                    }
                }

                OtherAssists.AssignSettingValue("version", versionFormat);
            }

            client.Proxy = null;
            client.Headers.Add("User-Agent: Other");

            string latestVersion = "";

            try {
                string downloadedJsonString = client.DownloadString($"https://api.github.com/repos/Ossyence/RBLXex-exploits-db/tags");
                dynamic json = JsonConvert.DeserializeObject(downloadedJsonString);

                latestVersion = json[0]["name"].ToString();
            }
            catch (Exception error) { MessageBox.Show(error.Message); Application.Current.Shutdown(); }

            string currentVersion = OtherAssists.GetSettingValue("hubversion");

            bool mainUpToDate = currentVersion == latestVersion;

            if (!mainUpToDate) {
                Directory.Delete(exploitsDatabasePath, true);
                Directory.CreateDirectory(exploitsDatabasePath);

                string zipDownloadUrl = $"https://github.com/Ossyence/RBLXex-exploits-db/releases/download/{latestVersion}/RBLXex-exploits-db.zip";
                string zipPath = $"{exploitsDatabasePath}/RBLXex-exploits-db-zip";

                client.DownloadFile(zipDownloadUrl, zipPath);

                ZipFile.ExtractToDirectory(zipPath, exploitsDatabasePath);
                File.Delete(zipPath);

                OtherAssists.AssignSettingValue("hubversion", latestVersion);
            }

            UpdateScriptList();

            UpdateCheckbox(search_through_database_checkbox, showScriptDB.ToString());
            UpdateCheckbox(search_through_own_checkbox, showExploitDirScript.ToString());
            UpdateCheckbox(opening_saves_checkbox, OtherAssists.GetSettingValue("opensavesfiles"));
            UpdateCheckbox(auto_execution_checkbox, OtherAssists.GetSettingValue("autoexecute"));
            UpdateCheckbox(auto_injection_checkbox, OtherAssists.GetSettingValue("autoinject"));

            OpenWindow();

            //Edit the registry to make sure the WebBrowser emulates a browser correctly so it renders in a modern way
            //I tried using WebView2 but it didnt work properly so I have to use stupid WebBrowser that is inneficient and uses alot more memory
            //also WebView2 was more difficult to get info out of so

            //Edit registries
            try {
                //Get the registry key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);

                string friendlyName = AppDomain.CurrentDomain.FriendlyName;
                bool flag2 = key.GetValue(friendlyName) == null;

                if (flag2) {
                    key.SetValue(friendlyName, 11001, RegistryValueKind.DWord); //Set the registry
                }
            }
            catch { }

            version_text.Content = OtherAssists.GetSettingValue("version");
            monaco_browser.Source = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "monaco_editor/index.html").Replace(@"C:\", "file://127.0.0.1/c$/").ToString());

            Fluxus.InitializeAPI("RBLXex");
            Fluxus.CreateDirectories();
            Fluxus.DownloadDLLs();
            Fluxus.DoAutoAttach = StringToBool(OtherAssists.GetSettingValue("autoinject"));

            new Thread(() => {
                while (true) {
                    if (Fluxus.IsInitialized) {
                        if (IsInjected()) {
                            Dispatcher.Invoke(() =>
                            {
                                inject_button.Background = new SolidColorBrush(Color.FromRgb(40, 125, 60));
                                inject_button.BorderBrush = new SolidColorBrush(Color.FromRgb(10, 60, 20));
                            });
                        }
                        else {
                            Dispatcher.Invoke(() =>
                            {
                                inject_button.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                                inject_button.BorderBrush = new SolidColorBrush(Color.FromRgb(152, 152, 152));
                            });
                        }
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private void save_file_button_Click(object sender, RoutedEventArgs e) {
            saveDialog.InitialDirectory = savedExploitsPath;
            saveDialog.FileName = $"exploit_{Directory.GetFiles(savedExploitsPath, "*", SearchOption.TopDirectoryOnly).Length + 1}";

            if (saveDialog.ShowDialog() == true) {
                if (File.Exists(saveDialog.FileName)) { File.Delete(saveDialog.FileName); }

                StreamWriter writer = new StreamWriter(File.Open(saveDialog.FileName, FileMode.CreateNew));

                writer.Write(MonacoHelper.GetCode(monaco_browser));
                writer.Close();
            }
        }

        private void open_file_button_Click(object sender, RoutedEventArgs e) {
            openDialog.InitialDirectory = savedExploitsPath;

            if (openDialog.ShowDialog() == true) {
                openDialog.Title = "Open";

                MonacoHelper.SetCode(monaco_browser, File.ReadAllText(openDialog.FileName));

                string path = openDialog.FileName.Substring(0, openDialog.FileName.Length - openDialog.SafeFileName.Length - 1);
                
                if (path != savedExploitsPath && path != exploitsDatabasePath && OtherAssists.GetSettingValue("opensavesfiles") == "true") {
                    // automatically add to saved exploits folder
                    string exploitName = openDialog.SafeFileName;
                    string[] split = exploitName.Split('.');
                    exploitName = split[0];

                    string name = $"{savedExploitsPath}/{exploitName}{Directory.GetFiles(savedExploitsPath, "*", SearchOption.TopDirectoryOnly).Length + 1}.lua";

                    StreamWriter writer = new StreamWriter(File.Open(name, FileMode.CreateNew));

                    writer.Write(File.ReadAllText(openDialog.FileName));
                    writer.Close();

                    UpdateScriptList();
                }
            }
        }

        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory);
        }

        private void execute_script_button_Click(object sender, RoutedEventArgs e) {
            string fav_selected = "";
            string storage_selected = "";

            if (favourite_list_box.SelectedItem != null) {
                fav_selected = favourite_list_box.SelectedItem.ToString();
            }

            if (storage_list_box.SelectedItem != null) {
                storage_selected = storage_list_box.SelectedItem.ToString();
            }

            void ExecuteSelected(string selected) {
                string file = GetFilePathFromListBoxItem(selected);

                if (file != "") {
                    string content = File.ReadAllText(file);

                    Execute(content);
                }
            }

            ExecuteSelected(fav_selected);
            ExecuteSelected(storage_selected);
        }

        private void favourite_button_Click(object sender, RoutedEventArgs e) {
            string fav_selected = "";
            string storage_selected = "";

            if (favourite_list_box.SelectedItem != null) {
                fav_selected = favourite_list_box.SelectedItem.ToString();
            }

            if (storage_list_box.SelectedItem != null) {
                storage_selected = storage_list_box.SelectedItem.ToString();
            }

            string fav = GetFilePathFromListBoxItem(fav_selected);
            string storage = GetFilePathFromListBoxItem(storage_selected);

            if (storage != "") {
                string completed = "";

                string[] lines = File.ReadAllLines(favouritesPath);
                
                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i];

                    completed = completed + line + "\n";
                }

                completed = completed + storage;

                File.WriteAllText(favouritesPath, completed);
            }

            if (fav != "") {
                string completed = "";

                string[] lines = File.ReadAllLines(favouritesPath);

                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i];

                    if (line != fav) {
                        completed = completed + line + "\n";
                    }
                }

                File.WriteAllText(favouritesPath, completed);
            }

            UpdateScriptList();
        }

        private void executor_tab_button_Click(object sender, RoutedEventArgs e) {
            windowNumber = 0;
            OpenWindow();
        }

        private void script_hub_tab_button_Click(object sender, RoutedEventArgs e) {
            windowNumber = 1;
            OpenWindow();
        }

        private void settings_tab_button_Click(object sender, RoutedEventArgs e) {
            windowNumber = 2;
            OpenWindow();
        }

        private void opening_saves_checkbox_Click(object sender, RoutedEventArgs e) {
            ToggleCheckbox(opening_saves_checkbox, "opensavesfiles");
        }

        private void auto_execution_checkbox_Click(object sender, RoutedEventArgs e) {
            ToggleCheckbox(auto_execution_checkbox, "autoexecute");
        }

        private void auto_injection_checkbox_Click(object sender, RoutedEventArgs e) {
            ToggleCheckbox(auto_injection_checkbox, "autoinject");
        }

        private void search_through_own_checkbox_Click(object sender, RoutedEventArgs e) {
            showExploitDirScript = !showExploitDirScript;

            UpdateCheckbox(search_through_own_checkbox, showExploitDirScript.ToString());

            UpdateScriptList();
        }

        private void search_through_database_checkbox_Click(object sender, RoutedEventArgs e) {
            showScriptDB = !showScriptDB;

            UpdateCheckbox(search_through_database_checkbox, showScriptDB.ToString());

            UpdateScriptList();
        }

        // Stop multiselection
        // i have no fucking idea how this lastbox shit worked i just put random stuff together to stop multi selection and it somehow worked????

        private void favourite_list_box_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (storage_list_box.SelectedItem != null && lastBox != storage_list_box) {
                storage_list_box.UnselectAll();
            }

            lastBox = storage_list_box;
        }

        private void storage_list_box_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (favourite_list_box.SelectedItem != null && lastBox != favourite_list_box) {
                favourite_list_box.UnselectAll();
            }

            lastBox = favourite_list_box;
        }
    }
}
