using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Diagnostics;
//using CSLServiceReserve.Configuration;
namespace CSLShowMoreLimits
{
    public class CSLShowMoreLimitsGUI : UIPanel
    {
        public static readonly string cacheName = "CSLShowMoreLimitsGUI";
        public static CSLShowMoreLimitsGUI instance;
        private const string DTMilli = "MM/dd/yyyy hh:mm:ss.fff tt";
        private const string sVALUE_PLACEHOLDER = "[00000]  |  [00000]";
        private const string TAG_VALUE_PREFIX = "CSLShowMoreLimits_Value_";
        private const string TAG_TEXT_PREFIX = "CSLShowMoreLimits_Text_";
        private const string sVALUE_FSTRING1 = "   {0}    |    [{1}]";
        private const string sVALUE_FSTRING2 = "   {0}";
        private const string sVALUE_FSTRING3 = "   {0}    |    {1}";
        private static readonly float WIDTH = 600f;
        private static readonly float HEIGHT = 500f;
        private static readonly float HEADER = 40f;
        private static readonly float SPACING = 10f;
        private static readonly float SPACING22 = 22f;
        private static bool isRefreshing = false;  //Used basically as a safety lock.
        private bool CoCheckStatsDataEnabled = false;   //These tell us if certain coroutine is running. 
        private bool CoDisplayRefreshEnabled = false;

        private object[] _tmpNetData;
        private object[] _tmpCitzData;
        private object[] _tmpotherdata;
        private object[] _tmpotherdata2;
        private object[] MaxsizesInt;
        private object[] LimitsizesInt;
        private Dictionary<string, UILabel> _txtControlContainer = new Dictionary<string, UILabel>(16);
        private Dictionary<string,UILabel> _valuesControlContainer = new Dictionary<string,UILabel>(16);

        UIDragHandle m_DragHandler; //lets us move the panel around.
        UIButton m_closeButton; //our close button
        UILabel m_title;
        UIButton m_refresh;  //our manual refresh button
        UILabel m_AutoRefreshChkboxText; //label that get assigned to the AutoRefreshCheckbox.
        UICheckBox m_AutoRefreshCheckbox; //Our AutoRefresh checkbox

        UILabel m_HeaderDataText;
        UILabel m_NetSegmentsText;
        UILabel m_NetSegmentsValue;
        UILabel m_NetNodesText;
        UILabel m_NetNodesValue;
        UILabel m_NetLanesText;
        UILabel m_NetLanesValue;
        UILabel m_BuildingsText;
        UILabel m_BuildingsValue;
        UILabel m_ZonedBlocksText;
        UILabel m_ZonedBlocksValue;
        UILabel m_VehiclesText;
        UILabel m_VehiclesValue;
        UILabel m_ParkedCarsText;
        UILabel m_ParkedCarsValue;
        UILabel m_CitizensText;
        UILabel m_CitizensValue;
        UILabel m_CitizenUnitsText;
        UILabel m_CitizenUnitsValue;
        UILabel m_CitizenAgentsText;
        UILabel m_CitizenAgentsValue;
        UILabel m_PathUnitsText;
        UILabel m_PathUnitsValue;
        UILabel m_TransportLinesText;
        UILabel m_TransportLinesValue;
        UILabel m_AreasText;
        UILabel m_AreasValue;
        UILabel m_DistrictsText;
        UILabel m_DistrictsValue;
        UILabel m_TreesText;
        UILabel m_TreesValue;
        UILabel m_UserPropsText;
        UILabel m_UserPropsValue;
        
        
        UILabel m_AdditionalText1Text;
        UIButton m_LogdataButton;
        UIButton m_ClearDataButton;

//        private Stopwatch MyPerfTimer = new Stopwatch();
        private ItemClass.Availability CurrentMode;



        /// <summary>
        /// Function gets called by unity on every single frame.
        /// We just check for our key combo, maybe there is a better way to register this with the game?
        /// </summary>
        public override void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.L))
            {
                this.ProcessVisibility();
            }
            base.Update();
        }


        /// <summary>
        /// Gets called upon the base UI component's creation. Basically it's the constructor...but not really.
        /// </summary>
        public override void Start()
        {
            base.Start();
            CSLShowMoreLimitsGUI.instance = this;
            if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL > 0) Helper.dbgLog(string.Concat("Attempting to create our display panel.  ",DateTime.Now.ToString(DTMilli).ToString()));
            this.size = new Vector2(WIDTH, HEIGHT);
            this.backgroundSprite = "MenuPanel";
            this.canFocus = true;
            this.isInteractive = true;
            this.BringToFront();
            this.relativePosition = new Vector3((Loader.parentGuiView.fixedWidth / 2) - 200, (Loader.parentGuiView.fixedHeight / 2) - 350);
            this.opacity = Mod.config.GuiOpacity;
            this.cachedName = cacheName;
            CurrentMode = Singleton<ToolManager>.instance.m_properties.m_mode;
/*            if (CurrentMode == ItemClass.Availability.MapEditor)
            {
                Helper.dbgLog("sdfsfasdfasdfasdfasdfasdfasdf");
                UITextureAtlas[] tAtlas;
                tAtlas = FindObjectsOfType<UITextureAtlas>();
                if (tAtlas != null)
                {
                    for (int i = 0; i < tAtlas.Length; i++)
                    {
                        if (tAtlas[i].name == "Ingame")
                        {
                            this.atlas = tAtlas[i];
                            Helper.dbgLog("sdfsfa22222222222222222222asdf");
                            break;
                        }
                    }
                }
            }
*/
            //DragHandler
            m_DragHandler = this.AddUIComponent<UIDragHandle>();
            m_DragHandler.target = this;
            //Title UILabel
            m_title = this.AddUIComponent<UILabel>();
            m_title.text = "Counter and Object Limit Data"; //spaces on purpose
            m_title.relativePosition = new Vector3(WIDTH / 2 - (m_title.width / 2) - 25f, (HEADER / 2) - (m_title.height / 2));
            m_title.textAlignment = UIHorizontalAlignment.Center;
            //Close Button UIButton
            m_closeButton = this.AddUIComponent<UIButton>();
            m_closeButton.normalBgSprite = "buttonclose";
            m_closeButton.hoveredBgSprite = "buttonclosehover";
            m_closeButton.pressedBgSprite = "buttonclosepressed";
            m_closeButton.relativePosition = new Vector3(WIDTH - 35, 5, 10);
            m_closeButton.eventClick += (component, eventParam) =>
            {
                this.Hide();
            };

            if (!Mod.config.AutoShowOnMapLoad)
            {
                this.Hide();
            }
            DoOnStartup();
            if (Mod.DEBUG_LOG_ON) Helper.dbgLog(string.Concat("Display panel created. ",DateTime.Now.ToString(DTMilli).ToString()));
        }

        /// <summary>
        /// Our initialize stuff; call after panel\form setup.
        /// </summary>
        private void DoOnStartup()
        {
            FetchMaxSizeLimitSizeData();
            CreateTextLabels();
            FetchValueLabelData();
            CreateDataLabels();
            PopulateControlContainers();

//            RefreshDisplayData(); //fill'em up manually initially.
            if (Mod.config.CheckStatsForLimitsEnabled)
            {
                this.StartCoroutine(CheckForStatsStatus());
                if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("CheckForStatsStatus coroutine started."); }
            }
            if (m_AutoRefreshCheckbox.isChecked)
            {
                this.StartCoroutine(RefreshDisplayDataWrapper());
                if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("RefreshDisplayDataWrapper coroutine started."); }
            }
            else
            {
                RefreshDisplayData(); //at least run once.
            }
        }

        private void FetchMaxSizeLimitSizeData()
        {
            try
            {
                VehicleManager vMgr = Singleton<VehicleManager>.instance;
                NetManager nMgr = Singleton<NetManager>.instance;
                CitizenManager cMgr = Singleton<CitizenManager>.instance;

                MaxsizesInt = new object[] { nMgr.m_segments.m_size, nMgr.m_nodes.m_size, nMgr.m_lanes.m_size,
                Singleton<BuildingManager>.instance.m_buildings.m_size,Singleton<ZoneManager>.instance.m_blocks.m_size,
                vMgr.m_vehicles.m_size,vMgr.m_parkedVehicles.m_size,cMgr.m_citizens.m_size ,cMgr.m_units.m_size,
                cMgr.m_instances.m_size,Singleton<TransportManager>.instance.m_lines.m_size,Singleton<PathManager>.instance.m_pathUnits.m_size,
                Singleton<GameAreaManager>.instance.m_areaGrid.Count(),Singleton<DistrictManager>.instance.m_districts.m_size,
                Singleton<TreeManager>.instance.m_trees.m_size,Singleton<PropManager>.instance.m_props.m_size};

                LimitsizesInt = new object[]{
                CheckMode() ? (int)32256 : (int)16384,
                CheckMode() ? (int)32256 : (int)16384,
                CheckMode() ? (int)258048 : (int)131072, //lanes
                CheckMode() ? (int)32256 : (int)8192, //build
                CheckMode() ? (int)32256 : (int)16384, //zoneblk
                (int)16384, //vehc
                (int)32767, //parked
                (int)1048575, //cit
                (int)524287, //units
                (int)65535,  //agents
                (int)249,
                (int)262143, //pathunits
                (int) Singleton<GameAreaManager>.instance.m_maxAreaCount, //areas
                (int)126,  //districts
                Singleton<TreeManager>.instance.m_trees.m_size > 262144 ? (int)(Singleton<TreeManager>.instance.m_trees.m_size - 10u) : (CheckMode() ? (int)262139 : (int)250000), //trees
                CheckMode() ? (int)65531 : (int)50000
                };
            }
            catch (Exception ex)
            { Helper.dbgLog("FetchMaxSizleData failed. ", ex, true); }
        }

        private void FetchValueLabelData()
        {
            try
            {

                _tmpNetData = Helper.AddLimitData(2);
                _tmpCitzData = Helper.AddLimitData(4);
                _tmpotherdata = Helper.AddLimitData(8);
                _tmpotherdata2 = Helper.AddLimitData(16);
            }
            catch (Exception ex)
            {
                Helper.dbgLog("fetchvalues died. ", ex, true);
            }
        }

        /// <summary>
        /// Returns true if we are in game, false if we are in map editor, assumes loader never loads this form in asset mode.
        /// </summary>
        /// <returns>true|False ; true if game, false if mapeditor</returns>
        private bool CheckMode()
        {
            if(Loader.CurrentLoadMode == LoadMode.LoadMap || Loader.CurrentLoadMode == LoadMode.NewMap)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// Create and setup up default text and stuff for our Text UILabels;
        /// </summary>
        private void CreateTextLabels() 
        {
            m_HeaderDataText = this.AddUIComponent<UILabel>();
            m_HeaderDataText.textScale = 0.825f;
            m_HeaderDataText.text = string.Concat("Object Type     [#maxsize]  |  [#defaultLimit]:         #In Use          [  |  #itemCount]");
            m_HeaderDataText.tooltip = string.Concat("Maxsize is the max size of the holding array, defaultlimit is the number the game or map by default is restricting you too.",
                "\n#InUse: is the number you are currently using.\n",
                "Optionally #itemCount is shown as checksum, it should never be more than one higher than #InUse if shown.");
            m_HeaderDataText.relativePosition = new Vector3(SPACING, 50f);
            //m_HeaderDataText.color  
            //m_NumOfVehText.minimumSize = new Vector2(100, 20);
            //m_NumOfVehText.maximumSize = new Vector2(this.width - SPACING * 2, 30);
            m_HeaderDataText.autoSize = true;

            m_NetSegmentsText = this.AddUIComponent<UILabel>();
            m_NetSegmentsText.text = string.Format("Net Segments     [{0}]]  |  [{1}]:", MaxsizesInt[0].ToString(), LimitsizesInt[0].ToString());
            m_NetSegmentsText.tooltip = "You can think of segments as roads, but they are used for far more then just roads.\n Each segment is typically connected in a chain of other segments to a 'node'.";
            m_NetSegmentsText.relativePosition = new Vector3(SPACING, (m_HeaderDataText.relativePosition.y + SPACING22));
            //m_NumOfVehText.minimumSize = new Vector2(100, 20);
            //m_NumOfVehText.maximumSize = new Vector2(this.width - SPACING * 2, 30);
            m_NetSegmentsText.autoSize = true;
            m_NetSegmentsText.name = TAG_TEXT_PREFIX + "0";


            m_NetNodesText = this.AddUIComponent<UILabel>();
            m_NetNodesText.relativePosition = new Vector3(SPACING, (m_NetSegmentsText.relativePosition.y + SPACING22));
            m_NetNodesText.text = string.Format("Net Nodes     [{0}]  |  [{1}]:", MaxsizesInt[1].ToString(), LimitsizesInt[1].ToString());
            m_NetNodesText.tooltip = "The number of Nodes. Think of nodes sort of like intersections, or the point that the first segment of a path connects too,\n each node typically contains zero or more segments.";
            m_NetNodesText.autoSize = true;
            m_NetNodesText.name = TAG_TEXT_PREFIX + "1";

            m_NetLanesText = this.AddUIComponent<UILabel>();
            m_NetLanesText.relativePosition = new Vector3(SPACING, (m_NetNodesText.relativePosition.y + SPACING22));
            m_NetLanesText.text = string.Format("Net Lanes     [{0}]  |  [{1}]:",MaxsizesInt[2].ToString(),LimitsizesInt[2].ToString()) ;
            m_NetLanesText.tooltip = "The number of lanes. Lanes are used by more than just roads and rail.\n Things like ped.paths, bike paths, transportlines and similar things create and use them too.\nIt's not alway logical most roads with only two lanes by default get assigned six lanes,\n so they can be upgraded later I presume.";
            m_NetLanesText.autoSize = true;
            m_NetLanesText.name = TAG_TEXT_PREFIX + "2";


            m_BuildingsText = this.AddUIComponent<UILabel>();
            m_BuildingsText.relativePosition = new Vector3(SPACING, (m_NetLanesText.relativePosition.y + SPACING22));
            m_BuildingsText.text = String.Format("Buildings    [{0}]  |  [{1}]:", MaxsizesInt[3].ToString(), LimitsizesInt[3].ToString());
            m_BuildingsText.tooltip = "The number of building objects created.\nSome things you might not expect count as buildings.";
            m_BuildingsText.autoSize = true;
            m_BuildingsText.name = TAG_TEXT_PREFIX + "3";

            m_ZonedBlocksText = this.AddUIComponent<UILabel>();
            m_ZonedBlocksText.relativePosition = new Vector3(SPACING, (m_BuildingsText.relativePosition.y + SPACING22));
            m_ZonedBlocksText.text = String.Format("Zoned Blocks     [{0}]  |  [{1}]:", MaxsizesInt[4].ToString() , LimitsizesInt[4].ToString());
            m_ZonedBlocksText.tooltip = "The number of Zoned blocks (squares) you have in your map.";
            m_ZonedBlocksText.autoSize = true;
            m_ZonedBlocksText.name = TAG_TEXT_PREFIX + "4";


            m_VehiclesText = this.AddUIComponent<UILabel>();
            m_VehiclesText.relativePosition = new Vector3(SPACING, (m_ZonedBlocksText.relativePosition.y + SPACING22));
            m_VehiclesText.text = String.Format("Vehicles Active     [{0}]  |  [{1}]:", MaxsizesInt[5].ToString(), LimitsizesInt[5].ToString());
            m_VehiclesText.tooltip = "The number of vehicles actively in use during the last update.\n Being at the max is technically ok, if if you permenantly though I suggest CSL Service Reserve mod.\n Also look for glitched(or internally backed up) cargo stations.";
            m_VehiclesText.autoSize = true;
            m_VehiclesText.name = TAG_TEXT_PREFIX + "5";

            m_ParkedCarsText  = this.AddUIComponent<UILabel>();
            m_ParkedCarsText.relativePosition = new Vector3(SPACING, (m_VehiclesText.relativePosition.y + SPACING22));
            m_ParkedCarsText.text = String.Format("Parked Cars     [{0}]  |  [{1}]:", MaxsizesInt[6].ToString(), LimitsizesInt[6].ToString());
            m_ParkedCarsText.tooltip = "The number of cars that are currently parked.";
            m_ParkedCarsText.autoSize = true;
            m_ParkedCarsText.name = TAG_TEXT_PREFIX + "6";

            m_CitizensText = this.AddUIComponent<UILabel>();
            m_CitizensText.relativePosition = new Vector3(SPACING, (m_ParkedCarsText.relativePosition.y + SPACING22));
            m_CitizensText.text = String.Format("Citizens     [{0}]  |  [{1}]:", MaxsizesInt[7].ToString(), LimitsizesInt[7].ToString()); ;
            m_CitizensText.tooltip = "The number of citizens the game is currently simulating.";
            m_CitizensText.autoSize = true;
            m_CitizensText.name = TAG_TEXT_PREFIX + "7";

            m_CitizenUnitsText = this.AddUIComponent<UILabel>();
            m_CitizenUnitsText.relativePosition = new Vector3(SPACING, (m_CitizensText.relativePosition.y + SPACING22));
            m_CitizenUnitsText.text = String.Format("Citizen Units     [{0}]  |  [{1}]:", MaxsizesInt[8].ToString(), LimitsizesInt[8].ToString());
            m_CitizenUnitsText.tooltip = "The number of citizen units in use, these are used by ai's to hold a group of citizens\nThey can represent a home, passengers, a work site, students,etc.\nFor example when a cop car gets created it will create one of these to hold the cop and any criminals caught.";
            m_CitizenUnitsText.autoSize = true;
            m_CitizenUnitsText.name = TAG_TEXT_PREFIX + "8";

            m_CitizenAgentsText  = this.AddUIComponent<UILabel>();
            m_CitizenAgentsText.relativePosition = new Vector3(SPACING, (m_CitizenUnitsText.relativePosition.y + SPACING22));
            m_CitizenAgentsText.text = String.Format("Citizen Instances     [{0}]  |  [{1}]:", MaxsizesInt[9].ToString(), LimitsizesInt[9].ToString());
            m_CitizenAgentsText.tooltip = "The number of cims during the last pass that were 'actively' being simulated,\nie walking, biking, chilling in the park, not at home or at work, etc";
            m_CitizenAgentsText.autoSize = true;
            m_CitizenAgentsText.name = TAG_TEXT_PREFIX + "9";

            m_TransportLinesText = this.AddUIComponent<UILabel>();
            m_TransportLinesText.relativePosition = new Vector3(SPACING, (m_CitizenAgentsText.relativePosition.y + SPACING22));
            m_TransportLinesText.text = String.Format("Transport Lines     [{0}]  |  [{1}]:", MaxsizesInt[10].ToString(), LimitsizesInt[10].ToString());
            m_TransportLinesText.tooltip = "The number of transport lines. \n";
            m_TransportLinesText.autoSize = true;
            m_TransportLinesText.name = TAG_TEXT_PREFIX + "10";

            m_PathUnitsText = this.AddUIComponent<UILabel>();
            m_PathUnitsText.relativePosition = new Vector3(SPACING, (m_TransportLinesText.relativePosition.y + SPACING22));
            m_PathUnitsText.text = String.Format("Path Units     [{0}]  |  [{1}]:", MaxsizesInt[11].ToString(), LimitsizesInt[11].ToString());
            m_PathUnitsText.tooltip = "Number of paths in use by the pathfinder.";
            m_PathUnitsText.autoSize = true;
            m_PathUnitsText.name = TAG_TEXT_PREFIX + "11";

            m_AreasText = this.AddUIComponent<UILabel>();
            m_AreasText.relativePosition = new Vector3(SPACING, (m_PathUnitsText.relativePosition.y + SPACING22));
            m_AreasText.text = String.Format("Areas     [{0}]  |  [{1}]:", MaxsizesInt[12].ToString(), LimitsizesInt[12].ToString());
            m_AreasText.tooltip = "The number of area grids you are allowed to buy\\purchase.";
            m_AreasText.autoSize = true;
            m_AreasText.name = TAG_TEXT_PREFIX + "12";

            m_DistrictsText = this.AddUIComponent<UILabel>();
            m_DistrictsText.relativePosition = new Vector3(SPACING, (m_AreasText.relativePosition.y + SPACING22));
            m_DistrictsText.text = String.Format("Districts     [{0}]  |  [{1}]:", MaxsizesInt[13].ToString(), LimitsizesInt[13].ToString()); 
            m_DistrictsText.tooltip = "The number of districts.";
            m_DistrictsText.autoSize = true;
            m_DistrictsText.name = TAG_TEXT_PREFIX + "13";

            m_TreesText = this.AddUIComponent<UILabel>();
            m_TreesText.relativePosition = new Vector3(SPACING, (m_DistrictsText.relativePosition.y + SPACING22));
            m_TreesText.text = String.Format("Trees     [{0}]  |  [{1}]:", MaxsizesInt[14].ToString(), LimitsizesInt[14].ToString());
            m_TreesText.tooltip = Singleton<TreeManager>.instance.m_trees.m_size > 262144 ? "The number of placed trees.\nUnlimited Trees mod detected!" : "The number of placed trees.\n Remember just cause you plow over a tree with a road does not mean it's gone.\n They must actually be bulldozed.";
            m_TreesText.autoSize = true;
            m_TreesText.name = TAG_TEXT_PREFIX + "14";

            m_UserPropsText = this.AddUIComponent<UILabel>();
            m_UserPropsText.relativePosition = new Vector3(SPACING, (m_TreesText.relativePosition.y + SPACING22));
            m_UserPropsText.text = String.Format("User Props     [{0}]  |  [{1}]:", MaxsizesInt[15].ToString(), LimitsizesInt[15].ToString());
            m_UserPropsText.tooltip = "The number of props placed on the map. \n This, far as I can tell is user placed prop limit, not counting those embedded with prefabs.";
            m_UserPropsText.autoSize = true;
            m_UserPropsText.name = TAG_TEXT_PREFIX + "15";

            m_AutoRefreshCheckbox = this.AddUIComponent<UICheckBox>();
            m_AutoRefreshCheckbox.relativePosition = new Vector3((SPACING), (m_UserPropsText.relativePosition.y + 30f));

            m_AutoRefreshChkboxText = this.AddUIComponent<UILabel>();
            m_AutoRefreshChkboxText.relativePosition = new Vector3(m_AutoRefreshCheckbox.relativePosition.x + m_AutoRefreshCheckbox.width + (SPACING * 3), (m_AutoRefreshCheckbox.relativePosition.y) + 5f);
            //m_AutoRefreshChkboxText.text = "Use Auto Refresh";
            m_AutoRefreshChkboxText.tooltip = "Enables these stats to update every few seconds \n Default is 5 seconds.";
            //m_AutoRefreshChkboxText.autoSize = true;

            m_AutoRefreshCheckbox.height = 16;
            m_AutoRefreshCheckbox.width = 16;
            m_AutoRefreshCheckbox.label = m_AutoRefreshChkboxText;
            m_AutoRefreshCheckbox.text = string.Concat("Use AutoRefresh  (", Mod.AutoRefreshSeconds.ToString("f1"), " sec)");

            UISprite uncheckSprite = m_AutoRefreshCheckbox.AddUIComponent<UISprite>();
            uncheckSprite.height = 20;
            uncheckSprite.width = 20;
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            UISprite checkSprite = m_AutoRefreshCheckbox.AddUIComponent<UISprite>();
            checkSprite.height = 20;
            checkSprite.width = 20;
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            m_AutoRefreshCheckbox.checkedBoxObject = checkSprite;
            m_AutoRefreshCheckbox.isChecked = Mod.UseAutoRefreshOption;
            m_AutoRefreshCheckbox.isEnabled = true;
            m_AutoRefreshCheckbox.isVisible = true;
            m_AutoRefreshCheckbox.canFocus = true;
            m_AutoRefreshCheckbox.isInteractive = true;
            //can't use this? m_AutoRefreshCheckbox.autoSize = true;  
            m_AutoRefreshCheckbox.eventCheckChanged += (component, eventParam) => { AutoRefreshCheckbox_OnCheckChanged(component, eventParam); };
            //AutoRefreshCheckbox_OnCheckChanged;

            m_AdditionalText1Text = this.AddUIComponent<UILabel>();
            m_AdditionalText1Text.relativePosition = new Vector3(m_AutoRefreshCheckbox.relativePosition.x + m_AutoRefreshCheckbox.width + SPACING, (m_AutoRefreshCheckbox.relativePosition.y) + 25f);
            m_AdditionalText1Text.width = 300f;
            m_AdditionalText1Text.height = 50f;
            m_AdditionalText1Text.textScale = 0.875f;
            //m_AdditionalText1Text.autoSize = true;
            //m_AdditionalText1Text.wordWrap = true;
            m_AdditionalText1Text.text = "* Use CTRL + L to show again. \n  More options available in CSLShowMoreLimits_Config.xml";

            m_refresh = this.AddUIComponent<UIButton>();
            m_refresh.size = new Vector2(120, 24);
            m_refresh.text = "Manual Refresh";
            m_refresh.tooltip = "Use to manually refresh the data. \n (use when auto enabled is off)";
            m_refresh.textScale = 0.875f;
            m_refresh.normalBgSprite = "ButtonMenu";
            m_refresh.hoveredBgSprite = "ButtonMenuHovered";
            m_refresh.pressedBgSprite = "ButtonMenuPressed";
            m_refresh.disabledBgSprite = "ButtonMenuDisabled";
            //m_refresh.relativePosition = m_closeButton.relativePosition + new Vector3(-60 - SPACING, 6f);
            m_refresh.relativePosition = m_AutoRefreshChkboxText.relativePosition + new Vector3((m_AutoRefreshChkboxText.width + SPACING * 2), -5f);
            m_refresh.eventClick += (component, eventParam) =>
            {
                FetchValueLabelData();
                RefreshDisplayData();
                CheckStatsForColorChange();
            };

            m_LogdataButton = this.AddUIComponent<UIButton>();
            m_LogdataButton.size = new Vector2(80, 24);
            m_LogdataButton.text = "Log Data";
            m_LogdataButton.tooltip = "Use to Log the current data to log file. \n (Saved to CSL standard output_log.txt log file)";
            m_LogdataButton.textScale = 0.875f;
            m_LogdataButton.normalBgSprite = "ButtonMenu";
            m_LogdataButton.hoveredBgSprite = "ButtonMenuHovered";
            m_LogdataButton.pressedBgSprite = "ButtonMenuPressed";
            m_LogdataButton.disabledBgSprite = "ButtonMenuDisabled";
            m_LogdataButton.relativePosition = m_refresh.relativePosition + new Vector3((m_refresh.width + SPACING * 3), 0f);
            m_LogdataButton.eventClick += (component, eventParam) => { ProcessOnLogButton(); };

            m_ClearDataButton = this.AddUIComponent<UIButton>();
            m_ClearDataButton.size = new Vector2(50, 24);
            m_ClearDataButton.text = "Copy";
            m_ClearDataButton.tooltip = "Use to manually clear and reset the above data values. \n Usefull to watch for changes over specific periods of time \n This is in addition to the ResetLogEveryFewMin option.";
            m_ClearDataButton.textScale = 0.875f;
            m_ClearDataButton.normalBgSprite = "ButtonMenu";
            m_ClearDataButton.hoveredBgSprite = "ButtonMenuHovered";
            m_ClearDataButton.pressedBgSprite = "ButtonMenuPressed";
            m_ClearDataButton.disabledBgSprite = "ButtonMenuDisabled";
            m_ClearDataButton.relativePosition = m_LogdataButton.relativePosition + new Vector3((m_LogdataButton.width + SPACING * 3), 0f);
            m_ClearDataButton.eventClick += (component, eventParam) => { ProcessOnCopyButton(); };
        }


        /// <summary>
        /// Event handler for clicking on AutoRefreshbutton.
        /// </summary>
        /// <param name="UIComp">The triggering UIComponent</param>
        /// <param name="bValue">The Value True|False (Checked|Unchecked)</param>

        private void AutoRefreshCheckbox_OnCheckChanged(UIComponent UIComp, bool bValue)
        {
            if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("AutoRefreshButton was toggled to: " + bValue.ToString()); }
            Mod.UpdateUseAutoRefeshValue(bValue);
            if (bValue == true)
            {
                byte bflag = 0;
                //if (!CoVehcRefreshEnabled) { this.StartCoroutine(RefreshVehcCount()); bflag += 1; }
                if (!CoDisplayRefreshEnabled) { this.StartCoroutine(RefreshDisplayDataWrapper()); bflag += 2; }
                if (!CoCheckStatsDataEnabled) { this.StartCoroutine(CheckForStatsStatus()); bflag += 4; }
                if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("Starting all coroutines that were not already started " + 
                    bValue.ToString() + " bflag=" + bflag.ToString()); }
            }
            else
            {
                //upon disabling auto refresh we *also* disable the the stat data refresher
                //I think this is logical, as people might want to start at data, plus can always manually refresh.
                this.StopAllCoroutines();
                ResetAllCoroutineState(false); //cleanup
                if (Mod.DEBUG_LOG_ON) { Helper.dbgLog("Stopping all coroutines: " + bValue.ToString()); }
            }
            return;
        }

        /// <summary>
        /// Sadly needed to reset state of Coroutines after forced stop.
        /// </summary>
        /// <param name="bStatus">True|False</param>
        private void ResetAllCoroutineState(bool bStatus)
        {
//            CoVehcRefreshEnabled = bStatus;
            CoCheckStatsDataEnabled = bStatus;
            CoDisplayRefreshEnabled = bStatus;
        }

        /// <summary>
        /// Function to check if we need to reset the stats, ment to check only every so often..like once a minute
        /// or modify it 
        /// </summary>
        private IEnumerator CheckForStatsStatus()
        {
            if (CoCheckStatsDataEnabled == true)
            {
                if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL > 0) Helper.dbgLog(" CheckkForStatsStatus* coroutine exited; Only one allowed at a time.");
                yield break;
            } //ensure only 1 copy at a time.
            while (Mod.config.CheckStatsForLimitsEnabled & this.isVisible )
            {
                CoCheckStatsDataEnabled = true;
                CheckStatsForColorChange();
                if (Mod.DEBUG_LOG_ON) Helper.dbgLog(string.Concat("CheckStats fired. will fire again in 60 seconds.", DateTime.Now.ToString(DTMilli)));
                // We hard code this to only check once a minute, keeps things simple.
                yield return new WaitForSeconds(Mod.config.StatsCheckEverySeconds);
            }
            CoCheckStatsDataEnabled = false;
            if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL > 0) Helper.dbgLog("CheckkForStatsStatus coroutine exited due to CheckStatsForLimitsEnabled = false or visibility change.");
            yield break;
        }




        /// <summary>
        /// Primary coroutine function to update the more static (seconds) information display.
        /// as there really is no need to update this more then once per second.
        /// </summary>
        private IEnumerator RefreshDisplayDataWrapper() 
        {
            if (CoDisplayRefreshEnabled == true)
            {
                if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL > 0) Helper.dbgLog("Refresh vehicleData* coroutine exited; Only one allowed at a time.");
                yield break;
            } //ensure only 1 active. 
            while (isRefreshing == false && this.isVisible == true && m_AutoRefreshCheckbox.isChecked)
            {
//                MyPerfTimer.Reset();
//                MyPerfTimer.Start();

                CoDisplayRefreshEnabled  = true;
                FetchValueLabelData();
                RefreshDisplayData();
//                MyPerfTimer.Stop();
//                Helper.dbgLog(string.Concat("Refresh took this many ticks:", MyPerfTimer.ElapsedTicks.ToString(), ":",MyPerfTimer.ElapsedMilliseconds.ToString()));
                yield return new WaitForSeconds(Mod.config.AutoRefreshSeconds);
            }
            CoDisplayRefreshEnabled = false;
            if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL > 0) Helper.dbgLog("Refresh vehicleData coroutine exited due to AutoRefresh disabled, visiblity change, or already refreshing.");
            yield break;
        }


        /// <summary>
        /// Function refreshes the display data. mostly called from coroutine timer.
        /// </summary>
        private void RefreshDisplayData()
        {
            isRefreshing = true; //safety lock so we never get more then one of these, probably don't need after co-routine refactor.
            try
            {
                
                m_NetSegmentsValue.text = string.Format(sVALUE_FSTRING1, _tmpNetData[0], _tmpNetData[1]);
                m_NetNodesValue.text = string.Format(sVALUE_FSTRING1, _tmpNetData[2], _tmpNetData[3]);
                m_NetLanesValue.text = string.Format(sVALUE_FSTRING1, _tmpNetData[4], _tmpNetData[5]);
                m_BuildingsValue.text = string.Format(sVALUE_FSTRING1, _tmpotherdata[0],_tmpotherdata[1]);
                m_ZonedBlocksValue.text = string.Format(sVALUE_FSTRING1, _tmpotherdata[2], _tmpotherdata[3]);
                m_VehiclesValue.text = string.Format(sVALUE_FSTRING1, _tmpotherdata2[1], _tmpotherdata2[2]);
                m_ParkedCarsValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata2[0]);
                m_CitizensValue.text = string.Format(sVALUE_FSTRING2, _tmpCitzData[0]);
                m_CitizenUnitsValue.text = string.Format(sVALUE_FSTRING2, _tmpCitzData[1]);
                m_CitizenAgentsValue.text = string.Format(sVALUE_FSTRING2, _tmpCitzData[2]);
                m_TransportLinesValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[4]);
                m_PathUnitsValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[6]);
                m_AreasValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[8]);
                m_DistrictsValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[9]);
                m_TreesValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[10]);
                m_UserPropsValue.text = string.Format(sVALUE_FSTRING2, _tmpotherdata[5]);

                if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL >= 3) Helper.dbgLog("Refreshing display data completed. " + DateTime.Now.ToString(DTMilli));
            }
            catch (Exception ex)
            {
                isRefreshing = false;
                Helper.dbgLog("ERROR during RefreshDisplayData. ",ex,true);
            }
            isRefreshing = false;

        }


        /// <summary>
        /// Checks stats against 10% of their limits, turns them orange.
        /// </summary>
        private void CheckStatsForColorChange()
        {
            try
            {
                Color32 cGreen = new Color32(0, 204, 0, 255);
                Color32 cOrange = new Color32(255, 204, 0, 255);
                m_HeaderDataText.color = cGreen;
                m_NetSegmentsValue.textColor  = isWithin10Percent(int.Parse(_tmpNetData[0].ToString()), (int)LimitsizesInt[0], 0.1f) ? cOrange : cGreen;
                m_NetNodesValue.textColor = isWithin10Percent(int.Parse(_tmpNetData[2].ToString()), (int)LimitsizesInt[1]) ? cOrange : cGreen;
                m_NetLanesValue.textColor = isWithin10Percent(int.Parse(_tmpNetData[4].ToString()), (int)LimitsizesInt[2]) ? cOrange : cGreen;
                m_BuildingsValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[0].ToString()), (int)LimitsizesInt[3]) ? cOrange : cGreen;
                m_ZonedBlocksValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[2].ToString()), (int)LimitsizesInt[4]) ? cOrange : cGreen;
                m_VehiclesValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata2[1].ToString()), (int)LimitsizesInt[5]) ? cOrange : cGreen;
                m_ParkedCarsValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata2[0].ToString()), (int)LimitsizesInt[6]) ? cOrange : cGreen;
                m_CitizensValue.textColor = isWithin10Percent(int.Parse(_tmpCitzData[0].ToString()), (int)LimitsizesInt[7]) ? cOrange : cGreen;
                m_CitizenUnitsValue.textColor = isWithin10Percent(int.Parse(_tmpCitzData[1].ToString()), (int)LimitsizesInt[8]) ? cOrange : cGreen;
                m_CitizenAgentsValue.textColor = isWithin10Percent(int.Parse(_tmpCitzData[2].ToString()), (int)LimitsizesInt[9]) ? cOrange : cGreen;
                m_TransportLinesValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[4].ToString()), (int)LimitsizesInt[10]) ? cOrange : cGreen;
                m_PathUnitsValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[6].ToString()), (int)LimitsizesInt[11]) ? cOrange : cGreen;
                m_AreasValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[8].ToString()), (int)LimitsizesInt[12]) ? cOrange : cGreen;
                m_DistrictsValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[9].ToString()), (int)LimitsizesInt[13]) ? cOrange : cGreen;
                m_TreesValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[10].ToString()), (int)LimitsizesInt[14]) ? cOrange : cGreen;
                m_UserPropsValue.textColor = isWithin10Percent(int.Parse(_tmpotherdata[5].ToString()), (int)LimitsizesInt[15]) ? cOrange : cGreen;
                if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL >= 2)
                {
                    Helper.dbgLog("Completed CheckStatsForColorChange. " + DateTime.Now.ToString(DTMilli));
                }
            }
            catch (Exception ex)
            { Helper.dbgLog("Error :", ex, true); }


        }

        private bool isWithin10Percent(int ival,int imaxlimit,float fPercent = 0.1f)
        {
            if (ival > (imaxlimit * (1.0f - fPercent)))
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Creates all our UILabels that store data that changes\gets refreshed.
        /// </summary>
        private void CreateDataLabels() 
        {
//            VehicleManager vMgr = Singleton<VehicleManager>.instance;
//            NetManager nMgr = Singleton<NetManager>.instance;
            m_NetSegmentsValue = this.AddUIComponent<UILabel>();
            m_NetSegmentsValue.text = sVALUE_PLACEHOLDER;
            m_NetSegmentsValue.relativePosition = new Vector3(m_NetSegmentsText.relativePosition.x + m_NetSegmentsText.width + (SPACING * 5), m_NetSegmentsText.relativePosition.y);
            m_NetSegmentsValue.autoSize = true;
            m_NetSegmentsValue.tooltip = "";
            m_NetSegmentsValue.name = TAG_VALUE_PREFIX + "0";

            m_NetNodesValue = this.AddUIComponent<UILabel>();
            m_NetNodesValue.relativePosition = new Vector3(m_NetSegmentsValue.relativePosition.x , m_NetNodesText.relativePosition.y);
            m_NetNodesValue.autoSize = true;
            m_NetNodesValue.text = sVALUE_PLACEHOLDER;
            m_NetNodesValue.name = TAG_VALUE_PREFIX + "1";

            m_NetLanesValue = this.AddUIComponent<UILabel>();
            m_NetLanesValue.relativePosition = new Vector3(m_NetNodesValue.relativePosition.x , m_NetLanesText.relativePosition.y);
            m_NetLanesValue.autoSize = true;
            m_NetLanesValue.text = sVALUE_PLACEHOLDER;
            m_NetLanesValue.name = TAG_VALUE_PREFIX + "2";

            m_BuildingsValue = this.AddUIComponent<UILabel>();
            m_BuildingsValue.relativePosition = new Vector3(m_NetLanesValue.relativePosition.x, m_BuildingsText.relativePosition.y);
            m_BuildingsValue.autoSize = true;
            m_BuildingsValue.text = sVALUE_PLACEHOLDER;
            m_BuildingsValue.name = TAG_VALUE_PREFIX + "3";

            m_ZonedBlocksValue = this.AddUIComponent<UILabel>();
            m_ZonedBlocksValue.relativePosition = new Vector3(m_BuildingsValue.relativePosition.x , m_ZonedBlocksText.relativePosition.y);
            m_ZonedBlocksValue.autoSize = true;
            m_ZonedBlocksValue.text = sVALUE_PLACEHOLDER;
            m_ZonedBlocksValue.name = TAG_VALUE_PREFIX + "4";

            m_VehiclesValue = this.AddUIComponent<UILabel>();
            m_VehiclesValue.relativePosition = new Vector3(m_ZonedBlocksValue.relativePosition.x, m_VehiclesText.relativePosition.y);
            m_VehiclesValue.autoSize = true;
            m_VehiclesValue.text = sVALUE_PLACEHOLDER;
            m_VehiclesValue.name = TAG_VALUE_PREFIX + "5";

            m_ParkedCarsValue = this.AddUIComponent<UILabel>();
            m_ParkedCarsValue.relativePosition = new Vector3(m_VehiclesValue.relativePosition.x, m_ParkedCarsText.relativePosition.y);
            m_ParkedCarsValue.autoSize = true;
            m_ParkedCarsValue.text = sVALUE_PLACEHOLDER;
            m_ParkedCarsValue.name = TAG_VALUE_PREFIX + "6";

            m_CitizensValue  = this.AddUIComponent<UILabel>();
            m_CitizensValue.relativePosition = new Vector3(m_ParkedCarsValue.relativePosition.x, m_CitizensText.relativePosition.y);
            m_CitizensValue.autoSize = true;
            m_CitizensValue.text = sVALUE_PLACEHOLDER;
            m_CitizensValue.name = TAG_VALUE_PREFIX + "7"; 

            m_CitizenUnitsValue  = this.AddUIComponent<UILabel>();
            m_CitizenUnitsValue.relativePosition = new Vector3(m_CitizensValue.relativePosition.x, m_CitizenUnitsText.relativePosition.y);
            m_CitizenUnitsValue.autoSize = true;
            m_CitizenUnitsValue.text = sVALUE_PLACEHOLDER;
            m_CitizenUnitsValue.name = TAG_VALUE_PREFIX + "8";

            m_CitizenAgentsValue  = this.AddUIComponent<UILabel>();
            m_CitizenAgentsValue.relativePosition = new Vector3(m_CitizenUnitsValue.relativePosition.x, m_CitizenAgentsText.relativePosition.y);
            m_CitizenAgentsValue.autoSize = true;
            m_CitizenAgentsValue.text = sVALUE_PLACEHOLDER;
            m_CitizenAgentsValue.name = TAG_VALUE_PREFIX + "9";

            m_TransportLinesValue = this.AddUIComponent<UILabel>();
            m_TransportLinesValue.relativePosition = new Vector3(m_CitizenAgentsValue.relativePosition.x, m_TransportLinesText.relativePosition.y);
            m_TransportLinesValue.autoSize = true;
            m_TransportLinesValue.text = sVALUE_PLACEHOLDER;
            m_TransportLinesValue.name = TAG_VALUE_PREFIX + "10";

            m_PathUnitsValue  = this.AddUIComponent<UILabel>();
            m_PathUnitsValue.relativePosition = new Vector3(m_TransportLinesValue.relativePosition.x, m_PathUnitsText.relativePosition.y);
            m_PathUnitsValue.autoSize = true;
            m_PathUnitsValue.text = sVALUE_PLACEHOLDER;
            m_PathUnitsValue.name = TAG_VALUE_PREFIX + "11";

            m_AreasValue  = this.AddUIComponent<UILabel>();
            m_AreasValue.relativePosition = new Vector3(m_PathUnitsValue.relativePosition.x, m_AreasText.relativePosition.y);
            m_AreasValue.autoSize = true;
            m_AreasValue.text = sVALUE_PLACEHOLDER;
            m_AreasValue.name = TAG_VALUE_PREFIX + "12";

            m_DistrictsValue  = this.AddUIComponent<UILabel>();
            m_DistrictsValue.relativePosition = new Vector3(m_AreasValue.relativePosition.x, m_DistrictsText.relativePosition.y);
            m_DistrictsValue.autoSize = true;
            m_DistrictsValue.text = sVALUE_PLACEHOLDER;
            m_DistrictsValue.name = TAG_VALUE_PREFIX + "13";

            m_TreesValue  = this.AddUIComponent<UILabel>();
            m_TreesValue.relativePosition = new Vector3(m_DistrictsValue.relativePosition.x, m_TreesText.relativePosition.y);
            m_TreesValue.autoSize = true;
            m_TreesValue.text = sVALUE_PLACEHOLDER;
            m_TreesValue.name = TAG_VALUE_PREFIX + "14";

            m_UserPropsValue   = this.AddUIComponent<UILabel>();
            m_UserPropsValue.relativePosition = new Vector3(m_TreesValue.relativePosition.x, m_UserPropsText.relativePosition.y);
            m_UserPropsValue.autoSize = true;
            m_UserPropsValue.text = sVALUE_PLACEHOLDER;
            m_UserPropsValue.name = TAG_VALUE_PREFIX + "15";
        }

        /// <summary>
        /// Handle action for Hide\Show events.
        /// </summary>
        private void ProcessVisibility()
        {
            if (!this.isVisible)
            {
                this.Show();
                if (!CoCheckStatsDataEnabled ) { this.StartCoroutine(CheckForStatsStatus()); }
                if (!CoDisplayRefreshEnabled) { this.StartCoroutine(RefreshDisplayDataWrapper()); }
                //we do not touch the Resetting of StatsData; that's left to autorefresh on\off only atm.
            }
            else
            {
                this.Hide();
                //we don't have to stop the two above coroutines, 
                //should do that themselves via their own visibility checks.
            }
        
        }

        /// <summary>
        /// Handles the Copy button press event by building the text of the last stats snapshot.
        /// and copying it to the clip board.
        /// </summary>
        private void ProcessOnCopyButton()
        {
            try
            {
                //refresh data
                //build the string
                List<UILabel> tmpList = new List<UILabel>();
                if (_txtControlContainer.Count == _valuesControlContainer.Count)
                {
                    UILabel tmpUIL; string[] tmpspliter;
                    System.Text.StringBuilder sb1 = new System.Text.StringBuilder(2048);
                    sb1.AppendLine(m_HeaderDataText.text);
                    foreach (KeyValuePair<string, UILabel> kvp in _txtControlContainer) 
                    {
                        tmpspliter = kvp.Key.Split('_');
                        int tmpnum;
                        if (int.TryParse(tmpspliter[2].ToString(),out tmpnum))
                        {
                            if(_valuesControlContainer.TryGetValue((TAG_VALUE_PREFIX + tmpnum.ToString()),out tmpUIL))
                            {
                                sb1.AppendLine(string.Concat(kvp.Value.text,tmpUIL.text));
                            }
                        }
                    }
                    ClipboardHelper.clipBoard = sb1.ToString();
                }
                else
                {
                    Helper.dbgLog("Missmatched control container counts.");
                }

            }
            catch (Exception ex)
            {
                Helper.dbgLog("Copy to clipboard error.", ex, true);
            }
        }


        /// <summary>
        /// Handles action for pressing Log Data Button
        /// </summary>
        private void ProcessOnLogButton()
        {
            if (Mod.config.UseCustomDumpFile)
            { Helper.LogExtentedWrapper((Helper.DumpOption.All) ); }
            else
            {
                Helper.LogExtentedWrapper(Helper.DumpOption.All ^ Helper.DumpOption.UseSeperateFile);
            }
        }


        private void PopulateControlContainers()
        {
            foreach (UILabel ul in this.GetComponentsInChildren<UILabel>(true))
            {
                if (ul.name.Contains(TAG_TEXT_PREFIX))
                {
                    _txtControlContainer.Add(ul.name, ul);
                    continue;
                }
                if (ul.name.Contains(TAG_VALUE_PREFIX))
                {
                    _valuesControlContainer.Add(ul.name, ul);
                }
            }
            if (Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL >= 2)
            { 
                Helper.dbgLog(String.Concat("Populated UI controls into containers. ", _txtControlContainer.Count.ToString(),"|",
                _valuesControlContainer.Count.ToString())); 
            }
        }

        /// <summary>
        /// Returns current private values from gui to caller.
        /// </summary>
        /// <returns></returns>
        public static  Helper.ExternalData GetInternalData()
        {
                Helper.ExternalData tmpobj = new Helper.ExternalData();
                tmpobj.CoDataRefreshEnabled = instance.CoCheckStatsDataEnabled;
                tmpobj.CoDisplayRefreshEnabled = instance.CoDisplayRefreshEnabled;
                tmpobj.cachedname = instance.cachedName.ToString();
                tmpobj.tag = instance.tag.ToString();
                tmpobj.name = instance.name.ToString();
                bool.TryParse(instance.m_IsVisible.ToString(), out tmpobj.isVisable);
                bool.TryParse(instance.m_AutoRefreshChkboxText.isEnabled.ToString(), out tmpobj.isAutoRefreshActive);
                if(Mod.DEBUG_LOG_ON & Mod.DEBUG_LOG_LEVEL >= 3) Helper.dbgLog("GetInternalData created and returning.");
                return tmpobj;
        }
        

    }
}
