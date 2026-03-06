using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;
using GEORGE.Shared.ViewModels;

public interface IShapeDC
{
    string ID { get; set; }

    double Szerokosc { get; set; }
    double Wysokosc { get; set; }

    List<XPoint> Points { get; set; }
    List<XPoint> NominalPoints { get; set; }

    List<XPoint> GetPoints();
    List<XPoint> GetNominalPoints();

    // 🔵 NOWE
    List<ContourSegment> ContourSegments { get; }
    List<ContourSegment> GetContourSegments();

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