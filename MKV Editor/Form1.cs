using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKV_Editor
{
    public partial class Form1 : Form
    {
        private string mkvmerge_fileName = "";
        private string input_folder = "";
        private string output_folder = "";
        private List<string> input_folderFiles = new List<string>();
        private bool IsSomeFolder = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "mkvmerge.exeを選択してください。";
            openFileDialog1.FileName = "mkvmerge.exe";
            openFileDialog1.Filter = "mkvmerge.exe|mkvmerge.exe";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.mkvmerge = mkvmerge_fileName = textBox1.Text = openFileDialog1.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "mkvがあるフォルダを指定してください。";
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                input_folderFiles.Clear();
                input_folder = textBox2.Text = folderBrowserDialog1.SelectedPath;
                IEnumerable<string> files;
                if (checkBox5.Checked)
                    files = Directory.EnumerateFiles(folderBrowserDialog1.SelectedPath, "*", SearchOption.AllDirectories);
                else
                    files = Directory.EnumerateFiles(folderBrowserDialog1.SelectedPath, "*", SearchOption.TopDirectoryOnly);
                foreach (string f in files)
                    if (Path.GetExtension(f) == ".mkv")
                        input_folderFiles.Add(Path.GetFileName(f));
                if (input_folderFiles.Count == 0)
                {
                    MessageBox.Show("mkvがあるフォルダを指定してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button4.Enabled = false;
                }
                else
                {
                    button4.Enabled = true;
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "出力先フォルダを指定してください。";
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (input_folder == folderBrowserDialog1.SelectedPath)
                {
                    if (MessageBox.Show("mkvがあるフォルダと同じフォルダですが、よろしいですか？\r\n\r\n(ファイル名)_2.mkvといった感じになります。", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        IsSomeFolder = true;
                        output_folder = textBox3.Text = folderBrowserDialog1.SelectedPath;
                    }
                    else MessageBox.Show("キャンセルされました。", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    output_folder = textBox3.Text = folderBrowserDialog1.SelectedPath;
                    IsSomeFolder = false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.mkvmerge))
                mkvmerge_fileName = textBox1.Text = Properties.Settings.Default.mkvmerge;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(mkvmerge_fileName)) { MessageBox.Show("mkvmerge.exeを指定してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (string.IsNullOrWhiteSpace(input_folder)) { MessageBox.Show("mkvがあるフォルダを指定してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (string.IsNullOrWhiteSpace(output_folder)) { MessageBox.Show("出力先フォルダを指定してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            bool Isrun = true;
            if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked && checkBox4.Checked)
                if (MessageBox.Show("何もないファイルが生成されますが、よろしいですか？", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    Isrun = false;
            if (IsSomeFolder)
                if (MessageBox.Show("mkvがあるフォルダと同じフォルダですが、よろしいですか？\r\n\r\n(ファイル名)_2.mkvといった感じになります。", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    Isrun = false;


            if (Isrun)
            {
                foreach (string file in input_folderFiles)
                {
                    string _input = input_folder + "\\" + file;
                    string _file = file;
                    if (IsSomeFolder) _file = Path.GetFileNameWithoutExtension(_file) + "_2" + Path.GetExtension(_file);
                    string _output = output_folder + "\\" + _file;
                    string command = "--output \"" + _output + "\"";
                    if (checkBox1.Checked && !checkBox2.Checked)
                        command = "--output \"" + _output.Replace(".mkv", ".mka") + "\"";
                    if (checkBox1.Checked && checkBox2.Checked && (!checkBox3.Checked | !checkBox4.Checked))
                        command = "--output \"" + _output.Replace(".mkv", ".mks") + "\"";
                    if (checkBox1.Checked) command += " --no-video";
                    if (checkBox2.Checked) command += " --no-audio";
                    if (checkBox3.Checked) command += " --no-subtitles";
                    if (checkBox4.Checked) command += " --no-chapters";
                    command += " \"" + _input + "\"";
                    Process process = Process.Start(mkvmerge_fileName, command);
                    process.WaitForExit();
                }
                MessageBox.Show("終了しました。", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else MessageBox.Show("キャンセルされました。", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
