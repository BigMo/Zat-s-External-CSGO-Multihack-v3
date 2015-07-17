using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSGOTriggerbot.CSGOClasses
{
    public class Framework
    {
        #region VARIABLES
        private int
            dwEntityList,
            dwViewMatrix,
            dwLocalPlayer,
            dwClientState,
            clientDllBase,
            engineDllBase,
            dwIGameResources;
        private bool mouseEnabled;
        private string mapName;
        #endregion
        #region PROPERTIES
        public CSLocalPlayer LocalPlayer { get; private set; }
        public BaseEntity Target { get; private set; }
        public Tuple<int, CSPlayer>[] Players { get; private set; }
        public Tuple<int, BaseEntity>[] Entities { get; private set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Vector3 ViewAngles { get; private set; }
        public Vector3 NewViewAngles { get; private set; }
        public int[] Kills { get; private set; }
        public int[] Deaths { get; private set; }
        public int[] Assists { get; private set; }
        public int[] Armor { get; private set; }
        public int[] Score { get; private set; }
        public string[] Clantags { get; private set; }
        public string[] Names { get; private set; }
        public SignOnState State { get; set; }
        public bool MouseEnabled
        {
            get { return mouseEnabled; }
            set
            {
                if (value != mouseEnabled)
                {
                    mouseEnabled = value;                    
                    //WinAPI.SetCursorPos(WithOverlay.SHDXOverlay.Location.X + WithOverlay.SHDXOverlay.Width / 2, WithOverlay.SHDXOverlay.Location.Y + WithOverlay.SHDXOverlay.Height / 2);
                    //WithOverlay.MemUtils.Write<byte>((IntPtr)(clientDllBase + CSGOOffsets.Misc.MouseEnable), value ? (byte)1 : (byte)0);
                }
            }
        }
        public bool AimbotActive { get; set; }
        public bool TriggerbotActive { get; set; }
        private bool RCSHandled { get; set; }
        private int LastShotsFired { get; set; }
        private int LastClip { get; set; }
        private Vector3 LastPunch { get; set; }
        private bool TriggerOnTarget { get; set; }
        private long TriggerLastTarget { get; set; }
        private long TriggerLastShot { get; set; }
        private int TriggerBurstFired { get; set; }
        private int TriggerBurstCount { get; set; }
        private bool TriggerShooting { get; set; }
        private Weapon LocalPlayerWeapon { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Framework(ProcessModule clientDll, ProcessModule engineDll)
        {
            CSGOScanner.ScanOffsets(WithOverlay.MemUtils, clientDll, engineDll);
            clientDllBase = (int)clientDll.BaseAddress;
            engineDllBase = (int)engineDll.BaseAddress;
            dwEntityList = clientDllBase + CSGOOffsets.Misc.EntityList;
            dwViewMatrix = clientDllBase + CSGOOffsets.Misc.ViewMatrix;
            dwClientState = WithOverlay.MemUtils.Read<int>((IntPtr)(engineDllBase + CSGOOffsets.ClientState.Base));
            mouseEnabled = true;
            AimbotActive = false;
            TriggerbotActive = false;
        }
        #endregion

        #region METHODS
        public void Update()
        {
            List<Tuple<int, CSPlayer>> players = new List<Tuple<int, CSPlayer>>();
            List<Tuple<int, BaseEntity>> entities = new List<Tuple<int, BaseEntity>>();
            List<Tuple<int, Weapon>> weapons = new List<Tuple<int, Weapon>>();

            dwLocalPlayer = WithOverlay.MemUtils.Read<int>((IntPtr)(clientDllBase + CSGOOffsets.Misc.LocalPlayer));
            dwIGameResources = WithOverlay.MemUtils.Read<int>((IntPtr)(clientDllBase + CSGOOffsets.GameResources.Base));

            State = (SignOnState)WithOverlay.MemUtils.Read<int>((IntPtr)(dwClientState + CSGOOffsets.ClientState.m_dwInGame));
            if (State != SignOnState.SIGNONSTATE_FULL)
                return;

            ViewMatrix = WithOverlay.MemUtils.ReadMatrix((IntPtr)dwViewMatrix, 4, 4);
            ViewAngles = WithOverlay.MemUtils.Read<Vector3>((IntPtr)(dwClientState + CSGOOffsets.ClientState.m_dwViewAngles));
            NewViewAngles = ViewAngles;
            RCSHandled = false;

            #region Read entities
            byte[] data = new byte[16 * 8192];
            WithOverlay.MemUtils.Read((IntPtr)(dwEntityList), out data, data.Length);

            for (int i = 0; i < data.Length / 16; i++)
            {
                int address = BitConverter.ToInt32(data, 16 * i);
                if (address != 0)
                {
                    BaseEntity ent = new BaseEntity(address);
                    if (!ent.IsValid())
                        continue;
                    if (ent.IsPlayer())
                        players.Add(new Tuple<int, CSPlayer>(i, new CSPlayer(ent)));
                    else if (ent.IsWeapon())
                        weapons.Add(new Tuple<int, Weapon>(i, new Weapon(ent)));
                    else
                        entities.Add(new Tuple<int, BaseEntity>(i, ent));
                }
            }

            Players = players.ToArray();
            Entities = entities.ToArray();
            Weapons = weapons.ToArray();
            #endregion

            #region LocalPlayer and Target
            if (players.Exists(x => x.Item2.Address == dwLocalPlayer))
            {
                LocalPlayer = new CSLocalPlayer(players.First(x => x.Item2.Address == dwLocalPlayer).Item2);
                LocalPlayerWeapon = LocalPlayer.GetActiveWeapon();
            }
            else
            {
                LocalPlayer = null;
                LocalPlayerWeapon = null;
            }

            if (LocalPlayer != null)
            {
                if (entities.Exists(x => x.Item1 == LocalPlayer.m_iCrosshairIdx - 1))
                    Target = entities.First(x => x.Item1 == LocalPlayer.m_iCrosshairIdx - 1).Item2;
                if (players.Exists(x => x.Item1 == LocalPlayer.m_iCrosshairIdx - 1))
                    Target = players.First(x => x.Item1 == LocalPlayer.m_iCrosshairIdx - 1).Item2;
                else
                    Target = null;
            }
            #endregion

            if (LocalPlayer == null)
                return;

            #region IGameResources
            if (dwIGameResources != 0)
            {
                Kills = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Kills), 65);
                Deaths = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Deaths), 65);
                Armor = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Armor), 65);
                Assists = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Assists), 65);
                Score = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Score), 65);

                byte[] clantagsData = new byte[16 * 65];
                WithOverlay.MemUtils.Read((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Clantag), out clantagsData, clantagsData.Length);
                string[] clantags = new string[65];
                for (int i = 0; i < 65; i++)
                    clantags[i] = Encoding.Unicode.GetString(clantagsData, i * 16, 16);
                Clantags = clantags;

                int[] namePtrs = WithOverlay.MemUtils.ReadArray<int>((IntPtr)(dwIGameResources + CSGOOffsets.GameResources.Names), 65);
                string[] names = new string[65];
                for (int i = 0; i < 65; i++)
                    try
                    {
                        names[i] = WithOverlay.MemUtils.ReadString((IntPtr)namePtrs[i], 32, Encoding.ASCII);
                    }
                    catch { }
                Names = names;
            }
            #endregion

            #region Aimbot
            if (WithOverlay.ConfigUtils.GetValue<bool>("aimEnabled"))
            {
                if (WithOverlay.ConfigUtils.GetValue<bool>("aimToggle"))
                {
                    if (WithOverlay.KeyUtils.KeyWentUp(WithOverlay.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("aimKey")))
                        AimbotActive = !AimbotActive;
                }
                else if (WithOverlay.ConfigUtils.GetValue<bool>("aimHold"))
                {
                    AimbotActive = WithOverlay.KeyUtils.KeyIsDown(WithOverlay.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("aimKey"));
                }
                if (AimbotActive)
                    DoAimbot();
            }
            #endregion

            #region RCS
            DoRCS();
            #endregion

            #region Set view angles
            if (NewViewAngles != ViewAngles)
                SetViewAngles(NewViewAngles);
            #endregion

            #region triggerbot
            if (WithOverlay.ConfigUtils.GetValue<bool>("triggerEnabled"))
            {
                if (WithOverlay.ConfigUtils.GetValue<bool>("triggerToggle"))
                {
                    if (WithOverlay.KeyUtils.KeyWentUp(WithOverlay.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("triggerKey")))
                        TriggerbotActive = !TriggerbotActive;
                }
                else if (WithOverlay.ConfigUtils.GetValue<bool>("triggerHold"))
                {
                    TriggerbotActive = WithOverlay.KeyUtils.KeyIsDown(WithOverlay.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("triggerKey"));
                }
                if (TriggerbotActive && !WithOverlay.KeyUtils.KeyIsDown(WinAPI.VirtualKeyShort.LBUTTON))
                    DoTriggerbot();
            }
            #endregion
            
            #region triggerbot-burst
            if (TriggerShooting)
            {
                if (LocalPlayerWeapon == null)
                {
                    TriggerShooting = false;
                }
                else
                {
                    if (WithOverlay.ConfigUtils.GetValue<bool>("triggerBurstEnabled"))
                    {
                        if (TriggerBurstFired >= TriggerBurstCount)
                        {
                            TriggerShooting = false;
                            TriggerBurstFired = 0;
                        }
                        else
                        {
                            if (LocalPlayerWeapon.m_iClip1 != LastClip)
                            {
                                TriggerBurstFired += Math.Abs(LocalPlayerWeapon.m_iClip1 - LastClip);
                            }
                            else
                            {
                                Shoot();
                            }
                        }
                    }
                    else
                    {
                        Shoot();
                        TriggerShooting = false;
                    }
                }
            }

            #endregion
            if (LocalPlayerWeapon != null)
                LastClip = LocalPlayerWeapon.m_iClip1;
            else
                LastClip = 0;
            LastShotsFired = LocalPlayer.m_iShotsFired;
            LastPunch = LocalPlayer.m_vecPunch;
        }

        public void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = MathUtils.ClampAngle(viewAngles);
            WithOverlay.MemUtils.Write<Vector3>((IntPtr)(dwClientState + CSGOOffsets.ClientState.m_dwViewAngles), viewAngles);
        }

        public bool IsPlaying()
        {
            return State == SignOnState.SIGNONSTATE_FULL && LocalPlayer != null;
        }

        public void DoAimbot()
        {
            if (LocalPlayer == null)
                return;
            var valid = Players.Where(x => x.Item2.IsValid() && x.Item2.m_iHealth != 0 && x.Item2.m_iDormant != 1);
            if (WithOverlay.ConfigUtils.GetValue<bool>("aimFilterSpotted"))
                valid = valid.Where(x => x.Item2.SeenBy(LocalPlayer));
            if (WithOverlay.ConfigUtils.GetValue<bool>("aimFilterSpottedBy"))
                valid = valid.Where(x => LocalPlayer.SeenBy(x.Item2));
            if (WithOverlay.ConfigUtils.GetValue<bool>("aimFilterEnemies"))
                valid = valid.Where(x => x.Item2.m_iTeamNum != LocalPlayer.m_iTeamNum);
            if (WithOverlay.ConfigUtils.GetValue<bool>("aimFilterAllies"))
                valid = valid.Where(x => x.Item2.m_iTeamNum == LocalPlayer.m_iTeamNum);

            valid = valid.OrderBy(x => (x.Item2.m_vecOrigin - LocalPlayer.m_vecOrigin).Length());
            Vector3 closest = Vector3.Zero;
            float closestFov = float.MaxValue;
            foreach (Tuple<int, CSPlayer> tpl in valid)
            {
                CSPlayer plr = tpl.Item2;
                Vector3 newAngles = MathUtils.CalcAngle(LocalPlayer.m_vecOrigin + LocalPlayer.m_vecViewOffset, plr.Bones.GetBoneByIndex(WithOverlay.ConfigUtils.GetValue<int>("aimBone"))) - NewViewAngles;
                newAngles = MathUtils.ClampAngle(newAngles);
                float fov = newAngles.Length() % 360f;
                if (fov < closestFov && fov < WithOverlay.ConfigUtils.GetValue<float>("aimFov"))
                {
                    closestFov = fov;
                    closest = newAngles;
                }
            }
            if (closest != Vector3.Zero)
            {
                DoRCS(true);
                if (WithOverlay.ConfigUtils.GetValue<bool>("aimSmoothEnabled"))
                    NewViewAngles = MathUtils.SmoothAngle(NewViewAngles, NewViewAngles + closest, WithOverlay.ConfigUtils.GetValue<float>("aimSmoothValue"));
                else
                    NewViewAngles += closest;
                NewViewAngles = NewViewAngles;
            }
        }

        public void DoRCS(bool aimbot = false)
        {
            if (WithOverlay.ConfigUtils.GetValue<bool>("rcsEnabled"))
            {
                if (LocalPlayerWeapon != null)
                {
                    if (!RCSHandled && (LocalPlayerWeapon.m_iClip1 != LastClip || LocalPlayer.m_iShotsFired > 0 || WithOverlay.KeyUtils.KeyIsDown(WinAPI.VirtualKeyShort.LBUTTON)))
                    {
                        if (aimbot)
                        {
                            NewViewAngles -= LocalPlayer.m_vecPunch * (2f / 100f * WithOverlay.ConfigUtils.GetValue<float>("rcsForce"));
                        }
                        else
                        {
                            Vector3 punch = LocalPlayer.m_vecPunch - LastPunch;
                            NewViewAngles -= punch * (2f / 100f * WithOverlay.ConfigUtils.GetValue<float>("rcsForce"));
                        }
                        RCSHandled = true;
                    }
                }
            }
        }

        public void DoTriggerbot()
        {
            if (LocalPlayer != null && !TriggerShooting)
            {
                if (Players.Count(x => x.Item2.m_iID == LocalPlayer.m_iCrosshairIdx) > 0)
                {
                    CSPlayer player = Players.First(x=>x.Item2.m_iID == LocalPlayer.m_iCrosshairIdx).Item2;
                    if ((WithOverlay.ConfigUtils.GetValue<bool>("triggerFilterEnemies") && player.m_iTeamNum != LocalPlayer.m_iTeamNum) ||
                        (WithOverlay.ConfigUtils.GetValue<bool>("triggerFilterAllies") && player.m_iTeamNum == LocalPlayer.m_iTeamNum))
                    {
                        if (!TriggerOnTarget)
                        {
                            TriggerOnTarget = true;
                            TriggerLastTarget = DateTime.Now.Ticks;
                        }
                        else
                        {
                            if (new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= WithOverlay.ConfigUtils.GetValue<float>("triggerDelayFirstShot"))
                            {
                                if (new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= WithOverlay.ConfigUtils.GetValue<float>("triggerDelayShots"))
                                {
                                    TriggerLastShot = DateTime.Now.Ticks;
                                    if(!TriggerShooting)
                                    {
                                        if (WithOverlay.ConfigUtils.GetValue<bool>("triggerBurstRandomize"))
                                            TriggerBurstCount = new Random().Next(1, (int)WithOverlay.ConfigUtils.GetValue<float>("triggerBurstShots"));
                                        else TriggerBurstCount = (int)WithOverlay.ConfigUtils.GetValue<float>("triggerBurstShots");
                                    }
                                    TriggerShooting = true;
                                }
                            }
                        }
                    }
                    else 
                    {
                        TriggerOnTarget = false; 
                    }
                }
            }
        }

        private void Shoot()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(1);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }
        #endregion
    }
}
