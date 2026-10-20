# Goblin Island

Submission for [Ludum Dare 60](https://ldjam.com/events/ludum-dare/60).

Do you like beer?  
Too bad, 'cause so do goblins.

Protect your holy beer stein from hordes of over-sobered goblins!  
Once a goblin touches your precious beer, it's tainted and you'll die.  
Make sure they NEVER reach it!

## How To Play

You'll essentially cycle through 2 phases.

### 1. Strategize
You have 3 tools available.  
They are aligned vertically at the bottom left of your screen.

#### Shop
Here you can purchase weapons for placing when using the `Place` tool.  
The weapons available for purchase will appear horizontally at the bottom left of your screen.
You'll need to expend gold to purchase weapons.
Your current amount of gold is displayed in the topleft of the screen.
The purchased stock for each weapon is displayed in the bottom right of its tool button.

#### Place
Place the weapons you purchased using the `Shop` tool.  
Select a weapon by clicking its weapon button (the same as in the `Shop` tool).
When moving your cursor over the map you'll see a preview.
Click to place.
You can't place a weapons inside other units or if you don't have any instances left.

#### Cursor
Nothing here yet.
This was supposed to show unit details, but cruchtime said otherwise.

### 2. Horde
Hordes of goblins will spawn from the three doors placed on the map.  
Each wave has a preset number of goblins.
You can use the `Cursor` and `Place` tool during this phase,
but the `Shop` is unavailable, so prepare wisely.
Kill all of them and the phase will end.  
You have two options to kill a goblin:

#### Damage
Use your weapons, like the Ballista, Catapult or Rock drops to hit a goblin directly.  
Killing them this way will yield 25 coins per goblin.

#### Kick them of the map
Use obstacles or weapon knockback to kick them off the map.  
While this may seem easier it only yields 10 coins per goblin.

## Building and running

Supported platforms are Windows and Linux (experimental).

Windows builds are available on the [releases page](https://github.com/ChaosTechnology/LD60/releases/).

Requirements:
- [dotnet 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or higher
- An OpenAL installation:
  - [Download for Windows](https://www.openal.org/downloads/)
  - for Linux, install via your package manager, for example:
  ```
  sudo apt install libopenal-dev
  ```

Then run the folliwing in a bash (git bash for Windows supported):

```
git clone --recurse-submodules https://github.com/ChaosTechnology/LD60.git
bash ./LD60/run.sh
```
