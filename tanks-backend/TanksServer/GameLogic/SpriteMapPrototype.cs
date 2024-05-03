using System.IO;
using TanksServer.Graphics;

namespace TanksServer.GameLogic;

internal sealed class SpriteMapPrototype : MapPrototype
{
    private readonly string _name;
    private readonly Sprite _sprite;

    public SpriteMapPrototype(string name, Sprite sprite)
    {
        if (sprite.Width != MapService.PixelsPerRow || sprite.Height != MapService.PixelsPerColumn)
            throw new FileLoadException($"invalid image size in file {_name}");

        _name = name;
        _sprite = sprite;
    }

    public override Map CreateInstance() => new(_name, _sprite.ToBoolArray());
}
