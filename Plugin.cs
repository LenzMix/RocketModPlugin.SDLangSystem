using Rocket.API.Collections;
using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Unturned.Player;
using MySql.Data.MySqlClient;
using Rocket.Unturned.Chat;
using UnityEngine;
using Rocket.Unturned;
using Rocket.Core;
using HarmonyLib;
using Steamworks;

namespace SDLangSystem
{
    public class DatabaseManager
    {
        private readonly Plugin _plugin;

        public DatabaseManager(Plugin plugin)
        {
            this._plugin = plugin;
            if (_plugin.Configuration.Instance.isDB)
                this.SQLChecker();
        }

        private MySqlConnection createConnection()
        {
            return new MySqlConnection(string.Concat(new string[]
            {
                "SERVER=",
                this._plugin.Configuration.Instance.mysqlip,
                ";DATABASE=",
                this._plugin.Configuration.Instance.mysqldb,
                ";UID=",
                this._plugin.Configuration.Instance.mysqlusr,
                ";PASSWORD=",
                this._plugin.Configuration.Instance.mysqlpass,
                ";PORT=",
                this._plugin.Configuration.Instance.mysqlport,
                ";"
            }));
        }

        public void GetInfo(UnturnedPlayer player)
        {
            if (Plugin.Instance.Configuration.Instance.isDB)
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[]
                {
                    "SELECT * FROM ",
                    Plugin.Instance.Configuration.Instance.mysqltable,
                    " WHERE Steam64ID = '",
                    player.CSteamID.m_SteamID.ToString(),
                    "';"
                });
                mySqlConnection.Open();
                MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
                bool hasRows = mySqlDataReader.HasRows;
                if (hasRows)
                {
                    mySqlDataReader.Read();
                    Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                    component.lang = mySqlDataReader.GetString("Language");
                    mySqlDataReader.Close();
                    mySqlDataReader.Dispose();
                    if (!Plugin.Instance.Configuration.Instance.Langs.Exists(x => x.id == component.lang))
                    {
                        component.lang = Plugin.Instance.Configuration.Instance.startlang;
                        Plugin.SDLL.MultiSay(player, Plugin.plugin, "changelang", Color.yellow, component.lang);
                    }
                }
                else
                {
                    mySqlDataReader.Close();
                    mySqlDataReader.Dispose();
                    Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                    component.lang = Plugin.Instance.Configuration.Instance.startlang;
                    mySqlCommand.CommandText = string.Concat(new string[]
                    {
                        "INSERT INTO ",
                        Plugin.Instance.Configuration.Instance.mysqltable,
                        " (`Steam64ID`,`Language`) VALUES('",
                        player.CSteamID.m_SteamID.ToString(),
                        "', '",
                        component.lang,
                        "')"
                    });
                    mySqlCommand.ExecuteNonQuery();
                }
                mySqlConnection.Close();
            }
            else
            {
                if (Plugin.Instance.Configuration.Instance.Users.Exists(x => x.playerid == player.CSteamID.m_SteamID))
                {
                    Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                    component.lang = Plugin.Instance.Configuration.Instance.Users.Find(x => x.playerid == player.CSteamID.m_SteamID).lang;
                    if (!Plugin.Instance.Configuration.Instance.Langs.Exists(x => x.id == component.lang))
                    {
                        component.lang = Plugin.Instance.Configuration.Instance.startlang;
                        Plugin.SDLL.MultiSay(player, Plugin.plugin, "changelang", Color.yellow, component.lang);
                    }
                }
                else
                {
                    Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                    component.lang = Plugin.Instance.Configuration.Instance.startlang;
                    Plugin.Instance.Configuration.Instance.Users.Add(new Config.User { lang = component.lang, playerid = player.CSteamID.m_SteamID });
                    Plugin.Instance.Configuration.Save();
                }
            }
        }

        public void UpdateInfo(UnturnedPlayer player, string lang)
        {
            if (Plugin.Instance.Configuration.Instance.isDB)
            {
                MySqlConnection mySqlConnection = this.createConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat(new string[]
                {
                    "UPDATE ",
                    Plugin.Instance.Configuration.Instance.mysqltable,
                    " SET Language = '",
                    lang,
                    "' WHERE Steam64ID = '",
                    player.CSteamID.m_SteamID.ToString(),
                    "';"
                });
                mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
            }
            else
            {
                if (Plugin.Instance.Configuration.Instance.Users.Exists(x => x.playerid == player.CSteamID.m_SteamID))
                {
                    Plugin.Instance.Configuration.Instance.Users.Find(x => x.playerid == player.CSteamID.m_SteamID).lang = lang;
                    Plugin.Instance.Configuration.Save();
                }
            }
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            component.lang = lang;
        }

        private void SQLChecker()
        {
            MySqlConnection mySqlConnection = this.createConnection();
            MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
            mySqlCommand.CommandText = "show tables like '" + this._plugin.Configuration.Instance.mysqltable + "'";
            mySqlConnection.Open();
            object obj = mySqlCommand.ExecuteScalar();
            bool flag = obj == null;
            if (flag)
            {
                mySqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS `" + this._plugin.Configuration.Instance.mysqltable + "` (\t`ID` INT NOT NULL AUTO_INCREMENT,\t`Steam64ID` TEXT NOT NULL, `Language` TEXT NOT NULL,\tPRIMARY KEY(`ID`))";
                mySqlCommand.ExecuteNonQuery();
            }
            mySqlConnection.Close();
        }
    }

    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin Instance;
        public DatabaseManager Database;
        public const string plugin = "SDLangSystem";
        public const string HarmonyInstanceId = "com.sodadevs.langsystem";
        public static SDMultiLangLib.Lib SDLL;
        private Harmony HarmonyInstance;
        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList
                {
                    {"changelang", "Use /lang to change language of server. Now your language: {0}"},
                    {"success", "Your language changed on {0}"},
                    {"langs", "Here languages: {0}"},
                    {"error", "Something went wrong with translate! Contact with owner - he must recreate plugin translation"},
                    {"nolang", "This language not exists"},
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("|                     Exclusive version                    |", System.ConsoleColor.Blue);
            Logger.Log("|                 SodaDevs: Language System                |", System.ConsoleColor.Blue);
            Logger.Log("|                     RocketMod Version                    |", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("Version: " + Assembly.GetName().Version, System.ConsoleColor.Blue);
            Logger.Log("WARNING! THIS PLUGIN MUST WORK PERMAMENT! IF YOU UNLOAD PLUGIN - IT CAN CRASH OTHER PLUGINS!");
            HarmonyInstance = new Harmony(HarmonyInstanceId);
            HarmonyInstance.PatchAll(Assembly);
            U.Events.OnPlayerConnected += OnConnect;
            Database = new DatabaseManager(this);
            SDLL = new SDMultiLangLib.Lib(this);

            if (Level.isLoaded)
                OnLevelLoaded(0);
            else
                Level.onLevelLoaded += OnLevelLoaded;
        }

        private void OnConnect(UnturnedPlayer player)
        {
            Database.GetInfo(player);
        }

        private void OnLevelLoaded(int level)
        {
            SDLL.CheckTranslateSystem(plugin, DefaultTranslations);
            /*foreach (IRocketPlugin p in R.Plugins.GetPlugins())
            {
                SyncTranslate(p.Name, p.DefaultTranslations);
            }*/
        }

        /*[HarmonyPatch(typeof(UnturnedChat), "Say")]
        public static class UnturnedChat_Say_Patch
        {
            [HarmonyPrefix]
            public static bool UnturnedChat_Say_Prefix(CSteamID CSteamID, string message, Color color, bool rich)
            {
                if (CSteamID.ToString() == "0")
                {
                    Rocket.Core.Logging.Logger.Log(message, ConsoleColor.Gray);
                    return;
                }
                SteamPlayer steamPlayer = PlayerTool.getSteamPlayer(CSteamID);
                foreach (string text in UnturnedChat.wrapMessage(message))
                {
                    ChatManager.serverSendMessage(text, color, null, steamPlayer, EChatMode.SAY, null, rich);
                }
                return true;
            }
        }*/
        
        public static string GetLang(UnturnedPlayer player)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            return component.lang;
        }

        public static string GetLangName(UnturnedPlayer player)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            return Instance.Configuration.Instance.Langs.Find(x => x.id == component.lang).name;
        }

        public static string GetLangName(string id)
        {
            if (Instance.Configuration.Instance.Langs.Exists(x => x.id == id)) return Instance.Configuration.Instance.Langs.Find(x => x.id == id).name;
            else return null;
        }

        public static bool SendChat(UnturnedPlayer player, string pluginid, string key, Color color, params object[] parameters)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            string lang = component.lang;
            string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
            if (string.IsNullOrEmpty(text))
            {
                UnturnedChat.Say(player, key, color);
                return false;
            }
            if (text.Contains("{") && text.Contains("}") && parameters != null && parameters.Length != 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == null)
                    {
                        parameters[i] = "NULL";
                    }
                }
                text = string.Format(text, parameters);
            }
            UnturnedChat.Say(player, text, color);
            return true;
        }

        public static bool SendChat(UnturnedPlayer player, string pluginid, string key, Color color)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            string lang = component.lang;
            string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
            if (string.IsNullOrEmpty(text))
            {
                UnturnedChat.Say(player, key, color);
                return false;
            }
            UnturnedChat.Say(player, text, color);
            return true;
        }

        public static string GetTranslate(UnturnedPlayer player, string pluginid, string key, params object[] parameters)
        {
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            string lang = component.lang;
            string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
            if (string.IsNullOrEmpty(text))
            {
                return key;
            }
            if (text.Contains("{") && text.Contains("}") && parameters != null && parameters.Length != 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == null)
                    {
                        parameters[i] = "NULL";
                    }
                }
                text = string.Format(text, parameters);
            }
            return text;
        }

        public static string GetTranslate(string lang, string pluginid, string key)
        {
            string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
            if (string.IsNullOrEmpty(text))
            {
                return key;
            }
            return text;
        }

        public static string GetTranslate(string lang, string pluginid, string key, params object[] parameters)
        {
            string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
            if (string.IsNullOrEmpty(text))
            {
                return key;
            }
            if (text.Contains("{") && text.Contains("}") && parameters != null && parameters.Length != 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] == null)
                    {
                        parameters[i] = "NULL";
                    }
                }
                text = string.Format(text, parameters);
            }
            return text;
        }

        public static bool BroadCast(string pluginid, string key, Color color, params object[] parameters)
        {
            foreach (SteamPlayer sp in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(sp);
                Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                string lang = component.lang;
                string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                if (text.Contains("{") && text.Contains("}") && parameters != null && parameters.Length != 0)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i] == null)
                        {
                            parameters[i] = "NULL";
                        }
                    }
                    text = string.Format(text, parameters);
                }
                UnturnedChat.Say(player, text, color);
            }
            return true;
        }

        public static bool BroadCast(string pluginid, string key, Color color)
        {
            foreach (SteamPlayer sp in Provider.clients)
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(sp);
                Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
                string lang = component.lang;
                string text = Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == pluginid).Langs.Find(x => x.LangID == lang).LangString.Find(x => x.id == key).text;
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                UnturnedChat.Say(player, text, color);
            }
            return true;
        }

        public class PlayerComponent : UnturnedPlayerComponent
        {
            protected override void Load()
            {
                this.lang = "";
            }

            public string lang;
        }


        public static bool SyncTranslate(string id, TranslationList list)
        {
            try
            {
                if (!Instance.Configuration.Instance.PluginTranslate.Exists(x => x.pluginid == id))
                {
                    return GenerateClear(id, list);
                }
                foreach (Config.Lang lang in Instance.Configuration.Instance.Langs)
                {
                    if (Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == id).Langs.Exists(x => x.LangID == lang.id))
                    {
                        foreach (TranslationListEntry entity in list)
                        {
                            if (!Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == id).Langs.Find(x => x.LangID == lang.id).LangString.Exists(x => x.id == entity.Id))
                                Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == id).Langs.Find(x => x.LangID == lang.id).LangString.Add(new Config.LangEntity { id = entity.Id, text = entity.Value });
                        }
                    }
                    else
                    {
                        List<Config.LangEntity> ListOfS = new List<Config.LangEntity>();
                        foreach (TranslationListEntry entity in list)
                        {
                            ListOfS.Add(new Config.LangEntity { id = entity.Id, text = entity.Value });
                        }
                        Instance.Configuration.Instance.PluginTranslate.Find(x => x.pluginid == id).Langs.Add(new Config.LangList { LangID = lang.id, LangString = ListOfS });
                    }
                    Instance.Configuration.Save();
                } 
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool GenerateClear(string id, TranslationList list)
        {
            try
            {
                List<Config.LangEntity> ListOfS = new List<Config.LangEntity>();
                foreach (TranslationListEntry entity in list)
                {
                    ListOfS.Add(new Config.LangEntity { id = entity.Id, text = entity.Value });
                }
                List<Config.LangList> LangLists = new List<Config.LangList>();
                foreach (Config.Lang l in Instance.Configuration.Instance.Langs)
                    LangLists.Add(new Config.LangList { LangID = l.id, LangString = ListOfS });
                Instance.Configuration.Instance.PluginTranslate.Add(new Config.PluginContainer { pluginid = id, Langs = LangLists });
                Instance.Configuration.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }


    class CommandLeaderBoard : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Name
        {
            get
            {
                return "language";
            }
        }

        public string Help
        {
            get
            {
                return "Select language";
            }
        }

        public string Syntax
        {
            get
            {
                return "Usage: /lang";
            }
        }

        public List<string> Aliases
        {
            get
            {
                return new List<string>
                {
                    "lang"
                };
            }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>
                {
                    "language"
                };
            }
        }


        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            Plugin.PlayerComponent component = player.GetComponent<Plugin.PlayerComponent>();
            if (command.Length == 0)
            {
                string text = "";
                foreach (Config.Lang lang in Plugin.Instance.Configuration.Instance.Langs)
                {
                    if (text != "") text += ", ";
                    text += lang.id + " (" + lang.name + ")";
                }
                Plugin.SDLL.MultiSay(player, Plugin.plugin, "langs", Color.yellow, text);

            }
            else
            {
                if (Plugin.Instance.Configuration.Instance.Langs.Exists(x => x.id == command[0]))
                {
                    Plugin.Instance.Database.UpdateInfo(player, command[0]);
                    Plugin.SDLL.MultiSay(player, Plugin.plugin, "success", Color.yellow, Plugin.Instance.Configuration.Instance.Langs.Find(x => x.id == command[0]).name);
                }
                else if (Plugin.Instance.Configuration.Instance.Langs.Exists(x => x.name.ToLower() == command[0].ToLower()))
                {
                    Plugin.Instance.Database.UpdateInfo(player, Plugin.Instance.Configuration.Instance.Langs.Find(x => x.name == command[0]).id);
                    Plugin.SDLL.MultiSay(player, Plugin.plugin, "success", Color.yellow, command[0]);
                }
                else
                {
                    Plugin.SDLL.MultiSay(player, Plugin.plugin, "nolang", Color.yellow);
                }
            }
        }
    }
}
