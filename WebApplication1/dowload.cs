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
    public class dowload
    {
        Global lc = new Global();

        public bool DownloadFiles(string[] nombres)
        {
            string _url = @"http://www.gestionix.com/Zip/";
            using (WebClient ClienteWeb = new WebClient())
            {
                try
                {
                    for (int i = 0; i < nombres.Length; i++)
                    {
                        ClienteWeb.Proxy = null;
                        ClienteWeb.DownloadFile(_url + nombres[i],lc.DirPath + nombres[i]);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    lc.WriteLog("ERROR", "Download " + ex.Message);
                }
            }
            lc.ChangeState(false);
            return false;
        }
    }
}