namespace LicenseManager.Generator
{
    partial class frmGenerateKey
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
            this.button1 = new System.Windows.Forms.Button();
            this.txtPubKey = new System.Windows.Forms.TextBox();
            this.txtPrvKey = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(371, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPubKey
            // 
            this.txtPubKey.Location = new System.Drawing.Point(12, 93);
            this.txtPubKey.Multiline = true;
            this.txtPubKey.Name = "txtPubKey";
            this.txtPubKey.Size = new System.Drawing.Size(369, 248);
            this.txtPubKey.TabIndex = 1;
            // 
            // txtPrvKey
            // 
            this.txtPrvKey.Location = new System.Drawing.Point(442, 93);
            this.txtPrvKey.Multiline = true;
            this.txtPrvKey.Name = "txtPrvKey";
            this.txtPrvKey.Size = new System.Drawing.Size(369, 248);
            this.txtPrvKey.TabIndex = 2;
            // 
            // frmGenerateKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 353);
            this.Controls.Add(this.txtPrvKey);
            this.Controls.Add(this.txtPubKey);
            this.Controls.Add(this.button1);
            this.Name = "frmGenerateKey";
            this.Text = "frmGenerateKey";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPubKey;
        private System.Windows.Forms.TextBox txtPrvKey;
    }
}