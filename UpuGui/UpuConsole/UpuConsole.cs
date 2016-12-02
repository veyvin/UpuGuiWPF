// Decompiled with JetBrains decompiler
// Type: UpuConsole.UpuConsole
// Assembly: UpuGui, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: DD1D21B2-102B-4937-9736-F13C7AB91F14
// Assembly location: C:\Users\veyvin\Desktop\UpuGui.exe

using Microsoft.Win32;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using UpuCore;


    public class UpuConsole
    {
        private KISSUnpacker m_unpacker = new KISSUnpacker();
        private const string m_contextMenuFileType = "Unity package file";
        private const string m_contextMenuShellKey = "Unpack";
        private const string m_contextMenuRegPath = "Unity package file\\shell\\Unpack";
        private const string m_contextMenuCommandRegPath = "Unity package file\\shell\\Unpack\\command";
        private string m_additionalCommandLineArgs;

        public string InputFile { get; private set; }

        public string OutputPath { get; private set; }

        public bool Register { get; private set; }

        public bool Unregister { get; private set; }

        internal int Start()
        {
            OptionSet p = new OptionSet()
      {
        {
          "i=|input=",
          "Unitypackage input file.",
          (Action<string>) (i => this.InputFile = i)
        }
      }.Add("o:|output:", "The output path of the extracted unitypackage.", (Action<string>)(o => this.OutputPath = o)).Add("r|register", "Register context menu handler", (Action<string>)(r => this.Register = r != null)).Add("u|unregister", "Unregister context menu handler", (Action<string>)(u => this.Unregister = u != null));
            p.Add("h|help", "Show help", (Action<string>)(h => Console.WriteLine(this.GetUsage(p))));
            p.Parse((IEnumerable<string>)Environment.GetCommandLineArgs());
            if (string.IsNullOrEmpty(this.InputFile) && !this.Register && !this.Unregister)
            {
                Console.WriteLine(this.GetUsage(p));
                return 1;
            }
            if (!string.IsNullOrEmpty(this.InputFile) && !File.Exists(this.InputFile))
            {
                Console.WriteLine("File not found: " + this.InputFile);
                Console.WriteLine(this.GetUsage(p));
                return 2;
            }
            if (!string.IsNullOrEmpty(this.InputFile))
            {
                this.DoUnpack(this.InputFile);
                return 0;
            }
            return (this.Register || this.Unregister) && !this.RegisterUnregisterShellHandler(this.Register) ? 1 : 0;
        }

        public bool RegisterUnregisterShellHandler(bool register)
        {
            if (register && this.IsContextMenuHandlerRegistered() || !register && !this.IsContextMenuHandlerRegistered())
                return true;
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) && !string.Join(" ", Environment.GetCommandLineArgs()).Contains("--elevated"))
            {
                if (Environment.GetCommandLineArgs().Length == 1)
                    this.m_additionalCommandLineArgs = !register ? "-u" : "-r";
                return this.RunElevatedAsAdmin() == 0;
            }
            this.RegisterShellHandler(this.Register);
            return true;
        }

        private int RunElevatedAsAdmin()
        {
            string str = "";
            for (int index = 1; index < Environment.GetCommandLineArgs().Length; ++index)
                str = str + Environment.GetCommandLineArgs()[index] + " ";
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
            startInfo.Arguments = str + " " + this.m_additionalCommandLineArgs + " --elevated";
            startInfo.Verb = "runas";
            try
            {
                Process.Start(startInfo);
                return 0;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine((object)ex);
                return -1;
            }
        }

        private bool RegisterShellHandler(bool register)
        {
            try
            {
                if (register)
                    this.RegisterShellHandler("Unity package file", "Unpack", "Unpack here", string.Format("\"{0}\" \"%L\"", (object)Assembly.GetEntryAssembly().Location));
                else
                    this.UnregisterShellHandler("Unity package file", "Unpack");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                if (this.Register)
                    Console.WriteLine("Error: UnauthorizedAccessException. Cannot register explorer context menu handler!");
                if (this.Unregister)
                    Console.WriteLine("Error: UnauthorizedAccessException. Cannot register explorer context menu handler!");
            }
            return false;
        }

        public string GetUsage(OptionSet p)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            using (StringWriter stringWriter = new StringWriter())
            {
                p.WriteOptionDescriptions((TextWriter)stringWriter);
                stringBuilder.AppendLine(stringWriter.ToString());
            }
            stringBuilder.AppendLine("Help us make to this piece of software even better and contribute!");
            stringBuilder.AppendLine("https://github.com/ChimeraEntertainment/UPU");
            return stringBuilder.ToString();
        }

        private void RegisterShellHandler(string fileType, string shellKeyName, string menuText, string menuCommand)
        {
            using (RegistryKey subKey = Registry.ClassesRoot.CreateSubKey("Unity package file\\shell\\Unpack"))
                subKey.SetValue((string)null, (object)menuText);
            using (RegistryKey subKey = Registry.ClassesRoot.CreateSubKey("Unity package file\\shell\\Unpack\\command"))
                subKey.SetValue((string)null, (object)menuCommand);
        }

        private void UnregisterShellHandler(string fileType, string shellKeyName)
        {
            if (string.IsNullOrEmpty(fileType) || string.IsNullOrEmpty(shellKeyName) || !this.IsContextMenuHandlerRegistered())
                return;
            Registry.ClassesRoot.DeleteSubKeyTree("Unity package file\\shell\\Unpack");
        }

        public bool IsContextMenuHandlerRegistered()
        {
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("Unity package file\\shell\\Unpack\\command");
            return registryKey != null && registryKey.GetValue((string)null) != null;
        }

        internal void DoUnpack(string fileName)
        {
            try
            {
                this.m_unpacker.RemapFiles(this.m_unpacker.Unpack(this.InputFile, this.OutputPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine("==========================================");
                Console.WriteLine((object)ex);
                Console.WriteLine("==========================================");
                if (!Environment.UserInteractive)
                    return;
                Console.WriteLine("An error occured (see above)!");
            }
        }
    }

