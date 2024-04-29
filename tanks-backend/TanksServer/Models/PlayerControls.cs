using System.Text;

namespace TanksServer.Models;

internal sealed class PlayerControls
{
    public bool Forward { get; set; }
    public bool Backward { get; set; }
    public bool TurnLeft { get; set; }
    public bool TurnRight { get; set; }
    public bool Shoot { get; set; }


    public string ToDisplayString()
    {
        var str = new StringBuilder("[ ");
        if (Forward)
            str.Append("▲ ");
        if (Backward)
            str.Append("▼ ");
        if (TurnLeft)
            str.Append("◄ ");
        if (TurnRight)
            str.Append("► ");
        if (Shoot)
            str.Append("• ");
        str.Append(']');
        return str.ToString();
    }
}
