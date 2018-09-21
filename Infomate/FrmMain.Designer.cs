namespace Infomate
{
    partial class FrmMain
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
            this.TmrAnimation = new System.Windows.Forms.Timer(this.components);
            this.TmrUpdateData = new System.Windows.Forms.Timer(this.components);
            this.MnuOptions = new System.Windows.Forms.ContextMenu();
            this.MenuItem2 = new System.Windows.Forms.MenuItem();
            this.MenuItem1 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // TmrAnimation
            // 
            this.TmrAnimation.Enabled = true;
            this.TmrAnimation.Interval = 33;
            this.TmrAnimation.Tick += new System.EventHandler(this.TmrAnimation_Timer);
            // 
            // TmrUpdateData
            // 
            this.TmrUpdateData.Enabled = true;
            this.TmrUpdateData.Interval = 1500;
            this.TmrUpdateData.Tick += new System.EventHandler(this.TmrUpdateData_Timer);
            // 
            // MnuOptions
            // 
            this.MnuOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItem2,
            this.MenuItem1});
            // 
            // MenuItem2
            // 
            this.MenuItem2.Index = 0;
            this.MenuItem2.Text = "Add Item...";
            this.MenuItem2.Click += new System.EventHandler(this.MenuItem2_Click);
            // 
            // MenuItem1
            // 
            this.MenuItem1.Index = 1;
            this.MenuItem1.Text = "Quit";
            this.MenuItem1.Click += new System.EventHandler(this.MenuItem1_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(259, 217);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmMain";
            this.Opacity = 0.5D;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Form1_Doubleclick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer TmrAnimation;
        private System.Windows.Forms.Timer TmrUpdateData;
        private System.Windows.Forms.ContextMenu MnuOptions;
        private System.Windows.Forms.MenuItem MenuItem1;
        private System.Windows.Forms.MenuItem MenuItem2;
    }
}

