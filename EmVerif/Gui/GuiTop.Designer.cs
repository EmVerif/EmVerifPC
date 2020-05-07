namespace EmVerif.Gui
{
    partial class GuiTop
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.publicGraphs1 = new EmVerif.Gui.Graph.PublicGraphs();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.publicLoadBar1 = new EmVerif.Gui.Graph.PublicLoadBar();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.publicVariableView1 = new EmVerif.Gui.Variable.PublicVariableView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.publicGraphs1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1076, 621);
            this.splitContainer1.SplitterDistance = 653;
            this.splitContainer1.TabIndex = 1;
            // 
            // publicGraphs1
            // 
            this.publicGraphs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicGraphs1.Location = new System.Drawing.Point(0, 0);
            this.publicGraphs1.Name = "publicGraphs1";
            this.publicGraphs1.Size = new System.Drawing.Size(653, 621);
            this.publicGraphs1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.publicLoadBar1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(419, 621);
            this.splitContainer2.SplitterDistance = 93;
            this.splitContainer2.TabIndex = 0;
            // 
            // publicLoadBar1
            // 
            this.publicLoadBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicLoadBar1.Location = new System.Drawing.Point(0, 0);
            this.publicLoadBar1.Name = "publicLoadBar1";
            this.publicLoadBar1.Size = new System.Drawing.Size(419, 93);
            this.publicLoadBar1.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.publicVariableView1);
            this.splitContainer3.Size = new System.Drawing.Size(419, 524);
            this.splitContainer3.SplitterDistance = 322;
            this.splitContainer3.TabIndex = 0;
            // 
            // publicVariableView1
            // 
            this.publicVariableView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicVariableView1.Location = new System.Drawing.Point(0, 0);
            this.publicVariableView1.Name = "publicVariableView1";
            this.publicVariableView1.Size = new System.Drawing.Size(419, 322);
            this.publicVariableView1.TabIndex = 0;
            // 
            // GuiTop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 621);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "GuiTop";
            this.Text = "GuiTop";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Graph.PublicGraphs publicGraphs1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Graph.PublicLoadBar publicLoadBar1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private Variable.PublicVariableView publicVariableView1;
    }
}