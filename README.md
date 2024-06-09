# CS2 HideAndSeek Plugin
A CS2 Plugin Made for Hide and Seek on the [EdgeGamers](EdgeGamers.Com) Events Server.
## Description
Hide and Seek Plugin manages gameplay of a Hide and Seek Gamemode.
- Selects random first seeker.
- The last hider to die will become the seeker next round.
- Displays who found who
## Requirements
- [MetaMod:Source](https://github.com/alliedmodders/metamod-source/)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
## What Works?
These features are what is currently working or planeed
- [x] Auto Select Random Seeker
  - [x] Select Hiding Winner as Next Seeker
  - [x] Allow for Multiple Starting Seekers
  - [ ] Allow Seekers to Instant Kill Hiders
  - [ ] Allow Seekers to be faster than Hiders
- [ ] Allow for Team Balance and Zombie Styles Interchangeably
  - [x] Zombie Stlye
  - [ ] Team Balance Style
- [ ] Add Custom Colors to Prefix for Customizablity
- [x] Show Name of Killing Seeker 
## Install
1. Install Metamod:Source and Counter Strike Sharp.
2. Copy `HideAndSeekPlugin` to `csgo/addons/counterstrikesharp/plugins/`.
3. After the first run, update the configuration file `HideAndSeekPlugin.json` as detailed below.
## Configuration
After first load, a configuration file will be created in 
`csgo/addons/counterstrikesharp/configs/plugins/HideAndSeekPlugin/HideAndSeekPlugin.json`.
### Game Settings

| Setting | Default | Description |
| --- | --- | --- |
| hns_prefix | "[HNS]" | Sets The prefix that the plugin uses for messages in chat. |
| hns_zombie_style | true | Sets the gmae to start with one seeker and everyone they kill joins them. (Currently Not used) |
| hns_min_players | 2 | Sets the Minimum players required to start. | 
| hns_t_instakills | true | Sets if the Seeker only needs to hit the ct's once to kill them (Currently Not used) |
| Hns _tr_t_speed | 1 | Sets the Multiplier for The Seekers Speed (Currently Not used) |
| hns_starting_ts | 1 | Sets the amount of seekers to start with, Zombie mode forces this to 1 (Currently Not used) |
