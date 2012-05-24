using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TBTGenerator
{
    public partial class Form1 : Form
    {
        string src_folder;
        string dest_folder;
        FolderBrowserDialog FolderSelect;
        tbt_generator tbt;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            src_folder = "C:\\";
            dest_folder = "C:\\";

            FolderSelect = new FolderBrowserDialog();

            textBox1.Text = src_folder;
            textBox2.Text = dest_folder;

            tbt = new tbt_generator(richTextBox1, progressBar1);
        }

        // 來源資料夾
        private void button1_Click(object sender, EventArgs e)
        {
            FolderSelect.SelectedPath = textBox1.Text;
            FolderSelect.ShowDialog();
            textBox1.Text = FolderSelect.SelectedPath;
        }

        // 目的資料夾
        private void button2_Click(object sender, EventArgs e)
        {
            FolderSelect.SelectedPath = textBox2.Text;
            FolderSelect.ShowDialog();
            textBox2.Text = FolderSelect.SelectedPath;
        }

        // 開始轉檔
        private void button3_Click(object sender, EventArgs e)
        {
            src_folder = textBox1.Text;
            dest_folder = textBox2.Text;

            tbt.setIOFloder(src_folder, dest_folder);
            tbt.run();
        }
    }
}
