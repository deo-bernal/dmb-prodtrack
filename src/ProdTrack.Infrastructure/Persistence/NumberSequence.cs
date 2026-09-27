namespace ProdTrack.Infrastructure.Persistence;

/// <summary>Yearly business number counter (BR-01). Provider-neutral alternative to a SQL SEQUENCE per year.</summary>
public sealed class NumberSequence
{
    private NumberSequence()
    {
    }

    public NumberSequence(string name, int year)
    {
        Name = name;
        Year = year;
    }

    public string Name { get; private set; } = string.Empty;

    public int Year { get; private set; }

    public int LastValue { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public int Next() => ++LastValue;
}
