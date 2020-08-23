namespace EmVerif.Core.Gui.Graph
{
    partial class PublicGraphs
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.AdChart = new EmVerif.Core.Gui.Graph.OneGraph();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.SpioutChart = new EmVerif.Core.Gui.Graph.OneGraph();
            this.PwmChart = new EmVerif.Core.Gui.Graph.OneGraph();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.AdChart);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(514, 600);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // Ad6ChChart
            // 
            this.AdChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdChart.Location = new System.Drawing.Point(0, 0);
            this.AdChart.Name = "Ad6ChChart";
            this.AdChart.Size = new System.Drawing.Size(514, 200);
            this.AdChart.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.SpioutChart);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.PwmChart);
            this.splitContainer2.Size = new System.Drawing.Size(514, 396);
            this.splitContainer2.SplitterDistance = 200;
            this.splitContainer2.TabIndex = 0;
            // 
            // Spiout12ChChart
            // 
            this.SpioutChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SpioutChart.Location = new System.Drawing.Point(0, 0);
            this.SpioutChart.Name = "Spiout12ChChart";
            this.SpioutChart.Size = new System.Drawing.Size(514, 200);
            this.SpioutChart.TabIndex = 0;
            // 
            // Pwm6ChChart
            // 
            this.PwmChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PwmChart.Location = new System.Drawing.Point(0, 0);
            this.PwmChart.Name = "Pwm6ChChart";
            this.PwmChart.Size = new System.Drawing.Size(514, 192);
            this.PwmChart.TabIndex = 0;
            // 
            // PublicGraphs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "PublicGraphs";
            this.Size = new System.Drawing.Size(514, 600);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private OneGraph AdChart;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private OneGraph PwmChart;
        private OneGraph SpioutChart;
    }
}
