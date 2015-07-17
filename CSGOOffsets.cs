using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOTriggerbot
{
    class CSGOOffsets
    {
        public class Misc
        {
            public static int EntityList = 0x00;
            public static int LocalPlayer = 0x00;
            public static int Jump = 0x00;
            public static int GlowManager = 0x00;
            public static int SignOnState = 0xE8;
            public static int WeaponTable = 0x04A5DC4C;
            public static int ViewMatrix = 0x00;
            public static int MouseEnable = 0xA7A4C0;
        }
        public class ClientState
        {
            public static int Base = 0x00;
            public static int m_dwInGame = 0xE8;
            public static int m_dwViewAngles = 0x00;
            public static int m_dwMapname = 0x26c;
            public static int m_dwMapDirectory = 0x168;
        }
        public class GameResources
        {
            public static int Base = 0x04A38E2C;
            public static int Names = 0x9D0;
            public static int Kills = 0xBD8;
            public static int Assists = 0xCDC;
            public static int Deaths = 0xDE0;
            public static int Armor = 0x182C;
            public static int Score = 0x192C;
            public static int Clantag = 0x4110;
        }
        public class NetVars
        {
            public class C_BaseEntity
            {
                public static int m_iHealth = 0x00;
                public static int m_iID = 0x00;
                public static int m_iTeamNum = 0x00;
                public static int m_vecOrigin = 0x134;
                public static int m_angRotation = 0x128;
                public static int m_bSpotted = 0x935;
                public static int m_bSpottedByMask = 0x978;
                public static int m_hOwnerEntity = 0x148;
                public static int m_bDormant = 0xE9;
            }

            public class C_CSPlayer
            {
                public static int m_lifeState = 0x25B;
                public static int m_hBoneMatrix = 0x00;
                public static int m_hActiveWeapon = 0x12C0;   // m_hActiveWeapon
                public static int m_iFlags = 0x100;
                public static int m_hObserverTarget = 0x173C;
                public static int m_iObserverMode = 0x1728;
                public static int m_vecVelocity = 0x110;
            }

            public class LocalPlayer
            {
                public static int m_vecViewOffset = 0x104;
                public static int m_vecPunch = 0x13E8;
                public static int m_iShotsFired = 0x1d6C;
                public static int m_iCrosshairIdx = 0x2410;
            }

            public class Weapon
            {
                public static int m_iItemDefinitionIndex = 0x131C;
                public static int m_iState = 0x15B4;
                public static int m_iClip1 = 0x15c0;
                public static int m_flNextPrimaryAttack = 0x159C;
                public static int m_iWeaponID = 0x1690;   // Search for weaponid
                public static int m_bCanReload = 0x15F9;
                public static int m_iWeaponTableIndex = 0x162C;
                public static int m_fAccuracyPenalty = 0x1670;
            }
        }
    }
}
