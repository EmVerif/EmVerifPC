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
            this.InChart = new EmVerif.Core.Gui.Graph.OneGraph();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.ThroughOutChart = new EmVerif.Core.Gui.Graph.OneGraph();
            this.MixOutChart = new EmVerif.Core.Gui.Graph.OneGraph();
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
            this.splitContainer1.Panel1.Controls.Add(this.InChart);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(514, 600);
            this.splitContainer1.SplitterDistance = 200;
            this.splitContainer1.TabIndex = 0;
            // 
            // In6ChChart
            // 
            this.InChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InChart.Location = new System.Drawing.Point(0, 0);
            this.InChart.Name = "In6ChChart";
            this.InChart.Size = new System.Drawing.Size(514, 200);
            this.InChart.TabIndex = 0;
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
            this.splitContainer2.Panel1.Controls.Add(this.ThroughOutChart);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MixOutChart);
            this.splitContainer2.Size = new System.Drawing.Size(514, 396);
            this.splitContainer2.SplitterDistance = 200;
            this.splitContainer2.TabIndex = 0;
            // 
            // ThroughOut12ChChart
            // 
            this.ThroughOutChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ThroughOutChart.Location = new System.Drawing.Point(0, 0);
            this.ThroughOutChart.Name = "ThroughOut12ChChart";
            this.ThroughOutChart.Size = new System.Drawing.Size(514, 200);
            this.ThroughOutChart.TabIndex = 0;
            // 
            // MixOut4ChChart
            // 
            this.MixOutChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MixOutChart.Location = new System.Drawing.Point(0, 0);
            this.MixOutChart.Name = "MixOut4ChChart";
            this.MixOutChart.Size = new System.Drawing.Size(514, 192);
            this.MixOutChart.TabIndex = 0;
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
        private OneGraph InChart;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private OneGraph MixOutChart;
        private OneGraph ThroughOutChart;
    }
}
