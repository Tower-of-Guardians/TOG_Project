using System;

[System.Serializable]
public readonly struct ActionData
{
    private readonly int _currentActionCount;
    public int Current => _currentActionCount;

    private readonly int _maxActionCount;
    public int Max => _maxActionCount;

    public ActionData(int currentActionCount,
                      int maxActionCount)
    {
        _currentActionCount = currentActionCount;
        _maxActionCount = maxActionCount;
    }
}