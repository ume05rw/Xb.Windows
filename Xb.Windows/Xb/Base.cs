using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Text;

namespace Xb
{
    /// <summary>
    /// 各種ユーティリティクラスのための名前空間定義クラス
    /// </summary>
    /// <remarks></remarks>
    public class Base : Xb.Util
    {
        /// <summary>
        /// アプリケーション実行ファイルが存在するパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>ClsApp.Path.Appと同じもの</remarks>
        public static string AppPath
        {
            get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
        }


        /// <summary>
        /// カレントディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>ClsApp.Path.Currentと同じもの</remarks>
        public static string CurrentPath
        {
            get { return System.IO.Directory.GetCurrentDirectory(); }
        }


        //System.Windows.Forms依存のため、削除。
        ///' <summary>
        ///' アプリケーションデータ用パスを返す。
        ///' </summary>
        ///' <value></value>
        ///' <returns></returns>
        ///' <remarks></remarks>
        //Public Shared ReadOnly Property AppDataPath() As String
        //    Get
        //        Return System.Windows.Forms.Application.UserAppDataPath
        //    End Get
        //End Property


        /// <summary>
        /// 動作環境が32bitか64bitかを判定して返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int OsBit
        {
            get { return (IntPtr.Size*8); }
        }


        /// <summary>
        /// 動作環境が64bitか否か
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool Is64Bit
        {
            get { return (IntPtr.Size == 8); }
        }


        /// <summary>
        /// 動作環境が32bitか否か
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool Is32Bit
        {
            get { return (IntPtr.Size == 4); }
        }


        /// <summary>
        /// コマンドライン起動時の引数を取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 起動時のコマンドライン引数を取得する
        /// http://dobon.net/vb/dotnet/programing/commandline.html
        /// </remarks>
        public static Dictionary<string, string> GetCommandlineArgsToDictionary()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            string[] sysArgs = Xb.Base.GetCommandlineArgsToArray();

            for (int i = 0; i <= sysArgs.Length - 1; i++)
            {
                string arg = sysArgs[i];

                //カレント引数文字列が２文字以上
                //カレント引数文字列の先頭文字が "/" or "-"
                if (arg.Length >= 2 
                    && (arg.Substring(0, 1) == "/" 
                        || arg.Substring(0, 1) == "-") 
                    && (sysArgs.Length - 1) >= (i + 1))
                {
                    string[] splitted = arg.Split(' ');
                    if (splitted.Length >= 2)
                    {
                        args.Add(splitted[0], string.Join(" ", splitted).Substring(splitted[0].Length));
                    }
                    else
                    {
                        args.Add(arg, "");
                    }
                }
                else
                {
                    args.Add(arg, "");
                }
            }
            return args;
        }


        /// <summary>
        /// コマンドライン起動時の引数を取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 起動時のコマンドライン引数を取得する
        /// http://dobon.net/vb/dotnet/programing/commandline.html
        /// </remarks>
        public static string[] GetCommandlineArgsToArray()
        {
            List<string> args = new List<string>();
            string[] sysArgs = System.Environment.GetCommandLineArgs();

            for (int i = 0; i <= sysArgs.Length - 1; i++)
            {
                if (i == 0)
                    continue;

                string arg = sysArgs[i];

                //カレント引数文字列が２文字以上
                //カレント引数文字列の先頭文字が "/" or "-"
                //カレント引数の次にも、引数が存在する。
                //カレント次引数の先頭文字列が、"/", "-" でない

                if (arg.Length >= 2 
                    && (arg.Substring(0, 1) == "/" 
                        || arg.Substring(0, 1) == "-") 
                    && (sysArgs.Length - 1) >= (i + 1) 
                        && sysArgs[i + 1].Substring(0, 1) != "/" 
                        && sysArgs[i + 1].Substring(0, 1) != "-")
                {
                    args.Add(arg + " " + sysArgs[i + 1]);
                    i += 1;
                }
                else
                {
                    args.Add(arg);
                }
            }

            return args.ToArray();

            //順序保証用ロジック
            //Dim result As String()
            //ReDim result(args.Count - 1)

            //For i As Integer = 0 To args.Count - 1
            //    result(i) = args(i)
            //Next

            //Return result
        }

        /// <summary>
        /// 遅延処理用デリゲート
        /// </summary>
        /// <param name="args"></param>
        public delegate void TimerDelegate(params object[] args);


        /// <summary>
        /// 引数１つのデリゲートを遅延実行する。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <param name="args"></param>
        /// <remarks></remarks>
        public static void SetTimeout(Xb.Base.TimerDelegate callback,
                                      int delay = 200,
                                      params object[] args)
        {
            //Xb.Base.Out("SetTimeout")
            if(args == null)
                args = new object[]{};

            Xb.Base._delayedProcStack.Add(new DelayedProcedure(callback, args, delay));
            Xb.Base.Repeater(null);
        }

        /// <summary>
        /// 引数無しのデリゲートを遅延実行する。
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="delay"></param>
        /// <remarks>
        /// 使用例：
        /// Xb.Base.SetTimeout(New Xb.Base.TimerDelegate0(AddressOf VoidMethod))
        /// </remarks>
        public static void SetTimeout(Xb.Base.TimerDelegate callback, int delay = 200)
        {
            //Xb.Base.Out("SetTimeout_0")
            Xb.Base._delayedProcStack.Add(new DelayedProcedure(callback, new object[]{}, delay));
            Xb.Base.Repeater(null);
        }


        /// <summary>
        /// 遅延処理デリゲート・引数保持クラス
        /// </summary>
        /// <remarks></remarks>
        private class DelayedProcedure
        {
            public readonly object Callback;
            public readonly int Delay;
            public readonly object[] Args;
            public readonly DateTime StackTime;

            public bool IsReady
            {
                get { return (this.StackTime.AddMilliseconds(this.Delay) < DateTime.Now); }
            }


            public DelayedProcedure(object callback, object[] args, int delay)
            {
                if (callback == null 
                    || delay < 0)
                {
                    Xb.Util.Out("Xb.Base.DelayedProcedure: 渡し値が不正か、または検出出来ません。");
                    throw new ArgumentException("Xb.Base.DelayedProcedure: 渡し値が不正か、または検出出来ません。");
                }

                this.Callback = callback;
                this.Args = args;
                this.Delay = delay;
                this.StackTime = DateTime.Now;
            }
        }

        private static List<DelayedProcedure> _delayedProcStack = new List<DelayedProcedure>();
        private static System.Threading.Timer _timerProcStack;

        /// <summary>
        /// 遅延処理スタックを監視、起動する
        /// </summary>
        /// <param name="dummy"></param>
        /// <remarks>
        /// Threading.TimerはThreadPoolの容量都合で破棄され、コールバックが
        /// 実行されないことがある。
        /// 対策として、一つだけ生成したタイマーインスタンスを全遅延処理スタックの
        /// 監視に使い、リソース消費を抑えるようにする。
        /// 
        /// ※注意※ Windows.Forms.Timerは使用できない。
        /// このタイマはWindows OSのタイマ・メッセージ（WM_TIMERメッセージ）をベースにしており、
        /// タイマを利用するにはメッセージ・ループの実行が必要である
        /// （一般的なWindowsフォーム・アプリケーションはApplication.Runメソッドの呼び出しを
        /// 含んでおり、このメソッドによりメッセージ・ループが実行される）。
        ///  http://www.atmarkit.co.jp/fdotnet/dotnettips/372formstimer/formstimer.html
        /// 
        /// </remarks>
        private static void Repeater(object dummy)
        {
            //Xb.Util.Out("Xb.Base.Repeater: StackedCount = " & Xb.Base._delayedProcStack.Count)
            //遅延処理スタックが存在しないとき、タイマーを破棄する。
            if ((Xb.Base._delayedProcStack.Count <= 0))
            {
                if (Xb.Base._timerProcStack != null)
                {
                    //Xb.Base.Out("Xb.Base.Repeater: Stop Timer")
                    Xb.Base._timerProcStack.Dispose();
                    Xb.Base._timerProcStack = null;
                }
                return;
            }

            //以降、スタック上に遅延処理が１件以上存在するときの処理
            if (Xb.Base._timerProcStack == null)
            {
                //Xb.Base.Out("Xb.Base.Repeater: Create, Start Timer")
                Xb.Base._timerProcStack = new System.Threading.Timer(Xb.Base.Repeater, null, 100, 100);
            }

            //スタック上の各遅延処理を確認、起動時間を過ぎているものをピックアップする。
            List<DelayedProcedure> execProcs = new List<DelayedProcedure>();
            foreach (DelayedProcedure proc in Xb.Base._delayedProcStack)
            {
                if (proc.IsReady)
                {
                    //Xb.Base.Out("Xb.Base.Repeater: Found Ready-Procedure")
                    execProcs.Add(proc);
                }
            }

            //起動予定の処理をスタックから削除する。
            foreach (DelayedProcedure proc in execProcs)
            {
                Xb.Base._delayedProcStack.Remove(proc);
            }

            //ピックアップした処理を順次起動する。
            foreach (DelayedProcedure proc in execProcs)
            {
                Xb.Base.ExecProcedure(proc);
            }

            //Xb.Util.Out("Util.Repeater_4: Timer Enabled? " & (Not Xb.Base._timerProcStack Is Nothing))
        }


        /// <summary>
        /// 遅延処理を実行する。
        /// </summary>
        /// <param name="proc"></param>
        /// <remarks></remarks>
        private static void ExecProcedure(DelayedProcedure proc)
        {
            //Xb.Util.Out("ExecProcedure")
            try
            {
                ((Xb.Base.TimerDelegate)proc.Callback).Invoke(proc.Args);
            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.Base.ExecProcedure: コールバックメソッドの実行に失敗しました：" + ex.Message);
                throw new ArgumentException("Xb.Base.ExecProcedure: コールバックメソッドの実行に失敗しました：" + ex.Message);
            }
        }



        /// <summary>
        /// ログを出力する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <remarks>
        /// ※注意※ メソッド実行の都度ファイルオープンしているため、遅い。早くしたい場合はLogger使用のこと。
        /// </remarks>
        public static bool Log(string message, string directory = null)
        {
            if (message == null)
                message = "Nothing";

            if (directory == null)
                directory = Xb.Base.DefaultLogDirectory;

            string fileName = DateTime.Now.ToString("yyyyMMdd") + "_log.txt";
            string fullPath = null;
            System.IO.StreamWriter wrtLogFile = null;

            //ログファイル出力先が存在しないとき、フォルダ生成を試みる。
            if (!System.IO.Directory.Exists(directory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (Exception)
                {
                    Xb.Util.Out("Xb.Base.Log: 指定されたログ出力先パスを生成出来ませんでした：" + directory);
                    throw new Exception("Xb.Base.Log: 指定されたログ出力先パスを生成出来ませんでした：" + directory);
                }
            }

            try
            {
                //ファイル名とパスを結合
                fullPath = System.IO.Path.Combine(directory, fileName);

                //ファイルの存在チェック
                if (!System.IO.File.Exists(fullPath))
                {
                    //新規モードでファイルを開き、作成日時を書き込む
                    wrtLogFile = new System.IO.StreamWriter(fullPath, false, System.Text.Encoding.UTF8);
                    wrtLogFile.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " : ログファイル作成");
                    wrtLogFile.Close();
                }

                //ログを書き込む
                wrtLogFile = new System.IO.StreamWriter(fullPath, true, System.Text.Encoding.UTF8);
                wrtLogFile.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " : " + message);
                //Util.Out(Now().ToString("yyyy/MM/dd HH:mm:ss.fff") & " : " & message)
                wrtLogFile.Close();

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 例外情報をログに書き出す。
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool Log(Exception ex, string directory = null)
        {
            return Xb.Base.Log(System.String.Join("\r\n", Xb.Util.GetErrorString(ex)), directory);
        }

        public enum LogLevel
        {
            Detail = 0,
            Normal = 3,
            Silent = 5
        }

        public static Xb.Base.LogLevel CurrentLogLevel = LogLevel.Detail;

        public static string DefaultLogDirectory =
            System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        ///' <summary>
        ///' コンソール出力
        ///' </summary>
        ///' <param name="text"></param>
        ///' <remarks></remarks>
        //Public Shared Sub Out(ByVal text As String, _
        //                        Optional ByVal messageLogLevel As Util.LogLevel = Nothing)

        //    Dim log As String = IIf(text Is Nothing, "Nothing", text).ToString()
        //    Debug.WriteLine(Now().ToString("HH:mm:ss.fff : ") & log)
        //    '#If DEBUG Then
        //    'Console.WriteLine(Now().ToString("HH:mm:ss.fff : ") & log)
        //    '#End If

        //    'Util.Log(log, Util.AppPath)

        //End Sub



        ///' <summary>
        ///' 例外を再帰的に取得し、エラー詳細記録用文字列を作る。
        ///' </summary>
        ///' <param name="ex"></param>
        ///' <param name="errString"></param>
        ///' <param name="depth"></param>
        ///' <returns></returns>
        ///' <remarks></remarks>
        //Public Shared Function GetErrorString(ByVal ex As Exception, _
        //                                        Optional ByVal errString As StringBuilder = Nothing, _
        //                                        Optional ByVal depth As Integer = 0) As String

        //    If (ex Is Nothing) Then
        //        Throw New ArgumentException("有効な例外を渡してください。")
        //    End If

        //    If (errString Is Nothing) Then
        //        errString = New StringBuilder()
        //        errString.AppendFormat("{0} ### Error Detail Log ###{0}", vbCrLf)
        //        depth = 1
        //    End If

        //    errString.AppendFormat("{1}--------- Generation: {0} ---------", depth, vbCrLf)
        //    errString.AppendFormat("{1}  Message: {0}", ex.Message, vbCrLf)
        //    errString.AppendFormat("{0}  StackTrace: ", vbCrLf)
        //    errString.AppendFormat("{1}{0}", ex.StackTrace.Replace("場所", vbCrLf & "場所"), vbCrLf)
        //    errString.Append(vbCrLf)

        //    Return If(ex.InnerException IsNot Nothing, _
        //                Util.GetErrorString(ex.InnerException, errString, depth + 1), _
        //                errString.ToString())

        //End Function


        /// <summary>
        /// デザインモードを厳密に判定する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsVsDesignMode()
        {

            bool result = false;

            if ((System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime))
            {
                result = true;
            }
            else if ((System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper().Equals("DEVENV") ||
                      System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper().Equals("VCSEXPRESS")))
            {
                result = true;
            }
            else if ((System.AppDomain.CurrentDomain.FriendlyName == "DefaultDomain"))
            {
                result = true;
            }

            return result;

        }



        private const string RandomNum = "0123456789";
        private const string RandomAlphaSmall = "abcdefghijklmnopqrstuvwxyz";
        private const string RandomAlphaLarge = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private const string RandomSymbol = "-+*~=!#$:;@_";

        public enum RandomStringType
        {
            Num,
            NumAlphaSmall,
            NumAlphaLarge,
            NumAlpha,
            NumAlphaSymbol
        }


        /// <summary>
        /// ランダム文字列を生成する。
        /// </summary>
        /// <param name="length"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetRandomString(int length,
            Xb.Base.RandomStringType type = Xb.Base.RandomStringType.NumAlpha)
        {
            System.Text.StringBuilder builder = new StringBuilder(length);
            Random rnd = new Random();
            string baseString = "";

            switch (type)
            {
                case Xb.Base.RandomStringType.Num:
                    baseString += Xb.Base.RandomNum;
                    break;
                case Xb.Base.RandomStringType.NumAlphaSmall:
                    baseString += Xb.Base.RandomNum;
                    baseString += Xb.Base.RandomAlphaSmall;
                    break;
                case Xb.Base.RandomStringType.NumAlphaLarge:
                    baseString += Xb.Base.RandomNum;
                    baseString += Xb.Base.RandomAlphaLarge;
                    break;
                case Xb.Base.RandomStringType.NumAlpha:
                    baseString += Xb.Base.RandomNum;
                    baseString += Xb.Base.RandomAlphaSmall;
                    baseString += Xb.Base.RandomAlphaLarge;
                    break;
                case Xb.Base.RandomStringType.NumAlphaSymbol:
                    baseString += Xb.Base.RandomNum;
                    baseString += Xb.Base.RandomAlphaSmall;
                    baseString += Xb.Base.RandomAlphaLarge;
                    baseString += Xb.Base.RandomSymbol;
                    break;
                default:
                    baseString += Xb.Base.RandomNum;
                    baseString += Xb.Base.RandomAlphaSmall;
                    break;
            }

            for (int i = 0; i <= length - 1; i++)
            {
                builder.Append(baseString[rnd.Next(baseString.Length)]);
            }

            return builder.ToString();

        }

    }
}