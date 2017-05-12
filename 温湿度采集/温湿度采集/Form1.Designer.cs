namespace 温湿度采集
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.wendu = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.shidu = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.wendu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shidu)).BeginInit();
            this.SuspendLayout();
            // 
            // wendu
            // 
            chartArea1.Name = "ChartArea1";
            this.wendu.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.wendu.Legends.Add(legend1);
            this.wendu.Location = new System.Drawing.Point(94, 68);
            this.wendu.Name = "wendu";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series1.IsXValueIndexed = true;
            series1.Legend = "Legend1";
            series1.Name = "温度";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;
            this.wendu.Series.Add(series1);
            this.wendu.Size = new System.Drawing.Size(1044, 244);
            this.wendu.TabIndex = 0;
            this.wendu.Text = "温度";
            // 
            // shidu
            // 
            chartArea2.Name = "ChartArea1";
            this.shidu.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.shidu.Legends.Add(legend2);
            this.shidu.Location = new System.Drawing.Point(94, 370);
            this.shidu.Name = "shidu";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            series2.IsXValueIndexed = true;
            series2.Legend = "Legend1";
            series2.Name = "湿度";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;
            this.shidu.Series.Add(series2);
            this.shidu.Size = new System.Drawing.Size(1044, 326);
            this.shidu.TabIndex = 3;
            this.shidu.Text = "湿度";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1350, 730);
            this.Controls.Add(this.shidu);
            this.Controls.Add(this.wendu);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wendu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shidu)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart wendu;
        private System.Windows.Forms.DataVisualization.Charting.Chart shidu;
    }
}

