﻿using System;
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
using System.Windows.Shapes;

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für ManualDBManager.xaml
    /// </summary>
    public partial class ManualDBManager : Window
    {
        List<DCSInput> renderedInputsDCS;
        List<OtherGameInput> renderedInputsOG;
        ScrollViewer old = null;
        public ManualDBManager()
        {
            InitializeComponent();
            renderedInputsDCS = new List<DCSInput>();
            renderedInputsOG = new List<OtherGameInput>();
            CloseBtn.Click += new RoutedEventHandler(CloseThis);
            SetupGameList();
            GamesDropDown.SelectionChanged += new SelectionChangedEventHandler(SetupPlaneList);
            RefreshManualEntries();
            AddBtn.Click += new RoutedEventHandler(AddItem);
        }

        void SetupPlaneList(object sender, EventArgs e)
        {
            PlaneDropDown.Items.Clear();
            string selectedGame = (string)GamesDropDown.SelectedItem;
            if (selectedGame != null && DBLogic.Planes.ContainsKey(selectedGame))
            {
                for (int i = 0; i < DBLogic.Planes[selectedGame].Count; ++i)
                    PlaneDropDown.Items.Add(DBLogic.Planes[selectedGame][i]);
            }
            PlaneDropDown.Items.Add("ALL");
        }

        void SetupGameList()
        {
            GamesDropDown.Items.Clear();
            GamesDropDown.Items.Add("DCS");
            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.OtherLib)
                GamesDropDown.Items.Add(kvp.Key);
        }
        Grid setupBaseGrid(int rowsNeeded)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.Name = "InnerSV";
            Grid.SetColumn(sv, 0);
            Grid.SetRow(sv, 2);
            if (old != null)
            {
                MainGrid.Children.Remove(old);
            }
            MainGrid.Children.Add(sv);
            old = sv;
            sv.SetValue(Grid.ColumnSpanProperty, 6);
            Grid grid = new Grid();
            int columnsNeeded = 6;
            for(int i=0; i<rowsNeeded; ++i)
            grid.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < columnsNeeded; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
            }
            grid.ShowGridLines = true;
            sv.Content = grid;
            return grid;
        }
        void RefreshManualEntries()
        {
            
            
            renderedInputsDCS = new List<DCSInput>();
            renderedInputsOG = new List<OtherGameInput>();
            if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
            int rows = 0;
            foreach(KeyValuePair<string, DCSPlane> kvp in DBLogic.ManualDatabase.DCSLib)
            {
                foreach(KeyValuePair<string, DCSInput> kvpAx in kvp.Value.Axis)
                {
                    rows++;
                }
                foreach(KeyValuePair<string, DCSInput> kvpBn in kvp.Value.Buttons)
                {
                    rows++;
                }
            }
            foreach(KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.ManualDatabase.OtherLib)
            {
                foreach(KeyValuePair<string, OtherGame> kvpInner in kvp.Value)
                {
                    foreach(KeyValuePair<string, OtherGameInput> kvAx in kvpInner.Value.Axis)
                    {
                        rows++;
                    }
                    foreach(KeyValuePair<string, OtherGameInput> kvBn in kvpInner.Value.Buttons)
                    {
                        rows++;
                    }
                }
            }
            Grid g = setupBaseGrid(rows);
            int rowInput = 0;
            foreach (KeyValuePair<string, DCSPlane> kvp in DBLogic.ManualDatabase.DCSLib)
            {
                foreach (KeyValuePair<string, DCSInput> kvpAx in kvp.Value.Axis)
                {
                    renderedInputsDCS.Add(kvpAx.Value);
                    createRow(rowInput, rowInput, kvpAx.Value.ID, "DCS", kvpAx.Value.Plane, kvpAx.Value.Title, true, false, g);
                    rowInput++;
                }
                foreach (KeyValuePair<string, DCSInput> kvpBn in kvp.Value.Buttons)
                {
                    renderedInputsDCS.Add(kvpBn.Value);
                    createRow(rowInput, rowInput, kvpBn.Value.ID, "DCS", kvpBn.Value.Plane, kvpBn.Value.Title, false, false, g);
                    rowInput++;
                }
            }
            int otherGameIndex = 0;
            foreach (KeyValuePair<string, Dictionary<string, OtherGame>> kvp in DBLogic.ManualDatabase.OtherLib)
            {
                foreach (KeyValuePair<string, OtherGame> kvpInner in kvp.Value)
                {
                    foreach (KeyValuePair<string, OtherGameInput> kvAx in kvpInner.Value.Axis)
                    {
                        renderedInputsOG.Add(kvAx.Value);
                        createRow(rowInput, otherGameIndex, kvAx.Value.ID, kvp.Key, kvAx.Value.Plane, kvAx.Value.Title, true,true, g);
                        otherGameIndex++;
                        rowInput++;
                    }
                    foreach (KeyValuePair<string, OtherGameInput> kvBn in kvpInner.Value.Buttons)
                    {
                        renderedInputsOG.Add(kvBn.Value);
                        createRow(rowInput, otherGameIndex, kvBn.Value.ID, kvp.Key, kvBn.Value.Plane, kvBn.Value.Title, false, true, g);
                        otherGameIndex++;
                        rowInput++;
                    }
                }
            }
            g.ShowGridLines = true;

        }

        void createRow(int rowInput, int listIndex, string id, string game, string plane, string description, bool axis, bool othergame, Grid g)
        {
            string itemName;
            if (othergame)
                itemName = "o";
            else
                itemName = "d";
            itemName += listIndex.ToString();
            createLabel(itemName + "id", id, 0, rowInput,g);
            createLabel(itemName + "plane", plane, 1, rowInput,g);
            createLabel(itemName + "desc", description, 2, rowInput, g);
            createLabel(itemName + "ax", axis.ToString(), 3, rowInput, g);
            createLabel(itemName + "game", game, 4, rowInput, g);
            Button Btn = new Button();
            Btn.Name = itemName+"dlt";
            Btn.Content = "Delete";
            Btn.HorizontalAlignment = HorizontalAlignment.Center;
            Btn.VerticalAlignment = VerticalAlignment.Center;
            Btn.Width = 70;
            Btn.Click += new RoutedEventHandler(DeleteItem);
            Grid.SetColumn(Btn, 5);
            Grid.SetRow(Btn, rowInput);
            g.Children.Add(Btn);
        }
        void AddItem(object sender, EventArgs e)
        {
            if(IDTF.Text.Length<1||
                DescriptionTF.Text.Length<1||
                GamesDropDown.SelectedItem==null||
                (GamesDropDown.SelectedItem!=null&&((string)GamesDropDown.SelectedItem).Length<1)||
                PlaneDropDown.SelectedItem==null||
                (PlaneDropDown.SelectedItem != null && ((string)PlaneDropDown.SelectedItem).Length < 1))
            {
                MessageBox.Show("Input incorrect. Please make sure to maker proper selections");
                return;
            }
            bool ax = AxisCB.IsChecked == true ? true : false;
            string plane = (string)PlaneDropDown.SelectedItem;
            if ((string)GamesDropDown.SelectedItem == "DCS")
            {
                if (plane == "ALL")
                {
                    for(int i=0; i<DBLogic.Planes["DCS"].Count; ++i)
                    {
                        plane = DBLogic.Planes["DCS"][i];
                        DCSInput di = new DCSInput(IDTF.Text, DescriptionTF.Text, ax, plane);
                        if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
                        if (!DBLogic.ManualDatabase.DCSLib.ContainsKey(plane))
                            DBLogic.ManualDatabase.DCSLib.Add(plane, new DCSPlane(plane));
                        if (ax)
                        {
                            if (!DBLogic.ManualDatabase.DCSLib[plane].Axis.ContainsKey(di.ID))
                                DBLogic.ManualDatabase.DCSLib[plane].Axis.Add(di.ID, di);
                        }
                        else
                        {
                            if (!DBLogic.ManualDatabase.DCSLib[plane].Buttons.ContainsKey(di.ID))
                                DBLogic.ManualDatabase.DCSLib[plane].Buttons.Add(di.ID, di);
                        }
                    }
                }
                else
                {
                    DCSInput di = new DCSInput(IDTF.Text, DescriptionTF.Text, ax, (string)PlaneDropDown.SelectedItem);
                    if (DBLogic.ManualDatabase == null) DBLogic.ManualDatabase = new ManualDatabaseAdditions();
                    if (!DBLogic.ManualDatabase.DCSLib.ContainsKey((string)PlaneDropDown.SelectedItem))
                        DBLogic.ManualDatabase.DCSLib.Add((string)PlaneDropDown.SelectedItem, new DCSPlane((string)PlaneDropDown.SelectedItem));
                    if (ax)
                    {
                        if (!DBLogic.ManualDatabase.DCSLib[(string)PlaneDropDown.SelectedItem].Axis.ContainsKey(di.ID))
                            DBLogic.ManualDatabase.DCSLib[(string)PlaneDropDown.SelectedItem].Axis.Add(di.ID, di);
                    }
                    else
                    {
                        if (!DBLogic.ManualDatabase.DCSLib[(string)PlaneDropDown.SelectedItem].Buttons.ContainsKey(di.ID))
                            DBLogic.ManualDatabase.DCSLib[(string)PlaneDropDown.SelectedItem].Buttons.Add(di.ID, di);
                    }
                }
            }
            else
            {
                if (plane == "ALL")
                {
                    for(int i=0; i<DBLogic.Planes[(string)GamesDropDown.SelectedItem].Count; ++i)
                    {
                        plane = DBLogic.Planes[(string)GamesDropDown.SelectedItem][i];
                        OtherGameInput ogi = new OtherGameInput(IDTF.Text, DescriptionTF.Text, ax, (string)GamesDropDown.SelectedItem, plane);
                        if (!DBLogic.ManualDatabase.OtherLib.ContainsKey((string)GamesDropDown.SelectedItem))
                            DBLogic.ManualDatabase.OtherLib.Add((string)GamesDropDown.SelectedItem, new Dictionary<string, OtherGame>());
                        if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem].ContainsKey(plane))
                            DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem].Add(plane, new OtherGame(plane, (string)GamesDropDown.SelectedItem, true));
                        if (ax)
                        {
                            if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][plane].Axis.ContainsKey(ogi.ID))
                                DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][plane].Axis.Add(ogi.ID, ogi);
                        }
                        else
                        {
                            if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][plane].Buttons.ContainsKey(ogi.ID))
                                DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][plane].Buttons.Add(ogi.ID, ogi);
                        }
                    }
                }
                else
                {
                    OtherGameInput ogi = new OtherGameInput(IDTF.Text, DescriptionTF.Text, ax, (string)GamesDropDown.SelectedItem, (string)PlaneDropDown.SelectedItem);
                    if (!DBLogic.ManualDatabase.OtherLib.ContainsKey((string)GamesDropDown.SelectedItem))
                        DBLogic.ManualDatabase.OtherLib.Add((string)GamesDropDown.SelectedItem, new Dictionary<string, OtherGame>());
                    if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem].ContainsKey((string)PlaneDropDown.SelectedItem))
                        DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem].Add((string)PlaneDropDown.SelectedItem, new OtherGame((string)PlaneDropDown.SelectedItem, (string)GamesDropDown.SelectedItem, true));
                    if (ax)
                    {
                        if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][(string)PlaneDropDown.SelectedItem].Axis.ContainsKey(ogi.ID))
                            DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][(string)PlaneDropDown.SelectedItem].Axis.Add(ogi.ID, ogi);
                    }
                    else
                    {
                        if (!DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][(string)PlaneDropDown.SelectedItem].Buttons.ContainsKey(ogi.ID))
                            DBLogic.ManualDatabase.OtherLib[(string)GamesDropDown.SelectedItem][(string)PlaneDropDown.SelectedItem].Buttons.Add(ogi.ID, ogi);
                    }
                }
            }
            RefreshManualEntries();
        }
        void DeleteItem(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int listIndex = Convert.ToInt32(b.Name.Replace("dlt", "").Substring(1));
            if (b.Name.Substring(0, 1) == "d")
            {
                DCSInput toDelete = renderedInputsDCS[listIndex];
                if (DBLogic.ManualDatabase.DCSLib.ContainsKey(toDelete.Plane))
                {
                    if (toDelete.IsAxis)
                    {
                        if (DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Axis.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Axis.Remove(toDelete.ID);
                    }
                    else
                    {
                        if (DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Buttons.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.DCSLib[toDelete.Plane].Buttons.Remove(toDelete.ID);
                    }
                }
                    
            }
            else
            {
                OtherGameInput toDelete = renderedInputsOG[listIndex];
                if (DBLogic.ManualDatabase.OtherLib.ContainsKey(toDelete.Game)&& DBLogic.ManualDatabase.OtherLib[toDelete.Game].ContainsKey(toDelete.Plane))
                {
                    if (toDelete.IsAxis)
                    {
                        if (DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Axis.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Axis.Remove(toDelete.ID);
                    }
                    else
                    {
                        if (DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Buttons.ContainsKey(toDelete.ID))
                            DBLogic.ManualDatabase.OtherLib[toDelete.Game][toDelete.Plane].Buttons.Remove(toDelete.ID);
                    }
                }
            }
            RefreshManualEntries();
        }
        void createLabel(string name, string content, int col, int row, Grid g)
        {
            Label lbl = new Label();
            lbl.Name = name;
            lbl.Foreground = Brushes.White;
            lbl.Content = content;
            lbl.HorizontalAlignment = HorizontalAlignment.Center;
            lbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(lbl, col);
            Grid.SetRow(lbl, row);
            g.Children.Add(lbl);
        }
        void CloseThis(object sender, EventArgs e)
        {
            MainStructure.SaveMetaLast();
            InitGames.ReloadGameData();
            InitGames.ReloadDatabase();
            MainStructure.SaveMetaLast();
            Close();
        }
    }
}
