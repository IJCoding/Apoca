public readonly struct MoveResolution
{
    public int DieOne { get; }

    public int DieTwo { get; }

    public int Modifier { get; }

    public int Total { get; }

    public MoveResult Result { get; }

    public MoveResolution(
        int dieOne,
        int dieTwo,
        int modifier,
        MoveResult result)
    {
        DieOne = dieOne;
        DieTwo = dieTwo;
        Modifier = modifier;
        Total =
            dieOne +
            dieTwo +
            modifier;

        Result = result;
    }
}