ADAGE Unity Client Release June 24, 2014
=========================================

###New Features
* Upgrades to the editor settings to make server connection and login configuration easier.
* Messenger class for querying data from the unity client. Useful for building realtime scoreboard, heatmap or other data driven tools that can accompany a game.
* Beginning support for uploading game specific data structure specification to the Game Version data API. This will enable more server side support of analysis and error checking of game data.


###Bug Fixes
* Protection errors caused by referenced data being buffered before being pushed to the server
* Memory optimizations for data buffering



ADAGE Unity Client Release April 1, 2014
=========================================

###New Features

* Support for Save Game and Configuration File API.
* Beginning of support for multiple users on a single device.
* Option to have game handle login UI.
* Option to force login of a pre-registered user account.
* Device info log on start game.
* Support for a game id and other revisions to the ADAGE Data specification
* Automatic Keyboard and Mouse log capture.
* ADAGETracker for querying ADAGE data from Unity.


###Bug Fixes

* Fallback to local data storage now does incremental write instead of storing all data in memory.
* Fixed save and restore of last logged in player fixing the issue with creating enourmous amounts of guest accounts or having to manually login at game start.
