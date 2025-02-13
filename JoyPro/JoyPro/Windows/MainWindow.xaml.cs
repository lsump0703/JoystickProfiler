﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

namespace JoyPro
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static double DEFAULT_WIDTH;
        public static double DEFAULT_HEIGHT;
        List<Button> ALLBUTTONS;
        List<Window> ALLWINDOWS;
        List<Relation> CURRENTDISPLAYEDRELATION;
        Button[] editBtns;
        Button[] dltBtns;
        Button[] dupBtns;
        Button[] setBtns;
        ComboBox[,] mods;
        Label[] stickLabels;
        Label[] relationLabels;
        ComboBox[] groupComboboxes;
        CheckBox[] invertedcbs;
        CheckBox[] slidercbs;
        CheckBox[] usercurvecbs;
        Button[] userCurveBtn;
        TextBox[,] tboxes; 
        int buttonSetting;
        JoystickReader joyReader;
        public string selectedSort1;
        public string selectedSort2;
        public string selectedSort3;
        List<ColumnDefinition> colDefs = null;
        List<ColumnDefinition> colHds = null;
        Control[] controls = null;
        List<Button> additional;
        int gridCols;
        List<string> possibleSticks;
        Dictionary<string, LayoutFile> usedImages;
        Dictionary<string, Dictionary<string, ComboBox>> renderedComboBoxes;
        Dictionary<ComboBox, string> deviceLookup;
        bool overlay_opened = false;
        OverlayWindow ow = null;
        string visualLastJoystickSelected = "";
        double visualVerticalLastScroll = 0.0;
        double visualHorizontalLastScroll = 0.0;

        public MainWindow()
        {
            
            Stopwatch stopwatch = new Stopwatch();
            usedImages = new Dictionary<string, LayoutFile>();
            renderedComboBoxes = new Dictionary<string, Dictionary<string, ComboBox>>();
            deviceLookup = new Dictionary<ComboBox, string>();
            stopwatch.Start();
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Application.Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            InitializeComponent();
            DEFAULT_HEIGHT = this.Height;
            DEFAULT_WIDTH = this.Width;
            gridCols = 15;
            VersionLabel.Content = "v" + MainStructure.version.ToString();
            CURRENTDISPLAYEDRELATION = new List<Relation>();
            ALLWINDOWS = new List<Window>();
            setGridBordersLightGray();
            MainStructure.mainW = this;
            ButtonsIntoList();
            SetupEventHandlers();
            FirstStart();
            joyReader = null;
            buttonSetting = -1;
            selectedSort1 = "NAME_NORM";
            selectedSort2 = "STICK_NORM";
            selectedSort3 = "BTN_NORM";
            scaleTBox.Text = MainStructure.ScaleFactor.ToString();
            scaleLbl.Visibility = Visibility.Hidden;
            scaleTBox.Visibility = Visibility.Hidden;
            
            stopwatch.Stop();
            Console.WriteLine("Startup Time: "+stopwatch.ElapsedMilliseconds.ToString() + "ms");
            this.Closing += new CancelEventHandler(ShutThreads);
        }
        void setGridBordersLightGray()
        {
            var T = Type.GetType("System.Windows.Controls.Grid+GridLinesRenderer," + " PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var GLR = Activator.CreateInstance(T);
            GLR.GetType().GetField("s_oddDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
            GLR.GetType().GetField("s_evenDashPen", BindingFlags.Static | BindingFlags.NonPublic).SetValue(GLR, new Pen(Brushes.LightGray, 0.5));
        }
        void FilterSearchConfirm(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterSearchResult(sender, e);
            }
        }
        void FilterSearchResult(object sender, EventArgs e)
        {
            string rawQry = SearchQueryRelationName.Text;
            if (rawQry.Replace(" ", "").Length < 1)
                InternalDataManagement.RelationWordFilter = null;
            else
            {
                rawQry = rawQry.Trim();
                InternalDataManagement.RelationWordFilter = rawQry.Split(' ');
            }
            InternalDataManagement.ResyncRelations();
        }
        void ButtonsIntoList()
        {
            ALLBUTTONS = new List<Button>();
            ALLBUTTONS.Add(LoadRelationsBtn);
            ALLBUTTONS.Add(SaveRelationsBtn);
            ALLBUTTONS.Add(AddRelationsBtn);
            ALLBUTTONS.Add(IncludeRelationsBtn);
            ALLBUTTONS.Add(MEBWEAKEBtn);
            ALLBUTTONS.Add(CEBAEBtn);
            ALLBUTTONS.Add(MEBWEAOEBtn);
            ALLBUTTONS.Add(SaveProfileBtn);
            ALLBUTTONS.Add(LoadProfileBtn);
            ALLBUTTONS.Add(MEBWEAABBtn);
            ALLBUTTONS.Add(ImportProfileBtn);
            ALLBUTTONS.Add(NewFileBtn);
            ALLBUTTONS.Add(ModManagerBtn);
            ALLBUTTONS.Add(ValidateBtn);
            ALLBUTTONS.Add(ReinstateBUBtn);
            ALLBUTTONS.Add(GroupManagerBtn);
            ALLBUTTONS.Add(ExchStickBtn);
            ALLBUTTONS.Add(SettingsBtn);
            ALLBUTTONS.Add(PrintBtn);
            ALLBUTTONS.Add(OTCBtn);
            ALLBUTTONS.Add(OverlayBtn);
            ALLBUTTONS.Add(SettingsOverlayBtn);
            ALLBUTTONS.Add(VisualAssigningModeBtn);
        }
        void SetupEventHandlers()
        {
            SearchQueryRelationName.AcceptsReturn = true;
            App.Current.MainWindow.Closing += new System.ComponentModel.CancelEventHandler(ProgramClosing);
            SetupButtonsEventHandler();
            SetupDropDownsEventHandlers();
            this.SizeChanged += new SizeChangedEventHandler(sizeChanged);
            sv.ScrollChanged += new ScrollChangedEventHandler(sizeChanged);
            this.ContentRendered += new EventHandler(setWindowPosSize);
            this.Loaded += new RoutedEventHandler(AfterLoading);
            SearchQueryRelationName.LostFocus += new RoutedEventHandler(FilterSearchResult);
            SearchQueryRelationName.KeyUp += new KeyEventHandler(FilterSearchConfirm);
            CBNukeUnused.Click += new RoutedEventHandler(MainStructure.SaveWindowState);
            scaleTBox.LostFocus += new RoutedEventHandler(SetNewScaleFactor);
        }
        void SetupDropDownsEventHandlers()
        {
            DropDownInstanceSelection.SelectionChanged += new SelectionChangedEventHandler(InstanceSelectionChanged);
        }
        void SetupButtonsEventHandler()
        {
            AddRelationsBtn.Click += new RoutedEventHandler(OpenRelation);
            LoadRelationsBtn.Click += new RoutedEventHandler(OpenLoadRelations);
            IncludeRelationsBtn.Click += new RoutedEventHandler(OpenIncludeRelations);
            LoadProfileBtn.Click += new RoutedEventHandler(OpenLoadProfile);
            SaveRelationsBtn.Click += new RoutedEventHandler(OpenSaveReleations);
            SaveProfileBtn.Click += new RoutedEventHandler(OpenSaveProfile);
            MEBWEAKEBtn.Click += new RoutedEventHandler(LoadExistingExportKeepExisting);
            MEBWEAOEBtn.Click += new RoutedEventHandler(LoadExistingExportOverwrite);
            CEBAEBtn.Click += new RoutedEventHandler(CleanAndExport);
            MEBWEAABBtn.Click += new RoutedEventHandler(LoadExistingExportAndAdd);
            ImportProfileBtn.Click += new RoutedEventHandler(OpenImportProf);
            NewFileBtn.Click += new RoutedEventHandler(NewFileEvent);
            ModManagerBtn.Click += new RoutedEventHandler(OpenModifierManager);
            ValidateBtn.Click += new RoutedEventHandler(OpenValidation);
            ExchStickBtn.Click += new RoutedEventHandler(OpenExchangeStick);
            SettingsBtn.Click += new RoutedEventHandler(OpenChangeJoystickSettings);
            ReinstateBUBtn.Click += new RoutedEventHandler(OpenBackupWindow);
            GroupManagerBtn.Click += new RoutedEventHandler(OpenGroupManager);
            OTCBtn.Click += new RoutedEventHandler(OpenSaveCSV);
            PrintBtn.Click += new RoutedEventHandler(PrintLayout);
            SettingsOverlayBtn.Click += new RoutedEventHandler(OpenOverlaySettings);
            OverlayBtn.Click += new RoutedEventHandler(OpenOverlay);
            VisualAssigningModeBtn.Click += new RoutedEventHandler(SwitchVisualMode);
        }
        void OpenBackupWindow(object sender, EventArgs e)
        {
            DisableInputs();
            ReinstateBackup ri = new ReinstateBackup();
            ri.Closing += new CancelEventHandler(ActivateInputs);
            ri.Show();
        }
        void OpenChangeJoystickSettings(object sender, EventArgs e)
        {
            DisableInputs();
            StickSettings stickSettings = new StickSettings();
            stickSettings.Show();
            stickSettings.Closing += new CancelEventHandler(ActivateInputs);
        }
        void OpenExchangeStick(object sender, EventArgs e)
        {
            List<string> sticksInBind = InternalDataManagement.LocalJoysticks.ToList();
            
            List<Modifier> mods = InternalDataManagement.GetAllModifiers();
            for (int i = 0; i < mods.Count; ++i)
            {
                string otherId = mods[i].device;
                if (otherId.Contains('{'))
                {
                    otherId = otherId.Split('{')[0] + "{" + otherId.Split('{')[1].ToUpper();
                }

                if (!sticksInBind.Contains(otherId))
                {
                    sticksInBind.Add(otherId);
                }
            }
            DisableInputs();
            StickToExchange ste = new StickToExchange(sticksInBind);
            ste.Show();
            ste.Closing += new CancelEventHandler(ActivateInputs);
        }
        void OpenValidation(object sender, EventArgs e)
        {
            Validation validate = new Validation();
            ValidationErrors win = new ValidationErrors(validate);
            win.Show();
        }
        void OpenModifierManager(object sender, EventArgs e)
        {
            DisableInputs();
            ModifierManager rw = new ModifierManager();
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
        }
        void NewFileEvent(object sender, EventArgs e)
        {
            InternalDataManagement.NewFile();
        }
        void AfterLoading(object sender, EventArgs e)
        {
            setWindowPosSize(sender, e);
            if (MainStructure.msave != null)
            {
                MainStructure.AfterLoad();
            }
            this.SizeChanged += new SizeChangedEventHandler(MainStructure.SaveWindowState);
            this.LocationChanged += new EventHandler(MainStructure.SaveWindowState);
        }
        void sizeChanged(object sender, EventArgs e)
        {
            if (colDefs != null && colHds != null)
            {
                for(int i=0; i < CURRENTDISPLAYEDRELATION.Count; ++i)
                {
                    //relationLabels[i].ActualWidth
                }
                for (int i = 0; i < colDefs.Count; ++i)
                {
                    colHds[i].MinWidth = colDefs[i].ActualWidth;
                    if (controls != null&&controls[i]!=null)
                    {
                        controls[i].MinWidth= colDefs[i].ActualWidth;
                    }
                }
                svHeader.ScrollToHorizontalOffset(sv.HorizontalOffset);
            }
        }
        void OpenGroupManager(object sender, EventArgs e)
        {
            GroupManagerW gmw = new GroupManagerW();
            DisableInputs();
            gmw.Show();
            gmw.Closing += new CancelEventHandler(InternalDataManagement.ResyncRelations);
            gmw.Closing += new CancelEventHandler(ActivateInputs);            
        }
        void OpenImportProf(object sender, EventArgs e)
        {
            if (MiscGames.DCSselectedInstancePath == null || MiscGames.DCSselectedInstancePath.Length < 1)
            {
                MessageBox.Show("Not Instance selected");
                return;
            }
            ImportWindow iw = new ImportWindow();
            DisableInputs();
            iw.Show();
            iw.Closing += new CancelEventHandler(ActivateInputs);

        }
        void LoadExistingExportKeepExisting(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Keep-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;

            PlanesToExport pex = new PlanesToExport(ExportMode.WriteCleanNotOverride, param);
            pex.Show();
            DisableInputs();
            pex.Closing+= new CancelEventHandler(ActivateInputs);
        }
        void LoadExistingExportOverwrite(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Overwrite-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;

            PlanesToExport pex = new PlanesToExport(ExportMode.WriteCleanOverride, param);
            pex.Show();
            DisableInputs();
            pex.Closing += new CancelEventHandler(ActivateInputs);
        }
        void LoadExistingExportAndAdd(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Add-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool? overwrite = CBKeepDefault.IsChecked;
            bool param;
            if (overwrite == null)
                param = true;
            else if (overwrite == true)
                param = false;
            else
                param = true;

            PlanesToExport pex = new PlanesToExport(ExportMode.WriteCleanAdd, param);
            pex.Show();
            DisableInputs();
            pex.Closing += new CancelEventHandler(ActivateInputs);
            
        }
        void CleanAndExport(object sender, EventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to export your profile (Clean-Mode)?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No || res == MessageBoxResult.Cancel || res == MessageBoxResult.None) return;
            bool nukeDevices = false;
            if (CBNukeUnused.IsChecked == true)
                nukeDevices = true;

            PlanesToExport pex = new PlanesToExport(ExportMode.WriteClean, nukeDevices);
            pex.Show();
            DisableInputs();
            pex.Closing += new CancelEventHandler(ActivateInputs);
            
        }
        void OpenSaveProfile(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Pr0file Files (*.pr0file)|*.pr0file|All filed (*.*)|*.*";
            saveFileDialog1.Title = "Save Pr0file";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            if(filePath.Length>4)
            {
                string[] pathParts = filePath.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataManagement.SaveProfileTo(filePath);
            }
        }
        void OpenSaveCSV(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Comma seperated values (*.csv)|*.csv|All filed (*.*)|*.*";
            saveFileDialog1.Title = "Save Overview";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            if (filePath.Length > 4)
            {
                string[] pathParts = filePath.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }

                InternalDataManagement.PrintOverviewToCsv(CURRENTDISPLAYEDRELATION, filePath);
            }
        }
        void OpenSaveReleations(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Relation Files (*.rl)|*.rl";
            saveFileDialog1.Title = "Save Relations";
            if (Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                saveFileDialog1.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            else
            {
                saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            string filePath;
            saveFileDialog1.ShowDialog();
            filePath = saveFileDialog1.FileName;
            string[] pathParts = filePath.Split('\\');
            if (pathParts.Length > 0)
            {
                MainStructure.msave.lastOpenedLocation = pathParts[0];
                for (int i = 1; i < pathParts.Length - 1; ++i)
                {
                    MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                }
            }
            InternalDataManagement.SaveRelationsTo(filePath);

        }
        public bool? RelationAlreadyExists(string relName)
        {
            // MessageBox.Show("The relation "+relName+" already exists. Do you want to overwrite it? Select None for auto renaming", )
            string message = "The relation " + relName + " already exists. Do you want to overwrite it? Select Cancel for auto renaming";
            const string caption = "Relation already exists";
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes) return true;
            else if (result == MessageBoxResult.No) return false;
            else return null;
        }
        void OpenLoadRelations(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Relation Files (*.rl)|*.rl|All filed (*.*)|*.*";
            ofd.Title = "Load Relations";
            if (MainStructure.msave == null) MainStructure.msave = new MetaSave();
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            string fileToOpen;
            if (ofd.ShowDialog() == true)
            {
                fileToOpen = ofd.FileName;
                string[] pathParts = fileToOpen.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataManagement.LoadRelations(fileToOpen);
            }
        }
        void OpenIncludeRelations(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Relation Files (*.rl)|*.rl|All filed (*.*)|*.*";
            ofd.Title = "Include Relations";
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }
            string[] filesToInclude;
            if (ofd.ShowDialog() == true)
            {
                filesToInclude = ofd.FileNames;
                string lastFile = filesToInclude[filesToInclude.Length - 1];
                string[] pathParts = lastFile.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataManagement.InsertRelations(filesToInclude);
            }
        }
        void OpenLoadProfile(object sendder, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Pr0file Files (*.pr0file)|*.pr0file|All filed (*.*)|*.*";
            ofd.Title = "Load Pr0file";
            if (MainStructure.msave.lastOpenedLocation.Length < 1 || !Directory.Exists(MainStructure.msave.lastOpenedLocation))
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                ofd.InitialDirectory = MainStructure.msave.lastOpenedLocation;
            }

            string fileToOpen;
            if (ofd.ShowDialog() == true)
            {
                Console.WriteLine(ofd.FileName);
                fileToOpen = ofd.FileName;
                string[] pathParts = fileToOpen.Split('\\');
                if (pathParts.Length > 0)
                {
                    MainStructure.msave.lastOpenedLocation = pathParts[0];
                    for (int i = 1; i < pathParts.Length - 1; ++i)
                    {
                        MainStructure.msave.lastOpenedLocation = MainStructure.msave.lastOpenedLocation + "\\" + pathParts[i];
                    }
                }
                InternalDataManagement.LoadProfile(fileToOpen);
            }
        }
        void DeleteRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int indx = Convert.ToInt32(b.Name.Replace("deleteBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            DeleteRelationButton(r);
        }
        public void DeleteRelationButton(Relation r)
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure that you want to delete the Relation: " + r.NAME, "Joy Pro Relation Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (mr == MessageBoxResult.Yes)
            {
                InternalDataManagement.RemoveRelation(r);
            }
        }
        void EditRelationButton(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            Console.WriteLine(b.Name);
            int indx = Convert.ToInt32(b.Name.Replace("editBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            EditRelationButton(r);
        }
        public void EditRelationButton(Relation r)
        {
            DisableInputs();
            RelationWindow rw = new RelationWindow(r);
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
            rw.Refresh();
            DisableInputs();
        }
        Grid BaseSetupRelationGrid()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colDefs = new List<ColumnDefinition>();
            for (int i = 0; i < gridCols; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                if (i == 6)
                {
                    c.Width = new GridLength(80, GridUnitType.Star);
                }
                grid.ColumnDefinitions.Add(c);
                colDefs.Add(c);
            }
            for (int i = 0; i < CURRENTDISPLAYEDRELATION.Count; i++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = (GridLength)converter.ConvertFromString("30");
                grid.RowDefinitions.Add(r);
            }
            grid.RowDefinitions.Add(new RowDefinition());
            editBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            dltBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            setBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            dupBtns = new Button[CURRENTDISPLAYEDRELATION.Count];
            stickLabels = new Label[CURRENTDISPLAYEDRELATION.Count];
            relationLabels = new Label[CURRENTDISPLAYEDRELATION.Count];
            mods = new ComboBox[CURRENTDISPLAYEDRELATION.Count, 4];
            tboxes = new TextBox[CURRENTDISPLAYEDRELATION.Count, 4];
            groupComboboxes= new ComboBox[CURRENTDISPLAYEDRELATION.Count];
            invertedcbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            slidercbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            usercurvecbs = new CheckBox[CURRENTDISPLAYEDRELATION.Count];
            userCurveBtn = new Button[CURRENTDISPLAYEDRELATION.Count];
            return grid;
        }
        public void ShowMessageBox(string msg)
        {
            MessageBox.Show(msg);
        }
        void SetBtnOrAxisEvent(object sender, EventArgs e)
        {
            DisableInputs();
            Button b = (Button)sender;
            int indx = Convert.ToInt32(b.Name.Replace("assignBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            buttonSetting = indx;
            b.Content = "SETTING";
            b.Background = Brushes.Green;
            BackgroundWorker bw = new BackgroundWorker();
            if (r.ISAXIS)
            {
                bw.DoWork += new DoWorkEventHandler(listenAxis);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AxisSet);
            }
            else
            {
                bw.DoWork += new DoWorkEventHandler(listenButton);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ButtonSet);
            }
            bw.RunWorkerAsync();
        }
        void listenButton(object sender, EventArgs e)
        {
            joyReader = new JoystickReader(false);
        }
        void listenAxis(object sender, EventArgs e)
        {
            joyReader = new JoystickReader(true);
        }
        void AxisSet(object sender, EventArgs e)
        {
            ActivateInputs();
            int indx = buttonSetting;
            buttonSetting = -1;
            setBtns[indx].Background = Brushes.White;
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (e == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                if (cr != null)
                {
                    InternalDataManagement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataManagement.AddBind(cr.Rl.NAME, cr);
            }
            if (joyReader == null)
            {
                MessageBox.Show("Something went wrong. Joyreader is null try again.");
                return;
            }
            if (joyReader.result == null)
            {
                //Delete Bind
                InternalDataManagement.DeleteBind(cr.Rl.NAME);
                InternalDataManagement.ResyncRelations();
                return;
            }
            cr.Joystick = joyReader.result.Device;
            cr.JAxis = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton.Replace("JOY_", "Axis-");
            stickLabels[indx].Content = joyReader.result.Device;
            joyReader = null;
            MainStructure.OverlayWorker.SetButtonMapping();
            Console.WriteLine(setBtns[indx].Content);
        }
        void ButtonSet(object sender, EventArgs e)
        {
            ActivateInputs();
            int indx = buttonSetting;
            buttonSetting = -1;
            setBtns[indx].Background = Brushes.White;
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (joyReader==null||joyReader.result == null)
            {
                setBtns[indx].Content = "None";
                stickLabels[indx].Content = "None";
                if (cr != null)
                {
                    InternalDataManagement.RemoveBind(cr);
                }
                return;
            }
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataManagement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Joystick = joyReader.result.Device;
            cr.JButton = joyReader.result.AxisButton;
            setBtns[indx].Content = joyReader.result.AxisButton.Replace("JOY_BTN", "Button-");
            Console.WriteLine(setBtns[indx].Content);
            stickLabels[indx].Content = joyReader.result.Device;
            joyReader = null;
            MainStructure.OverlayWorker.SetButtonMapping();
        }
        void modSelectionChanged(object sender, EventArgs e)
        {
            ComboBox cx = (ComboBox)sender;
            int indx = Convert.ToInt32(cx.Name.Split('x')[1]);
            Bind currentBind = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            int modint = Convert.ToInt32(cx.Name.Split('c')[0].Replace("mod", ""))-1;
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the modifiers");
                return;
            }
            string element = (string)cx.SelectedItem;
            if (element == "Delete")
            {
                if (modint < currentBind.AllReformers.Count)
                {
                    currentBind.AllReformers.RemoveAt(modint);
                }
            }
            else
            {
                Modifier m = InternalDataManagement.ModifierByName(element);
                if (m == null)
                {
                    MessageBox.Show("Something went wrong when trying to assing modifier please report with accurate repro steps or simply retry");
                    return;
                }
                if (!currentBind.AllReformers.Contains(m.toReformerString()))
                {
                    currentBind.AllReformers.Add(m.toReformerString());
                }
            }
            MainStructure.OverlayWorker.SetButtonMapping();
            InternalDataManagement.ResyncRelations();
        }
        void changeCurveToUserCurve(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Split('l')[1]);
            Bind currentBind = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the User Curve");
                return;
            }
            currentBind.Curvature = new List<double>();
            currentBind.Curvature.Add(0.0);
            if (cx.IsChecked == true)
            {
                currentBind.Curvature.Add(0.1);
                currentBind.Curvature.Add(0.2);
                currentBind.Curvature.Add(0.3);
                currentBind.Curvature.Add(0.4);
                currentBind.Curvature.Add(0.5);
                currentBind.Curvature.Add(0.6);
                currentBind.Curvature.Add(0.7);
                currentBind.Curvature.Add(0.8);
                currentBind.Curvature.Add(0.9);
                currentBind.Curvature.Add(1.0);
            }
            InternalDataManagement.ResyncRelations();
        }
        void changeUserCurve(object sender, EventArgs e)
        {
            Button cx = (Button)sender;
            int indx = Convert.ToInt32(cx.Name.Split('n')[1]);
            Bind currentBind = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            changeUserCurve(currentBind);
        }
        public void changeUserCurve(Bind currentBind)
        {
            if (currentBind == null)
            {
                MessageBox.Show("Please bind the main key first and then the User Curve");
                return;
            }
            UserCurveDCS uc = new UserCurveDCS(currentBind);
            uc.Show();
            DisableInputs();
            uc.Closing += new CancelEventHandler(ActivateInputs);
        }
        void DeviceFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Name == "ALL" && cb.IsChecked == true)
            {
                for (int i = InternalDataManagement.JoystickActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataManagement.JoystickActivity.ElementAt(i).Key;
                    InternalDataManagement.JoystickActivity[toChange] = true;
                }
                cb.IsChecked = false;
            }
            else if (cb.Name == "NONE" && cb.IsChecked == true)
            {
                for (int i = InternalDataManagement.JoystickActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataManagement.JoystickActivity.ElementAt(i).Key;
                    InternalDataManagement.JoystickActivity[toChange] = false;
                }

                cb.IsChecked = false;
            }
            else if(cb.Name== "UNASSIGNED")
            {
                if (cb.IsChecked == true)
                    InternalDataManagement.showUnassignedRelations = true;
                else
                    InternalDataManagement.showUnassignedRelations = false;
            }
            else
            {
                int indx = Convert.ToInt32(cb.Name.Replace("d", ""));
                if (cb.IsChecked == true)
                    InternalDataManagement.JoystickActivity[possibleSticks[indx]] = true;
                else
                    InternalDataManagement.JoystickActivity[possibleSticks[indx]] = false;
            }

            InternalDataManagement.ResyncRelations();
        }
        void GroupFilterChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Name == "ALL"&&cb.IsChecked==true)
            {
                for (int i = InternalDataManagement.GroupActivity.Count - 1; i >= 0; i--)
                {
                    string toChange = InternalDataManagement.GroupActivity.ElementAt(i).Key;
                    InternalDataManagement.GroupActivity[toChange] = true;
                }
                cb.IsChecked = false;
            }else if(cb.Name == "NONE"&&cb.IsChecked==true)
            {
                for (int i = InternalDataManagement.GroupActivity.Count - 1; i >= 0; i--)
                {
                    string toChange= InternalDataManagement.GroupActivity.ElementAt(i).Key;
                    InternalDataManagement.GroupActivity[toChange] = false;
                }
                
                cb.IsChecked = false;
            }else if (cb.Name == "UNASSIGNED")
            {
                if (cb.IsChecked == true)
                {
                    InternalDataManagement.showUnassignedGroups = true;
                }
                else
                {
                    InternalDataManagement.showUnassignedGroups = false;
                }
            }
            else
            {
                int indx = Convert.ToInt32(cb.Name.Replace("g",""));
                if (cb.IsChecked == true)
                    InternalDataManagement.GroupActivity[InternalDataManagement.AllGroups[indx]] = true;
                else
                    InternalDataManagement.GroupActivity[InternalDataManagement.AllGroups[indx]] = false;
            }

            InternalDataManagement.ResyncRelations();
        }

        public System.Drawing.Bitmap ResizeBitmap(System.Drawing.Bitmap bmp, int width, int height)
        {
            System.Drawing.Bitmap result = new System.Drawing.Bitmap(width, height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(result))
            {
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }
        void RefreshVisualRelations()
        {
            SetupLayerController();
            TabControl tc = new TabControl();
            tc.Name = "TabControllerVisual";
            
            for(int i = 0; i< InternalDataManagement.JoystickFileImages.Count; ++i)
            {
                string stick = InternalDataManagement.JoystickFileImages.ElementAt(i).Key;
                string pth = InternalDataManagement.JoystickFileImages.ElementAt(i).Value;
                if (!pth.EndsWith(".layout")) continue;
                TabItem tabItem = new TabItem();
                
                ScrollViewer sver = new ScrollViewer();
                if (InternalDataManagement.JoystickAliases.ContainsKey(stick))
                {
                    tabItem.Header = InternalDataManagement.JoystickAliases[stick];
                }
                else
                {
                    tabItem.Header = stick;
                }
                tabItem.Content = sver;
                tabItem.MouseRightButtonUp += new MouseButtonEventHandler(OpenJoystickCreateAliasVisual);
                tc.Items.Add(tabItem);
                sver.ScrollChanged += new ScrollChangedEventHandler(VisualScrollChanged);
                tc.SelectionChanged += new SelectionChangedEventHandler(VisualTabSelectionChanged);

                if (!renderedComboBoxes.ContainsKey(stick))
                {
                    renderedComboBoxes.Add(stick, new Dictionary<string, ComboBox>());
                }
                ImageBrush bgImg;
                LayoutFile lf;
                if (usedImages.ContainsKey(pth))
                {
                    lf= usedImages[pth];
                }
                else
                {
                    lf = MainStructure.ReadFromBinaryFile<LayoutFile>(pth);
                }
                System.Drawing.Bitmap scaled = ResizeBitmap(lf.backup, Convert.ToInt32(lf.backup.Width * MainStructure.ScaleFactor), Convert.ToInt32(lf.backup.Height * MainStructure.ScaleFactor));
                bgImg = new ImageBrush(CreateBitMapSourceFromBitmap(scaled));
                Grid grid = new Grid();
                ColumnDefinition colDef = new ColumnDefinition();
                RowDefinition rowDef = new RowDefinition();
                colDef.MinWidth= scaled.Width;
                rowDef.MinHeight= scaled.Height;
                grid.ColumnDefinitions.Add(colDef);
                grid.RowDefinitions.Add(rowDef);
                bgImg.Stretch = Stretch.None;
                bgImg.AlignmentX = AlignmentX.Left;
                bgImg.AlignmentY = AlignmentY.Top;
                grid.Background = bgImg;

                List<Label> axisRelations = new List<Label>();
                List<Label> buttonRelations = new List<Label>();
                Label lb = new Label();
                lb.FontSize = lf.Size;
                lb.Foreground = lf.ColorSCB;
                lb.FontFamily = new FontFamily(lf.Font);
                lb.HorizontalAlignment = HorizontalAlignment.Left;
                lb.VerticalAlignment = VerticalAlignment.Center;
                lb.Content = "REMOVE BINDING";
                axisRelations.Add(lb);
                buttonRelations.Add(lb);
                foreach (Relation kvpRel in CURRENTDISPLAYEDRELATION)
                {
                    if (kvpRel.ISAXIS)
                    {
                        Label lbl = new Label();
                        lbl.FontSize = lf.Size*MainStructure.ScaleFactor;
                        lbl.Foreground = lf.ColorSCB;
                        lbl.FontFamily = new FontFamily(lf.Font);
                        lbl.HorizontalAlignment = HorizontalAlignment.Left;
                        lbl.VerticalAlignment = VerticalAlignment.Center;
                        lbl.Content = kvpRel.NAME;
                        axisRelations.Add(lbl);
                    }
                    else
                    {
                        Label lbl = new Label();
                        lbl.FontSize = lf.Size * MainStructure.ScaleFactor;
                        lbl.Foreground = lf.ColorSCB;
                        lbl.FontFamily = new FontFamily(lf.Font);
                        lbl.HorizontalAlignment = HorizontalAlignment.Left;
                        lbl.VerticalAlignment = VerticalAlignment.Center;
                        lbl.Content = kvpRel.NAME;
                        buttonRelations.Add(lbl);
                    }
                }
                axisRelations.Sort((x,y) => ((string)x.Content).CompareTo((string)y.Content));
                buttonRelations.Sort((x, y) => ((string)x.Content).CompareTo((string)y.Content));

                foreach (KeyValuePair<string, Point> kvp in lf.Positions)
                {
                    if (kvp.Key == "Joystick" || kvp.Key == "Game" || kvp.Key == "Plane") continue;
                    ComboBox cb;
                    if (renderedComboBoxes[stick].ContainsKey(kvp.Key))
                    {
                        cb=renderedComboBoxes[stick][kvp.Key];
                        cb.SelectionChanged -= new SelectionChangedEventHandler(selectionChanged);
                        cb.MouseRightButtonUp -= new MouseButtonEventHandler(OpenBindSettings);
                    }
                    else
                    {
                        cb = new ComboBox();
                        deviceLookup.Add(cb, stick);                        
                    }
                    if (kvp.Key.Contains("JOY_BTN"))
                    {
                        cb.ItemsSource = buttonRelations;
                    }
                    else
                    {
                        cb.ItemsSource = axisRelations;
                    }
                    List<string> setRelation = InternalDataManagement.GetAllRelationNamesForJoystickButton(stick, kvp.Key);
                    int itemToSelect = -1;
                    if(setRelation.Count>MainStructure.VisualLayer)
                    {
                        for(int j=0; j<cb.Items.Count; ++j)
                        {
                            if (setRelation[MainStructure.VisualLayer] == (string)((Label)cb.Items[j]).Content)
                            {
                                itemToSelect = j;
                                break;
                            }
                        }
                        if (itemToSelect >= 0)
                            cb.SelectedIndex = itemToSelect;
                    }
                    if (deviceLookup.ContainsKey(cb)) deviceLookup[cb] = stick;
                    else deviceLookup.Add(cb, stick);
                    string altKey = kvp.Key.Replace("+", "ZPLUS").Replace("-", "ZMINUS");
                    cb.Name = altKey;
                    cb.Width = (lf.Size * 10) * MainStructure.ScaleFactor;
                    cb.Height = (lf.Size + 10)*MainStructure.ScaleFactor;
                    cb.HorizontalAlignment = HorizontalAlignment.Left;
                    cb.VerticalAlignment = VerticalAlignment.Top;
                    cb.Margin = new Thickness(kvp.Value.X*MainStructure.ScaleFactor, kvp.Value.Y * MainStructure.ScaleFactor, 0, 0);
                    Grid.SetColumn(cb, 0);
                    Grid.SetRow(cb, 0);
                    cb.SelectionChanged += new SelectionChangedEventHandler(selectionChanged);
                    cb.MouseRightButtonUp += new MouseButtonEventHandler(OpenBindSettings);
                    grid.Children.Add(cb);
                }
                sver.Content = grid;
            }
            if (visualLastJoystickSelected != null && visualLastJoystickSelected.Length > 0)
            {
                for(int i=0; i<tc.Items.Count; ++i)
                {
                    if (((string)((TabItem)tc.Items[i]).Header) == visualLastJoystickSelected)
                    {
                        tc.SelectedIndex = i;
                        ScrollViewer sv = (ScrollViewer)(((TabItem)tc.Items[i]).Content);
                        sv.ScrollToHorizontalOffset(visualHorizontalLastScroll);
                        sv.ScrollToVerticalOffset(visualVerticalLastScroll);
                        break;
                    }
                }
            }
            sv.Content = tc;
        }

        

        void VisualTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            visualLastJoystickSelected =(string) ((TabItem)tc.SelectedItem).Header;
        }

        void VisualScrollChanged(object sender, EventArgs e)
        {
            ScrollViewer scrollViewer  = (ScrollViewer)sender;
            visualHorizontalLastScroll = scrollViewer.HorizontalOffset;
            visualVerticalLastScroll = scrollViewer.VerticalOffset;
        }

        void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb =(ComboBox)sender;
            string stick = deviceLookup[cb];
            string relation = (string)((Label)cb.SelectedItem).Content;

            if (relation != "REMOVE BINDING")
            {
                string rawBtn = cb.Name;
                rawBtn = rawBtn.Replace("ZPLUS", "+" ).Replace("ZMINUS", "-");
                string[] parts = rawBtn.Split('+');
                string realBtn = parts[parts.Length - 1];
                List<string> reformers = new List<string>();
                for (int i = 0; i < parts.Length - 1; ++i)
                {
                    if (InternalDataManagement.AllModifiers.ContainsKey(parts[i]))
                    {
                        reformers.Add(InternalDataManagement.AllModifiers[parts[i]].toReformerString());
                    }
                    else
                    {
                        MessageBox.Show("The Modifier in the selected place does not exist anymore. Create a new layout file.");
                        return;
                    }
                }
                Bind b = null;
                if (InternalDataManagement.AllBinds.ContainsKey(relation))
                {
                    b = InternalDataManagement.AllBinds[relation];
                }
                else
                {
                    b = new Bind(InternalDataManagement.AllRelations[relation]);
                }
                b.Joystick = stick;
                if (InternalDataManagement.JoystickAliases.ContainsKey(stick)) b.aliasJoystick = InternalDataManagement.JoystickAliases[stick];
                if (b.Rl.ISAXIS) b.JAxis = realBtn;
                else b.JButton = realBtn;
                b.AllReformers = reformers;
                InternalDataManagement.AddBind(relation,b);
            }
            else
            {
                List<string> toRemove = new List<string>();
                foreach (var item in e.RemovedItems)
                {
                    Label l = (Label)item;
                    InternalDataManagement.RemoveBind(InternalDataManagement.AllBinds[(string)l.Content]);
                }
            }
            MainStructure.OverlayWorker.SetButtonMapping();
            InternalDataManagement.ResyncRelations();
        }
        void OpenBindSettings(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedIndex < 0) return;
            string stick = deviceLookup[cb];
            string relation = (string)((Label)cb.SelectedItem).Content;
            Bind b = InternalDataManagement.AllBinds[relation];
            if (b.Rl.ISAXIS)
            {
                BindAxisSetting bas = new BindAxisSetting(b, this);
                DisableInputs();
                bas.Show();
                bas.Closing += new CancelEventHandler(ActivateInputs);
            }
            else
            {
                BindButtonSetting bbs = new BindButtonSetting(b, this);
                DisableInputs();
                bbs.Show();
                bbs.Closing += new CancelEventHandler(ActivateInputs);
            }
        }
        BitmapSource CreateBitMapSourceFromBitmap(System.Drawing.Bitmap map)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            map.GetHbitmap(),
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        }
        void SetupLayerController()
        {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition());
            for(int i = 0; i< MainStructure.msave.maxVisualLayers; ++i)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition());
            }

            Label LayerLabel = new Label();
            LayerLabel.Name = "LayerLabel";
            LayerLabel.Content = "Layer";
            LayerLabel.Foreground = Brushes.White;
            LayerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            LayerLabel.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(LayerLabel, 0);
            Grid.SetRow(LayerLabel, 0);
            g.Children.Add(LayerLabel);

            for(int i=1; i<MainStructure.msave.maxVisualLayers; ++i)
            {
                Button layerButton = new Button();
                layerButton.Name = "LayerBtn" + i.ToString();
                layerButton.Content = i.ToString();
                layerButton.HorizontalAlignment = HorizontalAlignment.Center;
                layerButton.VerticalAlignment = VerticalAlignment.Center;
                if (MainStructure.VisualLayer + 1 == i)
                {
                    layerButton.Background = Brushes.Orange;
                }
                else
                {
                    layerButton.Background = Brushes.White;
                }
                layerButton.Width = 100;
                layerButton.Click += new RoutedEventHandler(SetLayer);
                Grid.SetColumn(layerButton, i);
                Grid.SetRow(layerButton, 0);
                g.Children.Add(layerButton);
            }
            svHeader.Content = g;
        }

        void RefreshDropDowns()
        {
            DeviceDropdown.Items.Clear();
            CheckBox dvcbAll = new CheckBox();
            dvcbAll.Name = "ALL";
            dvcbAll.Content = "ALL";
            dvcbAll.IsChecked = false;
            dvcbAll.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbAll);

            CheckBox dvcbNone = new CheckBox();
            dvcbNone.Name = "NONE";
            dvcbNone.Content = "NONE";
            dvcbNone.IsChecked = false;
            dvcbNone.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbNone);

            CheckBox dvcbUnassigned = new CheckBox();
            dvcbUnassigned.Name = "UNASSIGNED";
            dvcbUnassigned.Content = "UNASSIGNED";
            dvcbUnassigned.IsChecked = InternalDataManagement.showUnassignedRelations;
            dvcbUnassigned.Click += new RoutedEventHandler(DeviceFilterChanged);
            DeviceDropdown.Items.Add(dvcbUnassigned);

            possibleSticks = InternalDataManagement.GetAllPossibleJoysticks();
            possibleSticks.Sort();
            if (possibleSticks.Count != InternalDataManagement.JoystickActivity.Count)
            {
                InternalDataManagement.JoystickActivity.Clear();
                for (int i = 0; i < possibleSticks.Count; ++i)
                {
                    InternalDataManagement.JoystickActivity.Add(possibleSticks[i], true);
                }
            }
            if (InternalDataManagement.JoystickAliases == null)
                InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            for (int i = 0; i < InternalDataManagement.JoystickActivity.Count; ++i)
            {
                CheckBox dvcbItem = new CheckBox();
                dvcbItem.Name = "d" + i.ToString();
                string deviceNameToShow = InternalDataManagement.JoystickActivity.ElementAt(i).Key;
                if (InternalDataManagement.JoystickAliases.ContainsKey(deviceNameToShow) && InternalDataManagement.JoystickAliases[deviceNameToShow].Length > 0)
                {
                    deviceNameToShow = InternalDataManagement.JoystickAliases[deviceNameToShow];
                }
                dvcbItem.Content = deviceNameToShow;
                if (InternalDataManagement.JoystickActivity.ElementAt(i).Value == true)
                {
                    dvcbItem.IsChecked = true;
                }
                else
                {
                    dvcbItem.IsChecked = false;
                }
                dvcbItem.Click += new RoutedEventHandler(DeviceFilterChanged);
                DeviceDropdown.Items.Add(dvcbItem);
            }

            GroupFilterDropdown.Items.Clear();
            CheckBox cbAll = new CheckBox();
            cbAll.Name = "ALL";
            cbAll.Content = "ALL";
            cbAll.IsChecked = false;
            cbAll.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbAll);

            CheckBox cbNone = new CheckBox();
            cbNone.Name = "NONE";
            cbNone.Content = "NONE";
            cbNone.IsChecked = false;
            cbNone.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbNone);

            CheckBox cbUnass = new CheckBox();
            cbUnass.Name = "UNASSIGNED";
            cbUnass.Content = "UNASSIGNED";
            cbUnass.IsChecked = InternalDataManagement.showUnassignedGroups;
            cbUnass.Click += new RoutedEventHandler(GroupFilterChanged);
            GroupFilterDropdown.Items.Add(cbUnass);

            if (InternalDataManagement.AllGroups.Count != InternalDataManagement.GroupActivity.Count)
            {
                InternalDataManagement.GroupActivity.Clear();
                for (int b = 0; b < InternalDataManagement.AllGroups.Count; ++b)
                {
                    InternalDataManagement.GroupActivity.Add(InternalDataManagement.AllGroups[b], true);
                }
            }
            if (InternalDataManagement.JoystickAliases == null) InternalDataManagement.JoystickAliases = new Dictionary<string, string>();
            for (int b = 0; b < InternalDataManagement.AllGroups.Count; ++b)
            {
                CheckBox cbItem = new CheckBox();
                cbItem.Name = "g" + b.ToString();
                cbItem.Content = InternalDataManagement.AllGroups[b];
                if (InternalDataManagement.GroupActivity[InternalDataManagement.AllGroups[b]] == true)
                    cbItem.IsChecked = true;
                else
                    cbItem.IsChecked = false;
                cbItem.Click += new RoutedEventHandler(GroupFilterChanged);
                GroupFilterDropdown.Items.Add(cbItem);
            }

            PlaneDropdown.Items.Clear();
            CheckBox cbpAll = new CheckBox();
            cbpAll.Name = "ALL";
            cbpAll.Content = "ALL";
            cbpAll.IsChecked = false;
            cbpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
            PlaneDropdown.Items.Add(cbpAll);

            CheckBox cbpNone = new CheckBox();
            cbpNone.Name = "NONE";
            cbpNone.Content = "NONE";
            cbpNone.IsChecked = false;
            cbpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
            PlaneDropdown.Items.Add(cbpNone);

            for(int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                CheckBox cbgpAll = new CheckBox();
                cbgpAll.Name = "ALL";
                cbgpAll.Content = DBLogic.Planes.ElementAt(i).Key+":"+"ALL";
                cbgpAll.IsChecked = false;
                cbgpAll.Click += new RoutedEventHandler(PlaneFilterChanged);
                PlaneDropdown.Items.Add(cbgpAll);

                CheckBox cbgpNone = new CheckBox();
                cbgpNone.Name = "NONE";
                cbgpNone.Content = DBLogic.Planes.ElementAt(i).Key + ":" + "NONE";
                cbgpNone.IsChecked = false;
                cbgpNone.Click += new RoutedEventHandler(PlaneFilterChanged);
                PlaneDropdown.Items.Add(cbgpNone);
            }

            for (int i = 0; i < DBLogic.Planes.Count; ++i)
            {
                for(int j = 0; j < DBLogic.Planes.ElementAt(i).Value.Count; ++j)
                {
                    CheckBox pln = new CheckBox();
                    pln.Name = "plane";
                    string k = DBLogic.Planes.ElementAt(i).Key + ":" + DBLogic.Planes.ElementAt(i).Value[j];
                    pln.Content = k;
                    pln.IsChecked = InternalDataManagement.PlaneActivity[k];
                    pln.Click += new RoutedEventHandler(PlaneFilterChanged);
                    PlaneDropdown.Items.Add(pln);
                }
            }

        }
        private void PlaneFilterChanged(object sender, RoutedEventArgs e)
        {
            CheckBox sndr = (CheckBox)sender;
            if ((string)sndr.Content == "ALL")
            {
                for(int i=0; i<InternalDataManagement.PlaneActivity.Count; ++i)
                {
                    string key = InternalDataManagement.PlaneActivity.ElementAt(i).Key;
                    InternalDataManagement.PlaneActivity[key] = true;
                }
            }else if ((string)sndr.Content == "NONE")
            {
                for (int i = 0; i < InternalDataManagement.PlaneActivity.Count; ++i)
                {
                    string key = InternalDataManagement.PlaneActivity.ElementAt(i).Key;
                    InternalDataManagement.PlaneActivity[key] = false;
                }
            }
            else if (((string)sndr.Content).Contains(":ALL"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < InternalDataManagement.PlaneActivity.Count; ++i)
                {
                    string key = InternalDataManagement.PlaneActivity.ElementAt(i).Key;
                    if(key.StartsWith(game))InternalDataManagement.PlaneActivity[key] = true;
                }
            }
            else if (((string)sndr.Content).Contains(":NONE"))
            {
                string game = ((string)sndr.Content).Substring(0, ((string)sndr.Content).IndexOf(':'));
                for (int i = 0; i < InternalDataManagement.PlaneActivity.Count; ++i)
                {
                    string key = InternalDataManagement.PlaneActivity.ElementAt(i).Key;
                    if (key.StartsWith(game)) InternalDataManagement.PlaneActivity[key] = true;
                }
            }
            else
            {
                string key = ((string)sndr.Content);
                if (sndr.IsChecked == true)
                    InternalDataManagement.PlaneActivity[key] = true;
                else
                    InternalDataManagement.PlaneActivity[key] = false;
            }
            InternalDataManagement.ResyncRelations();
        }

        void RefreshRelationsToShow()
        {
            

            additional = new List<Button>();
            List<string> allMods = InternalDataManagement.GetAllModsAsString();
            allMods.Add("Delete");
            Grid grid = BaseSetupRelationGrid();
            for (int i = 0; i < CURRENTDISPLAYEDRELATION.Count; i++)
            {
                Label lblName = new Label();
                lblName.Name = "lblname" + i.ToString();
                lblName.Foreground = Brushes.White;
                lblName.Content = CURRENTDISPLAYEDRELATION[i].NAME;
                lblName.HorizontalAlignment = HorizontalAlignment.Left;
                lblName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(lblName, 0);
                Grid.SetRow(lblName, i);
                grid.Children.Add(lblName);
                relationLabels[i] = lblName;
                lblName.MouseEnter += new MouseEventHandler(OnHover);
                lblName.MouseLeave += new MouseEventHandler(OnLeave);

                Button editBtn = new Button();
                editBtns[i] = editBtn;
                editBtn.Name = "editBtn" + i.ToString();
                editBtn.Content = "Edit";
                editBtn.Click += new RoutedEventHandler(EditRelationButton);
                editBtn.HorizontalAlignment = HorizontalAlignment.Center;
                editBtn.VerticalAlignment = VerticalAlignment.Center;
                editBtn.Width = 50;
                Grid.SetColumn(editBtn, 2);
                Grid.SetRow(editBtn, i);
                grid.Children.Add(editBtn);
                editBtn.MouseEnter += new MouseEventHandler(OnHover);
                editBtn.MouseLeave += new MouseEventHandler(OnLeave);

                Button dupBtn = new Button();
                dupBtns[i] = dupBtn;
                dupBtn.Name = "dupBtn" + i.ToString();
                dupBtn.Content = "Duplicate";
                dupBtn.Click += new RoutedEventHandler(duplicateRelation);
                dupBtn.HorizontalAlignment = HorizontalAlignment.Center;
                dupBtn.VerticalAlignment = VerticalAlignment.Center;
                dupBtn.Width = 75;
                Grid.SetColumn(dupBtn, 3);
                Grid.SetRow(dupBtn, i);
                grid.Children.Add(dupBtn);
                dupBtn.MouseEnter += new MouseEventHandler(OnHover);
                dupBtn.MouseLeave += new MouseEventHandler(OnLeave);

                Button deleteBtn = new Button();
                dltBtns[i] = deleteBtn;
                deleteBtn.Name = "deleteBtn" + i.ToString();
                deleteBtn.Content = "Delete Relation";
                deleteBtn.Click += new RoutedEventHandler(DeleteRelationButton);
                deleteBtn.HorizontalAlignment = HorizontalAlignment.Center;
                deleteBtn.VerticalAlignment = VerticalAlignment.Center;
                deleteBtn.Width = 100;
                Grid.SetColumn(deleteBtn, 4);
                Grid.SetRow(deleteBtn, i);
                grid.Children.Add(deleteBtn);
                deleteBtn.MouseEnter += new MouseEventHandler(OnHover);
                deleteBtn.MouseLeave += new MouseEventHandler(OnLeave);

                ComboBox groupDropdown = new ComboBox();
                groupDropdown.Name = "GroupDropDown" + i.ToString();
                groupDropdown.HorizontalAlignment = HorizontalAlignment.Center;
                groupComboboxes[i] = groupDropdown;
                groupDropdown.VerticalAlignment = VerticalAlignment.Center;
                groupDropdown.Width = 150;
                for(int a=0; a< InternalDataManagement.AllGroups.Count; ++a)
                {
                    CheckBox cbxGroup = new CheckBox();
                    cbxGroup.Name = "r" + i.ToString() + "g" + a.ToString();
                    cbxGroup.Content = InternalDataManagement.AllGroups[a];
                    if (CURRENTDISPLAYEDRELATION[i].Groups!=null&&CURRENTDISPLAYEDRELATION[i].Groups.Contains(InternalDataManagement.AllGroups[a]))
                        cbxGroup.IsChecked = true;
                    else
                        cbxGroup.IsChecked = false;
                    cbxGroup.Click += new RoutedEventHandler(GroupManagementCheckboxChange);
                    groupDropdown.Items.Add(cbxGroup);
                }
                Grid.SetColumn(groupDropdown, 5);
                Grid.SetRow(groupDropdown, i);
                grid.Children.Add(groupDropdown);
                groupDropdown.MouseEnter += new MouseEventHandler(OnHover);
                groupDropdown.MouseLeave += new MouseEventHandler(OnLeave);

                Bind currentBind = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[i].NAME);

                Label joystickPick = new Label();
                joystickPick.Name = "joyLbl" + i.ToString();
                joystickPick.Content = "None";
                stickLabels[i] = joystickPick;
                joystickPick.Foreground = Brushes.White;
                if (currentBind != null)
                {                 

                     if (InternalDataManagement.JoystickAliases.ContainsKey(currentBind.Joystick) && InternalDataManagement.JoystickAliases[currentBind.Joystick].Length > 0)
                     {
                         joystickPick.Content = InternalDataManagement.JoystickAliases[currentBind.Joystick];
                     }
                     else
                     {
                         joystickPick.Content = currentBind.Joystick;
                     }
                    joystickPick.MouseLeftButtonUp += new MouseButtonEventHandler(OpenJoystickCreateAlias);                  
                }
                joystickPick.HorizontalAlignment = HorizontalAlignment.Center;
                joystickPick.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(joystickPick, 6);
                Grid.SetRow(joystickPick, i);
                grid.Children.Add(joystickPick);
                joystickPick.MouseEnter += new MouseEventHandler(OnHover);
                joystickPick.MouseLeave += new MouseEventHandler(OnLeave);

                Button joybtnin = new Button();
                joybtnin.Name = "assignBtn" + i.ToString();
                joybtnin.Content = "None";
                joybtnin.HorizontalAlignment = HorizontalAlignment.Center;
                joybtnin.VerticalAlignment = VerticalAlignment.Center;
                joybtnin.Width = 100;
                joybtnin.Click += new RoutedEventHandler(SetBtnOrAxisEvent);
                joybtnin.MouseRightButtonUp += new MouseButtonEventHandler(ManualBtnAxSet);
                setBtns[i] = joybtnin;
                Grid.SetColumn(joybtnin, 7);
                Grid.SetRow(joybtnin, i);
                grid.Children.Add(joybtnin);
                joybtnin.MouseEnter += new MouseEventHandler(OnHover);
                joybtnin.MouseLeave += new MouseEventHandler(OnLeave);

                if (CURRENTDISPLAYEDRELATION[i].ISAXIS)
                {

                    CheckBox cbx = new CheckBox();
                    cbx.Name = "cbxrel" + i.ToString();
                    cbx.Content = "Inverted";
                    cbx.Foreground = Brushes.White;
                    cbx.HorizontalAlignment = HorizontalAlignment.Center;
                    cbx.VerticalAlignment = VerticalAlignment.Center;
                    invertedcbs[i] = cbx;
                    Grid.SetColumn(cbx, 8);
                    Grid.SetRow(cbx, i);
                    grid.Children.Add(cbx);
                    cbx.MouseEnter += new MouseEventHandler(OnHover);
                    cbx.MouseLeave += new MouseEventHandler(OnLeave);

                    CheckBox cbxs = new CheckBox();
                    cbxs.Name = "cbxsrel" + i.ToString();
                    cbxs.Content = "Slider";
                    cbxs.Foreground = Brushes.White;
                    cbxs.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxs.VerticalAlignment = VerticalAlignment.Center;
                    slidercbs[i] = cbxs;
                    Grid.SetColumn(cbxs, 9);
                    Grid.SetRow(cbxs, i);
                    grid.Children.Add(cbxs);
                    cbxs.MouseEnter += new MouseEventHandler(OnHover);
                    cbxs.MouseLeave += new MouseEventHandler(OnLeave);

                    CheckBox cbxu = new CheckBox();
                    cbxu.Name = "cbxsrel" + i.ToString();
                    cbxu.Content = "User Curve";
                    cbxu.Foreground = Brushes.White;
                    cbxu.HorizontalAlignment = HorizontalAlignment.Center;
                    cbxu.VerticalAlignment = VerticalAlignment.Center;
                    cbxu.Click += new RoutedEventHandler(changeCurveToUserCurve);
                    usercurvecbs[i] = cbxu;
                    Grid.SetColumn(cbxu, 10);
                    Grid.SetRow(cbxu, i);
                    grid.Children.Add(cbxu);
                    cbxu.MouseEnter += new MouseEventHandler(OnHover);
                    cbxu.MouseLeave += new MouseEventHandler(OnLeave);

                    TextBox txrl = new TextBox();
                    txrl.Name = "txrldz" + i.ToString();
                    txrl.Width = 150;
                    txrl.Height = 24;
                    Grid.SetColumn(txrl, 11);
                    Grid.SetRow(txrl, i);
                    tboxes[i, 0] = txrl;
                    grid.Children.Add(txrl);
                    txrl.MouseEnter += new MouseEventHandler(OnHover);
                    txrl.MouseLeave += new MouseEventHandler(OnLeave);

                    TextBox txrlsx = new TextBox();
                    txrlsx.Name = "txrlsatx" + i.ToString();
                    txrlsx.Width = 150;
                    txrlsx.Height = 24;
                    Grid.SetColumn(txrlsx, 12);
                    Grid.SetRow(txrlsx, i);
                    tboxes[i, 1] = txrlsx;
                    grid.Children.Add(txrlsx);
                    txrlsx.MouseEnter += new MouseEventHandler(OnHover);
                    txrlsx.MouseLeave += new MouseEventHandler(OnLeave);

                    TextBox txrlsy = new TextBox();
                    txrlsy.Name = "txrlsaty" + i.ToString();
                    txrlsy.Width = 150;
                    txrlsy.Height = 24;
                    Grid.SetColumn(txrlsy, 13);
                    Grid.SetRow(txrlsy, i);
                    tboxes[i, 2] = txrlsy;
                    grid.Children.Add(txrlsy);
                    txrlsy.MouseEnter += new MouseEventHandler(OnHover);
                    txrlsy.MouseLeave += new MouseEventHandler(OnLeave);

                    TextBox txrlcv = new TextBox();
                    txrlcv.Name = "txrlsacv" + i.ToString();
                    txrlcv.Width = 150;
                    txrlcv.Height = 24;
                    Grid.SetColumn(txrlcv, 14);
                    Grid.SetRow(txrlcv, i);
                    tboxes[i, 3] = txrlcv;
                    txrlcv.MouseEnter += new MouseEventHandler(OnHover);
                    txrlcv.MouseLeave += new MouseEventHandler(OnLeave);

                    Button userCurvBtn = new Button();
                    userCurvBtn.Name = "UsrcvBtn" + i.ToString();
                    userCurvBtn.Content = "User Curve";
                    userCurvBtn.HorizontalAlignment = HorizontalAlignment.Center;
                    userCurvBtn.VerticalAlignment = VerticalAlignment.Center;
                    userCurvBtn.Width = 100;
                    userCurvBtn.Click += new RoutedEventHandler(changeUserCurve);
                    additional.Add(userCurvBtn);
                    Grid.SetColumn(userCurvBtn, 14);
                    Grid.SetRow(userCurvBtn, i);
                    userCurveBtn[i] = userCurvBtn;
                    userCurvBtn.MouseEnter += new MouseEventHandler(OnHover);
                    userCurvBtn.MouseLeave += new MouseEventHandler(OnLeave);

                    if (currentBind != null)
                    {
                        if(currentBind.JAxis!=null&& currentBind.JAxis.Length > 0)
                        {
                            joybtnin.Content = currentBind.JAxis.Replace("JOY_","Axis-").ToString();
                        } 
                        else
                        {
                            joybtnin.Content = "ERROR PLEASE REASSIGN";
                        }
                        if(currentBind.Inverted != null)
                            cbx.IsChecked = currentBind.Inverted;
                        else
                        {
                            currentBind.Inverted = false;
                            cbx.IsChecked = false;
                        }
                        if (currentBind.Slider!=null)
                        {
                            cbxs.IsChecked = currentBind.Slider;
                        }
                        else
                        {
                            currentBind.Slider = false;
                            cbxs.IsChecked = false;
                        }
                        if (!double.IsNaN(currentBind.Deadzone))
                        {
                            txrl.Text = currentBind.Deadzone.ToString();
                        }
                        else
                        {
                            currentBind.Deadzone = 0.0;
                            txrl.Text = "0";
                        }
                        if ( !double.IsNaN(currentBind.SaturationX))
                        {
                            txrlsx.Text = currentBind.SaturationX.ToString();
                        }
                        else
                        {
                            currentBind.SaturationX = 1.0;
                            txrlsx.Text = "1";
                        }
                        if ( !double.IsNaN(currentBind.SaturationY))
                        {
                            txrlsy.Text = currentBind.SaturationY.ToString();
                        }
                        else
                        {
                            currentBind.SaturationY = 1.0;
                            txrlsy.Text = "1";
                        }
                        if(currentBind.Curvature!=null&& currentBind.Curvature.Count > 0&&!double.IsNaN(currentBind.Curvature[0]))
                        {
                            if (currentBind.Curvature.Count == 1)
                            {
                                txrlcv.Text = currentBind.Curvature[0].ToString();
                                cbxu.IsChecked = false;
                                grid.Children.Add(txrlcv);
                            }
                            else
                            {
                                cbxu.IsChecked = true;
                                grid.Children.Add(userCurvBtn);
                            }
                            
                        }
                        else
                        {
                            currentBind.Curvature = new List<double>();
                            currentBind.Curvature.Add(0);
                            txrlcv.Text = "0.0";
                        }                        
                    }
                    else
                    {
                        txrl.Text = "Deadzone (Dec)";
                        txrlsx.Text = "SatX (Dec)";
                        txrlsy.Text = "SatY (Dec)";
                        txrlcv.Text = "Curvature (Dec)";
                        cbxu.IsChecked = false;
                        grid.Children.Add(txrlcv);
                    }
                    txrlcv.LostFocus += new RoutedEventHandler(CurvitureSelectionChanged);
                    txrlsy.LostFocus += new RoutedEventHandler(SaturationYSelectionChanged);
                    txrlsx.LostFocus += new RoutedEventHandler(SaturationXSelectionChanged);
                    txrl.LostFocus += new RoutedEventHandler(DeadzoneSelectionChanged);
                    cbxs.Click += new RoutedEventHandler(SliderAxisSelection);
                    cbx.Click += new RoutedEventHandler(InvertAxisSelection);

                    txrlcv.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlsy.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlsx.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrl.QueryCursor += new QueryCursorEventHandler(CleanText);
                    txrlcv.QueryCursor += new QueryCursorEventHandler(CleanText);


                }
                else
                {
                    ComboBox modCbx1 = new ComboBox();
                    modCbx1.Name = "mod1cbx" + i.ToString();
                    modCbx1.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx1.VerticalAlignment = VerticalAlignment.Center;
                    modCbx1.Width = 150;
                    modCbx1.ItemsSource = allMods;
                    mods[i, 0] = modCbx1;
                    Grid.SetColumn(modCbx1, 11);
                    Grid.SetRow(modCbx1, i);
                    grid.Children.Add(modCbx1);
                    modCbx1.MouseEnter += new MouseEventHandler(OnHover);
                    modCbx1.MouseLeave += new MouseEventHandler(OnLeave);

                    ComboBox modCbx2 = new ComboBox();
                    modCbx2.Name = "mod2cbx" + i.ToString();
                    modCbx2.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx2.VerticalAlignment = VerticalAlignment.Center;
                    modCbx2.Width = 150;
                    modCbx2.ItemsSource = allMods;
                    mods[i, 1] = modCbx2;
                    Grid.SetColumn(modCbx2, 12);
                    Grid.SetRow(modCbx2, i);
                    grid.Children.Add(modCbx2);
                    modCbx2.MouseEnter += new MouseEventHandler(OnHover);
                    modCbx2.MouseLeave += new MouseEventHandler(OnLeave);

                    ComboBox modCbx3 = new ComboBox();
                    modCbx3.Name = "mod3cbx" + i.ToString();
                    modCbx3.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx3.VerticalAlignment = VerticalAlignment.Center;
                    modCbx3.Width = 150;
                    modCbx3.ItemsSource = allMods;
                    mods[i, 2] = modCbx3;
                    Grid.SetColumn(modCbx3, 13);
                    Grid.SetRow(modCbx3, i);
                    grid.Children.Add(modCbx3);
                    modCbx3.MouseEnter += new MouseEventHandler(OnHover);
                    modCbx3.MouseLeave += new MouseEventHandler(OnLeave);

                    ComboBox modCbx4 = new ComboBox();
                    modCbx4.Name = "mod4cbx" + i.ToString();
                    modCbx4.HorizontalAlignment = HorizontalAlignment.Center;
                    modCbx4.VerticalAlignment = VerticalAlignment.Center;
                    modCbx4.Width = 150;
                    modCbx4.ItemsSource = allMods;
                    mods[i, 3] = modCbx4;
                    Grid.SetColumn(modCbx4, 14);
                    Grid.SetRow(modCbx4, i);
                    grid.Children.Add(modCbx4);
                    modCbx4.MouseEnter += new MouseEventHandler(OnHover);
                    modCbx4.MouseLeave += new MouseEventHandler(OnLeave);


                    //Check against mod buttons needed
                    if (currentBind != null)
                    {
                        string btnraw = currentBind.JButton.Replace("JOY_BTN","Button-");
                        joybtnin.Content = btnraw;
                        if (currentBind.AllReformers.Count > 0)
                        {
                            modCbx1.SelectedItem = currentBind.AllReformers[0].Split('§')[0];
                            if (currentBind.AllReformers.Count > 1)
                            {
                                modCbx2.SelectedItem = currentBind.AllReformers[1].Split('§')[0];
                                if (currentBind.AllReformers.Count > 2)
                                {
                                    modCbx3.SelectedItem = currentBind.AllReformers[2].Split('§')[0];
                                    if (currentBind.AllReformers.Count > 3)
                                    {
                                        modCbx4.SelectedItem = currentBind.AllReformers[3].Split('§')[0];
                                    }
                                }
                            }
                        }
                    }
                    modCbx1.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx2.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx3.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                    modCbx4.SelectionChanged += new SelectionChangedEventHandler(modSelectionChanged);
                }
            }
            grid.ShowGridLines = true;
            sv.Content = grid;
        }
        void OnHover(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            int id = Convert.ToInt32(removeIDAdditionalPartsFromControls(c.Name));
            ColorRowOrange(id);
        }
        void OnLeave(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            int id = Convert.ToInt32(removeIDAdditionalPartsFromControls(c.Name));
            UncolorRow(id);
        }
        string removeIDAdditionalPartsFromControls(string cntrl)
        {
            cntrl = cntrl.Replace("lblname", "")
                .Replace("editBtn", "")
                .Replace("dupBtn", "")
                .Replace("deleteBtn", "")
                .Replace("GroupDropDown", "")
                .Replace("joyLbl", "")
                .Replace("assignBtn", "")
                .Replace("cbxrel", "")
                .Replace("cbxsrel", "")
                .Replace("mod1cbx", "")
                .Replace("mod2cbx", "")
                .Replace("mod3cbx", "")
                .Replace("mod4cbx", "")
                .Replace("txrldz", "")
                .Replace("txrlsatx", "")
                .Replace("txrlsaty", "")
                .Replace("txrlsacv", "")
                .Replace("UsrcvBtn", "");
            return cntrl;
        }
        void ColorRowOrange(int row)
        {
            if (relationLabels.Length <= row ||
                editBtns.Length <= row ||
                dupBtns.Length <= row ||
                dltBtns.Length <= row ||
                groupComboboxes.Length <= row ||
                stickLabels.Length <= row ||
                setBtns.Length <= row ||
                invertedcbs.Length <= row ||
                slidercbs.Length <= row ||
                usercurvecbs.Length <= row ||
                mods.GetLength(0) <= row ||
                tboxes.GetLength(0) <= row) return;
            relationLabels[row].Foreground = Brushes.Orange;
            editBtns[row].Background = Brushes.Orange;
            dupBtns[row].Background = Brushes.Orange;
            dltBtns[row].Background = Brushes.Orange;
            groupComboboxes[row].Background = Brushes.Orange;
            groupComboboxes[row].BorderBrush = Brushes.Orange;
            groupComboboxes[row].Foreground = Brushes.Orange;
            //groupComboboxes[row].ItemContainerStyle.Setters.Add(new)
            stickLabels[row].Foreground = Brushes.Orange;
            setBtns[row].Background = Brushes.Orange;
            if (invertedcbs[row] != null) invertedcbs[row].Foreground = Brushes.Orange;
            if (slidercbs[row] != null) slidercbs[row].Foreground = Brushes.Orange;
            if (usercurvecbs[row] != null) usercurvecbs[row].Foreground = Brushes.Orange;
            for(int i=0; i<4; ++i)
            {
                if (mods[row, i] != null) mods[row, i].Background = Brushes.Orange;
                if (tboxes[row, i] != null) tboxes[row, i].Background = Brushes.Orange;
            }
            if (userCurveBtn[row] != null) userCurveBtn[row].Background = Brushes.Orange;
        }
        void UncolorRow(int row)
        {
            if (relationLabels.Length <= row ||
                editBtns.Length <= row ||
                dupBtns.Length <= row ||
                dltBtns.Length <= row ||
                groupComboboxes.Length <= row ||
                stickLabels.Length <= row ||
                setBtns.Length <= row ||
                invertedcbs.Length <= row ||
                slidercbs.Length <= row ||
                usercurvecbs.Length <= row ||
                mods.GetLength(0) <= row ||
                tboxes.GetLength(0) <= row) return;
            relationLabels[row].Foreground = Brushes.White;
            editBtns[row].Background = Brushes.White;
            dupBtns[row].Background = Brushes.White;
            dltBtns[row].Background = Brushes.White;
            groupComboboxes[row].Background = Brushes.White;
            stickLabels[row].Foreground = Brushes.White;
            setBtns[row].Background = Brushes.White;
            if (invertedcbs[row] != null) invertedcbs[row].Foreground = Brushes.White;
            if (slidercbs[row] != null) slidercbs[row].Foreground = Brushes.White;
            if (usercurvecbs[row] != null) usercurvecbs[row].Foreground = Brushes.White;
            for (int i = 0; i < 4; ++i)
            {
                if (mods[row, i] != null) mods[row, i].Background = Brushes.White;
                if (tboxes[row, i] != null) tboxes[row, i].Background = Brushes.White;
            }
            if (userCurveBtn[row] != null) userCurveBtn[row].Background = Brushes.White;
        }
        void ManualBtnAxSet(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("assignBtn", ""));
            ManualJoystickAssign mja = new ManualJoystickAssign(CURRENTDISPLAYEDRELATION[indx]);
            DisableInputs();
            ALLWINDOWS.Add(mja);
            mja.Closing += new CancelEventHandler(WindowClosing);
            mja.Show();
        }

        void OpenJoystickCreateAliasVisual(object sender, MouseButtonEventArgs e)
        {
            DisableInputs();
            CreateJoystickAlias cja = new CreateJoystickAlias(visualLastJoystickSelected);
            ALLWINDOWS.Add(cja);
            cja.Show();

            cja.Closing += new CancelEventHandler(WindowClosing);
        }

        void OpenJoystickCreateAlias(object sender, EventArgs e)
        {
            DisableInputs();
            string name = ((Label)sender).Name;
            int indx = Convert.ToInt32(name.Replace("joyLbl", ""));
            if (CURRENTDISPLAYEDRELATION[indx].bind == null) return;
            string joystick = CURRENTDISPLAYEDRELATION[indx].bind.Joystick;
            CreateJoystickAlias cja = new CreateJoystickAlias(joystick);
            ALLWINDOWS.Add(cja);
            cja.Show();
            
            cja.Closing += new CancelEventHandler(WindowClosing);
        }
        void GroupManagementCheckboxChange(object sender, EventArgs e)
        {
            string name = ((CheckBox)sender).Name;
            string[] nameParts = name.Split('g');
            int relIndex = Convert.ToInt32(nameParts[0].Replace("r", ""));
            int grpIndex = Convert.ToInt32(nameParts[1]);
            if (CURRENTDISPLAYEDRELATION[relIndex].Groups == null) CURRENTDISPLAYEDRELATION[relIndex].Groups = new List<string>();
            if (((CheckBox)sender).IsChecked == true)
            {
                if(!CURRENTDISPLAYEDRELATION[relIndex].Groups.Contains(InternalDataManagement.AllGroups[grpIndex]))
                CURRENTDISPLAYEDRELATION[relIndex].Groups.Add(InternalDataManagement.AllGroups[grpIndex]);
            }
            else
            {
                InternalDataManagement.RemoveGroupFromSpecificRelation(CURRENTDISPLAYEDRELATION[relIndex].NAME, InternalDataManagement.AllGroups[grpIndex]);
            }
        }
        void duplicateRelation(object sender, EventArgs e)
        {
            int indx= Convert.ToInt32(((Button)sender).Name.Replace("dupBtn", ""));
            Relation r = CURRENTDISPLAYEDRELATION[indx];
            duplicateRelation(r);
        }
        public void duplicateRelation(Relation r)
        {
            string name = r.NAME;
            while (InternalDataManagement.DoesRelationAlreadyExist(name))
            {
                name = name + "1";
            }
            Relation nw = r.Copy();
            nw.NAME = name;
            if (nw.bind != null)
            {
                InternalDataManagement.AddBind(nw.NAME, nw.bind);
            }
            InternalDataManagement.AddRelation(nw);
        }
        public void CleanText(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            if (cx.Text == "Deadzone (Dec)" ||
                cx.Text == "SatX (Dec)" ||
                cx.Text == "SatY (Dec)" ||
                cx.Text == "Curvature (Dec)" ||
                cx.Text == "Button (Int o Pov)")
            {
                cx.Text = "";
            }
        }
        public void SetRelationsToView(List<Relation> li)
        {
            CURRENTDISPLAYEDRELATION = li;
            RefreshDropDowns();
            if (!MainStructure.VisualMode)
            {

                RefreshRelationsToShow();
                SetHeadersForScrollView();
            }
            else
            {
                RefreshVisualRelations();
            }
            sizeChanged(null,null);
        }
        void sortName(object o, EventArgs e)
        {
            if (selectedSort1 == "NAME_NORM")
            {
                selectedSort1 = "NAME_DESC";
            }
            else
            {
                selectedSort3 = selectedSort2;
                selectedSort2 = selectedSort1;
                selectedSort1 = "NAME_NORM";
            }
            InternalDataManagement.ResyncRelations();
        }
        void sortStick(object o, EventArgs e)
        {
            if (selectedSort1 == "STICK_NORM")
            {
                selectedSort1 = "STICK_DESC";
            }
            else
            {
                selectedSort3 = selectedSort2;
                selectedSort2 = selectedSort1;
                selectedSort1 = "STICK_NORM";
            }
            InternalDataManagement.ResyncRelations();
        }
        void sortBtn(object o, EventArgs e)
        {
            if (selectedSort1 == "BTN_NORM")
            {

                selectedSort1 = "BTN_DESC";
            }
            else
            {
                selectedSort3 = selectedSort2;
                selectedSort2 = selectedSort1;
                selectedSort1 = "BTN_NORM";
            }
            InternalDataManagement.ResyncRelations();
        }
        void SetHeadersForScrollView()
        {
            var converter = new GridLengthConverter();
            Grid grid = new Grid();
            colHds = new List<ColumnDefinition>();
            controls = new Control[gridCols];
            for (int i = 0; i < gridCols; ++i)
            {
                ColumnDefinition c = new ColumnDefinition();
                grid.ColumnDefinitions.Add(c);
                colHds.Add(c);
                c.MinWidth = colDefs[i].ActualWidth;
                c.MaxWidth = colDefs[i].ActualWidth;
            }


            Button relPick = new Button();
            relPick.Name = "joyHdrLblRlName";
            relPick.Content = "Relation Name";
            for (int i = 0; i < 200; ++i) relPick.Content = relPick.Content + " ";
            relPick.Foreground = Brushes.White;
            relPick.HorizontalAlignment = HorizontalAlignment.Stretch;
            relPick.VerticalAlignment = VerticalAlignment.Center;
            relPick.Background = Brushes.Orange;
            relPick.Click += new RoutedEventHandler(sortName);
            Grid.SetColumn(relPick, 0);
            grid.Children.Add(relPick);
            controls[0] = relPick;

            Label abc = new Label();
            abc.Name = "relationManagement";
            abc.Content = "Relation Management";
            abc.Foreground = Brushes.White;
            abc.HorizontalAlignment = HorizontalAlignment.Center;
            abc.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumnSpan(abc, 4);
            Grid.SetColumn(abc, 2);
            grid.Children.Add(abc);

            Button joystickPick = new Button();
            joystickPick.Name = "joyHdrLbldeviceName";
            joystickPick.Content = "Device Name";
            for (int i = 0; i < 200; ++i) joystickPick.Content = joystickPick.Content + " ";
            joystickPick.Foreground = Brushes.White;
            joystickPick.HorizontalAlignment = HorizontalAlignment.Stretch;
            joystickPick.VerticalAlignment = VerticalAlignment.Center;
            joystickPick.Background = Brushes.Orange;
            joystickPick.Click += new RoutedEventHandler(sortStick);
            Grid.SetColumn(joystickPick, 6);
            grid.Children.Add(joystickPick);
            controls[4] = joystickPick;

            Button joystickBtn = new Button();
            joystickBtn.Name = "joyHdrLblaxisname";
            joystickBtn.Content = "Axis/Btn Name";
            joystickBtn.Foreground = Brushes.White;
            joystickBtn.HorizontalAlignment = HorizontalAlignment.Center;
            joystickBtn.VerticalAlignment = VerticalAlignment.Center;
            joystickBtn.Background = Brushes.Orange;
            joystickBtn.Click += new RoutedEventHandler(sortBtn);
            Grid.SetColumn(joystickBtn, 7);
            grid.Children.Add(joystickBtn);
            controls[5] = joystickBtn;

            Label joystickAxisS = new Label();
            joystickAxisS.Content = "Axis Setting";
            joystickAxisS.Foreground = Brushes.White;
            joystickAxisS.HorizontalAlignment = HorizontalAlignment.Center;
            joystickAxisS.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumnSpan(joystickAxisS, 3);
            Grid.SetColumn(joystickAxisS, 8);
            grid.Children.Add(joystickAxisS);

            Label dzlbl = new Label();
            dzlbl.Content = "Mod1/Deadzone";
            dzlbl.Foreground = Brushes.White;
            dzlbl.HorizontalAlignment = HorizontalAlignment.Center;
            dzlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(dzlbl, 11);
            grid.Children.Add(dzlbl);

            Label sxlbl = new Label();
            sxlbl.Content = "Mod2/Sat X";
            sxlbl.Foreground = Brushes.White;
            sxlbl.HorizontalAlignment = HorizontalAlignment.Center;
            sxlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sxlbl, 12);
            grid.Children.Add(sxlbl);

            Label sylbl = new Label();
            sylbl.Content = "Mod3/Sat Y";
            sylbl.Foreground = Brushes.White;
            sylbl.HorizontalAlignment = HorizontalAlignment.Center;
            sylbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(sylbl, 13);
            grid.Children.Add(sylbl);

            Label curvlbl = new Label();
            curvlbl.Content = "Mod4/Curv";
            curvlbl.Foreground = Brushes.White;
            curvlbl.HorizontalAlignment = HorizontalAlignment.Center;
            curvlbl.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(curvlbl, 14);
            grid.Children.Add(curvlbl);

            svHeader.Content = grid;
        }
        void InvertAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxrel", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataManagement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Inverted = cx.IsChecked;
        }
        void SliderAxisSelection(object sender, EventArgs e)
        {
            CheckBox cx = (CheckBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("cbxsrel", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                cr = new Bind(CURRENTDISPLAYEDRELATION[indx]);
                InternalDataManagement.AddBind(cr.Rl.NAME, cr);
            }
            cr.Slider = cx.IsChecked;
        }
        void SaturationXSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsatx", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                {
                    MessageBox.Show("Please set first the button or the axis.");
                }
                else
                {
                    InternalDataManagement.ResyncRelations();
                }

                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.SaturationX = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given SaturationX not a valid double");
            }
        }
        void SaturationYSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsaty", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataManagement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.SaturationY = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given SaturationY not a valid double");
            }
        }
        void CurvitureSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrlsacv", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataManagement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            double curv = double.NaN;
            string cleaned = cx.Text.Replace(',', '.');
            bool succ = false;
            try
            {
                curv = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
                succ = true;
            }
            catch
            {
                MessageBox.Show("Given Curviture not a valid double");
            }
            if (succ == true)
            {
                if (cr.Curvature.Count > 0) cr.Curvature[0] = curv;
                else cr.Curvature.Add(curv);
            }
        }
        void DeadzoneSelectionChanged(object sender, EventArgs e)
        {
            TextBox cx = (TextBox)sender;
            int indx = Convert.ToInt32(cx.Name.Replace("txrldz", ""));
            Bind cr = InternalDataManagement.GetBindForRelation(CURRENTDISPLAYEDRELATION[indx].NAME);
            if (cr == null)
            {
                if (cx.Text.Length > 0)
                    MessageBox.Show("Please set first the button or the axis.");
                else
                    InternalDataManagement.ResyncRelations();
                return;
            }
            if (cx.Text.Length < 1 || cx.Text.Replace(" ", "") == ".") return;
            string cleaned = cx.Text.Replace(',', '.');
            try
            {
                cr.Deadzone = Convert.ToDouble(cleaned, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                MessageBox.Show("Given Deadzone not a valid double");
            }
        }
        void ProgramClosing(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
        void WindowClosing(object sender, EventArgs e)
        {
            Window s = (Window)sender;
            ALLWINDOWS.Remove((Window)sender);
            ActivateInputs();
            InternalDataManagement.ResyncRelations();
        }
        void OpenRelation(object sender, EventArgs e)
        {
            DisableInputs();
            RelationWindow rw = new RelationWindow();
            ALLWINDOWS.Add(rw);
            rw.Show();
            rw.Closed += new EventHandler(WindowClosing);
        }
        void FirstStart()
        {
            MainStructure.InitProgram();
            InternalDataManagement.NewFile(true);
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = false;
            MainStructure.LoadMetaLast();
            ActivateInputs();

        }
        void setWindowPosSize(object sender, EventArgs e)
        {
            Console.WriteLine("Should set");
            if (MainStructure.msave != null && MainStructure.msave._MainWindow.Width > 0)
            {
                this.Top = MainStructure.msave._MainWindow.Top;
                this.Left = MainStructure.msave._MainWindow.Left;
                this.Width = MainStructure.msave._MainWindow.Width;
                this.Height = MainStructure.msave._MainWindow.Height;
                Console.WriteLine("Done set");
                CBNukeUnused.IsChecked = MainStructure.msave.NukeSticks;
            }
        }
        void ActivateInputs(object sender, EventArgs e)
        {
            ActivateInputs();
        }
        void ActivateInputs()
        {
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = true;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = true;
                    editBtns[i].IsEnabled = true;
                    setBtns[i].IsEnabled = true;
                    dupBtns[i].IsEnabled = true;
                }
            if (mods != null)
            {
                for(int i=0; i<mods.GetLength(0); ++i)
                {
                    for(int j=0; j<mods.GetLength(1); ++j)
                    {
                        if (mods[i, j] != null)
                            mods[i, j].IsEnabled = true;
                    }
                }
            }
            if (additional != null)
            {
                for(int i=0; i<additional.Count; ++i)
                {
                    if (additional[i] != null)
                        additional[i].IsEnabled = true;
                }
            }
            foreach (KeyValuePair<ComboBox, string> kvp in deviceLookup)
            {
                kvp.Key.IsEnabled = true;
            }
        }
        void DisableInputs()
        {
            for (int i = 0; i < ALLBUTTONS.Count; ++i)
                ALLBUTTONS[i].IsEnabled = false;
            if (dltBtns != null)
                for (int i = 0; i < dltBtns.Length; ++i)
                {
                    dltBtns[i].IsEnabled = false;
                    editBtns[i].IsEnabled = false;
                    setBtns[i].IsEnabled = false;
                    dupBtns[i].IsEnabled = false;
                }
            if (mods != null)
            {
                for (int i = 0; i < mods.GetLength(0); ++i)
                {
                    for (int j = 0; j < mods.GetLength(1); ++j)
                    {
                        if (mods[i, j] != null)
                            mods[i, j].IsEnabled = false;
                    }
                }
            }
            if (additional != null)
            {
                for (int i = 0; i < additional.Count; ++i)
                {
                    if (additional[i] != null)
                        additional[i].IsEnabled = false;
                }
            }
            foreach(KeyValuePair<ComboBox, string> kvp in deviceLookup)
            {
                kvp.Key.IsEnabled = false;
            }
        }
        private void InstanceSelectionChanged(object sender, EventArgs e)
        {
            Console.WriteLine(sender.GetType().ToString());
            if (((ComboBox)sender).SelectedIndex < 0) return;
            MiscGames.DCSInstanceSelectionChanged((string)DropDownInstanceSelection.SelectedItem);
        }    
        void PrintLayout(object sender, EventArgs e)
        {
            if (CURRENTDISPLAYEDRELATION.Count < 1) return;
            DisableInputs();
            CollectJoystickImages cjii = new CollectJoystickImages();
            cjii.Show();
            cjii.Closing += new CancelEventHandler(ActivateInputs);
        }
        void OpenOverlaySettings(object sender, EventArgs e)
        {
            DisableInputs();
            OverlaySettings os = new OverlaySettings();
            os.Show();
            os.Closing += new CancelEventHandler(ActivateInputs);
        }
        public void OpenOverlay(object sender, EventArgs e)
        {
            if (!overlay_opened)
            {
                ow = new OverlayWindow();
                ow.Show();
                ow.Closing += new CancelEventHandler(CloseOverlay);
                MainStructure.OverlayWorker.overlay = ow;
                if (MainStructure.msave.OvlBtnChangeMode)
                {
                    MainStructure.joystickInputRead = new Thread(MainStructure.JrContReading.StartReadingInputsChangeMode);
                }
                else
                {
                    MainStructure.joystickInputRead = new Thread(MainStructure.JrContReading.StartReadingInputsCurrentMode);
                }
                MainStructure.joystickInputRead.Name = "InputReader";
                MainStructure.JrContReading.KeepDaemonRunning = true;
                MainStructure.OverlayWorker.keepDisplayBackgroundWorkerRunning = true;
                MainStructure.OverlayWorker.keepDisplayDispatcherRunning = true;
                MainStructure.joystickInputRead.Start();
                MainStructure.DisplayBackgroundWorker = new System.Threading.Thread(MainStructure.OverlayWorker.StartDisplayBackgroundWorker);
                MainStructure.DisplayBackgroundWorker.Name = "DisplayBackgroundWorker";
                MainStructure.DisplayBackgroundWorker.Start();
                MainStructure.DisplayDispatcherWorker = new System.Threading.Thread(MainStructure.OverlayWorker.StartDisplayDispatcher);
                MainStructure.DisplayDispatcherWorker.Name = "DisplayDispatcher";
                MainStructure.DisplayDispatcherWorker.Start();
                overlay_opened = true;
                OverlayBtn.Background = Brushes.Orange;
            }
            else
            {
                ow.Close();   
            }
        }
        void CloseOverlay(object sender, EventArgs e)
        {
            MainStructure.JrContReading.KeepDaemonRunning = false;
            MainStructure.OverlayWorker.keepDisplayBackgroundWorkerRunning = false;
            MainStructure.OverlayWorker.keepDisplayDispatcherRunning = false;
            MainStructure.DisplayDispatcherWorker.Abort();
            MainStructure.DisplayBackgroundWorker.Abort();
            MainStructure.joystickInputRead.Abort();
            OverlayBtn.Background = Brushes.White;
            overlay_opened = false;
        }
        void ShutThreads(object sender, EventArgs e)
        {
            
            if(MainStructure.DisplayBackgroundWorker!=null)MainStructure.DisplayBackgroundWorker.Abort();
            if(MainStructure.DisplayDispatcherWorker != null) MainStructure.DisplayDispatcherWorker.Abort();
            if(MainStructure.DCSServerSocket!=null)MainStructure.DCSServerSocket.Abort();
            if(MainStructure.joystickInputRead!=null)MainStructure.joystickInputRead.Abort();
            if(MainStructure.runningGameCheck!=null)MainStructure.runningGameCheck.Abort();
            Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
        }
        void SwitchVisualMode(object sender, EventArgs e)
        {
            if (MainStructure.VisualMode)
            {
                scaleTBox.Visibility = Visibility.Hidden;
                scaleLbl.Visibility = Visibility.Hidden;
                MainStructure.VisualMode = false;
                VisualAssigningModeBtn.Content = "Visual Mode";
                InternalDataManagement.ResyncRelations();
            }
            else
            {
                scaleTBox.Visibility = Visibility.Visible;
                scaleLbl.Visibility = Visibility.Visible;
                scaleTBox.Text = MainStructure.ScaleFactor.ToString();
                CollectSticksForVisual csfv = new CollectSticksForVisual();
                csfv.Show();
                VisualAssigningModeBtn.Content = "Table Mode";
            }
        }
        void SetLayer(object sender, EventArgs e)
        {
            int indx = Convert.ToInt32(((Button)sender).Name.Replace("LayerBtn", ""));
            MainStructure.VisualLayer = indx - 1;
            InternalDataManagement.ResyncRelations();
        }

        void SetNewScaleFactor(object sender, EventArgs e)
        {
            try
            {
                MainStructure.ScaleFactor = Convert.ToDouble(scaleTBox.Text, new CultureInfo("en-US"));
                InternalDataManagement.ResyncRelations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Not a valid double");
            }
        }
    }
}
