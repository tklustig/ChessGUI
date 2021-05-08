namespace ChessGUI {
    partial class Zugtransfer {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Zugtransfer));
            this.lblTime = new System.Windows.Forms.Label();
            this.timerTime = new System.Windows.Forms.Timer(this.components);
            this.lblServer = new System.Windows.Forms.Label();
            this.lblanDerReihe = new System.Windows.Forms.Label();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatusStand = new System.Windows.Forms.Label();
            this.timerData = new System.Windows.Forms.Timer(this.components);
            this.lblMove = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.btnPDF = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.ForeColor = System.Drawing.Color.Blue;
            this.lblTime.Location = new System.Drawing.Point(731, 9);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(35, 13);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "label1";
            // 
            // timerTime
            // 
            this.timerTime.Interval = 1000;
            this.timerTime.Tick += new System.EventHandler(this.TimerTime_Tick);
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(2, 28);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(35, 13);
            this.lblServer.TabIndex = 1;
            this.lblServer.Text = "label1";
            // 
            // lblanDerReihe
            // 
            this.lblanDerReihe.AutoSize = true;
            this.lblanDerReihe.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblanDerReihe.Location = new System.Drawing.Point(7, 162);
            this.lblanDerReihe.Name = "lblanDerReihe";
            this.lblanDerReihe.Size = new System.Drawing.Size(46, 18);
            this.lblanDerReihe.TabIndex = 2;
            this.lblanDerReihe.Text = "label1";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.lblHeader.Font = new System.Drawing.Font("Modern No. 20", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(218, 109);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(38, 15);
            this.lblHeader.TabIndex = 3;
            this.lblHeader.Text = "label1";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(2, 52);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status:";
            // 
            // lblStatusStand
            // 
            this.lblStatusStand.AutoSize = true;
            this.lblStatusStand.Location = new System.Drawing.Point(38, 52);
            this.lblStatusStand.Name = "lblStatusStand";
            this.lblStatusStand.Size = new System.Drawing.Size(35, 13);
            this.lblStatusStand.TabIndex = 5;
            this.lblStatusStand.Text = "label1";
            // 
            // timerData
            // 
            this.timerData.Interval = 3000;
            this.timerData.Tick += new System.EventHandler(this.TimerData_Tick);
            // 
            // lblMove
            // 
            this.lblMove.AutoSize = true;
            this.lblMove.Font = new System.Drawing.Font("Papyrus", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMove.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMove.Location = new System.Drawing.Point(7, 200);
            this.lblMove.Name = "lblMove";
            this.lblMove.Size = new System.Drawing.Size(55, 24);
            this.lblMove.TabIndex = 6;
            this.lblMove.Text = "label1";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(5, 28);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(783, 561);
            this.webBrowser1.TabIndex = 7;
            this.webBrowser1.Url = new System.Uri("", System.UriKind.Relative);
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1_DocumentCompleted);
            // 
            // btnPDF
            // 
            this.btnPDF.BackColor = System.Drawing.Color.Transparent;
            this.btnPDF.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.btnPDF.FlatAppearance.BorderSize = 0;
            this.btnPDF.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Blue;
            this.btnPDF.Image = ((System.Drawing.Image)(resources.GetObject("btnPDF.Image")));
            this.btnPDF.Location = new System.Drawing.Point(691, 47);
            this.btnPDF.Name = "btnPDF";
            this.btnPDF.Size = new System.Drawing.Size(75, 23);
            this.btnPDF.TabIndex = 8;
            this.btnPDF.UseVisualStyleBackColor = false;
            this.btnPDF.Click += new System.EventHandler(this.BtnPDF_Click);
            // 
            // Zugtransfer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(814, 601);
            this.Controls.Add(this.btnPDF);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.lblMove);
            this.Controls.Add(this.lblStatusStand);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblanDerReihe);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblTime);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Zugtransfer";
            this.Text = "Login&Zugtransfer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Zugtransfer_FormClosed);
            this.Load += new System.EventHandler(this.Zugtransfer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Timer timerTime;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblanDerReihe;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusStand;
        private System.Windows.Forms.Timer timerData;
        private System.Windows.Forms.Label lblMove;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button btnPDF;
    }
}