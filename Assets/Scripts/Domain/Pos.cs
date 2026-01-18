using System;

public readonly struct Pos : IEquatable<Pos>
{
    public readonly int X, Y;
    public Pos(int x, int y) { X = x; Y = y; }

    public bool Equals(Pos other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is Pos other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Pos a, Pos b) => a.Equals(b);
    public static bool operator !=(Pos a, Pos b) => !a.Equals(b);

    public override string ToString() => $"({X},{Y})";
}
