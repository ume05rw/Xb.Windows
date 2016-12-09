using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;


using System.Text.RegularExpressions;

namespace Xb.File
{

    /// <summary>
    /// ファイル操作関連関数群
    /// </summary>
    /// <remarks></remarks>
    public class Util
    {
        /// <summary>
        /// 渡し値ファイル内容をバイト配列で取得する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static byte[] GetBytes(string fileName)
        {
            //ファイル存在チェック
            if (!System.IO.File.Exists(fileName))
            {
                Xb.Util.Out("Xb.File.GetBytes: 不正なファイルパスです。");
                throw new ArgumentException("不正なファイルパスです。");
            }

            var fileStream = new System.IO.FileStream(fileName, 
                                                      System.IO.FileMode.Open, 
                                                      System.IO.FileAccess.Read);
            byte[] bytes = null;

            //バイト配列のサイズを、ファイルサイズに合わせて生成する。
            //TODO: .NetFW2.0の制限か、配列最大用素数がInt32.MaxValueになっているようだ。
            //TODO: 1GB 以上のファイルを取得出来ないため、対策を考える。
            bytes = new byte[Convert.ToInt32(fileStream.Length - 1) + 1];

            //byte配列に読み込む
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();

            return bytes;
        }


        /// <summary>
        /// 渡し値ファイルの文字コードを判定する
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Text.Encoding GetEncode(string fileName)
        {
            return Xb.Str.GetEncode(Xb.File.Util.GetBytes(fileName));
        }


        /// <summary>
        /// 渡し値ファイル内容文字列を取得する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetString(string fileName)
        {
            var bytes = Xb.File.Util.GetBytes(fileName);
            return Xb.Str.GetEncode(bytes).GetString(bytes);
        }


        /// <summary>
        /// 渡し値バイト配列をファイルに書き込む。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks>
        /// 指定ファイル名の既存ファイルが存在する場合、Falseを返す。
        /// </remarks>
        public static bool WriteBytes(string fileName, byte[] bytes)
        {
            if (fileName == null || bytes == null)
                return false;

            try
            {
                fileName = Xb.App.Path.GetAbsPath(fileName);
            }
            catch (Exception)
            {
                return false;
            }

            if (System.IO.File.Exists(fileName))
                return false;

            try
            {
                using (var fileStream = new System.IO.FileStream(fileName, 
                                                                 System.IO.FileMode.Create, 
                                                                 System.IO.FileAccess.Write))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }


        /// <summary>
        /// 渡し値バイト配列を文字列にキャストしてファイルに書き込む。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="encode"></param>
        /// <param name="lineFeed"></param>
        /// <returns></returns>
        /// <remarks>
        /// 指定ファイル名の既存ファイルが存在する場合、Falseを返す。
        /// </remarks>
        public static bool WriteString(string fileName, 
                                       byte[] bytes, System.Text.Encoding encode = null, 
                                       Xb.Str.LinefeedType lineFeed = Xb.Str.LinefeedType.CrLf)
        {

            if (fileName == null || bytes == null)
                return false;

            try
            {
                fileName = Xb.App.Path.GetAbsPath(fileName);
            }
            catch (Exception)
            {
                return false;
            }

            if (System.IO.File.Exists(fileName))
                return false;

            if (encode == null)
            {
                encode = System.Text.Encoding.GetEncoding("Shift_JIS");
            }
            else if (encode.Equals(System.Text.Encoding.UTF8))
            {
                //UTF8指定のとき、BOM無しになるようEncodingオブジェクトを生成する。
                //http://dobon.net/vb/dotnet/file/writeutf8withoutbom.html
                encode = new System.Text.UTF8Encoding(false);
            }

            var lineFeedChar = Xb.Str.GetLinefeed(lineFeed);
            var text = Xb.Str.GetString(bytes);

            //改行文字を指定のものに書き変える。
            //Replaceメソッドを連結するのはNG。左から順に実行されるとは限らない。
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");
            text = text.Replace("\n", lineFeedChar);

            //ファイルを生成し、書き込む。
            try
            {
                var streamWriter = new System.IO.StreamWriter(fileName, false, encode);
                streamWriter.Write(text);
                streamWriter.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 渡し値文字列をファイルに書き込む。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        /// <param name="encode"></param>
        /// <param name="lineFeed"></param>
        /// <returns></returns>
        /// <remarks>
        /// 指定ファイル名の既存ファイルが存在する場合、Falseを返す。
        /// </remarks>
        public static bool WriteString(string fileName, 
                                       string text, 
                                       System.Text.Encoding encode = null, 
                                       Xb.Str.LinefeedType lineFeed = Xb.Str.LinefeedType.CrLf)
        {
            if (fileName == null || text == null)
                return false;

            try
            {
                fileName = Xb.App.Path.GetAbsPath(fileName);
            }
            catch (Exception)
            {
                return false;
            }

            if (System.IO.File.Exists(fileName))
                return false;

            if (encode == null)
            {
                encode = System.Text.Encoding.GetEncoding("Shift_JIS");
            }
            else if (encode.Equals(System.Text.Encoding.UTF8))
            {
                //UTF8指定のとき、BOM無しになるようEncodingオブジェクトを生成する。
                //http://dobon.net/vb/dotnet/file/writeutf8withoutbom.html
                encode = new System.Text.UTF8Encoding(false);
            }

            var lineFeedChar = Xb.Str.GetLinefeed(lineFeed);

            //改行文字を指定のものに書き変える。
            //Replaceメソッドを連結するのはNG。左から順に実行されるとは限らない。
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");
            text = text.Replace("\n", lineFeedChar);

            //ファイルを生成し、書き込む。
            try
            {
                var streamWriter = new System.IO.StreamWriter(fileName, false, encode);
                streamWriter.Write(text);
                streamWriter.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// 指定パスのファイルが存在するとき、リネームを試みる。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool MoveIfExists(string fileName, 
                                        string directory = null, 
                                        string prefix = null, 
                                        string postfix = null)
        {
            try
            {
                var baseDirectory = System.IO.Path.GetDirectoryName(fileName);
                string nameBase = null;
                string backupFileName = null;
                string backupFullPath = null;
                System.IO.FileInfo info = null;

                //渡し値パスのディレクトリ存在確認。
                if (!System.IO.Directory.Exists(baseDirectory))
                    return false;

                //指定パスのファイルが存在しないとき、何もせずに正常終了する。
                if (!System.IO.File.Exists(fileName))
                    return true;

                //渡し値バックアップ先ディレクトリのパス検証
                if (directory == null)
                    directory = baseDirectory;

                if (!System.IO.Directory.Exists(directory))
                {
                    //渡し値ファイル名と同じパス上のディレクトリであることを仮定してディレクトリ存在チェック。
                    directory = Xb.App.Path.GetAbsPath(directory, baseDirectory);
                    if (!System.IO.Directory.Exists(directory))
                    {
                        return false;
                    }
                }

                //退避ファイル名を生成する。
                info = new System.IO.FileInfo(fileName);

                //渡し値の退避ファイル名プレフィクスが未指定なら空文字を、指定されているときは末尾に"_"をセットする。
                if (prefix == null)
                {
                    prefix = "";
                }
                else
                {
                    prefix = prefix.Trim(Convert.ToChar("_")) + "_";
                }

                //渡し値の退避ファイル名ポストフィクスが未指定のとき、現在時刻を使用する。
                if (postfix == null)
                {
                    postfix = DateTime.Now.ToString("_yyyyMMdd_HHmmssfff");
                }
                else
                {
                    postfix = "_" + postfix.Trim(Convert.ToChar("_"));
                }

                nameBase = info.Name;
                if (!string.IsNullOrEmpty(info.Extension))
                {
                    //拡張子が存在するとき
                    nameBase = Xb.Str.SliceSentence(info.Name, 1, ".");
                    //info.Name.Substring(0, info.Name.IndexOf("."))
                }

                //退避ファイル名の各要素を結合。
                backupFileName =
                    $"{prefix}{nameBase}{postfix}{(string.IsNullOrEmpty(info.Extension) ? "" : info.Extension).ToString()}";

                //退避先ディレクトリと結合した絶対パスを取得
                backupFullPath = Xb.App.Path.GetAbsPath(backupFileName, directory);

                //バックアップファイル名が既存ファイルと被ったとき、異常終了
                if (System.IO.File.Exists(backupFullPath) || System.IO.Directory.Exists(backupFullPath))
                    return false;

                //既存ファイルの名前変更を試みる。
                info.MoveTo(backupFullPath);
            }
            catch (Exception)
            {
                //パス検証時の例外。パス文字列に不正文字を検出したときなど。
                return false;
            }

            return true;
        }
    }
}
