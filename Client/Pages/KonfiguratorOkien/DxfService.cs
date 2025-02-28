using System.IO;
using netDxf;
using netDxf.Entities;

namespace GEORGE.Client.Pages.KonfiguratorOkien
{
    public class DxfService
    {
        public DxfDocument LoadDxf(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Plik DXF nie został znaleziony!");

            return DxfDocument.Load(path);
        }
    }

}
