using System;

[Serializable]
public abstract class GameCondition
{
    public abstract bool IsMet(
        PlayerGameState gameState);

    public abstract string GetDescription();
}