using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shell;

using PortableSteam;
using Newtonsoft.Json;
using FuzzyString;

namespace WPFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsIdSet { get; set; }
        public bool IsMainPathSet { get; set; }
        public bool IsStashPathSet { get; set; }
        public string MainPath { get; set; }
        public string StashPath { get; set; }
        public List<string> RecentlyPlayed { get; set; }
        public string[] mainGameFolders { get; set; }
        public string[] stashedGameFolders { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SteamWebAPI.SetGlobalKey("90F5E96041068FC9C75AD83D42C0623A");

            IsIdSet = false;
            IsMainPathSet = false;
            IsStashPathSet = false;

            SteamIDBox.Text = "76561198060676822";

            MainPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common";
            MainPathSelect.Text = MainPath; 

            StashPath = "D:\\Games\\Steam Games\\steamapps\\common";
            StashPathSelect.Text = StashPath;

            RecentlyPlayed = new List<string>();

            mainGameFolders = new string[0];
            stashedGameFolders = new string[0];
        }

        private void loadLocalSteamApps()
        {
            mainGameFolders = System.IO.Directory.GetDirectories(MainPath).Select(d => new System.IO.DirectoryInfo(d).Name).ToArray();
            SteamOutputRecentMain.Items.Clear();

            foreach (var gameFolder in mainGameFolders)
            {
                CheckBox gameFolderCheckbox = new CheckBox();
                gameFolderCheckbox.Content = gameFolder.ToString();
                SteamOutputRecentMain.Items.Add(gameFolderCheckbox);          
            }

            stashedGameFolders = System.IO.Directory.GetDirectories(StashPath).Select(d => new System.IO.DirectoryInfo(d).Name).ToArray();
            SteamOutputRecentStashed.Items.Clear();

            foreach (var gameFolder in stashedGameFolders)
            {
                CheckBox gameFolderCheckbox = new CheckBox();
                gameFolderCheckbox.Content = gameFolder.ToString();
                SteamOutputRecentStashed.Items.Add(gameFolderCheckbox);
            }
        }

        private void OnSetProfileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                long steamId = Convert.ToInt64(SteamIDBox.Text);            
                SteamIdentity steamIdentity = SteamIdentity.FromSteamID(steamId);

                var response = SteamWebAPI.General().IPlayerService().GetRecentlyPlayedGames(steamIdentity).GetResponseString();
                dynamic parsedResponse = JsonConvert.DeserializeObject(response);

                foreach (dynamic game in parsedResponse.response.games)
                {
                    RecentlyPlayed.Add(game.name.Value);
                }

                ConsoleOut.Text = "Set Steam ID to " + steamId;

                IsIdSet = true;

                if (IsIdSet && IsMainPathSet && IsStashPathSet)
                {
                    loadLocalSteamApps();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                ConsoleOut.Text = "Invalid Steam ID";
            }
        }

        private void OnSetMainPathClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.IO.Directory.Exists(MainPathSelect.Text))
                {
                    MainPath = MainPathSelect.Text;
                    ConsoleOut.Text = "Set main steam path to " + MainPath;
                    IsMainPathSet = true;

                    if (IsIdSet && IsMainPathSet && IsStashPathSet)
                    {
                        loadLocalSteamApps();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                ConsoleOut.Text = "Invalid Path for Main Steam Path";
            }
        }

        private void OnSetStashPathClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.IO.Directory.Exists(StashPathSelect.Text))
                {
                    StashPath = StashPathSelect.Text;
                    ConsoleOut.Text = "Set stash steam path to " + StashPath;
                    IsStashPathSet = true;

                    if (IsIdSet && IsMainPathSet && IsStashPathSet)
                    {
                        loadLocalSteamApps();
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                ConsoleOut.Text = "Invalid Path for Stash Steam Path";
            }
        }

        // Checkbox operations

        private void OnMainSelectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentMain.Items) {
                gameFolder.IsChecked = true;
            }
        }

        private void OnStashedSelectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentStashed.Items)
            {
                gameFolder.IsChecked = true;
            }
        }


        private void OnMainUnselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentMain.Items) {
                gameFolder.IsChecked = false;
            }
        }

        private void OnStashedUnselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentStashed.Items)
            {
                gameFolder.IsChecked = false;
            }
        }

        private void OnMainSelectRecentClick(object sender, RoutedEventArgs e)
        {
            OnMainUnselectAllClick(sender, e);

            foreach (CheckBox gameFolder in SteamOutputRecentMain.Items)
            {
                foreach (string recentGame in RecentlyPlayed) 
                {
                    string source = recentGame;
                    string target = gameFolder.Content.ToString();
                    List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
                    options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);

                    if (FuzzyString.ComparisonMetrics.ApproximatelyEquals(source, target, options, FuzzyStringComparisonTolerance.Strong))
                    {
                        gameFolder.IsChecked = true;
                    }
                }
              
            }
        }

        private void OnStashedSelectRecentClick(object sender, RoutedEventArgs e)
        {
            OnStashedUnselectAllClick(sender, e);

            foreach (CheckBox gameFolder in SteamOutputRecentStashed.Items)
            {
                foreach (string recentGame in RecentlyPlayed)
                {
                    string source = recentGame;
                    string target = gameFolder.Content.ToString();
                    List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();
                    options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);

                    if (FuzzyString.ComparisonMetrics.ApproximatelyEquals(source, target, options, FuzzyStringComparisonTolerance.Strong))
                    {
                        gameFolder.IsChecked = true;
                    }
                }

            }
        }

        private void OnStashSelectedClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentMain.Items)
            {
                if (gameFolder.IsChecked.Equals(true))
                {
                    string pathToSource = MainPath + "\\" + gameFolder.Content.ToString();
                    string pathToDestination = StashPath + "\\" + gameFolder.Content.ToString();

                    ConsoleOut.Text = "Moving " + pathToSource + " to " + pathToDestination;
                    if (!System.IO.Directory.Exists(pathToDestination))
                    {
                        System.IO.Directory.CreateDirectory(pathToDestination);
                    }
                    CopyFolder(pathToSource, pathToDestination);
                    EmptyFolder(pathToSource);
                }
            }
            loadLocalSteamApps();
        }

        private void OnRestoreSelectedClick(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox gameFolder in SteamOutputRecentStashed.Items)
            {
                if (gameFolder.IsChecked.Equals(true))
                {
                    string pathToSource = StashPath + "\\" + gameFolder.Content.ToString();
                    string pathToDestination = MainPath + "\\" + gameFolder.Content.ToString();

                    ConsoleOut.Text = "Moving " + pathToSource + " to " + pathToDestination;
                    if (!System.IO.Directory.Exists(pathToDestination))
                    {
                        System.IO.Directory.CreateDirectory(pathToDestination);
                    }
                    CopyFolder(pathToSource, pathToDestination);
                    EmptyFolder(pathToSource);
                    ConsoleOut.Text = "Moved " + pathToSource + " to " + pathToDestination;
                }
            }
            loadLocalSteamApps();
        }

        static public void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!System.IO.Directory.Exists(destFolder))
                System.IO.Directory.CreateDirectory(destFolder);
            string[] files = System.IO.Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string dest = System.IO.Path.Combine(destFolder, name);
                System.IO.File.Copy(file, dest);
            }
            string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        static public void EmptyFolder(string sourceFolder)
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(sourceFolder);

            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (System.IO.DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }

            directory.Delete();
        }
    }
}
