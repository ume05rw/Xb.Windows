using System;

namespace Xb.App
{
    public partial class Thread
    {
        /// <summary>
        /// 別スレッドでのメソッド実行を管理するクラス
        /// </summary>
        /// <remarks>
        /// 
        /// App.Classコンストラクタにて クラスを生成した上で実行する機能があるが、
        /// 通常の実装ではインスタンス生成を実装者が行うため、本クラスでは対象にしない。
        /// 
        /// 実行サンプル：
        /// Private Sub Run()
        ///     Dim thread As ClsThread.Executer = New ClsThread.Executer(GetType(ClsFile), "GetNodeList")
        ///     AddHandler thread.Finished, AddressOf callback
        ///     thread.Execute(New Object() {"C:\Users\ikaruga\Downloads"})
        /// End Sub
        /// 
        /// Private Sub callback(ByVal sender As Object, ByVal e As ClsThread.Executer.FinishEventArgs)
        ///     Dim nodes As ClsFile.NodeList = CType(e.Result, ClsFile.NodeList)
        /// End Sub
        /// 
        /// 参考ページ：
        /// 第2回 .NETにおけるマルチスレッドの実装方法を総括 (3/4)
        /// http://www.atmarkit.co.jp/ait/articles/0504/20/news111_3.html
        /// 
        /// 文字列で指定されたクラスのインスタンスを作成し、メソッドを実行する
        /// http://dobon.net/vb/dotnet/programing/createinstancefromstring.html
        /// 
        /// 型のメンバを動的に呼び出す
        /// http://dobon.net/vb/dotnet/programing/typeinvokemember.html
        /// 
        /// メソッドやプロパティの有無を確認して呼び出すには？
        /// http://www.atmarkit.co.jp/fdotnet/dotnettips/359callbyname/callbyname.html
        /// 
        /// 指定したクラスのメソッド一覧を取得する
        /// http://blog.livedoor.jp/akf0/archives/51455238.html
        /// 
        /// </remarks>
        public class Executer : IDisposable
        {
            /// <summary>
            /// 終了イベントの引数クラス
            /// </summary>
            /// <remarks></remarks>
            public class FinishEventArgs : EventArgs
            {
                private object _result;
                public object Result
                {
                    get { return this._result; }
                }

                public FinishEventArgs(object res)
                {
                    this._result = res;
                }
            }

            //イベント用デリゲート
            public delegate void FinishEventHandler(object sender, FinishEventArgs e);
            public event FinishEventHandler Finished;

            //スレッド生成用デリゲート
            private delegate object ThreadDelegate(App.Class.Executer methodInfo);
            private readonly ThreadDelegate _threadDelegate;
            private delegate object ThreadDelegate2(App.Class.Executer methodInfo, string methodName, object[] methodArgs);

            private ThreadDelegate2 _threadDelegate2;

            private App.Class.Executer _classExec;


            /// <summary>
            /// コンストラクタ１ - Shared/Staticメソッドを実行する。
            /// </summary>
            /// <param name="classType"></param>
            /// <param name="methodName"></param>
            /// <param name="args"></param>
            /// <remarks></remarks>
            public Executer(System.Type classType, string methodName, object[] args = null)
            {
                this._classExec = new App.Class.Executer(classType, methodName, args);
                this._threadDelegate = new ThreadDelegate(this.ExecOnThread);
            }


            /// <summary>
            /// コンストラクタ２ - インスタンスメソッドを実行する。
            /// </summary>
            /// <param name="classInstance"></param>
            /// <param name="classType"></param>
            /// <param name="methodName"></param>
            /// <param name="args"></param>
            /// <remarks>
            /// ※インスタンスとその型を両方渡すのは冗長だが、一度Object型に
            /// ※キャストされたインスタンスから元の型を類推することが難しいため、
            /// ※この実装にする。
            /// </remarks>
            public Executer(object classInstance, System.Type classType, string methodName, object[] args = null)
            {
                this._classExec = new App.Class.Executer(classInstance, classType, methodName, args);
                this._threadDelegate = new ThreadDelegate(this.ExecOnThread);
            }


            /// <summary>
            /// コンストラクタ３ - インスタンスの生成のみを行う。
            /// </summary>
            /// <param name="classType"></param>
            /// <param name="classArgs"></param>
            /// <remarks></remarks>
            public Executer(System.Type classType, object[] classArgs = null)
            {
                this._classExec = new App.Class.Executer(classType, classArgs);
                this._threadDelegate = new ThreadDelegate(this.ExecOnThread);
            }


            /// <summary>
            /// 処理を開始する。
            /// </summary>
            /// <remarks></remarks>
            public void Execute()
            {
                //スレッドを生成して ExecOnThreadメソッドを実行する。
                //処理結果は Callback メソッドで受け取る。
                this._threadDelegate.BeginInvoke(this._classExec, new AsyncCallback(Callback), new object());
            }

            /// <summary>
            /// 処理を開始する。
            /// </summary>
            /// <remarks></remarks>

            public void Execute(string methodName, object[] methodArgs = null)
            {
                this._threadDelegate2 = new ThreadDelegate2(this.ExecOnThread2);

                //スレッドを生成して ExecOnThreadメソッドを実行する。
                //処理結果は Callback メソッドで受け取る。
                this._threadDelegate2.BeginInvoke(this._classExec, methodName, methodArgs, new AsyncCallback(Callback), new object());
            }


            /// <summary>
            /// UIスレッドで渡し値イベントをレイズする。
            /// </summary>
            /// <param name="eventType"></param>
            /// <param name="args"></param>
            /// <remarks></remarks>
            private void FireEvent(object eventType, object args)
            {
                switch (eventType.ToString())
                {
                    case "Finished":
                        if (Finished != null)
                        {
                            Finished(this, new FinishEventArgs(args));
                        }

                        break;
                    default:
                        break;
                        //何もしない。
                }
            }


            /// <summary>
            /// 別スレッドで実行する関数
            /// </summary>
            /// <param name="method"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private object ExecOnThread(App.Class.Executer method)
            {
                //ここでの戻り値は、Callbackメソッド上で EndInvokeして取得する。
                object result = null;
                try
                {
                    if (object.ReferenceEquals(method.MethodType, typeof(void)) )
                    {
                        method.Execute();
                    }
                    else
                    {
                        result = method.Execute();
                    }
                }
                catch (Exception ex)
                {
                    //例外が発生したとき、呼び出し先の例外をスローする。
                    Xb.Util.Out("Xb.App.Thread.Executer.ExecOnThread: " + ex.InnerException.Message);
                    throw ex.InnerException;
                }

                return result;
            }

            /// <summary>
            /// 別スレッドで実行する関数
            /// </summary>
            /// <param name="method"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private object ExecOnThread2(App.Class.Executer method, string methodName, object[] methodArgs)
            {
                //ここでの戻り値は、Callbackメソッド上で EndInvokeして取得する。
                object result = null;
                try
                {
                    method.SetMethod(methodName, methodArgs);

                    if (object.ReferenceEquals(method.MethodType, typeof(void)))
                    {
                        method.Execute();
                    }
                    else
                    {
                        result = method.Execute();
                    }
                }
                catch (Exception ex)
                {
                    //例外が発生したとき、呼び出し先の例外をスローする。
                    Xb.Util.Out("Xb.App.Thread.Executer.ExecOnThread: " + ex.InnerException.Message);
                    throw ex.InnerException;
                }

                return result;
            }


            /// <summary>
            /// スレッド終了時の処理
            /// </summary>
            /// <param name="ar"></param>
            /// <remarks></remarks>
            private void Callback(IAsyncResult ar)
            {
                object result = null;

                if (ar.IsCompleted)
                {
                    //戻り値が存在するときのみ、取得する。
                    if (!object.ReferenceEquals(this._classExec.MethodType, typeof(void)))
                    {
                        result = this._threadDelegate.EndInvoke(ar); //this._classExec, ar);
                    }

                    //処理終了イベントをレイズする。
                    this.FireEvent("Finished", result);
                }
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
                        // TODO: スレッドの強制終了方法を検討する。
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
