
namespace logFileAnaly
{
	partial class frmWordFormat
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
			this.txtWordFormat = new System.Windows.Forms.TextBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(163, 30);
			this.button1.TabIndex = 10;
			this.button1.Tag = "LOG";
			this.button1.Text = "파일읽기";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// txtWordFormat
			// 
			this.txtWordFormat.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtWordFormat.Location = new System.Drawing.Point(12, 51);
			this.txtWordFormat.MaxLength = 0;
			this.txtWordFormat.Multiline = true;
			this.txtWordFormat.Name = "txtWordFormat";
			this.txtWordFormat.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtWordFormat.Size = new System.Drawing.Size(1613, 1055);
			this.txtWordFormat.TabIndex = 11;
			this.txtWordFormat.WordWrap = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 1113);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(1616, 10);
			this.progressBar1.TabIndex = 12;
			// 
			// frmWordFormat
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1637, 1135);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.txtWordFormat);
			this.Controls.Add(this.button1);
			this.Name = "frmWordFormat";
			this.Text = "frmWordFormat";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox txtWordFormat;
		private System.Windows.Forms.ProgressBar progressBar1;
	}
}