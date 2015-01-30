using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Threading;
using System.Configuration;

namespace WebApplication1
{
    public class ExecuteCMD {
        Global g;
        
        public ExecuteCMD(){
            g = new Global();
        }

        bool ExecuteCommand(string _Command) {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + @_Command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = false;
                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();
                return true;
            }
            catch (Exception e){
                g.WriteLog("ERROR", "Exec Command " + e.Message);
            }
            g.ChangeState(false);
            return false;
        }

       public bool CleaningFiles(string[] xml, string[] nombres)
        {
            Thread.Sleep(1000);
            string DirOpenSSL = ConfigurationManager.AppSettings["Ruta"];
            for (int i = 0; i < xml.Length; i++)
            {
                string cmd = DirOpenSSL + "openssl smime -decrypt -verify -inform DER -in " + g.DirPath + xml[i] + " -noverify -out " + g.DirPath + nombres[i];
                if (!ExecuteCommand(cmd))
                {
                    g.ChangeState(false);
                    return false;
                }
            }
            return true;
        }
    }
}