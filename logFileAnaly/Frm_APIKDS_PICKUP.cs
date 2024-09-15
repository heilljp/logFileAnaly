using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace logFileAnaly
{
    public partial class Frm_APIKDS_PICKUP : Form
    {
        public Frm_APIKDS_PICKUP()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            txtVoidOrder.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);
            txrAppTrace.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

        }

		private void KeyEvent(object sender, KeyEventArgs e)
		{
            if (e.KeyCode == Keys.F3)
            {
                //System.Diagnostics.Debug.Print(txtVoidOrder.SelectedText);
                //txtJSON.Text = txtVoidOrder.SelectedText.Trim();

                txtJSON.Clear();
                string selectString = string.Empty;

                var txtbox = (TextBox)sender;
                switch (txtbox.Name) {
                    case "txtVoidOrder":
                            selectString = txtVoidOrder.SelectedText.Trim();
                            selectString = selectString.Substring(1, selectString.Length - 2).Trim();
                            break;

                    case "txrAppTrace":
                            selectString = txrAppTrace.SelectedText.Trim();
                        selectString = selectString.Replace("/NewOrder|", string.Empty).Replace("/VoidOrder|", "");
                            break; 
                }

                try
                {

                    JObject jobject = JObject.Parse(selectString);


                    txtJSON.Text = "CommHeader" + System.Environment.NewLine + jobject["CommHeader"].ToString() + System.Environment.NewLine;
                    txtJSON.Text += "OrderHeader" + System.Environment.NewLine + jobject["OrderHeader"].ToString() + System.Environment.NewLine;
                    txtJSON.Text += "OrderItemList" + System.Environment.NewLine + jobject["OrderItemList"].ToString() + System.Environment.NewLine;

                }
                catch (Exception es) {

                    System.Diagnostics.Debug.Print("ERR >>>> JSON 형식 확인요청 : " + System.Environment.NewLine +  es.ToString());
                }
            }
        }

		string fileSelectFilter(string gbn) {

            string rtnFilter = string.Empty;

            switch (gbn) {

                case "TXT":
                    rtnFilter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"; ; 
                    break;

                case "LOG":
                    rtnFilter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*"; ;
                    break;

                case "BAK":
                    rtnFilter = "bak files (*.bak)|*.bak|All files (*.*)|*.*"; ;
                    break;
            }

            return rtnFilter; 
        }

        void enableTrue(Boolean TF) {

            foreach (Control btn in panel2.Controls)
            {
                if (btn.Tag != null)
                {

                    if (btn.Tag.Equals("LOG"))
                    {
                        btn.Enabled = TF;
                    }
                }
            }
        }

        private void btnSelectLog_Click(object sender, EventArgs e)
        {

            enableTrue(false);

            string filter = fileSelectFilter("LOG");

            // Retrieve the selected file path
            string filePath =  ClsFile.selectLogFile(filter);  //로그파일 선택 


            if (string.IsNullOrEmpty(filePath))
            {
                enableTrue(true);
                return;  //선택된 파일이 없으면 패스.
            }


            string[] readText = File.ReadAllLines(filePath);   //로그파일 읽기

            //픽업 정보 필터적용
            var selectFindLogTxt = readText
                                        .Where(x => x.Contains("Start Pickup   SaleDate") ||
                                                    x.Contains("Start Complete SaleDate") ||
                                                    x.Contains("Company="))
                                        .Select(z=> new {
                                            nm = z.Split(' ')
                                        });

            //픽업 정보 필터적용 [ERR]
            var selectFindERRLogTxt = readText
                                        .Where(x => x.Contains("ERR") ||
                                                    x.Contains("Exception") ||
                                                    x.Contains("at ") ||
                                                    x.Contains("Error")
                                                    
                                        )
                                        .Select(z => new {
                                            nm = z.Split(' ')
                                        });


            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;

            int fProcess = 0;
            int fmax = 0;
            txtLog.Clear();
            txtERRLog.Clear();


            fmax = selectFindLogTxt.Count();
            progressBar1.Maximum = fmax;

            //
            if (fmax == 0) { enableTrue(true); };



            Thread th = new Thread(new ThreadStart(

                () =>
                {
                    foreach (var logdata in selectFindLogTxt)
                    {
                        fProcess++;

                        progressBar1.Value = fProcess;

                        if (logdata.nm.Count() < 23)
                        {
                            string strGbn = string.Empty;

                            switch (logdata.nm[16]) {

                                case "Complete":
                                    strGbn = "==";
                                    break;
                                case "RecallDID":
                                    strGbn = "@@";
                                    break;

                            }

                            logdata.nm[16] = strGbn  + logdata.nm[16]  + strGbn + "\t"; 
                            txtLog.Text += string.Join(" ", logdata.nm) + Environment.NewLine;

                        }
                        else
                        {
                            logdata.nm[16] = "<<" + logdata.nm[16] + ">>" + "\t";

                            if (logdata.nm.Length < 23) {
                                //tran No 필터 적용
                                var d = logdata.nm[23].Replace("TranNo=", "").Replace("\"", "");


                                if (txtSearchTranNo.Text.Contains(d))
                                {
                                    if (!string.IsNullOrEmpty(d))
                                    {
                                        Console.WriteLine(d);
                                        txtLog.Text += string.Join(" ", logdata.nm) + Environment.NewLine;

                                    }
                                }
                            }
                           
                        }

                        Application.DoEvents();

                        if (fmax.Equals(fProcess))
                        {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";


                            enableTrue(true);
                        }


                        txtLog.Refresh();

                    }
                }
            ));
            th.Start();


            Thread th2 = new Thread(new ThreadStart(

                () =>
                {
                    foreach (var logdata in selectFindERRLogTxt)
                    {


                        if (logdata.nm.Contains("ERR"))
                        {
                            if (string.IsNullOrEmpty(txtERRLog.Text.Trim()))
                            {
                                txtERRLog.Text += string.Join(" ", logdata.nm) + " -----------------------------------------------------------------------" + Environment.NewLine;
                            }
                            else
                            {
                                txtERRLog.Text += Environment.NewLine + string.Join(" ", logdata.nm) + " -----------------------------------------------------------------------" + Environment.NewLine;
                            }

                            
                        }
                        else {
                            txtERRLog.Text += string.Join(" ", logdata.nm) + Environment.NewLine;
                        }
                         
                        Application.DoEvents();

                        txtERRLog.Refresh();

                    }
                }
            ));
            th2.Start();
        }



        /// <summary>
        /// ApiManager log IRT 통신 로그 확인용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectApiManager_IRT_Click(object sender, EventArgs e)
        {

            enableTrue(false);

            string filter =fileSelectFilter("LOG");

            // Retrieve the selected file path
            string filePath = ClsFile.selectLogFile(filter);  //로그파일 선택 

            if (string.IsNullOrEmpty(filePath))
            {
                enableTrue(true);
                return;  //선택된 파일이 없으면 패스.
            }

            string[] readText = File.ReadAllLines(filePath);   //로그파일 읽기

            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0; 

            //픽업 정보 필터적용
            var selectFindLogTxt = readText
                                        .Where(x =>
                                                    x.Contains(":9011] SendData") ||
                                                    x.Contains(":9011] RecvData") ||
                                                    x.Contains(":9020] SendData") ||
                                                    x.Contains(":9020] RecvData") ||

                                                    x.Contains("[e-bps.bkr.co.kr:9011] SendData") ||
                                                    x.Contains("[e-bps.bkr.co.kr:9011] RecvData") ||

                                                    x.Contains("[e-bps.burgerking.co.jp:9011] SendData") || 
                                                    x.Contains("[e-bps.burgerking.co.jp:9011] RecvData") ||
                                                    x.Contains("[10.11.90.31:9011] RecvData") ||
                                                    x.Contains("[10.11.90.31:9011] SendData")

                                                    
                                                    );             
            int fProcess = 0;
            int fmax = 0;


            fmax = selectFindLogTxt.Count();
            progressBar1.Maximum = fmax;
            txrApiManager.Clear();


            if (fmax == 0) { enableTrue(true); };


            Thread th = new Thread(new ThreadStart(

                () => {

                    foreach (var logdata in selectFindLogTxt)
                    {
                        fProcess++;

                        progressBar1.Value = fProcess;


                        Console.WriteLine("rogressBar1.Value : " + progressBar1.Value);

                        string rdata = logdata
                                            .Replace("BK.KDS.Manager.dll  ", "")
                                            .Replace("SocketClient.", "")
                                            .Replace("SendProcess()", "[SEND]")
                                            .Replace("RecvVarDataProcess()", "[RECV]")
                                            .Replace("                ", "->")
                                            .Replace("         ", "->")
                                            .Replace("Host=", "")
                                            .Replace("RecvData=","")
                                            .Replace("SendData=", "") 
                                            ;

                        txrApiManager.Text += string.Join(" ", rdata) + Environment.NewLine;


                        Application.DoEvents();

                        if (fmax.Equals(fProcess)) {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";

                            enableTrue(true);
                        }
                    }
                }

            ));
            th.Start();
        }

        private void btnSelectAppTrace_Click(object sender, EventArgs e)
        {
             

            enableTrue(false);

            string filter = fileSelectFilter("TXT");

            // Retrieve the selected file path
            string filePath = ClsFile.selectLogFile(filter);  //로그파일 선택 

            if (string.IsNullOrEmpty(filePath)) {
                enableTrue(true);
                return;  //선택된 파일이 없으면 패스.
            }
            

            string[] readText = File.ReadAllLines(filePath);   //로그파일 읽기

            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;


            //픽업 정보 필터적용
            var selectFindLogTxt = readText
                                        .Where(x =>
                                                    x.Contains("[ERR]") ||
                                                    x.Contains(".WebException")

                                                    );
            int fProcess = 0;
            int fmax = 0;
            txrAppTrace.Clear();
            txtErrDate.Clear();

            fmax = selectFindLogTxt.Count();
            progressBar1.Maximum = fmax;

            if (fmax == 0) { enableTrue(true); };

            Thread th = new Thread(new ThreadStart(

                 () => {

                     foreach (var logdata in selectFindLogTxt)
                     {
                         fProcess++;

                         progressBar1.Value = fProcess;
                          
                         Console.WriteLine("rogressBar1.Value : " + progressBar1.Value);


                         string rdata = logdata
                                             .Replace(": BK.KDS.dll     ApiHandler.ApiSendRecv()                  ", "")
                                             .Replace("Exception : ", "\t")
                                             .Replace("URL Suffix ", " ----> ");
                                             ;


                         string spritData = string.Empty;

                         if (rdata.Split('[').Length > 1) {
                             spritData = rdata.Split('[')[0].ToString().Trim() + System.Environment.NewLine;
                         }

                         txtErrDate.Text += spritData;
                         txrAppTrace.Text += string.Join(" ", rdata) + Environment.NewLine;


                         Application.DoEvents();

                         if (fmax.Equals(fProcess))
                         {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";
                             enableTrue(true);
                         }
                     }
                 }

             ));
            th.Start();

        }
 
        private void txtApiManagerParsing_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtApiManagerParsingSource_TextChanged(object sender, EventArgs e)
        {
            string strSprint = txtApiManagerParsingSource.Text.Trim();
            string[] separatingStrings = { "#~" };

            var splitData = strSprint.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);

            txtApiManagerParsing.Clear();

            foreach (var d in splitData)
            {
                //Console.WriteLine(d);
                txtApiManagerParsing.Text += d + Environment.NewLine;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filter = fileSelectFilter("LOG");

            // Retrieve the selected file path
            string filePath = ClsFile.selectLogFile(filter);  //로그파일 선택 


            if (string.IsNullOrEmpty(filePath))
            {
                enableTrue(true);
                return;  //선택된 파일이 없으면 패스.
            }

            string[] readText = File.ReadAllLines(filePath);   //로그파일 읽기

            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;

            //픽업 정보 필터적용
            var selectFindLogTxt = readText
                                        .Where(x =>
                                                    x.Contains("NewOrder")   

                                                    );


            var selectFindVoidLogTxt = readText
                                        .Where(x =>
                                                    x.Contains("VoidOrder")

                                                    );



            int fProcess = 0;
            int fmax = 0;


            fmax = selectFindLogTxt.Count();
            progressBar1.Minimum = 0; 
            progressBar1.Maximum = fmax;
            txrAppTrace.Clear();
            txtVoidOrder.Clear();

            if (fmax == 0) { enableTrue(true); };



            //int rowcount = 0; 

            Thread th = new Thread(new ThreadStart(

                () => {

                    foreach (var logdata in selectFindLogTxt)
                    {
                        //if (rowcount > 160) break;    //160 개 주문정도만 로딩 발송..
                        //rowcount++; 


                        fProcess++;

                        progressBar1.Value = fProcess;


                        Console.WriteLine("rogressBar1.Value : " + progressBar1.Value);

                        string rdata = string.Empty;

                        if (logdata.IndexOf("Start NewOrder") > 0)
                        {
                            //rdata = logdata.Substring(logdata.IndexOf("Start NewOrder")).Replace("\\", "").Replace("Start NewOrder ", "");
                            continue;
                        }
                        else if (logdata.IndexOf("NewOrder Data") > 0)
                        {
                            rdata= logdata.Substring(logdata.IndexOf("NewOrder Data")).Replace("\\", "").Replace("NewOrder Data=\"", "");
                            rdata = rdata.Substring(0, rdata.Length - 1);

                            txrAppTrace.Text += ( rdata + "/NewOrder|" + Environment.NewLine ) ;
                        }

                        Application.DoEvents();

                        if (fmax.Equals(fProcess))
                        {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";

                            enableTrue(true);
                        }
                    }
                     
                     
                    foreach (var logdata in selectFindVoidLogTxt)
                    {
                        //if (rowcount > 160) break;    //160 개 주문정도만 로딩 발송..
                        //rowcount++; 

                        string rdata = string.Empty;

                        if (logdata.IndexOf("Start NewOrder") > 0)
                        {
                            //rdata = logdata.Substring(logdata.IndexOf("Start NewOrder")).Replace("\\", "").Replace("Start NewOrder ", "");
                            continue;
                        }
                        else if (logdata.IndexOf("VoidOrder Data") > 0)
                        {
                            rdata = logdata.Substring(logdata.IndexOf("VoidOrder Data")).Replace("\\", "").Replace("VoidOrder Data=\"", "");
                            rdata = rdata.Substring(0, rdata.Length - 1);

                            txrAppTrace.Text += ( rdata + "/VoidOrder|" + Environment.NewLine );
                        }

                        

                        Application.DoEvents();

                        /*
                        if (fmax.Equals(fProcess))
                        {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";

                            enableTrue(true);
                        }

                        */
                    }
                     


                }

            ));
            th.Start();
        }


        //string NowOrderURL = "http://10.85.2.60:5000/api/Order/NewOrder";
        string NowOrderURL = "http://10.85.2.61:5000/api/Order/NewOrder";
        string OrderURL = "http://{0}:5000/api/Order/NewOrder";
        string ValidOrderURL = "http://{0}:5000/api/Order/VoidOrder";


        public string callWebClient()
        {
            string result = string.Empty;
            string svrIP = string.Empty;
            string svrURL = string.Empty;

            svrIP = this.comboBox2.Text.Trim();
            svrURL = string.Format(OrderURL, svrIP); 

            try
            {
                WebClient client = new WebClient();

                //특정 요청 헤더값을 추가해준다. 
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                using (Stream data = client.OpenRead(svrURL))
                {
                    using (StreamReader reader = new StreamReader(data))
                    {
                        string s = reader.ReadToEnd();
                        result = s;

                        reader.Close();
                        data.Close();
                    }
                }

            }
            catch (Exception e)
            {
                //통신 실패시 처리로직
                Console.WriteLine(e.ToString());
            }
            return result;
        }


        public string callWebRequest()
        {
            string responseFromServer = string.Empty;

            try
            {

                WebRequest request = WebRequest.Create(NowOrderURL);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers["user-agent"] = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                using (WebResponse response = request.GetResponse())
                using (Stream dataStream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(dataStream))
                {
                    responseFromServer = reader.ReadToEnd();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return responseFromServer;


        }


        public object lock1 = new object();
        public object lock2 = new object();

        private async void button2_Click(object sender, EventArgs e)
        {

            await ServerDataSendData(); 

            /*

            sendDAta = arrySendData[0]; 


            JObject json = JObject.Parse(sendDAta);

            json["CommHeader"]["StoreCode"] = textStoreCode.Text;
            json["CommHeader"]["SystemDate"] = DateTime.Now.ToString("yyyyMMddhhmm");

            json["OrderHeader"]["SaleDate"] = DateTime.Now.ToString("yyyyMMdd");
            json["OrderHeader"]["SystemDate"] = DateTime.Now.ToString("yyyyMMddhhmm");


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(NowOrderURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
 
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();


                this.txtErrDate.Text = result; 
            }
            */



            /*
            foreach (var eeee in json)
            {
                Console.WriteLine(eeee);
            }
            */


        }

        async Task ServerDataSendData() {



            string sendDAta = txtApiManagerParsing.Text.Trim();


            // if (string.IsNullOrEmpty(sendDAta)) return;

            try
            {


                var arrySendData = txrAppTrace.Text.Split('|');

                OrderURL = "http://{0}:5000/api/Order/NewOrder";
                Task t = orderSendData(arrySendData);
                //Task t = orderSendData(arrySendData, "NewOrder");



                /*
                await Task.Delay(5000);
                //await t;


                var arryVoidSendData = txtVoidOrder.Text.Split('|');

                OrderURL = "http://{0}:5000/api/Order/VoidOrder"; ;
                Task t2 = orderSendData(arryVoidSendData, "VoidOrder");

                await t2;
                */


            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.Print("ERROR : >> " + ex.Message);
            }

        }


        Thread g_th; 

        async Task orderSendData(string[] arrySendData) {

            int rowcount = 0;

            int fProcess = 0;
            int fmax = 0;


            string svrIP = this.comboBox2.Text.Split(' ')[0].Trim();
            string svrURL = string.Format(OrderURL, svrIP);
            string svrValidURL = string.Format(ValidOrderURL, svrIP); 

            fmax = arrySendData.Length;

            progressBar1.Minimum = 0; 
            progressBar1.Maximum = fmax;
            txrApiManager.Clear();

            string storecode = string.Empty;
            storecode = comboBox1.Text.Trim();

            g_th = new Thread(new ThreadStart(

                () => {

                    foreach (var logdata in arrySendData)
                    {


                        try 
                        {
                            //Task.Delay(2000).Wait();

                            txrApiManager.Text = logdata + Environment.NewLine + txrApiManager.Text;
                            fProcess++;


                            if (string.IsNullOrEmpty(logdata)) continue;

                            if (logdata.Equals(Environment.NewLine)) continue;

                            //string strJson = string.Empty;
                            //strJson = logdata.Substring(0, logdata.LastIndexOf("/NewOrder"));

                            string[] separatingStrings = { "/NewOrder" };

                            JObject json = JObject.Parse(logdata.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)[0]);
                            //JObject json = JObject.Parse(strJson);

                            json["CommHeader"]["StoreCode"] = storecode; // "00000160"; // "00000160";//"00000027"; // textStoreCode.Text;  테스트 매장 : 종로구청 : 00000160
                            json["CommHeader"]["SystemDate"] = DateTime.Now.ToString("yyyyMMddhhmm");

                            //if (orderGbn.Equals("NewOrder")) {
                            //if (logdata.Split('/')[1].Equals("NewOrder"))
                            if(logdata.Contains("NewOrder"))
                            {
                                // progressBar1.Value = fProcess;

                                json["OrderHeader"]["SaleDate"] = DateTime.Now.ToString("yyyyMMdd");
                                json["OrderHeader"]["SystemDate"] = DateTime.Now.ToString("yyyyMMddhhmm");

                                //json["OrderHeader"]["OrderTime"] = DateTime.Now.ToString("yyyyMMddhhmm");

                                svrURL = string.Format(OrderURL, svrIP);
                            }
                            else
                            {
                                json["SaleDate"] = DateTime.Now.ToString("yyyyMMdd");

                                svrURL = string.Format(ValidOrderURL, svrIP);
                            }
                            
                            var httpWebRequest = (HttpWebRequest)WebRequest.Create(svrURL);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";

                            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            {
                                /*
                                string json = new JavaScriptSerializer().Serialize(new
                                {
                                    Username = "myusername",
                                    Password = "pass"
                                });
                                */

                                streamWriter.Write(json);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }

                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = streamReader.ReadToEnd();


                                this.txtErrDate.Text = result;
                            }
                        }
                        catch (Exception ex ) {
                            System.Diagnostics.Debug.Print("ERROR : >> " + ex.Message);
                        }
                        System.Diagnostics.Debug.Print("---->" + logdata.Split('/')[1].ToString());
                    }

                    
                }
                 

            ));
            g_th.Start();

        }

        private void Frm_APIKDS_PICKUP_Load(object sender, EventArgs e)
        {

        }

        private void Frm_APIKDS_PICKUP_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (g_th == null) return; 

            //강제 종료 시
            g_th.Interrupt();

            if (g_th.ThreadState != ThreadState.Suspended) {
                g_th.Abort(); //종료     
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //강제 종료 시
            g_th.Interrupt();

            if (g_th.ThreadState != ThreadState.Suspended)
            {
                g_th.Abort(); //종료     
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (g_th.ThreadState == ThreadState.Suspended)
            {
                button4.Text = "중지";
                g_th.Resume();
            }
            else 
            {

                if (g_th.IsAlive == false) return; 
                button4.Text = "계속실행";
                g_th.Suspend();    
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

		private void button5_Click(object sender, EventArgs e)
		{
            Form frm = new Form1();
            frm.ShowDialog();
		}

		private void button6_Click(object sender, EventArgs e)
		{
            Console.WriteLine("-------------> " + IsCheckNetwork().ToString());


        }

        /// <summary>
        /// 네트워크 연결 확인 체크 .......
        /// </summary>
        /// <returns></returns>
        private bool IsCheckNetwork() {

            bool networkState = NetworkInterface.GetIsNetworkAvailable();
            bool pingResult = true;

            string checkIP = txtCheckIP.Text.Trim();

            if (string.IsNullOrEmpty(checkIP)) return false;

            //네트워크가 연결이 되어 있다면..
            if (networkState) {

                string addr = checkIP; // "10.85.2.88";  //상태체크 아이피 
                Ping pingSender = new Ping();

                //ping 체크 (IP, TimeOut 지정)
                PingReply reply = pingSender.Send(addr, 300);

                pingResult = reply.Status == IPStatus.Success;
            }

            return networkState & pingResult;
        }

		private void button7_Click(object sender, EventArgs e)
		{


            //테스트 더 해봐야 할 듯..... 소켓 excepton  이 계속 발생하네.. 
            string checkIP = txtCheckIP.Text.Trim();
            int checkPort = 9100;

           // bool rtnNetCheck = Check_TcpPort(checkIP, 9100);
            //Console.WriteLine("TCP / PORT check : " + rtnNetCheck.ToString());


            bool rtnNetCheck = Check_TcpPort2(checkIP, 9100);
            Console.WriteLine("TCP / PORT check : " + rtnNetCheck.ToString());


        }


        private bool Check_TcpPort2(string dstIp, int dstPort) {

            int connectTimeoutMilliseconds = 1000;
            var tcpClient = new TcpClient();
            var connectionTask = tcpClient.ConnectAsync(dstIp, dstPort).ContinueWith(task =>
            {
                return task.IsFaulted ? null : tcpClient;
            }, TaskContinuationOptions.ExecuteSynchronously);

            var timeoutTask = Task.Delay(connectTimeoutMilliseconds)
                .ContinueWith<TcpClient>(task => null, TaskContinuationOptions.ExecuteSynchronously);

            var resultTask = Task.WhenAny(connectionTask, timeoutTask).Unwrap();
            var resultTcpClient = resultTask.GetAwaiter().GetResult();

            if (resultTcpClient != null)
            {
                // Connected!
                return true; 
            }
            else
            {
                // Not connected
                return false; 
            }
        }



        private bool Check_TcpPort(string dstIp, int dstPort)

        {
            
            try
            {
                var tcpClient = new TcpClient();
                int dstTimeout = 500;  // msec

                if (tcpClient.ConnectAsync(dstIp, dstPort).Wait(dstTimeout))
                {
                    
                    return true;
                }
                else {
                    //tcpClient.Close();
                    return false;
                }
            }

            catch (Exception ex)
            { 

                Console.WriteLine(ex.Message);
                
                return false;
            }

        }

		private void button8_Click(object sender, EventArgs e)
		{

            enableTrue(false);

            string filter = fileSelectFilter("LOG");

            // Retrieve the selected file path
            string filePath = ClsFile.selectLogFile(filter);  //로그파일 선택 


            if (string.IsNullOrEmpty(filePath))
            {
                enableTrue(true);
                return;  //선택된 파일이 없으면 패스.
            }


            string[] readText = File.ReadAllLines(filePath, System.Text.Encoding.GetEncoding("euc-kr"));
            
            //MST_KTCH_ORD_MSG Insert Query :  필터적용
                        var selectFindLogTxt = readText
                                        .Where(x => x.Contains("MST_KTCH_ORD_MSG Insert Query :") ||
                                                    x.Contains("MST_KTCH_ORD_MSG htKDSSendingItem count :") ||
                                                    x.Contains("KDS 전송 JSON: [")


                                                    )
                                        .Select(z => new {
                                            nm = z.Split(' ')
                                            //nm = z
                                        });

            //픽업 정보 필터적용 [ERR]
            /*
            var selectFindERRLogTxt = readText
                                        .Where(x => x.Contains("NewOrder Start: ")  
                                        )
                                        .Select(z => new {
                                            nm = z.Split(' ')
                                        });

            */

            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;

            int fProcess = 0;
            int fmax = 0;
            txtVoidOrder.Clear();
            txtERRLog.Clear();


            fmax = selectFindLogTxt.Count();
            progressBar1.Maximum = fmax;

            //
            if (fmax == 0) { enableTrue(true); };

            Thread th = new Thread(new ThreadStart(

                () =>
                {
                    foreach (var logdata in selectFindLogTxt)
                    {
                        fProcess++;

                        progressBar1.Value = fProcess;

                        if (logdata.nm.Count() > 100)
                        {
                            string strGbn = string.Empty;

                            switch (logdata.nm[16])
                            {

                                case "MST_KTCH_ORD_MSG":
                                    strGbn = "==";
                                    break;
                                case "RecallDID":
                                    strGbn = "@@";
                                    break;

                            }

                            //logdata.nm[16] = strGbn + logdata.nm[16] + strGbn + "\t";

                            var data = string.Join(" ", logdata.nm);

                            data =  data.Substring(data.IndexOf("INSERT INTO POSMST..MST_KTCH_ORD_MSG"));

                            txtVoidOrder.Text += data + Environment.NewLine;

                        }
                        else
                        {
                            if (logdata.nm[56].Equals("KDS"))
                            {
                                var data = string.Join(" ", logdata.nm);

                                data = data.Substring(data.IndexOf("KDS 전송 JSON: [")).Replace("KDS 전송 JSON: ","");
                                txtVoidOrder.Text += data + Environment.NewLine + Environment.NewLine;

                            }
                            else {

                                logdata.nm[16] = "<<" + logdata.nm[16] + ">>" + "\t";

                                if (logdata.nm.Length < 23)
                                {
                                    //tran No 필터 적용
                                    var d = logdata.nm[23].Replace("TranNo=", "").Replace("\"", "");


                                    if (txtSearchTranNo.Text.Contains(d))
                                    {
                                        if (!string.IsNullOrEmpty(d))
                                        {
                                            Console.WriteLine(d);
                                            txtLog.Text += string.Join(" ", logdata.nm) + Environment.NewLine;

                                        }
                                    }
                                }

                            }

                        }

                        Application.DoEvents();

                        if (fmax.Equals(fProcess))
                        {
                            //MessageBox.Show("loading finished 1!!");
                            label2.Text = "loading finished !!!";


                            enableTrue(true);
                        }


                        txtLog.Refresh();

                    }
                }
            ));
            th.Start();



        }
         
	}
}
