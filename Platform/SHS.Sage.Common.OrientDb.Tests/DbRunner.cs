﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SHS.Sage.OrientDb.Tests
{
    public class DbRunner
    {
        private static Process _process;


        public static void StartOrientDb(string dbDir, string javaDir)
        {
            _process = new Process();
            var path = Path.Combine(dbDir, @"bin\server.bat");

            _process.StartInfo.FileName = "cmd.exe";
            _process.StartInfo.Arguments = @"/K " + path;
            _process.StartInfo.CreateNoWindow = false;
            _process.StartInfo.UseShellExecute = false;
            if (_process.StartInfo.EnvironmentVariables.ContainsKey("ORIENTDB_HOME"))
                _process.StartInfo.EnvironmentVariables["ORIENTDB_HOME"] = dbDir;
            else
                _process.StartInfo.EnvironmentVariables.Add("ORIENTDB_HOME", dbDir);
            if (_process.StartInfo.EnvironmentVariables.ContainsKey("JAVA_HOME"))
                _process.StartInfo.EnvironmentVariables["JAVA_HOME"] = javaDir;
            else
                _process.StartInfo.EnvironmentVariables.Add("JAVA_HOME", javaDir);
            _process.StartInfo.WorkingDirectory = dbDir;
            //_process.StartInfo.RedirectStandardError = true;
            //_process.StartInfo.RedirectStandardOutput = true;
            _process.Start();

            Thread.Sleep(1000);

            //var output = _process.StandardOutput.ReadToEnd();
            //var error = _process.StandardError.ReadToEnd();

            if (_process.HasExited)
                throw new Exception("OrientDB did not start correctly");

        }


        public static void StopOrientDb()
        {
            ProcessUtilities.KillProcessTree(_process);
        }
    }

    class ProcessUtilities
    {
        public static void KillProcessTree(Process root)
        {
            if (root != null)
            {
                var list = new List<Process>();
                GetProcessAndChildren(Process.GetProcesses(), root, list, 1);

                foreach (Process p in list)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception ex)
                    {
                        //Log error?
                    }
                }
            }
        }

        private static int GetParentProcessId(Process p)
        {
            int parentId = 0;
            try
            {
                ManagementObject mo = new ManagementObject("win32_process.handle='" + p.Id + "'");
                mo.Get();
                parentId = Convert.ToInt32(mo["ParentProcessId"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                parentId = 0;
            }
            return parentId;
        }

        private static void GetProcessAndChildren(Process[] plist, Process parent, List<Process> output, int indent)
        {
            foreach (Process p in plist)
            {
                if (GetParentProcessId(p) == parent.Id)
                {
                    GetProcessAndChildren(plist, p, output, indent + 1);
                }
            }
            output.Add(parent);
        }
    }
}
