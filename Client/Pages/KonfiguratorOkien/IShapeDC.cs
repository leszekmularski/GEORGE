using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

public interface IShapeDC
{
    // Identyfikator
    string ID { get; set; }

    // Wymiary nominalne
    double Szerokosc { get; set; }
    double Wysokosc { get; set; }

    // Punkty aktualne (przeskalowane)
    List<XPoint> Points { get; set; }

    // Punkty nominalne (bazowe, do liczenia skali)
    List<XPoint> NominalPoints { get; set; }

    // Pobranie punktów
    List<XPoint> GetPoints();
    List<XPoint> GetNominalPoints();

    // Rysowanie
    Task Draw(Canvas2DContext ctx);

    // Edytowalne właściwości
    List<EditableProperty> GetEditableProperties();

    // Transformacje geometryczne
    void Scale(double factor);
    void Move(double offsetX, double offsetY);
    void Transform(double scale, double offsetX, double offsetY);
    void Transform(double scaleX, double scaleY, double offsetX, double offsetY);

    // Bounding box
    BoundingBox GetBoundingBox();

    // Klonowanie
    IShapeDC Clone();

    // Podmiana punktów (ustawia nominalne i aktualne)
    void UpdatePoints(List<XPoint> newPoints);
}
