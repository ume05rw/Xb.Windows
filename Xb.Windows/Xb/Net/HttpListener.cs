using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Xb.Net
{
    /// <summary>
    /// Httpリスナークラス
    /// </summary>
    /// <remarks>
    /// 動作には管理者権限が必要。
    /// </remarks>
    public class HttpListener : IDisposable
    {
        /// <summary>
        /// リスナー接続イベント引数クラス
        /// </summary>
        /// <remarks></remarks>
        public class AcceptEventArgs : EventArgs
        {
            public System.Net.HttpListenerRequest Request;
            public System.Net.HttpListenerResponse Response;
            public AcceptEventArgs(System.Net.HttpListenerContext context)
            {
                this.Request = context.Request;
                this.Response = context.Response;
            }
        }

        public delegate void AcceptEventHandler(object sender, AcceptEventArgs e);
        public event AcceptEventHandler Accepted;

        private int _port;
        private System.Threading.Thread _listenerThread;
        private System.Net.HttpListener _httpListener;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="port"></param>
        /// <remarks></remarks>
        public HttpListener(int port = 80)
        {
            this._port = port;
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
                case "Accepted":
                    try
                    {
                        Accepted?.Invoke(this, new AcceptEventArgs((System.Net.HttpListenerContext)args));
                    }
                    catch (Exception)
                    {
                        Xb.Util.Out("Xb.Net.Http.Listener.FireEvent: イベント用パラメータパースに失敗しました。");
                        throw new ArgumentException("Xb.Net.Http.Listener.FireEvent: イベント用パラメータパースに失敗しました。");
                    }
                    break;
                default:
                    break;
                    //何もしない。
            }
        }


        /// <summary>
        /// 待ち受け動作を開始する。
        /// </summary>
        /// <remarks></remarks>
        public void Start()
        {
            this._listenerThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Listen));
            this._listenerThread.Start(this._port);
        }


        /// <summary>
        /// 待ち受け動作を停止し、インスタンスを破棄する。
        /// </summary>
        /// <remarks></remarks>
        public void Close()
        {
            this.Dispose();
        }


        /// <summary>
        /// 待ち受け処理開始 - 別スレッドで実行するメソッド
        /// </summary>
        /// <param name="port"></param>
        /// <remarks></remarks>
        private void Listen(object port)
        {
            string prefix = string.Format("http://*:{0}/", Convert.ToInt32(port));

            this._httpListener = new System.Net.HttpListener();
            this._httpListener.Prefixes.Add(prefix);
            this._httpListener.Start();
            this._httpListener.BeginGetContext(_httpListener_Accept, new object[]{});
        }


        /// <summary>
        /// Httpリスナー接続受付時のイベント処理
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>
        private void _httpListener_Accept(IAsyncResult ar)
        {
            System.Net.HttpListenerContext context = null;
            try
            {
                context = this._httpListener.GetContext();
            }
            catch (Exception)
            {
                //HttpListener.Stopメソッド実行時はここに入る。
                return;
            }

            //接続受付イベントをレイズする。
            this.FireEvent("Accepted", context);
            context.Response.Close();

            //再度待ち受けを開始する。
            this._httpListener.BeginGetContext(_httpListener_Accept, new object[] {});
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
                    if (this._httpListener != null)
                    {
                        if (this._httpListener.IsListening)
                            this._httpListener.Stop();
                    }
                    if (this._listenerThread != null)
                    {
                        this._listenerThread.Abort();
                    }
                    this._httpListener = null;
                    this._listenerThread = null;
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
