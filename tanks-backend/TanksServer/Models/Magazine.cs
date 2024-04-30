using System.Text;

namespace TanksServer.Models;

[Flags]
internal enum MagazineType
{
    Basic = 0,
    Fast = 1 << 0,
    Explosive = 1 << 1,
    Smart = 1 << 2,
}

internal readonly record struct Magazine(MagazineType Type, byte UsedBullets, byte MaxBullets)
{
    public bool Empty => UsedBullets >= MaxBullets;

    public string ToDisplayString()
    {
        var sb = new StringBuilder();

        if (Type.HasFlag(MagazineType.Fast))
            sb.Append("» ");
        if (Type.HasFlag(MagazineType.Explosive))
            sb.Append("* ");
        if (Type.HasFlag(MagazineType.Smart))
            sb.Append("@ ");

        sb.Append("[ ");
        for (var i = 0; i < UsedBullets; i++)
            sb.Append("\u25cb ");
        for (var i = UsedBullets; i < MaxBullets; i++)
            sb.Append("• ");
        sb.Append(']');

        return sb.ToString();
    }
}
