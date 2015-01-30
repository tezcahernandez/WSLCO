using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Xml;

namespace WebApplication1
{
    public class ProcesoLCO
    {
        string DirPath;
        List<Contribuyente> _lista;

        public void Main() {
            DirPath = HostingEnvironment.ApplicationPhysicalPath;
            DirPath += @"Files\";
            string[] dwFiles = new string[4] { "A1.gz", "A2.gz", "A3.gz", "A4.gz" };
            string[] XMLFiles = new string[4] { "A1.xml", "A2.xml", "a3.xml", "a4.xml" };
            string[] XMLFiles1 = new string[4] { "lco1.xml", "lco2.xml", "lco3.xml", "lco4.xml" };
            ChangeState(true);
            this.WriteLog("STEP", "START");

            WriteLog("STEP", "dw");
            if (!DownloadFiles(dwFiles)) return;

            WriteLog("STEP", "unzip");
            if(!unzipFiles(dwFiles))return;

            WriteLog("STEP", "clean");
            if(!CleaningFiles(XMLFiles, XMLFiles1)) return;

            WriteLog("STEP", "xmlwrite");
            foreach(string fPath in XMLFiles1){
                if (!LoadXML2List(fPath)) return;
                if(!CreaXML(DirPath + "final.txt")) return;
            }
            //if(!ExecuteSP()) return;
            if (!spExecute("spExecuteBULK")) return;
            this.WriteLog("STEP", "DONE");
        }
        bool DownloadFiles(string[] nombres)
        {
            string _url = @"http://www.gestionix.com/Zip/";
            using (WebClient ClienteWeb = new WebClient())
            {
                try
                {
                    for (int i = 0; i < nombres.Length; i++)
                    {
                        ClienteWeb.Proxy = null;
                        ClienteWeb.DownloadFile(_url + nombres[i], DirPath + nombres[i]);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    WriteLog("ERROR", "Download "+ex.Message);
                }
            }
            ChangeState(false);
            return false;
        }
        bool unzipFiles(string[] nombres)
        {
            
            for (int i = 0; i < nombres.Length; i++)
            {
                try
                {
                    FileInfo archivo = new FileInfo(DirPath + nombres[i]);
                    FileStream ArchivoOriginal = archivo.OpenRead();
                    string NombreArchivo = archivo.FullName;
                    string NuevoNombre = NombreArchivo.Remove(NombreArchivo.Length - archivo.Extension.Length);
                    FileStream ArchivoDescomprimido = File.Create(NuevoNombre + ".xml");
                    GZipStream Descomprimir = new GZipStream(ArchivoOriginal, CompressionMode.Decompress);
                    Descomprimir.CopyTo(ArchivoDescomprimido);
                }
                catch (Exception e)
                {
                    WriteLog("ERROR","Unzip " + e.Message);
                    ChangeState(false);
                    return false;
                }
            }
            return true;
        }
        bool ExecuteCommand(string _Command)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + @_Command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = false;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();
                return true;
            }
            catch (Exception e)
            {
                WriteLog("ERROR","Exec Command " + e.Message);
            }
            ChangeState(false);
            return false;
        }
        bool CleaningFiles(string[] xml, string[] nombres)
        {
            Thread.Sleep(1000);
            string DirOpenSSL = ConfigurationManager.AppSettings["Ruta"];
            for (int i = 0; i < xml.Length; i++)
            {
                string cmd = DirOpenSSL + "openssl smime -decrypt -verify -inform DER -in " + DirPath + xml[i] + " -noverify -out " + DirPath + nombres[i];
                if (!ExecuteCommand(cmd)) 
                { 
                    ChangeState(false); 
                    return false; 
                }
            }
            return true;
        }
        bool ExecuteSP()
        {
            List<String> SPNames = new List<String>();
            SPNames.Add("XMLIMPORT1");
            SPNames.Add("XMLIMPORT2");
            SPNames.Add("XMLIMPORT3");
            SPNames.Add("XMLIMPORT4");
            foreach (String _SP in SPNames)
            {
                if (!spExecute(_SP)) { 
                    ChangeState(false); 
                    return false; 
                }
            }
            return true;
        }
        bool spExecute(string spName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conexionLCD"].ConnectionString;
            using (SqlConnection dbcon1 = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand comand = new SqlCommand(spName, dbcon1);
                    comand.CommandTimeout = 200;
                    comand.CommandType = CommandType.StoredProcedure;
                    dbcon1.Open();
                    WriteLog("STEP","SP Executing " + spName);
                    comand.ExecuteNonQuery();
                    comand.Dispose();
                }
                catch (Exception ex)
                {
                    WriteLog("ERROR", "SP Exec " + ex.Message);
                    ChangeState(false);
                    return false;
                }
            }
            return true;
        }
        void WriteLog(string type, string linea) 
        {
            string fic = DirPath+"log.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fic,true);
            sw.WriteLine(DateTime.Now.ToString() +" "+ type+ ": "+ linea);
            sw.Close();
        }
        void ChangeState(bool state)
        {
            string fic = DirPath+"status.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fic);
            sw.WriteLine(state);
            sw.Close();
        }
        bool LoadXML2List(string file)
        {
            Contribuyente Persona;
            _lista = new List<Contribuyente>();
            string _rfc, _ValidezObligaciones, _EstatusVerficado, _noCertificado, _fInicio, _fFinal = "";
            try
            {
                using (XmlTextReader reader = new XmlTextReader(DirPath+file))
                {
                    _rfc = "";
                    reader.MoveToContent();
                    while (reader.Read())
                    {
                        _ValidezObligaciones = "";
                        _EstatusVerficado = "";
                        _noCertificado = "";
                        _fInicio = "";
                        _fFinal = "";
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "lco:Contribuyente"))
                        {
                            _rfc = reader.GetAttribute(0);
                        }
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "lco:Certificado"))
                        {
                            _ValidezObligaciones = reader.GetAttribute(0);
                            _EstatusVerficado = reader.GetAttribute(1);
                            _noCertificado = reader.GetAttribute(2);
                            _fFinal = reader.GetAttribute(3);
                            _fInicio = reader.GetAttribute(4);
                            Persona = new Contribuyente(_rfc, _ValidezObligaciones, _EstatusVerficado, _noCertificado, _fFinal, _fInicio);
                            _lista.Add(Persona);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
    }
        public bool CreaXML(string FilePath)
        {
            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(FilePath, true))
                {
                    foreach (Contribuyente persona in _lista)
                    {
                        sw.WriteLine(persona.ToString());
                    }
                    sw.Close();
                }
                _lista = null;
                return true;
            }catch(Exception e){
                WriteLog("ERROR","WRITE TEXT "+e.Message);
            }
            _lista = null;
            return false;
        }
    }
    public class Contribuyente
    {
        public string _RFC;
        public string _ValidezObligaciones;
        public string _EstatusVerficado;
        public string _noCertificado;
        public string _fInicio;
        public string _fFinal;
        public Contribuyente()
        {
            _RFC = null;
        }
        public Contribuyente(string RFC, string ValidezObligaciones, string EstatusVerficado, string noCertificado, string fFinal, string fInicio)
        {
            _RFC = RFC;
            _ValidezObligaciones = ValidezObligaciones;
            _EstatusVerficado = EstatusVerficado;
            _noCertificado = noCertificado;
            _fInicio = fInicio;
            _fFinal = fFinal;
        }
        public override string ToString()
        {
            return _RFC + "|" + _ValidezObligaciones + "|" + _EstatusVerficado + "|" + _noCertificado + "|" + _fFinal + "|" + _fInicio;
        }
    }
}