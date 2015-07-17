using CSGOTriggerbot.CSGO.Enums;
using CSGOTriggerbot.CSGOClasses;
using CSGOTriggerbot.Properties;
using CSGOTriggerbot.UI;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;
using ExternalUtilsCSharp.SharpDXRenderer;
using ExternalUtilsCSharp.SharpDXRenderer.Controls;
using ExternalUtilsCSharp.SharpDXRenderer.Controls.Crosshairs;
using ExternalUtilsCSharp.SharpDXRenderer.Controls.Layouts;
using ExternalUtilsCSharp.UI.UIObjects;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSGOTriggerbot
{
    public class WithOverlay
    {
        #region CONSTANTS
        private const string GAME_PROCESS = "csgo";
        private const string GAME_TITLE = "Counter-Strike: Global Offensive";
        #endregion

        #region VARIABLES
        public static KeyUtils KeyUtils;
        private static IntPtr hWnd;
        private static double seconds = 0;
        public static Framework Framework;
        public static ProcUtils ProcUtils;
        public static MemUtils MemUtils;
        public static CSGOConfigUtils ConfigUtils;
        private static string scrollText = "~~~ [CSGO] Zat's Multihack v3 ~~~ UC-exclusive ~~~ www.unknowncheats.me - Leading the game hacking scene since 2000 ";
        private static int scrollIndex = 0;
        private static int scrollLength = 32;
        #endregion

        #region CONTROLS
        public static SharpDXOverlay SHDXOverlay;

        private static SharpDXCursor cursor;
        //Menu-window
        private static SharpDXWindow windowMenu;
        private static SharpDXTabControl tabsMenu;

        private static SharpDXLabel labelHotkeys;
        private static SharpDXPanel panelESPContent;
        private static SharpDXCheckBox checkBoxESPEnabled;
        private static SharpDXCheckBox checkBoxESPBox;
        private static SharpDXCheckBox checkBoxESPSkeleton;
        private static SharpDXCheckBox checkBoxESPName;
        private static SharpDXCheckBox checkBoxESPHealth;
        private static SharpDXCheckBox checkBoxESPAllies;
        private static SharpDXCheckBox checkBoxESPEnemies;

        private static SharpDXPanel panelAimContent;
        private static SharpDXCheckBox checkBoxAimEnabled;
        private static SharpDXCheckBox checkBoxAimDrawFov;
        private static SharpDXCheckBox checkBoxAimFilterSpotted;
        private static SharpDXCheckBox checkBoxAimFilterSpottedBy;
        private static SharpDXCheckBox checkBoxAimFilterEnemies;
        private static SharpDXCheckBox checkBoxAimFilterAllies;
        private static SharpDXRadioButton radioAimToggle;
        private static SharpDXRadioButton radioAimHold;
        private static SharpDXTrackbar trackBarAimFov;
        private static SharpDXCheckBox checkBoxAimSmoothEnaled;
        private static SharpDXTrackbar trackBarAimSmoothValue;
        private static SharpDXButtonKey keyAimKey;
        private static SharpDXComboValue<int> comboValueAimBone;

        private static SharpDXPanel panelRCSContent;
        private static SharpDXCheckBox checkBoxRCSEnabled;
        private static SharpDXTrackbar trackBarRCSForce;

        private static SharpDXPanel panelTriggerContent;
        private static SharpDXCheckBox checkBoxTriggerEnabled;
        private static SharpDXCheckBox checkBoxTriggerFilterEnemies;
        private static SharpDXCheckBox checkBoxTriggerFilterAllies;
        private static SharpDXRadioButton radioTriggerToggle;
        private static SharpDXRadioButton radioTriggerHold;
        private static SharpDXButtonKey keyTriggerKey;
        private static SharpDXTrackbar trackBarTriggerDelayFirstShot;
        private static SharpDXTrackbar trackBarTriggerDelayShots;
        private static SharpDXCheckBox checkBoxTriggerBurstEnabled;
        private static SharpDXCheckBox checkBoxTriggerBurstRandomize;
        private static SharpDXTrackbar trackBarTriggerBurstShots;

        private static SharpDXPanel panelRadarContent;
        private static SharpDXCheckBox checkBoxRadarEnabled;
        private static SharpDXCheckBox checkBoxRadarAllies;
        private static SharpDXCheckBox checkBoxRadarEnemies;
        private static SharpDXTrackbar trackBarRadarScale;
        private static SharpDXTrackbar trackBarRadarWidth;
        private static SharpDXTrackbar trackBarRadarHeight;

        private static SharpDXPanel panelCrosshairContent;
        private static SharpDXCheckBox checkBoxCrosshairEnabled;
        private static SharpDXTrackbar trackBarCrosshairRadius;
        private static SharpDXTrackbar trackBarCrosshairWidth;
        private static SharpDXTrackbar trackBarCrosshairSpreadScale;
        private static SharpDXCheckBox checkBoxCrosshairOutline;
        private static SharpDXComboValue<int> comboValueCrosshairType;
        private static SharpDXColorControl colorControlCrosshairPrimary;
        private static SharpDXColorControl colorControlCrosshairSecondary;

        private static SharpDXPanel panelWindows;
        private static SharpDXCheckBox checkBoxGraphsEnabled;
        private static SharpDXCheckBox checkBoxSpectatorsEnabled;
        private static SharpDXCheckBox checkBoxBotsEnabled;
        private static SharpDXCheckBox checkBoxEnemiesEnabled;

        //Performance-window
        private static SharpDXWindow windowGraphs;
        private static SharpDXGraph graphMemRead;
        private static SharpDXGraph graphMemWrite;

        //Spectators-window
        private static SharpDXWindow windowSpectators;
        private static SharpDXLabel labelSpectators;

        //Aimbot/Triggerbot window
        private static SharpDXWindow windowBots;
        private static SharpDXLabel labelAimbot;
        private static SharpDXLabel labelTriggerbot;

        //Others
        private static PlayerRadar ctrlRadar;
        private static PlayerESP[] ctrlPlayerESP;
        private static Crosshair ctrlCrosshair;
        private static SharpDX.Direct2D1.Bitmap ranksBmp;
        #endregion

        #region METHODS
        public static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            PrintSuccess("[>]=-- Zat's CSGO-ESP");
            PrintEncolored("[www.unknowncheats.me - Leading the game hacking scene since 2000]", ConsoleColor.Cyan);
            Thread scroller = new Thread(new ThreadStart(LoopScroll));
            scroller.IsBackground = true;
            scroller.Start();
            KeyUtils = new KeyUtils();
            ConfigUtils = new CSGOConfigUtils();

            //ESP
            ConfigUtils.BooleanSettings.AddRange(new string[] {"espEnabled","espBox","espSkeleton", "espName","espHealth", "espAllies", "espEnemies"});
            //Aim
            ConfigUtils.BooleanSettings.AddRange(new string[] { "aimDrawFov", "aimEnabled", "aimToggle", "aimHold", "aimSmoothEnabled", "aimFilterSpotted", "aimFilterSpottedBy", "aimFilterEnemies", "aimFilterAllies", "aimFilterSpottedBy" });
            ConfigUtils.KeySettings.Add("aimKey");
            ConfigUtils.FloatSettings.AddRange(new string[] { "aimFov", "aimSmoothValue" });
            ConfigUtils.IntegerSettings.Add("aimBone");
            //RCS
            ConfigUtils.BooleanSettings.Add("rcsEnabled");
            ConfigUtils.FloatSettings.Add("rcsForce");
            //Trigger
            ConfigUtils.BooleanSettings.AddRange(new string[] { "triggerEnabled", "triggerToggle", "triggerHold", "triggerFilterEnemies", "triggerFilterAllies", "triggerBurstEnabled", "triggerBurstRandomize" });
            ConfigUtils.KeySettings.Add("triggerKey");
            ConfigUtils.FloatSettings.AddRange(new string[] { "triggerDelayFirstShot", "triggerDelayShots", "triggerBurstShots" });
            //Radar
            ConfigUtils.BooleanSettings.AddRange(new string[] { "radarEnabled", "radarAllies", "radarEnemies" });
            ConfigUtils.FloatSettings.AddRange(new string[] { "radarScale", "radarWidth", "radarHeight" });
            //Crosshair
            ConfigUtils.BooleanSettings.AddRange(new string[] { "crosshairEnabled", "crosshairOutline" });
            ConfigUtils.IntegerSettings.AddRange(new string[] { "crosshairType" });
            ConfigUtils.UIntegerSettings.AddRange(new string[] { "crosshairPrimaryColor", "crosshairSecondaryColor" });
            ConfigUtils.FloatSettings.AddRange(new string[] { "crosshairWidth", "crosshairSpreadScale", "crosshairRadius" });
            //Windows
            ConfigUtils.BooleanSettings.AddRange(new string[] { "windowSpectatorsEnabled", "windowPerformanceEnabled", "windowBotsEnabled", "windowEnemiesEnabled" });
            

            ConfigUtils.FillDefaultValues();

            if (!File.Exists("euc_csgo.cfg"))
                ConfigUtils.SaveSettingsToFile("euc_csgo.cfg");
            ConfigUtils.ReadSettingsFromFile("euc_csgo.cfg");

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GAME_PROCESS))
                Thread.Sleep(250);

            ProcUtils = new ProcUtils(GAME_PROCESS, WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite | WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new ExternalUtilsCSharp.MemUtils();
            MemUtils.Handle = ProcUtils.Handle;

            PrintInfo("> Waiting for CSGOs window to show up...");
            while ((hWnd = WinAPI.FindWindowByCaption(hWnd, GAME_TITLE)) == IntPtr.Zero)
                Thread.Sleep(250);

            ProcessModule clientDll, engineDll;
            PrintInfo("> Waiting for CSGO to load client.dll...");
            while ((clientDll = ProcUtils.GetModuleByName(@"bin\client.dll")) == null)
                Thread.Sleep(250);
            PrintInfo("> Waiting for CSGO to load engine.dll...");
            while ((engineDll = ProcUtils.GetModuleByName(@"engine.dll")) == null)
                Thread.Sleep(250);

            Framework = new Framework(clientDll, engineDll);

            PrintInfo("> Initializing overlay");
            using (SHDXOverlay = new SharpDXOverlay())
            {
                SHDXOverlay.Attach(hWnd);
                SHDXOverlay.TickEvent += overlay_TickEvent;
                SHDXOverlay.BeforeDrawingEvent += SHDXOverlay_BeforeDrawingEvent;
                InitializeComponents();
                SharpDXRenderer renderer = SHDXOverlay.Renderer;
                TextFormat smallFont = renderer.CreateFont("smallFont", "Century Gothic", 10f);
                TextFormat largeFont = renderer.CreateFont("largeFont", "Century Gothic", 14f);
                TextFormat heavyFont = renderer.CreateFont("heavyFont", "Century Gothic", 14f, FontStyle.Normal, FontWeight.Heavy);

                windowMenu.Font = smallFont;
                windowMenu.Caption.Font = largeFont;
                windowGraphs.Font = smallFont;
                windowGraphs.Caption.Font = largeFont;
                windowSpectators.Font = smallFont;
                windowSpectators.Caption.Font = largeFont;
                windowBots.Font = smallFont;
                windowBots.Caption.Font = largeFont;
                graphMemRead.Font = smallFont;
                graphMemWrite.Font = smallFont;

                for (int i = 0; i < ctrlPlayerESP.Length; i++)
                {
                    ctrlPlayerESP[i].Font = heavyFont;
                    SHDXOverlay.ChildControls.Add(ctrlPlayerESP[i]);
                }
                ctrlRadar.Font = smallFont;

                windowMenu.ApplySettings(ConfigUtils);

                SHDXOverlay.ChildControls.Add(ctrlCrosshair);
                SHDXOverlay.ChildControls.Add(ctrlRadar);
                SHDXOverlay.ChildControls.Add(windowMenu);
                SHDXOverlay.ChildControls.Add(windowGraphs);
                SHDXOverlay.ChildControls.Add(windowSpectators);
                SHDXOverlay.ChildControls.Add(windowBots);
                SHDXOverlay.ChildControls.Add(cursor);
                PrintInfo("> Running overlay");
                System.Windows.Forms.Application.Run(SHDXOverlay);
            }
            ConfigUtils.SaveSettingsToFile("euc_csgo.cfg");
        }

        private static void LoopScroll()
        {
            scrollText += scrollText;
            while(true)
            {
                scrollIndex++;
                scrollIndex %= (scrollText.Length / 2);
                Console.Title = string.Format("[ {0} ]", scrollText.Substring(scrollIndex, scrollLength));
                Thread.Sleep(150);
            }
        }

        static void SHDXOverlay_BeforeDrawingEvent(object sender, ExternalUtilsCSharp.UI.Overlay<SharpDXRenderer, SharpDX.Color, SharpDX.Vector2, TextFormat>.OverlayEventArgs e)
        {
            if (ranksBmp == null)
            {
                System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)Resources.ResourceManager.GetObject("uc_exclusive");
                try
                {
                    ranksBmp = SDXBitmapFromSysBitmap(e.Overlay.Renderer.Device, bmp);
                }
                catch { }
            }
            else
            {
                SharpDX.RectangleF source = new SharpDX.RectangleF(0,0,ranksBmp.PixelSize.Width, ranksBmp.PixelSize.Height);

                float mul = 1f + 0.2f * (float)Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 0.5f);
                SharpDX.RectangleF dest = new SharpDX.RectangleF(e.Overlay.Width / 2f - source.Width / 2f * mul, e.Overlay.Height - source.Height * 2f * mul, source.Width * mul, source.Height * mul);
                
                e.Overlay.Renderer.Device.DrawBitmap(ranksBmp, dest, 0.9f, BitmapInterpolationMode.Linear, source);
            }

            if (ConfigUtils.GetValue<bool>("aimEnabled") && ConfigUtils.GetValue<bool>("aimDrawFov"))
            {
                float fov = ConfigUtils.GetValue<float>("aimFov");
                
                SharpDX.Color foreColor = new SharpDX.Color(0.2f, 0.2f, 0.2f, 0.8f);
                SharpDX.Color backColor = new SharpDX.Color(0.8f, 0.8f, 0.8f, 0.9f);
                SharpDX.Vector2 size = new SharpDX.Vector2(e.Overlay.Width / 90f * fov);
                SharpDX.Vector2 center = new SharpDX.Vector2(e.Overlay.Width / 2f, e.Overlay.Height / 2f);

                e.Overlay.Renderer.DrawEllipse(foreColor, center, size, true, 3f);
                e.Overlay.Renderer.DrawEllipse(backColor, center, size, true);
            }
        }

        private static void overlay_TickEvent(object sender, SharpDXOverlay.DeltaEventArgs e)
        {
            seconds += e.SecondsElapsed;
            //Update logic
            KeyUtils.Update();
            Framework.Update();
            SHDXOverlay.UpdateControls(e.SecondsElapsed, KeyUtils);

            //Process input
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.DELETE))
                SHDXOverlay.Kill();
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.INSERT))
                Framework.MouseEnabled = !Framework.MouseEnabled;
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.HOME))
                windowMenu.Visible = !windowMenu.Visible;

            //Update UI
            cursor.Visible = !Framework.MouseEnabled;

            if (seconds >= 1)
            {
                seconds = 0;
                graphMemRead.AddValue(MemUtils.BytesRead);
                graphMemWrite.AddValue(MemUtils.BytesWritten);
            }

            ctrlRadar.X = SHDXOverlay.Width - ctrlRadar.Width;
            ctrlCrosshair.X = SHDXOverlay.Width / 2f;
            ctrlCrosshair.Y = SHDXOverlay.Height / 2f;


            for (int i = 0; i < ctrlPlayerESP.Length; i++)
                ctrlPlayerESP[i].Visible = false;

            labelAimbot.Text = string.Format("Aimbot: {0}", Framework.AimbotActive ? "ON" : "OFF");
            labelAimbot.ForeColor = Framework.AimbotActive ? SharpDX.Color.Green : windowBots.Caption.ForeColor;
            labelTriggerbot.Text = string.Format("Triggerbot: {0}", Framework.TriggerbotActive ? "ON" : "OFF");
            labelTriggerbot.ForeColor = Framework.TriggerbotActive ? SharpDX.Color.Green : windowBots.Caption.ForeColor;

            windowGraphs.Visible = ConfigUtils.GetValue<bool>("windowPerformanceEnabled");
            windowBots.Visible = ConfigUtils.GetValue<bool>("windowBotsEnabled");
            windowSpectators.Visible = ConfigUtils.GetValue<bool>("windowSpectatorsEnabled");
            ctrlRadar.Visible = ConfigUtils.GetValue<bool>("radarEnabled");
            ctrlRadar.Width = ConfigUtils.GetValue<float>("radarWidth");
            ctrlRadar.Height = ConfigUtils.GetValue<float>("radarHeight");
            if (ctrlCrosshair.PrimaryColor.ToRgba() != colorControlCrosshairPrimary.SDXColor.ToRgba())
                ctrlCrosshair.PrimaryColor = colorControlCrosshairPrimary.SDXColor;
            if (ctrlCrosshair.SecondaryColor.ToRgba() != colorControlCrosshairSecondary.SDXColor.ToRgba())
                ctrlCrosshair.SecondaryColor = colorControlCrosshairSecondary.SDXColor;
            ctrlCrosshair.Type = (Crosshair.Types)ConfigUtils.GetValue<int>("crosshairType");
            ctrlCrosshair.Visible = ConfigUtils.GetValue<bool>("crosshairEnabled");
            ctrlCrosshair.Outline = ConfigUtils.GetValue<bool>("crosshairOutline");
            ctrlCrosshair.Radius = ConfigUtils.GetValue<float>("crosshairRadius");
            ctrlCrosshair.Width = ConfigUtils.GetValue<float>("crosshairWidth");
            ctrlCrosshair.SpreadScale = ConfigUtils.GetValue<float>("crosshairSpreadScale");

            if (Framework.LocalPlayer != null)
            {
                Weapon wpn = Framework.LocalPlayer.GetActiveWeapon();
                if (wpn != null)
                    ctrlCrosshair.Spread = wpn.m_fAccuracyPenalty * 10000;
                else
                    ctrlCrosshair.Spread = 1f;
            }
            else { ctrlCrosshair.Spread = 1f; }
            if (Framework.IsPlaying())
            {
                #region ESP
                for (int i = 0; i < ctrlPlayerESP.Length && i < Framework.Players.Length; i++)
                {
                    if (Framework.Players[i].Item2.m_iDormant != 1 &&
                        (ConfigUtils.GetValue<bool>("espEnemies") && Framework.Players[i].Item2.m_iTeamNum != Framework.LocalPlayer.m_iTeamNum) ||
                        (ConfigUtils.GetValue<bool>("espAllies") && Framework.Players[i].Item2.m_iTeamNum == Framework.LocalPlayer.m_iTeamNum))
                    ctrlPlayerESP[i].Visible = true;
                    ctrlPlayerESP[i].Player = Framework.Players[i].Item2;
                }
                #endregion
                #region Spectators
                if (Framework.LocalPlayer != null)
                {
                    var spectators = Framework.Players.Where(x => x.Item2.m_hObserverTarget == Framework.LocalPlayer.m_iID && x.Item2.m_iHealth == 0 && x.Item2.m_iDormant != 1);
                    StringBuilder builder = new StringBuilder();
                    foreach (Tuple<int, CSPlayer> spec in spectators)
                    {
                        CSPlayer player = spec.Item2;
                        builder.AppendFormat("{0} [{1}]{2}", Framework.Names[player.m_iID], (SpectatorView)player.m_iObserverMode, builder.Length > 0 ? "\n" : "");
                    }
                    if (builder.Length > 0)
                        labelSpectators.Text = builder.ToString();
                    else
                        labelSpectators.Text = "<none>";
                }
                else
                {
                    labelSpectators.Text = "<none>";
                }
                #endregion
            }
            else
            {
                labelSpectators.Text = "<none>";
                //ctrlRadar.Visible = false;
            }
        }

        private static void InitializeComponents()
        {
            PrintInfo("> Initializing controls");

            cursor = new SharpDXCursor();

            windowGraphs = new SharpDXWindow();
            windowGraphs.Caption.Text = "Performance";

            graphMemRead = new SharpDXGraph();
            graphMemRead.DynamicMaximum = true;
            graphMemRead.Width = 256;
            graphMemRead.Height = 48;
            graphMemRead.Text = "RPM data/s";
            graphMemWrite = new SharpDXGraph();
            graphMemWrite.DynamicMaximum = true;
            graphMemWrite.Width = 256;
            graphMemWrite.Height = 48;
            graphMemWrite.Text = "WPM data/s";

            windowGraphs.Panel.AddChildControl(graphMemRead);
            windowGraphs.Panel.AddChildControl(graphMemWrite);  

            windowMenu = new SharpDXWindow();
            windowMenu.Caption.Text = "[UC|CSGO] Zat's Multihack v3";
            windowMenu.X = 500;
            windowMenu.Panel.DynamicWidth = true;

            InitLabel(ref labelHotkeys, "[INS] Toggle mouse [HOME] Toggle menu\n[DEL] Terminate hack", false, 150, SharpDXLabel.TextAlignment.Center);

            tabsMenu = new SharpDXTabControl();
            tabsMenu.FillParent = false;

            InitPanel(ref panelESPContent, "ESP", false, true, true, false);
            panelESPContent.ContentLayout = new TableLayout(2);
            InitCheckBox(ref checkBoxESPEnabled, "Enabled", "espEnabled", true);
            InitCheckBox(ref checkBoxESPBox, "Draw box", "espBox", false);
            InitCheckBox(ref checkBoxESPSkeleton, "Draw skeleton", "espSkeleton", true);
            InitCheckBox(ref checkBoxESPName, "Draw name", "espName", false);
            InitCheckBox(ref checkBoxESPHealth, "Draw health", "espHealth", true);
            InitCheckBox(ref checkBoxESPAllies, "Filter: Draw allies", "espAllies", true);
            InitCheckBox(ref checkBoxESPEnemies, "Filter: Draw enemies", "espEnemies", true);

            InitPanel(ref panelAimContent, "Aim", false, true, true, false);
            panelAimContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref checkBoxAimEnabled, "Enabled", "aimEnabled", true);
            InitCheckBox(ref checkBoxAimDrawFov, "Draw fov", "aimDrawFov", true);
            InitButtonKey(ref keyAimKey, "Key", "aimKey");
            InitTrackBar(ref trackBarAimFov, "Aimbot FOV", "aimFov", 1, 180, 20, 0);
            InitRadioButton(ref radioAimHold, "Mode: Hold key", "aimHold", true);
            InitRadioButton(ref radioAimToggle, "Mode: Toggle", "aimToggle", false);
            InitCheckBox(ref checkBoxAimSmoothEnaled, "Smoothing", "aimSmoothEnabled", true);
            InitTrackBar(ref trackBarAimSmoothValue, "Smooth-factor", "aimSmoothValue", 0, 1, 0.2f, 4);
            InitCheckBox(ref checkBoxAimFilterSpotted, "Filter: Spotted by me", "aimFilterSpotted", false);
            InitCheckBox(ref checkBoxAimFilterSpottedBy, "Filter: Spotted me", "aimFilterSpottedBy", false);
            InitCheckBox(ref checkBoxAimFilterEnemies, "Filter: Enemies", "aimFilterEnemies", true);
            InitCheckBox(ref checkBoxAimFilterAllies, "Filter: Allies", "aimFilterAllies", false);
            InitComboValue(ref comboValueAimBone, "Bone", "aimBone", new Tuple<string, int>("Neck", 10), new Tuple<string, int>("Chest", 4), new Tuple<string, int>("Hips", 1));

            InitPanel(ref panelRCSContent, "RCS", false, true, true, false);
            InitCheckBox(ref checkBoxRCSEnabled, "Enabled", "rcsEnabled", true);
            InitTrackBar(ref trackBarRCSForce, "Force (%)", "rcsForce", 1, 100, 100, 2);
            
            InitPanel(ref panelTriggerContent, "Trigger", false, true, true, false);
            panelTriggerContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref checkBoxTriggerEnabled, "Enabled", "triggerEnabled", true);
            InitButtonKey(ref keyTriggerKey, "Key", "triggerKey");
            InitTrackBar(ref trackBarTriggerDelayFirstShot, "Delay (ms, first shot)", "triggerDelayFirstShot", 1, 1000, 30, 0);
            InitTrackBar(ref trackBarTriggerDelayShots, "Delay (ms)", "triggerDelayShots", 1, 1000, 30, 0);
            InitRadioButton(ref radioTriggerHold, "Mode: Hold key", "triggerHold", true);
            InitRadioButton(ref radioTriggerToggle, "Mode: Toggle", "triggerToggle", false);
            InitCheckBox(ref checkBoxTriggerFilterEnemies, "Filter: Enemies", "triggerFilterEnemies", true);
            InitCheckBox(ref checkBoxTriggerFilterAllies, "Filter: Allies", "triggerFilterAllies", false);
            InitCheckBox(ref checkBoxTriggerBurstEnabled, "Burst enabled", "triggerBurstEnabled", true);
            InitCheckBox(ref checkBoxTriggerBurstRandomize, "Burst randomization", "triggerBurstRandomize", true);
            InitTrackBar(ref trackBarTriggerBurstShots, "No. of burst-fire shots", "triggerBurstShots", 1, 10, 3, 0);

            InitPanel(ref panelRadarContent, "Radar", false, true, true, false);
            panelRadarContent.ContentLayout = TableLayout.ThreeColumns;
            InitCheckBox(ref checkBoxRadarEnabled, "Enabled", "radarEnabled", true);
            InitCheckBox(ref checkBoxRadarAllies, "Filter: Draw allies", "radarAllies", true);
            InitCheckBox(ref checkBoxRadarEnemies, "Filter: Draw enemies", "radarEnemies", true);
            InitTrackBar(ref trackBarRadarScale, "Scale", "radarScale", 0, 0.25f, 0.02f, 4);
            InitTrackBar(ref trackBarRadarWidth, "Width (px)", "radarWidth", 16, 1024, 256, 0);
            InitTrackBar(ref trackBarRadarHeight, "Height (px)", "radarHeight", 16, 1024, 256, 0);

            InitPanel(ref panelCrosshairContent, "Crosshair", false, true, true, false);
            panelCrosshairContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref checkBoxCrosshairEnabled, "Enabled", "crosshairEnabled", true);
            InitTrackBar(ref trackBarCrosshairRadius, "Radius (px)", "crosshairRadius", 1, 128, 16f, 1);
            InitTrackBar(ref trackBarCrosshairWidth, "Width (px)", "crosshairWidth", 0.1f, 32f, 1f, 1);
            InitTrackBar(ref trackBarCrosshairSpreadScale, "Spread-scale", "crosshairSpreadScale", 0.01f, 1f, 1f, 2);
            InitCheckBox(ref checkBoxCrosshairOutline, "Outline", "crosshairOutline", true);
            InitComboValue(ref comboValueCrosshairType, "Type", "crosshairType", 
                new Tuple<string, int>("Default", 0),
                new Tuple<string, int>("Default tilted", 1),
                new Tuple<string, int>("Rectangle", 2),
                new Tuple<string, int>("Rectangle tilted", 3),
                new Tuple<string, int>("Circle", 4));
            InitColorControl(ref colorControlCrosshairPrimary, "Primary color", "crosshairPrimaryColor", new Color(255, 255, 255, 255));
            InitColorControl(ref colorControlCrosshairSecondary, "Secondary color", "crosshairSecondaryColor", new Color(255, 255, 255, 255));


            InitPanel(ref panelWindows, "Windows", true, true, true, true);
            InitCheckBox(ref checkBoxGraphsEnabled, "Performance-window enabled", "windowPerformanceEnabled", true);
            InitCheckBox(ref checkBoxSpectatorsEnabled, "Spectators-window enabled", "windowSpectatorsEnabled", true);
            InitCheckBox(ref checkBoxBotsEnabled, "Bots-window enabled", "windowBotsEnabled", true);
            InitCheckBox(ref checkBoxEnemiesEnabled, "Enemies-info enaled", "windowEnemiesEnabled", true);

            tabsMenu.AddChildControl(panelAimContent);
            tabsMenu.AddChildControl(panelESPContent);
            tabsMenu.AddChildControl(panelRadarContent);
            tabsMenu.AddChildControl(panelRCSContent);
            tabsMenu.AddChildControl(panelTriggerContent);
            tabsMenu.AddChildControl(panelCrosshairContent);
            tabsMenu.AddChildControl(panelWindows);
            windowMenu.Panel.AddChildControl(labelHotkeys);
            windowMenu.Panel.AddChildControl(tabsMenu);

            panelESPContent.AddChildControl(checkBoxESPEnabled);
            panelESPContent.AddChildControl(checkBoxESPBox);
            panelESPContent.AddChildControl(checkBoxESPSkeleton);
            panelESPContent.AddChildControl(checkBoxESPName);
            panelESPContent.AddChildControl(checkBoxESPHealth);
            panelESPContent.AddChildControl(checkBoxESPAllies);
            panelESPContent.AddChildControl(checkBoxESPEnemies);

            panelAimContent.AddChildControl(checkBoxAimEnabled);
            panelAimContent.AddChildControl(checkBoxAimSmoothEnaled);
            panelAimContent.AddChildControl(trackBarAimFov);
            panelAimContent.AddChildControl(trackBarAimSmoothValue);
            panelAimContent.AddChildControl(radioAimHold);
            panelAimContent.AddChildControl(radioAimToggle);
            panelAimContent.AddChildControl(checkBoxAimFilterSpotted);
            panelAimContent.AddChildControl(checkBoxAimFilterSpottedBy);
            panelAimContent.AddChildControl(checkBoxAimFilterEnemies);
            panelAimContent.AddChildControl(checkBoxAimFilterAllies);
            panelAimContent.AddChildControl(checkBoxAimDrawFov);
            panelAimContent.AddChildControl(comboValueAimBone);

            panelAimContent.AddChildControl(keyAimKey);

            panelRCSContent.AddChildControl(checkBoxRCSEnabled);
            panelRCSContent.AddChildControl(trackBarRCSForce);

            panelTriggerContent.AddChildControl(checkBoxTriggerEnabled);
            panelTriggerContent.AddChildControl(keyTriggerKey);
            panelTriggerContent.AddChildControl(trackBarTriggerDelayFirstShot);
            panelTriggerContent.AddChildControl(trackBarTriggerDelayShots);
            panelTriggerContent.AddChildControl(radioTriggerHold);
            panelTriggerContent.AddChildControl(radioTriggerToggle);
            panelTriggerContent.AddChildControl(checkBoxTriggerFilterEnemies);
            panelTriggerContent.AddChildControl(checkBoxTriggerFilterAllies);
            panelTriggerContent.AddChildControl(checkBoxTriggerBurstEnabled);
            panelTriggerContent.AddChildControl(checkBoxTriggerBurstRandomize);
            panelTriggerContent.AddChildControl(trackBarTriggerBurstShots);

            panelRadarContent.AddChildControl(checkBoxRadarEnabled);
            panelRadarContent.AddChildControl(checkBoxRadarAllies);
            panelRadarContent.AddChildControl(checkBoxRadarEnemies);
            panelRadarContent.AddChildControl(trackBarRadarScale);
            panelRadarContent.AddChildControl(trackBarRadarWidth);
            panelRadarContent.AddChildControl(trackBarRadarHeight);

            panelCrosshairContent.AddChildControl(checkBoxCrosshairEnabled);
            panelCrosshairContent.AddChildControl(checkBoxCrosshairOutline);
            panelCrosshairContent.AddChildControl(trackBarCrosshairRadius);
            panelCrosshairContent.AddChildControl(trackBarCrosshairWidth);
            panelCrosshairContent.AddChildControl(trackBarCrosshairSpreadScale);
            panelCrosshairContent.AddChildControl(comboValueCrosshairType);
            panelCrosshairContent.AddChildControl(colorControlCrosshairPrimary);
            panelCrosshairContent.AddChildControl(colorControlCrosshairSecondary);

            panelWindows.AddChildControl(checkBoxGraphsEnabled);
            panelWindows.AddChildControl(checkBoxSpectatorsEnabled);
            panelWindows.AddChildControl(checkBoxBotsEnabled);
            panelWindows.AddChildControl(checkBoxEnemiesEnabled);

            windowSpectators = new SharpDXWindow();
            windowSpectators.Caption.Text = "Spectators";
            windowSpectators.Y = 500;
            InitLabel(ref labelSpectators, "<none>", false, 200f, SharpDXLabel.TextAlignment.Left);
            windowSpectators.Panel.AddChildControl(labelSpectators);

            windowBots = new SharpDXWindow();
            windowBots.Caption.Text = "Bot-status";
            windowBots.Y = 300;
            InitLabel(ref labelAimbot, "Aimbot: -", false, 200f, SharpDXLabel.TextAlignment.Left);
            InitLabel(ref labelTriggerbot, "Triggerbot: -", false, 200f, SharpDXLabel.TextAlignment.Left);
            windowBots.Panel.AddChildControl(labelAimbot);
            windowBots.Panel.AddChildControl(labelTriggerbot);

            ctrlRadar = new PlayerRadar();
            ctrlRadar.Width = 128;
            ctrlRadar.Height = 128;
            ctrlRadar.Scaling = 0.02f;
            ctrlRadar.DotRadius = 2f;
            ctrlRadar.Rotating = true;

            ctrlPlayerESP = new PlayerESP[64];
            for (int i = 0; i < ctrlPlayerESP.Length;i++)
            {
                ctrlPlayerESP[i] = new PlayerESP();
                ctrlPlayerESP[i].Visible = false;
            }

            ctrlCrosshair = new Crosshair();
            ctrlCrosshair.Radius = 16f;
            ctrlCrosshair.Width = 2f;
        }

        static void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            SharpDXCheckBox control = (SharpDXCheckBox)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Checked);
        }
        private static void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            SharpDXRadioButton control = (SharpDXRadioButton)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Checked);
        }
        private static void trackBar_ValueChangedEvent(object sender, EventArgs e)
        {
            SharpDXTrackbar control = (SharpDXTrackbar)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Value);
        }
        static void buttonKey_KeyChangedEvent(object sender, EventArgs e)
        {
            SharpDXButtonKey control = (SharpDXButtonKey)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Key);
        }
        static void button_MouseClickEventUp(object sender, ExternalUtilsCSharp.UI.Control<SharpDXRenderer, SharpDX.Color, SharpDX.Vector2, TextFormat>.MouseEventArgs e)
        {
            if (!e.LeftButton)
                return;
            SharpDXPanel panel = (SharpDXPanel)((SharpDXButton)sender).Tag;
            panel.Visible = !panel.Visible;
        }

        private static void comboValue_SelectedIndexChangedEvent<T>(object sender, SharpDXComboValue<T>.ComboValueEventArgs e)
        {
            ConfigUtils.SetValue(e.Tag.ToString(), e.Value);
        }

        static void control_ColorChangedEvent(object sender, EventArgs e)
        {
            SharpDXColorControl control = (SharpDXColorControl)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Color.ToRGBA());
        }
        #endregion

        #region HELPERS
        private static SharpDX.Direct2D1.Bitmap SDXBitmapFromSysBitmap(SharpDX.Direct2D1.WindowRenderTarget device, System.Drawing.Bitmap bitmap)
        {
            var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapProperties = new BitmapProperties(new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
            var size = new SharpDX.Size2(bitmap.Width, bitmap.Height);

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof(int);
            using (var tempStream = new SharpDX.DataStream(bitmap.Height * stride, true, true))
            {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                // Convert all pixels 
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int offset = bitmapData.Stride * y;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Not optimized 
                        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                        tempStream.Write(rgba);
                    }

                }
                bitmap.UnlockBits(bitmapData);
                tempStream.Position = 0;

                return new SharpDX.Direct2D1.Bitmap(device, size, tempStream, stride, bitmapProperties);
            }
        }
        private static void InitColorControl(ref SharpDXColorControl control, string text, object tag, Color color)
        {
            control = new SharpDXColorControl();
            control.Text = text;
            control.Tag = tag;
            control.Color = color;
            control.ColorChangedEvent += control_ColorChangedEvent;
        }
        
        private static void InitComboValue<T>(ref SharpDXComboValue<T> control, string text, object tag, params Tuple<string, T>[] values)
        {
            control = new SharpDXComboValue<T>();
            control.Text = text;
            control.Tag = tag;
            control.Values = values;
            control.SelectedIndexChangedEvent += comboValue_SelectedIndexChangedEvent;
        }
        private static void InitButtonKey(ref SharpDXButtonKey control, string text, object tag)
        {
            control = new SharpDXButtonKey();
            control.Text = text;
            control.Tag = tag;
            control.KeyChangedEvent += buttonKey_KeyChangedEvent;
        }
        private static void InitPanel(ref SharpDXPanel control, string text, bool dynamicWidth = true, bool dynamicHeight = true, bool fillParent = true, bool visible = true)
        {
            control = new SharpDXPanel();
            control.Text = text;
            control.DynamicHeight = dynamicHeight;
            control.DynamicWidth = dynamicWidth;
            control.FillParent = fillParent;
            control.Visible = visible;
        }
        private static void InitToggleButton(ref SharpDXButton control, string text, SharpDXPanel tag)
        {
            control = new SharpDXButton();
            control.Text = text;
            control.Tag = tag;
            control.MouseClickEventUp += button_MouseClickEventUp;
        }
        private static void InitTrackBar(ref SharpDXTrackbar control, string text, object tag, float min =0, float max = 100, float value = 50, int numberofdecimals = 2)
        {
            control = new SharpDXTrackbar();
            control.Text = text;
            control.Tag = tag;
            control.Minimum = min;
            control.Maximum = max;
            control.Value = value;
            control.NumberOfDecimals = numberofdecimals;
            control.ValueChangedEvent += trackBar_ValueChangedEvent;
        }

        private static void InitRadioButton(ref SharpDXRadioButton control, string text, object tag, bool bChecked)
        {
            control = new SharpDXRadioButton();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += radioButton_CheckedChanged;
        }
        private static void InitLabel(ref SharpDXLabel control, string text, bool fixedWidth = false, float width = 0f, SharpDXLabel.TextAlignment alignment = SharpDXLabel.TextAlignment.Left)
        {
            control = new SharpDXLabel();
            control.FixedWidth = fixedWidth;
            control.Width = width;
            control.TextAlign = alignment;
            control.Text = text;
            control.Tag = null;
        }
        private static void InitCheckBox(ref SharpDXCheckBox control, string text, object tag, bool bChecked)
        {
            control = new SharpDXCheckBox();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += checkBox_CheckedChanged;
        }
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }
        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }
        public static void PrintException(Exception ex)
        {
            PrintError("An Exception occured: {0}\n\"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
        }
        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            ConsoleColor clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion
    }
}
