using System;
using System.Collections.Generic;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System.Linq;
using UnityEngine;
using Oxide.Core;
using Random = UnityEngine.Random;
using Rust;
using ProtoBuf;

namespace Oxide.Plugins
{
    [Info("SIMMONYGUARD for 220DEVBLOG", "TTcux", "0.1.0")]
    public class SIMMONYGUARD : RustPlugin
    {
        [PluginReference] Plugin EnhancedBanSystem;
        
        [JsonProperty("Токен от группы ВК(От группы будут идти сообщения в беседу.Вам нужно добавить свою группу в беседу!)")]
        public string Token = "fb21e2a199a1c269a647fa0a25349c15f3b7af1576882e2eaad6c945adcaa35ac820f0c8d62e0cdc249cd";
        [JsonProperty("ID беседы для группы")]
        public string ChatID = "8";
        
        const string ipinnfourl = "https://ipinfo.io/{ip}/privacy?token={token}";
        const bool enablefull = true;
        const string ipinfosingup = "https://ipinfo.io/signup";

        #region CODE LOCK
        private static bool IsBanned(ulong userid)
        {
            return ServerUsers.Is(userid, ServerUsers.UserGroup.Banned);
        }

        private static bool IsImprisoned(ulong userid)
        {
            if (ins.PrisonBitch == null) return false;
            return ins.PrisonBitch.Call<bool>("ISIMPRISONED", userid);
        }

        private void OnCodeEntered(CodeLock codeLock, BasePlayer player, string code)
        {
            if (player == null) return;
            ulong owner = codeLock.OwnerID;
            if (owner == 0UL || code  != codeLock.code) return;
            bool bann = IsBanned(owner);
            bool unprisoned = IsImprisoned(owner);

            if(bann || unprisoned)
            {
                ADDLOG($"{player.displayName}({player.UserIDString}) ввёл пароль от кодового замка {(bann ? "забанненого" : "заключенного")} игрока({owner})!", 2);
                if (config.cODELOCK.enable)
                {
                    timer.Once(config.cODELOCK.seconds, () =>
                    {
                        //EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.cODELOCK.reason, config.cODELOCK.hours);
                        ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                        //BAN(player.UserIDString, config.cODELOCK.reason, config.cODELOCK.hours, player.displayName);
                    });
                }
            }

        }
        #endregion

        #region КОНФИГ
        private static PluginConfig config;

        protected override void LoadDefaultConfig()
        {
            config = PluginConfig.DefaultConfig();
        }
        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<PluginConfig>();
        }
        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        class SILENT
        {
            [JsonProperty("Автобан для опытных пользователей")]
            public int xdetects;

            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class SPIDER
        {
            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class FLY
        {
            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class HITMOD
        {
            [JsonProperty("Банить?")]
            public bool enable;

            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class CODELOCK
        {
            [JsonProperty("Банить?")]
            public bool enable;

            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Задержка в секундах перед баном, после детекта")]
            public float seconds;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class TEAMBAN
        {
            [JsonProperty("Банить?")]
            public bool enable;

            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Банить, если в команде N забаненных")]
            public int num;

            [JsonProperty("Учитывать бан за макрос?")]
            public bool macros;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class SMARTBAN
        {
            [JsonProperty("Банить?")]
            public bool enable;

            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Fly: очков")]
            public int fly;

            [JsonProperty("EspStash: очков")]
            public int espstash;

            [JsonProperty("Macro: очков")]
            public int macro;

            [JsonProperty("HitMod: очков")]
            public int hitmod;

            [JsonProperty("Spider: очков")]
            public int spider;

            [JsonProperty("Silent: очков")]
            public int silent;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        class ESPSTASH
        {
            [JsonProperty("Количество")]
            public int amount;

            [JsonProperty("Возможный лут")]
            public Dictionary<string, int> loots;
        }

        class MACROS
        {
            [JsonProperty("Причина бана")]
            public string reason;

            [JsonProperty("Количество выстрелов")]
            public int shoots;

            [JsonProperty("Количество детектов")]
            public int amount;

            [JsonProperty("Время бана в часах")]
            public int hours;
        }

        private class PluginConfig
        {
            [JsonProperty("SteamAPI")]
            public string steampi;

            [JsonProperty("Silent Aim: настройка")]
            public SILENT sILENT;

            [JsonProperty("Spider: настройка")]
            public SPIDER sPIDER;

            [JsonProperty("ESP SmallStash: настройка")]
            public ESPSTASH ESPStash;

            [JsonProperty("FLY: настройка")]
            public FLY fLY;

            [JsonProperty("TEAMBAN: настройка")]
            public TEAMBAN tEAMBAN;

            [JsonProperty("CODELOCK: настройка")]
            public CODELOCK cODELOCK;

            [JsonProperty("HITMOD: настройка")]
            public HITMOD hITMOD;

            [JsonProperty("MACROS: настройка")]
            public MACROS mACROS;

            [JsonProperty("Debug camera: Банить?")]
            public bool debugcamera;

            [JsonProperty("Отображать данные при подключении игрока?")]
            public bool show;

            [JsonProperty("Не банить Steam игроков?")]
            public bool steamplayer;

            [JsonProperty("SOON не трогать")]
            public bool prison;

            [JsonProperty("Банить не подтвержденные аккаунты?")]
            public bool bannensatroyen;

            [JsonProperty("Банить аккаунты, которые пытались обойти бан?")]
            public int banday;

            [JsonProperty("На сколько часов банить за попытку обхода?")]
            public int bannewaccountday;

            [JsonProperty("Кикать не подтвержденные аккаунты")]
            public bool kicknenastoyen;

            [JsonProperty("Кикать приватные аккаунты")]
            public bool kickprivate;

            [JsonProperty("Кикать игроков использующих спуфер IP?")]
            public bool kickvpn;

            [JsonProperty("Не кикать аккаунты с лицензии?")]
            public bool steamkick;

            [JsonProperty("Не банить лицушников за новые аккаунты?")]
            public bool steam;

            [JsonProperty("Писать/сохранять логи [0 - нет | 1 - только баны | 2 - все]")]
            public int logspriority;

            [JsonProperty("Discord: ID канала")]
            public string discordid;

            [JsonProperty(" Логи попаданий с огнестрела в консоль")]
            public bool logs;

            [JsonProperty("IPINFO ТОКЕН")]
            public string ipinfotoken;

            [JsonProperty("tt")]
            public string tt;

            [JsonProperty("Сообщения")]
            public Dictionary<string, string> messages;

            [JsonProperty("Шаблоны банов")]
            public Dictionary<string, string> pattern;

            public static PluginConfig DefaultConfig()
            {
                return new PluginConfig()
                {
                    ipinfotoken = ipinfosingup,
                    kickvpn = false,
                    tEAMBAN = new TEAMBAN
                    {
                        enable = true,
                        hours = 168,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> друг читера\"",
                        num = 2
                    },
                    cODELOCK = new CODELOCK
                    {
                        enable = true,
                        hours = 336,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> похоже у вас есть бан на сервере\"",
                        seconds = 75
                    },
                    sPIDER = new SPIDER
                    {
                        hours = 720,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> обнаружен паучок\""
                    },
                    sILENT = new SILENT
                    {
                        xdetects = 7,
                        hours = 1440,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> обнаружен SilentAim! \""
                    },
                    fLY = new FLY
                    {
                        hours = 720,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> обнаружен флайхак!!\""
                    },
                    hITMOD = new HITMOD
                    {
                        enable = true,
                        hours = 720,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> обнаружен чит!\""
                    },
                    mACROS = new MACROS
                    {
                        amount = 5,
                        hours = 168,
                        reason = "\"<color=#0000ff>[SIMMONY-GUARD]</color> обнаружены макросы!!\"",
                        shoots = 10
                    },
                    prison = true,
                    steamkick = true,
                    debugcamera = true,
                    logs = false,
                    steam = true,
                    steampi = defaultsteamapi,
                    banday = 5,
                    kicknenastoyen = true,
                    kickprivate = true,
                    tt = "nothing",
                    bannensatroyen = false,
                    show = true,
                    bannewaccountday = 120,
                    steamplayer = false,
                    messages = new Dictionary<string, string>
                    {
                        { "BAN.OBHOD", "\"<color=#0000ff>[SIMMONY-GUARD]</color> нам кажется, что вы пытаетесь обойти бан\"" },
                        { "KICK.PRIVATE", "\"<color=#0000ff>[SIMMONY-GUARD]</color> Подвердите аккаунт открыв профиль Steam\"" },
                        { "KICK.NENASTROYEN", "\"<color=#0000ff>[SIMMONY-GUARD]</color> вы уже близко настройте профиль Steam\""},
                        { "KICK.VPN", "\"<color=#0000ff>[SIMMONY-GUARD]</color> нам кажется, что вы пытаетесь зайти с измененным IP-adress\""}
                    },
                    pattern = new Dictionary<string, string>
                    {
                        { "BAN.ACCOUNT", "ban {steamid} {reason} {time}" },
                        { "PRISON.ACCOUNT", "prison.add {steamid} {time} {reason}" },
                    },
                    logspriority = 2,
                    ESPStash = new ESPSTASH
                    {
                        amount = 100,
                        loots = new Dictionary<string, int>
                        {
                            { "rifle.ak", 1 },
                            { "rifle.bolt", 1 },
                            { "rifle.l96", 1 },
                            { "rifle.lr300", 1 },
                            { "rifle.semiauto", 1 },
                            { "wood", 10000 },
                            { "stones", 10000 },
                            { "metal.refined", 50 },
                            { "metal.fragments", 10000 },
                            { "metal.facemask", 1 },
                            { "scrap", 500 },
                        }
                    },
                    discordid = ""
                };
            }
        }
        #endregion

        int nsnext = 0;
        [ChatCommand("ns")]
        private void COMMANSTASH(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            player.Teleport(stashContainers[nsnext].transform.position + Vector3.up * 1.5f);
            nsnext++;
            if (stashContainers.Count >= nsnext) nsnext = 0;
        }

        Vector3 lastshash;
        [ChatCommand("ls")]
        private void COMMALS(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            if(lastshash == Vector3.zero)
            {
                player.ChatMessage("<color=yellow>Еще не раскопали ни одного стеша!</color>");
                return;
            }
            player.Teleport(lastshash + Vector3.up * 1.5f);
        }

        [ChatCommand("testaim")]
        private void COMMANDERAIM(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;
            TESTAIM(player);
        }

        [ConsoleCommand("test.aim")]
        private void cmdtest(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin || !arg.HasArgs()) return;
            ulong ID;
            if(!ulong.TryParse(arg.Args[0], out ID))
            {
                arg.ReplyWith("ЭТО НЕ СТИМ ИД!");
                return;
            }
            BasePlayer player = BasePlayer.FindByID(ID);
            if(player == null)
            {
                arg.ReplyWith("ИГРОК НЕ НАЙДЕН!");
                return;
            }
            TESTAIM(player);
        }

      /* Dictionary<BasePlayer, DateTime> lastbowtime = new Dictionary<BasePlayer, DateTime>();
        void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (player == null || projectile == null || mod == null || projectiles == null)  return;
            Item item = player.GetActiveItem();
            if (item == null || item.info.itemid != 1443579727)  return;
            DateTime dateTime;
            if (!lastbowtime.TryGetValue(player, out dateTime))
            {
                lastbowtime[player] = DateTime.Now;
                return;
            }
            Debug.Log(player.displayName + " - " + (DateTime.Now- dateTime).TotalMilliseconds);
            lastbowtime[player] = DateTime.Now;
        }*/

        private void TESTAIM(BasePlayer player)
        {
            int i = 0;
            Vector3 head = player.eyes.MovementForward();
            Debug.Log($"{i}. {player.displayName} - {player.eyes.HeadForward()} - {player.eyes.MovementForward()}");
            Vector3 pos = new Vector3(player.transform.position.x + (10f * player.eyes.MovementForward().x), 500f, player.transform.position.z + (10f * head.z));
          //  Vector3 vector3 = player.eyes.
            BasePlayer npc = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", new Vector3(pos.x, TerrainMeta.HeightMap.GetHeight(pos) - 2f, pos.z), new Quaternion(), true) as BasePlayer;
            if (npc == null) return;
            npc.enableSaving = false;
            npc.Spawn();
            NextTick(() =>
            {
                PlayerInventory inv = npc.GetComponent<PlayerInventory>();
                inv.Strip();
            });

            timer.Repeat(0.1f, 20, () => {i++; Debug.Log($"{i}. {player.displayName} - {player.eyes.HeadForward()}"); });
            timer.Once(2.5f, () => { if (!npc.IsDead()) npc.Kill(); });
        }

        [ChatCommand("testfly")]
        private void COMMANDER(BasePlayer player, string command, string[] args)
        {
            if (player.IsAdmin)
            {
                List<BaseEntity> list = Pool.GetList<BaseEntity>();
                Vis.Entities<BaseEntity>(player.transform.position, 2f, list);
                List<TreeEntity> list2 = Pool.GetList<TreeEntity>();
                Vis.Entities<TreeEntity>(player.transform.position, 8.5f, list2);
                string elements = "";
                bool pl = false;
                foreach (var z in list)
                {
                    if (z is BasePlayer && (z as BasePlayer) != player) pl = true;
                    elements += z.ShortPrefabName + (list.Count > 1 ? " | " : "");
                }
                RaycastHit hit;
                var raycast = Physics.Raycast(player.transform.position, Vector3.down, out hit, 500f);
                if (raycast)
                {
                    bool spider = false;
                    RaycastHit hit2;
                    var raycast2 = Physics.Raycast(player.transform.position, player.eyes.BodyForward(), out hit2, 1f);
                    if (raycast2)
                    {
                        spider = hit2.collider.name.Contains("wall");
                        Debug.Log(hit2.collider.name);
                    }
                    bool ins = hit.collider.name.Contains("assets/prefabs/building core");
                    float distance = player.Distance(hit.point);
                    Debug.Log($"-[TEST]- {player.displayName}({player.UserIDString}) - [{elements}] - высота: {distance.ToString("F1")} м. ({hit.collider.name}) | Дерево: {(list2.Count > 0 ? "Да" : "Нет")} | Игрок: {(pl ? "Да" : "Нет")} | В здании: {(ins ? "Да" : "Нет")} | Спайдер: {(spider ? "Да" : "Нет")}");
                }
            }
        }

        [PluginReference] private Plugin PrisonBitch;

        private static void BAN(string steamid, string reason, int time, string displayname, bool checkteam = true)
        {
            if (!enablefull) return;

            ulong usteam = Convert.ToUInt64(steamid);
            if (config.steamplayer)
            {
                BasePlayer pl = BasePlayer.FindByID(usteam);
                if(pl != null)
                {
                    if (ins.ISSTEAM(pl.Connection))
                    {
                        ADDLOG($"{pl.displayName}({pl.UserIDString}) отмазали от бана.", 2);
                        return;
                    }
                }
            }

            ulong usteamid = Convert.ToUInt64(steamid);


            if (config.prison && prison)
            {
                if (!IsImprisoned(usteamid))
                {
                    ADDLOG($"Отправили в тюрьму {displayname}({steamid}) на {(time * 1f / 24f).ToString("F1")} дней [{reason}]", 1);
                    time = (int)(time * 60f);
                    ins.Server.Command(config.pattern["PRISON.ACCOUNT"].Replace("{steamid}", steamid).Replace("{time}", time.ToString()).Replace("{reason}", reason));
                }
            }
            else
            {
                if (!IsBanned(usteamid))
                {
                    ins.Server.Command(config.pattern["BAN.ACCOUNT"].Replace("{steamid}", steamid).Replace("{time}", time.ToString()).Replace("{reason}", reason));
                    ADDLOG($"Забанили {displayname}({steamid}) на {(time * 1f / 24f).ToString("F1")} дней [{reason}]", 1);
                }
            }
        }

        const int flylimit = 2;
        const int spiderlimit = 2;

        private static Dictionary<ulong, int> macros = new Dictionary<ulong, int>();

        class ANTICHEAT : MonoBehaviour
        {
            private int stash;
            private int silent;
            private int spider;
            private int fly;
            BasePlayer player;
            private DateTime lastban;

            private DateTime firsthit;
            private DateTime hitmod;
            string lasthit;
            int hits;
            float distancehit;
            string weaponhit;

            public DateTime LastFires;
            public int fires;
            string weaponfire;
            float posfire;
            float posfirel;
            float posfirer;

            private int numdetecthit;

            bool macromove;
            Vector3 startshoots;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                if (player == null || enablefull && (player.IsAdmin || ins.permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.allow")))
                {
                    Destroy(this);
                    return;
                }
          //      if (!enablefull) Debug.Log(player.displayName + " добавили в античит.");
                lasthit = "";
                silent = 0;
                weaponhit = "";
                weaponfire = "";
                if (config.debugcamera) InvokeRepeating(nameof(TICK), 0f, 10f);
                InvokeRepeating(nameof(SILENTCLEAR), 120f, 120f);
               // InvokeRepeating(nameof(WH), 0f, 1f);
            }

            private void SILENTCLEAR()
            {
                spider = 0;
                silent = 0;
                fly = 0;
            }

            public void ADDFLY()
            {
                fly++;
                ADDLOG($"-[Fly]- {player.displayName}({player.UserIDString}) детектов {fly}/{flylimit}", 2);
                if (fly >= flylimit && lastban < DateTime.Now)
                {
                    //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.fLY.reason, config.fLY.hours);
                    //BAN(player.UserIDString, config.fLY.reason, config.fLY.hours, player.displayName);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                    lastban = DateTime.Now.AddSeconds(10f);
                    fly = 0;
                    return;
                }
            }

            public void ADDFIRE(string weapon)
            {
                if (weapon == "ak47u.item")
                {
                    Vector3 vector3 = player.transform.position;
                    double sec = (DateTime.Now - LastFires).TotalSeconds;
                    Vector3 current = player.eyes.HeadForward();
                    if (fires == 0)
                    {
                        startshoots = vector3;
                        macromove = false;
                        sec = 0;
                        posfire = current.y;
                        posfirel = current.x;
                        posfirer = current.z;
                    }
                    else
                    {
                        if (!macromove && startshoots != vector3)
                        {
                            macromove = true;
                        }
                    }
                    float razn = Mathf.Abs(posfire - current.y);
                    float raznl = Mathf.Abs(posfirel - current.x);
                    float raznr = Mathf.Abs(posfirer - current.z);
                    
                    if (current.y < 0.9f && current.y > -0.9f && sec < 0.2f && razn <= 0.009f && raznl <= 0.009f && raznr <= 0.009f)
                    {
                        fires++;
                        LastFires = DateTime.Now;
                        weaponfire = weapon;
                        if (IsInvoking(nameof(FIREEND))) CancelInvoke(nameof(FIREEND));
                        Invoke(nameof(FIREEND), 0.21f);
                    }
                }
            }

            private void FIREEND()
            {
                if (fires >= config.mACROS.shoots)
                {
                    if (!macros.ContainsKey(player.userID)) macros.Add(player.userID, 1);
                    else macros[player.userID] += 1;

                    if(macros[player.userID] >= config.mACROS.amount)
                    {
                        //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.mACROS.reason, config.mACROS.hours);
                        ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                        //BAN(player.UserIDString, config.mACROS.reason, config.mACROS.hours, player.displayName);
                        lastban = DateTime.Now.AddSeconds(10f);
                    }

                    ADDLOG($"-[Macro]- {player.displayName}({player.UserIDString}) | выстрелов {fires} | использовал {weaponfire} | двигался: {(macromove ? "да" : "нет")} | детект #{macros[player.userID]}", 2);
                }
                fires = 0;
            }

            public void ADDHIT(string hitbone, string weapon, float distance)
            {
                if (hitbone == "N/A" || distance < 30f) return;
                if(hitbone != lasthit && lasthit != "")
                {
                    CLEARHIT();
                    return;
                }
                if (hits == 0)
                {
                    hitmod = DateTime.Now;
                    firsthit = DateTime.Now;
                }
                 hits++;

                //  if (!enablefull) Debug.Log(player.displayName + " - " + hits);
                lasthit = hitbone;
                distancehit += distance;
                if(hits >= 2) distancehit /= 2;
                if(!weaponhit.Contains(weapon)) weaponhit += (hits >= 2 ? ", " : string.Empty) + weapon;
                if (hits >= 5)
                {
                    ADDLOG($"-[HitMod]- {player.displayName}({player.UserIDString}) | {hitbone} | средняя дистанция {distancehit.ToString("F1")} | использовал {weaponhit} | ({(DateTime.Now-firsthit).TotalMinutes.ToString("F1")})", 2);
                    if (distancehit > 100 && (weaponhit.Contains("bow_hunting.entity") || weaponhit.Contains("crossbow.entity") || weaponhit.Contains("bow.compound") || weaponhit.Contains("pistol_eoka.entity"))
                        || distancehit > 65 && (weaponhit == "bow_hunting.entity" || weaponhit == "crossbow.entity" || weaponhit == "bow.compound")
                        || distancehit > 40 && weaponhit == "pistol_eoka.entity")
                    {
                        //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.hITMOD.reason, config.hITMOD.hours);
                        ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                        //BAN(player.UserIDString, config.hITMOD.reason, config.hITMOD.hours, player.displayName);
                        lastban = DateTime.Now.AddSeconds(10f);
                    }

                    if(!weapon.Contains("l96.entity"))
                    {
                        if ((DateTime.Now - hitmod).TotalMinutes < 10f)
                        {
                            numdetecthit++;
                            if (numdetecthit >= 3)
                            {
                                //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.hITMOD.reason, config.hITMOD.hours);
                                ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                               // BAN(player.UserIDString, config.hITMOD.reason, config.hITMOD.hours, player.displayName);
                                lastban = DateTime.Now.AddSeconds(10f);
                            }
                        }
                        else
                        {
                            numdetecthit = 0;
                        }
                    }

                    hitmod = DateTime.Now;
                    CLEARHIT();
                }
            }
            /*
            private void WH()
            {
                RaycastHit hitInfo;
                if (GamePhysics.Trace(new Ray(player.eyes.position, player.eyes.HeadForward()), 0.0f, out hitInfo, 300f, 1219701521, QueryTriggerInteraction.UseGlobal))
                {
                    //targetPos = hitInfo.point;
                    if ((bool)((UnityEngine.Object)hitInfo.collider))
                    {
                        BaseEntity entity = hitInfo.GetEntity();
                        if ((bool)((UnityEngine.Object)entity))
                        {
                            BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
                            if (baseCombatEntity is BasePlayer)
                            {
                                BasePlayer basePlayer = baseCombatEntity as BasePlayer;
                                RaycastHit hitInfo2;
                                if (GamePhysics.Trace(new Ray(player.eyes.position, player.eyes.HeadForward()), 0.0f, out hitInfo2, 300f, 1218652417, QueryTriggerInteraction.UseGlobal))
                                {
                                    BaseEntity entity2 = hitInfo2.GetEntity();
                                    if ((bool)((UnityEngine.Object)entity2))
                                    {
                                        BaseCombatEntity baseCombatEntity2 = entity as BaseCombatEntity;
                                        if (!(baseCombatEntity2 is BasePlayer))
                                        {
                                            ADDLOG($"[#] {player.displayName} -> {basePlayer.displayName} [{player.Distance(basePlayer)} м. | {(basePlayer.eyes.transform.position.y - hitInfo.point.y).ToString("F1")} м. | {baseCombatEntity2.ShortPrefabName}]", 2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            */
            private void CLEARHIT()
            {
                hits = 0;
                lasthit = "";
                weaponhit = "";
                distancehit = 0;
            }

            public void ADDSTASH()
            {
                stash++;
                if (stash >= 2 && lastban < DateTime.Now)
                {
                    //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, "\"CheatDetected (6)\"", config.fLY.hours);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                    //BAN(player.UserIDString, "\"CheatDetected (6)\"", config.fLY.hours, player.displayName);
                    lastban = DateTime.Now.AddSeconds(10f);
                    stash = 0;
                    return;
                }
            }

            public void ADDSPIDER()
            {
                spider++;
                ADDLOG($"-[Spider]- {player.displayName}({player.UserIDString}) детектов {spider}/{spiderlimit}", 2);
                if (spider >= spiderlimit && lastban < DateTime.Now)
                {
                    //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.sPIDER.reason, config.sPIDER.hours);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                    //BAN(player.UserIDString, config.sPIDER.reason, config.sPIDER.hours, player.displayName);
                    lastban = DateTime.Now.AddSeconds(10f);
                    spider = 0;
                    return;
                }
            }

            public void ADDSILENT(int amount)
            {
                silent += amount;
                ADDLOG($"-[SAim]- {player.displayName}({player.UserIDString}) детектов {silent}/{config.sILENT.xdetects}", 2);
                if (silent >= config.sILENT.xdetects && lastban < DateTime.Now)
                {
                    //ins.EnhancedBanSystem.Call("BanPlayer", player, player.userID, config.sILENT.reason, config.sILENT.hours);
                    ConsoleSystem.Run(ConsoleSystem.Option.Server, $"player.ban {player.userID}");
                    //BAN(player.UserIDString, config.sILENT.reason, config.sILENT.hours, player.displayName);
                    lastban = DateTime.Now.AddSeconds(10f);
                    silent = 0;
                    return;
                }
            }

            private void TICK()
            {
                /*    if (player.IsFlying)
                    {
                        if (!IsInvoking(nameof(FLYTICK)))
                        {
                            height = player.transform.position.y;
                            InvokeRepeating(nameof(FLYTICK), 1f, 1f);
                        }
                    }*/

                if (enablefull)
                {
                    player.SendConsoleCommand("noclip");
                    player.SendConsoleCommand("camspeed 0");
                }

               

                /* if (player.modelState.flying && !player.IsAdmin && !player.IsDeveloper)
                {
                    BAN(player.UserIDString, "\"Cheat Detected! (4)\"", 60, player.displayName);
                    lastban = DateTime.Now.AddSeconds(10f);
                }*/
            }

         /*   private float height;
            private void FLYTICK()
            {
                if (!player.IsFlying)
                {
                    fly = 0;
                    CancelInvoke(nameof(FLYTICK));
                }

                fly++;
                if (fly >= 5)
                {
                    ADDLOG($"[Fly] {player.displayName}({player.UserIDString}) возможно флай.", 2);
                    fly = 0;
                    CancelInvoke(nameof(FLYTICK));
                }
            }*/

            public void DoDestroy() => Destroy(this);

            private void OnDestroy()
            {
                if (IsInvoking(nameof(TICK))) CancelInvoke(nameof(TICK));
               // if (IsInvoking(nameof(WH))) CancelInvoke(nameof(WH));
                if (IsInvoking(nameof(FIREEND))) CancelInvoke(nameof(FIREEND));
                //  if (IsInvoking(nameof(FLYTICK))) CancelInvoke(nameof(FLYTICK));
            }
        }

      /*  private void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
        {
            if (player == null || projectile == null || mod == null || projectiles == null) return;
            RaycastHit hitInfo;
            if (GamePhysics.Trace(new Ray(player.eyes.position, player.eyes.HeadForward()), 0.0f, out hitInfo, 300f, 1219701521, QueryTriggerInteraction.UseGlobal))
            {
                //targetPos = hitInfo.point;
                if ((bool)((UnityEngine.Object)hitInfo.collider))
                {
                    BaseEntity entity = hitInfo.GetEntity();
                    if ((bool)((UnityEngine.Object)entity))
                    {
                        BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
                        if (baseCombatEntity is BasePlayer)
                        {
                            BasePlayer basePlayer = baseCombatEntity as BasePlayer;
                          //  if(basePlayer)
                            Debug.Log($"{player.displayName} -> {basePlayer.displayName} [{player.Distance(basePlayer)} м. | {(basePlayer.eyes.transform.position.y - hitInfo.point.y).ToString("F1")} м.]");
                        }
                    }
                }
            }
        }*/

        private void OnPlayerBanned(string name, ulong id, string address, string reason)
        {
            if (reason == "Cheat Detected!") ADDLOG($"Забанили {name}({id}) [FakeAdmin/DebugCamera]", 1);
        }

        const string defaultsteamapi = "https://steamcommunity.com/dev/apikey";
        class resp
        {
            public avatar response;
        }

        class avatar
        {
            public List<Players> players;
        }

        class Players
        {
            public int? profilestate;
            public int? timecreated;
        }

        class INFO
        {
            public DateTime dateTime;
            public bool profilestate;
            public bool steam;
            public Dictionary<string, Dictionary<string, int>> hitinfo;
        }

        Dictionary<ulong, INFO> PLAYERINFO = new Dictionary<ulong, INFO>();

        static SIMMONYGUARD ins;
        private void Init()
        {
            ins = this;
            //   Unsubscribe(nameof(OnPlayerConnected));
        }

        #region Grid
        Dictionary<string, Vector3> Grids = new Dictionary<string, Vector3>();
        const float calgon = 0.0066666666666667f;
        void CreateSpawnGrid()
        {
            var worldSize = (ConVar.Server.worldsize);
            float offset = worldSize / 2;
            var gridWidth = (calgon * worldSize);
            float step = worldSize / gridWidth;

            string start = "";

            char letter = 'A';
            int number = 0;

            for (float zz = offset; zz > -offset; zz -= step)
            {
                for (float xx = -offset; xx < offset; xx += step)
                {
                    Grids.Add($"{start}{letter}{number}", new Vector3(xx - 55f, 0, zz));
                    if (letter.ToString().ToUpper() == "Z")
                    {
                        start = "A";
                        letter = 'A';
                    }
                    else
                    {
                        letter = (char)(((int)letter) + 1);
                    }


                }
                number++;
                start = "";
                letter = 'A';
            }
        }

        private string GetNameGrid(Vector3 pos)
        {
            return Grids.Where(x => x.Value.x < pos.x && x.Value.x + 150f > pos.x && x.Value.z > pos.z && x.Value.z - 150f < pos.z).FirstOrDefault().Key;
        }
        #endregion

        private static bool prison = false;
        private List<StashContainer> stashContainers = new List<StashContainer>();
        private float sizeworldx;
        private float sizeworldz;

        List<Vector3> OntheMap = new List<Vector3>();
        void foundmonuments()
        {
            OntheMap.Clear();
            foreach (var z in UnityEngine.Object.FindObjectsOfType<MonumentInfo>())
            {
                if (z.name.Contains("/cave") || z.name.Contains("/tiny") || z.name.Contains("/power substations") || z.name.Contains("OilrigAI")) continue;
                Vector3 pos = z.transform.position;
                if (!OntheMap.Contains(pos)) OntheMap.Add(pos);
            }
        }

        private Vector3 RANDOMPOS() => new Vector3(Random.Range(-sizeworldx, sizeworldx), 400f, Random.Range(-sizeworldz, sizeworldz));

        List<string> names = new List<string>();

        private Vector3 FINDSPAWNPOINT(int num = 1)
        {
            if (num >= 300) return Vector3.zero;
            Vector3 pos = RANDOMPOS();

            RaycastHit hitInfo;
            if (!Physics.Raycast(pos, Vector3.down, out hitInfo, 450f, Layers.Solid)) return FINDSPAWNPOINT(num++);
            if (hitInfo.collider == null || hitInfo.collider.name != "Terrain" && hitInfo.collider.name != "Road Mesh") return FINDSPAWNPOINT(num++);
            if (hitInfo.point.y - TerrainMeta.WaterMap.GetHeight(hitInfo.point) < 0) return FINDSPAWNPOINT(num++);
            if (WaterLevel.Test(hitInfo.point)) return FINDSPAWNPOINT(num++);
            if (OntheMap.Any(x => Vector3.Distance(x, hitInfo.point) < 170f)) return FINDSPAWNPOINT(num++);
            if (stashContainers.Any(x => Vector3.Distance(x.transform.position, hitInfo.point) < 30f)) return FINDSPAWNPOINT(num++);
            if (Mathf.Abs((TerrainMeta.HeightMap.GetHeight(hitInfo.point) - hitInfo.point.y)) > 0.1f) return FINDSPAWNPOINT(num++);
            if (Mathf.Abs((TerrainMeta.HeightMap.GetHeight(hitInfo.point + Vector3.left * 0.2f) - hitInfo.point.y)) > 0.01f) return FINDSPAWNPOINT(num++);
            if (Mathf.Abs((TerrainMeta.HeightMap.GetHeight(hitInfo.point + Vector3.right * 0.2f) - hitInfo.point.y)) > 0.01f) return FINDSPAWNPOINT(num++);
            if (Mathf.Abs((TerrainMeta.HeightMap.GetHeight(hitInfo.point + Vector3.forward * 0.2f) - hitInfo.point.y)) > 0.01f) return FINDSPAWNPOINT(num++);
            if (Mathf.Abs((TerrainMeta.HeightMap.GetHeight(hitInfo.point + Vector3.back * 0.2f) - hitInfo.point.y)) > 0.01f) return FINDSPAWNPOINT(num++);
            return hitInfo.point;
        }

        private void CanSeeStash(BasePlayer player, StashContainer stash)
        {
            if (stash.OwnerID != 0 || !stashContainers.Contains(stash)) return;
            ADDLOG($"-[ESPStash]- {player.displayName}({player.UserIDString}) - квадрат {GetNameGrid(stash.transform.position)}", 2);
            timer.Once(75f, () => 
            {
                if (!player.IsConnected) return;
            });
            lastshash = stash.transform.position;
            stashContainers.Remove(stash);
        }

        private void OnServerInitialized()
        {
            SaveConfig();
            if(config.tt == null || config.tt != "nothing")
            {
                config.tt = "nothing";
                config.hITMOD.enable = true;
                SaveConfig();
            }

            if (config.tEAMBAN == null)
            {
                config.tEAMBAN = new TEAMBAN
                {
                    hours = 168,
                    reason = "\"bb with teammates\"",
                    num = 2
                };
                config.cODELOCK = new CODELOCK
                {
                    hours = 336,
                    reason = "\"Ban Detected! (1)\"",
                    seconds = 75
                };
                SaveConfig();
            }

            if(config.tEAMBAN.num == 0)
            {
                config.tEAMBAN.num = 2;
                SaveConfig();
            }

            if (config.discordid == null)
            {
                config.discordid = "";
                SaveConfig();
            }
            CreateSpawnGrid();
            if(config.ipinfotoken == null)
            {
                config.ipinfotoken = ipinfosingup;
                if (!config.messages.ContainsKey("KICK.SPUFER")) config.messages.Add("KICK.SPUFER", "\"На сервере запрещено играть с VPN! (VPN DETECTED)\"");
                config.kickvpn = true;
                SaveConfig();
            }

            if(config.mACROS == null)
            {
                config.hITMOD = new HITMOD
                {
                    hours = 720,
                    reason = "\"Cheat Detected! (6)\""
                };
                config.mACROS = new MACROS
                {
                    amount = 5,
                    hours = 168,
                    shoots = 10,
                    reason = "\"Cheat Detected! (7)\""
                };
                SaveConfig();
            }

            if(config.mACROS.shoots == 0)
            {
                config.mACROS.shoots = 10;
                SaveConfig();
            }

            if (config.ESPStash == null) // патч 04.06 
            {
                config.ESPStash = new ESPSTASH
                {
                    amount = 100,
                    loots = new Dictionary<string, int>
                    {
                        { "rifle.ak", 1 },
                        { "rifle.bolt", 1 },
                        { "rifle.l96", 1 },
                        { "rifle.lr300", 1 },
                        { "rifle.semiauto", 1 },
                        { "wood", 10000 },
                        { "stones", 10000 },
                        { "metal.refined", 50 },
                        { "metal.fragments", 10000 },
                        { "metal.facemask", 1 },
                        { "scrap", 500 },
                    }
                };
                config.steam = true;
                SaveConfig();
            }

            if(config.ipinfotoken == ipinfosingup)
            {
                Debug.LogWarning("Введите в конфиг токен для IPINFO, если хотите включить автоопределение использования игроком VPN!");
            }
            foundmonuments();

            sizeworldx = TerrainMeta.Size.x / 2.5f;
            sizeworldz = TerrainMeta.Size.z / 2.5f;

            namefile = DateTime.Now.ToString("MM/dd");
            LOGS = Interface.Oxide.DataFileSystem.ReadObject<List<string>>("XAC/" + namefile);
            if (config.sPIDER == null)
            {
                config.sPIDER = new SPIDER
                {
                    hours = 720,
                    reason = "\"Cheat Detected! (2)\""
                };
                SaveConfig();
            }
            if (config.sILENT == null)
            {
                config.sILENT = new SILENT
                {
                    xdetects = 7,
                    hours = 1440,
                    reason = "\"Cheat Detected! (1)\""
                };
                config.pattern = new Dictionary<string, string>
                {
                    { "BAN.ACCOUNT", "ban {steamid} {reason} {time}" },
                    { "PRISON.ACCOUNT", "prison.add {steamid} {time} {reason}" },
                };
                SaveConfig();
            }
            if(config.fLY == null)
            {
                config.fLY = new FLY
                {
                    hours = 720,
                    reason = "\"Cheat Detected! (3)\""
                };
                SaveConfig();
            }
            if (config.sILENT.xdetects == 0)
            {
                config.sILENT.xdetects = 7;
                config.debugcamera = true;
                SaveConfig();
            }

            if (!config.messages.ContainsKey("KICK.NENASTROYEN"))
            {
                config.messages.Add("KICK.NENASTROYEN", "Настройте профиль, что бы играть на этом сервере! (Make your Steam profile public to play on this server)");
                SaveConfig();
            }

            if (string.IsNullOrEmpty(config.steampi) || config.steampi == defaultsteamapi)
            {
                Debug.LogError("УКАЖИТЕ STEAMAPI В КОНФИГЕ!");
                return;
            }

            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
             /*   HeldEntity heldEntity = player.GetHeldEntity();
                if (heldEntity != null)
                {
                    BaseProjectile baseProjectile = heldEntity.GetComponent<BaseProjectile>();
                    if (baseProjectile == null) continue;
                    baseProjectile.recoil.recoilYawMin = -2f;
                    baseProjectile.recoil.recoilPitchMin = -4f;
                    baseProjectile.recoil.recoilYawMax = 8f;
                    baseProjectile.recoil.recoilPitchMax = -30f;
                    baseProjectile.recoil.ADSScale = 0.5f;
                    baseProjectile.recoil.movementPenalty = 0.5f;

                    Debug.Log(player.displayName + " " + baseProjectile.recoil.recoilYawMin + " " + heldEntity.GetComponent<BaseProjectile>().recoil.recoilPitchMin + " " + heldEntity.GetComponent<BaseProjectile>().recoil.recoilYawMax + " " + heldEntity.GetComponent<BaseProjectile>().recoil.recoilPitchMax + " " + heldEntity.GetComponent<BaseProjectile>().recoil.ADSScale + " " + heldEntity.GetComponent<BaseProjectile>().recoil.movementPenalty);
                }*/
                if (player.GetComponent<ANTICHEAT>() == null) player.gameObject.AddComponent<ANTICHEAT>();
            }
            timer.Once(5f, () => { if (PrisonBitch != null) prison = true; });
            permission.RegisterPermission("SIMMONYGUARD.allow", this);
            permission.RegisterPermission("SIMMONYGUARD.skip", this);
            permission.RegisterPermission("SIMMONYGUARD.command", this);
            permission.RegisterPermission("SIMMONYGUARD.chat", this);
            // Subscribe(nameof(OnPlayerConnected));
            timer.Every(3600f, () => Save());

            stashContainers.Clear();

            int i = 0;
            while(i < config.ESPStash.amount)
            {
                Vector3 pos = FINDSPAWNPOINT();
                if (pos == Vector3.zero) continue;
                StashContainer stashContainer = GameManager.server.CreateEntity("assets/prefabs/deployable/small stash/small_stash_deployed.prefab", pos, new Quaternion(), true) as StashContainer;
                stashContainer.enableSaving = false;
                stashContainer.Spawn();
                int max = Random.Range(2, 7);
                int current = 0;
                foreach (var z in config.ESPStash.loots)
                {
                    if (Random.Range(0f, 1f) >= 0.65f)
                    {
                        if (current < max)
                        {
                            Item item = ItemManager.CreateByName(z.Key, Random.Range(1, z.Value));
                            if (item != null)
                            {
                                if (item.hasCondition)
                                {
                                    item.LoseCondition(Random.Range(0f, 100f));
                                    BaseProjectile weapon = item.GetHeldEntity() as BaseProjectile;
                                    if (weapon != null)
                                    {
                                        if (weapon.primaryMagazine != null)
                                        {
                                            weapon.primaryMagazine.contents = Random.Range(1, weapon.primaryMagazine.capacity + 1);
                                        }
                                    }
                                }
                                if (!item.MoveToContainer(stashContainer.inventory, Random.Range(0, 6), false)) item.MoveToContainer(stashContainer.inventory);
                                current++;
                            }
                        }
                    }
                }
                stashContainer.SetHidden(true);
                stashContainers.Add(stashContainer);
                i++;
            }
            Debug.Log($"Создали {stashContainers.Count} стешей-ловушек");

            moders.Clear();
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (ins.permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.chat") && !moders.Contains(player)) moders.Add(player);
            }
        }

        private readonly int constructionColl = LayerMask.GetMask(new string[] { "Construction", "Deployable", "Prevent Building", "Deployed" });
        private readonly int buildingLayer = LayerMask.GetMask("Terrain", "World", "Construction", "Deployed");
        //   private static Dictionary<ulong, int> FLYHACK = new Dictionary<ulong, int>();
        const string sspiral = "block.stair.spiral";
        const string sroof = "roof";
        const string sfly = "supply_drop";
        const string prefroof = "roof";
        const string prefspiral = "stairs.spiral";
        const string iceberg = "iceberg";
        private void OnPlayerViolation(BasePlayer player, AntiHackType type, float amount)
        {
            if (type == AntiHackType.FlyHack && !IsBattles(player.userID))
            {
                List<BaseEntity> list = Pool.GetList<BaseEntity>();
                Vis.Entities<BaseEntity>(player.transform.position, 2f, list);
                List<TreeEntity> list2 = Pool.GetList<TreeEntity>();
                Vis.Entities<TreeEntity>(player.transform.position, 6f, list2);
                string elements = "";
                bool pl = false;
                bool more1 = list.Count > 1;
                foreach (var z in list)
                {
                    if (z is BasePlayer && (z as BasePlayer) != player) pl = true;
                    elements += z.ShortPrefabName + (more1 ? " | " : "");
                }
                RaycastHit hit;
                var raycast = Physics.Raycast(player.transform.position, Vector3.down, out hit, 500f);
                if (raycast)
                {
                    bool spider = false;
                    bool drop = hit.collider.name.Contains(sfly);
                    bool spiral = hit.collider.name.Contains(prefspiral); 
                    bool roof = hit.collider.name.Contains(prefroof);
                    bool ice = hit.collider.name.Contains(iceberg);

                    RaycastHit hit2;
                    var raycast2 = Physics.Raycast(player.transform.position, player.eyes.BodyForward(), out hit2, 1f);
                    if (raycast2)
                    {
                        spider = hit2.collider.name.Contains("wall");
                    }
                    if(!spiral)
                    {
                        spiral = elements.Contains(sspiral);
                    }
                    if (!drop)
                    {
                        drop = elements.Contains(sfly);
                    }
                    if (!roof) roof = elements.Contains(sroof);


                    bool tree = list2.Count > 0;
                    bool ins = hit.collider.name.Contains("assets/prefabs/building core");
                    float distance = player.Distance(hit.point);
                    ADDLOG($"-[Fly]- {player.displayName}({player.UserIDString}) - [{elements}] - высота: {distance.ToString("F1")} м. ({hit.collider.name}) | Дерево: {(tree ? "Да" : "Нет")} | Игрок: {(pl ? "Да" : "Нет")} | В здании: {(ins ? "Да" : "Нет")} | Спайдер: {(spider ? "Да" : "Нет")} | Спиральная лестница: {(spiral ? "Да" : "Нет")} | Крыша: {(roof ? "Да" : "Нет")} | Аир: {(drop ? "Да" : "Нет")}", 1);
                    if (roof || drop || spiral || ice || tree || pl || distance < 3f || distance > 7f) return;
                }
            }
            /*
            if (elements.Contains("wall"))
            {
                ANTICHEAT aNTICHEAT;
                if (!player.GetComponent<ANTICHEAT>(out aNTICHEAT)) return;
                RaycastHit hit;
                var raycast = Physics.Raycast(player.transform.position, Vector3.down, out hit, 500f, buildingLayer);
                if (raycast)
                {
                    float distance = player.Distance(hit.point);
                    ADDLOG($"-[Spider]- {player.displayName}({player.UserIDString}) - [{elements}] - высота: {distance.ToString("F1")} м.", 2);
                    if (distance >= 3f && distance <= 7f) aNTICHEAT.ADDSPIDER();
                }
            }
            else if(elements.Count() == 0)
            {
                if (player.IsAdmin || ins.permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.allow")) return;
                RaycastHit hit;
                var raycast = Physics.Raycast(player.transform.position, Vector3.down, out hit, 500f, buildingLayer);
                if (raycast)
                {
                    ANTICHEAT aNTICHEAT;
                    if (!player.GetComponent<ANTICHEAT>(out aNTICHEAT)) return;
                    float distance = player.Distance(hit.point);
                    ADDLOG($"-[Fly]- {player.displayName}({player.UserIDString}) - высота: {distance.ToString("F1")} м.", 2);
                    if(distance >= 3f && distance <= 7f) aNTICHEAT.ADDFLY();
                }
            }*/
        }
        

        private static string namefile;
        private static List<string> LOGS = new List<string>();
        private static List<BasePlayer> moders = new List<BasePlayer>();
        private static void ADDLOG(string text, int priority)
        {
            if (config.logspriority >= priority)
            {
                Debug.LogWarning(text);
                if (!string.IsNullOrEmpty(config.discordid))
                {
                    if (ins.DiscordCore != null) ins.DiscordCore.Call("SendMessageToChannel", config.discordid, text);
                    if (ins.HaxBot != null) ins.HaxBot.Call("MESSAGE", text, 14177041, config.discordid);
                }

                foreach(var z in moders)
                {
                    z.ChatMessage(text); 
                }

                ins.VKSendMessage(text);
                LOGS.Add($"[{DateTime.Now.ToShortTimeString()}] " + text);
            }
        }

        void VKSendMessage(string Message)
        {
            if (String.IsNullOrEmpty(ChatID) || String.IsNullOrEmpty(Token))
            {
                PrintWarning("Вы не настроили конфигурацию,в пункте с ВК");
                return;
            }
            int RandomID = UnityEngine.Random.Range(0, 9999);
            while (Message.Contains("#"))
                Message = Message.Replace("#", "%23");
            webrequest.EnqueueGet($"https://api.vk.com/method/messages.send?chat_id={ChatID}&random_id={RandomID}&message={Message}&access_token={Token}&v=5.92", (code, response) => { }, this);
        }

        private void Save()
        {
            Interface.Oxide.DataFileSystem.WriteObject($"XAC/{namefile}", LOGS);
            Debug.Log("[SIMMONYGUARD] Сохранили логи в файлик.");
        }

        [ConsoleCommand("ac.logs")]
        private void cmdlastlogs(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (arg.IsAdmin || player != null && permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.command"))
            {
                if (LOGS.Count > 0)
                {
                    int number;
                    if (!arg.HasArgs() || !int.TryParse(arg.Args[0], out number)) number = 10;
                    int skip = LOGS.Count - number;
                    if (skip < 0) skip = 0;
                    string text = string.Join("\n", LOGS.Skip(skip).Take(number).ToArray());
                    arg.ReplyWith("XAC - Последние логи:\n" + text + "\n------------------");
                }
                else
                {
                    arg.ReplyWith("XAC - В логах пусто :(");
                }
            }
        }

        [ConsoleCommand("ac.save")]
        private void cmdsavecommand(ConsoleSystem.Arg arg)
        {
            if (!arg.IsAdmin) return;
            Save();
        }

        private void Unload()
        {
            foreach(var z in stashContainers)
            {
                if (!z.IsDestroyed) z.Kill();
            }

            Save();

            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                
            }
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (!player.IsConnected) return;
            if (player.IsReceivingSnapshot)
            {
                timer.Once(1f, () => OnPlayerInit(player));
                return;
            }
            if (player.GetComponent<ANTICHEAT>() == null) player.gameObject.AddComponent<ANTICHEAT>();
            if (ins.permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.chat") && !moders.Contains(player)) moders.Add(player);
            
            timer.Once(1f, () => GETINFO(player));
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (moders.Contains(player)) moders.Remove(player);
        }

        private object OnPlayerAttack(BasePlayer player, HitInfo info)
        {
            if (player == null || info.HitEntity == null || !(info.HitEntity is BasePlayer) || IsBattles(player.userID)) return null;
/*
            float pos = player.eyes.HeadForward().y;
            NextTick(() =>
            {
                float current = player.eyes.HeadForward().y;
                if (pos < -0.9f || current < -0.9f)
                {
                    ADDLOG($"[SPINNER] {player.displayName}({player.UserIDString}) - [S{pos}][C{current}]", 2);
                }
            });
            */
            float distnace = info.HitEntity.Distance(player);
            if (distnace < 15f) return null;
            float y = Mathf.Abs(info.HitPositionWorld.y - info.HitEntity.CenterPoint().y);
            if (y >= 2.05f)
            {
                string weapon = info.WeaponPrefab.ShortPrefabName ?? "x";
                ADDLOG($"-[SAim]- {player.displayName}({player.UserIDString}) - {y.ToString("F1")} м.- [{weapon} | {info.boneName ?? "x"} | {distnace.ToString("F1")} м.]", 2);
                if (info.boneName != "head" && info.boneName != "neck") return null;
                int amount = 4;
                if (weapon == "crossbow.entity" || weapon == "bow_hunting.entity" || weapon == "bow_compound.entity" || weapon == "pistol_eoka.entity") amount = 7;
                return true;
            }
            return null;
        }

        private void OnEntityTakeDamage(object entity, HitInfo info)
        {
            if (info == null || info.Weapon == null || info.InitiatorPlayer == null || info.damageTypes.IsMeleeType()) return;
            if (IsNPC(info.InitiatorPlayer) || !(entity is BasePlayer)) return;

            BasePlayer player = entity as BasePlayer;
            if (player == null || IsNPC(player) || !player.IsConnected || player.IsSleeping() || info.InitiatorPlayer == player) return;

          //  info.damageTypes.ScaleAll(0f);

            string weapon = info.WeaponPrefab != null ? info.WeaponPrefab.ShortPrefabName : "x";
            string bone = !string.IsNullOrEmpty(info.boneName) ? info.boneName : "x";
            float distance = info.InitiatorPlayer.Distance(player);
            //{(info.InitiatorPlayer.IsFlying? " | в полёте" : "")}{(!info.InitiatorPlayer.IsAiming ? " | от бедра" : "")}
            if (config.logs) Debug.Log($"-- {info.InitiatorPlayer.displayName}({info.InitiatorPlayer.UserIDString}) [{weapon} | {bone} | {distance.ToString("F1")} м.] => {player.displayName}({player.UserIDString})");
        }

        private bool IsNPC(BasePlayer player)
        {
            if (player is NPCPlayer) return true;
            if (!(player.userID >= 76560000000000000L || player.userID <= 0L)) return true;
            return false;
        }

        [PluginReference] Plugin MultiFighting, Battles, HaxBot, DiscordCore;

        private bool IsBattles(ulong userid)
        {
            return Battles != null && Battles.Call<bool>("IsPlayerOnBattle", userid);
        }

        private bool ISSTEAM(Network.Connection connection)
        {
            if (MultiFighting == null) return true;
            return MultiFighting.Call<bool>("IsSteam", connection);
        }

        private void GETINFO(BasePlayer player) // пиздим инфу со стима
        {
            if (!player.IsConnected) return;
            webrequest.Enqueue($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={config.steampi}&steamids={player.UserIDString}&format=json", null, (code, response) =>
            {
                if (response != null && code == 200)
                {
                    if (!player.IsConnected) return;
                    string steamid = player.UserIDString;
                    string text = $"------------\n{player.displayName} ({steamid})";
                    bool act = false;
                    INFO iNFO = new INFO();
                    resp sr = JsonConvert.DeserializeObject<resp>(response);
                    int datetime = sr.response.players[0].timecreated ?? 0;
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime create = epoch.AddSeconds(datetime).AddHours(3);
                    bool steam = ISSTEAM(player.Connection);
                    text += $"\nВерсия игры: {(steam ? "Лицензия" : "Пиратка")}";
                    int nastr = sr.response.players[0].profilestate ?? 0;
                    bool ns = ISNASTROEN(nastr);
                    text += $"\nАккаунт настроен: {(ns ? "Да" : "Нет")}";
                    if (!ns && config.kicknenastoyen)
                    {
                        if (!permission.UserHasPermission(steamid, "SIMMONYGUARD.allow") && !permission.UserHasPermission(steamid, "SIMMONYGUARD.skip"))
                        {
                            Server.Command($"kick {steamid} {config.messages["KICK.NENASTROYEN"]}");
                            act = true;
                        }
                    }
                    if (datetime > 0)
                    {
                        text += $"\nАккаунт создан: {create.ToShortDateString()}";
                    }
                    else
                    {
                        text += "\nПрофиль закрытый: Да";
                        if (!steam || !config.steamkick)
                        {
                            if (config.kickprivate && !permission.UserHasPermission(steamid, "SIMMONYGUARD.allow") && !permission.UserHasPermission(steamid, "SIMMONYGUARD.skip"))
                            {
                                Server.Command($"kick {steamid} {config.messages["KICK.PRIVATE"]}");
                                act = true;
                            }
                        }
                    }

                    if (config.show) Debug.Log(text + "\n------------");

                    if (!permission.UserHasPermission(steamid, "SIMMONYGUARD.allow") && !permission.UserHasPermission(steamid, "SIMMONYGUARD.skip") && (config.bannensatroyen && nastr != 1 || create.AddDays(config.banday) > DateTime.Now))
                    {
                        if (act || steam && config.steam) return;
                        Server.Command(config.pattern["BAN.ACCOUNT"].Replace("{steamid}", steamid).Replace("{reason}", config.messages["BAN.OBHOD"]).Replace("{time}", config.bannewaccountday.ToString()));
                        return;
                    }
                }
            }, this);


            //VPN
            if (config.ipinfotoken == ipinfosingup) return;
            string[] ip = player.IPlayer.Address.Split(':');
            webrequest.Enqueue(ipinnfourl.Replace("{token}", config.ipinfotoken).Replace("{ip}", ip[0]), null, (code, response) =>
            {
                if (response != null && code == 200)
                {
                    if (!player.IsConnected) return;
                    VPNINFO sr = JsonConvert.DeserializeObject<VPNINFO>(response);
                    bool VPN = sr.vpn;
                    Debug.Log($"[{player.displayName}({player.UserIDString}) | IP: {ip[0]} | VPN: {(VPN ? "Да" : "Нет")}]");
                    if (!VPN) return;
                    if (config.kickvpn && !permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.allow") && !permission.UserHasPermission(player.UserIDString, "SIMMONYGUARD.skip"))
                    {
                        Server.Command($"kick {player.UserIDString} {config.messages["KICK.SPUFER"]}");
                    }
                }
            }, this);
        }

        class VPNINFO
        {
            public bool vpn;
            public bool proxy;
            public bool tor;
            public bool hosting;
        }

        private bool ISNASTROEN(int num)
        {
            if (num == 1) return true;
            return false;
        }

      /*  #region FakeWorkbench
        private object CanCraft(ItemCrafter itemCrafter, ItemBlueprint bp, int amount)
        {
            BasePlayer player = itemCrafter.GetComponent<BasePlayer>();
            if (player == null) return null;
            Debug.Log(player.currentCraftLevel + "/" + bp.workbenchLevelRequired);
            return null;
        }
        #endregion*/
    }
}
