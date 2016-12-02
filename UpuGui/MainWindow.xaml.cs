using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using UpuCore;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
//using System.Windows.Forms;


namespace UpuGui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KISSUnpacker m_ku;
        private Dictionary<string, string> m_remapInfo;
        private readonly Timer m_shellHandlerCheckTimer;
        private string m_tmpUnpackedOutputPathForUi;
        private readonly UpuConsole m_upu;
        private readonly OpenFileDialog openFileDialog;
        private FolderBrowserDialog saveToFolderDialog;

        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            m_ku = new KISSUnpacker();
            m_upu = new UpuConsole();
            m_shellHandlerCheckTimer = new Timer();
            m_shellHandlerCheckTimer.Interval = 5000;
            m_shellHandlerCheckTimer.Tick += ShellHandlerCheckTimer_Tick;
            m_shellHandlerCheckTimer.Enabled = true;
            m_shellHandlerCheckTimer.Start();
            ShellHandlerCheckTimer_Tick(null, EventArgs.Empty);
        }

        private void ShellHandlerCheckTimer_Tick(object sender, EventArgs e)
        {
            if (m_upu.IsContextMenuHandlerRegistered())
                btnRegister.Content = "Unregister Explorer Context Menu Handler";
            else
                btnRegister.Content = "Register Explorer Context Menu Handler";
            btnRegister.IsEnabled = true;
        }

        private void OpenFile(string filePathName)
        {
            groupBox.Header = new FileInfo(filePathName).Name;
            btnExit.IsEnabled = false;
            btnSelect.IsEnabled = false;
            progressBar.IsEnabled = true;
            treeView.Items.Clear();
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += ReadInputFileWorker;
            backgroundWorker.RunWorkerCompleted += ReadInputFileWorkerCompleted;
            backgroundWorker.RunWorkerAsync(filePathName);
        }

        private void ReadInputFileWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                var num =
                    (int)
                    MessageBox.Show("An exception happened: \n" + e.Result, "Ooops...", MessageBoxButtons.OK,
                        MessageBoxIcon.Hand);
            }
            else
            {
                foreach (var node in (List<TreeNode>) e.Result)
                    treeView.Items.Add(node);
            }
            progressBar.IsEnabled = false;
            btnSelect.IsEnabled = true;
            btnUnPack.IsEnabled = true;
            btnSelectAll.IsEnabled = true;
            btnDeselectAll.IsEnabled = true;
            btnExit.IsEnabled = true;
        }

        private void ReadInputFileWorker(object sender, DoWorkEventArgs e)
        {
            var list = new List<TreeNode>();
            try
            {
                m_tmpUnpackedOutputPathForUi = m_ku.GetTempPath();
                m_remapInfo = m_ku.Unpack(e.Argument.ToString(), m_tmpUnpackedOutputPathForUi);
                foreach (var keyValuePair in m_remapInfo)
                    if (File.Exists(keyValuePair.Key))
                    {
                        var text = keyValuePair.Value.Replace(m_tmpUnpackedOutputPathForUi, "");
                        if (text.StartsWith(Path.DirectorySeparatorChar.ToString()))
                            text = text.Substring(1);
                        list.Add(new TreeNode(text)
                        {
                            Checked = true,
                            Tag = keyValuePair
                        });
                    }
                list.Sort((t1, t2) => t1.Text.CompareTo(t2.Text));
            }
            catch (Exception ex)
            {
                e.Result = ex;
                return;
            }
            e.Result = list;
        }

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            if (m_remapInfo == null)
                return;
            var num = (int) saveToFolderDialog.ShowDialog();
            if (string.IsNullOrEmpty(saveToFolderDialog.SelectedPath))
                return;
            btnSelect.IsEnabled = false;
            btnUnPack.IsEnabled = false;
            btnExit.IsEnabled = false;
            progressBar.IsEnabled = true;
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += UnpackInputFileWorker;
            backgroundWorker.RunWorkerCompleted += UnpackInputFileWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void UnpackInputFileWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnSelect.IsEnabled = true;
            btnUnPack.IsEnabled = true;
            btnExit.IsEnabled = true;
            progressBar.IsEnabled = false;
        }

        private void UnpackInputFileWorker(object sender, DoWorkEventArgs e)
        {
            if (!Directory.Exists(saveToFolderDialog.SelectedPath))
                Directory.CreateDirectory(saveToFolderDialog.SelectedPath);
            var map = new Dictionary<string, string>();
            var dictionary = new Dictionary<string, string>();
            foreach (TreeNode treeNode in treeView.Items)
                if (treeNode.Checked)
                    dictionary.Add(((KeyValuePair<string, string>) treeNode.Tag).Key,
                        ((KeyValuePair<string, string>) treeNode.Tag).Value);
            foreach (var keyValuePair in dictionary)
                map[keyValuePair.Key] = keyValuePair.Value.Replace(m_tmpUnpackedOutputPathForUi,
                    saveToFolderDialog.SelectedPath);
            m_ku.RemapFiles(map);
        }


        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {

            foreach (TreeNode treeNode in treeView.Items)
            {
                treeNode.Checked = true;
                
            }
               
           
        }

        private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TreeNode treeNode in treeView.Items)
                treeNode.Checked = false;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            m_upu.RegisterUnregisterShellHandler(!m_upu.IsContextMenuHandlerRegistered());
            btnRegister.IsEnabled = false;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "Unitypackage Files|*.unitypackage";
            openFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(openFileDialog.FileName))
                return;
            OpenFile(openFileDialog.FileName);
        }

     

        private void Initialized_Test(object sender, EventArgs e)
        {

        }
    }
}