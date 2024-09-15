using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace logFileAnaly
{
	public partial class Form1 : Form
	{

		private delegate void CSafeSetText(string text);
		private delegate void CSafeSetMaximum(Int32 value);
		private delegate void CSafeSetValue(Int32 value);

		private CSafeSetText csst;
		private CSafeSetMaximum cssm;
		private CSafeSetValue cssv;
		private WebClient wc;
		private Boolean setBaseSize;
		private Boolean nowDownloading;


		public Form1()
		{
			cssm = new CSafeSetMaximum(CrossSafeSetMaximumMethod);
			cssv = new CSafeSetValue(CrossSafeSetValueMethod);

			// 웹 클라이언트 개체를 초기화하고,
			wc = new WebClient();

			InitializeComponent();
		}

		private void btnDownLoad_Click(object sender, EventArgs e)
		{
			var downFolderPath = @"C:\\tmp\down_mp3";

			if (nowDownloading)
			{
				MessageBox.Show("이미 다운로드가 진행 중입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			String remoteAddress = txtAddress.Text.Trim();
			if (String.IsNullOrEmpty(remoteAddress))
			{
				MessageBox.Show("주소가 입력되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			// 파일이 저장될 위치를 저장한다.
			//String fileName = String.Format(downFolderPath+ "\\{0}", System.IO.Path.GetFileName(remoteAddress));
			String fileName = String.Format(downFolderPath + "\\{0}", "123.mp3");

			// 폴더가 존재하지 않는다면 폴더를 생성한다.
			if (!System.IO.Directory.Exists(downFolderPath))
				System.IO.Directory.CreateDirectory(downFolderPath);

			try
			{

				// C 드라이브 밑의 downloadFiles 폴더에 파일 이름대로 저장한다.
				wc.DownloadFileAsync(new Uri(remoteAddress), fileName);

				// 다운로드 중이라는걸 알리기 위한 값을 설정하고,
				// 프로그레스바의 크기를 0으로 만든다.
				prgDownload.Value = 0;
				setBaseSize = false;
				nowDownloading = true;
				btnStart.Enabled = false;
				txtAddress.Enabled = false;

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.GetType().FullName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		void CrossSafeSetValueMethod(Int32 value)
		{
			if (prgDownload.InvokeRequired)
				prgDownload.Invoke(cssm, value);
			else
				prgDownload.Value = value;
		}
		void CrossSafeSetMaximumMethod(Int32 value)
		{
			if (prgDownload.InvokeRequired)
				prgDownload.Invoke(cssm, value);
			else
				prgDownload.Maximum = value;
		}
		void CrossSafeSetTextMethod(String text)
		{
			if (this.InvokeRequired)
				this.Invoke(csst, text);
			else
				this.Text = text;
		}

		private void Form1_Load(object sender, EventArgs e)
		{

			// 이벤트를 연결한다.
			wc.DownloadFileCompleted += new AsyncCompletedEventHandler(fileDownloadCompleted);
			wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(fileDownloadProgressChanged);
		}
		void fileDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{

			// e.BytesReceived
			//   받은 데이터의 크기를 저장합니다.

			// e.TotalBytesToReceive
			//   받아야 할 모든 데이터의 크기를 저장합니다.

			// 프로그레스바의 최대 크기가 정해지지 않은 경우,
			// 받아야 할 최대 데이터 량으로 설정한다.
			if (!setBaseSize)
			{
				CrossSafeSetMaximumMethod((int)e.TotalBytesToReceive);
				setBaseSize = true;
			}

			// 받은 데이터 량을 나타낸다.
			CrossSafeSetValueMethod((int)e.BytesReceived);

			// 받은 데이터 / 받아야할 데이터 (퍼센트) 로 나타낸다.
			CrossSafeSetTextMethod(String.Format("{0:N0} / {1:N0} ({2:P})", e.BytesReceived, e.TotalBytesToReceive, (Double)e.BytesReceived / (Double)e.TotalBytesToReceive));
		}

		void fileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			nowDownloading = false;
			btnStart.Enabled = true;
			txtAddress.Enabled = true;
			MessageBox.Show("파일 다운로드 완료!", "오류", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void BtnCloseClick(object sender, EventArgs e)
		{
			Application.Exit();
		}

	}
}
