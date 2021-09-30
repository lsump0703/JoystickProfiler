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
    /// Interaktionslogik für CreateJoystickAlias.xaml
    /// </summary>
    public partial class CreateJoystickAlias : Window
    {
        string originalName;
        public CreateJoystickAlias(string original)
        {
            InitializeComponent();
            originalName = original;
            if (MainStructure.msave != null && MainStructure.msave.GrpMngr != null)
            {
                if (MainStructure.msave.GrpMngr.Top > 0) this.Top = MainStructure.msave.GrpMngr.Top;
                if (MainStructure.msave.GrpMngr.Left > 0) this.Left = MainStructure.msave.GrpMngr.Left;
                if (MainStructure.msave.GrpMngr.Width > 0) this.Width = MainStructure.msave.GrpMngr.Width;
                if (MainStructure.msave.GrpMngr.Height > 0) this.Height = MainStructure.msave.GrpMngr.Height;
            }
            else
            {
                MainStructure.msave = new MetaSave();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
            CloseBtn.Click += new RoutedEventHandler(CloseCreateJoystickAlias);
            DeviceOriginalNameLabel.Content = original;
            if (InternalDataMangement.JoystickAliases == null) InternalDataMangement.JoystickAliases = new Dictionary<string, string>();
            if (InternalDataMangement.JoystickAliases.ContainsKey(original)) NewAliasTF.Text = InternalDataMangement.JoystickAliases[original];
            RestoreBtn.Click += new RoutedEventHandler(RestoreOriginal);
            ApplyBtn.Click += new RoutedEventHandler(ApplyChange);
        }

        void RestoreOriginal(object sender, EventArgs e)
        {
            if (InternalDataMangement.JoystickAliases.ContainsKey(originalName))
            {
                InternalDataMangement.JoystickAliases[originalName] = "";
            }
            CloseCreateJoystickAlias(sender, e);
        }

        void ApplyChange(object sender, EventArgs e)
        {
            if(NewAliasTF.Text.Replace(" ","").Length<2|| 
                NewAliasTF.Text.Replace(" ", "") == "None"||
                NewAliasTF.Text.Replace(" ", "") == "ALL" ||
                NewAliasTF.Text.Replace(" ", "") == "NONE" ||
                NewAliasTF.Text.Replace(" ", "") == "UNASSIGNED" ||
                NewAliasTF.Text.Replace(" ", "").Contains("\"") ||
                NewAliasTF.Text.Replace(" ", "").Contains("\\") ||
                NewAliasTF.Text.Replace(" ", "").Contains(",") ||
                InternalDataMangement.DoesJoystickAliasExist(NewAliasTF.Text))
            {
                MessageBox.Show("Name to short, or reserved name or symbol or already exists");
                return;
            }
            if (InternalDataMangement.JoystickAliases.ContainsKey(originalName))
            {
                InternalDataMangement.JoystickAliases[originalName] = NewAliasTF.Text;
            }
            else
            {
                InternalDataMangement.JoystickAliases.Add(originalName, NewAliasTF.Text);
            }
            CloseCreateJoystickAlias(sender, e);
        }

        void CloseCreateJoystickAlias(object sender, EventArgs e)
        {
            MainStructure.SaveMetaLast();
            Close();
        }
    }
}
