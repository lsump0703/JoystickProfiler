﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoyPro
{
    public static class InitGames
    {
        public static void InitDCSJoysticks()
        {
            List<string> Joysticks = new List<string>();
            if (Directory.Exists(MiscGames.DCSselectedInstancePath + "\\InputLayoutsTxt"))
            {
                string[] subs = Directory.GetDirectories(MiscGames.DCSselectedInstancePath + "\\InputLayoutsTxt");
                for (int j = 0; j < subs.Length; j++)
                {
                    string[] files = Directory.GetFiles(subs[j]);
                    for (int k = 0; k < files.Length; k++)
                    {
                        string[] parts = files[k].Split('\\');
                        string toCompare = parts[parts.Length - 1];
                        if (toCompare.EndsWith(".html"))
                        {
                            string toAdd = toCompare.Replace(".html", "");
                            if (!Joysticks.Contains(MiscGames.DCSStickNaming(toAdd)) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR")
                            {
                                Joysticks.Add(MiscGames.DCSStickNaming(toAdd));
                            }
                        }
                    }
                }
            }
            if (Directory.Exists(MiscGames.DCSselectedInstancePath + "\\Config\\Input"))
            {
                string[] subs = Directory.GetDirectories(MiscGames.DCSselectedInstancePath + "\\Config\\Input");
                for (int j = 0; j < subs.Length; j++)
                {
                    string[] inputs = Directory.GetDirectories(subs[j]);
                    for (int k = 0; k < inputs.Length; ++k)
                    {
                        string[] planes = Directory.GetFiles(inputs[k]);
                        for (int z = 0; z < planes.Length; ++z)
                        {
                            string[] parts = planes[z].Split('\\');
                            string toCompare = parts[parts.Length - 1];
                            if (toCompare.EndsWith(".diff.lua"))
                            {
                                string toAdd = toCompare.Replace(".diff.lua", "");
                                if (!Joysticks.Contains(MiscGames.DCSStickNaming(toAdd)) && toAdd != "Keyboard" && toAdd != "Mouse" && toAdd != "TrackIR") Joysticks.Add(MiscGames.DCSStickNaming(toAdd));
                            }
                        }
                    }
                }
            }
            if (InternalDataMangement.LocalJoysticks == null)
                InternalDataMangement.LocalJoysticks = Joysticks.ToArray();
            else
            {
                List<string> pp = InternalDataMangement.LocalJoysticks.ToList();
                for (int i = 0; i < Joysticks.Count; ++i)
                {
                    if (!pp.Contains(Joysticks[i]))
                        pp.Add(Joysticks[i]);
                }
                InternalDataMangement.LocalJoysticks = pp.ToArray();
            }
        }
        public static void InitIL2Joysticks()
        {
            List<string> Joysticks = new List<string>();
            if (File.Exists(MiscGames.IL2Instance + "\\data\\input\\devices.txt"))
            {
                StreamReader sr = new StreamReader(MiscGames.IL2Instance + "\\data\\input\\devices.txt");
                string content = sr.ReadToEnd().Replace("\r", "").Replace("|", "");
                string[] lines = content.Split('\n');
                List<string> crSticks = InternalDataMangement.LocalJoysticks.ToList();
                for (int i = 1; i < lines.Length; ++i)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length > 2 && int.TryParse(parts[0], out int id) == true)
                    {
                        string joy = MiscGames.IL2JoyIdToDCSJoyId(parts[1], parts[2]);
                        Joysticks.Add(joy);
                        if (!crSticks.Contains(joy))
                        {
                            crSticks.Add(joy);
                        }
                        if (!MiscGames.IL2JoystickId.ContainsKey(joy) && !MiscGames.IL2JoystickId.ContainsValue(id))
                        {
                            MiscGames.IL2JoystickId.Add(joy, id);
                        }

                    }
                }
                InternalDataMangement.LocalJoysticks = crSticks.ToArray();
            }
        }
        public static string GetDCSInstallationPath()
        {
            if (MiscGames.installPathsDCS.Length > 0)
            {
                if (MiscGames.installPathsDCS.Length == 1 && Directory.Exists(MiscGames.installPathsDCS[0]))
                {
                    return MiscGames.installPathsDCS[0];
                }
                else
                {
                    for (int i = 0; i < MiscGames.installPathsDCS.Length; ++i)
                    {
                        if (MiscGames.DCSselectedInstancePath.ToLower().Contains("openbeta") && MiscGames.installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(MiscGames.installPathsDCS[i]))
                        {
                            return MiscGames.installPathsDCS[i];
                        }
                        else if (!MiscGames.DCSselectedInstancePath.ToLower().Contains("openbeta") && !MiscGames.installPathsDCS[i].ToLower().Contains("beta") && Directory.Exists(MiscGames.installPathsDCS[i]))
                        {
                            return MiscGames.installPathsDCS[i];
                        }
                    }
                }
            }
            else
            {
                string basePath = "Program Files\\Eagle Dynamics\\DCS World";
                string openBetaExtention = " OpenBeta";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                for (int i = 0; i < allDrives.Length; ++i)
                {
                    string path = allDrives[i].Name + basePath;
                    if (MiscGames.DCSselectedInstancePath.ToLower().Contains("openbeta"))
                    {
                        path += openBetaExtention;
                    }
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }
            }
            return null;
        }
        public static string[] GetDCSUserFolders()
        {
            KnownFolder sg = KnownFolder.SavedGames;
            MiscGames.SaveGamesPath = KnownFolders.GetPath(sg);
            string[] dirs = Directory.GetDirectories(MiscGames.SaveGamesPath);
            List<string> candidates = new List<string>();
            for (int i = 0; i < dirs.Length; ++i)
            {
                string[] parts = dirs[i].Split('\\');
                string lastPart = parts[parts.Length - 1];
                if (lastPart.StartsWith("DCS")) candidates.Add(dirs[i]);
            }
            return candidates.ToArray();
        }
        public static void LoadIL2Path()
        {
            string pth = MainStructure.GetRegistryValue("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 307960", "InstallLocation", "LocalMachine");
            if (pth != null) MiscGames.IL2Instance = pth;
        }
        public static void InitIL2Data()
        {
            MainStructure.PROGPATH = Environment.CurrentDirectory;
            Console.WriteLine(MainStructure.PROGPATH);
            LoadIL2Path();
            if (MiscGames.IL2PathOverride.Length > 0)
            {
                MiscGames.IL2Instance = MiscGames.IL2PathOverride;
            }
            InitIL2Joysticks();
            DBLogic.PopulateIL2Dictionary();
            Console.WriteLine(MiscGames.IL2Instance);
        }
        public static void InitDCSData()
        {
            MainStructure.PROGPATH = Environment.CurrentDirectory;
            Console.WriteLine(MainStructure.PROGPATH);
            LoadDcsData();
            DCSIOLogic.LoadCleanLuasDCS();
            DCSIOLogic.LoadLocalDefaultsDCS();
            List<string> installs = new List<string>();
            string pth = MainStructure.GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            pth = MainStructure.GetRegistryValue("SOFTWARE\\Eagle Dynamics\\DCS World OpenBeta", "Path", "CurrentUser");
            if (pth != null) installs.Add(pth);
            MiscGames.installPathsDCS = installs.ToArray();
            MainStructure.mainW.DropDownInstanceSelection.Items.Clear();
            if (MiscGames.DCSInstanceOverride.Length > 0)
            {
                MainStructure.mainW.DropDownInstanceSelection.Items.Add(MiscGames.DCSInstanceOverride);
            }
            else
            {
                foreach (string inst in MiscGames.DCSInstances)
                    MainStructure.mainW.DropDownInstanceSelection.Items.Add(inst);
            }
        }
        public static void UnloadGameData()
        {
            DBLogic.DCSLib.Clear();
            MiscGames.installPathsDCS = new string[0];
            InternalDataMangement.LocalJoysticks = new string[0];
            DCSIOLogic.EmptyOutputsDCS = new Dictionary<string, DCSLuaInput>();
            MiscGames.IL2JoystickId = new Dictionary<string, int>();
            DBLogic.OtherLib = new Dictionary<string, Dictionary<string, OtherGame>>();
            MiscGames.IL2Instance = "";

        }
        public static void ReloadGameData()
        {
            UnloadGameData();
            InitDCSData();
            InitIL2Data();
            try
            {
                List<string> connectedSticks = JoystickReader.GetConnectedJoysticks();
                List<string> crSticks = InternalDataMangement.LocalJoysticks.ToList();
                for (int i = 0; i < connectedSticks.Count; ++i)
                {
                    if (!crSticks.Contains(connectedSticks[i]))
                        crSticks.Add(connectedSticks[i]);
                }
                InternalDataMangement.LocalJoysticks = crSticks.ToArray();
            }
            catch
            {

            }
        }
        public static void LoadDcsData()
        {
            DBLogic.PopulateDCSDictionaryWithProgram();
        }

    }
}
