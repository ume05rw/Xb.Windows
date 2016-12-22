using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Xb.Net
{

    /// <summary>
    /// FTP接続管理クラス
    /// </summary>
    /// <remarks>
    /// ※注意※ 
    /// 本クラスは下記の制限がある。FtpWebRequestの仕様上の制限による。
    /// 　１．サーバ上ファイルのパーミッション変更が出来ない。
    /// 　２．シンボリックリンクのファイル／フォルダが検出出来ない。
    /// 　３．パッシブモードは使用出来ない(可能だが実装しない)。
    /// 　４．ファイルの更新時刻を取得出来ない(可能、いずれ実装するか？)。
    /// 
    /// 特にファイルのアップロードでは問題が発生しやすいと思われる。
    /// Webサーバがファイルを取得出来ないなど。
    /// 
    /// なお、WindowsスタイルのFTPサーバでの動作は未検証。
    /// 
    /// </remarks>
    public class Ftp : IDisposable
    {
        /// <summary>
        /// FTPリクエストのクエリタイプ
        /// </summary>
        /// <remarks></remarks>
        private enum RequestType
        {
            Connect,
            List,
            ListDetail,
            GetTimestamp,
            Download,
            Upload,
            DeleteFile,
            DeleteDirectory,
            MakeDirectory,
            Close
        }

        private const char Separator = '/';
        private readonly string _address;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _serverDirectory;

        private NetworkCredential _credential;

        /// <summary>
        /// 接続先FTPサーバのアドレス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Address
        {
            get { return this._address; }
        }


        /// <summary>
        /// 接続先ポート
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Port
        {
            get { return this._port; }
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
        /// 接続先サーバのURLパス
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RootPath
        {
            get { return this._serverDirectory; }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>

        public Ftp(string address, string userName = "anonymous", string password = null, int port = 21)
        {
            this._address = address;
            this._port = port;
            this._userName = userName;
            this._password = password;
            this._serverDirectory = address;

            //FTPサーバ接続用の資格情報オブジェクトを生成する。
            this._credential = new NetworkCredential(userName, password);

            try
            {
                var response = this.GetResponse(RequestType.Connect);
                //using (FtpWebResponse response = this.GetResponse(RequestType.Connect))
                //{
                //    Xb.App.Out(response.StatusCode & " / " & response.StatusDescription)
                //}
            }
            catch (Exception ex)
            {
                //接続に失敗した
                Xb.Util.Out("Net.Frp.New: " + ex.Message);
                throw;
            }
        }


        /// <summary>
        /// パス文字列を結合する。
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string BuildPath(string path1, string path2)
        {
            if (path1 == null)
                path1 = "";

            if (path2 == null)
                path2 = "";

            path1 = path1.TrimEnd(Xb.Net.Ftp.Separator); //Xb.Str.RTrim(path1, Xb.Net.Ftp.Separator);
            path2 = path2.TrimStart(Xb.Net.Ftp.Separator); //Xb.Str.LTrim(path2, Xb.Net.Ftp.Separator);

            //If (Type.String.Slice(path1, -1) = Net.Ftp.Separator) Then
            //    path1 = Type.String.Right(path1, -1)
            //End If
            //If (Type.String.Slice(path2, 1) = Net.Ftp.Separator) Then
            //    path2 = path2.Substring(1)
            //End If

            //string dbsSep = Net.Ftp.Separator + Net.Ftp.Separator;

            //If (path1 = "" And path2 = "") Then Return ""
            return (path1 + Separator + path2);
            //.Replace(dbsSep, Net.Ftp.Separator)
            //Return (path1 & path2).Replace(dbsSep, Net.Ftp.Separator)
        }


        /// <summary>
        /// FTPリクエストを取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private FtpWebRequest GetRequest(RequestType type, string path = null)
        {
            if (path == null)
                path = "";

            string url = this.BuildPath(this._serverDirectory, path);

            if (url.ToLower().IndexOf("ftp:") == -1)
            {
                url = "ftp://" + url.Trim('/');
            }

            url = System.Uri.EscapeUriString(url);
            FtpWebRequest result = (FtpWebRequest)WebRequest.Create(url);

            result.Credentials = this._credential;
            result.UsePassive = false;
            result.UseBinary = true;
            result.Proxy = null;
            result.KeepAlive = (type != RequestType.Close);
            //切断するとき以外は常にKeepAliveしておく。

            switch (type)
            {
                case RequestType.Connect:
                    result.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
                    break;
                case RequestType.List:
                    result.Method = WebRequestMethods.Ftp.ListDirectory;
                    break;
                case RequestType.ListDetail:
                    result.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    break;
                case RequestType.GetTimestamp:
                    result.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    break;
                case RequestType.Download:
                    result.Method = WebRequestMethods.Ftp.DownloadFile;
                    break;
                case RequestType.Upload:
                    result.Method = WebRequestMethods.Ftp.UploadFile;
                    break;
                case RequestType.DeleteFile:
                    result.Method = WebRequestMethods.Ftp.DeleteFile;
                    break;
                case RequestType.DeleteDirectory:
                    result.Method = WebRequestMethods.Ftp.RemoveDirectory;
                    break;
                case RequestType.MakeDirectory:
                    result.Method = WebRequestMethods.Ftp.MakeDirectory;
                    break;
                case RequestType.Close:
                    result.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return result;
        }


        /// <summary>
        /// FTPリクエストを実行し、応答オブジェクトを取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>
        /// 生成時の型定義・設定値セットが冗長なので、関数に切り出した。
        /// </remarks>
        private FtpWebResponse GetResponse(RequestType type, string path = null)
        {
            FtpWebRequest request = this.GetRequest(type, path);
            return (FtpWebResponse) request.GetResponse();
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリ構造を一階層分のみ取得する。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns>パターンに合致したファイルと、全てのディレクトリのパスリスト</returns>
        /// <remarks></remarks>
        public File.NodeList GetList(string directory = null, string extensions = "")
        {
            //App.Out("GetList - directory: " & directory)

            //If (Not IO.Directory.Exists(directory)) Then Return Nothing
            if ((extensions == null))
                extensions = "";

            File.NodeList result = new File.NodeList(directory, DateTime.MinValue, true);
            string[] names = null;
            string[] details = null;
            string[] filter = null;
            string filterString = null;
            DateTime[] updateDate = null;
            Regex regex = null;
            FtpWebResponse response = null;

            //ファイル絞り込み用に、拡張子検出正規表現オブジェクトを生成する。
            filter = extensions.Split(Convert.ToChar(","));
            filterString = "";
            if ((!string.IsNullOrEmpty(extensions)))
            {
                for (int i = 0; i <= filter.Length - 1; i++)
                {
                    filterString += filter[i].ToLower() + "|";
                    filterString += filter[i].ToUpper() + "|";
                }
                filterString = filterString.Substring(0, filterString.Length - 1);
            }
            regex = new Regex(".+\\.(" + filterString + ")$", RegexOptions.IgnoreCase);

            //クエリを2回に分ける。リンクの表記が名前のみクエリと詳細込みクエリとで戻り値が異なるため。
            //クエリ1回目、予めファイル名のみのリストを作っておく。
            try
            {
                response = this.GetResponse(RequestType.List, directory);
            }
            catch (Exception)
            {
                //パーミッションエラー等で例外キャッチ
            }


            if (response != null)
            {
                using (FtpWebResponse res = response)
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream()))
                    {
                        names = Xb.Str.GetMultiLine(reader.ReadToEnd());
                        updateDate = new DateTime[names.Length];
                        //名前リストを加工。
                        //取得出来る名前が、パスに関わらず"直下のディレクトリ名"/"ファイル・ディレクトリ名"の文字列なので。
                        for (int i = 0; i <= names.Length - 1; i++)
                        {
                            names[i] = this.BuildPath(directory, Xb.Str.SliceSentence(names[i], -1, Xb.Net.Ftp.Separator.ToString()));
                            try
                            {
                                using (FtpWebResponse res2 = this.GetResponse(RequestType.GetTimestamp, names[i]))
                                {
                                    updateDate[i] = res2.LastModified;
                                    //App.Out("LastModified {0}  {1}: {2}", res2.LastModified, res2.StatusCode, res2.StatusDescription)
                                }
                            }
                            catch (Exception)
                            {
                                updateDate[i] = DateTime.MinValue;
                                //App.Out("UpdateDate Fail.")
                            }
                        }
                    }
                }

                //クエリ2回目、詳細情報付きファイルリストを読み込む。
                using (FtpWebResponse res = this.GetResponse(RequestType.ListDetail, directory))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream()))
                    {
                        details = Xb.Str.GetMultiLine(reader.ReadToEnd());
                        foreach (File.Node node in this.ParseLines(names, updateDate, details))
                        {
                            if (node.Type == File.Node.NodeType.File)
                            {
                                if (string.IsNullOrEmpty(extensions) | regex.Match(node.Path).Success)
                                {
                                    result.AddAsRootChild(node);
                                }
                            }
                            else
                            {
                                result.AddAsRootChild(node);
                            }
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 名称配列、詳細情報配列からファイル／ディレクトリ情報を解析・取得する。
        /// </summary>
        /// <param name="names"></param>
        /// <param name="updateDate"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        /// <remarks>
        /// 以下を参考にした。
        /// Sample code for parsing FtpwebRequest response for ListDirectoryDetails
        /// http://blogs.msdn.com/b/adarshk/archive/2004/09/15/230177.aspx
        /// </remarks>
        private List<File.Node> ParseLines(string[] names, DateTime[] updateDate, string[] details)
        {
            string tmp = null;
            File.Node.Separator separator = File.Node.Separator.Slash;
            int index = 0;
            List<File.Node> nodes = new List<File.Node>();

            //パスの区切り文字を判定・取得する。
            foreach (string line in details)
            {
                if (line.Length > 10 && Regex.IsMatch(line.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    //Unixスタイル
                    separator = File.Node.Separator.Slash;
                }
                else if (line.Length > 8 && Regex.IsMatch(line.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    //Windowsスタイル
                    separator = File.Node.Separator.Backslash;
                }
            }

            //行ごとのファイル情報を解析・取得する。
            foreach (string line in details)
            {
                switch (separator)
                {
                    case File.Node.Separator.Backslash:
                        //Windowsスタイルのリスト表記をパース 
                        //例:02-03-04  07:46PM       <DIR>          Append

                        //名称を取得:ディレクトリ識別子の有無で分岐
                        tmp = line.Trim().Substring(0, 16).Trim();
                        //ディレクトリのときの名称取得
                        if (tmp.Substring(0, 5) == "<DIR>")
                        {
                            nodes.Add(new File.Node(names[index], updateDate[index], true));
                            //ファイルのときの名称取得
                        }
                        else
                        {
                            nodes.Add(new File.Node(names[index], updateDate[index], false));
                        }
                        break;
                    default:
                        //含む:File.Node.Separator.Slash
                        //Unixスタイルのリスト表記をパース 
                        //例:dr-xr-xr-x   1 owner    group               0 Nov 25  2002 bussys

                        //名称を取得:要素種別識別子の値で分岐
                        tmp = line.Trim();
                        //シンボリックリンクのとき
                        if ((tmp.StartsWith("l")))
                        {
                            //ファイル or ディレクトリを判別できない上、FTPサーバ上でchrootしていると参照出来ないため、
                            //対応しないこととする。
                            //ディレクトリのとき
                        }
                        else if (tmp.StartsWith("d"))
                        {
                            nodes.Add(new File.Node(names[index], updateDate[index], true));
                            //ファイルのとき
                        }
                        else
                        {
                            nodes.Add(new File.Node(names[index], updateDate[index], false));
                        }
                        break;
                }

                index += 1;
            }

            return nodes;
        }


        /// <summary>
        /// 接続先の指定ディレクトリ以下全ファイル／ディレクトリ構造を取得する。
        /// ※注意※基本的に使用しないこと。サーバに大きな負荷が掛かる。
        /// </summary>
        /// <param name="directory">走査の起点となるディレクトリパス。</param>
        /// <param name="extensions">ファイル名の拡張子候補(カンマ区切り文字列)</param>
        /// <returns>パターンに合致したファイルと、全てのディレクトリのパスリスト</returns>
        /// <remarks></remarks>
        public File.NodeList GetListRecursive(string directory, string extensions = "")
        {
            File.NodeList result = default(File.NodeList);
            File.NodeList tmp = default(File.NodeList);
            Dictionary<string, File.Node> backup = new Dictionary<string, File.Node>();

            result = this.GetList(directory, extensions);

            foreach (string key in result.RootNode.Children.Keys)
            {
                if (result.RootNode.Children[key].Type == File.Node.NodeType.Directory)
                {
                    //App.Out("recursive Target: " & result.RootNode.Children.Item(key).Path)
                    //要素がディレクトリのとき、そのディレクトリ以下の要素を再帰取得する。
                    tmp = GetListRecursive(result.RootNode.Children[key].Path, extensions);

                    //参照中に要素上書きができないため、Nodeオブジェクトの書き換え候補をリストに保持しておく。
                    backup.Add(key, tmp.RootNode);

                    //戻り値に、直下でない要素を追加する。
                    foreach (File.Node childNode in tmp.Nodes)
                    {
                        result.Add(childNode);
                    }
                }
            }
            //ディレクトリ要素を、再帰処理完了後の要素と差し替える。
            foreach (string key in backup.Keys)
            {
                result.RootNode.Children[key] = backup[key];
            }

            GC.Collect();

            return result;
        }


        /// <summary>
        /// 接続先のファイル／ディレクトリが存在するか否か検証する。
        /// </summary>
        /// <param name="serverPath">サーバ上のパス。ディレクトリ検証時には、末尾に"/"を付けてはならない。</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Exists(string serverPath)
        {
            string fileName = null;
            string baseDirectory = null;
            File.Node node = default(File.Node);

            fileName = Xb.Str.RightSentence(serverPath, 1);
            baseDirectory = Xb.Str.RightSentence(serverPath, -1);

            node = GetList(baseDirectory).RootNode;
            foreach (File.Node child in node.Children.Values)
            {
                if (node.Name == fileName)
                    return true;
            }
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
            //渡し値フォルダが存在しないとき、例外
            if (!System.IO.Directory.Exists(localDirectory))
                throw new DirectoryNotFoundException("Not Found: " + localDirectory);

            var fullPath = System.IO.Path.Combine(localDirectory, serverFileName);

            //既存ファイルがあるとき、削除を試みる
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Xb.Util.Out(ex);
                    throw ex;
                }
            }

            try
            {
                //サーバからFileStreamを取得し、ローカルに書き出す。
                using (var response = this.GetResponse(RequestType.Download, serverFileName))
                {
                    using (var contentStream = response.GetResponseStream())
                    {
                        using (var fileStream = new System.IO.FileStream(fullPath, 
                                                                         System.IO.FileMode.Create, 
                                                                         System.IO.FileAccess.Write))
                        {
                            byte[] buffer = new byte[1024];
                            while (true)
                            {
                                var readSize = contentStream.Read(buffer, 0, buffer.Length);
                                if (readSize == 0)
                                    break;

                                fileStream.Write(buffer, 0, readSize);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Xb.Util.Out(ex);
                return false;
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
        public bool Upload(string serverDirectory, string localFileName)
        {
            string serverFullPath = serverDirectory.Trim(Convert.ToChar("\\")) + "\\";
            FtpWebRequest request = null;
            FtpWebResponse response = null;

            serverFullPath += Xb.Str.RightSentence(localFileName, 1, "\\");

            try
            {
                //送付対象ファイルが存在しないとき、異常終了
                if (!System.IO.File.Exists(localFileName))
                    return false;

                request = this.GetRequest(RequestType.Upload, serverFullPath);

                using (System.IO.Stream reqStream = request.GetRequestStream())
                {
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(localFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        byte[] buffer = new byte[1024];
                        while (true)
                        {
                            int readSize = fileStream.Read(buffer, 0, buffer.Length);
                            if (readSize == 0)
                                break; // TODO: might not be correct. Was : Exit While
                            reqStream.Write(buffer, 0, readSize);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                response = (FtpWebResponse) request.GetResponse();
                //App.Out(response.StatusCode & " / " & response.StatusDescription)
                response.Close();
            }
            catch (Exception)
            {
                return false;
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
            string name = Xb.Str.SliceSentence(serverPath, -1);
            string parent = Xb.Str.SliceReverseSentence(serverPath, -1);
            File.Node node = default(File.Node);
            bool isDirectory = false;
            bool isFound = false;

            //渡し値のパスを検証し、ファイル／ディレクトリの判別を行う。
            try
            {
                node = this.GetList(parent).RootNode;
                isFound = false;
                foreach (File.Node child in node.Children.Values)
                {
                    if ((name == child.Name))
                    {
                        isDirectory = (child.Type == File.Node.NodeType.Directory);
                        isFound = true;
                        break;
                    }
                }

                //指定のファイル／ディレクトリが見つからないとき、異常終了する。
                if (!isFound)
                    return false;
            }
            catch (Exception)
            {
                //ファイルリスト取得に失敗したとき異常終了する。
                return false;
            }

            //削除を実行する。
            try
            {
                if (isDirectory)
                {
                    using (FtpWebResponse response = this.GetResponse(RequestType.DeleteDirectory, serverPath))
                    {
                        //App.Out(res.StatusCode & " / " & res.StatusDescription)
                    }
                }
                else
                {
                    using (FtpWebResponse response = this.GetResponse(RequestType.DeleteFile, serverPath))
                    {
                        //App.Out(res.StatusCode & " / " & res.StatusDescription)
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
                using (FtpWebResponse response = this.GetResponse(RequestType.MakeDirectory, serverPath))
                {
                    //App.Out(res.StatusCode & " / " & res.StatusDescription)
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// FTP接続を閉じ、本インスタンスを破棄する。
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
                    try
                    {
                        using (FtpWebResponse response = this.GetResponse(RequestType.Close))
                        {
                        }
                    }
                    catch (Exception)
                    {
                    }
                    this._credential = null;
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
