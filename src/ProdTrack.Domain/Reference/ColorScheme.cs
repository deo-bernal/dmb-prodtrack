using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Reference;

/// <summary>ASME A13.1-style pipe marker colour scheme (reference data, editable by Admin). Verify against the current edition.</summary>
public sealed class ColorScheme : Entity
{
    private ColorScheme()
    {
    }

    public ColorScheme(string code, string name, string textColor, string backgroundColor)
    {
        Code = code;
        Name = name;
        TextColor = textColor;
        BackgroundColor = backgroundColor;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string TextColor { get; private set; } = string.Empty;

    public string BackgroundColor { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;
}
