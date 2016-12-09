using System;

namespace Xb.App
{

    /// <summary>
    /// OS設定画面関数群
    /// </summary>
    /// <remarks></remarks>
    public class Os
    {
        /// <summary>
        /// コントロールパネル／管理ツール等、Windows設定画面区分
        /// </summary>
        /// <remarks></remarks>
        public enum Form
        {
            /// <summary>
            /// コントロールパネル
            /// </summary>
            /// <remarks></remarks>
            ControlPanel,

            /// <summary>
            /// プログラムと機能
            /// </summary>
            /// <remarks></remarks>
            Programs,

            /// <summary>
            /// 画面のプロパティ
            /// </summary>
            /// <remarks></remarks>
            WindowProperty,

            /// <summary>
            /// 地域と言語のオプション
            /// </summary>
            /// <remarks></remarks>
            Locale,


            /// <summary>
            /// 日付と時刻
            /// </summary>
            /// <remarks></remarks>
            DateAndTime,

            /// <summary>
            /// システムのプロパティ
            /// </summary>
            /// <remarks></remarks>
            SystemProperty,

            /// <summary>
            /// ネットワーク接続
            /// </summary>
            /// <remarks></remarks>
            NetworkAdapters,

            /// <summary>
            /// ネットワークと共有センター
            /// </summary>
            /// <remarks></remarks>
            NetworkCenter,

            /// <summary>
            /// 電源オプション
            /// </summary>
            /// <remarks></remarks>
            PowerOption,

            /// <summary>
            /// フォント
            /// </summary>
            /// <remarks></remarks>
            Fonts,

            /// <summary>
            /// デバイスとプリンター
            /// </summary>
            /// <remarks></remarks>
            Printers,

            /// <summary>
            /// Bluetoothデバイス
            /// </summary>
            /// <remarks></remarks>
            Bluetooth,

            /// <summary>
            /// Windowsファイアウォール
            /// </summary>
            /// <remarks></remarks>
            WindowsFirewall,

            /// <summary>
            /// ユーザーアカウント
            /// </summary>
            /// <remarks></remarks>
            UserAccount,

            /// <summary>
            /// フォルダオプション
            /// </summary>
            /// <remarks></remarks>
            FolderOption,

            /// <summary>
            /// Windows Update
            /// </summary>
            /// <remarks></remarks>
            WindowsUpdate,

            /// <summary>
            /// 管理ツール
            /// </summary>
            /// <remarks></remarks>
            ManagementTools,

            /// <summary>
            /// デバイスマネージャ
            /// </summary>
            /// <remarks></remarks>
            DeviceManager,

            /// <summary>
            /// イベントビューア
            /// </summary>
            /// <remarks></remarks>
            EventViewer,

            /// <summary>
            /// サービス
            /// </summary>
            /// <remarks></remarks>
            Services,

            /// <summary>
            /// タスクスケジューラ
            /// </summary>
            /// <remarks></remarks>
            TaskScheduler,

            /// <summary>
            /// パフォーマンスモニタ
            /// </summary>
            /// <remarks></remarks>
            PerformanceMonitor,

            /// <summary>
            /// コンピュータの管理
            /// </summary>
            /// <remarks></remarks>
            ComputerManager,

            /// <summary>
            /// 証明書
            /// </summary>
            /// <remarks></remarks>
            Certificate,

            /// <summary>
            /// ローカルユーザーとグループ
            /// </summary>
            /// <remarks></remarks>
            LocalUserAndGroup,

            /// <summary>
            /// ディスクデフラグツール
            /// </summary>
            /// <remarks></remarks>
            DeflagTool,

            /// <summary>
            /// ディスクの管理
            /// </summary>
            /// <remarks></remarks>
            DiskManager,

            /// <summary>
            /// ディスククリーンアップ
            /// </summary>
            /// <remarks></remarks>
            DiskCleanup
        }


        /// <summary>
        /// コントロールパネル／管理ツール画面を表示する。
        /// </summary>
        /// <param name="formKind"></param>
        /// <remarks>
        /// Win8で動作検証。Vista,XPでは動作しないものもあると思われる。
        /// </remarks>

        public static void OpenForm(App.Os.Form formKind)
        {
            System.Diagnostics.ProcessStartInfo procInfo = new System.Diagnostics.ProcessStartInfo();
            System.Diagnostics.Process proc = new System.Diagnostics.Process();

            switch (formKind)
            {
                case Form.ControlPanel:
                    //コントロールパネル
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "";
                    break;
                case Form.Programs:
                    //プログラムと機能
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "appwiz.cpl";
                    break;
                case Form.WindowProperty:
                    //画面のプロパティ
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "desk.cpl";
                    break;
                case Form.Locale:
                    //地域と言語のオプション
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "intl.cpl";
                    break;
                case Form.DateAndTime:
                    //日付と時刻
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "timedate.cpl";
                    break;
                case Form.SystemProperty:
                    //システムのプロパティ
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "sysdm.cpl";
                    break;
                case Form.NetworkAdapters:
                    //ネットワーク接続
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "ncpa.cpl";
                    break;
                case Form.NetworkCenter:
                    //ネットワークと共有センター
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "/name Microsoft.NetworkAndSharingCenter";
                    break;
                case Form.PowerOption:
                    //電源オプション
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "powercfg.cpl";
                    break;
                case Form.Fonts:
                    //フォント
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "fonts";
                    break;
                case Form.Printers:
                    //デバイスとプリンター
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "printers";
                    break;
                case Form.Bluetooth:
                    //Bluetoothデバイス
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "bthprops.cpl";
                    break;
                case Form.WindowsFirewall:
                    //Windowsファイアウォール
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "firewall.cpl";
                    break;
                case Form.UserAccount:
                    //ユーザーアカウント
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "nusrmgr.cpl";
                    break;
                case Form.FolderOption:
                    //フォルダオプション
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "folders";
                    break;
                case Form.WindowsUpdate:
                    //Windows Update
                    procInfo.FileName = "wuapp.exe";
                    procInfo.Arguments = "";
                    break;
                case Form.ManagementTools:
                    //管理ツール
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "admintools";
                    break;
                case Form.DeviceManager:
                    //デバイスマネージャ
                    procInfo.FileName = "devmgmt.msc";
                    procInfo.Arguments = "";
                    break;
                case Form.EventViewer:
                    //イベントビューア
                    procInfo.FileName = "eventvwr.msc";
                    procInfo.Arguments = "/s";
                    break;
                case Form.Services:
                    //サービス
                    procInfo.FileName = "services.msc";
                    procInfo.Arguments = "";
                    break;
                case Form.TaskScheduler:
                    //タスクスケジューラ
                    procInfo.FileName = "control.exe";
                    procInfo.Arguments = "schedtasks";
                    break;
                case Form.PerformanceMonitor:
                    //パフォーマンスモニタ
                    procInfo.FileName = "perfmon.msc";
                    procInfo.Arguments = "/s";
                    break;
                case Form.ComputerManager:
                    //コンピュータの管理
                    procInfo.FileName = "compmgmt.msc";
                    procInfo.Arguments = "/s";
                    break;
                case Form.Certificate:
                    //証明書
                    procInfo.FileName = "certmgr.msc";
                    procInfo.Arguments = "";
                    break;
                case Form.LocalUserAndGroup:
                    //ローカルユーザーとグループ
                    procInfo.FileName = "lusrmgr.msc";
                    procInfo.Arguments = "";
                    break;
                case Form.DeflagTool:
                    //ディスクデフラグツール
                    procInfo.FileName = "dfrgui.exe";
                    procInfo.Arguments = "";
                    break;
                case Form.DiskManager:
                    //ディスクの管理
                    procInfo.FileName = "diskmgmt.msc";
                    procInfo.Arguments = "";
                    break;
                case Form.DiskCleanup:
                    //ディスクのクリーンアップ
                    procInfo.FileName = "cleanmgr.exe";
                    procInfo.Arguments = "";
                    break;
                default:
                    Xb.Util.Out("Xb.App.Os.OpenForm: 設定画面区分が不明です。");
                    throw new ArgumentException("Xb.App.Os.OpenForm: 設定画面区分が不明です。");
            }

            proc.StartInfo = procInfo;
            proc.Start();

        }
    }
}
