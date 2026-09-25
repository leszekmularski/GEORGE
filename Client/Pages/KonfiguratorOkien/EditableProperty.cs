namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    // /Models/EditableProperty.cs

    public record EditableProperty(
        string Label,
        Func<double> GetValue,
        Action<double> SetValue,
        string NazwaObiektu,
        bool IsReadOnly = false,
        bool slupekRuchomy = false,
        bool pionPoziom = false,
        bool systemowa = false,
        bool gabarytOkna = false,
        string? ShapeId = null)
    {
        public double Value
        {
            get => Math.Round(GetValue(), 1);
            set
            {
                if (!IsReadOnly) SetValue(value);
            }
        }
        public EditableProperty SetObjectName(string newName) => this with { NazwaObiektu = newName };
        public bool IsSkosna { get; init; } = false;
        public bool IsPionowa { get; init; } = false;   // linia pionowa (X1 == X2)
        public bool IsPozioma { get; init; } = false;   // linia pozioma (Y1 == Y2)
        public string? SplitGroupId { get; init; }
    }

}
