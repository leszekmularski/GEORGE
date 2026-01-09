using GEORGE.Client.Pages.Models;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class ConstWlasciwosciOkna
    {
        public int IdZmiany { get; set; }
        public List<XPoint>? Wierzcholki { get; set; }
        public List<XPoint>? WierzcholkiWartosciNominalne { get; set; }
        public MVCKonfModele? MVCKonfModelu { get; set; }
        public KonfModele? WybranyModel { get; set; } 
        public List<DaneKwadratu>? ListaKwadratow { get; set; }
        public int Szerokosc { get; set; } = 1250;
        public int Wysokosc { get; set; } = 1000;
        public string KolorZewnetrzny { get; set; }
        public string KolorWewnetrzny { get; set; }
        public string KolorSzyby { get; set; }
        public string TypSzyby { get; set; }
        public float GruboscPakietu { get; set; }
        public float WspolczynnikPrzepuszczalnosciCieplaSzyby { get; set; }
        public float WspolczynnikPrzepuszczalnosciCieplaOkna { get; set; }
        public string TypKlamki { get; set; }
        public string TypUszczelek { get; set; }
        public string TypOslonPrzeciwsłonecznych { get; set; }
        public string TypMontazu { get; set; }

        public const string DomyslnyKolorZewnetrzny = "Bialy";
        public const string DomyslnyKolorWewnetrzny = "Bialy";

        public const string DomyslnyKolorSzyby = "Przezroczysta";
        public const string DomyslnyTypSzyby = "Standardowa";
        public const string DomyslnyTypKlamki = "Standardowa";
        public const string DomyslnyTypUszczelek = "Standardowe";
        public const string DomyslnyTypOslonPrzeciwsłonecznych = "Brak";
        public const string DomyslnyTypMontazu = "Standard";


        public ConstWlasciwosciOkna()
        {
            IdZmiany = 0;
            Wierzcholki = new List<XPoint>();
            WierzcholkiWartosciNominalne = new List<XPoint>();
            MVCKonfModelu = new MVCKonfModele();
            WybranyModel = new KonfModele();
            ListaKwadratow = new List<DaneKwadratu>();
            Szerokosc = 1250;
            Wysokosc = 1000;
            KolorZewnetrzny = DomyslnyKolorZewnetrzny;
            KolorWewnetrzny = DomyslnyKolorWewnetrzny;
            KolorSzyby = DomyslnyKolorSzyby;
            TypSzyby = DomyslnyTypSzyby;
            GruboscPakietu = 0;
            WspolczynnikPrzepuszczalnosciCieplaSzyby = 0;
            WspolczynnikPrzepuszczalnosciCieplaOkna = 0;
            TypKlamki = DomyslnyTypKlamki;
            TypUszczelek = DomyslnyTypUszczelek;
            TypOslonPrzeciwsłonecznych = DomyslnyTypOslonPrzeciwsłonecznych;
            TypMontazu = DomyslnyTypMontazu;
        }

    }

}
