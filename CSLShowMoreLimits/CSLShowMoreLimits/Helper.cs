﻿using ICities;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.Packaging;
using ColossalFramework.Steamworks;
using ColossalFramework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSLShowMoreLimits
{
    public class Helper
    {
        public class ExternalData
        {
            public bool CoDataRefreshEnabled=false;
            public bool CoDisplayRefreshEnabled=false;
            public string cachedname ="n/a";
            public string name= "n/a";
            public string tag = "n/a";
            public bool isVisable;
            public bool isAutoRefreshActive;

            public object[] ToStringArray() 
            {
                object[] tmpStrArr = new object[] { name.ToString(), cachedname.ToString(),
                 tag.ToString(), isAutoRefreshActive.ToString(), isVisable.ToString(),
                CoDataRefreshEnabled.ToString(),CoDisplayRefreshEnabled.ToString(), };
                return tmpStrArr;
            }
        }

        [Flags]
        public enum DumpOption : byte
        {
            None = 0,
            Default = 1,
            MapLoaded = 2,
            OptionsOnly = 4,
            DebugInfo = 8,
            UseSeperateFile = 16,
            VehicleData = 32,
            GUIActive = 64,
            ExtendedInfo = 128,
            All = 255
        }
        private const string DumpStatsHeader = "\r\n---------- CSL Show More Limits StatsDump ----------\r\n";
        private const string DumpVersion = "ModVersion: {0}   Current DateTime: {1}\r\n";

        private const string dbgDumpstr1 = "\r\n----- Debug info -----\r\n";
        private const string dbgDumpstr2 = "DebugEnabled: {0}  DebugLogLevel: {1}  isGuiEnabled {2}  AutoRefreshEnabled {3} \r\n";   
        private const string dbgDumpstr3 = "IsEnabled: {4}  IsInited: {5}  isGuiRunning {6} \r\n";
        private const string dbgDumpstr4 = "UseAutoRefreshOption: {7}  AutoRefreshSeconds: {8}  GUIOpacity: {9} \r\n";
        private const string dbgDumpstr5 = "CheckStatsEnabled: {10}  UseCustomLogfile: {11}  DumpLogOnMapEnd: {12} \r\n";
        private const string dbgDumpstr6 = "UseCustomDumpFile: {13}  DumpFileFullpath: {14}\r\nCustomLogfilePath: {15}  \r\n";
        private const string dbgDumpstrGUIExtra1 = "IsAutoRefreshActive {3}  CoroutineCheckStats: {5}  CoRoutineDisplayData: {6} \r\n";
        private const string dbgDumpstrGUIExtra2 = "NewGameAppVersion: {0}  CityName: {1}  Paused: {2}  Mode:{3}\r\n";
        private const string dbgDumpPaths = "Path Info: \r\n AppBase: {0} \r\n AppExe: {1} \r\n Mods: {2} \r\n Saves: {3} \r\n gContent: {4} \r\n AppLocal: {5} \r\n";

        private const string dbgGameVersion = "UnityProd: {0}  UnityPlatform: {1} \r\nProductName: {2}  ProductVersion: {3}  ProductVersionString: {4}\r\n";

        private const string sbgMapLimits1 = "#NetSegments: {0} | {1}   #NetNodes: {2} | {3}  #NetLanes: {4} | {5} \r\n";
        private const string sbgMapLimits2 = "#Buildings: {0} | {1}  #ZonedBlocks: {2} | {3} \r\n";
        private const string sbgMapLimits3 = "#Transportlines: {4}  #UserProps: {5}  #PathUnits: {6} \r\n#Areas: {8}  #Districts: {9} #Tress: {10}\r\n#BrokenAssets: {7}\r\n";
        private const string sbgMapLimits4 = "#Citizens: {0}  #Families: {1}  #ActiveCitzenAgents: {2} \r\n";
        private const string sbgMapLimits5 = "#Vehicles: {1}  #ParkedCars: {0} \r\n";


        private static object[] tmpVer;
//        private static object[] tmpVehc;
        private static object[] tmpPaths;
        private static object[] tmpdbg;
        private static object[] tmpGuiExtra;
        private static object[] tmpGuiExtra2;

        //should be enough for most log messages and we want this guy in the HFHeap.
        private static StringBuilder logSB = new System.Text.StringBuilder(512); 

        public static void LogExtentedWrapper(DumpOption bMode)
        {
            StringBuilder sb = new StringBuilder((bMode | DumpOption.GUIActive) == bMode ? (Mod.config.IncludePackageInfoInDump ? 262144 : 16384) : 8192);
            RefreshSourceData(bMode);
            buildTheString(sb,bMode);
            DumpStatsToLog(sb.ToString(),bMode);
        }

        
        private static void RefreshSourceData(DumpOption bMode)
        {
            //Version & Platform data
            tmpVer = new object[] { Application.productName, Application.platform.ToString(),
                DataLocation.productName, DataLocation.productVersion.ToString(), 
                DataLocation.productVersionString };
            //PathData
            tmpPaths = new object[]{DataLocation.applicationBase,DataLocation.executableDirectory,
                DataLocation.modsPath,DataLocation.saveLocation,DataLocation.gameContentPath,DataLocation.localApplicationData};


/*            //VehicleData
            tmpVehc = new object[]{ ((bMode | DumpOption.MapLoaded) == bMode)? Singleton<VehicleManager>.instance.m_vehicleCount.ToString() : "n\\a",
                    Mod.RESERVEAMOUNT.ToString(),
                    ((bMode | DumpOption.MapLoaded) == bMode) ? (Singleton<VehicleManager>.instance.m_vehicles.m_size - 1).ToString() : "16383",
                    (16383 - Mod.RESERVEAMOUNT).ToString(),Mod.timesReservedAttempted.ToString(),
                    Mod.timesReserveAttemptFailed.ToString(), Mod.timesLimitReached.ToString(),
                    Mod.timesFailedByReserve.ToString(), Mod.timesFailedToCreate.ToString(), Mod.timesCV_CalledTotal.ToString()};

*/
            //debugdata
            tmpdbg = new object[]{Mod.DEBUG_LOG_ON.ToString(),Mod.DEBUG_LOG_LEVEL.ToString(),Mod.IsGuiEnabled.ToString() ,
                Mod.UseAutoRefreshOption.ToString(), Mod.IsEnabled.ToString(),Mod.IsInited.ToString(),
                Loader.isGuiRunning.ToString(), Mod.UseAutoRefreshOption.ToString(),
                Mod.AutoRefreshSeconds.ToString("F2"),Mod.config.GuiOpacity.ToString("F04"), Mod.config.CheckStatsForLimitsEnabled.ToString(),
                Mod.config.UseCustomLogFile.ToString(), Mod.config.DumpStatsOnMapEnd.ToString(),Mod.config.UseCustomDumpFile,
                Mod.config.DumpStatsFilePath,Mod.config.CustomLogFilePath};

            if ((bMode | Helper.DumpOption.GUIActive) == bMode)
            {            //gui mode exclusive
                tmpGuiExtra2 = new object[]{(Singleton<SimulationManager>.instance.m_metaData != null) ? Singleton<SimulationManager>.instance.m_metaData.m_newGameAppVersion.ToString():"n/a",
                (Singleton<SimulationManager>.instance.m_metaData.m_CityName != null) ? Singleton<SimulationManager>.instance.m_metaData.m_CityName.ToString():"n/a",
                (Singleton<SimulationManager>.exists == true) ? Singleton<SimulationManager>.instance.SimulationPaused.ToString():"n/a",
                (Singleton<ToolManager>.instance != null) ? (Singleton<ToolManager>.instance.m_properties.CurrentTool !=null ? Singleton<ToolManager>.instance.m_properties.m_mode.ToString():"n/a"):"n/a"};

                Helper.ExternalData Mytmp;
                Mytmp = CSLShowMoreLimitsGUI.GetInternalData();
                tmpGuiExtra = Mytmp.ToStringArray();
                //CSLServiceReserveGUI.GetInternalData.ToStringArray();
            }
        }


        private  static void AddGetPluginList(StringBuilder sb)
        {
            int tmpcount = 0;
            StringBuilder tmpSB = new StringBuilder(2048);
            sb.AppendLine("\r\n----- Enabled Mod List ------");
            try
            {

                foreach (PluginManager.PluginInfo p in Singleton<PluginManager>.instance.GetPluginsInfo())
                {
                    IUserMod[] tmpInstances = p.GetInstances<IUserMod>();
                    if (p.isEnabled)
                    {
                        if (tmpInstances.Length == 1)
                        { sb.AppendLine(string.Concat("Mod Fullname: ", tmpInstances[0].Name, "  Description: ",
                            tmpInstances[0].Description)); }
                        else
                        { sb.AppendLine(string.Concat("(***MultipleInstances***)Mod Fullname: ", tmpInstances[0].Name,
                            "  Description: ", tmpInstances[0].Description)); }
                        sb.AppendLine(string.Concat("LocalName: ", p.name.ToString(), "  WorkShopID: ", p.publishedFileID.AsUInt64.ToString(), "  AssemblyPath: ", p.modPath, p.assembliesString));
                        tmpcount++;
                    }
                    else
                    {
                        if(Mod.config.IncludeDisabledModsInDumps)
                        {
                            if (tmpInstances.Length == 1)
                            {
                                tmpSB.AppendLine(string.Concat("Mod Fullname: ", tmpInstances[0].Name, "  Description: ",
                                  tmpInstances[0].Description));
                            }
                            else
                            {
                                tmpSB.AppendLine(string.Concat("(***MultipleInstances***)Mod Fullname: ", tmpInstances[0].Name,
                                  "  Description: ", tmpInstances[0].Description));
                            }
                            tmpSB.AppendLine(string.Concat("LocalName: ", p.name.ToString(), "  WorkShopID: ", p.publishedFileID.AsUInt64.ToString(), "  AssemblyPath: ", p.modPath, p.assembliesString));
                        }
                    }
                }
                sb.AppendLine(string.Format("* {0} Plugins\\Mods are enabled of {1} installed.", tmpcount.ToString(), Singleton<PluginManager>.instance.modCount.ToString()));
                if (Mod.config.IncludeDisabledModsInDumps)
                {
                    sb.AppendLine("\r\n----- Disabled mods -----");
                    sb.Append(tmpSB.ToString());
                }
                
            }
            catch (Exception ex)
            { Helper.dbgLog("Error getting list of plugins.",ex,true); }

            
            sb.AppendLine("--------End Plugins--------\r\n");
//            if(Singleton<SimulationManager>.exists)
//            {
//                string abc = Singleton<SimulationManager>.instance.SimulationPaused.ToString();
//                    string abc2 = Singleton<SimulationManager>.instance.m_metaData.m_MapName.ToString();
//                    string abc3 = Singleton<SimulationManager>.instance.m_metaData.m_CityName.ToString();
//                    string abc4 = String.Format("AppFullVersion: {0}  AppDataFormatVersion: {1}",BuildConfig.applicationVersionFull,BuildConfig.DATA_FORMAT_VERSION.ToString());
//                   string abc5 = string.Format("UnlockDLCAssets: {0} IgnoreDLCAssets{1}",Singleton<LoadingManager>.instance.m_unlockDlcAssets.ToString(),Singleton<LoadingManager>.instance.m_ignoreDlcAssets.ToString());
//                    string abc5 = string.Format("BrokenAssets: {0} \n IgnoreDLCAssets{1}",Singleton<LoadingManager>.instance.m_brokenAssets.ToString(),Singleton<LoadingManager>.instance.ToString());
//                    string ab = Steam.workshop.GetSubscribedItems
//            }
        }


        private static void DoSubscribedItems()
        {
/*          if (Steam.active == true && Steam.workshop != null && PackageManager.noWorkshop == false)
            {
                PublishedFileId[] SteamSubItems = Steam.workshop.GetSubscribedItems();
                int tWksCount = SteamSubItems.Count();
                int folderwithfiles = 0;
                int filecount = 0;
                long Totalbytes = 0;
                if (tWksCount > 0)
                {
                    for (int i = 0; i < SteamSubItems.Length; i++)
                    {
                        string subscribedItemPath = Steam.workshop.GetSubscribedItemPath(SteamSubItems[i]);
                        if (subscribedItemPath != null)
                        {
                            DirectoryInfo folders = new DirectoryInfo(subscribedItemPath);
                            FileInfo[] files = folders.GetFiles();
                            if (files.Length > 0)
                            {
                                foreach (FileInfo f in files)
                                {
                                    filecount++;
                                    if (Path.GetExtension(f.FullName) == PackageManager.packageExtension)
                                    {
                                        Totalbytes += f.Length;
                                        folderwithfiles++;
                                    }

                                }
                            }
                            DirectoryInfo[] folders2 = folders.GetDirectories();
                            if(folders2.Count() > 0) 
                            {
                                foreach (DirectoryInfo di in folders2)
                                {
                                    foreach (FileInfo f in files)
                                    {
                                        filecount++;
                                        if (Path.GetExtension(f.FullName) == PackageManager.packageExtension)
                                        {
                                            Totalbytes += f.Length;
                                            folderwithfiles++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                object[] wksobj = new object[] { tWksCount.ToString(), folderwithfiles.ToString(), filecount.ToString(), (Totalbytes / 1024).ToString() };
                sb.AppendFormat("\r\nYou are subscribed to {0} workshop items, \r\n stored in about {1} folders, containing about {2} total files\r\n with a total size of {3}kb .", wksobj);
            }
*/ 
        }

        private static StringBuilder DoAllPackages()
        {
            ////
            //PackageManager  PkgMrg = Singleton<PackageManager>..instance ;
            //PkgMrg.m
            StringBuilder pSB = new StringBuilder(262144);
            if (pSB != null)
            {
                if (Steam.active == true && Steam.workshop != null && PackageManager.noWorkshop == false)
                {
                    //                        PackageManager ppp = Singleton<PackageManager>.instance;
                    //                        List<FileSystemReporter> hhh = (List<FileSystemReporter>)typeof(PackageManager).GetField("m_FileSystemReporters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ppp);//.GetValue(ppp);
                    //                        dbgLog("Number of FileSystemReporters: " + hhh.Count().ToString());

                    //                        foreach (FileSystemReporter fsr in hhh)
                    //                        {
                    //                            WatchersCollection jjj = new WatchersCollection();
                    //                            WatchersCollection jjj = (WatchersCollection)typeof(FileSystemReporter).GetField("m_Watchers",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(
                    //                          }

                    if (Mod.DEBUG_LOG_ON) dbgLog("PackageManager.allPackages.Count: " + PackageManager.allPackages.Count().ToString());
                    //PMgr = Singleton<PackageManager>;
                    int tmpPackageCount = 0;
                    int tmpAssetCount = 0;
                    long tmpPackagesSize = 0;
                    pSB.AppendLine("\r\n----- Packages Installed list -----\r\n");
                    foreach (Package pkg in PackageManager.allPackages)
                    {
                        tmpPackageCount++;
                        pSB.Append("\r\nName: " + pkg.packageName + " Author: " + pkg.packageAuthor + " MainAssett: " + pkg.packageMainAsset +
                            "\r\nPath: " + pkg.packagePath + "  Version: " + pkg.packageVersionStr + "\r\n");
                        Dictionary<string, Package.Asset> bbb = (Dictionary<string, Package.Asset>)typeof(Package).GetField("m_IndexTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pkg);
                        foreach (KeyValuePair<string, Package.Asset> kvp in bbb)
                        {
                            Package.Asset pa = kvp.Value;
                            tmpPackagesSize += pa.size;
                            pSB.Append(" PkgAsset_idx: " + kvp.Key.ToString() + "  PkgAsset_name: " + pa.name + " PkgAsset_Type: " + pa.type.ToString() +
                                "\r\n  PathOnDisk: " + pa.pathOnDisk + "  Size: " + pa.size.ToString() + "  isWorkshop: " + pa.isWorkshopAsset.ToString() + "\r\n");
                            tmpAssetCount++;
                        }
                    }

                    pSB.AppendLine(String.Concat("* There were ", tmpPackageCount.ToString(), " packages, containing ",
                        tmpAssetCount.ToString(), " assets, totalling ", (tmpPackagesSize / 1024).ToString(), " KiB in size."));
                }
            }
            return pSB;
        }

        private static string buildTheString(StringBuilder sb,DumpOption bMode)
        {
            try
            {
//                Debug.Log(string.Concat("[CSLServiceReserve.Helper] elements tmpVer:", tmpVer.Length.ToString(),
//                    " tmpPaths:", tmpPaths.Length.ToString()," tmpVehc:", tmpVehc.Length.ToString(),
//                    " tmpdbg:",tmpdbg.Length.ToString()," tmpGuiExtra:", tmpGuiExtra.Length.ToString(), " tmpguiExtra2:",tmpGuiExtra2.Length.ToString() ));

                // Do our header & Mod version info if Default.
                sb.Append(DumpStatsHeader);

                if ((bMode | DumpOption.Default) == bMode)
                {
                    sb.Append(String.Format(DumpVersion, Mod.VERSION_BUILD_NUMBER, DateTime.Now.ToString()));
                    sb.AppendLine(String.Format("CSLAppFullVersion: {0}  AppDataFormatVersion: {1}", BuildConfig.applicationVersionFull.ToString(), BuildConfig.DATA_FORMAT_VERSION.ToString()));
                    if (Steam.active)
                    {
                        sb.AppendLine(String.Concat("ContentAllowed: ", Steam.appID.ToString(), Steam.IsDlcInstalled(346791) ? ", Deluxe" : "",
                            Steam.IsDlcInstalled(365040) ? ", preorder extras?" : "", Steam.IsDlcInstalled(369150) ? ", AfterDark" : "", "\r\n"));
                    }
                }

                //dump Version and Path data if DebugInfo enabled.
                if ((bMode | DumpOption.DebugInfo) == bMode)
                {
                    sb.Append("raw commandline: " + CommandLine.raw.ToString() + "\r\n");
                    sb.Append(string.Format(dbgGameVersion, tmpVer));
                    sb.Append(String.Format(dbgDumpPaths, tmpPaths));
                }


                //debug into
                if ((bMode | DumpOption.DebugInfo) == bMode)
                {
                    sb.Append(String.Format(string.Concat(dbgDumpstr1,dbgDumpstr2, dbgDumpstr3,
                    dbgDumpstr4,dbgDumpstr5,dbgDumpstr6), tmpdbg));
                }

                //gui | map for sure loaded related things.
                if ((bMode | DumpOption.GUIActive) == bMode & (bMode | DumpOption.DebugInfo) == bMode)
                {
                    sb.Append(String.Format(dbgDumpstrGUIExtra1 ,tmpGuiExtra));
                    sb.Append(String.Format(dbgDumpstrGUIExtra2, tmpGuiExtra2));
                }
                if ((bMode | DumpOption.MapLoaded) == bMode)
                {
                    AddLimitData(0, sb);
                }

                //dump pluging\mod info
                if ((bMode | DumpOption.ExtendedInfo) == bMode)
                {
                    AddGetPluginList(sb);

                    if (Mod.config.IncludePackageInfoInDump & (bMode | DumpOption.GUIActive) == bMode)
                    {
                        sb.Append(DoAllPackages().ToString());
                    }
                }
                sb.AppendLine("--------End Dump--------\r\n");

            }
            catch (Exception ex)
            {
                dbgLog("Error:\r\n",ex,true);
            }

            if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL >= 2) dbgLog("Built the log string to dump.");
            return sb.ToString();
        }

        /// <summary>
        /// Adds building and network limit information into the string builder stream... we added other data why not this.
        /// </summary>
        /// <param name="sb">an already created stringbuilder object.</param>
        public static object[] AddLimitData(byte bObjFlag=0,StringBuilder sb = null)
        {
            try
            {
                object[] tmpdata;
                if (bObjFlag == 0 & sb != null)
                {
                    sb.AppendLine("\r\n----- Map Counter and Object Limit Data -----\r\n");
                }
                if (bObjFlag == 0 || bObjFlag == 2)
                {
                    NetManager tMgr = Singleton<NetManager>.instance;
                    tmpdata = new object[]{tMgr.m_segmentCount.ToString(), tMgr.m_segments.ItemCount().ToString(),
                    tMgr.m_nodeCount.ToString(), tMgr.m_nodes.ItemCount().ToString(), tMgr.m_laneCount.ToString(),
                    tMgr.m_lanes.ItemCount().ToString()};
                    if (bObjFlag == 2)
                    { return tmpdata; }
                    sb.AppendFormat(sbgMapLimits1, tmpdata);
                }

                if (bObjFlag == 0 || bObjFlag == 4)
                {
                    CitizenManager cMgr = Singleton<CitizenManager>.instance;
                    tmpdata = new object[] {cMgr.m_citizens.ItemCount().ToString(),cMgr.m_units.ItemCount().ToString(),
                    cMgr.m_instances.ItemCount().ToString() };
                    if (bObjFlag == 4)
                    { return tmpdata; }
                    sb.AppendFormat(sbgMapLimits4, tmpdata);
                }

                if (bObjFlag == 0 || bObjFlag == 8)
                {
                    tmpdata = new object[]{Singleton<BuildingManager>.instance.m_buildingCount.ToString(),
                    Singleton<BuildingManager>.instance.m_buildings.ItemCount().ToString(), Singleton<ZoneManager>.instance.m_blockCount.ToString(),
                    Singleton<ZoneManager>.instance.m_blocks.ItemCount(),Singleton<TransportManager>.instance.m_lines.ItemCount().ToString(),
                    Singleton<PropManager>.instance.m_props.ItemCount(),Singleton<PathManager>.instance.m_pathUnits.ItemCount(),
                    Singleton<LoadingManager>.instance.m_brokenAssets,Singleton<GameAreaManager>.instance.m_areaCount.ToString(),
                    Singleton<DistrictManager>.instance.m_districts.ItemCount().ToString(),Singleton<TreeManager>.instance.m_treeCount.ToString()};
                    if (bObjFlag == 8)
                    { return tmpdata; }
                    sb.AppendFormat(sbgMapLimits2, tmpdata);
                    sb.AppendFormat(sbgMapLimits3, tmpdata);
                }

                if (bObjFlag == 0 || bObjFlag == 16)
                {
                    tmpdata = new object[] { Singleton<VehicleManager>.instance.m_parkedCount.ToString(),
                Singleton<VehicleManager>.instance.m_vehicleCount.ToString(),Singleton<VehicleManager>.instance.m_vehicles.ItemCount().ToString()};
                    if (bObjFlag == 16)
                    { return tmpdata; }
                    sb.AppendFormat(sbgMapLimits5, tmpdata);
                }
                


            }
            catch(Exception ex)
            {
                dbgLog("Error:\r\n",ex,true);
            }
            return null;

        }


        /// <summary>
        /// Dumps our stats to a custom file or normal log file.
        /// </summary>
        /// <param name="strText">The string data.</param>
        /// <param name="bMode">The options flags that was used to create it (used to know if custom file or not)</param>
        public static void DumpStatsToLog(string strText, DumpOption bMode = 0)
        {
            try
            {
                string strTempPath = "";
                bool bDumpToLog = true;
                if ((bMode | DumpOption.UseSeperateFile) == bMode)
                {
                    if (Mod.DEBUG_LOG_LEVEL > 1) dbgLog("-----Using Seperate file mode flagged" + bMode.ToString() + "-----\r\n");
                    if (Mod.config.UseCustomDumpFile)
                    {
                        if (Mod.DEBUG_LOG_LEVEL > 1) { dbgLog("\r\n--------------Using Seperate file enabled-----\r\n"); }
                        if (Mod.DEBUG_LOG_LEVEL > 1)
                        {
                            dbgLog("\r\n\r\n[debugdata] ---" + Mod.config.DumpStatsFilePath + " existspath: " + 
                            Path.GetDirectoryName(Mod.config.DumpStatsFilePath) + "  combopath: " + 
                            Path.Combine(DataLocation.executableDirectory.ToString(), Mod.config.DumpStatsFilePath));
                        }
                        strTempPath = System.IO.Directory.Exists(Path.GetDirectoryName(Mod.config.DumpStatsFilePath)) ? Mod.config.DumpStatsFilePath.ToString() : Path.Combine(DataLocation.executableDirectory.ToString(), Mod.config.DumpStatsFilePath);
                        bDumpToLog = false; //flag us custom file.
                    }
                }

                if (bDumpToLog)
                {
                    if (Mod.DEBUG_LOG_ON) { dbgLog("\r\n Dumping to default game log."); }
                    Debug.Log(strText);
                }
                else
                {
                    if (Mod.DEBUG_LOG_ON) { dbgLog("\r\n Dumping to custom log. " + strTempPath); }
                    using (StreamWriter streamWriter = new StreamWriter(strTempPath, true))
                    {
                        streamWriter.WriteLine(strText);
                    }
                }
            }
            catch (Exception ex)
            {
                dbgLog("Error:\r\n",ex,true);
            }

        }


        /// <summary>
        /// Our LogWrapper...used everywhere.
        /// </summary>
        /// <param name="sText">Text to log</param>
        /// <param name="ex">An Exception - if not null it's basic data will be printed.</param>
        /// <param name="bDumpStack">If an Exception was passed do you want the stack trace?</param>
        /// <param name="bNoIncMethod">If for some reason you don't want the method name prefaced with the log line.</param>
        public static void dbgLog(string sText, Exception ex = null, bool bDumpStack = false, bool bNoIncMethod = false) 
        {
            try
            {
                logSB.Length = 0;
                string sPrefix = string.Concat("[", Mod.MOD_DBG_Prefix);
                if (bNoIncMethod) { string.Concat(sPrefix, "] "); }
                else
                {
                    System.Diagnostics.StackFrame oStack = new System.Diagnostics.StackFrame(1); //pop back one frame, ie our caller.
                    sPrefix = string.Concat(sPrefix, ":", oStack.GetMethod().DeclaringType.Name, ".", oStack.GetMethod().Name, "] ");
                }
                logSB.Append(string.Concat(sPrefix, sText));

                if (ex != null)
                {
                    logSB.Append(string.Concat("\r\nException: ", ex.Message.ToString()));
                }
                if (bDumpStack)
                {
                    logSB.Append(string.Concat("\r\nStackTrace: ", ex.ToString()));
                }
                if (Mod.config != null && Mod.config.UseCustomLogFile == true)
                {
                    string strPath = System.IO.Directory.Exists(Path.GetDirectoryName(Mod.config.CustomLogFilePath)) ? Mod.config.CustomLogFilePath.ToString() : Path.Combine(DataLocation.executableDirectory.ToString(), Mod.config.CustomLogFilePath);
                    using (StreamWriter streamWriter = new StreamWriter(strPath, true))
                    {
                        streamWriter.WriteLine(logSB.ToString());
                    }
                }
                else 
                {
                    Debug.Log(logSB.ToString());
                }
            }
            catch (Exception Exp)
            {
                Debug.Log(string.Concat("[CSLShowMoreLimits.Helper.dbgLog()] Error in log attempt!  ", Exp.Message.ToString()));
            }
        }
    }

}
