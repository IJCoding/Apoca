using System;

[Serializable]
public abstract class GameEffect
{
    public abstract void Apply(
        PlayerGameState gameState);

    public abstract string GetDescription();
}