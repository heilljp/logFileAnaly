using BK.KDS.Common;
using BK.KDS.Manager;
using BK.KDS.Models;
using BK.KDS.Processes;
using BK.KDS.Services;
using BK.KDS.ViewExtensions;
using BK.KDS.ViewModels.Main;
using BK.KDS.Views.Sys;

using FKL.ApiServer.Biz.ApiReference.ApiItem;
using FKL.Framework.UI.Xamarin;
using FKL.Framework.UI.Xamarin.Component;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Xamarin.Essentials;
using Xamarin.Forms;

namespace BK.KDS.Views.Main
{
    [PageInfo(nameof(KdsMain))]
    public class KdsMain : ContentPageEx, INotifyPropertyChanged, IPageController
    {
        /// <summary>
        /// 주문정보 리스트
        /// </summary>
        private List<MenuInfo> _ordInfoList;
        /// <summary>
        /// 주문메뉴 리스트
        /// </summary>
        private List<MenuList> _ordMenuList;
        /// <summary>
        /// Void(Cancel) Label 리스트
        /// </summary>
        private List<Label> _ordVoidLabelList;
        /// <summary>
        /// 데이터 리스트 => 서버에서 받은 데이터
        /// </summary>
        private List<OrderInfo> _dataList;
        /// <summary>
        /// 서버에서 받은 데이터를 페이지 별로 관리한 데이터 리스트 (page, page Datas)
        /// </summary>
        private Dictionary<int, List<OrderInfo>> _datas;
        /// <summary>
        /// Summary 데이터 리스트
        /// </summary>
        private List<OrderInfo.MenuItem> _summaryDataList;
        /// <summary>
        /// Revert 데이터 리스트
        /// </summary>
        private List<RevertInfo> _revertList;
        /// <summary>
        /// 주문 복원을 위해 가지고 있는 데이터 최대 수
        /// </summary>
        private const int MAX_REVERT_COUNT = 5;

        /// <summary>
        /// 화면 row 개수
        /// </summary>
        private int _rowCount;
        /// <summary>
        /// 화면 col 개수
        /// </summary>
        private int _colCount;
        /// <summary>
        /// 주문 메뉴 수
        /// </summary>
        private int _lineCount;
        /// <summary>
        /// 화면 Count (8 ,10, 12, 14)
        /// </summary>
        private int _dispCount;

        /// <summary>
        /// 메인 화면 구성 Grid
        /// </summary>
        private Grid _grid;
        /// <summary>
        /// 하단 정보 및 버튼 Grid
        /// </summary>
        private Grid _bottomGrid;
        /// <summary>
        /// 써머리 
        /// </summary>
        private Summary _summary;

        /// <summary>
        /// 하단 정보 Label(KDS Name, No, Date, Pgm Version) 
        /// </summary>
        private Label _bottomInfo1;
        /// <summary>
        /// 하단 정보 (Page)
        /// </summary>
        private Label _bottomInfo2;
        /// <summary>
        /// 하단 정보 (알림 메시지)
        /// </summary>
        private Label _bottomInfo3;
        /// <summary>
        /// 하단 정보 (Input DispOrder Seq)
        /// </summary>
        private Label _bottomInfo4;

        /// <summary>
        /// 하단 네트워크 상태 <br />
        /// add, 2023.03.07, KDS 메인화면 네트워크 상태확인 추가
        /// </summary>
        private Color _stateNetwork = Color.Black;

        private int _currentPage;
        private int _totalPage;
        private bool _isBusy = false;

        /// <summary>
        /// 알람 메시지 타이머
        /// </summary>
        System.Threading.Timer _alarmTimer;
        /// <summary>
        /// 알람 메시지 시간 (10초)  -> 5초  / 2023-10-20 
        /// </summary>
        private const uint DEFAULT_ALRAM_TIME = 10 * 500;

        /// <summary>
        /// 오래걸리는 작업시 사용
        /// </summary>
        private ActivityIndicator _activityIndicator;
        /// <summary>
        /// ActivityIndicator 사용시 배경 불투명으로 만들기위해
        /// </summary>
        private BoxView _boxView;
        /// <summary>
        /// 주문정보와 하단 정보/버튼으로 구성됨
        /// </summary>
        private StackLayout _stackLayout;
        /// <summary>
        /// ActivityIndicator를 쓰기 위해 _stackLayout와 _activityIndicator를 포함하는 전체 layout
        /// </summary>
        private AbsoluteLayout _mainLayout;
        /// <summary>
        /// 주문조회 비동기 처리를 위한 worker
        /// </summary>
        private BackgroundWorker _orderSearchWorker;
        /// <summary>
        /// 메모데이터 있을때 화면표시용
        /// </summary>
        private const string MEMO = "MEMO";

        private ImageSource img_bkr_66 = ImageManagerExtension.GetImageSource("BK.KDS.Resources.Images.5.png");


        /// <summary>
        /// 2023=11-20
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private string _bottomMessage = string.Empty;


        int threadCountCheck = 0; //임시 . 사용 후 삭제 예정 

        /// <summary>
        /// 2023-11-29 김신 수석님 추가 사항 lock 처리 추가 
        /// </summary>
        private static object lockDispObject = new object();

        private static object lockCmdObject = new object();

        public string BottomMessage { get { return _bottomMessage; }

            set 
            {
                try {

                    //if (_bottomMessage == null) return; 

                    if (_bottomMessage.Equals(value)) return;

                    this._bottomMessage = value;
                    OnPropertyChanged("BottomMessage");


                }
                catch (Exception ex) {

                    ComLog.WriteIOSLog(ComLog.IOSType.PAGE_SHOW
                 , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                 + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                 , "BottomMessage : Exception ;;;;;");
                }
            }
        }


        public KdsMain()
        {
            try
            {
                ComLog.WriteIOSLog(ComLog.IOSType.PAGE_SHOW
                                 , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                 + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                 , "***** KDS MAIN PAGE_SHOW *****");

                BindingContext = this;
                this.BackgroundColor = Color.Black;
                NavigationPage.SetHasNavigationBar(this, false);

                // 화면 layout 구성
                InitLayout();

                // Init Data
                _datas = new Dictionary<int, List<OrderInfo>>();
                _ordInfoList = new List<MenuInfo>();
                _ordMenuList = new List<MenuList>();
                _ordVoidLabelList = new List<Label>();
                _summaryDataList = new List<OrderInfo.MenuItem>();

                _lineCount = StatusManager.Config.LineCount;
                _dispCount = StatusManager.Config.DisplayCount;

                _currentPage = 0;

                // Get Revert List
                GetRevertList();

                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                 , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                 + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                 , "***** KDS INFO *****"
                                 + " LanguageCode : " + StatusManager.Config.LanguageCode
                                 + ", CompanyCode : " + StatusManager.Config.CompanyCode
                                 + ", StoreCode : " + StatusManager.Config.StoreCode
                                 + ", Kds No : " + StatusManager.Config.KdsNo
                                 + ", SystemType : " + StatusManager.Config.SystemType
                                 + ", DispItemKind : " + StatusManager.Config.DispItemKind);

                // Bottom 
                CreateBottom();

                // Main
                CreateMainGrid();

                // Bottom Display
                DisplayBottomInfo();

                // layout을 만들고 Content에 설정해야 화면에 보인다.
                Content = _mainLayout;

                _orderSearchWorker = new BackgroundWorker();
                _orderSearchWorker.DoWork += DoWork;
                _orderSearchWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                _orderSearchWorker.WorkerSupportsCancellation = true;
                _orderSearchWorker.RunWorkerAsync();

                // add, 2023.03.06, KDS메인화명에 네트워크 상태확인 (BKJH 송상현님 요청)
                // 네트워크 변경정보 이벤트 설정 
                Connectivity.ConnectivityChanged += OnConnectivityChanged;
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// OnAppearing
        /// </summary>
        protected override void OnAppearing()
        {
            KeyEventHandler.AddKeyEventOnce(KeyEvent);
        }
        /// <summary>
        /// OnDisappearing
        /// </summary>
        protected override void OnDisappearing()
        {
            KeyEventHandler.RemoveKeyEvent(KeyEvent);
            AlaramMessageVisible(false);
        }

        /// <summary>
        /// 네트워크 변경 이벤트  <br />
        /// add, 2023.03.07, KDS 메인화면 네트워크 상태확인 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            try
            {
                var access = e.NetworkAccess;
                switch (access)
                {
                    case NetworkAccess.None:
                    case NetworkAccess.Unknown:
                        if (_stateNetwork != Color.Red)
                        {
                            _stateNetwork = Color.Red;
                            _bottomInfo1.BackgroundColor = _stateNetwork;
                            _bottomInfo2.BackgroundColor = _stateNetwork;
                        }
                        break;
                    default:
                        if (_stateNetwork != Color.Black)
                        {
                            _stateNetwork = Color.Black;
                            _bottomInfo1.BackgroundColor = _stateNetwork;
                            _bottomInfo2.BackgroundColor = _stateNetwork;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        private async void ClosePage()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsRunning = true);
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsVisible = true);
                MainThread.BeginInvokeOnMainThread(() => _boxView.IsVisible = true);

                _orderSearchWorker.CancelAsync();

                // UwpHelper 종료
                if (StatusManager.Basic.SystemExit != SystemExitMode.AppInstall)
                    ComLib.UwpHelperClose();

                // Timer 종료
                _alarmTimer?.Dispose();
                _alarmTimer = null;

                // worker 작업완료 까지 기다림
                for (var i = 0; i < 10; i++)
                {
                    if (_orderSearchWorker.IsBusy == false) break;

                    //sleep 주석처리  20203-11-15 
                    //Thread.Sleep(300);

                    //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                    taskDelayTime(300);

                }
                _orderSearchWorker?.Dispose();
                _orderSearchWorker = null;

                ComLog.WriteIOSLog(ComLog.IOSType.PAGE_CLOSE
                                 , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                 + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                 , "***** KDS MAIN PAGE_CLOSE *****");
            }
            catch (Exception ex) {

                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                   , "ClasePage > Exception " + ex.Message);
            }

            finally
            {
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsRunning = false);
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsVisible = false);
                MainThread.BeginInvokeOnMainThread(() => _boxView.IsVisible = false);
                PageController.ShowPrevPage();



                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                   , "ClasePage > Finally ");


            }
        }

        /// <summary>
        /// 주문조회 Work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , "Backgroundworker Start");

                var sw = new Stopwatch();
                var interval = StatusManager.Config.DataRecvInterval;

                // add, 2023.02.16, KDS 데이터 수신/자동라벨출력 주기 변경 
                //                  라벨자동출력 이고 FP-2000L 프린터를 사용하면 조회주기를 500ms 강제 조정 
                //                  (FP-2000L 프린터 상태값 조회 시간이 오래걸려서 1초 주기로 조회하면 라벨출력이 2초이상 걸림)
                if (IsAutoPrintCheck())
                {
                    switch (StatusManager.Config.PrinterType)
                    {
                        case PrinterTypes.FP_2000L_LAN:
                        case PrinterTypes.FP_2000L_USB:
                            interval = 500;
                            break;
                    }
                }

                while (true)
                {

                    /*로그 추가 / 2023-09-23*/
                   // ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                   // , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                   // + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                   // , "=> DoWork> START1 ===DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork== START ");
                   // ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                   //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                   //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                   //, "******************************************************************************************************************************" + interval.ToString() + Environment.NewLine + Environment.NewLine);


                    // 취소 체크
                    if (_orderSearchWorker.CancellationPending)
                    {

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "=> DoWork> _orderSearchWorker.CancellationPending   ---------XXXXXXXXXXXXXX ");


                        e.Cancel = true;
                        return;
                    }

                    if (_isBusy)
                    {
                        //sleep 주석처리  20203-11-15 
                        //Thread.Sleep(200);

                        //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                        taskDelayTime(200);

                        continue;
                    }

                    /*로그 추가 / 2023-09-23*/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "=> SearchOrders 처리 시작 (DoWork) ==---------------------------------------------------- start ");

                    // Data Recv
                    // DataManager.SetDataObject<List<OrderInfo>>()를 SearchOrders 내부 외 다른 곳에서 해야할 경우가 생기면 isBusy 처리해야 한다.
                    var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.SearchOrders);

                    /*로그 추가 / 2023-09-23*/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "=> SearchOrders 처리 끝 (DoWork) ==---------------------------------------------------- end ");


                    //로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "프로그램 변경 체크 시작 ");


                    // 프로그램버전 변경
                    if (ret == ComLib.RST_CHGPGM)
                    {
                        if (_isBusy == false)
                        {
                            CommandRun(KDS_FUNC_KEY.PGM_UPDATE, "NOMESSAGE");
                            if (StatusManager.Basic.SystemExit == SystemExitMode.AppInstall) return;
                        }
                    }

                    //로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "마스터버전 변경 체크 시작 ");


                    // 마스터버전 변경
                    if (ret == ComLib.RST_CHGMST)
                    {
                        if (_isBusy == false)
                            CommandRun(KDS_FUNC_KEY.MST_UPDATE);
                    }

                    //로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "ORDER_DATA_UPDATE 시작 ");


                    //왜 데이터 업데이트를 while 로 실행을...........  /2023-11-30
                    //아래 한번만 실행하도록 함 ( 테스트 진행 중....)
                    {
                        sw.Start();
                        while (true)
                        {
                            //로그 추가 / 2023-11-18/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "ORDER_DATA_UPDATE > while start ...... ");

                            if (_isBusy == false)
                            {

                                //로그 추가 / 2023-11-13/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "_isBusy == false   ORDER_DATA_UPDATE  == start ");

                                CommandRun(KDS_FUNC_KEY.ORDER_DATA_UPDATE);

                                //로그 추가 / 2023-11-13/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "_isBusy == false   ORDER_DATA_UPDATE  == end ");

                                break;
                            }

                            if (sw.ElapsedMilliseconds > 500) break;
                        }
                        sw.Stop();
                    }


                    ////로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "CommandRun(KDS_FUNC_KEY.ORDER_DATA_UPDATE); ======------------------------------------------------------------------------------------------" + _isBusy.ToString());


                    //한번만 실행하도록 함 ( 테스트 진행 중....)  /2023-11-30
                    //if (_isBusy == false)
                    //{
                    //    CommandRun(KDS_FUNC_KEY.ORDER_DATA_UPDATE);
                    //}


                    //로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "출력 후 하단 실행 여부 체크 시작 ");

                    //서버 접속 오류 시 하단 코드 실행은 안하도록 함. / 2023-10-11
                    if (ret.StartsWith(ComLib.RST_ERROR))
                    {
                        //sleep 주석처리  20203-11-15 
                        //Thread.Sleep(interval);

                        //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                        taskDelayTime(interval);

                        continue;
                    }

                    //로그 추가 / 2023-11-13/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "출력 후 하단 실행 여부 체크 끝 ");


                    // 라벨자동출력 처리
                    if (_isBusy == false && IsAutoPrintCheck() == true)
                    {

                        //await taskDelayTime(interval);  //화면 로딩 후 대기 시간 추가 / 2023-11-22


                        /*로그 추가 / 2023-09-23*/
                        ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        , "라벨자동출력 처리 시작(DoWork) ==---------------------------------------------------- start _isBusy : " + _isBusy.ToString());

                        CommandRun(KDS_FUNC_KEY.AUTO_PRINT_LABEL);

                        //AutoPrintLabel();


                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "라벨자동출력 처리 끝(DoWork) ==---------------------------------------------------- end _isBusy : " + _isBusy.ToString() );
                    }

                    //조건추가 / 2023-09-23*/
                    if (_isBusy == false)
                    {

                        //로그 추가 / 2023-11-13/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "24시간 점포 대응 시작 "  );


                        // 24시간 점포 대응
                        // Log정리 
                        if ((DateTime.Now - StatusManager.Basic.CleanDate).TotalHours > 24 &&
                        DateTime.Now.Hour == 1)
                        {

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "DoWork . LOG_BACKUP  ==----------------------------------------------------  ......  " + Environment.NewLine + Environment.NewLine);

                            ProcessManager.GetInstance<ProgramStart>().Execute(ProgramStart.Tasks.LOG_BACKUP);
                        }

                        /*  키 패드 사용안함 처리 - 2023-0-10-17 
                        // Numlock check
                        if (Device.RuntimePlatform == Device.UWP)
                        {
                            ComLib.TurnOnNumLock();
                        }
                        */

                        // bw 주기   //sleep 주석처리  20203-11-15 
                        //Thread.Sleep(interval); 

                        //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                        taskDelayTime(interval);

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "=> DoWork> END ===DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork==DoWork== END ~......  after interval : " + interval.ToString() + Environment.NewLine + Environment.NewLine);

                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@" + interval.ToString() + Environment.NewLine + Environment.NewLine);


                    }
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }
 

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                     , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                     + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                     , "Backgroundworker Completed");

                if (e.Error != null)
                {
                    ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                  , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                  + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                  , "Backgroundworker is Error"
                                  , e.Error);

                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// 키 입력 이벤트
        /// </summary>
        /// <param name="args"></param>
        private void KeyEvent(KeyEventArgs args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args.FuncKey)) return;

                ComLog.WriteIOSLog(ComLog.IOSType.KBD
                                 , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                 + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                 , "FuncKey=[{0, -4} FuncDesc=[{1}]", args.FuncKey, args.FuncDesc);

                var numKey = KdsKey.GetNumericKeyValue(args.FuncKey);
                if (string.IsNullOrWhiteSpace(numKey))
                {
                    CommandRun(args.FuncKey);
                }
                else
                {
                    if (ComLib.IsNumeric(numKey) == false) return;

                    DisplayInputKdsNo(numKey);
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 기능키 클릭 이벤트
        /// </summary>
        /// <param name="command"></param>
        private void ButtonClickHandler(object command)
        {
            try
            {
                ComLog.WriteIOSLog(ComLog.IOSType.MOUSE
                                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                , "command=[{0}]", command);

                CommandRun(command.ToString());
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 주문 클릭 이벤트
        /// </summary>
        /// <param name="param"></param>
        private void OrderClickHandler(string ordNo)
        {
            try
            {
                // Expo에서만 터치 가능
                if (StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.EXPO &&
                    StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.SUB_EXPO &&
                    StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.LZ) return;

                ComLog.WriteIOSLog(ComLog.IOSType.MOUSE
                                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                , "command=[{0}] ordNo=[{1}]", KDS_FUNC_KEY.ORDER_CLICK, ordNo);

                CommandRun(KDS_FUNC_KEY.ORDER_CLICK, ordNo);
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// CommandRun
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        public void CommandRun(string command, string param = "")
        {
            if (_isBusy) return;

            try
            {

                if (MainThread.IsMainThread)
                {
                    _ = new TaskFactory().StartNew(() => CommandRun(command, param));
                    return;
                }

                lock (lockCmdObject)
                {

                    _isBusy = true;

                    // num: bumpKey로 누른 번호
                    var num = ComLib.IntParse(_bottomInfo4.Text);
                    // CommandRun이 진행되는 동안 숫자키를 누르면 _bottomInfo4.Text에 입력되므로 finally에서 옮김
                    //if (command != KDS_FUNC_KEY.ORDER_DATA_UPDATE)
                    //{
                    //    DisplayInputKdsNo(null);
                    //}
                    switch (command)
                    {
                        case KDS_FUNC_KEY.ORDER_DATA_UPDATE:
                        case KDS_FUNC_KEY.AUTO_PRINT_LABEL:
                            break;
                        default:
                            // 사용자 입력 숫자키 초기화
                            DisplayInputKdsNo(null);
                            break;
                    }

                    // 진입 후 다른 command로 가야하는 경우 선처리
                    switch (command)
                    {
                        case KDS_FUNC_KEY.MENU:
                            {
                                var vm = new SideMenuPopupViewModel();
                                PageController.ShowPage(nameof(SideMenuPopupPage), vm);
                                if (vm.RetValue == ComLib.RST_OK)
                                {
                                    _isBusy = false;
                                    return;
                                }

                                ComLog.WriteIOSLog(ComLog.IOSType.MOUSE
                                                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                                , "command=[{0}]", vm.RetValue);
                                command = vm.RetValue;
                            }
                            break;

                        case KDS_FUNC_KEY.COMPLETE:
                            if (_datas.Keys.Count == 0)
                            {
                                _isBusy = false;
                                return;
                            }

                            // 빈 값이면 첫번째 상품으로
                            if (num == 0) num = 1;
                            if (num > _dispCount)
                            {
                                _isBusy = false;
                                return;
                            }

                            var ord = _datas[_currentPage].Find(x => x.DispSeq == num);
                            if (ord == null)
                            {
                                _isBusy = false;
                                return;
                            }

                            ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                               , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                               + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                               , "Input DispOrder Seq=[{0:00}], OrderNo=[{1:0000}]"
                                               , _bottomInfo4.Text
                                               , ord.OrderNo);

                            #region mod, 2022.11.08, 일본 키보드 대응, EXPO KDS 완료 BumpKey 처리 (안호성B 요청)
                            //if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                            //    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                            //    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            //{
                            //    command = KDS_FUNC_KEY.ORDER_CLICK;
                            //    param = ord.OrderNo;
                            //}
                            //else if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.BOARD ||
                            //         StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.FRY ||
                            //         StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.DnD)
                            //{
                            //    OrderComplete(ord);
                            //    return;
                            //}
                            //break; 
                            #endregion

                            OrderComplete(ord);
                            _isBusy = false;
                            return; ;
                    }

                    if (command == KDS_FUNC_KEY.LEFT)
                    {
                        if (_currentPage - 1 < 0)
                        {
                            _isBusy = false;
                            return;
                        }

                        _currentPage--;
                        //ProcessingData();     //2023-11-29   lock  구문에 포함하여 처리 함. ( DisplayData 메소드에 포함 ) 
                        DisplayData(_currentPage);
                    }
                    else if (command == KDS_FUNC_KEY.RIGHT)
                    {
                        if (_currentPage + 1 >= _datas.Keys.Count)
                        {
                            _isBusy = false;
                            return;
                        }

                        _currentPage++;
                        //ProcessingData();   //2023-11-29   lock  구문에 포함하여 처리 함. ( DisplayData 메소드에 포함 ) 
                        DisplayData(_currentPage);
                    }
                    else if (command == KDS_FUNC_KEY.APP_EXIT)
                    {
                        StatusManager.Basic.SystemExit = SystemExitMode.Exit;
                        ClosePage();
                    }
                    else if (command == KDS_FUNC_KEY.REVERT)
                    {
                        if (_revertList.Count == 0)
                        {
                            _isBusy = false;
                            return;
                        }

                        // 마지막 완료처리 상품 Revert 처리
                        var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.RevertOrder, _revertList[_revertList.Count - 1]);
                        if (ret.StartsWith(ComLib.RST_ERROR))
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                            _isBusy = false;
                            return;
                        }
                        else if (ret.StartsWith(ComLib.RST_IGNORE))
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                        }

                        // 목록에서 제거
                        _revertList.RemoveAt(_revertList.Count - 1);

                        // Revert List 파일에 저장
                        DataManager.SetDataObject<List<RevertInfo>>(_revertList);
                        ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.SaveRevertList);
                        if (ret != ComLib.RST_OK)
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                            _isBusy = false;
                            return;
                        }
                    }
                    else if (command == KDS_FUNC_KEY.REPRINT)
                    {
                        if (StatusManager.Config.PrinterUseYN == false)
                        {
                            ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0041));
                            _isBusy = false;
                            return;
                        }
                        if (StatusManager.Config.PrinterType == PrinterTypes.None)
                        {
                            ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0064));
                            _isBusy = false;
                            return;
                        }

                        if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.BOARD ||
                            StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.FRY ||
                            StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.DnD)
                        {
                            if (StatusManager.Config.PrintItemType == FLAG_PRT_ITEM_TYPE.NOPRINT)
                            {
                                ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0065));
                                _isBusy = false;
                                return;
                            }
                            if (StatusManager.Config.PrintItemType == FLAG_PRT_ITEM_TYPE.AUTO)
                            {
                                ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0066));
                                _isBusy = false;
                                return;
                            }
                        }

                        var ret = ProcessManager.GetInstance<PrintProcess>().RePrint();
                        if (ret == ComLib.RST_IGNORE)
                        {
                            _isBusy = false;
                            return;
                        }
                        if (ret != ComLib.RST_OK)
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                            _isBusy = false;
                            return;
                        }
                    }
                    else if (command == KDS_FUNC_KEY.SUM_DAYS)
                    {
                        var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.GetTotalOrders);
                        if (ret.StartsWith(ComLib.RST_IGNORE))
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                            _isBusy = false;
                            return;
                        }
                        else if (ret != ComLib.RST_OK)
                        {
                            ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0026), MessageBoxTypes.Error);
                            _isBusy = false;
                            return;
                        }

                        var vm = new KdsOrderStatsViewModel(DataManager.GetDataObject<List<OrderStats>>());
                        PageController.ShowPage(nameof(KdsOrderStats), vm);
                    }
                    else if (command == KDS_FUNC_KEY.SETTING)
                    {
                        var vm = new ViewModels.Sys.KdsConfigViewModel();
                        PageController.ShowPage(nameof(KdsConfigPage), vm);
                        if (vm.RetValue == ComLib.RST_IGNORE)
                        {
                            _isBusy = false;
                            return;
                        }

                        UpdateMaster();
                    }
                    else if (command == KDS_FUNC_KEY.ORDER_CLICK)
                    {
                        if (_datas.Keys.Count == 0)
                        {
                            _isBusy = false;
                            return;
                        }

                        var ord = _datas[_currentPage].Find(x => x.OrderNo == param);
                        if (ord == null)
                        {
                            _isBusy = false;
                            return;
                        }

                        // 20221004, mod, 메모값 있는데 Y가 아니면 메모내용으로 본다.
                        var vm = new PopupMenuViewModel(ord);
                        if (string.IsNullOrWhiteSpace(ord.Memo) == false && ord.Memo.ToUpper() != MEMO_FLAG.MEMO_Y)
                            PageController.ShowPage(nameof(PopupMenuWithMemoPage), vm);
                        else
                            PageController.ShowPage(nameof(PopupMenuPage), vm);

                        if (vm.RetValue == KDS_FUNC_KEY.COMPLETE)
                        {
                            /*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "KDS_FUNC_KEY.COMPLETE========= start");

                            OrderComplete(ord);

                            ///*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "KDS_FUNC_KEY.COMPLETE========= end");
                        }
                        else if (vm.RetValue == KDS_FUNC_KEY.COMPLETE_PICKUP)
                        {
                            /*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "OrderPickupComplete========= start");

                            OrderPickupComplete(ord);

                            /*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "OrderPickupComplete========= end");

                        }
                        else if (vm.RetValue == KDS_FUNC_KEY.PRINT)
                        {

                            /*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "OrderPrint========= start");

                            OrderPrint(ord);

                            /*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "OrderPrint========= end");

                        }
                        else if (vm.RetValue.StartsWith(ComLib.RST_ERROR))
                        {
                            ///*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "ShowMessage(vm.RetValue); ========= start");

                            ShowMessage(vm.RetValue);

                            ///*로그 추가 / 2023-11-28*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "ShowMessage(vm.RetValue); ========= start");

                        }
                    }
                    else if (command == KDS_FUNC_KEY.PRINT)
                    {
                        switch (StatusManager.Config.SystemType)
                        {
                            case FLAG_SYSTEM_TYPE.EXPO:
                            case FLAG_SYSTEM_TYPE.SUB_EXPO:
                            case FLAG_SYSTEM_TYPE.LZ:
                                if (_datas.Keys.Count == 0)
                                {
                                    _isBusy = false;
                                    return;
                                }
                                if (num <= 0 || num > _dispCount)
                                {
                                    _isBusy = false;
                                    return;
                                }

                                var ord = _datas[_currentPage].Find(x => x.DispSeq == num);
                                if (ord == null)
                                {
                                    _isBusy = false;
                                    return;
                                }

                                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                                   , "Input DispOrder Seq=[{0:00}], OrderNo=[{1:0000}]"
                                                   , _bottomInfo4.Text
                                                   , ord.OrderNo);
                                OrderPrint(ord);
                                break;
                            default:
                                OrderPrint();
                                break;
                        }
                    }
                    else if (command == KDS_FUNC_KEY.MST_UPDATE)
                    {
                        UpdateMaster();
                    }
                    else if (command == KDS_FUNC_KEY.PGM_UPDATE)
                    {
                        var updPgm = ProcessManager.GetInstance<DownProgram>();
                        var ret = ViewManager.ExecuteWaitProcess(updPgm, DownProgram.Tasks.UPDATE, param);
                        if (ret == ComLib.RST_OK)
                        {

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "ExecuteWaitProcess========= start");


                            StatusManager.Basic.SystemExit = SystemExitMode.AppInstall;
                            ClosePage();


                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "ExecuteWaitProcessL========= END");


                        }
                        else if (ret == ComLib.RST_IGNORE)
                        {
                            ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0044), MessageBoxTypes.Confirm);
                        }
                        else if (ret == ComLib.RST_ERROR)
                        {
                            ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0045), MessageBoxTypes.Error);
                        }
                    }
                    else if (command == KDS_FUNC_KEY.ORDER_DATA_UPDATE)
                    {
                        #region (삭제) 2022.12.27, 신규/변경주문 알람
                        //_dataList = DataManager.GetDataObject<List<OrderInfo>>(); 
                        //// Data Processing
                        //ProcessingData();
                        //// Data Display
                        //DisplayData(_currentPage); 
                        #endregion

                        //로그 추가 / 2023-11-14/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "_isBusy == false   DisplayOrderDataUpdate  == start  ");

                        DisplayOrderDataUpdate();

                        //로그 추가 / 2023-11-14/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "_isBusy == false   DisplayOrderDataUpdate  == end  ");


                    }
                    else if (command == KDS_FUNC_KEY.RECALL)
                    {
                        var vm = new DidRecallListViewModel();
                        PageController.ShowPage(nameof(DidRecallListPage), vm);
                        if (vm.RetValue == ComLib.RST_IGNORE)
                        {
                            _isBusy = false;
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(vm.OrderNo) == true)
                        {
                            _isBusy = false;
                            return;
                        }

                        // did 재호출
                        var ret = ProcessManager.GetInstance<DidProcess>().Execute(DidProcess.Tasks.ReCallDID, vm.OrderNo);
                        if (ret != ComLib.RST_OK)
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                        }
                    }
                    else if (command == KDS_FUNC_KEY.AUTO_PRINT_LABEL)
                    {
                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "KDS_FUNC_KEY.AUTO_PRINT_LABEL========= start");

                        // 라벨자동출력 처리
                        //AutoPrintLabel();

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "KDS_FUNC_KEY.AUTO_PRINT_LABEL========= end");
                    }

                    //finaly 처리 목록 이동 함. /2023-11-29
                    #region finaly 처리 목록 이동 함. /2023-11-29
                    {

                        // 주문조회 및 프로그램 업데이트로 CommandRun을 들어온 경우가 아니면
                        // 화면표시를 빨리해주기 위해서 주문조회 및 화면표시를 해준다.
                        if (command != KDS_FUNC_KEY.ORDER_DATA_UPDATE &&
                            command != KDS_FUNC_KEY.AUTO_PRINT_LABEL &&
                            command != KDS_FUNC_KEY.PGM_UPDATE &&
                            MainThread.IsMainThread == false)
                        {

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "=> CommandRun > Finally > 시작전.. ==---------------------------------------------------- start ");

                            //IsBusy = true; 
                            ///2023-11-28 IsBusy = true 일 경우만 하단 코드 실행으로 변경 

                            //조건 적용 테스트 진행 중 2023-11-28  백과장님과 함께 테스트 진행 중. 
                            if (_isBusy)
                            {

                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "=> CommandRun > Finally > SearchOrders  try 진입 isbusy = true ==---------------------------------------------------- start " + IsBusy.ToString());

                                try
                                {
                                    /*로그 추가 / 2023-09-23*/
                                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                    //, "=> CommandRun > Finally > SearchOrders 처리 시작 (DoWork) isbusy = true ==---------------------------------------------------- start " + IsBusy.ToString());

                                    var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.SearchOrders);

                                    /*로그 추가 / 2023-09-23*/
                                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                    //, "=> CommandRun > Finally > SearchOrders 처리 시작 (DoWork) isbusy = true ==---------------------------------------------------- start " + IsBusy.ToString());

                                    if (ret != ComLib.RST_CHGMST && ret != ComLib.RST_CHGPGM)
                                    {
                                        #region (삭제) 2022.12.27, 신규/변경주문 알람
                                        //_dataList = DataManager.GetDataObject<List<OrderInfo>>(); 
                                        //// Data Processing
                                        //ProcessingData();
                                        //// Data Display
                                        //DisplayData(_currentPage); 
                                        #endregion

                                        /*로그 추가 / 2023-09-23*/
                                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                        //, "=> CommandRun > Finally > DisplayOrderDataUpdate 처리 시작 (DoWork) ==---------------------------------------------------- start ");

                                        DisplayOrderDataUpdate();

                                        /*로그 추가 / 2023-09-23*/
                                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                        //, "=> CommandRun > Finally > DisplayOrderDataUpdate 처리 끝 (DoWork) ==---------------------------------------------------- end ");
                                    }

                                }
                                catch (Exception ex)
                                {

                                    /*로그 추가 / 2023-09-23*/
                                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                    , "=> CommandRun > Finally > XXXXXXXXXXXXXXXXXXXXXXXX SearchOrders Exception  XXXXXXXXXXXXXXXXXXXXXXXX ==---------------------------------------------------- end " + ex.ToString());

                                }


                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "=> CommandRun > Finally > SearchOrders 처리 끝 (DoWork)  isbusy = true  ==---------------------------------------------------- end ");
                            }
                        }

                        _isBusy = false;
                    }
                    #endregion finaly 처리 목록 이동 함. /2023-11-29
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            } 
        }

        /// <summary>
        /// 업데이트된 주문데이터 화면표시 및 신규/변경주문 알람
        /// </summary>
        private void DisplayOrderDataUpdate()
        {
            

            try
            {
                var isBeep = false;
                //  var oldData = ComLib.DeepClone(_dataList);
                //  _dataList = DataManager.GetDataObject<List<OrderInfo>>();
                /*
                  // 신규&변경주문확인
                  if (oldData != null)
                  {
                      for (var i = _dataList.Count - 1; i >= 0; i--)
                      {
                          var data = _dataList[i];
                          var check = oldData.Where(x => x.SaleDate == data.SaleDate &&
                                                         x.PosNo == data.PosNo &&
                                                         x.TranNo == data.TranNo).FirstOrDefault();
                          if (check == null) isBeep = true;
                          if (check?.ModCnt < data.ModCnt) isBeep = true;

                          if (isBeep) break;
                      }
                  }
                */

                //로그 추가 / 2023-11-14/
                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "_isBusy == false   ProcessingData  == start   ");

                // Data Processing
                //ProcessingData();  //2023-11-29   lock  구문에 포함하여 처리 함. ( DisplayData 메소드에 포함 ) 

                //로그 추가 / 2023-11-14/
                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                , "DisplayOrderDataUpdate >    DisplayData(_currentPage, true)    :  page :   " + _currentPage.ToString());

                // Data Display
                isBeep = DisplayData(_currentPage, true);

                //로그 추가 / 2023-11-14/
                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "_isBusy == false   DisplayData  == end   ");



                /*
                // add, 2023.03.06, BKJH EXPO 신규주문 알람제외 (안호성B 요청)
                //                  EXPO에서 기능키 제어시(revert) 알람음 재생으로 야간의 지연현상 때문에 EXPO에서 신규주문 알람제외 
                if (StatusManager.Config.CompanyCode == COMPANY.BKJH)
                {
                    if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                        StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO)
                    {
                        isBeep = false;
                    }
                }

                if (isBeep)
                {
                    // mod, 2023.03.01, BKJH 신규주문 알람음 소리가 작아서 다른 음원으로 변경
                    //ComLib.Beep(BeepKinds.Beep2);

                    // mod, 2023.04.04, BKR 종로구청점 알람음 소리가 작다고 해서 BKJH와 음원 통일함
                    //switch (StatusManager.Config.CompanyCode)
                    //{
                    //    case COMPANY.BKJH:  ComLib.Beep(BeepKinds.Beep3); break;
                    //    default:            ComLib.Beep(BeepKinds.Beep2); break;
                    //}

                    ComLib.Beep(BeepKinds.Beep3);
                }

                */

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 화면 layout Init
        /// </summary>
        private void InitLayout()
        {
            try
            {
                _mainLayout = new AbsoluteLayout();
                _mainLayout.VerticalOptions = LayoutOptions.FillAndExpand;
                _mainLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
                _mainLayout.Margin = new Thickness(0);
                _mainLayout.Padding = new Thickness(0);

                _stackLayout = new StackLayout();
                _stackLayout.VerticalOptions = LayoutOptions.FillAndExpand;
                _stackLayout.HorizontalOptions = LayoutOptions.FillAndExpand;
                _stackLayout.Margin = new Thickness(0);
                _stackLayout.Padding = new Thickness(0);
                _stackLayout.Spacing = 0;

                _activityIndicator = new ActivityIndicator();
                _activityIndicator.HorizontalOptions = LayoutOptions.CenterAndExpand;
                _activityIndicator.VerticalOptions = LayoutOptions.CenterAndExpand;
                _activityIndicator.Color = Color.OrangeRed;
                _activityIndicator.IsVisible = false;
                _activityIndicator.Scale = 2;

                _boxView = new BoxView();
                _boxView.HorizontalOptions = LayoutOptions.FillAndExpand;
                _boxView.VerticalOptions = LayoutOptions.FillAndExpand;
                _boxView.BackgroundColor = Color.FromRgba(0, 0, 0, 128);
                _boxView.IsVisible = false;

                _mainLayout.Children.Add(_stackLayout, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                _mainLayout.Children.Add(_boxView, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                _mainLayout.Children.Add(_activityIndicator, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 메인화면 생성
        /// </summary>
        private async void CreateMainGrid()
        {
            try
            {
                if (!MainThread.IsMainThread)
                {
                    MainThread.BeginInvokeOnMainThread(() => CreateMainGrid());
                    return;
                }

                // Main Grid
                _grid = new Grid();
                _grid.VerticalOptions = LayoutOptions.FillAndExpand;
                _grid.HorizontalOptions = LayoutOptions.FillAndExpand;
                _grid.Margin = new Thickness(10, 10, 10, 10);

                _lineCount = StatusManager.Config.LineCount;
                _dispCount = StatusManager.Config.DisplayCount;

                // 화면 분할 수에 따라 그리드 분할
                // 2022.06.09 현업워크샵 내용에 따라 4분할은 없으므로 row 개수는 2로 고정 (분할수 : 10, 12, 14)
                _rowCount = 2;
                _colCount = _dispCount / _rowCount;
                for (var i = 0; i < _rowCount; i++)
                {
                    _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                }
                for (var i = 0; i < _colCount; i++)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                }

                //그리드에 이미지 추가
                for (var r = 0; r < _rowCount; r++)
                {
                    for (var c = 0; c < _colCount; c++)
                    {
                        _grid.Children.Add(new Image
                        {
                            IsVisible = true,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Aspect = Aspect.AspectFit,
                            Source = ImageManagerExtension.GetImageSource("BK.KDS.Resources.Images.EmptyOrder.png")
                        }, c, r);


                        //sleep 주석처리  20203-11-15 
                        //Thread.Sleep(50);

                        //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                        taskDelayTime(50);

                    }
                }

                // * 데이터 추가/제거 시 깜박임 때문에
                //   미리 컨트롤을 올려두고 데이터만 넣으면 되도록 한다.
                _ordInfoList.Clear();
                _ordMenuList.Clear();
                _ordVoidLabelList.Clear();
                // 데이터만 바꾸면 되도록 메뉴정보 컨트롤만 먼저 생성
                for (var r = 0; r < _rowCount; r++)
                {
                    for (var c = 0; c < _colCount; c++)
                    {
                        var menu = new MenuInfo();
                        menu.IsVisible = false;
                        menu.Data = new OrderInfo();
                        menu.ClickEvent = OrderClickHandler;

                        _ordInfoList.Add(menu);
                        _grid.Children.Add(menu, c, r);
                    }
                }
                // 데이터만 바꾸면 되도록 메뉴리스트 컨트롤만 먼저 생성
                for (var r = 0; r < _rowCount; r++)
                {
                    for (var c = 0; c < _colCount; c++)
                    {
                        var gridView = new MenuList(_lineCount);
                        gridView.CreateGrid(_lineCount);
                        gridView.ClickEvent = OrderClickHandler;
                        gridView.IsVisible = false;
                        _ordMenuList.Add(gridView);
                        _grid.Children.Add(gridView, c, r);

                        var voidLabel = new Label();
                        voidLabel.Text = MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0030);
                        voidLabel.TextColor = Color.Red;
                        voidLabel.FontSize = 30;
                        voidLabel.FontAttributes = FontAttributes.Bold;
                        voidLabel.IsVisible = false;
                        voidLabel.HorizontalTextAlignment = TextAlignment.Center;
                        voidLabel.VerticalTextAlignment = TextAlignment.Center;
                        voidLabel.VerticalOptions = LayoutOptions.End;
                        voidLabel.BackgroundColor = Color.FromRgba(0, 0, 0, 33);
                        voidLabel.Margin = new Thickness(5, 0, 5, 10);
                        _ordVoidLabelList.Add(voidLabel);
                        _grid.Children.Add(voidLabel, c, r);
                    }
                }

                // Summary 컨트롤 생성
                _summary = new Summary();
                _summary.IsVisible = StatusManager.Config.IsShowSummary;
                _grid.Children.Add(_summary, _colCount - 1, _rowCount - 1);

                _stackLayout.Children.Clear();
                _stackLayout.Children.Add(_grid);
                _stackLayout.Children.Add(_bottomGrid);
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 하단 정보 및 버튼 생성
        /// </summary>
        private void CreateBottom()
        {
            try
            {
                _bottomGrid = new Grid
                {
                    HeightRequest = 50,
                    MinimumHeightRequest = 50,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    ColumnSpacing = 1,
                    RowSpacing = 1,
                    Margin = new Thickness(10, 0, 10, 1),
                    BackgroundColor = Color.Orange
                };
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420, GridUnitType.Absolute) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Absolute) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Star) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Star) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Star) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Star) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90, GridUnitType.Star) });
                _bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50, GridUnitType.Star) });

                // Bottom Label 생성
                _bottomInfo1 = new Label
                {
                    Padding = new Thickness(10, 0, 0, 0),
                    Margin = new Thickness(1, 1, 0, 1),

                    //BackgroundColor = Color.Black,
                    BackgroundColor = _stateNetwork,            // add, 2023.03.07, KDS 메인화면 네트워크 상태확인 추가

                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = "",
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                };
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) =>
                {
                    CommandRun(KDS_FUNC_KEY.MENU);
                };
                _bottomInfo1.GestureRecognizers.Add(tapGestureRecognizer);

                _bottomInfo2 = new Label
                {
                    Margin = new Thickness(-1, 1, 0, 1),
                    //BackgroundColor = Color.Black,
                    BackgroundColor = _stateNetwork,            // add, 2023.03.07, KDS 메인화면 네트워크 상태확인 추가
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = "",
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                };
                _bottomInfo2.GestureRecognizers.Add(tapGestureRecognizer);

                _bottomInfo3 = new Label
                {
                    Margin = new Thickness(1, 1, 1, 1),
                    //BackgroundColor = Color.Black,
                    BackgroundColor = _stateNetwork,            // add, 2023.03.07, KDS 메인화면 네트워크 상태확인 추가
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = "",
                    TextColor = Color.White,
                    FontAttributes = FontAttributes.Bold,
                    IsVisible = false,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                };

                var binding = new Binding("BottomMessage",BindingMode.TwoWay);
                binding.Source = this;
                
                _bottomInfo3.SetBinding(Label.TextProperty, binding);


                _bottomInfo3.GestureRecognizers.Add(tapGestureRecognizer);
                _bottomInfo4 = new Label
                {
                    Margin = new Thickness(1, 1, 2, 1),
                    BackgroundColor = Color.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    Text = "",
                    TextColor = Color.Yellow,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                };

                ImageButton btn0 = new ImageButton
                {
                    Margin = new Thickness(0, 1, 1, 1),
                    Padding = new Thickness(0, 10, 0, 10),
                    BackgroundColor = Color.SlateGray,
                    Source = ImageManagerExtension.GetImageSource("BK.KDS.Resources.Images.arrow_left.png")
                };
                btn0.Command = new Command(ButtonClickHandler);
                btn0.CommandParameter = KDS_FUNC_KEY.LEFT;

                ImageButton btn1 = new ImageButton
                {
                    Margin = new Thickness(0, 1, 1, 1),
                    Padding = new Thickness(0, 10, 0, 10),
                    BackgroundColor = Color.SlateGray,
                    Source = ImageManagerExtension.GetImageSource("BK.KDS.Resources.Images.arrow_right.png")
                };
                btn1.Command = new Command(ButtonClickHandler);
                btn1.CommandParameter = KDS_FUNC_KEY.RIGHT;

                Button btn2 = new Button
                {
                    Margin = new Thickness(0, 1, 0, 1),
                    BackgroundColor = Color.Gray,
                    Text = "REVERT",
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                };
                btn2.Command = new Command(ButtonClickHandler);
                btn2.CommandParameter = KDS_FUNC_KEY.REVERT;


                #region mod, 2022.11.08, EXPO 하단 재출력 메뉴를 DID재호출로 변경 (안호성B 요청)
                //Button btn3 = new Button
                //{
                //    Margin = new Thickness(0, 1, 0, 1),
                //    BackgroundColor = Color.Gray,
                //    Text = "REPRINT",
                //    TextColor = Color.White,
                //    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                //};
                //btn3.Command = new Command(ButtonClickHandler);
                //btn3.CommandParameter = KDS_FUNC_KEY.REPRINT; 
                #endregion
                Button btn3 = new Button
                {
                    Margin = new Thickness(0, 1, 0, 1),
                    BackgroundColor = Color.Gray,
                    Text = "DID RECALL",
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                };
                btn3.Command = new Command(ButtonClickHandler);
                btn3.CommandParameter = KDS_FUNC_KEY.RECALL;

                Button btn4 = new Button
                {
                    Margin = new Thickness(1, 1, 0, 1),
                    BackgroundColor = Color.Gray,
                    Text = "MENU",
                    TextColor = Color.White,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                };
                btn4.Command = new Command(ButtonClickHandler);
                btn4.CommandParameter = KDS_FUNC_KEY.MENU;

                _bottomGrid.Children.Add(_bottomInfo1, 0, 0);
                _bottomGrid.Children.Add(_bottomInfo2, 1, 0);
                _bottomGrid.Children.Add(_bottomInfo4, 7, 0);

                if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO)
                {
                    _bottomGrid.Children.Add(_bottomInfo3, 0, 0);
                    Grid.SetColumnSpan(_bottomInfo3, 2);

                    _bottomGrid.Children.Add(btn0, 2, 0);
                    _bottomGrid.Children.Add(btn1, 3, 0);
                    _bottomGrid.Children.Add(btn2, 4, 0);
                    _bottomGrid.Children.Add(btn3, 5, 0);
                    _bottomGrid.Children.Add(btn4, 6, 0);
                }
                else
                {
                    _bottomGrid.Children.Add(_bottomInfo3, 2, 0);
                    Grid.SetColumnSpan(_bottomInfo3, 5);
                    _bottomInfo3.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 하단 정보 표시
        /// </summary>
        private void DisplayBottomInfo()
        {
            try
            {
                if (!MainThread.IsMainThread)
                {
                    MainThread.BeginInvokeOnMainThread(() => DisplayBottomInfo());
                    return;
                }

                _bottomInfo1.Text = string.Format("{0}({1}) {2} / {3}{4}", StatusManager.Config.KdsName
                                                                         , StatusManager.Config.KdsNo
                                                                         , DateTime.Now.ToString("yyyy-MM-dd")
                                                                         , MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0004)
                                                                         , StatusManager.Basic.AppVersion);
                if (_totalPage == 0)
                {
                    _bottomInfo2.Text = MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0005) + " " + (_currentPage + 1) + " / 1";
                }
                else
                {
                    _bottomInfo2.Text = MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0005) + " " + (_currentPage + 1) + " / " + _totalPage;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 마스터 및 UI 업데이트
        /// </summary>
        /// <remarks>
        /// 업데이트 시기 <br />
        /// 1. 설정화면에서 회사코드, 점포코드, KDS No 변경 시  <br />
        /// 2. 주문조회 중 CommHeader MstVersion이 갖고 있는 것과 다를 시
        /// </remarks>
        private void UpdateMaster()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsRunning = true);
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsVisible = true);
                MainThread.BeginInvokeOnMainThread(() => _boxView.IsVisible = true);

                // 마스터 변경
                var ret = ProcessManager.GetInstance<DownMaster>().Execute(DownMaster.Tasks.MST_DOWN);
                if (ret.StartsWith(ComLib.RST_ERROR))
                {
                    ShowMessage(ret);
                    return;
                }

                // Get Revert List
                GetRevertList();

                KdsKey.SetKeyTable(StatusManager.Config.BumpKeyKind);

                CreateBottom();
                CreateMainGrid();
                DisplayBottomInfo();
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsRunning = false);
                MainThread.BeginInvokeOnMainThread(() => _activityIndicator.IsVisible = false);
                MainThread.BeginInvokeOnMainThread(() => _boxView.IsVisible = false);
            }
        }

        /// <summary>
        /// 하단에 알람 메시지표시
        /// </summary>
        /// <param name="msg">메시지</param>
        /// <param name="displayTime">기본 10초</param>
        private void DisplayAlarmMessage(string msg, uint displayTime = DEFAULT_ALRAM_TIME)
        {
            try
            {

                if (MainThread.IsMainThread == false)
                {
                    MainThread.BeginInvokeOnMainThread(() => DisplayAlarmMessage(msg, displayTime));
                    return;
                }

                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "ShowMessage > DisplayAlarmMessage START -----------> ");


                // 알람메시지 표시를 위해 알람타이머 작동
                if (_alarmTimer == null)
                {

                    //2023-10-18 add 
                    ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        , "ShowMessage > DisplayAlarmMessage  > _alarmTimer ----------->  NULL");

                    _alarmTimer = new System.Threading.Timer((state) => AlaramMessageVisible(false), null, displayTime, System.Threading.Timeout.Infinite);
                }
                else
                {
                    //2023-10-18 add 
                    ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        , "ShowMessage > DisplayAlarmMessage  > _alarmTimer ----------->  NOT NUILL ");

                    _alarmTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    _alarmTimer.Change(displayTime, System.Threading.Timeout.Infinite);
                }

                AlaramMessageVisible(true);

                msg = msg.Replace(ComLib.RST_OK, "")
                         .Replace(ComLib.RST_ERROR, "")
                         .Replace(ComLib.RST_RETRY, "")
                         .Replace(ComLib.RST_IGNORE, "");

                msg = msg.Replace(@"\n", "■")
                         .Replace(@"\r", "■")
                         .Replace(@"\\r", "■")
                         .Replace(@"\\n", "■")
                         .Replace(@"\r\n", "■")
                         .Replace(@"\\r\\n", "■");

                _bottomInfo3.Text = msg;

                //메모리 사용 확인을 위한 임시주석 (메모리 누수 확인이 필요한 기능..)/ 2023-10-26 
                //_bottomInfo3.ColorTo(Color.White, Color.Yellow, c => _bottomInfo3.TextColor = c, displayTime);



                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "ShowMessage > DisplayAlarmMessage EMD -----------> ");

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }

            //2023-10-23 타이머 종료 추가 
            finally {

                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "ShowMessage > DisplayAlarmMessage _alarmTimer Dispose ============================================================================================ ");


                // Timer 종료
                _alarmTimer?.Dispose();
                _alarmTimer = null;
            }
        }

        /// <summary>
        /// 알람메시지 Visible 처리 및 알람 타이머 종료처리
        /// </summary>
        /// <param name="visible"></param>
        private void AlaramMessageVisible(bool visible)
        {
            try
            {

                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "ShowMessage > DisplayAlarmMessage> AlaramMessageVisible start ---------- Alarms --> " + visible.ToString() + " MainThread.IsMainThread  : " + MainThread.IsMainThread.ToString());


                if (MainThread.IsMainThread == false)
                {

                    //2023-10-18 add 
                    ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        , "ShowMessage > DisplayAlarmMessage> AlaramMessageVisible MainThread.IsMainThread  --------- Alarms-> " + MainThread.IsMainThread.ToString());

                    
                    MainThread.BeginInvokeOnMainThread(() => AlaramMessageVisible(visible));
                    return;
                }

                if (StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.EXPO &&
                    StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.SUB_EXPO)
                {
                    _bottomInfo3.IsVisible = true;
                    _bottomInfo3.Text = string.Empty;
                }
                else
                {
                    _bottomInfo3.IsVisible = visible;
                }

                if (visible == false && _alarmTimer != null)
                {
                    _alarmTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                }


                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "ShowMessage > DisplayAlarmMessage> AlaramMessageVisible end ---------- Alarms-> " + visible.ToString());

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }


        /// <summary>
        /// 화면에 알림 노출 메세지  / 2023-10-23
        /// </summary>
        /// <param name="msg"></param>
        private void DispAlarmMessage(string msg, bool visible, uint displayTime = DEFAULT_ALRAM_TIME) 
        {
            try 
            { 
                if (!MainThread.IsMainThread)
                {
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "MainThread - false -------------------------------falsefalsefalsefalsefalsefalsefalsefalse------------------------------------------------------------" + msg);

                    MainThread.BeginInvokeOnMainThread(() => DispAlarmMessage(msg,visible, displayTime));
                    return;
                }

                if (StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.EXPO &&
                    StatusManager.Config.SystemType != FLAG_SYSTEM_TYPE.SUB_EXPO)
                {
                    _bottomInfo3.IsVisible = true;
                    //_bottomInfo3.Text = string.Empty;
                    BottomMessage = string.Empty;

                }
                else
                {
                    _bottomInfo3.IsVisible = visible;
                }

                msg = msg.Replace(ComLib.RST_OK, "")
                             .Replace(ComLib.RST_ERROR, "")
                             .Replace(ComLib.RST_RETRY, "")
                             .Replace(ComLib.RST_IGNORE, "");

                msg = msg.Replace(@"\n", "■")
                         .Replace(@"\r", "■")
                         .Replace(@"\\r", "■")
                         .Replace(@"\\n", "■")
                         .Replace(@"\r\n", "■")
                         .Replace(@"\\r\\n", "■");
                
                
                //바인딩으로 하단 메세지 처리로 변경  / 2023-11-20 
                BottomMessage = msg;
                //_bottomInfo3.Text = msg;

                //ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //                   , "DispAlarmMessage -- Message (END >> )  " + msg);

                 
                //메모리 사용 확인을 위한 임시주석 (메모리 누수 발생 ---확인이 필요한 기능..)/ 2023-10-26 
                //_bottomInfo3.ColorTo(Color.White, Color.Yellow, c => _bottomInfo3.TextColor = c, displayTime);
                
            }
            catch (Exception ex)
            {
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex.Message);
            }
        }

        /// <summary>
        /// 사용자 입력 KDS 번호 표시
        /// </summary>
        /// <param name="input"></param>
        private void DisplayInputKdsNo(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    MainThread.BeginInvokeOnMainThread(() => _bottomInfo4.Text = string.Empty);
                    return;
                }

                MainThread.BeginInvokeOnMainThread(() => _bottomInfo4.Text += input);
                if (_bottomInfo4.Text.Length == 3)
                {
                    MainThread.BeginInvokeOnMainThread(() => _bottomInfo4.Text = _bottomInfo4.Text.Substring(1, 2));
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// RevertList 파일에서 받아오기
        /// </summary>
        private void GetRevertList()
        {
            try
            {
                _revertList = new List<RevertInfo>();

                var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.GetRevertList);
                if (ret == ComLib.RST_IGNORE) return;
                if (ret != ComLib.RST_OK)
                {
                    ShowMessage(ret, MessageBoxTypes.Error);
                    return;
                }

                _revertList = DataManager.GetDataObject<List<RevertInfo>>();
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// RevertList 파일에 저장
        /// </summary>
        private void SaveRevertList(string ordNo)
        {
            try
            {
                var saveData = _dataList.Find(x => x.OrderNo == ordNo);
                if (saveData == null) return;

                if (_revertList.Count == MAX_REVERT_COUNT)
                    _revertList.RemoveAt(0);

                RevertInfo revertInfo = new RevertInfo();
                revertInfo.SaleDate = saveData.SaleDate;
                revertInfo.PosNo = saveData.PosNo;
                revertInfo.TranNo = saveData.TranNo;

                _revertList.Add(revertInfo);
                DataManager.SetDataObject<List<RevertInfo>>(_revertList);

                var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.SaveRevertList);
                if (ret != ComLib.RST_OK)
                {
                    ShowMessage(ret, MessageBoxTypes.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 조리/주문 완료처리
        /// </summary>
        /// <param name="ord"></param>
        private void OrderComplete(OrderInfo ord)
        {
            try
            {
                var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.CompleteOrder, ord);
                if (ret != ComLib.RST_OK)
                {
                    ShowMessage(ret, MessageBoxTypes.Error);
                    return;
                }

                if (ord.VoidType == FLAG_VOID_TYPE.CANCEL)
                    return;

                SaveRevertList(ord.OrderNo);
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// Pickup 완료처리
        /// </summary>
        /// <param name="ord"></param>
        private void OrderPickupComplete(OrderInfo ord)
        {
            try
            {
                var ret = ProcessManager.GetInstance<KdsProcess>().Execute(KdsProcess.Tasks.CompletePick, ord.SaleDate, ord.PosNo, ord.TranNo);
                if (ret != ComLib.RST_OK)
                {
                    ShowMessage(ret, MessageBoxTypes.Error);
                    return;
                }

                SaveRevertList(ord.OrderNo);
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }
         

        /// <summary>
        /// Landing Zone 자동 출력 //2023.05.31 LZ 자동출력
        /// OrderPrint 메소드 참고하여 구성 함. 
        /// </summary>
        /// <param name="ord"></param>
        private string OrderPrintAutoLandingZone(OrderInfo ord = null)
        {

            try
            {
                // 매개변수로 받은 OrderInfo는 페이지별로 나눠진 정보이므로 서버에서 받은 데이터에서 다시 찾는다.
                // (다량주문일때 페이지가 넘어가거나 row가 바뀌면 메뉴정보가 전부 없을 수 있기때문)
                var orderInfo = _dataList.Find(x => x.OrderNo == ord.OrderNo);
                if (orderInfo == null) return ComLib.RST_IGNORE;

                DataManager.SetDataObject<OrderInfo>(orderInfo);

                //재 시도 자동 출력여부 확인 
                Boolean isRePrint = false;

                string strOrderNo = string.Concat(StatusManager.Config.KdsNo, ord.TranNo, ord.OrderNo, ord.SaleDate);  // StatusManager.Config.KdsNo + ord.TranNo + ord.OrderNo;
                if (PrintFailOrderNo.Contains(strOrderNo)) 
                {
                    isRePrint = true; 
                }

                /*로그 추가 / 2023-09-23*/
                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "자동출력진입 (OrderPrintAutoLandingZone) --> start ");

                
                var ret = ProcessManager.GetInstance<PrintProcess>().OrderPrint(isRePrint);


                /*로그 추가 / 2023-09-23*/
                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "자동출력진입 (OrderPrintAutoLandingZone) ---------------> end ");



                return ret;
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
            return ComLib.RST_ERROR;
        }

        /// <summary>
        /// 라벨출력 처리
        /// </summary>
        /// <param name="ord"></param>
        private void OrderPrint(OrderInfo ord = null)
        {
            try
            {
                if (StatusManager.Config.PrinterUseYN == false)
                {
                    ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0041));
                    return;
                }
                if (StatusManager.Config.PrinterType == PrinterTypes.None)
                {
                    ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0064));
                    return;
                }

                if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                {
                    // 매개변수로 받은 OrderInfo는 페이지별로 나눠진 정보이므로 서버에서 받은 데이터에서 다시 찾는다.
                    // (다량주문일때 페이지가 넘어가거나 row가 바뀌면 메뉴정보가 전부 없을 수 있기때문)
                    var orderInfo = _dataList.Find(x => x.OrderNo == ord.OrderNo);
                    if (orderInfo == null) return;

                    string strCheckLastAutoPrintOrderNo = string.Concat(StatusManager.Config.KdsNo , ord.TranNo , ord.OrderNo, ord.SaleDate) ;

                    //이미 출력된 정보가 있다면  Reprint 여부를 확인할 수 있도록 영수증에 표기 함. / 2023-07-11 
                    if (LastAutoPrintOrderNo.Contains(strCheckLastAutoPrintOrderNo)) {
                        orderInfo.IsLandingZonePrinted = true; 
                    }

                    DataManager.SetDataObject<OrderInfo>(orderInfo);
                }
                else
                {
                    // 라벨자동출력 조건확인
                    if (StatusManager.Config.PrintItemType == FLAG_PRT_ITEM_TYPE.NOPRINT)
                    {
                        ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0065));
                        return;
                    }
                    if (StatusManager.Config.PrintItemType == FLAG_PRT_ITEM_TYPE.AUTO)
                    {
                        ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0066));
                        return;
                    }
                }
                var ret = ProcessManager.GetInstance<PrintProcess>().OrderPrint();
                if (ret != ComLib.RST_OK)
                {
                    ShowMessage(ret, MessageBoxTypes.Error);
                }

                if (ord == null) {
                    return; 
                }

                if (ret == ComLib.RST_OK) {

                    if (ord != null) {
                        string strCheckLastAutoPrintOrderNo = string.Concat(StatusManager.Config.KdsNo, ord.TranNo, ord.OrderNo, ord.SaleDate);

                        //이미 출력된 정보가 있다면  Reprint 여부를 확인할 수 있도록 영수증에 표기 함. / 2023-07-11 
                        LastAutoPrintOrderNo.Add(strCheckLastAutoPrintOrderNo);
                    }
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 라벨자동출력여부
        /// </summary>
        /// <returns></returns>
        private bool IsAutoPrintCheck()
        {
            try
            {
                // 자동출력대상 station 확인
                switch (StatusManager.Config.SystemType)
                {
                    case FLAG_SYSTEM_TYPE.BOARD:
                    case FLAG_SYSTEM_TYPE.FRY:
                    case FLAG_SYSTEM_TYPE.DnD:

                    case FLAG_SYSTEM_TYPE.LZ:           //2023.05.31 LZ 자동출력
                        break;
                    default:
                        return false;
                }

                // 자동출력대상 옵션 확인 
                if (StatusManager.Config.PrinterUseYN == false) return false;
                if (StatusManager.Config.PrintItemType != FLAG_PRT_ITEM_TYPE.AUTO) return false;

                return true;
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
                return false; 
            }
        }

        /// <summary>
        /// 라벨자동출력 재시도(재출력) 여부
        /// </summary>
        private bool IsAutoPrintRetry { get; set; } = false;
        /// <summary>
        /// 라벨자동출력 오류메시지 여부
        /// </summary>
        private bool IsAutoPrintError { get; set; } = false;
        /// <summary>
        /// 마지막 자동 출력된 정보 ( KDS_NO + TRAN_ON + CALL_NO )  //2023.05.31 LZ 자동출력
        /// </summary>
        private List<string> LastAutoPrintOrderNo { set; get; } = new List<string>();// string.Empty;
        /// <summary>
        /// 화면로딩 전에 주문된 이력은 자동출력에서 제외하기 위한 처리 ( LZ에서는 화면 로딩 이후의 주문을 자동출력 대상으로 함. ) 
        /// </summary>
        private string LoadingDatetime { set; get; } = string.Empty; //현재 로딩 시간 

        /// <summary>
        /// 프린터의 접속 상황에 따라서 출력실패된 주문번호를 관리하며 "재출력" 메세지를 상단에 노출하기 위하여 처음 출력되는 주문번호와 재출력되는 주문번호를 구분하기 위함. 
        /// </summary>
        private List<string> PrintFailOrderNo { set; get; } = new List<string>();


        private DateTime startTime = DateTime.Now;

        /// <summary>
        /// KDS 상품라벨 자동출력 <br />
        /// (Board, Fry, DnD Station) 
        /// </summary>
        private async void AutoPrintLabel()
        {
            try
            { 
                TimeSpan span = DateTime.Now - startTime;
                if (span.TotalSeconds < 5) return;   //출력실패 시 5초 후 프린터 재출력 시도 /2023-09-05


                // 자동출력 대상여부 확인 
                if (IsAutoPrintCheck() == false) return;

                if (StatusManager.Config.PrinterType == PrinterTypes.None)
                {
                    ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0064), MessageBoxTypes.Error);

                    //sleep 주석처리  20203-11-15 
                    //System.Threading.Thread.Sleep(300);

                    //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                    taskDelayTime(300);

                    return;
                }
                var prtType = ComLib.GetEnumCode(StatusManager.Config.PrinterType);
                if (prtType != "LAN")
                {
                    ShowMessage(MessageManager.GetTextMessage(KDS_MESSAGE.TEXT.MSG_0067), MessageBoxTypes.Error);

                    //sleep 주석처리  20203-11-15 
                    //System.Threading.Thread.Sleep(300);

                    //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                    taskDelayTime(300);


                    return;
                }

                var ret = ComLib.RST_ERROR;
                if (IsAutoPrintRetry)
                {
                    // 자동재출력
                    ret = ProcessManager.GetInstance<PrintProcess>().AutoRePrint();
                    if (ret.StartsWith(ComLib.RST_ERROR))
                    {
                        ShowMessage(ret, MessageBoxTypes.Error);
                        IsAutoPrintError = true;
                        ret = ComLib.RST_RETRY;

                        //sleep 주석처리  20203-11-15 
                        //System.Threading.Thread.Sleep(300);

                        //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                        taskDelayTime(300);
                    }

                    // 자동재출력모드설정
                    IsAutoPrintRetry = ret.StartsWith(ComLib.RST_RETRY);
                }
                else
                {
                    ////2023.05.31 자동출력 대상 : LZ, Expo, Sub Expo
                    if (StatusManager.Config.SystemType.Equals(FLAG_SYSTEM_TYPE.LZ) ||
                        StatusManager.Config.SystemType.Equals(FLAG_SYSTEM_TYPE.EXPO) ||
                        StatusManager.Config.SystemType.Equals(FLAG_SYSTEM_TYPE.SUB_EXPO))
                    {
                         
                        //화면에 노출된 주문내역 확인 
                        var _dataList = DataManager.GetDataObject<List<OrderInfo>>().ToList().Count;
                        if (_dataList < 1) 
                        {
                            //if (_datas.Keys.Count == 0) { //화면에 데이터가 없다면. 

                            //출력대상이 없다면 재출력 대상 및 출력 실패 대상을 초기화 함. / 2023-08-30
                            LastAutoPrintOrderNo.Clear();
                            PrintFailOrderNo.Clear();

                            //sleep 주석처리  20203-11-15 
                            //System.Threading.Thread.Sleep(300);  //출력할 내용이 없다면 0.3초 대기   /2023-10-10

                            //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                            taskDelayTime(300);

                            return;
                        }

                        //초기 로딩 시간 이후에 주문받은 정보만 자동출력 대상으로 함. 
                        if (LastAutoPrintOrderNo.Count == 0)
                        //if (string.IsNullOrEmpty(LastAutoPrintOrderNo))
                        {
                            LoadingDatetime = DateTime.Now.AddMinutes(-5).ToString("yyyyMMddHHmmss");
                            //LastAutoPrintOrderNo = "start";
                            LastAutoPrintOrderNo.Add("start");
                        }


                        //랜딩존 자동출력 대상은 화면에 노출된 (페이지에 상관없이 )  전체 주문 대상으로 함 / 2023-10-10 
                        var ordLst = DataManager.GetDataObject<List<OrderInfo>>()
                                                .Where(x => string.Compare(LoadingDatetime, x.OrderTime) <= 0)
                                                .Where(x => x.VoidType.Equals("N"))   //일반(VOID  제외 )                                             
                                                .Where(x => !LastAutoPrintOrderNo.Contains(string.Concat(StatusManager.Config.KdsNo, x.TranNo, x.OrderNo, x.SaleDate)))    //이미 출력된 것 제외 / 2023-10-10
                                                .ToList()
                                                ;
                         
                        //출력대상 카운트 확인 
                        if (ordLst == null || ordLst.Count == 0) {

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "XXXXXXXXX------출력할 주문 없음..------------XXXXXXXXX-------");

                            //출력대상이 없다면 재출력 대상 및 출력 실패 대상을 초기화 함. / 2023-08-30
                            //LastAutoPrintOrderNo.Clear();
                            PrintFailOrderNo.Clear();

                            //sleep 주석처리  20203-11-15 
                            //System.Threading.Thread.Sleep(300);  //출력할 내용이 없다면 0.3초 대기   /2023-10-10

                            //하단 자동 출력관련 메세지 노출 후 Expo에서 조리 완료 상황이 되면 메시지가 노출된 상태가 되어 있어서 출력되는 내용이 없다면 하단 메세지 노출은 초기화 한다. /2023-11-20
                            if (!string.IsNullOrEmpty(BottomMessage)) {
                                BottomMessage = string.Empty;
                            }

                            //sleep 처리 아래 메소드로 변경 함. 20203-11-15 
                            taskDelayTime(300);

                            return;
                        }

                        string strCheckLastAutoPrintOrderNo = string.Empty;

                        //LZ 화면에 노출된 출력 대상 목록
                        foreach (var ord in ordLst)
                        {
                            strCheckLastAutoPrintOrderNo = string.Concat(StatusManager.Config.KdsNo, ord.TranNo, ord.OrderNo, ord.SaleDate);

                            //출력호출 
                            ret = OrderPrintAutoLandingZone(ord);

                            if (ret.Equals(ComLib.RST_OK))
                            {
                                //정상출력된 정보는 자동 재출력되지 않도록 기록 함 ( KDS_NO + TRAN_ON + CALL_NO : order_no )  .   //2023.05.31 LZ 자동출력
                                //LastAutoPrintOrderNo.Add(StatusManager.Config.KdsNo + ord.TranNo + ord.OrderNo);
                                LastAutoPrintOrderNo.Add(strCheckLastAutoPrintOrderNo);

                                //재출력 대상 목록에서 삭제 함. / 2023-07-11 
                                //PrintFailOrderNo.Remove(StatusManager.Config.KdsNo + ord.TranNo + ord.OrderNo);
                                PrintFailOrderNo.Remove(strCheckLastAutoPrintOrderNo);

                                // 출력성공시 프린터 오류메시지 초기화
                                if (ret == ComLib.RST_OK && IsAutoPrintError)
                                {
                                    ShowMessage(string.Empty, MessageBoxTypes.Confirm);
                                    IsAutoPrintError = false;
                                }

                                //성공한 출력이라면 바로 출력할 수 있도록 대기하지 않음. / 2023-09-05
                                startTime = DateTime.Now.AddSeconds(-5);

                                //랜딩존 자동 출력 후 로그 
                                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                                   , "[{0}] Auto Print Info --> Form Loading complete Time=[{1}], OrderTime=[{2}], OrderNo=[{3:0000}], Result=[{4}]"
                                                   , StatusManager.Config.SystemType
                                                   , LoadingDatetime
                                                   , ord.OrderTime
                                                   , ord.OrderNo
                                                   , ret
                                                   );

                                //출력 후 0.5초 대기   /2023-10-10
                                //Thread.Sleep(500);  

                                //출력 후 0.5초 대기   /2023-11-15   //대기상태 Task로  변경   ///// 프로그램 다운로그가 여기서 확인되어 아래의 루프로 변경하여 테스트 진행. 
                                //taskDelayTime(500);


                                /*로그 추가 / 2023-11-14*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "Stopwatch 진입 ( 0.3초.) ------Stopwatch------Stopwatch-StopwatchStopwatchStopwatch------StopwatchStopwatchStopwatch");


                                var sw = new Stopwatch();
                                sw.Start();
                                while (true)
                                {
                                    try
                                    {
                                        /*로그 추가 / 2023-11-14*/
                                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                        //, "Stopwatch 루프..진행중.. ( 0.3초.) ------Stopwatch------" + sw.ElapsedMilliseconds.ToString());

                                        if (sw.ElapsedMilliseconds > 200) break;
                                    }
                                    catch (Exception ex) {

                                        /*로그 추가 / 2023-11-14*/
                                        ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                        , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                        + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                        , "Stopwatch 루프..XXXXXX Exception.XXXXXXX. ( 0.3초.) ------Stopwatch------" );

                                    }
                                }
                                sw.Stop();
                                

                                /*로그 추가 / 2023-11-14*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "Stopwatch 루프..종료 ( 0.5초.) ------Stopwatch------Stopwatch-StopwatchStopwatchStopwatch------StopwatchStopwatchStopwatch");


                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "botomTextClear 진입 >>>>>>>>>  ui thread 호출로 변경 " + ret);

                                botomTextClear();

                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "botomTextClear " + ret);


                                /*로그 추가 / 2023-11-14*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "PRINT-PRINT--PRINT--PRINT--PRINT--PRINT-------출력주문 후 대기 완료  -----PRINT--PRINT--PRINT--PRINT--PRINT--");

                            }
                            else if (ret.Equals(ComLib.RST_RETRY)) 
                            {
                                //김대령 차장 요청 ( 2023.07.11 ) 
                                //2023-07-11 출력 시도 후 출력되지 않은 랜딩존 출력물은 재 출력 시 : [재출력] 주문일자 : 20102302222  를 출력물 상단에 노출 함. / 
                                //재출력 대상 목록에서 삭제 함. / 2023-07-11 
                                /*
                                PrintFailOrderNo.Remove(StatusManager.Config.KdsNo + ord.TranNo + ord.OrderNo);
                                PrintFailOrderNo.Add(StatusManager.Config.KdsNo + ord.TranNo + ord.OrderNo);
                                */
                                PrintFailOrderNo.Remove(strCheckLastAutoPrintOrderNo);
                                PrintFailOrderNo.Add(strCheckLastAutoPrintOrderNo);

                                break; 
                            }
                            else if (ret.StartsWith(ComLib.RST_ERROR))
                            {

                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "  (ret.StartsWith(ComLib.RST_ERROR))  => ShowMessage 호출 시작 ###################################/*###################################/*###################################" + ret);


                                /*################################### 화면출력 임시 주석.*/
                                ///////////////////////////////////메모리 발생 의심 지점
                                ShowMessage(ret, MessageBoxTypes.Error);
                                IsAutoPrintError = true;
                                /*###########################################*/

                                /*로그 추가 / 2023-09-23*/
                                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                //, "ShowMessage 호출 종료 " + ret);

                                //
                                //System.Threading.Thread.Sleep(2000);
                                startTime = DateTime.Now;

                                break;
                            }
                        }
                    }
                    else
                    {
                        // 자동출력 ( Board ) 
                        ret = ProcessManager.GetInstance<PrintProcess>().AutoOrderPrint();
                        if (ret.StartsWith(ComLib.RST_ERROR))
                        {
                            ShowMessage(ret, MessageBoxTypes.Error);
                            IsAutoPrintError = true;

                            //출력 오류 시 sleep  / 2023-08-25
                            //System.Threading.Thread.Sleep(300);

                            startTime = DateTime.Now.AddSeconds(-3);  //board  2초 후 재 출력 시도 
                        }
                        else
                        {

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "botomTextClear 진입 >>>>>>>>>  ui thread 호출로 변경 " + ret);

                            botomTextClear();    

                            /*로그 추가 / 2023-09-23*/
                            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            //, "botomTextClear " + ret);
                        }
                        
                        // 자동재출력모드설정
                        IsAutoPrintRetry = ret.StartsWith(ComLib.RST_RETRY);
                    }
                }

                /*
                if (IsAutoPrintRetry) {

                    //Debug.Print("reprint");
                    
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , "FLAG FOR RE-PRINT");
                }
                */


                /*
                // 출력성공시 프린터 오류메시지 초기화
                if (ret == ComLib.RST_OK && IsAutoPrintError)
                {

                    //*로그 추가 / 2023-09-23 
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "출력성공시 프린터 오류메시지 초기화");

                    ShowMessage(string.Empty, MessageBoxTypes.Confirm);
                    IsAutoPrintError = false;
                }*/

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        private  void taskDelayTime(int waitTime) {

            try
            {
                /*로그 추가 / 2023-11-14*/
                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                , "-==========================>>>>>>>>>>>>>---  taskDelayTime  시작  -=========>>>>>>>>>>>>>---:      " + waitTime.ToString());

                //Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
                //Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
                //Thread.Sleep(waitTime);

                Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();

                /*로그 추가 / 2023-11-14*/
                ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                , "-===================================>>>>>--  taskDelayTime  끝  -======>>>>>>>>>>>>>>>>>>>>>>>:     " + waitTime.ToString());
            }
            catch (Exception ex)
            {

                //2023-10-18 add 
                ComLog.WriteAppLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "taskDelayTime exption : " + ex.ToString());
            }
        }

        private void botomTextClear() {

            try
            {
                /*로그 추가 / 2023-11-14*/
                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "botomTextClear  처음진입 -=--- MainThread.IsMainThread : " + MainThread.IsMainThread.ToString());


                //if (MainThread.IsMainThread)
                //{
                //    /*로그 추가 / 2023-11-14*/
                //    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //    , "botomTextClear  재 진입 -=---  MainThread.IsMainThread : true");
                //}


                    if (!MainThread.IsMainThread)
                {

                    /*로그 추가 / 2023-11-14*/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "botomTextClear MainThread.IsMainThread  에서 실행  -=---  MainThread.IsMainThread : " + MainThread.IsMainThread.ToString());


                    MainThread.BeginInvokeOnMainThread(() => botomTextClear());
                    return;
                }

                //화면에 노출된 내용이 있을 때만 초기화 함. - 2023-11-23
                if (!string.IsNullOrEmpty(BottomMessage))
                {

                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, " BottomMessage = string.Empty; 실행  -=---clear---- -=---clear---- -=---clear---- -=---clear---- -=---clear---- -=---clear---- -=---clear---- -=---clear---- -=---clear---- ");


                    BottomMessage = string.Empty;
                }

                /*
                if (!MainThread.IsMainThread)
                {
                    MainThread.BeginInvokeOnMainThread(() => botomTextClear());

                    //로그 추가 / 2023-11-14
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "botomTextClear  진입> 메인 스레드로 호출 리턴  -=---");

                    return;
                }

                if (!string.IsNullOrEmpty(_bottomInfo3.Text))
                {
                    //로그 추가 / 2023-11-14
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "botomTextClear  실행 전 -=---");

                    _bottomInfo3.Text = string.Empty;

                     //로그 추가 / 2023-11-14*
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "botomTextClear  실행 후-=---");
                }
                */

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 데이터표시
        /// </summary>
        /// <param name="page"></param>
        private bool DisplayData(int page = 0, bool isOrderData =false)
        {
            bool isBeep = false; 

            try
            {
                if (!MainThread.IsMainThread)
                {
                    

                    /*로그 추가 / 2023-09-23*/
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "DisplayData IsMainThread ==  > 1차 진입..---- " + MainThread.IsMainThread.ToString());

                    MainThread.BeginInvokeOnMainThread(() => DisplayData(page, isOrderData));
                    return isBeep; ;
                }

                if (MainThread.IsMainThread) {

                    threadCountCheck++;

                    /*로그 추가 / 2023-09-23*/
                    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    , "DisplayData IsMainThread ==  > 1차 진입  IsMainThread : {0}   " + MainThread.IsMainThread.ToString(), threadCountCheck);
                }

                //김신 수석Manager 추가 요청 사항 2023-11-29  ( lock 처리 ) 
                lock (lockDispObject) {

                    if (isOrderData) 
                    {
                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "DisplayData > isOrderData  > " + isOrderData.ToString());

                        var oldData = ComLib.DeepClone(_dataList);
                        _dataList = DataManager.GetDataObject<List<OrderInfo>>();

                        // 신규&변경주문확인
                        if (oldData != null)    
                        {
 
                            for (var i = _dataList.Count - 1; i >= 0; i--)
                            {
                                var data = _dataList[i];
                                var check = oldData.Where(x => x.SaleDate == data.SaleDate &&
                                                               x.PosNo == data.PosNo &&
                                                               x.TranNo == data.TranNo).FirstOrDefault();
                                if (check == null) isBeep = true;
                                if (check?.ModCnt < data.ModCnt) isBeep = true;

                                if (isBeep) break;
                            }
                        }

                    }

                    /*로그 추가 / 2023-09-23*/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "DisplayData > ProcessingData : 데이터 가공 시작   > " );



                    //데이터 가공      //김신 수석Manager 추가 요청 사항 2023-11-29 
                    ProcessingData(); 


                    // 화면분리(상/하)에 따라 데이터 표시 분기
                    if (StatusManager.Config.IsDivideDisplay)
                    {
                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "DisplayData1 :  >> start ");

                        DisplayData1(page);

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "DisplayData 1  : >> end ");


                    }
                    else
                    {
                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "DisplayData2  :  >> start   ");


                        DisplayData2(page);

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "DisplayData   2  : >> end ");

                    }


                    // Summary Display
                    if (StatusManager.Config.IsShowSummary)
                    {

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "SetSummaryData  start  ");


                        ///2023-11-29 _summary null체크 추가 
                        if (_summary == null)
                        {

                            ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                            , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                            + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                            , "_summary is null ????   ");

                        }
                        else {

                            SetSummaryData();
                            _summary.SetData(_summaryDataList);

                            _summary.IsVisible = StatusManager.Config.IsShowSummary;
                        }
                    
                    }

                    // add, 2023.03.06, BKJH EXPO 신규주문 알람제외 (안호성B 요청)
                    //                  EXPO에서 기능키 제어시(revert) 알람음 재생으로 야간의 지연현상 때문에 EXPO에서 신규주문 알람제외 
                    if (StatusManager.Config.CompanyCode == COMPANY.BKJH)
                    {
                        if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                            StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO)
                        {
                            isBeep = false;
                        }
                    }

                    if (isBeep)
                    {
                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "Beep start --> ");


                        // mod, 2023.03.01, BKJH 신규주문 알람음 소리가 작아서 다른 음원으로 변경
                        //ComLib.Beep(BeepKinds.Beep2);

                        // mod, 2023.04.04, BKR 종로구청점 알람음 소리가 작다고 해서 BKJH와 음원 통일함
                        //switch (StatusManager.Config.CompanyCode)
                        //{
                        //    case COMPANY.BKJH:  ComLib.Beep(BeepKinds.Beep3); break;
                        //    default:            ComLib.Beep(BeepKinds.Beep2); break;
                        //}

                        ComLib.Beep(BeepKinds.Beep3);

                        /*로그 추가 / 2023-09-23*/
                        //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                        //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                        //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                        //, "Beep end --> ");

                    }

                    /*로그 추가 / 2023-09-23*/
                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "SetSummaryData  end  ");
                    
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }

            /*로그 추가 / 2023-09-23*/
            //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
            //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
            //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
            //, "isBeep :   " + isBeep.ToString());

            return isBeep;

        }

        /// <summary>
        /// 상하 나눠서 데이터 표시
        /// </summary>
        /// <param name="page"></param>
        private void DisplayData1(int page = 0)
        {
            try
            {
                _ordInfoList.ForEach(x => x.IsVisible = false);
                _ordMenuList.ForEach(x => x.IsVisible = false);
                _ordVoidLabelList.ForEach(x => x.IsVisible = false);

                // 그리드
                // data 없음
                if (_datas.Keys.Count == 0) return;
                // 현재 페이지가 최대 페이지 이상일때
                if (_datas.Keys.Count <= page)
                {
                    _currentPage = _datas.Keys.Count - 1;
                    page = _datas.Keys.Count - 1;
                }

                var condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType1?.Split(','));
                var dispData = _datas[page].Where(x => condition.Contains(x.OrderType)).ToList();
                int dispCount = _dispCount / 2;
                int col = 0;
                int index = 0;

                // Order Info (UP)
                for (var i = 0; i < dispCount; i++)
                {
                    if (dispData.Count == i) break;

                    var orderInfo = new OrderInfo();
                    orderInfo.SaleDate = dispData[i].SaleDate;
                    orderInfo.PosNo = dispData[i].PosNo;
                    orderInfo.TranNo = dispData[i].TranNo;
                    orderInfo.OrderNo = dispData[i].OrderNo;
                    orderInfo.DispOrdNo = dispData[i].DispOrdNo;
                    orderInfo.OrderType = dispData[i].OrderType;
                    orderInfo.OrderEtc = dispData[i].OrderEtc;
                    orderInfo.DispSeq = dispData[i].DispSeq = index + 1;
                    orderInfo.OrderTime = dispData[i].OrderTime;
                    var ts = DateTime.Now - DateTime.ParseExact(dispData[i].OrderTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    var date = ts.Hours == 0 ? ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00")
                                             : ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    orderInfo.OrderDelayTime = date;
                    orderInfo.ProcStep = dispData[i].ProcStep;
                    orderInfo.VoidType = dispData[i].VoidType;
                    orderInfo.RevertFlag = dispData[i].RevertFlag;
                    orderInfo.OrderState = dispData[i].OrderState == FLAG_ORDER_STATE.ORDER_COMPLETE
                                                                   ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0006)
                                                                   : MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0007);
                    // 조리완료시 조리완료라고 표시
                    orderInfo.OrderState = dispData[i].ProcStep == FLAG_PROC_STEP.COOK_COMPLETED
                                                                 ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0015)
                                                                 : orderInfo.OrderState;
                    if (dispData[i].ModCnt > 0)
                        orderInfo.OrderState = orderInfo.OrderState + MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0029);
                    orderInfo.OrderTag = dispData[i].OrderTag;
                    orderInfo.Memo = dispData[i].Memo;
                    orderInfo.DispMemo = string.IsNullOrWhiteSpace(orderInfo.Memo) == false ? MEMO : string.Empty;

                    orderInfo.BorderColor = dispData[i].BorderColor;
                    orderInfo.BorderColor = dispData[i].VoidType == FLAG_VOID_TYPE.CANCEL ? StatusManager.ColorOption.BorderVoid : orderInfo.BorderColor;
                    orderInfo.DelayTimeFontColor = GetDelayColor(ts);
                    orderInfo.OrderItemCount = dispData[i].OrderItemCount;

                    // Grid Span(다량주문일 경우)
                    if (orderInfo.OrderItemCount > _lineCount)
                    {
                        // 스팬할 길이
                        int spanCnt = (orderInfo.OrderItemCount != 0 &&(orderInfo.OrderItemCount % _lineCount) == 0) ? (orderInfo.OrderItemCount / _lineCount)
                                                                                                                     : (orderInfo.OrderItemCount / _lineCount) + 1;
                        // 가능한 스팬길이 (현재 열에서)
                        int ableSpanCol = _colCount - col;

                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], spanCnt);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index + spanCnt - 1].IsVisible = true;

                        col = col + spanCnt;
                        index = index + spanCnt;
                    }
                    // 일반주문(다량주문 x)
                    else
                    {
                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], 1);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index].IsVisible = true;

                        col++;
                        index++;
                    }

                    if (col >= _colCount) break;
                }

                condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType2?.Split(','));
                dispData = _datas[page].Where(x => condition.Contains(x.OrderType)).ToList();
                col = 0;
                index = _dispCount / 2;

                // Order Info (DOWN)
                for (var i = 0; i < dispCount; i++)
                {
                    if (dispData.Count == i) break;

                    var orderInfo = new OrderInfo();
                    orderInfo.SaleDate = dispData[i].SaleDate;
                    orderInfo.PosNo = dispData[i].PosNo;
                    orderInfo.TranNo = dispData[i].TranNo;
                    orderInfo.OrderNo = dispData[i].OrderNo;
                    orderInfo.DispOrdNo = dispData[i].DispOrdNo;
                    orderInfo.OrderType = dispData[i].OrderType;
                    orderInfo.OrderEtc = dispData[i].OrderEtc;
                    orderInfo.DispSeq = dispData[i].DispSeq = index + 1;
                    orderInfo.OrderTime = dispData[i].OrderTime;
                    var ts = DateTime.Now - DateTime.ParseExact(dispData[i].OrderTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    var date = ts.Hours == 0 ? ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00")
                                             : ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    orderInfo.OrderDelayTime = date;
                    orderInfo.ProcStep = dispData[i].ProcStep;
                    orderInfo.VoidType = dispData[i].VoidType;
                    orderInfo.RevertFlag = dispData[i].RevertFlag;
                    orderInfo.OrderState = dispData[i].OrderState == FLAG_ORDER_STATE.ORDER_COMPLETE
                                                                   ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0006)
                                                                   : MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0007);
                    // 조리완료시 조리완료라고 표시
                    orderInfo.OrderState = dispData[i].ProcStep == FLAG_PROC_STEP.COOK_COMPLETED
                                                                 ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0015)
                                                                 : orderInfo.OrderState;
                    if (dispData[i].ModCnt > 0)
                        orderInfo.OrderState = orderInfo.OrderState + MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0029);
                    orderInfo.OrderTag = dispData[i].OrderTag;
                    orderInfo.Memo = dispData[i].Memo;
                    orderInfo.DispMemo = string.IsNullOrWhiteSpace(orderInfo.Memo) == false ? MEMO : string.Empty;

                    orderInfo.BorderColor = dispData[i].BorderColor;
                    orderInfo.BorderColor = dispData[i].VoidType == FLAG_VOID_TYPE.CANCEL ? StatusManager.ColorOption.BorderVoid : orderInfo.BorderColor;
                    orderInfo.DelayTimeFontColor = GetDelayColor(ts);
                    orderInfo.OrderItemCount = dispData[i].OrderItemCount;

                    // Grid Span(다량주문일 경우)
                    if (orderInfo.OrderItemCount > _lineCount)
                    {
                        // 스팬할 길이
                        int spanCnt = (orderInfo.OrderItemCount != 0 && (orderInfo.OrderItemCount % _lineCount) == 0) ? (orderInfo.OrderItemCount / _lineCount)
                                                                                                                      : (orderInfo.OrderItemCount / _lineCount) + 1;
                        // 가능한 스팬길이 (현재 열에서)
                        int offset = StatusManager.Config.IsShowSummary ? -1 : 0;
                        int ableSpanCol = _colCount - col + offset;

                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], spanCnt);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index + spanCnt - 1].IsVisible = true;

                        col = col + spanCnt;
                        index = index + spanCnt;
                    }
                    // 일반주문(다량주문 x)
                    else
                    {
                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], 1);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index].IsVisible = true;

                        col++;
                        index++;
                    }

                    if (col >= _colCount) break;
                }

                // Order MenuList (UP)
                condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType1?.Split(','));
                dispData = _datas[page].Where(x => condition.Contains(x.OrderType)).ToList();
                col = 0;
                index = 0;
                var isfull = false;
                for (var i = 0; i < dispCount; i++)
                {
                    if (dispData.Count == i) break;

                    var item = new List<OrderInfo.MenuItem>();
                    // 한 주문 메뉴개수가 라인수 초과면
                    if (dispData[i].OrderItemCount > _lineCount)
                    {
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);

                            if (j != 0 && (j + 1) % _lineCount == 0 && dispData[i].MenuList.Count - 1 != j)
                            {
                                _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                                _ordMenuList[index].SetData(item);
                                _ordMenuList[index].IsVisible = true;
                                col++;
                                index++;

                                item.Clear();
                                if (col == _colCount)
                                {
                                    isfull = true;
                                    break;
                                }
                            }
                        }
                    }
                    // 라인수 이하 메뉴개수일 때
                    else
                    {
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);
                        }
                    }

                    if (isfull) break;

                    _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                    _ordMenuList[index].SetData(item);
                    _ordMenuList[index].IsVisible = true;

                    col++;
                    index++;

                    if (col == _colCount) break;
                }

                // Order MenuList (DOWN)
                condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType2?.Split(','));
                dispData = _datas[page].Where(x => condition.Contains(x.OrderType)).ToList();
                col = 0;
                index = _dispCount / 2;
                isfull = false;
                for (var i = 0; i < dispCount; i++)
                {
                    if (dispData.Count == i) break;

                    // summary 표시되야하면
                    if (i == dispCount - 1 && StatusManager.Config.IsShowSummary)
                    {
                        break;
                    }

                    var item = new List<OrderInfo.MenuItem>();
                    // 한 주문 메뉴개수가 라인수 초과면
                    if (dispData[i].OrderItemCount > _lineCount)
                    {
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);

                            if (j != 0 && (j + 1) % _lineCount == 0 && dispData[i].MenuList.Count - 1 != j)
                            {
                                _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                                _ordMenuList[index].SetData(item);
                                _ordMenuList[index].IsVisible = true;
                                col++;
                                index++;

                                item.Clear();

                                if (col == _colCount || (StatusManager.Config.IsShowSummary && col == _colCount - 1))
                                {
                                    isfull = true;
                                    break;
                                }
                            }
                        }
                    }
                    // 라인수 이하 메뉴개수일 때
                    else
                    {
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);
                        }
                    }

                    if (isfull) break;

                    _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                    _ordMenuList[index].SetData(item);
                    _ordMenuList[index].IsVisible = true;

                    col++;
                    index++;
                    if (col == _colCount) break;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 상하 나누지않고 데이터 표시
        /// </summary>
        /// <param name="page"></param>
        private void DisplayData2(int page = 0)
        {
            try
            {
                _ordInfoList.ForEach(x => x.IsVisible = false);
                _ordMenuList.ForEach(x => x.IsVisible = false);
                _ordVoidLabelList.ForEach(x => x.IsVisible = false);

                // 그리드
                // data 없음
                if (_datas.Keys.Count == 0) return;
                // 현재 페이지가 최대 페이지 이상일때
                if (_datas.Keys.Count <= page)
                {
                    _currentPage = _datas.Keys.Count - 1;
                    page = _datas.Keys.Count - 1;
                }

                var condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType1?.Split(','));
                var dispData = _datas[page].Where(x => condition.Contains(x.OrderType)).ToList();

                int row = 0;
                int col = 0;
                int index = 0;

                // Order Info
                for (var i = 0; i < _dispCount; i++)
                {
                    if (dispData.Count == i) break;
                    if (row == _rowCount) break;

                    var orderInfo = new OrderInfo();
                    orderInfo.SaleDate = dispData[i].SaleDate;
                    orderInfo.PosNo = dispData[i].PosNo;
                    orderInfo.TranNo = dispData[i].TranNo;
                    orderInfo.OrderNo = dispData[i].OrderNo;
                    orderInfo.DispOrdNo = dispData[i].DispOrdNo;
                    orderInfo.OrderType = dispData[i].OrderType;
                    orderInfo.OrderEtc = dispData[i].OrderEtc;
                    orderInfo.DispSeq = dispData[i].DispSeq = index + 1;
                    orderInfo.OrderTime = dispData[i].OrderTime;
                    var ts = DateTime.Now - DateTime.ParseExact(dispData[i].OrderTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    var date = ts.Hours == 0 ? ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00")
                                             : ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    orderInfo.OrderDelayTime = date;
                    orderInfo.ProcStep = dispData[i].ProcStep;
                    orderInfo.VoidType = dispData[i].VoidType;
                    orderInfo.RevertFlag = dispData[i].RevertFlag;
                    orderInfo.OrderState = dispData[i].OrderState == FLAG_ORDER_STATE.ORDER_COMPLETE
                                                                   ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0006)
                                                                   : MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0007);
                    // 조리완료시 조리완료라고 표시
                    orderInfo.OrderState = dispData[i].ProcStep == FLAG_PROC_STEP.COOK_COMPLETED
                                                                 ? MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0015)
                                                                 : orderInfo.OrderState;
                    if (dispData[i].ModCnt > 0)
                        orderInfo.OrderState = orderInfo.OrderState + MessageManager.GetLableMessage(KDS_MESSAGE.LABEL.MSG_0029);
                    orderInfo.OrderTag = dispData[i].OrderTag;
                    orderInfo.Memo = dispData[i].Memo;
                    orderInfo.DispMemo = string.IsNullOrWhiteSpace(orderInfo.Memo) == false ? MEMO : string.Empty;

                    orderInfo.BorderColor = dispData[i].BorderColor;
                    orderInfo.BorderColor = dispData[i].VoidType == FLAG_VOID_TYPE.CANCEL ? StatusManager.ColorOption.BorderVoid : orderInfo.BorderColor;
                    orderInfo.DelayTimeFontColor = GetDelayColor(ts);
                    orderInfo.OrderItemCount = dispData[i].OrderItemCount;

                    // Grid Span(다량주문일 경우)
                    if (orderInfo.OrderItemCount > _lineCount)
                    {
                        // 스팬할 길이
                        int spanCnt = (orderInfo.OrderItemCount != 0 && (orderInfo.OrderItemCount % _lineCount) == 0) ? (orderInfo.OrderItemCount / _lineCount)
                                                                                                                      : (orderInfo.OrderItemCount / _lineCount) + 1;
                        // 가능한 스팬길이 (현재 열에서)
                        int offset = row == 1 && StatusManager.Config.IsShowSummary ? -1 : 0;
                        int ableSpanCol = _colCount - col + offset;

                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], spanCnt);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index + spanCnt - 1].IsVisible = true;

                        col = col + spanCnt;
                        index = index + spanCnt;
                    }
                    // 일반주문(다량주문 x)
                    else
                    {
                        _ordInfoList[index].Data = orderInfo;
                        _ordInfoList[index].IsVisible = true;
                        Grid.SetColumnSpan(_ordInfoList[index], 1);

                        if (orderInfo.VoidType == FLAG_VOID_TYPE.CANCEL)
                            _ordVoidLabelList[index].IsVisible = true;


                        col++;
                        index++;
                    }

                    if (col >= _colCount)
                    {
                        row++;
                        col = 0;
                    }
                }

                // Order MenuList
                row = 0;
                col = 0;
                index = 0;
                for (var i = 0; i < _dispCount; i++)
                {
                    if (dispData.Count == i) break;
                    if (row == _rowCount) break;

                    // summary 표시되야하면
                    if (i == _dispCount - 1 && StatusManager.Config.IsShowSummary)
                    {
                        break;
                    }

                    var item = new List<OrderInfo.MenuItem>();
                    // 한 주문 메뉴개수가 라인수 초과면
                    if (dispData[i].OrderItemCount > _lineCount)
                    {
                        var cnt = (dispData[i].OrderItemCount / _lineCount) + 1;
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);

                            if (j != 0 && (j + 1) % _lineCount == 0 && dispData[i].MenuList.Count - 1 != j)
                            {
                                _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                                _ordMenuList[index].SetData(item);
                                _ordMenuList[index].IsVisible = true;
                                col++;
                                index++;

                                item.Clear();
                                if (col == _colCount)
                                {
                                    col = 0;
                                    row++;
                                }
                                if (row == _rowCount) break;
                            }
                        }
                    }
                    // 라인수 이하 메뉴개수일 때
                    else
                    {
                        for (var j = 0; j < dispData[i].MenuList.Count; j++)
                        {
                            var order = new OrderInfo.MenuItem();
                            order.ItemCode = dispData[i].MenuList[j].ItemCode;
                            order.MainItemCode = dispData[i].MenuList[j].MainItemCode;
                            order.ItemName = dispData[i].MenuList[j].ItemName;
                            order.Seq = dispData[i].MenuList[j].Seq;
                            order.ItemType = dispData[i].MenuList[j].ItemType;
                            order.ItemAttr = dispData[i].MenuList[j].ItemAttr;
                            order.MenuColor = dispData[i].MenuList[j].MenuColor;
                            order.Qty = dispData[i].MenuList[j].Qty;
                            order.ProcStep = dispData[i].MenuList[j].ProcStep;

                            bool isShow = false;
                            // 1. 마지막 상품
                            if (dispData[i].MenuList.Count - 1 == j) isShow = true;
                            // 2. Expo, SubExpo, Lz : 단품이거나 세트,팩 상품
                            // 2-1. Board           : 단품이거나 세트,팩 구성품
                            if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO ||
                                StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.LZ)
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                   (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_MAIN)) isShow = true;
                            }
                            else
                            {
                                if (dispData[i].MenuList.Count - 1 != j &&
                                  (dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.SINGLE || dispData[i].MenuList[j + 1].ItemType == FLAG_ITEM_TYPE.COMBO_SUB)) isShow = true;
                            }

                            order.ShowBottomLine = isShow;
                            item.Add(order);
                        }
                    }

                    if (row == _rowCount) break;

                    _ordMenuList[index].OrderNo = dispData[i].OrderNo;
                    _ordMenuList[index].SetData(item);
                    _ordMenuList[index].IsVisible = true;

                    col++;
                    index++;
                    if (col == _colCount)
                    {
                        col = 0;
                        row++;
                    }
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// DisplayOrderType => OrderType
        /// </summary>
        /// <param name="dispOrdType"></param>
        /// <returns></returns>
        /// <remarks>
        /// KDS 마스터에 화면표시 Type이 DisplayOrderType으로 내려와서 OrderType으로 바꿔준다.
        /// </remarks>
        private List<string> DispOrdTypeToOrdType(string[] dispOrdType)
        {
            try
            {
                List<string> ordType = new List<string>(5) { FLAG_ORDER_TYPE.EAT_IN, FLAG_ORDER_TYPE.TAKE_OUT, FLAG_ORDER_TYPE.DELIVERY, FLAG_ORDER_TYPE.DRIVE_THRU, FLAG_ORDER_TYPE.MEAL };
                if (dispOrdType.Contains(FLAG_DISP_ORDER_TYPE.ALL))
                {
                    return ordType;
                }

                if (dispOrdType.Contains(FLAG_DISP_ORDER_TYPE.DINE_IN) == false)
                {
                    ordType.Remove(FLAG_ORDER_TYPE.EAT_IN);
                    ordType.Remove(FLAG_ORDER_TYPE.TAKE_OUT);
                    ordType.Remove(FLAG_ORDER_TYPE.MEAL);
                }

                if (dispOrdType.Contains(FLAG_DISP_ORDER_TYPE.DELIVERY) == false)
                {
                    ordType.Remove(FLAG_ORDER_TYPE.DELIVERY);
                }
                
                if (dispOrdType.Contains(FLAG_DISP_ORDER_TYPE.DRIVE_THRU) == false)
                {
                    ordType.Remove(FLAG_ORDER_TYPE.DRIVE_THRU);
                }

                return ordType;
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
                return null;
            }
        }

        /// <summary>
        /// 데이터 가공
        /// </summary>
        /// <remarks>
        /// 1. 다량주문(화면상 LineCount를 넘기는 메뉴수가 있는 주문)을 화면에 맞게 데이터 분배 <br />
        /// 2. 페이지에 맞게 데이터 분배
        /// </remarks>
        private void ProcessingData()
        {
            try
            {
                // 화면분리(상/하)에 따라 데이터 가공 분기
                if (StatusManager.Config.IsDivideDisplay)
                {
                    ProcessingData1();
                }
                else
                {
                    ProcessingData2();
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 상하 나눠서 데이터 가공
        /// </summary>
        private void ProcessingData1()
        {
            try
            {
                if (_datas == null)
                {
                    // page, pageOrdInfoList
                    _datas = new Dictionary<int, List<OrderInfo>>();
                }
                else
                {
                    _datas.Clear();
                }

                // ** 상단 **
                int page = 0;                   // 현재 페이지
                int addDispCnt = 0;             // 추가될 화면 수
                int filledCnt = 0;              // 현재 페이지에서 채워진 화면 수
                int addedCnt = 0;               // 추가된 페이지
                int offset = 0;                 // summary표시할 경우 화면 수 offset
                int dispCount = _dispCount / 2; // row당 화면 개수
                int ordSeq = 1;                 // 다량주문일때 주문번호 뒤에 붙을 seq
                var menuList = new List<OrderInfo>();

                var condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType1?.Split(','));
                foreach (var data in _dataList.Where(x => condition.Contains(x.OrderType)))
                {
                    if (_datas.ContainsKey(page) == false)
                        _datas.Add(page, menuList);

                    ordSeq = 1;
                    addedCnt = 0;
                    // 데이터가 다량주문인지 체크
                    addDispCnt = (data.OrderItemCount != 0 && (data.OrderItemCount % _lineCount) == 0) ? (data.OrderItemCount / _lineCount) : (data.OrderItemCount / _lineCount) + 1;
                    if (addDispCnt == 1)
                    {
                        data.DispOrdNo = data.OrderNo;
                        _datas[page].Add(data);
                        filledCnt++;
                    }
                    // 다량주문
                    else
                    {
                        // 화면분할 수를 넘기면 다음페이지에 추가
                        if (filledCnt + addDispCnt > dispCount)
                        {
                            // 남은 화면 수
                            int remainCnt = dispCount - filledCnt;
                            // 주문별 메뉴리스트 시작 index
                            int startIdx = 0;
                            // 추가될 상품 수
                            int addItemCnt = remainCnt * _lineCount;
                            List<OrderInfo.MenuItem> addOrdList = null;
                            OrderInfo ordInfo = null;

                            while (true)
                            {
                                // 남은 빈 화면 수
                                remainCnt = dispCount - filledCnt;
                                // 남은 화면 수만큼 GetRange하고
                                addOrdList = data.MenuList.GetRange(startIdx, addItemCnt);
                                // GetRange 한 만큼 idx 옮기고
                                startIdx = startIdx + addItemCnt;

                                // 주문 추가
                                ordInfo = data.Clone();
                                ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                ordInfo.OrderItemCount = addOrdList.Count;
                                ordInfo.IsGroup = true;
                                ordInfo.MenuList = addOrdList;
                                _datas[page].Add(ordInfo);

                                // 추가될 화면 수는 추가된 화면 수만큼 마이너스
                                addDispCnt = addDispCnt - remainCnt;
                                // 추가된 페이지 Count 추가
                                addedCnt = addedCnt + remainCnt;

                                // 나머지는 다음페이지에 추가
                                page++;
                                menuList = new List<OrderInfo>();
                                filledCnt = 0;
                                ordSeq++;

                                // 페이지 생성
                                if (_datas.ContainsKey(page) == false)
                                {
                                    _datas.Add(page, menuList);
                                }

                                // 추가될 화면수가 총 화면개수 보다 크면 loop 돌면서 계속 추가
                                if (addDispCnt > dispCount)
                                {
                                    // 추가될 상품수는 전체 화면 수만큼
                                    addItemCnt = dispCount * _lineCount;
                                }
                                else
                                {
                                    // 남은 메뉴 수만큼 GetRange
                                    addOrdList = data.MenuList.GetRange(startIdx, data.MenuList.Count - startIdx);

                                    // 주문 추가
                                    ordInfo = data.Clone();
                                    ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                    ordInfo.OrderItemCount = addOrdList.Count;
                                    ordInfo.IsGroup = true;
                                    ordInfo.MenuList = addOrdList;
                                    _datas[page].Add(ordInfo);

                                    var fCnt = (data.MenuList.Count != 0 && (data.MenuList.Count - addedCnt * _lineCount) % _lineCount == 0) ? (data.MenuList.Count - addedCnt * _lineCount) / _lineCount
                                                                                                                                             : ((data.MenuList.Count - addedCnt * _lineCount) / _lineCount) + 1;
                                    filledCnt = filledCnt + fCnt;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            data.IsGroup = true;
                            data.DispOrdNo = data.OrderNo;
                            _datas[page].Add(data);
                            var fCnt = (data.MenuList.Count != 0 && data.MenuList.Count % _lineCount == 0) ? (data.MenuList.Count / _lineCount) : (data.MenuList.Count / _lineCount) + 1;
                            filledCnt = filledCnt + fCnt;
                        }
                    }

                    if (filledCnt == dispCount)
                    {
                        page++;
                        menuList = new List<OrderInfo>();
                        filledCnt = 0;
                    }
                }

                // ** 하단 **
                page = 0;               // 현재 페이지
                addDispCnt = 0;         // 추가될 화면 수 
                filledCnt = 0;          // 현재 페이지에서 채워진 화면 수
                offset = StatusManager.Config.IsShowSummary ? -1 : 0; // summary표시할 경우 화면 수 offset
                dispCount = (_dispCount / 2) + offset;
                menuList = new List<OrderInfo>();
                condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType2?.Split(','));
                foreach (var data in _dataList.Where(x => condition.Contains(x.OrderType)))
                {
                    if (_datas.ContainsKey(page) == false)
                        _datas.Add(page, menuList);

                    ordSeq = 1;
                    addedCnt = 0;
                    // 데이터가 다량주문인지 체크
                    addDispCnt = (data.OrderItemCount != 0 && (data.OrderItemCount % _lineCount) == 0) ? (data.OrderItemCount / _lineCount) : (data.OrderItemCount / _lineCount) + 1;
                    if (addDispCnt == 1)
                    {
                        data.DispOrdNo = data.OrderNo;
                        _datas[page].Add(data);
                        filledCnt++;
                    }
                    // 다량주문
                    else
                    {
                        // 화면분할 수를 넘기면 다음페이지에 추가
                        if (filledCnt + addDispCnt > dispCount)
                        {
                            // 남은 화면 수
                            int remainCnt = dispCount - filledCnt;
                            // 주문별 메뉴리스트 시작 index
                            int startIdx = 0;
                            // 추가될 상품 수
                            int addItemCnt = remainCnt * _lineCount;
                            List<OrderInfo.MenuItem> addOrdList = null;
                            OrderInfo ordInfo = null;

                            while (true)
                            {
                                // 남은 화면 수
                                remainCnt = dispCount - filledCnt;
                                // 남은 화면 수만큼 GetRange하고 
                                addOrdList = data.MenuList.GetRange(startIdx, addItemCnt);
                                // GetRange 한 만큼 idx 옮기고
                                startIdx = startIdx + addItemCnt;

                                // 주문 추가
                                ordInfo = data.Clone();
                                ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                ordInfo.OrderItemCount = addOrdList.Count;
                                ordInfo.IsGroup = true;
                                ordInfo.MenuList = addOrdList;
                                _datas[page].Add(ordInfo);

                                // 추가될 화면 수는 추가된 화면 수만큼 마이너스 
                                addDispCnt = addDispCnt - remainCnt;
                                // 추가된 페이지 Count 추가
                                addedCnt = addedCnt + remainCnt;

                                // 나머지는 다음페이지에 추가
                                page++;
                                menuList = new List<OrderInfo>();
                                filledCnt = 0;
                                ordSeq++;

                                // 페이지 생성
                                if (_datas.ContainsKey(page) == false)
                                {
                                    _datas.Add(page, menuList);
                                }

                                // 추가될 화면수가 총 화면개수 보다 크면 loop 돌면서 계속 추가
                                if (addDispCnt > dispCount)
                                {
                                    // 추가될 상품수는 전체 화면 수만큼
                                    addItemCnt = dispCount * _lineCount;
                                }
                                else
                                {
                                    // 남은 메뉴 수만큼 GetRange
                                    addOrdList = data.MenuList.GetRange(startIdx, data.MenuList.Count - startIdx);

                                    // 주문 추가
                                    ordInfo = data.Clone();
                                    ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                    ordInfo.OrderItemCount = addOrdList.Count;
                                    ordInfo.IsGroup = true;
                                    ordInfo.MenuList = addOrdList;
                                    _datas[page].Add(ordInfo);

                                    var fCnt = (data.MenuList.Count != 0 && (data.MenuList.Count - addedCnt * _lineCount) % _lineCount == 0) ? (data.MenuList.Count - addedCnt * _lineCount) / _lineCount
                                                                                                                                             : ((data.MenuList.Count - addedCnt * _lineCount) / _lineCount) + 1;
                                    filledCnt = filledCnt + fCnt;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            data.IsGroup = true;
                            data.DispOrdNo = data.OrderNo;
                            _datas[page].Add(data);
                            var fCnt = (data.MenuList.Count != 0 && data.MenuList.Count % _lineCount == 0) ? (data.MenuList.Count / _lineCount) : (data.MenuList.Count / _lineCount) + 1;
                            filledCnt = filledCnt + fCnt;
                        }
                    }

                    if (filledCnt == dispCount)
                    {
                        page++;
                        menuList = new List<OrderInfo>();
                        filledCnt = 0;
                    }
                }

                _totalPage = _datas.Keys.Count;
                DisplayBottomInfo();
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 상하 나누는거 없이 데이터 가공
        /// </summary>
        private void ProcessingData2()
        {
            try
            {
                if (_datas == null)
                {
                    // page, pageOrdInfoList
                    _datas = new Dictionary<int, List<OrderInfo>>();
                }
                else
                {
                    _datas.Clear();
                }

                int page = 0;               // 현재 페이지
                int addDispCnt = 0;         // 아래 data가 표시될 화면 수
                int filledCnt = 0;          // 현재 페이지에서 채워진 화면 수
                int addedCnt = 0;           // 추가된 페이지
                int offset = StatusManager.Config.IsShowSummary ? -1 : 0; // summary표시할 경우 화면 수 offset
                int dispCount = _dispCount + offset;
                int topDispCount = _dispCount / 2;
                int btmDispCount = (_dispCount / 2) + offset;
                int ordSeq = 1;             // 다량주문일때 주문번호 뒤에 붙을 seq
                var menuList = new List<OrderInfo>();
                var condition = DispOrdTypeToOrdType(StatusManager.Config.DispOrdType1?.Split(','));

                int row = 0;
                foreach (var data in _dataList.Where(x => condition.Contains(x.OrderType)))
                {
                    if (_datas.ContainsKey(page) == false)
                        _datas.Add(page, menuList);

                    ordSeq = 1;
                    addedCnt = 0;
                    // 데이터가 다량주문인지 체크
                    addDispCnt = (data.OrderItemCount != 0 && (data.OrderItemCount % _lineCount) == 0) ? (data.OrderItemCount / _lineCount) : (data.OrderItemCount / _lineCount) + 1;
                    if (addDispCnt == 1)
                    {
                        data.DispOrdNo = data.OrderNo;
                        _datas[page].Add(data);
                        filledCnt++;
                    }
                    // 다량주문
                    else
                    {
                        dispCount = row == 0 ? topDispCount : btmDispCount;
                        // row == 0 : 줄을 바꿔야 하는경우
                        // row == 1 : page를 추가해야하는경우
                        if (filledCnt + addDispCnt > dispCount)
                        {
                            // 남은 화면 수
                            int remainCnt = dispCount - filledCnt;
                            // 주문별 메뉴리스트 시작 index
                            int startIdx = 0;
                            // 추가될 상품 수
                            int addItemCnt = remainCnt * _lineCount;
                            List<OrderInfo.MenuItem> addOrdList = null;
                            OrderInfo ordInfo = null;

                            while(true)
                            {
                                // 남은 빈 화면 수
                                remainCnt = dispCount - filledCnt;
                                // 남은 화면 수만큼 GetRange하고
                                addOrdList = data.MenuList.GetRange(startIdx, addItemCnt);
                                // GetRange 한 만큼 idx 옮기고
                                startIdx = startIdx + addItemCnt;

                                // 주문 추가
                                ordInfo = data.Clone();
                                ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                ordInfo.OrderItemCount = addOrdList.Count;
                                ordInfo.IsGroup = true;
                                ordInfo.MenuList = addOrdList;
                                _datas[page].Add(ordInfo);

                                // 추가될 화면 수는 추가된 화면 수만큼 마이너스
                                addDispCnt = addDispCnt - remainCnt;
                                addedCnt = addedCnt + remainCnt;

                                menuList = new List<OrderInfo>();
                                filledCnt = 0;
                                ordSeq++;

                                // 나머지는 다음줄에 추가
                                if (row == 0)
                                {
                                    row++;
                                }
                                // 나머지는 다음페이지에 추가
                                else
                                {
                                    row = 0;
                                    page++;
                                    // 페이지 생성
                                    if (_datas.ContainsKey(page) == false)
                                        _datas.Add(page, menuList);
                                }

                                dispCount = row == 0 ? topDispCount : btmDispCount;
                                    
                                // 추가될 화면수가 총 화면개수 보다 크면 loop 돌면서 계속 추가
                                if (addDispCnt > dispCount)
                                {
                                    // 추가될 상품수는 전체 화면 수만큼
                                    addItemCnt = dispCount * _lineCount;
                                }
                                else
                                {
                                    // 남은 메뉴 수만큼 GetRange
                                    addOrdList = data.MenuList.GetRange(startIdx, data.MenuList.Count - startIdx);

                                    // 주문 추가
                                    ordInfo = data.Clone();
                                    ordInfo.DispOrdNo = ordInfo.OrderNo + "-" + ordSeq;
                                    ordInfo.OrderItemCount = addOrdList.Count;
                                    ordInfo.IsGroup = true;
                                    ordInfo.MenuList = addOrdList;
                                    _datas[page].Add(ordInfo);

                                    var fCnt = (data.MenuList.Count != 0 && (data.MenuList.Count - addedCnt * _lineCount) % _lineCount == 0) ? (data.MenuList.Count - addedCnt * _lineCount) / _lineCount
                                                                                                                                             : ((data.MenuList.Count - addedCnt * _lineCount) / _lineCount) + 1;
                                    filledCnt = filledCnt + fCnt;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            data.IsGroup = true;
                            data.DispOrdNo = data.OrderNo;
                            _datas[page].Add(data);
                            var fCnt = (data.MenuList.Count != 0 && data.MenuList.Count % _lineCount == 0) ? (data.MenuList.Count / _lineCount) : (data.MenuList.Count / _lineCount) + 1;
                            filledCnt = filledCnt + fCnt;
                        }
                    }

                    dispCount = row == 0 ? topDispCount : btmDispCount;
                    if (filledCnt == dispCount)
                    {
                        filledCnt = 0;
                        if (row == 0)
                        {
                            row++;
                        }
                        else
                        {
                            row = 0;
                            page++;
                            menuList = new List<OrderInfo>();
                        }
                    }
                }

                _totalPage = _datas.Keys.Count;
                DisplayBottomInfo();
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }

        /// <summary>
        /// 지연색 가져오기
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        private Color GetDelayColor(TimeSpan timeSpan)
        {
            Color c = StatusManager.ColorOption.DelayColor1;
            try
            {
                // 지연색 변경
                if (timeSpan.TotalSeconds > DELAY_TIME.DELAY_4)
                {
                    c = StatusManager.ColorOption.DelayColor4;
                }
                else if (timeSpan.TotalSeconds > DELAY_TIME.DELAY_3)
                {
                    c = StatusManager.ColorOption.DelayColor3;
                }
                else if (timeSpan.TotalSeconds > DELAY_TIME.DELAY_2)
                {
                    c = StatusManager.ColorOption.DelayColor2;
                }
                else if (timeSpan.TotalSeconds > DELAY_TIME.DELAY_1)
                {
                    c = StatusManager.ColorOption.DelayColor1;
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);

            }
            return c;
        }

        /// <summary>
        /// SummaryData 세팅
        /// </summary>
        private void SetSummaryData()
        {
            try
            {
                _summaryDataList.Clear();
                if (_datas.Keys.Count == 0) return;

                foreach (var ordInfo in _dataList.Where(x => x.ProcStep == FLAG_PROC_STEP.ORDER_COMPLETED &&
                                                             x.VoidType == FLAG_VOID_TYPE.NORMAL))
                {
                    foreach (var data in ordInfo.MenuList.Where(x => x.ItemType == FLAG_ITEM_TYPE.SINGLE ||
                                                                     x.ItemType == FLAG_ITEM_TYPE.COMBO_SUB))
                    {
                        var item = _summaryDataList.Find(x => x.ItemCode == data.ItemCode);
                        if (item == null)
                        {
                            _summaryDataList.Add(data.Clone());
                        }
                        else
                        {
                            item.Qty += data.Qty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }


        private void ShowMessage(string msg, MessageBoxTypes type = MessageBoxTypes.Error, uint displayTime = DEFAULT_ALRAM_TIME, bool beep = false)
        {
            try
            {

                //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //   , "ShowMessage - start : " + msg);

                if (StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.EXPO ||
                    StatusManager.Config.SystemType == FLAG_SYSTEM_TYPE.SUB_EXPO)
                {
                    ViewManager.ShowMessageBox(type, msg, beep);
                }
                else
                {

                    //2023-10-24 임시 주석 
                    //DisplayAlarmMessage(msg, displayTime);

                    //ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                    //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                    //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                    //, "ShowMessage > DispAlarmMessage : start " + msg);


                    DispAlarmMessage(msg, true, displayTime);


                //    ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                //, "ShowMessage > DispAlarmMessage : end " + msg);

                    //}
                }

               // ComLog.WriteTraceLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
               //, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
               //+ System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
               //, "ShowMessage - end : " + msg);

            }
            catch (Exception ex)
            {
                ComLog.WriteFatalLog(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name
                                   , System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "."
                                   + System.Reflection.MethodBase.GetCurrentMethod().Name + "()"
                                   , ex);
            }
        }


        protected virtual void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
