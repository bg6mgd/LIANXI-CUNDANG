namespace arduino两路输出两路输入客户端
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.comserial = new System.Windows.Forms.ComboBox();
            this.combtl = new System.Windows.Forms.ComboBox();
            this.com = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.a0lab = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.a1lab = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.d1lab = new System.Windows.Forms.Label();
            this.d0lab = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comserial
            // 
            this.comserial.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.comserial.FormattingEnabled = true;
            this.comserial.Location = new System.Drawing.Point(98, 29);
            this.comserial.Name = "comserial";
            this.comserial.Size = new System.Drawing.Size(121, 20);
            this.comserial.TabIndex = 0;
            // 
            // combtl
            // 
            this.combtl.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combtl.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.combtl.FormattingEnabled = true;
            this.combtl.Items.AddRange(new object[] {
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.combtl.Location = new System.Drawing.Point(346, 29);
            this.combtl.Name = "combtl";
            this.combtl.Size = new System.Drawing.Size(121, 20);
            this.combtl.TabIndex = 1;
            // 
            // com
            // 
            this.com.AutoSize = true;
            this.com.Location = new System.Drawing.Point(31, 36);
            this.com.Name = "com";
            this.com.Size = new System.Drawing.Size(41, 12);
            this.com.TabIndex = 2;
            this.com.Text = "串口号";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(287, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "波特率";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "设备返回信息";
            // 
            // a0lab
            // 
            this.a0lab.AutoSize = true;
            this.a0lab.Location = new System.Drawing.Point(150, 153);
            this.a0lab.Name = "a0lab";
            this.a0lab.Size = new System.Drawing.Size(0, 12);
            this.a0lab.TabIndex = 5;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(124, 92);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(343, 26);
            this.textBox1.TabIndex = 6;
            // 
            // a1lab
            // 
            this.a1lab.AutoSize = true;
            this.a1lab.Location = new System.Drawing.Point(150, 204);
            this.a1lab.Name = "a1lab";
            this.a1lab.Size = new System.Drawing.Size(0, 12);
            this.a1lab.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(33, 142);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "A0电压采集";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(33, 193);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "A1电压采集";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(329, 193);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(102, 23);
            this.button3.TabIndex = 13;
            this.button3.Text = "D1数字状态采集";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(329, 142);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(102, 23);
            this.button4.TabIndex = 12;
            this.button4.Text = "D0数字状态采集";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // d1lab
            // 
            this.d1lab.AutoSize = true;
            this.d1lab.Location = new System.Drawing.Point(446, 204);
            this.d1lab.Name = "d1lab";
            this.d1lab.Size = new System.Drawing.Size(0, 12);
            this.d1lab.TabIndex = 11;
            // 
            // d0lab
            // 
            this.d0lab.AutoSize = true;
            this.d0lab.Location = new System.Drawing.Point(446, 153);
            this.d0lab.Name = "d0lab";
            this.d0lab.Size = new System.Drawing.Size(0, 12);
            this.d0lab.TabIndex = 10;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(496, 29);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 14;
            this.button5.Text = "Open";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 358);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.d1lab);
            this.Controls.Add(this.d0lab);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.a1lab);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.a0lab);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.com);
            this.Controls.Add(this.combtl);
            this.Controls.Add(this.comserial);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comserial;
        private System.Windows.Forms.ComboBox combtl;
        private System.Windows.Forms.Label com;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label a0lab;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label a1lab;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label d1lab;
        private System.Windows.Forms.Label d0lab;
        private System.Windows.Forms.Button button5;
    }
}

