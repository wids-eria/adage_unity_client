#iOS KeyChain Sharing
####Sharing authorization info between multiple iOS applications

Until the update, ADAGE used text file (session.info in persistentDataPath) to save cached authorization info. After the update, it can also use iOS KeyChain to store cached authorization info (player name, adage token, and other information), which means this info can now be shared between apps using Keychain Sharing feature. 

In order to do that, you need to:

1. Set "**Use iOS KeyChain**" to "**true**" in the Adage component.

2. If you are using Unity 4, copy the content of the "**Assets/ADAGE_EXT/Plugins/**" folder to the "**Assets/Plugins/**" folder (create it if it doesn’t exist yet).

3. Every time you build the project, you should enable the "**KeyChain Sharing**" in your "**Capabilities tab**" (see the screenshot attached).

4. Change your KeyChain group name to something unique. It must follow the "bundle id" dormat, i.e. reverse-dns format ("org.learninggamesnetwork.devry.adage.auth"). You can specify anything instead of "devry.adage.auth" but you have to use the same id for all your apps that will share the same KeyChain group.


You need to repeat these steps for each project that will share the same keychain group. You should also make sure you are using the same "app id prefix" ("Bundle Seed ID") for your apps.

You have to repeat steps 3 and 4 every time you build a new project or replace an existing one. You don’t need to repeat them if you are using "append" feature while building a project in Unity.

