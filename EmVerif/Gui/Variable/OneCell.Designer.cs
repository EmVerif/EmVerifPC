namespace EmVerif.Gui.Variable
{
    partial class OneCell
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
            this.tb_VarName = new System.Windows.Forms.TextBox();
            this.tb_Value = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tb_VarName
            // 
            this.tb_VarName.Location = new System.Drawing.Point(0, 0);
            this.tb_VarName.Name = "tb_VarName";
            this.tb_VarName.ReadOnly = true;
            this.tb_VarName.Size = new System.Drawing.Size(74, 19);
            this.tb_VarName.TabIndex = 0;
            // 
            // tb_Value
            // 
            this.tb_Value.Location = new System.Drawing.Point(80, 0);
            this.tb_Value.Name = "tb_Value";
            this.tb_Value.Size = new System.Drawing.Size(159, 19);
            this.tb_Value.TabIndex = 1;
            this.tb_Value.Leave += new System.EventHandler(this.tb_Value_Leave);
            // 
            // OneCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tb_Value);
            this.Controls.Add(this.tb_VarName);
            this.Name = "OneCell";
            this.Size = new System.Drawing.Size(242, 21);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox tb_VarName;
        public System.Windows.Forms.TextBox tb_Value;
    }
}
