using Blazor.Extensions.Canvas.Canvas2D;
using GEORGE.Client.Pages.KonfiguratorOkien;

public interface IShapeDC
{
    Task Draw(Canvas2DContext ctx);
    List<EditableProperty> GetEditableProperties();
    void Scale(double factor);
    void Move(double offsetX, double offsetY);
    void Transform(double scale, double offsetX, double offsetY);
    void Transform(double scaleX, double scaleY, double offsetX, double offsetY);
    BoundingBox GetBoundingBox();
}
