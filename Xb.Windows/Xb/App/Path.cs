using System;

namespace Xb.App
{

    /// <summary>
    /// パス取得用名前空間定義
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class Path
    {
        /// <summary>
        /// アプリケーション実行ファイルが存在するパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string App
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().Location; }
        }

        /// <summary>
        /// カレントディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Current
        {
            get { return System.IO.Directory.GetCurrentDirectory(); }
        }

        /// <summary>
        /// "Program Files"ディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ProgramFiles
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles); }
        }

        /// <summary>
        /// "Program Files (x86)"ディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string ProgramFilesX86
        {
            get
            {
                System.IO.DirectoryInfo path = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
                string result = path.Parent.FullName + "\\Program Files (x86)";

                return (System.IO.Directory.Exists(result) ? result : path.FullName).ToString();
            }
        }

        /// <summary>
        /// デスクトップのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Desktop
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }
        }

        /// <summary>
        /// ApplicationDataディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// ローミング ユーザー用のディレクトリ
        /// </remarks>
        public static string ApplicationData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
        }

        /// <summary>
        /// LocalApplicationDataディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// 非ローミング ユーザー用のディレクトリ
        /// </remarks>
        public static string LocalApplicationData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
        }

        /// <summary>
        /// システムディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SystemDirectory
        {
            get { return Environment.SystemDirectory; }
        }

        /// <summary>
        /// テンポラリディレクトリのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Temp
        {
            get { return System.IO.Path.GetTempPath(); }
        }

        /// <summary>
        /// カレントユーザーのマイドキュメントのパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string MyDocument
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); }
        }

        /// <summary>
        /// カレントユーザーのマイピクチャパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string MyPictures
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); }
        }

        /// <summary>
        /// カレントユーザーのマイミュージックパスを返す。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string MyMusic
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); }
        }

        /// <summary>
        /// システム上に存在可能な絶対パスを取得する。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetAbsPath(string path = null, string baseDirectory = null)
        {
            if (path == null)
                path = "";

            //渡し値パスが既に絶対パスだった場合、渡し値パスを返す。
            if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
            {
                //末尾には"\"がないようにする。
                if (Xb.Str.Right(path, 1) == "\\")
                    path = Xb.Str.Right(path, -1);
                return path;
            }

            string curDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            curDir = curDir.Trim(Convert.ToChar("\\")) + "\\";

            //起点ディレクトリの整形
            if (baseDirectory == null)
            {
                //起点ディレクトリが渡されていないとき、カレントディレクトリを起点とする。
                baseDirectory = curDir;
            }
            else
            {
                //末尾に必ず"\" が付くように整形。
                baseDirectory = baseDirectory.Trim(Convert.ToChar("\\")) + "\\";

                if (!System.IO.Directory.Exists(baseDirectory))
                {
                    //渡し値の起点ディレクトリが検出出来ないとき、
                    //相対パス渡しを想定してカレントパスとマージした上で試す。
                    baseDirectory = new Uri(new Uri(curDir.Replace("%", "%25")), baseDirectory.Replace("%", "%25")).LocalPath.Replace("%25", "%");

                    //起点ディレクトリが不明のとき、例外。
                    if (!System.IO.Directory.Exists(baseDirectory))
                    {
                        Xb.Util.Out("Xb.App.Path.GetAbsPath: 渡し値ディレクトリが検出できません。");
                        throw new ArgumentException("Xb.App.Path.GetAbsPath: 渡し値ディレクトリが検出できません。");
                    }
                }
            }

            //パスが渡されていないとき、起点ディレクトリを返す。
            //起点ディレクトリも渡されていないときはカレントパスが戻り値になる。
            if (string.IsNullOrEmpty(path))
            {
                //末尾には"\"がないようにする。
                if (Xb.Str.Right(baseDirectory, 1) == "\\")
                    baseDirectory = Xb.Str.Right(baseDirectory, -1);
                return baseDirectory;
            }

            return new Uri(new Uri(baseDirectory.Replace("%", "%25")), path.Replace("%", "%25")).LocalPath.Replace("%25", "%");
        }


        /// <summary>
        /// 一時ファイル名をランダム生成する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetTempFilename(string extension = "tmp", string directory = null)
        {
            if ((extension == null))
                extension = "tmp";
            if ((directory == null))
                directory = Xb.App.Path.Temp;

            if ((!System.IO.Directory.Exists(directory)))
            {
                Util.Out("Path.GetTempFilename: 指定のディレクトリパスが検出出来ません。");
                throw new ArgumentException("指定のディレクトリパスが検出出来ません。");
            }

            string name = null;
            string result = null;
            name = Xb.Str.Left(System.IO.Path.GetRandomFileName(), -4) + "." + extension;
            result = Xb.App.Path.GetAbsPath(name, directory);

            //存在しないファイルパスが取得出来るまで再帰する。
            if ((System.IO.File.Exists(result)))
                result = GetTempFilename(extension);

            return result;
        }
    }
}
