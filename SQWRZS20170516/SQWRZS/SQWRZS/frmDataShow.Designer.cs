namespace SQWRZS
{
    partial class frmDataShow
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnSaveToDB = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnSKC = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.ColTDH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColJLZ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColSK = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColSKC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colZZ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColCKZ = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColRecvTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSaveToDB);
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.btnSKC);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1350, 92);
            this.panel1.TabIndex = 0;
            // 
            // btnSaveToDB
            // 
            this.btnSaveToDB.Location = new System.Drawing.Point(170, 30);
            this.btnSaveToDB.Name = "btnSaveToDB";
            this.btnSaveToDB.Size = new System.Drawing.Size(128, 44);
            this.btnSaveToDB.TabIndex = 2;
            this.btnSaveToDB.Text = "保存到数据库";
            this.btnSaveToDB.UseVisualStyleBackColor = true;
            this.btnSaveToDB.Click += new System.EventHandler(this.btnSaveToDB_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(304, 30);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(128, 44);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "导出到Excel";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnSKC
            // 
            this.btnSKC.Location = new System.Drawing.Point(36, 30);
            this.btnSKC.Name = "btnSKC";
            this.btnSKC.Size = new System.Drawing.Size(128, 44);
            this.btnSKC.TabIndex = 0;
            this.btnSKC.Text = "计算时刻差";
            this.btnSKC.UseVisualStyleBackColor = true;
            this.btnSKC.Click += new System.EventHandler(this.btnCBZZ_Click);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 603);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1350, 127);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dgvData);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 92);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1350, 511);
            this.panel3.TabIndex = 2;
            // 
            // dgvData
            // 
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColTDH,
            this.ColJLZ,
            this.ColSK,
            this.ColSKC,
            this.colZZ,
            this.ColCKZ,
            this.ColRecvTime});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(0, 0);
            this.dgvData.Name = "dgvData";
            this.dgvData.RowTemplate.Height = 23;
            this.dgvData.Size = new System.Drawing.Size(1350, 511);
            this.dgvData.TabIndex = 0;
            // 
            // ColTDH
            // 
            this.ColTDH.FillWeight = 5F;
            this.ColTDH.HeaderText = "通道号";
            this.ColTDH.Name = "ColTDH";
            // 
            // ColJLZ
            // 
            this.ColJLZ.FillWeight = 10F;
            this.ColJLZ.HeaderText = "计量值";
            this.ColJLZ.Name = "ColJLZ";
            // 
            // ColSK
            // 
            this.ColSK.FillWeight = 10F;
            this.ColSK.HeaderText = "时刻";
            this.ColSK.Name = "ColSK";
            // 
            // ColSKC
            // 
            this.ColSKC.FillWeight = 10F;
            this.ColSKC.HeaderText = "时刻差";
            this.ColSKC.Name = "ColSKC";
            // 
            // colZZ
            // 
            this.colZZ.FillWeight = 10F;
            this.colZZ.HeaderText = "总重";
            this.colZZ.Name = "colZZ";
            // 
            // ColCKZ
            // 
            this.ColCKZ.FillWeight = 10F;
            this.ColCKZ.HeaderText = "参考值";
            this.ColCKZ.Name = "ColCKZ";
            // 
            // ColRecvTime
            // 
            this.ColRecvTime.FillWeight = 13F;
            this.ColRecvTime.HeaderText = "接收时间";
            this.ColRecvTime.Name = "ColRecvTime";
            // 
            // frmDataShow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 730);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "frmDataShow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmDataShow";
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnSKC;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnSaveToDB;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColTDH;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColJLZ;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColSK;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColSKC;
        private System.Windows.Forms.DataGridViewTextBoxColumn colZZ;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColCKZ;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColRecvTime;
    }
}