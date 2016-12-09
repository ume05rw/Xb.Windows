using System;

namespace Xb.App
{
    /// <summary>
    /// レジストリへのアプリケーション登録、ファイル関連付け操作用クラス
    /// </summary>
    /// <remarks>
    /// XP/2000    ：拡張子別関連付け機能対応
    /// Vista/7    ：「規定のプログラム」一覧への追加のみ対応(拡張子別関連付けは要手動登録)
    /// 8/8.1      ：管理者権限でないときエラーになる。
    /// </remarks>
    public class Regist : IDisposable
    {
        private readonly string _name;
        private readonly string _path;

        private readonly string _companyName;

        /// <summary>
        /// レジストリルート区分
        /// </summary>
        /// <remarks></remarks>
        public enum RegistryRoot
        {
            ClassRoot,
            CurrentUser,
            LocalMachine,
            Users,
            CurrentConfig
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>
        public Regist(string name, string path, string companyName)
        {
            this._name = name;
            this._path = path;
            this._companyName = companyName;

            //アプリケーション名の有無
            if (this._name.Length <= 0)
            {
                Xb.Util.Out("Xb.App.Regist.Constructor: アプリケーション名がセットされていません。");
                throw new Exception("Xb.App.Regist.Constructor: アプリケーション名がセットされていません。");
            }

            //会社名の有無
            if (this._companyName.Length <= 0)
            {
                Xb.Util.Out("Xb.App.Regist.Constructor: 会社名がセットされていません。");
                throw new Exception("Xb.App.Regist.Constructor: 会社名がセットされていません。");
            }

            if (!System.IO.File.Exists(this._path))
            {
                Xb.Util.Out("Xb.App.Regist.Constructor: アプリケーションの実行ファイルパスがセットされていません。");
                throw new Exception("Xb.App.Regist.Constructor: アプリケーションの実行ファイルパスがセットされていません。");
            }
        }


        /// <summary>
        /// 拡張子、実行ファイルパスの検証、およびファイルタイプ文字列作成を行う。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool ValidateExtension(string extension)
        {
            //渡し値の入力有無を検証する。
            if (extension.Length < 2)
                return false;

            //渡し値にドットが無いとき、追加する。
            if (extension.Substring(0, 1) != ".")
                extension = "." + extension;

            extension = extension.ToLower();

            return true;
        }


        /// <summary>
        /// 指定したレジストリキーが存在するか否かを検証する。
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyRoot"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool ExistKey(string keyName, RegistryRoot keyRoot)
        {
            Microsoft.Win32.RegistryKey regkey = null;

            //指定ルートのキーを開く
            switch (keyRoot)
            {
                case RegistryRoot.ClassRoot:
                    regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.CurrentUser:
                    regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.LocalMachine:
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.Users:
                    regkey = Microsoft.Win32.Registry.Users.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.CurrentConfig:
                    regkey = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(keyName, false);
                    break;
                default:
                    //キーが取得出来ないとき、存在しないのでFalseを返す。
                    return false;
            }

            //存在するとき、開いたキーを閉じてTrueを返す。
            regkey.Close();
            return true;
        }


        /// <summary>
        /// 指定したレジストリキーの値が存在するか否かを検証する。
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyRoot"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool ExistValue(string keyName, RegistryRoot keyRoot, string valueName)
        {
            Microsoft.Win32.RegistryKey regkey = null;
            string stringValue = null;

            //指定ルートのキーを開く
            switch (keyRoot)
            {
                case RegistryRoot.ClassRoot:
                    regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.CurrentUser:
                    regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.LocalMachine:
                    regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.Users:
                    regkey = Microsoft.Win32.Registry.Users.OpenSubKey(keyName, false);
                    break;
                case RegistryRoot.CurrentConfig:
                    regkey = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(keyName, false);
                    break;
                default:
                    //キーが取得出来ないとき、存在しないのでFalseを返す。
                    return false;
            }

            //拡張子登録キーから、プログラムIDを取得する。
            stringValue = Convert.ToString(regkey.GetValue(valueName));

            regkey.Close();

            //指定の値が登録されていないとき、Falseを返す。
            if (stringValue == null)
                return false;

            //登録されているとき、Trueを返す。
            return true;
        }


        /// <summary>
        /// アプリケーションをレジストリに登録する。
        /// </summary>
        /// <param name="applicationDesctiption"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool RegistApplication(string applicationDesctiption = "")
        {
            Microsoft.Win32.RegistryKey reg1 = null;
            Microsoft.Win32.RegistryKey reg2 = null;
            Microsoft.Win32.RegistryKey reg3 = null;

            //１．アプリケーション概要を登録する。
            //社名で登録されているか検証する。
            reg1 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + this._companyName, false);

            //社名が登録されていないときのみ、登録する。
            if (reg1 == null)
            {
                Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\" + this._companyName);
            }

            //アプリケーション名で登録する
            Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name);

            //"Capabilities"サブキーを追加する
            reg2 = reg1.CreateSubKey("Capabilities");

            //アプリケーション名を登録する。
            reg2.SetValue("ApplicationName", this._name);

            //アプリケーションの説明を登録する
            reg2.SetValue("ApplicationDescription", applicationDesctiption);


            //２．OSが管理する登録済みアプリケーションとして、アプリケーション概要登録のパスを登録する。
            //アプリケーション登録用キーを取得する
            reg3 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true);

            //登録用キーに、アプリケーション情報を書き込む
            reg3.SetValue(this._name, "SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities");

            return true;
        }


        /// <summary>
        /// アプリケーション登録を削除する
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool DeleteApplication()
        {
            Microsoft.Win32.RegistryKey regkey = null;

            //１．OSが管理する登録済みアプリケーションリストから、指定アプリケーションを削除する。

            //登録用キー上にアプリケーション情報が登録されているとき、アプリケーション情報を削除する。
            if (this.ExistValue("SOFTWARE\\RegisteredApplications", RegistryRoot.LocalMachine, this._name))
            {
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", true);
                regkey.DeleteValue(this._name);
            }

            //２．アプリケーション概要登録を削除する。
            //アプリケーション名を削除する。
            if (this.ExistKey("SOFTWARE\\" + this._companyName + "\\" + this._name, RegistryRoot.LocalMachine))
            {
                Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree("SOFTWARE\\" + this._companyName + "\\" + this._name);
            }

            return true;
        }


        /// <summary>
        /// アプリケーションが登録済みかどうかを検証する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsRegistedApplication()
        {
            Microsoft.Win32.RegistryKey regkey = null;
            string stringValue = null;

            //キーを読み取り専用で開く
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name, false);

            //キーが存在しないときはNothingが返される
            if (regkey == null)
                return false;

            //アプリケーション登録キーを取得する
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\RegisteredApplications", false);

            //指定のアプリケーション名の値を取得する。
            stringValue = Convert.ToString(regkey.GetValue(this._name));

            //登録されていないとき、値はNothingが返る
            if (stringValue == null)
                return false;

            return true;
        }


        /// <summary>
        /// アプリケーションの、拡張子の関連付け宣言を登録する。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool RegistExtensionRelation(string extension)
        {
            Microsoft.Win32.RegistryKey regkey = null;
            Microsoft.Win32.RegistryKey openkey = null;
            Microsoft.Win32.RegistryKey appkey = null;
            Microsoft.Win32.RegistryKey iconkey = null;
            string programId = null;
            string commandline = null;

            commandline = "\"" + this._path + "\" " + "\"%1\"";

            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedApplication())
            {
                Xb.Util.Out("Xb.App.Regist.RegistExtensionRelation: アプリケーションが登録されていません。");
                throw new Exception("Xb.App.Regist.RegistExtensionRelation: アプリケーションが登録されていません。");
            }

            //渡し値の実行パス、拡張子を検証する。
            if (!this.ValidateExtension(extension))
            {
                Xb.Util.Out("Xb.App.Regist.RegistExtensionRelation: 拡張子指定が異常です。");
                throw new Exception("Xb.App.Regist.RegistExtensionRelation: 拡張子指定が異常です。");
            }

            //アプリケーション登録上の、ファイル関連付けキーを取得する。
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities\\FileAssociations", true);

            //ファイル関連付けキーが存在しないとき、追加する。
            if (regkey == null)
            {
                regkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities\\FileAssociations");
            }

            //１．HKLM上のアプリケーション登録へ、拡張子別プログラムIDを作り、追加する。
            //拡張子別プログラムID(実行ファイル名＋拡張子大文字)を作る。
            programId = this._name + ".AssocFile" + extension.ToUpper();

            //アプリケーション登録上へ、拡張子の名称でプログラムIDをセットする。
            regkey.SetValue(extension, programId);

            //レジストリキーオブジェクトを閉じる。
            regkey.Close();

            //２．拡張子別プログラムIDを、HKCRへ登録する。
            //HKCRへ、拡張子別プログラムIDでキーを登録する。
            regkey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(programId);

            //説明を登録する。
            regkey.SetValue("", extension.ToUpper() + " File");

            //コマンドラインの関連付けを作る
            regkey = regkey.CreateSubKey("shell");
            openkey = regkey.CreateSubKey("open");
            openkey = openkey.CreateSubKey("command");
            openkey.SetValue("", commandline);

            appkey = regkey.CreateSubKey(this._name);
            appkey.SetValue("", this._name + "で開く");
            appkey = appkey.CreateSubKey("command");
            appkey.SetValue("", commandline);

            //アイコンを登録する。
            iconkey = regkey.CreateSubKey("DefaultIcon");
            iconkey.SetValue("", this._path + ",0");

            //レジストリキーオブジェクトを閉じる。
            regkey.Close();
            openkey.Close();
            appkey.Close();
            iconkey.Close();

            return true;
        }


        /// <summary>
        /// アプリケーションの、拡張子関連付け宣言登録を削除する。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool DeleteExtensionRelation(string extension)
        {
            Microsoft.Win32.RegistryKey regkey = null;
            string programId = null;

            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedApplication())
            {
                Xb.Util.Out("Xb.App.Regist.DeleteExtensionRelation: アプリケーションが登録されていません。");
                throw new Exception("Xb.App.Regist.DeleteExtensionRelation: アプリケーションが登録されていません。");
            }

            //渡し値の実行パス、拡張子を検証する。
            if (!this.ValidateExtension(extension))
            {
                Xb.Util.Out("Xb.App.Regist.DeleteExtensionRelation: 拡張子指定が異常です。");
                throw new Exception("Xb.App.Regist.DeleteExtensionRelation: 拡張子指定が異常です。");
            }

            //１．HKCR上の拡張子別プログラムIDを、削除する。
            //拡張子別プログラムID(実行ファイル名＋拡張子大文字)を作る。
            programId = this._name + ".AssocFile" + extension.ToUpper();

            //HKCR上の拡張子別プログラムID登録を削除する。
            if (this.ExistKey(programId, RegistryRoot.ClassRoot))
            {
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(programId);
            }

            //２．HKLM上のアプリケーション登録から、拡張子別プログラムIDを削除する。

            //アプリケーション登録上の、拡張子別プログラムIDを削除する。

            if (this.ExistValue("SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities\\FileAssociations", RegistryRoot.LocalMachine, extension))
            {
                //アプリケーション登録上の、ファイル関連付けキーを取得する。
                regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities\\FileAssociations", true);

                regkey.DeleteValue(extension);

                //レジストリキーオブジェクトを閉じる。
                regkey.Close();
            }

            return true;
        }


        /// <summary>
        /// アプリケーション登録上で、拡張子関連付け宣言登録がなされているかを検証する。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsRegistedExtensionRelation(string extension)
        {
            Microsoft.Win32.RegistryKey regkey = null;
            string programId = null;
            string stringValue = null;


            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedApplication())
            {
                Xb.Util.Out("Xb.App.Regist.IsRegistedExtensionRelation: アプリケーションが登録されていません。");
                throw new Exception("Xb.App.Regist.IsRegistedExtensionRelation: アプリケーションが登録されていません。");
            }

            //渡し値の実行パス、拡張子を検証する。
            if (!this.ValidateExtension(extension))
            {
                Xb.Util.Out("Xb.App.Regist.IsRegistedExtensionRelation: 拡張子指定が異常です。");
                throw new Exception("Xb.App.Regist.IsRegistedExtensionRelation: 拡張子指定が異常です。");
            }

            //１．アプリケーション登録上のデータを検証する。
            //アプリケーション登録上の、ファイル関連付けキーを取得する。
            regkey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\" + this._companyName + "\\" + this._name + "\\Capabilities\\FileAssociations", true);

            //ファイル関連付けキーが存在しないとき、拡張子は未登録とする。
            if (regkey == null)
                return false;

            //ファイル関連付けキーから、指定拡張子の登録値を取得する。
            stringValue = Convert.ToString(regkey.GetValue(extension));

            //指定拡張子の値が取得できないとき、拡張子は未登録とする。
            if (stringValue == null)
            {
                regkey.Close();
                return false;
            }


            //２．拡張子別プログラムID登録を検証する。
            //拡張子別プログラムID(実行ファイル名＋拡張子大文字)を作る。
            programId = this._name + ".AssocFile" + extension.ToUpper();

            //HKCR上の、拡張子別プログラムIDの実行ファイル登録キーを取得する。
            regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(programId + "\\shell\\open\\command", false);

            //ファイル関連付けキーが存在しないとき、拡張子は未登録とする。
            if (regkey == null)
                return false;

            //実行ファイル登録キーから、実行ファイルパスを取得する。
            stringValue = Convert.ToString(regkey.GetValue(""));

            if (stringValue == null)
            {
                regkey.Close();

                this.DeleteExtensionRelation(extension);

                return false;
            }

            stringValue = stringValue.Replace("%1", "").Replace("\"", "").Trim();

            //実行ファイルのパスが異なるとき、登録を削除して未登録とする。
            if (this._path.ToUpper() != stringValue.ToUpper())
            {
                regkey.Close();

                this.DeleteExtensionRelation(extension);

                return false;
            }

            //レジストリキーオブジェクトを閉じる。
            regkey.Close();

            //アプリケーション上の拡張子登録と、HKCR上のプログラムID登録の両方が存在するとき、拡張子関連付けは登録済みとする。
            return true;
        }


        /// <summary>
        /// 指定アプリケーションと拡張子を関連付ける
        /// </summary>
        /// <param name="extension">拡張子</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool RegistExtension(string extension)
        {
            //拡張子の関連付け宣言文字列
            Microsoft.Win32.RegistryKey regkey = null;
            string programId = null;

            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedExtensionRelation(extension))
            {
                Xb.Util.Out("Xb.App.Regist.IsRegistedExtensionRelation: アプリケーションと拡張子の関連付け宣言が登録されていません。");
                throw new Exception("Xb.App.Regist.IsRegistedExtensionRelation: アプリケーションと拡張子の関連付け宣言が登録されていません。");
            }

            //渡し値の拡張子を整形する。
            this.ValidateExtension(extension);

            //拡張子別プログラムID(実行ファイル名＋拡張子大文字)を作る。
            programId = this._name + ".AssocFile" + extension.ToUpper();

            //HKCR上の拡張子キーが存在するとき、一旦削除する。
            if (this.ExistKey(extension, RegistryRoot.ClassRoot))
            {
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(extension);
            }

            //HKCR上の拡張子キーへ、拡張子別プログラムIDを登録する。
            regkey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(extension);
            regkey.SetValue("", programId);
            regkey.Close();

            //エクスプローラのデフォルトアプリケーション設定
            //Vista以降は、以下のパスにデフォルトアプリケーション設定が格納されている。
            // ・HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU\(拡張子)
            // ・HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\(拡張子)
            //但し、win7では直接書き込みオープンが出来ない
            //※API(IApplicationAssociationRegistration)を使うことで可能なようだが、WinXPとの共通バイナリにならないため断念する。

            //Dim stringValue As String = ""
            //regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" _
            //            & Extension & "\UserChoice", True)

            //'HKCU上のキーが取得できなかったとき、正常終了する。
            //If (regkey Is Nothing) Then Return True

            //'拡張子登録キーから、プログラムIDを取得する。
            //stringValue = CType(regkey.GetValue(""), String)

            //'プログラムIDが登録されていないとき、登録して正常終了する。
            //If (stringValue Is Nothing) Then
            //    regkey.SetValue("", programID)
            //    Return True
            //End If

            //'プログラムIDが、本アプリケーションのものと異なるときｊ、一旦削除した上で登録して正常終了する。
            //If (stringValue <> programID) Then
            //    regkey.DeleteValue("")
            //    regkey.SetValue("", programID)
            //    Return True
            //End If

            return true;
        }


        /// <summary>
        /// 拡張子とアプリケーションの関連付けを削除する。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool DeleteExtension(string extension)
        {
            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedExtensionRelation(extension))
            {
                Xb.Util.Out("Xb.App.Regist.RegistExtensionRelation: アプリケーションと拡張子の関連付け宣言が登録されていません。");
                throw new Exception("Xb.App.Regist.RegistExtensionRelation: アプリケーションと拡張子の関連付け宣言が登録されていません。");
            }

            //渡し値の拡張子を整形する。
            this.ValidateExtension(extension);

            //HKCR上の拡張子キーを削除する。
            if (this.ExistKey(extension, RegistryRoot.ClassRoot))
            {
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(extension);
            }

            return true;
        }


        /// <summary>
        /// 拡張子とアプリケーションの関連付け存在を検証する。
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool IsRegistedExtension(string extension)
        {
            //拡張子の関連付け宣言文字列
            string programId = null;
            string stringValue = null;

            //アプリケーション登録が済んでいないとき、異常終了する。
            if (!this.IsRegistedExtensionRelation(extension))
            {
                Xb.Util.Out("Xb.App.Regist.IsRegistedExtension: アプリケーションと拡張子の関連付け宣言が登録されていません。");
                throw new Exception("Xb.App.Regist.IsRegistedExtension: アプリケーションと拡張子の関連付け宣言が登録されていません。");
            }

            //渡し値の拡張子を整形する。
            this.ValidateExtension(extension);

            //キーを読み取り専用で開く
            Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension, false);

            //キーが存在しないときはNothingが返される
            if (regkey == null)
                return false;

            //拡張子別プログラムID(実行ファイル名＋拡張子大文字)を作る。
            programId = this._name + ".AssocFile" + extension.ToUpper();

            //拡張子登録キーから、プログラムIDを取得する。
            stringValue = Convert.ToString(regkey.GetValue(""));

            //プログラムIDが取得できないとき、拡張子登録キーを削除して未登録とする。
            if (stringValue == null)
            {
                regkey.Close();

                this.DeleteExtension(extension);

                return false;
            }

            //登録済みのプログラムIDが本アプリケーションのプログラムIDと異なるとき、登録は削除せずに未登録とする。
            if (stringValue != programId)
            {
                regkey.Close();

                return false;
            }

            regkey.Close();

            return true;
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
                    // TODO: 明示的に呼び出されたときにマネージ リソースを解放します
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
