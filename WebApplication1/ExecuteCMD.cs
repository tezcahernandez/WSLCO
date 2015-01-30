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
         
        string DirPath;
        Global messages;
        
        public ExecuteCMD(string path){
            DirPath = path;
            messages = new Global();
        }

        public bool ExecuteCommand(string _Command) {
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
                messages.WriteLog("ERROR", "Exec Command " + e.Message);
            }
            messages.ChangeState(false);
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
                    messages.ChangeState(false);
                    return false;
                }
            }
            return true;
        }
    }
}