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
        bool pionPoziom = false)
    {
        public double Value
        {
            get => GetValue();
            set
            {
                if (!IsReadOnly) SetValue(value);
            }
        }

        public EditableProperty SetObjectName(string newName) => this with { NazwaObiektu = newName };
    }


}
