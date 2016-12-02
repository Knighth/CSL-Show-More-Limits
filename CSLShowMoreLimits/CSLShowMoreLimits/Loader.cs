using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.Threading;
using ICities;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace CSLShowMoreLimits
{
	public class Loader : LoadingExtensionBase
	{
        public static UIView parentGuiView;
        public static CSLShowMoreLimitsGUI guiPanel;
        public static bool isGuiRunning = false;
        internal static LoadMode CurrentLoadMode;
        public Loader() { }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            try
            {
                if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("Reloading config before mapload."); }
                // *reload config values again after map load. This should not be problem atm.
                // *So long as we do this before OnLevelLoaded we should be ok;
                Mod.ReloadConfigValues(false, false);
            }
            catch (Exception ex)
            { Helper.dbgLog("Error:", ex, true); }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            CurrentLoadMode = mode;
            try
            {
                if (Mod.DEBUG_LOG_ON && Mod.DEBUG_LOG_LEVEL > 0) { Helper.dbgLog("LoadMode:" + mode.ToString()); }
                if (Mod.IsEnabled == true)
                {
                    // only setup redirect when in a real game
                /*  NewScenarioFromGame,8
                	NewScenarioFromMap,9
	                LoadScenario,10
	                NewGameFromScenario,11
	                UpdateScenarioFromGame,12
	                UpdateScenarioFromMap,13
                */
                    if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode==LoadMode.LoadMap ||mode==LoadMode.NewMap || mode== LoadMode.NewGame 
                        || (int)mode == 8 || (int)mode == 9 || (int)mode == 10 || (int)mode == 11 || (int)mode == 12 || (int)mode == 13)
                    {
                        if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("Asset modes not detcted"); }
                        if (Mod.IsGuiEnabled) { SetupGui(); } //setup gui if we're enabled.
                    }
                    int a = (int)CurrentLoadMode;
                }
                else
                {
                    //This should never happen.
                    if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("We fired when we were not even enabled active??"); }
                    if (Mod.IsGuiEnabled) { RemoveGui(); }
                }
            }
            catch(Exception ex)
            { Helper.dbgLog("Error:", ex, true); }
        }


        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            try
            {

                if (!(CurrentLoadMode == LoadMode.NewAsset || CurrentLoadMode == LoadMode.LoadAsset) & Mod.config.DumpStatsOnMapEnd) //Dump the Stats if we're told too.
                {
                    if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("OnLevelUnloading we've been asked to dump stats."); }
                    if (Mod.config.UseCustomDumpFile)
                    {
                        Helper.LogExtentedWrapper(Helper.DumpOption.Default | Helper.DumpOption.MapLoaded | Helper.DumpOption.UseSeperateFile |
                           (isGuiRunning ? Helper.DumpOption.GUIActive : Helper.DumpOption.None) | (Mod.config.IncludeModsInAfterMapDump ? Helper.DumpOption.ExtendedInfo : Helper.DumpOption.None));
                    }
                    else
                    {
                        Helper.LogExtentedWrapper(Helper.DumpOption.Default | Helper.DumpOption.MapLoaded |
                            (isGuiRunning ? Helper.DumpOption.GUIActive : Helper.DumpOption.None) | (Mod.config.IncludeModsInAfterMapDump ? Helper.DumpOption.ExtendedInfo : Helper.DumpOption.None));
                    }
                }

                if (Mod.IsEnabled & (Mod.IsGuiEnabled | isGuiRunning))
                {
                    RemoveGui();
                }
            }
            catch (Exception ex1)
            {
                Helper.dbgLog("Error: \r\n", ex1, true);
            }


        }


        public override void OnReleased()
        {
            base.OnReleased();
            if (Mod.DEBUG_LOG_ON) { Helper.dbgLog ("Releasing Completed."); }
        }

        public static void SetupGui()
        {
            //if(Mod.IsEnabled && Mod.IsGuiEnabled)
            if (Mod.DEBUG_LOG_ON) Helper.dbgLog(" Setting up Gui panel.");
            try
            {
                parentGuiView = null;
                parentGuiView = UIView.GetAView();
                if (guiPanel == null)
                {
                    guiPanel = (CSLShowMoreLimitsGUI)parentGuiView.AddUIComponent(typeof(CSLShowMoreLimitsGUI));
                    if (Mod.DEBUG_LOG_ON) Helper.dbgLog(" GUI Setup.");
                    //guiPanel.Hide();
                }
                isGuiRunning = true;
            }
            catch (Exception ex)
            {
                Helper.dbgLog("Error: \r\n", ex,true);
            }

        }

        public static void RemoveGui()
        {

            if (Mod.DEBUG_LOG_ON) Helper.dbgLog(" Removing Gui.");
            try
            {
                if (guiPanel != null)
                {
                    //is this causing on exit exception problem?
                    //guiPanel.gameObject.SetActive(false);
                    //GameObject.DestroyImmediate(guiPanel.gameObject);
                    //guiPanel = null;
                    if (Mod.DEBUG_LOG_ON) Helper.dbgLog("Destroyed GUI objects.");
                }
            }
            catch (Exception ex)
            {
                Helper.dbgLog("Error: ",ex,true);
            }

            isGuiRunning = false;
            if (parentGuiView != null) { parentGuiView = null; } //toast our ref to guiview
        }

	}
}
