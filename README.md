# Instructions
When moving upward with jumps remaining, jumping works as vanilla.
When moving upward without jumps remaining, pressing jump will automatically enable hover when Artificer begins falling.
When moving downward without jumps remaining, pressing jump will enable/disable hover.
When moving downward with jumps remaining, the jump state is replaced with a timer that tracks whether or not jump is released before a configurable interval. If it's released early, Artificer will jump; otherwise, hover is enabled/disabled without jumping.

Eventually, I'll rewrite the upward movement jump code to allow for holding jump -> automatic hover

Feel free to use any code from this in your own projects; please credit me if you do, though.
