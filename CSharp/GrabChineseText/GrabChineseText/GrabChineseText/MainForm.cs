using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Shark;
using System.IO;
using System.Text.RegularExpressions;

namespace LYTools
{
    public partial class MainForm : Form
    {
        

        public MainForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            tbGrabSrcPath.Text = System.Windows.Forms.Application.StartupPath;
            
        }

        private void GrabSrcTextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void GrabSrcTextBox_DragDrop(object sender, DragEventArgs e)
        {
            tbGrabSrcPath.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void GrabTgtTextBox_DragDrop(object sender, DragEventArgs e)
        {
            tbGrabTgtPath.Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            GrabTool.Start(tbGrabSrcPath.Text, tbGrabTgtPath.Text);
        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            TranslateTool.Start(tbTransSrcPath.Text, tbTransTgtPath.Text);
        }
    }
}
