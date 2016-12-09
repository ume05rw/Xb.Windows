using System;
using System.Collections.Generic;

namespace Xb.App
{
    /// <summary>
    /// プロセス管理ユーティリティクラス
    /// </summary>
    /// <remarks></remarks>
    public partial class Process
    {
        /// <summary>
        /// 動作中の全プロセスを取得する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Diagnostics.Process[] GetProcesses()
        {
            return System.Diagnostics.Process.GetProcesses();
        }


        /// <summary>
        /// 渡し値名称に合致したプロセスを返す。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Diagnostics.Process Find(string name)
        {
            System.Diagnostics.Process[] procs = GetProcesses();
            foreach (System.Diagnostics.Process proc in procs)
            {
                if (proc.ProcessName.ToLower().IndexOf(name.ToLower()) != -1)
                    return proc;
            }
            return null;
        }


        /// <summary>
        /// 渡し値名称に合致したプロセスを返す。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static List<System.Diagnostics.Process> FindAll(string name)
        {
            var procs = Xb.App.Process.GetProcesses();
            var result = new List<System.Diagnostics.Process>();

            foreach (System.Diagnostics.Process proc in procs)
            {
                if (proc.ProcessName.ToLower().IndexOf(name.ToLower()) != -1)
                {
                    result.Add(proc);
                }
            }
            return result;
        }


        /// <summary>
        /// 渡し値ファイルを、OS上で関連付けられたプログラムで起動する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <remarks></remarks>
        public static void Kick(string fileName)
        {
            App.Process.Execute("explorer.exe", "\"" + fileName + "\"");
        }

        /// <summary>
        /// 渡し値パスを選択した状態のエクスプローラを表示する。
        /// </summary>
        /// <param name="path"></param>
        /// <remarks></remarks>

        public static void ShowPathByExplorer(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                App.Process.Execute("explorer.exe", "\"" + path + "\"");
            }
            else if (System.IO.File.Exists(path))
            {
                App.Process.Execute("explorer.exe", "/select," + "\"" + path + "\"");
            }
            else
            {
                throw new NullReferenceException("渡し値パスが存在しません。");
            }
        }


        /// <summary>
        /// 外部プログラムを実行する。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <param name="isCommandline"></param>
        /// <param name="errorDialogParentHandle">ウインドウのハンドル。エラー表示が、指定ウインドウに対してモーダルで表示される</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Execute(string fileName, 
                                     string arguments = "", 
                                     bool isCommandline = false, 
                                     IntPtr? errorDialogParentHandle = null)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            string result = "";

            try
            {
                if (!isCommandline)
                {
                    //通常のプログラム実行のとき
                    proc.StartInfo.FileName = fileName;
                    proc.StartInfo.Arguments = arguments;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.CreateNoWindow = false;
                }
                else
                {
                    //コマンドラインプログラムを実行するとき
                    //"cmd.exe"を実行プログラムとする。
                    proc.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");
                    //cmd.exeの引数として、渡し値ファイル名と引数をセット。
                    proc.StartInfo.Arguments = "/c " + fileName + " " + arguments;

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardInput = false;
                }

                proc.StartInfo.Verb = "runas";
                //動詞に「runas」をつける

                if (errorDialogParentHandle != null)
                {
                    //UACダイアログが親プログラムに対して表示されるようにする
                    proc.StartInfo.ErrorDialog = true;
                    proc.StartInfo.ErrorDialogParentHandle = (IntPtr)errorDialogParentHandle;
                }

                proc.Start();

                if (isCommandline)
                {
                    //出力を読み取る
                    result = proc.StandardOutput.ReadToEnd();

                    //プロセス終了まで待機する
                    //WaitForExitはReadToEndの後である必要がある
                    //(親プロセス、子プロセスでブロック防止のため)
                    proc.WaitForExit();
                }

                proc.Close();

                return result;

            }
            catch (Exception ex)
            {
                Xb.Util.Out("Xb.App.Process.Execute: プロセスの実行に失敗しました：" + ex.Message);
                throw new ArgumentException("Xb.App.Process.Execute: プロセスの実行に失敗しました：" + ex.Message);
            }
        }
    }
}
