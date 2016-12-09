using System;

namespace Xb.App
{
    public partial class Process
    {
        /// <summary>
        /// 実行ファイルを起動してそのプロセスを管理するクラス
        /// </summary>
        /// <remarks></remarks>
        public class Executer : IDisposable
        {
            /// <summary>
            /// 終了イベントの引数クラス
            /// </summary>
            /// <remarks></remarks>
            public class FinishEventArgs : EventArgs
            {
                private string _result;

                /// <summary>
                /// プロセス終了時の標準出力内容
                /// </summary>
                /// <value></value>
                /// <returns></returns>
                /// <remarks></remarks>
                public string Result
                {
                    get { return this._result; }
                }

                /// <summary>
                /// コンストラクタ
                /// </summary>
                /// <param name="res"></param>
                /// <remarks></remarks>
                public FinishEventArgs(string res)
                {
                    this._result = res;
                }
            }

            public delegate void FinishEventHandler(object sender, FinishEventArgs e);
            public event FinishEventHandler Finished;
            public event EventHandler Failed;

            private delegate void ProcessDelegate(System.Diagnostics.Process process);
            private readonly ProcessDelegate _processDelegate;

            private System.Diagnostics.Process _process;

            private System.DateTime _startTime;

            /// <summary>
            /// プロセスオブジェクト
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public System.Diagnostics.Process Process
            {
                get { return this._process; }
            }


            /// <summary>
            /// プロセス開始時点のシステム日付
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public System.DateTime StartTime
            {
                get { return this._startTime; }
            }


            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="fileName"></param>
            /// <param name="arguments"></param>
            /// <param name="isCommandline"></param>
            /// <param name="errorDialogParentHandle"></param>
            /// <remarks>
            /// 参考：
            /// UACが有効の時、必要な処理だけ管理者に昇格させて実行する
            /// http://dobon.net/vb/dotnet/system/runelevated.html
            /// </remarks>
            public Executer(string fileName, 
                            string arguments = "", 
                            bool isCommandline = false, 
                            IntPtr? errorDialogParentHandle = null)
            {
                this._process = new System.Diagnostics.Process();

                if (!isCommandline)
                {
                    //通常のプログラム実行のとき
                    this._process.StartInfo.FileName = fileName;
                    this._process.StartInfo.Arguments = arguments;
                    this._process.StartInfo.UseShellExecute = true;
                    this._process.StartInfo.CreateNoWindow = false;
                }
                else
                {
                    //コマンドラインプログラムを実行するとき
                    //"cmd.exe"を実行プログラムとする。
                    this._process.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
                    //cmd.exeの引数として、渡し値ファイル名と引数をセット。
                    this._process.StartInfo.Arguments = "/c " + fileName + " " + arguments;

                    this._process.StartInfo.UseShellExecute = false;
                    this._process.StartInfo.CreateNoWindow = true;
                    this._process.StartInfo.RedirectStandardOutput = true;
                    this._process.StartInfo.RedirectStandardInput = false;
                }

                this._process.StartInfo.Verb = "runas";
                //動詞に「runas」をつける

                if (errorDialogParentHandle != null)
                {
                    //UACダイアログが親プログラムに対して表示されるようにする
                    this._process.StartInfo.ErrorDialog = true;
                    this._process.StartInfo.ErrorDialogParentHandle = (IntPtr)errorDialogParentHandle;
                }

                this._processDelegate = new ProcessDelegate(ThreadProcess);

                //呼び出し元でイベントハンドラ定義後に Executeメソッドを使用してプロセスを開始する。
            }


            /// <summary>
            /// 処理を開始する。
            /// </summary>
            /// <remarks></remarks>
            public void Execute()
            {
                //別スレッドでプロセスを起動する。戻り値を取得しないので、BeginInvokeでなくInvoke。
                this._processDelegate.Invoke(this._process);
            }


            /// <summary>
            /// イベントをレイズする。
            /// </summary>
            /// <param name="eventType"></param>
            /// <param name="args"></param>
            /// <remarks>Windowsフォームでイベントを受け取る際は、UIスレッドか否かに注意すること</remarks>
            private void FireEvent(object eventType, object args)
            {
                switch (eventType.ToString())
                {
                    case "Failed":
                        Failed?.Invoke(this, new EventArgs());

                        break;
                    case "Finished":
                        Finished?.Invoke(this, new FinishEventArgs(args.ToString()));

                        break;
                    default:
                        break;
                        //何もしない。
                }
            }

            /// <summary>
            /// プロセスを実行するスレッドメソッド
            /// </summary>
            /// <param name="process"></param>
            /// <remarks></remarks>
            private void ThreadProcess(System.Diagnostics.Process process)
            {
                string result = null;

                try
                {
                    this._process.Start();
                }
                catch (Exception)
                {
                    //処理失敗イベントをレイズする。
                    this.FireEvent("Failed", null);
                    return;
                }

                //プロセス開始時点のシステム日付を取得しておく
                this._startTime = DateTime.Now;

                try
                {
                    //出力を読み取る
                    result = this._process.StandardOutput.ReadToEnd();
                }
                catch (Exception)
                {
                    //応答が取得出来ないプロセスでは常に例外が発生する。
                }

                try
                {
                    //プロセス終了まで待機する
                    //WaitForExitはReadToEndの後である必要がある
                    //(親プロセス、子プロセスでブロック防止のため)
                    this._process.WaitForExit();

                    this._process.Close();

                    //処理終了イベントをレイズする。
                    if ((result == null))
                        result = "";
                    this.FireEvent("Finished", result);

                }
                catch (Exception)
                {
                    //例外が発生したとき
                    //処理失敗イベントをレイズする。
                    this.FireEvent("Failed", null);
                }
            }


            /// <summary>
            /// プロセスが開始してからの経過時間を取得する。
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public TimeSpan GetActiveTimespan()
            {
                return DateTime.Now.Subtract(this._startTime);
            }


            /// <summary>
            /// プロセスを強制終了して本オブジェクトを破棄する。
            /// </summary>
            /// <remarks></remarks>
            public void Close()
            {
                this.Dispose();
            }


            // 重複する呼び出しを検出するには
            private bool _disposedValue = false;

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!this._disposedValue)
                {
                    if (disposing)
                    {
                        if (((this._process != null)))
                            this._process.Kill();
                    }
                }
                this._disposedValue = true;
            }

            #region " IDisposable Support "
            // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
