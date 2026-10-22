public enum GameColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink,
    White,
    Emerald,
    StrongBlue,
    Black
}

[System.Flags]
public enum BoxType
{
    None = 0,
    Normal = 1 << 0,
    Large = 1 << 1,
    Secret = 1 << 2,
    Star = 1 << 3,
    Connect = 1 << 4,
}
