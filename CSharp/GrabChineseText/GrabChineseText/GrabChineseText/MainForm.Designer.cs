namespace LYTools
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbGrabTgtPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbGrabSrcPath = new System.Windows.Forms.TextBox();
            this.btnGrab = new System.Windows.Forms.Button();
            this.btnTranslate = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTransTgtPath = new System.Windows.Forms.TextBox();
            this.tbTransSrcPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbGrabTgtPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbGrabSrcPath);
            this.groupBox1.Controls.Add(this.btnGrab);
            this.groupBox1.Location = new System.Drawing.Point(34, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(899, 118);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "提取数据表中的中文";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "目标数据表路径：";
            // 
            // tbGrabTgtPath
            // 
            this.tbGrabTgtPath.AllowDrop = true;
            this.tbGrabTgtPath.Location = new System.Drawing.Point(107, 52);
            this.tbGrabTgtPath.Name = "tbGrabTgtPath";
            this.tbGrabTgtPath.Size = new System.Drawing.Size(786, 21);
            this.tbGrabTgtPath.TabIndex = 3;
            this.tbGrabTgtPath.Text = "C:\\AllChinese.xls";
            this.tbGrabTgtPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.GrabTgtTextBox_DragDrop);
            this.tbGrabTgtPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.GrabSrcTextBox_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "源数据表路径：";
            // 
            // tbGrabSrcPath
            // 
            this.tbGrabSrcPath.AllowDrop = true;
            this.tbGrabSrcPath.Location = new System.Drawing.Point(107, 25);
            this.tbGrabSrcPath.Name = "tbGrabSrcPath";
            this.tbGrabSrcPath.Size = new System.Drawing.Size(786, 21);
            this.tbGrabSrcPath.TabIndex = 1;
            this.tbGrabSrcPath.Text = "H:\\Thai_ob20181019\\Tables";
            this.tbGrabSrcPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.GrabSrcTextBox_DragDrop);
            this.tbGrabSrcPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.GrabSrcTextBox_DragEnter);
            // 
            // btnGrab
            // 
            this.btnGrab.Location = new System.Drawing.Point(423, 79);
            this.btnGrab.Name = "btnGrab";
            this.btnGrab.Size = new System.Drawing.Size(75, 23);
            this.btnGrab.TabIndex = 0;
            this.btnGrab.Text = "开始提取";
            this.btnGrab.UseVisualStyleBackColor = true;
            this.btnGrab.Click += new System.EventHandler(this.btnGrab_Click);
            // 
            // btnTranslate
            // 
            this.btnTranslate.Location = new System.Drawing.Point(423, 79);
            this.btnTranslate.Name = "btnTranslate";
            this.btnTranslate.Size = new System.Drawing.Size(75, 23);
            this.btnTranslate.TabIndex = 5;
            this.btnTranslate.Text = "开始翻译";
            this.btnTranslate.UseVisualStyleBackColor = true;
            this.btnTranslate.Click += new System.EventHandler(this.btnTranslate_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnTranslate);
            this.groupBox2.Controls.Add(this.tbTransTgtPath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.tbTransSrcPath);
            this.groupBox2.Location = new System.Drawing.Point(34, 136);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(899, 118);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "将翻译写入Excel";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "目标数据表路径：";
            // 
            // tbTransTgtPath
            // 
            this.tbTransTgtPath.AllowDrop = true;
            this.tbTransTgtPath.Location = new System.Drawing.Point(107, 52);
            this.tbTransTgtPath.Name = "tbTransTgtPath";
            this.tbTransTgtPath.Size = new System.Drawing.Size(786, 21);
            this.tbTransTgtPath.TabIndex = 3;
            this.tbTransTgtPath.Text = "C:\\TablesExcel";
            // 
            // tbTransSrcPath
            // 
            this.tbTransSrcPath.AllowDrop = true;
            this.tbTransSrcPath.Location = new System.Drawing.Point(107, 25);
            this.tbTransSrcPath.Name = "tbTransSrcPath";
            this.tbTransSrcPath.Size = new System.Drawing.Size(786, 21);
            this.tbTransSrcPath.TabIndex = 1;
            this.tbTransSrcPath.Text = "H:\\Thai_ob20181019\\Tables";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "源数据表路径：";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 325);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.Text = "提取中文&将泰文写回数据表";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbGrabSrcPath;
        private System.Windows.Forms.Button btnGrab;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbGrabTgtPath;
        private System.Windows.Forms.Button btnTranslate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTransTgtPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTransSrcPath;
    }
}