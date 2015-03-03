#Tic Tac Toe

###_An example of using ADAGE to log game data (in Unity-C#)_

This implementation of Tic Tac Toe consists of only one player controlled actor and the other decisions are made by an AI. The player is allowed the option of picking the size of the grid. Starting from the standard 3 by 3 and working up to a 6 by 6. 

Structure that is logged when the player starts a new match. The additional fields we added for this game record the size of the grid and if the player gets the first move.

```C#
TTTMatchStart : ADAGEContextStart 
{ 
	int grid_size;
	bool player_starts;
} 
```
Structure logged at the end of a match. The Success field would be filled in as true is the player won the match and false if the player lost or it is a draw. Draw is set to false if there is a winner, true if there is no winner. 

```C#
TTTEndMatch : ADAGEContextEnd 
{ 
	bool draw;
} 
```

The player is presented with a choice of marker to use on the board. This log records which marker (e.g. “X” or “O”) they chose. 

```C#
TTTPlayerMarkerChoice : ADAGEGameEvent 
{ 
	string marker;
} 
```

Tic Tac Toe is a 2D game the positional context isn’t really necessary. All events will be logged as a subclass of the ADAGEGameEvent. The position is the location on the grid that the player or AI played their marker. The grid is indexed from 0,0 to n,n (in this case, n=2) starting in the upper left to lower right. For example the topmost, leftmost grid position is 0,0. 

```C#
TTTPlayerMove : ADAGEGameEvent 
{ 
	Vector2 position;
} 
```

```C#
TTTAIMove : ADAGEGameEvent
{ 
	Vector2 position;
} 
```

The entire state for the grid is logged after every move in grid_state. The grid is initialized with all zeros. All the player moves are represented with ones and the AI moves with 2s. 

```C#
TTTGridState : ADAGEGameEvent 
{ 
	int grid_state[][];
}
```