using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace SDLangSystem
{
    public class Config : IRocketPluginConfiguration, IDefaultable
    {
        public void LoadDefaults()
        {
            this.isDB = false;
            this.mysqlip = "IP";
            this.mysqlport = "Port";
            this.mysqlusr = "User";
            this.mysqlpass = "Password";
            this.mysqldb = "Database";
            this.mysqltable = "Table";
            this.startlang = "en";
            this.Langs = new List<Lang>
            {
                new Lang
                {
                    id = "en",
                    name = "English"
                },
                new Lang
                {
                    id = "ru",
                    name = "Русский"
                }
            };
            this.PluginTranslate = new List<PluginContainer>
            {
                
            };
            this.Users = new List<User>();
        }

        public bool isDB;
        public string mysqlip;
        public string mysqlusr;
        public string mysqlpass;
        public string mysqlport;
        public string mysqldb;
        public string mysqltable;
        public string startlang;
        public List<Lang> Langs;
        public List<PluginContainer> PluginTranslate;
        public List<User> Users;

        public class User
        {
            public ulong playerid;
            public string lang;
        }

        public class Lang
        {
            [XmlAttribute]
            public string id;
            [XmlText]
            public string name;
        }

        public class PluginContainer
        {
            [XmlAttribute]
            public string pluginid;
            public List<LangList> Langs;
        }

        public class LangList
        {
            [XmlAttribute]
            public string LangID;
            public List<LangEntity> LangString;
        }

        public class LangEntity
        {
            [XmlAttribute]
            public string id;
            [XmlText]
            public string text;
        }
    }
}