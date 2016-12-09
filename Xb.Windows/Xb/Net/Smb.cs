using System;
using System.Runtime.InteropServices;

namespace Xb.Net
{

    /// <summary>
    /// 共有フォルダへのアクセス管理クラス
    /// </summary>
    /// <remarks></remarks>
    public class Smb : IDisposable
    {
        /// <summary>
        /// WNetAddConnection2用、共有フォルダアクセスパラメータ用構造体
        /// </summary>
        /// <remarks></remarks>
        public struct Netresource
        {
            //列挙の範囲
            public int DwScope;
            //リソースタイプ
            public int DwType;
            //表示オブジェクト
            public int DwDisplayType;
            //リソースの使用方法
            public int DwUsage;
            //ローカルデバイス名。使わないならNULL。
            public string LpLocalName;
            //リモートネットワーク名。使わないならNULL
            public string LpRemoteName;
            //ネットワーク内の提供者に提供された文字列
            public string LpComment;
            //リソースを所有しているプロバイダ名
            public string LpProvider;
        }

        private readonly string _address;
        private readonly string _share;
        private readonly string _userName;
        private readonly string _password;
        //接続先共有フォルダのUNCパス
        private readonly string _shareUncPath;


        /// <summary>
        /// 接続先共有サーバのアドレス(UNC名)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Address
        {
            get { return this._address; }
        }


        /// <summary>
        /// 接続先の共有フォルダ名
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Share
        {
            get { return this._share; }
        }


        /// <summary>
        /// 接続ユーザー名
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string UserName
        {
            get { return this._userName; }
        }


        ///' <summary>
        ///' 接続ユーザーのパスワード
        ///' </summary>
        ///' <value></value>
        ///' <returns></returns>
        ///' <remarks></remarks>
        //Public ReadOnly Property Password() As String
        //    Get
        //        Return Me._password
        //    End Get
        //End Property


        /// <summary>
        /// 接続先共有のUNCパス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RootPath
        {
            get { return this._shareUncPath; }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="address"></param>
        /// <param name="share"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <remarks></remarks>

        public Smb(string address, string share, string userName, string password)
        {
            this._address = address;
            this._share = share;
            this._userName = userName;
            this._password = password;

            this._shareUncPath = string.Format("\\\\{0}\\{1}", address, share);
            if ((this._shareUncPath.Substring(this._shareUncPath.Length - 1, 1) == "\\"))
            {
                this._shareUncPath = this._shareUncPath.Substring(0, this._shareUncPath.Length - 1);
            }

            Netresource res = new Netresource();
            res.DwScope = 0;
            res.DwType = 1;
            res.DwDisplayType = 0;
            res.DwUsage = 0;
            res.LpLocalName = "";
            res.LpRemoteName = this._shareUncPath;

            //接続済みの場合があるので、切断試行する。
            try
            {
                WNetCancelConnection(this._shareUncPath, true);
            }
            catch (Exception)
            {
            }

            //接続する。
            try
            {
                WNetAddConnection2(ref res, password, userName, 0);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Net.Smb.New: 共有フォルダへの接続に失敗しました。：" + ex.Message);
                throw new Exception("共有フォルダへの接続に失敗しました。：" + ex.Message);
            }

        }


        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2A", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern int WNetAddConnection2(ref Netresource netResource, string password, string username, int flag);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnectionA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern bool WNetCancelConnection(string lpName, bool fForce = false);


        /// <summary>
        /// 共有フォルダ上のパス文字列先頭に、"\"を付与する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string Format(string path)
        {
            return ("\\" + path).Replace("\\\\", "\\");
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリ構造を一階層分のみ取得する。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Xb.File.NodeList GetList(string directory, string extensions = "")
        {
            return Xb.File.NodeList.GetNodeList(directory, extensions);
        }


        /// <summary>
        /// 接続先の指定ディレクトリ以下全ファイル／ディレクトリ構造を取得する。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public Xb.File.NodeList GetListRecursive(string directory, string extensions = "")
        {
            return Xb.File.NodeList.GetNodeListRecursive(directory, extensions);
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリが存在するか否か検証する。
        /// </summary>
        /// <param name="serverPath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Exists(string serverPath)
        {
            if (System.IO.File.Exists(this._shareUncPath + this.Format(serverPath)))
                return true;
            if (System.IO.Directory.Exists(this._shareUncPath + this.Format(serverPath)))
                return true;

            return false;
        }


        /// <summary>
        /// ローカルフォルダへ、接続先フォルダのファイルをダウンロード
        /// </summary>
        /// <param name="serverFileName"></param>
        /// <param name="localDirectory"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Download(string serverFileName, string localDirectory)
        {
            string fileName = null;
            string remoteFullPath = null;
            string localFullPath = null;

            remoteFullPath = this._shareUncPath + this.Format(serverFileName);
            fileName = System.IO.Path.GetFileName(remoteFullPath);

            //ディレクトリ渡し値を検証する。
            if (!System.IO.Directory.Exists(localDirectory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(localDirectory);

                    //指定のローカルディレクトリが存在せず、生成も出来ないとき、異常終了。
                    if (!System.IO.Directory.Exists(localDirectory))
                    {
                        return false;
                    }

                }
                catch (Exception)
                {
                    return false;
                }
            }
            localFullPath = this.Format(localDirectory + "\\" + fileName);

            try
            {
                System.IO.File.Copy(this._shareUncPath + this.Format(serverFileName), localFullPath);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Net.Smb.Download: ファイルのコピーに失敗しました。：" + ex.Message);
                throw new Exception("ファイルのコピーに失敗しました。：" + ex.Message);
            }

            return true;
        }


        /// <summary>
        /// 接続先へ、ローカルのファイルをアップロードする。
        /// </summary>
        /// <param name="localFileName"></param>
        /// <param name="serverDirectory"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Upload(string localFileName, string serverDirectory)
        {
            string fileName = null;
            string remoteFullDirectory = null;

            fileName = System.IO.Path.GetFileName(localFileName);
            remoteFullDirectory = this._shareUncPath + this.Format(serverDirectory);

            if (!System.IO.Directory.Exists(remoteFullDirectory))
                return false;

            remoteFullDirectory = this._shareUncPath + this.Format(serverDirectory + "\\" + fileName);

            try
            {
                System.IO.File.Copy(localFileName, remoteFullDirectory);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Net.Smb.Upload: ファイルのコピーに失敗しました。：" + ex.Message);
                throw new Exception("ファイルのコピーに失敗しました。：" + ex.Message);
            }

            return true;
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリを削除する。
        /// </summary>
        /// <param name="serverPath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Delete(string serverPath)
        {
            try
            {
                if (System.IO.File.Exists(this._shareUncPath + this.Format(serverPath)))
                {
                    System.IO.File.Delete(this._shareUncPath + this.Format(serverPath));
                    return true;
                }
                else if (System.IO.Directory.Exists(this._shareUncPath + this.Format(serverPath)))
                {
                    System.IO.Directory.Delete(this._shareUncPath + this.Format(serverPath));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Net.Smb.Delete: ファイル／ディレクトリの削除に失敗しました。：" + ex.Message);
                throw new Exception("ファイル／ディレクトリの削除に失敗しました。：" + ex.Message);
            }

            return false;
        }


        /// <summary>
        /// 接続先にフォルダを作る。
        /// </summary>
        /// <param name="serverPath"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool MakeDirectory(string serverPath)
        {
            try
            {
                System.IO.Directory.CreateDirectory(this._shareUncPath + this.Format(serverPath));
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Net.Smb.MakeDirectory: フォルダの作成に失敗しました。：" + ex.Message);
                throw new Exception("フォルダの作成に失敗しました。：" + ex.Message);
            }

            return true;
        }


        // 重複する呼び出しを検出するには
        private bool _disposedValue;

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }
            }
            this._disposedValue = true;
        }

        #region "IDisposable Support"
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
