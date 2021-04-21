# RocketModPlugin.SDLangSystem - Multilanguage
Free-To-Use plugin

What this plugin do?
===========
This plugin copy all translation lists of plugins that use SDMultiLangLib. In configuration of plugin serverowner can add languages and then translate lists for all plugins, that you can find in .cfg

Configuration
===========
```
<?xml version="1.0" encoding="utf-8"?>
<Config xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <isDB>false</isDB>
  <mysqlip>IP</mysqlip>
  <mysqlusr>User</mysqlusr>
  <mysqlpass>Password</mysqlpass>
  <mysqlport>Port</mysqlport>
  <mysqldb>Database</mysqldb>
  <mysqltable>Table</mysqltable>
  <startlang>en</startlang>
  <Langs>
    <Lang id="en">English</Lang>
    <Lang id="ru">Русский</Lang>
  </Langs>
  <PluginTranslate>
    <PluginContainer pluginid="SDLangSystem">
      <Langs>
        <LangList LangID="en">
          <LangString>
            <LangEntity id="changelang">Use /lang to change language of server. Now your language: {0}</LangEntity>
            <LangEntity id="success">Your language changed on {0}</LangEntity>
            <LangEntity id="langs">Here languages: {0}</LangEntity>
            <LangEntity id="error">Something went wrong with translate! Contact with owner - he must recreate plugin translation</LangEntity>
            <LangEntity id="nolang">This language not exists</LangEntity>
          </LangString>
        </LangList>
        <LangList LangID="ru">
          <LangString>
            <LangEntity id="changelang">Use /lang to change language of server. Now your language: {0}</LangEntity>
            <LangEntity id="success">Your language changed on {0}</LangEntity>
            <LangEntity id="langs">Here languages: {0}</LangEntity>
            <LangEntity id="error">Something went wrong with translate! Contact with owner - he must recreate plugin translation</LangEntity>
            <LangEntity id="nolang">This language not exists</LangEntity>
          </LangString>
        </LangList>
      </Langs>
    </PluginContainer>
  </PluginTranslate>
  <Users />
</Config>
```

Commands
===========
> * /lang - Just change language for player. Premission: language

In future
===========
* Agressive translation - It will make copy all of plugins (Who don't use library), make copy documents of translations for all languages and remake Translate() function LanguageSystem
* Google Translator API anf Yandex Translator API - Its need only for BIG project that can pay for this API to Google or Yandex - It can automatic translate all translation list and automatic translate all player's messages in chat

Why Developers must use LIB, not this plugin?
===========
If serverowner don't need multi-language support, he don't want install this plugin, and then developer's plugin will crash (If developer don't add too much trashcodes). My library check, is player have SDLangSystem, and if plugin not found - it will use standart Translation system of RocketMod.
And second problem is my fresh code, that soon will be remake, but all libs will work good.

Library: https://github.com/LenzMix/RocketModLibrary.MultiLanguage
