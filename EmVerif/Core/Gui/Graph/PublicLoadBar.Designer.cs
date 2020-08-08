namespace EmVerif.Core.Gui.Graph
{
    partial class PublicLoadBar
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            this.chart_Bar = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Bar)).BeginInit();
            this.SuspendLayout();
            // 
            // chart_Bar
            // 
            chartArea1.AxisY.Interval = 10D;
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartArea1.AxisY.Maximum = 100D;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.Name = "ChartArea1";
            this.chart_Bar.ChartAreas.Add(chartArea1);
            this.chart_Bar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart_Bar.Location = new System.Drawing.Point(0, 0);
            this.chart_Bar.Name = "chart_Bar";
            this.chart_Bar.Size = new System.Drawing.Size(360, 420);
            this.chart_Bar.TabIndex = 0;
            this.chart_Bar.Text = "chart1";
            // 
            // PublicLoadBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chart_Bar);
            this.Name = "PublicLoadBar";
            this.Size = new System.Drawing.Size(360, 420);
            ((System.ComponentModel.ISupportInitialize)(this.chart_Bar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Bar;
    }
}
