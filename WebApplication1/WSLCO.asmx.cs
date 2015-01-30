using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebApplication1
{
    /// <summary>
    /// Summary description for WSLCO
    /// </summary>
    [WebService(Namespace = "http://localhost/wslco")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WSLCO : System.Web.Services.WebService
    {

        [WebMethod]
        public void UpdateLCO()
        {
            ProcesoLCO proc = new ProcesoLCO();
            proc.Main();
        }
        [WebMethod]
        public bool GetStatus()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(@"D:\GITHUB\status.txt");
            string res = file.ReadLine();
            file.Close();
            if (res == "True") return true;
            return false;
        }
    }
}
