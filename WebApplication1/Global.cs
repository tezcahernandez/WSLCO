using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace WebApplication1
{
    public class Global
    {
        string DirPath;
        public Global(){
            DirPath = HostingEnvironment.ApplicationPhysicalPath;
            DirPath += @"Files\";
        }
        public void WriteLog(string type, string linea)
        {
            string fic = DirPath + "log.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fic, true);
            sw.WriteLine(DateTime.Now.ToString() + " " + type + ": " + linea);
            sw.Close();
        }
        public void ChangeState(bool state)
        {
            string fic = DirPath + "status.txt";
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fic);
            sw.WriteLine(state);
            sw.Close();
        }
    }
}