using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Reference;

/// <summary>ANSI Z535-style safety sign signal word with header colours. Verify against the current edition.</summary>
public sealed class SignalWord : Entity
{
    private SignalWord()
    {
    }

    public SignalWord(string word, string headerColor, string textColor)
    {
        Word = word;
        HeaderColor = headerColor;
        TextColor = textColor;
    }

    public string Word { get; private set; } = string.Empty;

    public string HeaderColor { get; private set; } = string.Empty;

    public string TextColor { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;
}
