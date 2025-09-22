namespace Mir3DClientEditor
{
    partial class FSplash
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSplash));
            label1 = new Label();
            OpenPaypal = new Button();
            button1 = new Button();
            label2 = new Label();
            NoShowCheckbox = new CheckBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label1.Location = new Point(45, 26);
            label1.Name = "label1";
            label1.Size = new Size(764, 96);
            label1.TabIndex = 0;
            label1.Text = "This application is totally free. \r\nIf you liked it, please, do not forget to make a small contribution \r\nto guarantee the continuous maintenance.";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // OpenPaypal
            // 
            OpenPaypal.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
            OpenPaypal.Location = new Point(285, 151);
            OpenPaypal.Name = "OpenPaypal";
            OpenPaypal.Size = new Size(288, 67);
            OpenPaypal.TabIndex = 1;
            OpenPaypal.Text = "Donate from PayPal";
            OpenPaypal.UseVisualStyleBackColor = true;
            OpenPaypal.Click += OpenPaypal_Click;
            // 
            // button1
            // 
            button1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
            button1.Location = new Point(285, 278);
            button1.Name = "button1";
            button1.Size = new Size(288, 67);
            button1.TabIndex = 2;
            button1.Text = "Donate from PayPal Me";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            label2.Location = new Point(130, 231);
            label2.Name = "label2";
            label2.Size = new Size(613, 32);
            label2.TabIndex = 3;
            label2.Text = "If you have problems donating, try this other option";
            label2.TextAlign = ContentAlignment.MiddleCenter;
            label2.Click += label2_Click;
            // 
            // NoShowCheckbox
            // 
            NoShowCheckbox.AutoSize = true;
            NoShowCheckbox.Location = new Point(752, 349);
            NoShowCheckbox.Name = "NoShowCheckbox";
            NoShowCheckbox.Size = new Size(118, 19);
            NoShowCheckbox.TabIndex = 4;
            NoShowCheckbox.Text = "Don't show again";
            NoShowCheckbox.UseVisualStyleBackColor = true;
            // 
            // FSplash
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(882, 380);
            Controls.Add(NoShowCheckbox);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(OpenPaypal);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FSplash";
            Text = "Mir3D Client Editor";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label label1;
        private Button OpenPaypal;
        private Button button1;
        private Label label2;
        private CheckBox NoShowCheckbox;
    }
}