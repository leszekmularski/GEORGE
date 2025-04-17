namespace GEORGE.Client.Pages.Models
{
    // 🖌️ KLASA LINII
    public class XLineShape
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public string NazwaObj { get; set; } = "Linia";
        public bool RuchomySlupek { get; set; } = false;
        public bool PionPoziom { get; set; } = false;
        public bool DualRama { get; set; } = false;


    }
}
