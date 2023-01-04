﻿using SAM.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;

namespace SAM.Analytical.UI.WPF
{
    /// <summary>
    /// Interaction logic for MapInternalConditionsControl.xaml
    /// </summary>
    public partial class MapInternalConditionsControl : UserControl
    {
        private static string selectText = "Select...";
        private static string internalText = "<Internal>";

        private TextMap textMap_Loaded;
        private InternalConditionLibrary internalConditionLibrary_Loaded;

        private TextMap textMap;
        private InternalConditionLibrary internalConditionLibrary;

        public MapInternalConditionsControl()
        {
            InitializeComponent();

            Load();
        }

        public MapInternalConditionsControl(IEnumerable<Space> spaces, TextMap textMap = null, InternalConditionLibrary internalConditionLibrary = null)
        {
            InitializeComponent();

            TextMap = textMap;
            InternalConditionLibrary = internalConditionLibrary;

            SetSpaces(spaces);

            Load();
        }

        private void Load()
        {
            LoadInternalConditionLibrary();
            LoadTextMap();
        }

        public TextMap TextMap
        {
            get
            {
                return textMap;
            }

            set
            {
                SetTextMap(value);
            }
        }

        public InternalConditionLibrary InternalConditionLibrary
        {
            get
            {
                return internalConditionLibrary;
            }

            set
            {
                SetInternalConditionLibrary(value);
            }
        }

        public List<Space> Spaces
        {
            get
            {
                return GetSpaces();
            }

            set
            {
                SetSpaces(value);
            }
        }

        public List<Space> GetSpaces(bool selected = false)
        {
            if (wrapPanel.Children == null || wrapPanel.Children.Count == 0)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach(DockPanel dockPanel in wrapPanel.Children)
            {
                if(selected)
                {
                    if(!(dockPanel.Children[0] as CheckBox).IsChecked.Value)
                    {
                        continue;
                    }
                }

                Space space = dockPanel.Tag as Space;
                if(space == null)
                {
                    continue;
                }

                InternalCondition internalCondition = null;

                string internalConditionName = (dockPanel.Children[1] as ComboBox).Text;
                if(!string.IsNullOrWhiteSpace(internalConditionName))
                {
                    internalCondition = internalConditionLibrary.GetInternalConditions(internalConditionName)?.FirstOrDefault();
                }

                space = new Space(space);

                if(internalCondition != null)
                {
                    space.InternalCondition = internalCondition;
                }

                result.Add(space);
            }

            return result;
        }

        private List<Tuple<Space, string>> GetTuples()
        {
            if (wrapPanel.Children == null || wrapPanel.Children.Count == 0)
            {
                return null;
            }

            int index = 0;

            List<Tuple<Space, string>> result = new List<Tuple<Space, string>>();
            foreach (DockPanel dockPanel in wrapPanel.Children)
            {
                Space space = dockPanel.Tag as Space;
                index++;
                if (space == null)
                {
                    continue;
                }

                string internalConditionName = (dockPanel.Children[1] as ComboBox).Text;
                if(string.IsNullOrWhiteSpace(internalConditionName))
                {
                    internalConditionName = null;
                }

                result.Add(new Tuple<Space, string>(space, internalConditionName));
            }

            return result;
        }

        private void SetSpaces(IEnumerable<Space> spaces)
        {
            List<Tuple<Space, string>> tuples = GetTuples();

            wrapPanel.Children.Clear();

            if(spaces == null || spaces.Count() == 0)
            {
                return;
            }

            HashSet<string> hashSet = new HashSet<string>();
            hashSet.Add(string.Empty);
            if(internalConditionLibrary_Loaded != null)
            {
                List<InternalCondition> internalConditons = internalConditionLibrary_Loaded.GetInternalConditions();
                if(internalConditons != null && internalConditons.Count != 0)
                {
                    foreach(InternalCondition internalCondition in internalConditons)
                    {
                        string name = internalCondition?.Name;
                        if(string.IsNullOrWhiteSpace(name))
                        {
                            continue;
                        }

                        hashSet.Add(name);
                    }
                }
            }

            foreach (Space space in spaces)
            {
                string internalConditionName = string.Empty;
                if(tuples != null)
                {
                    int index = tuples.FindIndex(x => x.Item1.Guid == space.Guid);
                    if(index != -1 && hashSet.Contains(tuples[index].Item2))
                    {
                        internalConditionName = tuples[index].Item2;
                    }
                }

                DockPanel dockPanel = new DockPanel() { Width = 330, Height = 30, Tag = space };

                CheckBox checkBox = new CheckBox() { Width = 100, Content = space.Name, IsChecked = true, VerticalAlignment = VerticalAlignment.Center };
                dockPanel.Children.Add(checkBox);

                ComboBox comboBox = new ComboBox() { MinWidth = 150, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Height = 25 };
                foreach(string internalConditionName_Temp in hashSet)
                {
                    comboBox.Items.Add(internalConditionName_Temp);
                }

                comboBox.Text = internalConditionName;

                dockPanel.Children.Add(comboBox);

                wrapPanel.Children.Add(dockPanel);
            }
        }

        private void LoadInternalConditionLibrary()
        {
            Load(comboBox_InternalConditionLibrary, internalConditionLibrary);
        }

        private void LoadTextMap()
        {
            Load(comboBox_TextMap, textMap);
        }

        private void SetInternalConditionLibrary(InternalConditionLibrary internalConditionLibrary)
        {
            if(this.internalConditionLibrary == internalConditionLibrary)
            {
                return;
            }

            this.internalConditionLibrary = internalConditionLibrary;
            internalConditionLibrary_Loaded = this.internalConditionLibrary;

            LoadInternalConditionLibrary();

            List<Space> spaces = GetSpaces();
            SetSpaces(spaces);
        }

        private void SetTextMap(TextMap textMap)
        {
            if (this.textMap == textMap)
            {
                return;
            }

            this.textMap = textMap;
            textMap_Loaded = this.textMap;

            LoadTextMap();
        }

        private void Assign()
        {
            foreach(DockPanel dockPanel in wrapPanel.Children)
            {
                if(!(dockPanel.Children[0] as CheckBox).IsChecked.Value)
                {
                    continue;
                }

                Space space = dockPanel.Tag as Space;
                if(space == null)
                {
                    continue;
                }

                if(Analytical.Query.TryGetInternalCondition(space, internalConditionLibrary_Loaded, textMap, out InternalCondition internalCondition) && internalCondition != null)
                {
                    if(internalCondition.Name != null)
                    {
                        (dockPanel.Children[1] as ComboBox).Text = internalCondition.Name;
                    }
                }
            }
        }

        private void Button_Assign_Click(object sender, RoutedEventArgs e)
        {
            Assign();
        }

        private void button_SelectNone_Click(object sender, RoutedEventArgs e)
        {
            CheckAll(false);
        }

        private void button_SelectAll_Click(object sender, RoutedEventArgs e)
        {
            CheckAll(true);
        }

        private void CheckAll(bool isChecked)
        {
            foreach (DockPanel dockPanel in wrapPanel.Children)
            {
                (dockPanel.Children[0] as CheckBox).IsChecked = isChecked;
            }
        }

        private void comboBox_InternalConditionLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InternalConditionLibrary internalConditionLibrary = GetJSAMObject(comboBox_InternalConditionLibrary, this.internalConditionLibrary);

            if(internalConditionLibrary == null)
            {
                comboBox_InternalConditionLibrary.SelectedItem = comboBox_InternalConditionLibrary.Text;
                return;
            }

            internalConditionLibrary_Loaded = internalConditionLibrary;
        }

        private void comboBox_TextMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextMap textMap = GetJSAMObject(comboBox_TextMap, this.textMap);

            if (textMap == null)
            {
                comboBox_TextMap.SelectedItem = comboBox_TextMap.Text;
                return;
            }

            textMap_Loaded = textMap;
        }

        private static T GetJSAMObject<T>(ComboBox comboBox, T jSAMObject) where T : IJSAMObject
        {
            string text = comboBox.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                return default(T);
            }

            T result = default(T);
            if (text.Equals(selectText))
            {
                string path = null;
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    string directory = Analytical.Query.ResourcesDirectory();
                    if (System.IO.Directory.Exists(directory))
                    {
                        openFileDialog.InitialDirectory = directory;
                    }

                    openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;
                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        comboBox.SelectedItem = comboBox.Text;
                        return result;
                    }

                    path = openFileDialog.FileName;
                    comboBox.Items.Add(path);
                    comboBox.Text = path;
                }

                List<T> ts = Core.Convert.ToSAM<T>(path);
                if (ts != null && ts.Count != 0)
                {
                    result = ts[0];
                }
            }
            else if (text.Equals(internalText))
            {
                result = jSAMObject;
            }
            else
            {
                List<T> ts = Core.Convert.ToSAM<T>(text);
                if (ts != null && ts.Count != 0)
                {
                    result = ts[0];
                }
            }

            return result;
        }

        private static void Load(ComboBox comboBox, object @object)
        {
            string selectedValue = comboBox?.Text;

            HashSet<string> hashSet = new HashSet<string>();
            foreach (string item in comboBox.Items)
            {
                hashSet.Add(item);
            }

            if (@object == null)
            {
                hashSet.Remove(internalText);
            }
            else
            {
                hashSet.Add(internalText);
            }

            if (!hashSet.Contains(selectText))
            {
                hashSet.Add(selectText);
            }

            List<string> values = hashSet.ToList();

            if (values.Contains(internalText))
            {
                values.Remove(internalText);
                values.Add(internalText);
            }

            if (values.Contains(selectText))
            {
                values.Remove(selectText);
                values.Add(selectText);
            }

            comboBox.Items.Clear();
            if (comboBox.Items.Count != 0)
            {
                comboBox.Items.Clear();
            }

            foreach (string value in values)
            {
                comboBox.Items.Add(value);
            }

            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                comboBox.Text = selectedValue;
            }

            if (string.IsNullOrWhiteSpace(comboBox.Text) && @object != null && comboBox.Items.Contains(internalText))
            {
                comboBox.Text = internalText;
            }
        }
    }
}
