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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.tm_Update = new System.Windows.Forms.Timer(this.components);
            this.publicGraphs1 = new EmVerif.Gui.Graph.PublicGraphs();
            this.publicLoadBar1 = new EmVerif.Gui.Graph.PublicLoadBar();
            this.publicVariableView1 = new EmVerif.Gui.Variable.PublicVariableView();
            this.publicLog1 = new EmVerif.Gui.Log.PublicLog();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
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
            this.splitContainer1.Size = new System.Drawing.Size(1420, 693);
            this.splitContainer1.SplitterDistance = 477;
            this.splitContainer1.TabIndex = 0;
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
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(939, 693);
            this.splitContainer2.SplitterDistance = 464;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.publicLog1);
            this.splitContainer3.Size = new System.Drawing.Size(939, 464);
            this.splitContainer3.SplitterDistance = 419;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.publicLoadBar1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.publicVariableView1);
            this.splitContainer4.Size = new System.Drawing.Size(419, 464);
            this.splitContainer4.SplitterDistance = 99;
            this.splitContainer4.TabIndex = 0;
            // 
            // tm_Update
            // 
            this.tm_Update.Tick += new System.EventHandler(this.tm_Update_Tick);
            // 
            // publicGraphs1
            // 
            this.publicGraphs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicGraphs1.Location = new System.Drawing.Point(0, 0);
            this.publicGraphs1.Name = "publicGraphs1";
            this.publicGraphs1.Size = new System.Drawing.Size(477, 693);
            this.publicGraphs1.TabIndex = 0;
            // 
            // publicLoadBar1
            // 
            this.publicLoadBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicLoadBar1.Location = new System.Drawing.Point(0, 0);
            this.publicLoadBar1.Name = "publicLoadBar1";
            this.publicLoadBar1.Size = new System.Drawing.Size(419, 99);
            this.publicLoadBar1.TabIndex = 0;
            // 
            // publicVariableView1
            // 
            this.publicVariableView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicVariableView1.Location = new System.Drawing.Point(0, 0);
            this.publicVariableView1.Name = "publicVariableView1";
            this.publicVariableView1.Size = new System.Drawing.Size(419, 361);
            this.publicVariableView1.TabIndex = 0;
            // 
            // publicLog1
            // 
            this.publicLog1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.publicLog1.Location = new System.Drawing.Point(0, 0);
            this.publicLog1.Name = "publicLog1";
            this.publicLog1.Size = new System.Drawing.Size(516, 464);
            this.publicLog1.TabIndex = 0;
            // 
            // GuiTop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 693);
            this.Controls.Add(this.splitContainer1);
            this.Name = "GuiTop";
            this.Text = "GuiTop";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GuiTop_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Graph.PublicGraphs publicGraphs1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private Graph.PublicLoadBar publicLoadBar1;
        private Variable.PublicVariableView publicVariableView1;
        private Log.PublicLog publicLog1;
        private System.Windows.Forms.Timer tm_Update;
    }
}