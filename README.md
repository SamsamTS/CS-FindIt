# Find It! 2 [TEST]  

Forked from SamSamTS's original Find It! mod.  

## Future plans
- Check the issues tab to see WIP and feature suggesions that are under investigating.

### Contact
Steam workshop [link](https://steamcommunity.com/sharedfiles/filedetails/?id=2133885971)

You can use Github's issues tab or via Steam. This repo is not under my main Github account so you probably will get a faster response if you send me a Steam message or leave a comment on the workshop page. I'm open to collabration and suggestion. You can use the issues tab to report  bug or provide suggesion, pull requests are also welcome. 

## Change Notes

Finished and planned to be included in the next release:
- Reintroduce the new thumbnail generation approach after the freezing issue was solved. Reduced memory usage and faster generation. Credit to algernon

- Add 'x" to recently introduced custom tag pop-ups

- Fix the issue where the custom tag pop-up may be shown out of screen(when Resize It makes the panel too wide)

v1.7.0-3 Update:

- Switch back to the original way of thumbnail generation temporarily to avoid the 'freezing' issue.

v1.7.0-2 Update:

- Attempt to fix searchbox input issues

v1.7.0 Update:
- Remove built-in Net Picker 3  

Quboid's new Picker mod is the recommended replacement. It offers similar features with additional support for Move It.

- Revise thumbnail generation for reduced memory usage and faster generation. Credit to algernon.  

v1.6.9 Update:
- Custom tag batch actions:  
-- Add a tag to multiple assets   
-- Remove a tag from multiple assets  

- Picker now can pick decal
- New picker enable/disable checkbox.
- Remove picker hotkey temporarily for upcoming hotkey implementation rewrite

v1.6.8 Update:  
- New drop-down menu in custom tag pop-up window. Easier to add existing tags to an asset.  

- New custom tag panel:  
-- Show a list of custom tags and number of assets in each tag.  
   By default the list is sorted by the number of assets in each tag. It can be changed to use alphabetical order in mod settings.  
-- The drop-down list also can be used as a filter to see all assets with this tag  
-- Rename tags  
-- Merge tags  
-- Delete tags  
- **Experimental:** Add built-in Elektrix Net Picker 3.0 (thanks Elektrix for sharing the source code)  

- Add Tree size filter tabs

- Add language support: Deutsch, polski, español, Français. (not fully translated yet)

- Add tooltip message to the workshop icon. Not a new feature, just a reminder. You can open an asset's workshop page by right-clicking the icon.

v1.6.7 Update:
- Add new Growable/RICO combined search option. You can select and place both types in one place.  
- Show custom tags file path in mod settings  

v1.6.6 Update:
- Move from the deprecated detour library to Harmony 2, credit to algernon. boformer's [Cities Harmony mod](https://steamcommunity.com/workshop/filedetails/?id=2040656402) now is a required item.

v1.6.5-2 Update:
- Fix some bugs
- Add language support: 한국어, Русский
- Custom language setting menu(needed for 繁體中文), credit to algernon.
- New XML settings implementation, credit to algernon.
- New keyboard shortcut implementation, credit to algernon.

v1.6.5 Update:
- Add filter tabs for props. The categories of the props were decided by which asset editor import templates the asset creators chose, so many are not properly set up. Similar categorization as in More Beautification.

- Three new building size filter options for RICO: 5-8, 9-12, 13+. Easier to find larger RICO buildings.

- Modern Japan Content Creator Pack buildings are counted both as "custom" and "vanilla".

v1.6.4 Update:
- **Experimental** Sort assets by most recently downloaded. A new button is added and you can toggle between the sorting methods.

This will help you find your recent subscriptions unless you reinstall the game recently. This feature was tested on Windows and it worked as expected.

On Linux(Flatpak Steam) if an asset creator updates their asset, that asset would be moved to the top of the list. This is not the case for Windows. I don't know if it will work correctly on other platforms(especially macOS).

- Add language support: 日本語, 繁體中文, 简体中文

- 日本語：kei_em さんの [Japanese Localization Mod (日本語化MOD]([url=https://steamcommunity.com/sharedfiles/filedetails/?id=427164957) を使用している場合は、自動的に日本語で表示されます。ゲーム設定で英語に変更してから日本語に戻す必要があるかもしれません

- 繁體中文：需要到MOD設定頁面手動設定 

- 简体中文：若游戏已默认设定显示简体中文, 则此模组会自动显示简体中文

v1.6.3 Update:
- Multi-language support is added(thanks algernon for sharing the translation framework!) and translation is WIP. Volunteers for translation are need. You can use [this google sheet](https://docs.google.com/spreadsheets/d/16KPl6X8SZAJTKzXZtQn_Xnh0kelOF2b56tpS4ax8P2E/edit#gid=0) to help the translation.

- Improve filter tab toggle behavior: Click a highlighted filter tab again will "select all" instead of doing nothing unless shift or crtl is pressed. I think this is more intuitive.

v1.6.2 Update:
- New ploppable tabs & icon change. Some DLC buildings are moved to new tabs.

v1.6.1 Update:
- New filter checkbox: choose to include/exclude vanilla assets and custom assets (from Steam workshop subscription or saved in local asset folder) in the search results.

- More flexible search by growable size: You can search for sizes like 1xAll, 2xAll, 3xAll and 4xAll. Notice: CO named their assets inconsistently. For example a building was mistakenly named as 3x4 when it is actually 4x3. The search is based on the actual dimension not the name.
 
v1.6(first release):
- Fix some known minor issues in the original Find It.
- New filter tabs for ECO growables.
- Buildings added in the newer DLCs now are included and can be found under the filter tabs.
