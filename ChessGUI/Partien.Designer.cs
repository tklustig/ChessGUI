namespace ChessGUI {
    partial class Partien {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Partien));
            this.txtBTerm = new System.Windows.Forms.TextBox();
            this.lblBez = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.dtP = new System.Windows.Forms.DateTimePicker();
            this.btnExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtBTerm
            // 
            this.txtBTerm.Location = new System.Drawing.Point(26, 47);
            this.txtBTerm.Name = "txtBTerm";
            this.txtBTerm.Size = new System.Drawing.Size(100, 20);
            this.txtBTerm.TabIndex = 0;
            // 
            // lblBez
            // 
            this.lblBez.AutoSize = true;
            this.lblBez.Location = new System.Drawing.Point(23, 20);
            this.lblBez.Name = "lblBez";
            this.lblBez.Size = new System.Drawing.Size(61, 13);
            this.lblBez.TabIndex = 1;
            this.lblBez.Text = "Suchbegriff";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(151, 44);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Suchen";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.BtnSearch_Click);
            // 
            // dtP
            // 
            this.dtP.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtP.Location = new System.Drawing.Point(110, 12);
            this.dtP.MaxDate = new System.DateTime(2021, 12, 31, 0, 0, 0, 0);
            this.dtP.Name = "dtP";
            this.dtP.Size = new System.Drawing.Size(116, 20);
            this.dtP.TabIndex = 3;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.Transparent;
            this.btnExport.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnExport.BackgroundImage")));
            this.btnExport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnExport.Location = new System.Drawing.Point(232, 26);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(43, 41);
            this.btnExport.TabIndex = 4;
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
            // 
            // Partien
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 510);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.dtP);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lblBez);
            this.Controls.Add(this.txtBTerm);
            this.Name = "Partien";
            this.Text = "Partien";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Partien_FormClosing);
            this.Load += new System.EventHandler(this.Partien_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBTerm;
        private System.Windows.Forms.Label lblBez;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.DateTimePicker dtP;
        private System.Windows.Forms.Button btnExport;
    }
}