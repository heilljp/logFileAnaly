using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace logFileAnaly
{
	public partial class frmWordFormat : Form
	{
		public frmWordFormat()
		{
			InitializeComponent();


            /*
             애플리케이션이 디버깅되는 동안 컨트롤의 핸들속성에 액세스하는 잘못된 스레드에 대한 호출을
                    catch할지를 나타내는 값입니다.
                    즉, 크로스 스레드 에러가 발생해도 표출시키지 마!!
             */
           CheckForIllegalCrossThreadCalls = false;
        }

		private void button1_Click(object sender, EventArgs e)
		{
            string filter = fileSelectFilter("TXT");

            // Retrieve the selected file path
            string filePath = ClsFile.selectLogFile(filter);  //로그파일 선택 


            if (string.IsNullOrEmpty(filePath))
            { 
                return;  //선택된 파일이 없으면 패스.
            }


            string[] readText = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
            string[] separatingStrings = { "@@@" };


            //MST_KTCH_ORD_MSG Insert Query :  필터적용
            var selectFindLogTxt = readText
                                        .Select(z => new {
                                            nm = z.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries)
                                        });



            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;

            int fProcess = 0;
            int fmax = 0;
            txtWordFormat.Clear();


            fmax = selectFindLogTxt.Count();
            progressBar1.Maximum = fmax;


            //if (fmax == 0) { enableTrue(true); };

            string tagFormat = @"        
<div class='body'>
<div><span name = 'eng'> @@@eng<br> <hr></span><span class='span'>@@@kor<br></span>
    <img src = '@@@lnk'/></div>
        <div>
            <input type= 'button' value= '듣기' onclick= ""wordSound('/mp3/@@@mp3.mp3');"" class='wordsound'> 
        </div>
</div>" ;


            string tagData = string.Empty;

            Thread th = new Thread(new ThreadStart(

                () =>
                {
                    foreach (var logdata in selectFindLogTxt)
                    {

                        fProcess++;

                        progressBar1.Value = fProcess;

                        var eng = logdata.nm[0].Trim();
                        var mp3 = logdata.nm[0].Trim().Replace(' ', '_');
                        var kor = logdata.nm[1].Trim();
                        var lnk = logdata.nm[2].Trim();

                        tagData += tagFormat.Replace("@@@eng", eng).Replace("@@@kor", kor).Replace("@@@lnk", lnk).Replace("@@@mp3", mp3);
                        tagData += Environment.NewLine;

                        Application.DoEvents();
 

                    }


                    txtWordFormat.Text = tagData; 

                }
            ));
            th.Start();
        }

        string fileSelectFilter(string gbn)
        {

            string rtnFilter = string.Empty;

            switch (gbn)
            {

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
    }
}
