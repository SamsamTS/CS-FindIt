# Find It! 2

Forked from SamSamTS's original Find It! mod.  

## Development Environment  
Visual Studio 2019 should be able to set up everything automatically. Just open the .csproject file. Andreas Pardeike's [Harmony](https://github.com/pardeike/Harmony) patching library is a needed dependency. If Visual Studio doesn't set it up for you, you can get Harmony 2 via boformer's [CitiesHarmony](https://github.com/boformer/CitiesHarmony) nuget package.  

## Pull Requests
Pull requests are welcome. I write code in other languages at work but I only write C# code for modding this game, so the code here isn't well written. Legacy code from the original Find It lacks of documentation too(I'm slowly working on this). If it is just a few lines of code, you can send in the pull request directly. If it is going to be a big change, you can create a new issue for discussion before you start working on it.  

## Future plans
- Check the issues tab to see WIP and feature suggesions that are under investigating.

### Contact
Steam workshop [link](https://steamcommunity.com/sharedfiles/filedetails/?id=2133885971)

You can use Github's issues tab or via Steam. This repo is not under my main Github account so you probably will get a faster response if you send me a Steam message or leave a comment on the workshop page. I'm open to collabration and suggestion. You can use the issues tab to report  bug or provide suggesion, pull requests are also welcome. 

## Change Notes

v2.0.3 Update:
- Add 'Include Procedural Objects' option in quick menu for 'Show asset instance count'  

- Minor UI changes

- Fix [this and other inherited bugs](https://www.youtube.com/watch?v=-G6lxpwON4Q) related to the road options panel  

- Assets from all 6 content creator packs are considered as both 'vanilla' and 'custom'. The asset creator filter now associates these assets with the creators' Steam profile names, along with their other workshop assets.  
Content Creator Pack: Art Deco by "Shroomblaze"  
Content Creator Pack: High-Tech Buildings by "GCVos"  
Content Creator Pack: European Suburbia by "Avanya"  
Content Creator Pack: University City by "KingLeno"  
Content Creator Pack: Modern City Center by "AmiPolizeiFunk"  
Content Creator Pack: Modern Japan by "Ryuichi Kaminogi"  

- Add more keyboard shortcuts (the bulldoze tool issue is still unsolved)  

- Change the search button to a "clear search box" button  

- Add scrollbars to custom tag drop-downs  

- When a drop-down menu is clicked and focused, you can also use the ↑ ↓ arrow keys to move around  

- Fix the thumbnail issue related to vanilla props with "prop fence" shader  

v2.0.2 Update:
- New 'Unused Assets' filter in the extra filters panel.

- New 'Show asset instance count' and sorting options in the quick menu.

- Add 'Quick Menu' button

- Add 'Unsorted' filter tab for growable

- Fix Esc key bug

- When Resize It changes the scroll panel width, the filter tab panel will adjust itself to match the width

- Move 'Building Level' filter to the extra filters panel

- UI changes

- Changing the visibility of marker type props in game mode doesn't require a restart anymore.

- Expand search functionality. Can use prefix ! to exclude a word, and use prefix # to search for custom tag only.

v2.0.1 Update:  
- Add random selection button to all asset types (user requested)

- Add building size filter to Ploppable

- Add 'Unsorted' filter tab to Ploppable

- Add filter tabs for network assets

- Show extra asset info in thumbnail pop-up(asset type & sub-type, building size, building height, etc.) 

- Add 'Prop Marker' filter tab to Prop. It will only appear in 'editor' mode

- 'Prop Marker' type props will no longer appear in 'game' mode. They only work in 'editor' mode and you couldn't bulldoze them easily in 'game' mode, so they will be hidden unless you're in the asset or map editor.

- It is now possible to move around the asset type drop-down menu using the  ↑ ↓ arrow keys when you're typing in the search input box (added by [Brot](https://github.com/gregorsimpson))

v2.0.0 Update:  
- No major changes in this update.  
Just that the version number jumps from v1.7.3-3 to v2.0.0 to match the name of this mod, and the [TEST] label is removed.

v1.7.3-2 Update:
- Unchecked filter tab will show its normal sprite when it is hovered

V1.7.3-1 Update:
- Change tree filter tab icons  

V1.7.3 Update:
- Option to sort asset creator list alphabetically

- Random selection button for growable and RICO

V1.7.2 Update:
- UI Changes. New panel for extra filters    

- Extra filters:  
-- a list of asset creators and number of assets made by each asset creator  
-- building height  

- Adjust thumbnail model rotation(algernon)

V1.7.1 Update:
- Reintroduce the new thumbnail generation approach after the freezing issue was solved. Reduced memory usage and faster generation. Credit to algernon

- Add 'x" button to recently introduced custom tag pop-ups

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
