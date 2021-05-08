using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System;
using System.Runtime.InteropServices;
using System.Globalization;
using NLog;

namespace ChessGUI {
    public partial class About : Form {

        #region globale Anweisungen / Varibalen/ Instanzen
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        [DllImport("user32.dll")]
        public static extern int ShowWindow(int Wnd, int Flags);
        [DllImport("user32.dll")]
        public static extern int FindWindow(string strCName, string strWndName);
        private System.ComponentModel.IContainer components;
        private string rootFolder = AppDomain.CurrentDomain.BaseDirectory + @"icons\";
        private Timer timer1;
        private float angle = 0;
        #endregion

        #region Konstruktor
        public About() {
            InitializeComponent();
        }
        #endregion

        #region Windows Form Designer generated code
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new Timer(this.components);
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new EventHandler(this.timer1_Tick);
            // 
            // Form4
            // 
            this.ClientSize = new Size(632, 463);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form4";
            this.SizeGripStyle = SizeGripStyle.Show;
            this.Load += new EventHandler(this.Form4_Load);
            this.Paint += new PaintEventHandler(this.Form1_Paint);
            this.MouseUp += new MouseEventHandler(this.Form2_MouseClick);
            this.ResumeLayout(false);

        }
        #endregion

        #region Paint Methode, generiert durch die Toolbox
        private void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
            Rotate(e.Graphics);
        }
        #endregion

        #region Mausklick-Methode, generiert durch die Toolbox
        private void Form2_MouseClick(object sender, MouseEventArgs e) {
            try {
                timer1.Enabled = false;
                using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                    speechSynthesizer.SetOutputToDefaultAudioDevice();
                    speechSynthesizer.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.NotSet, 0, CultureInfo.GetCultureInfo("en"));
                    speechSynthesizer.Rate = -2;
                    speechSynthesizer.Speak("For questions, please contact me by phone, or by email");
                }
                this.Close();
            }
            catch (Exception er) {
                _logger.Error(er.Message + Environment.NewLine + er.ToString());
                SchachGUI o = new SchachGUI();
                o.Ausgabe(er.Message, "Error", MessageBoxIcon.Error);
                this.Close();
            }
        }
        #endregion

        #region Formularlade-Methode. Generiert durch die Toolbox
        private void Form4_Load(object sender, EventArgs e) {
            _logger.Info("DeveloperInfo succesfully loaded");
        }
        #endregion

        #region gekapselte Grafikmethoden

        private LinearGradientBrush GetBrush() {
            return new LinearGradientBrush(
              new Rectangle(1, 1, 1024, 700),
              Color.Red,
              Color.Yellow,
              0.0F,
              true);
        }

        private void Rotate(Graphics graphics, LinearGradientBrush brush) {
            brush.RotateTransform(angle);
            brush.SetBlendTriangularShape(.5F);
            graphics.FillRectangle(brush, brush.Rectangle);
        }

        private void Rotate(Graphics graphics) {
            angle += 5 % 360;
            Rotate(graphics, GetBrush());
        }
        #endregion

        #region Timer Code
        private void timer1_Tick(object sender, System.EventArgs e) {
            try {
                Rotate(CreateGraphics());
                Graphics g = this.CreateGraphics();
                Icon IconPhone = new Icon(rootFolder + "phone.ico");
                Icon IconUrl = new Icon(rootFolder + "web.ico");
                Icon IconMail = new Icon(rootFolder + "mail.ico");
                DateTime dt = new DateTime(2019, 9, 1);
                using (Font bigFont = new Font(SystemFonts.DefaultFont.FontFamily, 28, FontStyle.Regular))
                using (Font mediumFont = new Font(SystemFonts.DefaultFont.FontFamily, 18, FontStyle.Regular))
                using (Font smallFont = new Font(SystemFonts.DefaultFont.FontFamily, 14, FontStyle.Regular))
                using (Font VsmallFont = new Font(SystemFonts.DefaultFont.FontFamily, 10, FontStyle.Regular)) {
                    g.DrawString("ChessGUI", bigFont, Brushes.Green, 30, 30);
                    g.DrawString(String.Format("{0:d/MM/yyyy}", dt.ToLongDateString()), smallFont, Brushes.Green, 270, 45);
                    g.DrawString("© by Thomas Kipp", mediumFont, Brushes.Blue, 30, 70);
                    g.DrawString("with graphical support by Patrick Priebke", mediumFont, Brushes.Blue, 55, 100);
                    g.DrawIcon(IconMail, 35, 175);
                    g.DrawIcon(IconPhone, 35, 200);
                    g.DrawIcon(IconUrl, 35, 230);
                    g.DrawString("Contact:" + Environment.NewLine +
                    "    tklustig.thomas@gmail.con" + Environment.NewLine +
                    "    0152/37389041" + Environment.NewLine +
                    "    http://tklustig.ddns.net", mediumFont, Brushes.Black, 40, 150);
                    g.DrawString("programming language: C# (WindowsForms)", smallFont, Brushes.Turquoise, 30, 260);
                    g.DrawString("click me to get closed", VsmallFont, Brushes.Beige, 35, 280);
                }
                ShowWindow(FindWindow("Progman", "Program Manager"), 1);
            }
            catch (Exception er) {
                timer1.Stop();
                SchachGUI fm = new SchachGUI();
                //Die oeffentliche Methode Message wurde in der Klasse Form1 implementiert
                fm.Ausgabe("Symbols not available." + Environment.NewLine + "System error has been protocollated in log!", "Error", MessageBoxIcon.Error);
                _logger.Error("Symbols(Ressources) not available." + Environment.NewLine + er.ToString());
                this.Close();
            }
        }
        #endregion
    }
}
