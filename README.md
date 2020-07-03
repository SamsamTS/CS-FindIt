# Find It! Fix  

Forked from SamSamTS's original Find It! mod.  

## Future plans
- Check the issues tab to see WIP and feature suggesions that are under investigating.

### Contact
Steam workshop [link](https://steamcommunity.com/sharedfiles/filedetails/?id=2133885971)

You can use Github's issues tab or via Steam. This repo is not under my main Github account so you probably will get a faster response if you send me a Steam message or leave a comment on the workshop page. I'm open to collabration and suggestion. You can use the issues tab to report  bug or provide suggesion, pull requests are also welcome. 

## Change Notes

Implemented and planned be included in the next release:

- Show custom tags file path in mod settings  
(- Growable/RICO combined search option)  

New in v1.6.6:
- Move from the deprecated detour library to Harmony 2, credit to algernon. boformer's [Cities Harmony mod](https://steamcommunity.com/workshop/filedetails/?id=2040656402) now is a required item.

New in v1.6.5-2:
- Fix some bugs
- Add language support: 한국어, Русский
- Custom language setting menu(needed for 繁體中文), credit to algernon.
- New XML settings implementation, credit to algernon.
- New keyboard shortcut implementation, credit to algernon.

New in v1.6.5:
- Add filter tabs for props. The categories of the props were decided by which asset editor import templates the asset creators chose, so many are not properly set up. Similar categorization as in More Beautification.

- Three new building size filter options for RICO: 5-8, 9-12, 13+. Easier to find larger RICO buildings.

- Modern Japan Content Creator Pack buildings are counted both as "custom" and "vanilla".

New in v1.6.4:
- **Experimental** Sort assets by most recently downloaded. A new button is added and you can toggle between the sorting methods.

This will help you find your recent subscriptions unless you reinstall the game recently. This feature was tested on Windows and it worked as expected.

On Linux(Flatpak Steam) if an asset creator updates their asset, that asset would be moved to the top of the list. This is not the case for Windows. I don't know if it will work correctly on other platforms(especially macOS).

- Add language support: 日本語, 繁體中文, 简体中文

- 日本語：kei_em さんの [Japanese Localization Mod (日本語化MOD]([url=https://steamcommunity.com/sharedfiles/filedetails/?id=427164957) を使用している場合は、自動的に日本語で表示されます。ゲーム設定で英語に変更してから日本語に戻す必要があるかもしれません

- 繁體中文：需要到MOD設定頁面手動設定 

- 简体中文：若游戏已默认设定显示简体中文, 则此模组会自动显示简体中文

New in v1.6.3:
- Multi-language support is added(thanks algernon for sharing the translation framework!) and translation is WIP. Volunteers for translation are need. You can use [this google sheet](https://docs.google.com/spreadsheets/d/16KPl6X8SZAJTKzXZtQn_Xnh0kelOF2b56tpS4ax8P2E/edit#gid=0) to help the translation.

- Improve filter tab toggle behavior: Click a highlighted filter tab again will "select all" instead of doing nothing unless shift or crtl is pressed. I think this is more intuitive.

New in v1.6.2:
- New ploppable tabs & icon change. Some DLC buildings are moved to new tabs.

New in v1.6.1:
- New filter checkbox: choose to include/exclude vanilla assets and custom assets (from Steam workshop subscription or saved in local asset folder) in the search results.

- More flexible search by growable size: You can search for sizes like 1xAll, 2xAll, 3xAll and 4xAll. Notice: CO named their assets inconsistently. For example a building was mistakenly named as 3x4 when it is actually 4x3. The search is based on the actual dimension not the name.
 
New in v1.6(first release):
- Fix some known minor issues in the original Find It.
- New filter tabs for ECO growables.
- Buildings added in the newer DLCs now are included and can be found under the filter tabs.
