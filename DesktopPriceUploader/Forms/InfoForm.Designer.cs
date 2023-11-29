namespace DesktopPriceUploader.Forms
{
    partial class InfoForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoForm));
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.btnClose = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.richTextBox1.Location = new System.Drawing.Point(22, 22);
			this.richTextBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.richTextBox1.MaximumSize = new System.Drawing.Size(748, 639);
			this.richTextBox1.MinimumSize = new System.Drawing.Size(748, 639);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(748, 639);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "Программа предназначена для автоматической синхронизации данных в 1С и данных на " +
    "сайте магазина.";
			// 
			// btnClose
			// 
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(302, 694);
			this.btnClose.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(220, 42);
			this.btnClose.TabIndex = 1;
			this.btnClose.Text = "Закрыть";
			this.btnClose.UseVisualStyleBackColor = false;
			// 
			// InfoForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(781, 713);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.richTextBox1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(805, 777);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(805, 777);
			this.Name = "InfoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Информация о программе";
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnClose;
    }
}