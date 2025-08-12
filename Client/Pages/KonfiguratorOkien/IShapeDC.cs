using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

public interface IShapeDC
{
    // Dodane właściwości
    double Szerokosc { get; set; }
    double Wysokosc { get; set; }
    public List<XPoint> Points { get; set; }
    public List<XPoint> GetPoints() => Points;
    Task Draw(Canvas2DContext ctx);
    List<EditableProperty> GetEditableProperties();
    void Scale(double factor);
    void Move(double offsetX, double offsetY);
    void Transform(double scale, double offsetX, double offsetY);
    void Transform(double scaleX, double scaleY, double offsetX, double offsetY);
    BoundingBox GetBoundingBox();
    IShapeDC Clone();
    void UpdatePoints(List<XPoint> newPoints);
}
