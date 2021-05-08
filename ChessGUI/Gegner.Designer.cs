namespace ChessGUI {
    partial class Gegner {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Gegner));
            this.btnSubmit = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbWeiss = new System.Windows.Forms.RadioButton();
            this.rbSchwarz = new System.Windows.Forms.RadioButton();
            this.lblHeaderPanel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSubmit
            // 
            this.btnSubmit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSubmit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSubmit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnSubmit.FlatAppearance.BorderSize = 0;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSubmit.ForeColor = System.Drawing.Color.Blue;
            this.btnSubmit.Location = new System.Drawing.Point(430, 96);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 1;
            this.btnSubmit.Text = "Auswaehlen";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.BtnSubmit_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblHeaderPanel);
            this.panel1.Controls.Add(this.rbSchwarz);
            this.panel1.Controls.Add(this.rbWeiss);
            this.panel1.Location = new System.Drawing.Point(12, 338);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(506, 100);
            this.panel1.TabIndex = 2;
            // 
            // rbWeiss
            // 
            this.rbWeiss.AutoSize = true;
            this.rbWeiss.Location = new System.Drawing.Point(15, 67);
            this.rbWeiss.Name = "rbWeiss";
            this.rbWeiss.Size = new System.Drawing.Size(50, 17);
            this.rbWeiss.TabIndex = 0;
            this.rbWeiss.TabStop = true;
            this.rbWeiss.Text = "Weiss";
            this.rbWeiss.UseVisualStyleBackColor = true;
            // 
            // rbSchwarz
            // 
            this.rbSchwarz.AutoSize = true;
            this.rbSchwarz.Location = new System.Drawing.Point(408, 67);
            this.rbSchwarz.Name = "rbSchwarz";
            this.rbSchwarz.Size = new System.Drawing.Size(66, 17);
            this.rbSchwarz.TabIndex = 1;
            this.rbSchwarz.TabStop = true;
            this.rbSchwarz.Text = "Schwarz";
            this.rbSchwarz.UseVisualStyleBackColor = true;
            // 
            // lblHeaderPanel
            // 
            this.lblHeaderPanel.AutoSize = true;
            this.lblHeaderPanel.Font = new System.Drawing.Font("Sitka Text", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeaderPanel.Location = new System.Drawing.Point(129, 15);
            this.lblHeaderPanel.Name = "lblHeaderPanel";
            this.lblHeaderPanel.Size = new System.Drawing.Size(236, 30);
            this.lblHeaderPanel.TabIndex = 2;
            this.lblHeaderPanel.Text = "Bitte die Farbe waehlen";
            // 
            // Gegner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnSubmit);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Gegner";
            this.Text = "Gegner";
            this.Load += new System.EventHandler(this.Gegner_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblHeaderPanel;
        private System.Windows.Forms.RadioButton rbSchwarz;
        private System.Windows.Forms.RadioButton rbWeiss;
    }
}