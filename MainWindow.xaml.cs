using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace IsaacModManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private String dir;
        private List<Mod> mods = new List<Mod>();

        public MainWindow() {
            InitializeComponent();
            openDirectory();
        }

        //Load mod folder click event
        private void openModsFolder_clk(object sender,RoutedEventArgs e) {
            openDirectory();
        }

        private void openDirectory() {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.AllowNonFileSystemItems = true;
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result != CommonFileDialogResult.Ok) {
                MessageBox.Show("No Folder Selected");
                return;
            }

            mods.Clear();

            dir = dialog.FileName;

            foreach (var file in Directory.GetDirectories(dir)) {
                addMods(file);
            }

            mods.Sort();
            refreshList();
        }

        //Add mod path to list if valid
        private void addMods(string path) {
            if(!File.Exists(path + @"\metadata.xml")) {
                return;  
            }

            XmlDocument xml = new XmlDocument();

            try {
                xml.Load(path + @"\metadata.xml");
            }
            catch {
                return;
            }

            XmlNodeList metadata = xml.GetElementsByTagName("metadata");

            try {
                Mod mod = new Mod(metadata[0], !File.Exists(path + @"\disable.it"), path);

                mods.Add(mod);
            }
            catch {
                MessageBox.Show($"{path} is not a valid mod folder.\nCheck the metadata.xml");
            }
        }

        //Refresh Lists
        private void refreshList() {
            //Clear lists
            enabledList.Items.Clear();
            disabledList.Items.Clear();

            if(mods.Count == 0) {
                MessageBox.Show("No mods found in directory");
                return;
            }

            //Add items to list
            foreach (var mod in mods) {
                //MessageBox.Show(mod.ToString());
                if (mod.Enabled) {
                    enabledList.Items.Add(mod);
                }
                else {
                    disabledList.Items.Add(mod);                  
                }
            }

            disabledLabel.Content = $"Disabled {disabledList.Items.Count}/{mods.Count}";
            enabledLabel.Content = $"Enabled {enabledList.Items.Count}/{mods.Count}";
        }

        //Enable selected mod
        private void enableButton_Click(object sender,RoutedEventArgs e) {
            Mod selected = (Mod) disabledList.SelectedValue;

            //Check if any item is selected
            if (selected == null) {
                return;
            }

            enableMod(selected);
            refreshList();
        }

        //Enable all mods
        private void enableAllButton_Click(object sender,RoutedEventArgs e) {
            if (disabledList.Items.Count == 0) {
                return;
            }

            foreach (Mod mod in disabledList.Items) {
                enableMod(mod);
            }

            refreshList();
        }

        //Enable mod
        private void enableMod(Mod mod) {
            try {
                int index = mods.BinarySearch(mod);
                File.Delete(mods[index].Path + @"\disable.it");
                mods[index].Enabled = true;
            }
            catch { return; }
        }

        //Disable selected mod
        private void disableButton_Click(object sender,RoutedEventArgs e) {
            Mod selected = (Mod)enabledList.SelectedValue;

            //Check if any item is selected
            if (selected == null) {
                return;
            }

            disableMod(selected);
            refreshList();
        }

        //Disable all mods
        private void disableAllButton_Click(object sender,RoutedEventArgs e) {
            disableAllMods();
            refreshList();
        }

        private void disableAllMods() {
            if (enabledList.Items.Count == 0) {
                return;
            }

            foreach (Object mod in enabledList.Items) {
                disableMod(mod as Mod);
            }
        }

        //Disable mod
        private void disableMod(Mod mod) {
            try {
                int index = mods.BinarySearch(mod);
                File.Create(mods[index].Path + @"\disable.it");
                mods[index].Enabled = false;
            }
            catch { return; }
        }

        //Detect when list selection changes
        private void disabledList_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            Mod selected = (Mod)disabledList.SelectedValue;

            //Check if any item is selected
            if (selected == null) {
                return;
            }

            modDescription(selected);
        }

        private void enabledList_SelectionChanged(object sender,SelectionChangedEventArgs e) {
            Mod selected = (Mod)enabledList.SelectedValue;

            //Check if any item is selected
            if (selected == null) {
                return;
            }

            modDescription(selected);
        }

        //Update mod description field
        private void modDescription(Mod mod) {
            modNameLabel.Content = mod.Name;
            descriptionTextBlock.Document.Blocks.Clear();
            descriptionTextBlock.AppendText(mod.Description);
        }

        //Save enabled mods to file
        private void saveButton_Click(object sender,RoutedEventArgs e) {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "My Mod List";
            dialog.DefaultExt = ".imm";
            dialog.Filter = "Isaac Mod List (.imm)|*.imm";

            bool? result = dialog.ShowDialog();

            StreamWriter sw = File.CreateText(dialog.FileName);
            sw.WriteLine("IMM File");

            foreach (Mod mod in enabledList.Items) {
                sw.Write(mod.Id + ",");
            }

            sw.Close();
        }

        //Load mod list from file
        private void loadButton_Click(object sender,RoutedEventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.AllowNonFileSystemItems = true;
            dialog.IsFolderPicker = false;
            dialog.Filters.Add(new CommonFileDialogFilter("Isaac Mod List (.imm)",".imm"));
            CommonFileDialogResult result = dialog.ShowDialog();

            StreamReader sr = new StreamReader(dialog.FileName);

            if(sr.ReadLine() != "IMM File") {
                sr.Close();
                MessageBox.Show("Not a valid mod list file");
                return;
            }

            disableAllMods();
            String[] input = sr.ReadLine().Split(',');
            foreach(Mod mod in mods) {
                if(input.Contains(mod.Id)) {
                    enableMod(mod);
                }
            }

            refreshList();
        }
    }
}
