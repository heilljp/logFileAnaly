
namespace logFileAnaly
{
	partial class Form1
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
			this.btnStart = new System.Windows.Forms.Button();
			this.txtAddress = new System.Windows.Forms.TextBox();
			this.prgDownload = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(1165, 66);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(157, 33);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "DownLoad Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnDownLoad_Click);
			// 
			// txtAddress
			// 
			this.txtAddress.Location = new System.Drawing.Point(12, 26);
			this.txtAddress.Multiline = true;
			this.txtAddress.Name = "txtAddress";
			this.txtAddress.Size = new System.Drawing.Size(1310, 34);
			this.txtAddress.TabIndex = 1;
			// 
			// prgDownload
			// 
			this.prgDownload.Location = new System.Drawing.Point(12, 7);
			this.prgDownload.Name = "prgDownload";
			this.prgDownload.Size = new System.Drawing.Size(1310, 13);
			this.prgDownload.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1334, 108);
			this.Controls.Add(this.prgDownload);
			this.Controls.Add(this.txtAddress);
			this.Controls.Add(this.btnStart);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Opacity = 0.6D;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "frmFileDownload";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.TextBox txtAddress;
		private System.Windows.Forms.ProgressBar prgDownload;
	}
}