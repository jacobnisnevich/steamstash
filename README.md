# Steam Stash
Steam Stash is a Windows desktop utility for backing up unplayed or unneeded Steam games to a different drive or directory. Steam Stash is built with C# and the Windows Presentation Foundation UI suite, and as such is only compatible with Windows at the moment. In order to select recent games, Steam Stash connects to your Steam account via the Steam Web API to determine which games you have played in the past two weeks then uses a fuzzy matching algorithm to find the corresponding ```steamapps``` folders.