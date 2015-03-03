#Super Mario Brothers
####_An example of using ADAGE to log game data (in Unity-C#)_



A short example of how we might extend the ADAGE base data structures to log data for a simple and iconic platform game.

The start and end context are useful because once opened all events in a level will be tagged with all open context names. This will give us an easy way to later query all events that happened on a certain game level.

```C#
MarioLevelStart : ADAGEContextStart {}
```

```C#
MarioLevelEnd : ADAGEContextEnd 
{
	int coins;
	int time;
	int score;
	string mario_size;
} 
```

A couple of special events for when the game is over or when the game is won.

```C#
MarioGameOver : ADAGEGameEvent {}
```

```C#
MarioGameWon : ADAGEGameEvent {}
```

The following events all have to do with collecting items in the game. We create a general collection subclass and then name the item collected as a field. This will give us an easy way to query all collection events later and then break them down into their specific types.

```C#
MarioCollectEvent : ADAGEPlayerEvent 
{
	string item_name; //coin, super mushroom, fire flower, etc.
}
```

The following events deal Mario changing form, with Mario killing enemies and being hit. These are pretty straight forward. 

```C#
MarioHit : ADAGEPlayerEvent
{
	bool death_blow; //did this hit kill mario
	bool invincible; //was mario invincible
}
```

```C#
MarioChangeForm : ADAGEPlayerEvent
{
	string form; //mario, super mario, fire mario
}
```

```C#
MarioEnemyKill : ADAGEPlayerEvent
{
	string enemy_type; //gumba, kuppa, etc
	string death_type; //Squish, fireball, etc
}
```

For this game we are also going to record input events. Input logging is pretty intense and can create a lot of data. For this particular case we have chosen to record input handling because it would be possible to playback an entire session from the input information which might be valuable. For a games that are less deterministic this would probably not be sufficient to recreate a session.  From the ADAGE Unity client input logging can be simply enabled with a check box and would look like this:

```C#
MarioButtonDown : ADAGEGameEvent
{
	string button_name;
}
```

```C#
MarioButtonUp : ADAGEGameEvent
{
	string button_name;
}
```