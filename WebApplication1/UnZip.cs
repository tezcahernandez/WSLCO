using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public class UnZip
    {
        Global g = new Global();
        public bool unzipFiles(string[] nombres)
        {

            for (int i = 0; i < nombres.Length; i++)
            {
                try
                {
                    FileInfo archivo = new FileInfo(g.DirPath + nombres[i]);
                    FileStream ArchivoOriginal = archivo.OpenRead();
                    string NombreArchivo = archivo.FullName;
                    string NuevoNombre = NombreArchivo.Remove(NombreArchivo.Length - archivo.Extension.Length);
                    FileStream ArchivoDescomprimido = File.Create(NuevoNombre + ".xml");
                    GZipStream Descomprimir = new GZipStream(ArchivoOriginal, CompressionMode.Decompress);
                    Descomprimir.CopyTo(ArchivoDescomprimido);
                }
                catch (Exception e)
                {
                    g.WriteLog("ERROR", "Unzip " + e.Message);
                    g.ChangeState(false);
                    return false;
                }
            }
            return true;
        }
    }
}