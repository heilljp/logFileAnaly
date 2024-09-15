
namespace logFileAnaly
{
    partial class Frm_APIKDS_PICKUP
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Frm_APIKDS_PICKUP));
			this.btnSelectLog = new System.Windows.Forms.Button();
			this.btnSelectApiManager_IRT = new System.Windows.Forms.Button();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label2 = new System.Windows.Forms.Label();
			this.btnSelectAppTrace = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.txtERRLog = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.txtSearchTranNo = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.panel3 = new System.Windows.Forms.Panel();
			this.txtApiManagerParsing = new System.Windows.Forms.TextBox();
			this.txtApiManagerParsingSource = new System.Windows.Forms.TextBox();
			this.txrApiManager = new System.Windows.Forms.TextBox();
			this.txtVoidOrder = new System.Windows.Forms.TextBox();
			this.txrAppTrace = new System.Windows.Forms.TextBox();
			this.txtErrDate = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.button8 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.txtCheckIP = new System.Windows.Forms.TextBox();
			this.button6 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.txtJSON = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnSelectLog
			// 
			this.btnSelectLog.Location = new System.Drawing.Point(14, 7);
			this.btnSelectLog.Name = "btnSelectLog";
			this.btnSelectLog.Size = new System.Drawing.Size(248, 30);
			this.btnSelectLog.TabIndex = 0;
			this.btnSelectLog.Tag = "LOG";
			this.btnSelectLog.Text = "[SERVER]_로그파일선택_ApiKDS.Log";
			this.btnSelectLog.UseVisualStyleBackColor = true;
			this.btnSelectLog.Click += new System.EventHandler(this.btnSelectLog_Click);
			// 
			// btnSelectApiManager_IRT
			// 
			this.btnSelectApiManager_IRT.Location = new System.Drawing.Point(268, 7);
			this.btnSelectApiManager_IRT.Name = "btnSelectApiManager_IRT";
			this.btnSelectApiManager_IRT.Size = new System.Drawing.Size(288, 30);
			this.btnSelectApiManager_IRT.TabIndex = 5;
			this.btnSelectApiManager_IRT.Tag = "LOG";
			this.btnSelectApiManager_IRT.Text = "[SERVER]_로그파일선택_ApiManager.Log";
			this.btnSelectApiManager_IRT.UseVisualStyleBackColor = true;
			this.btnSelectApiManager_IRT.Click += new System.EventHandler(this.btnSelectApiManager_IRT_Click);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(14, 69);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(788, 10);
			this.progressBar1.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(395, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(0, 12);
			this.label2.TabIndex = 7;
			// 
			// btnSelectAppTrace
			// 
			this.btnSelectAppTrace.Location = new System.Drawing.Point(562, 7);
			this.btnSelectAppTrace.Name = "btnSelectAppTrace";
			this.btnSelectAppTrace.Size = new System.Drawing.Size(240, 30);
			this.btnSelectAppTrace.TabIndex = 8;
			this.btnSelectAppTrace.Tag = "LOG";
			this.btnSelectAppTrace.Text = "[kDS]_로그파일선택_AppTrace.Log";
			this.btnSelectAppTrace.UseVisualStyleBackColor = true;
			this.btnSelectAppTrace.Click += new System.EventHandler(this.btnSelectAppTrace_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.txtLog);
			this.splitContainer1.Panel1.Controls.Add(this.txtERRLog);
			this.splitContainer1.Panel1.Controls.Add(this.panel1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = new System.Drawing.Size(1656, 1192);
			this.splitContainer1.SplitterDistance = 379;
			this.splitContainer1.TabIndex = 9;
			// 
			// txtLog
			// 
			this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtLog.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtLog.Location = new System.Drawing.Point(980, 0);
			this.txtLog.MaxLength = 0;
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(417, 379);
			this.txtLog.TabIndex = 2;
			this.txtLog.WordWrap = false;
			// 
			// txtERRLog
			// 
			this.txtERRLog.Dock = System.Windows.Forms.DockStyle.Left;
			this.txtERRLog.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtERRLog.Location = new System.Drawing.Point(0, 0);
			this.txtERRLog.MaxLength = 0;
			this.txtERRLog.Multiline = true;
			this.txtERRLog.Name = "txtERRLog";
			this.txtERRLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtERRLog.Size = new System.Drawing.Size(980, 379);
			this.txtERRLog.TabIndex = 6;
			this.txtERRLog.WordWrap = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.txtSearchTranNo);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(1397, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(259, 379);
			this.panel1.TabIndex = 5;
			// 
			// txtSearchTranNo
			// 
			this.txtSearchTranNo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSearchTranNo.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtSearchTranNo.Location = new System.Drawing.Point(0, 26);
			this.txtSearchTranNo.Multiline = true;
			this.txtSearchTranNo.Name = "txtSearchTranNo";
			this.txtSearchTranNo.Size = new System.Drawing.Size(259, 353);
			this.txtSearchTranNo.TabIndex = 3;
			this.txtSearchTranNo.Text = resources.GetString("txtSearchTranNo.Text");
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(259, 26);
			this.label1.TabIndex = 4;
			this.label1.Text = "TRAN_NO";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.panel3);
			this.splitContainer2.Panel1.Controls.Add(this.txrApiManager);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.txtVoidOrder);
			this.splitContainer2.Panel2.Controls.Add(this.txrAppTrace);
			this.splitContainer2.Panel2.Controls.Add(this.txtErrDate);
			this.splitContainer2.Size = new System.Drawing.Size(1656, 809);
			this.splitContainer2.SplitterDistance = 379;
			this.splitContainer2.TabIndex = 0;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.txtApiManagerParsing);
			this.panel3.Controls.Add(this.txtApiManagerParsingSource);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(980, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(676, 379);
			this.panel3.TabIndex = 6;
			// 
			// txtApiManagerParsing
			// 
			this.txtApiManagerParsing.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtApiManagerParsing.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtApiManagerParsing.Location = new System.Drawing.Point(0, 105);
			this.txtApiManagerParsing.MaxLength = 0;
			this.txtApiManagerParsing.Multiline = true;
			this.txtApiManagerParsing.Name = "txtApiManagerParsing";
			this.txtApiManagerParsing.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtApiManagerParsing.Size = new System.Drawing.Size(676, 274);
			this.txtApiManagerParsing.TabIndex = 4;
			this.txtApiManagerParsing.WordWrap = false;
			this.txtApiManagerParsing.TextChanged += new System.EventHandler(this.txtApiManagerParsing_TextChanged);
			// 
			// txtApiManagerParsingSource
			// 
			this.txtApiManagerParsingSource.Dock = System.Windows.Forms.DockStyle.Top;
			this.txtApiManagerParsingSource.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtApiManagerParsingSource.Location = new System.Drawing.Point(0, 0);
			this.txtApiManagerParsingSource.MaxLength = 0;
			this.txtApiManagerParsingSource.Multiline = true;
			this.txtApiManagerParsingSource.Name = "txtApiManagerParsingSource";
			this.txtApiManagerParsingSource.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtApiManagerParsingSource.Size = new System.Drawing.Size(676, 105);
			this.txtApiManagerParsingSource.TabIndex = 5;
			this.txtApiManagerParsingSource.WordWrap = false;
			this.txtApiManagerParsingSource.TextChanged += new System.EventHandler(this.txtApiManagerParsingSource_TextChanged);
			// 
			// txrApiManager
			// 
			this.txrApiManager.Dock = System.Windows.Forms.DockStyle.Left;
			this.txrApiManager.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txrApiManager.Location = new System.Drawing.Point(0, 0);
			this.txrApiManager.MaxLength = 0;
			this.txrApiManager.Multiline = true;
			this.txrApiManager.Name = "txrApiManager";
			this.txrApiManager.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txrApiManager.Size = new System.Drawing.Size(980, 379);
			this.txrApiManager.TabIndex = 3;
			this.txrApiManager.WordWrap = false;
			// 
			// txtVoidOrder
			// 
			this.txtVoidOrder.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtVoidOrder.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtVoidOrder.Location = new System.Drawing.Point(373, 203);
			this.txtVoidOrder.MaxLength = 0;
			this.txtVoidOrder.Multiline = true;
			this.txtVoidOrder.Name = "txtVoidOrder";
			this.txtVoidOrder.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtVoidOrder.Size = new System.Drawing.Size(1283, 223);
			this.txtVoidOrder.TabIndex = 5;
			this.txtVoidOrder.WordWrap = false;
			// 
			// txrAppTrace
			// 
			this.txrAppTrace.Dock = System.Windows.Forms.DockStyle.Top;
			this.txrAppTrace.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txrAppTrace.Location = new System.Drawing.Point(373, 0);
			this.txrAppTrace.MaxLength = 0;
			this.txrAppTrace.Multiline = true;
			this.txrAppTrace.Name = "txrAppTrace";
			this.txrAppTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txrAppTrace.Size = new System.Drawing.Size(1283, 203);
			this.txrAppTrace.TabIndex = 3;
			this.txrAppTrace.WordWrap = false;
			// 
			// txtErrDate
			// 
			this.txtErrDate.Dock = System.Windows.Forms.DockStyle.Left;
			this.txtErrDate.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtErrDate.Location = new System.Drawing.Point(0, 0);
			this.txtErrDate.MaxLength = 0;
			this.txtErrDate.Multiline = true;
			this.txtErrDate.Name = "txtErrDate";
			this.txtErrDate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtErrDate.Size = new System.Drawing.Size(373, 426);
			this.txtErrDate.TabIndex = 4;
			this.txtErrDate.WordWrap = false;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.button8);
			this.panel2.Controls.Add(this.button7);
			this.panel2.Controls.Add(this.txtCheckIP);
			this.panel2.Controls.Add(this.button6);
			this.panel2.Controls.Add(this.button5);
			this.panel2.Controls.Add(this.comboBox2);
			this.panel2.Controls.Add(this.comboBox1);
			this.panel2.Controls.Add(this.button4);
			this.panel2.Controls.Add(this.button3);
			this.panel2.Controls.Add(this.button2);
			this.panel2.Controls.Add(this.button1);
			this.panel2.Controls.Add(this.btnSelectAppTrace);
			this.panel2.Controls.Add(this.btnSelectApiManager_IRT);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.btnSelectLog);
			this.panel2.Controls.Add(this.progressBar1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 1192);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1656, 86);
			this.panel2.TabIndex = 10;
			this.panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.panel2_Paint);
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(14, 42);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(542, 30);
			this.button8.TabIndex = 18;
			this.button8.Tag = "LOG";
			this.button8.Text = "[POS]_FrmeWork_로그파일선택_FrameWork_xx.Log";
			this.button8.UseVisualStyleBackColor = true;
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(1401, 44);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(138, 30);
			this.button7.TabIndex = 17;
			this.button7.Tag = "LOG";
			this.button7.Text = "network_check2";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// txtCheckIP
			// 
			this.txtCheckIP.Location = new System.Drawing.Point(1401, 16);
			this.txtCheckIP.Name = "txtCheckIP";
			this.txtCheckIP.Size = new System.Drawing.Size(132, 21);
			this.txtCheckIP.TabIndex = 16;
			this.txtCheckIP.Text = "10.85.2.61";
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(1545, 10);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(138, 30);
			this.button6.TabIndex = 15;
			this.button6.Tag = "LOG";
			this.button6.Text = "network_check";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(1545, 46);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(138, 30);
			this.button5.TabIndex = 14;
			this.button5.Tag = "LOG";
			this.button5.Text = "Webclient Form";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// comboBox2
			// 
			this.comboBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Items.AddRange(new object[] {
            "10.84.21.110 BK-REAL",
            "10.84.21.120 BK-QA",
            "10.84.21.130 BK-TEST",
            "10.84.21.10 TH-REAL",
            "10.84.21.20 TH-QA",
            "10.84.21.30 TH-TEST",
            "10.84.21.53 BKJH-EXPO-TEST",
            "",
            "10.85.2.62 BKR_DEV_PC"});
			this.comboBox2.Location = new System.Drawing.Point(1182, 26);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(213, 20);
			this.comboBox2.TabIndex = 13;
			this.comboBox2.Text = "10.84.21.130 BK-TEST";
			this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
			// 
			// comboBox1
			// 
			this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "00099998",
            "00000160",
            "00000160",
            "00099994",
            "20004100",
            "20004100",
            "Z0099998"});
			this.comboBox1.Location = new System.Drawing.Point(1182, 52);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(197, 20);
			this.comboBox1.TabIndex = 11;
			this.comboBox1.Text = "00000160";
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(986, 10);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(138, 30);
			this.button4.TabIndex = 12;
			this.button4.Tag = "LOG";
			this.button4.Text = "주문전송일시정지";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(1038, 46);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(138, 30);
			this.button3.TabIndex = 11;
			this.button3.Tag = "LOG";
			this.button3.Text = "주문전송정지";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(817, 46);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(215, 30);
			this.button2.TabIndex = 10;
			this.button2.Tag = "LOG";
			this.button2.Text = "주문로그전송";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(817, 10);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(163, 30);
			this.button1.TabIndex = 9;
			this.button1.Tag = "LOG";
			this.button1.Text = "주문로그읽기";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// txtJSON
			// 
			this.txtJSON.Dock = System.Windows.Forms.DockStyle.Right;
			this.txtJSON.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
			this.txtJSON.Location = new System.Drawing.Point(1656, 0);
			this.txtJSON.MaxLength = 0;
			this.txtJSON.Multiline = true;
			this.txtJSON.Name = "txtJSON";
			this.txtJSON.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtJSON.Size = new System.Drawing.Size(451, 1278);
			this.txtJSON.TabIndex = 11;
			this.txtJSON.WordWrap = false;
			// 
			// Frm_APIKDS_PICKUP
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(2107, 1278);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.txtJSON);
			this.Name = "Frm_APIKDS_PICKUP";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "api_KDS_로그파일분석";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Frm_APIKDS_PICKUP_FormClosing);
			this.Load += new System.EventHandler(this.Frm_APIKDS_PICKUP_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectLog;
        private System.Windows.Forms.Button btnSelectApiManager_IRT;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectAppTrace;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txrApiManager;
        private System.Windows.Forms.TextBox txrAppTrace;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtSearchTranNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtApiManagerParsing;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txtApiManagerParsingSource;
        private System.Windows.Forms.TextBox txtErrDate;
        private System.Windows.Forms.TextBox txtERRLog;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.TextBox txtVoidOrder;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.TextBox txtCheckIP;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.TextBox txtJSON;
	}
}

