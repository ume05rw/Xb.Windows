using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Xb.App
{
    /// <summary>
    /// クラス管理関数群
    /// </summary>
    /// <remarks></remarks>
    public partial class Class
    {

        /// <summary>
        /// メソッド情報保持クラス
        /// </summary>
        /// <remarks>
        /// </remarks>
        public class Method
        {
            public readonly string Name;
            public readonly System.Reflection.MethodInfo Info;

            public readonly System.Reflection.ParameterInfo[] ParamInfos;
            public Method(System.Reflection.MethodInfo info)
            {
                this.Info = info;
                this.Name = info.Name;
                this.ParamInfos = info.GetParameters();
            }
        }


        /// <summary>
        /// 文字型引数指定されたメソッドを実行する。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// コマンドライン引数を解釈してクラス型取得、インスタンス生成して実行するためのSharedメソッド
        /// 
        /// 引数構成：
        ///   (1)コマンドラインスイッチ
        ///     --class / -c           クラス名
        ///     --classArgs / -ca      クラスインスタンス生成用引数 (省略可、現状では省略時はSharedメソッドのみ実行可能)
        ///     --method / -m          メソッド名
        ///     --methodArgs / -ma     メソッド引数 (省略可)
        /// 
        ///   (2)文字列の先頭から順に解釈する。
        ///     1.クラス名
        ///     2.Sharedメソッド名
        ///     3つめ以降．すべてメソッド引数
        /// 
        /// 
        /// (実行ファイル.exe) --class Aaa.Bbb --method TestMethod  
        ///   --classArgs 1 "a b" --methodArgs a false
        /// 
        /// -> (New Aaa.Bbb(1, "a b")).TestMethod("a", False)
        /// 
        /// 引数には文字列、数値、Bool以外に使用できない。
        /// 
        /// </remarks>
        public static bool ExecByCommandSwitch(string[] args)
        {
            Xb.Util.Out("Try Method Parse...");
            List<object> methodArgs = new List<object>();
            Xb.App.Class.Executer method = default(Xb.App.Class.Executer);
            Xb.App.Arguments argList = new Xb.App.Arguments(args);

            //渡し値引数をダンプ
            for (int i = 0; i <= args.Length - 1; i++)
            {
                Xb.Util.Out(string.Format("Arg {0}: {1}", i, args[i]));
            }

            //引数キーのうち、同じ意味合いの値をマージする。
            argList.MergeArguments("--class", new string[] { "-c" });
            argList.MergeArguments("--classArgs", new string[] { "-ca" });
            argList.MergeArguments("--method", new string[] { "-m" });
            argList.MergeArguments("--methodArgs", new string[] {
                "-ma",
                App.Arguments.KeyNotFound
            });

            //引数中の特定のコマンドラインスイッチ有無で分岐
            if (argList.Has("--class") & argList.Has("--method"))
            {
                //引数のコマンドラインスイッチで、クラス・メソッドの両方が検出できたとき
                Xb.Util.Out("Parse By Command-Switch");

                try
                {
                    if (argList.Has("--classArgs"))
                    {
                        method = new App.Class.Executer(argList.Find("--class"), 
                                                        argList.FindAll("classArgs"), 
                                                        argList.Find("--method"), 
                                                        argList.FindAll("--methodArgs"));
                    }
                    else
                    {
                        method = new App.Class.Executer(argList.Find("--class"), 
                                                        argList.Find("--method"), 
                                                        argList.FindAll("--methodArgs"));
                    }
                }
                catch (Exception)
                {
                    Xb.Util.Out("Fail Method Parse");
                    return false;
                }
            }
            else
            {
                //特定のスイッチが検出できないときは、先頭からクラス名、メソッド名、引数、の順に割り当てる。
                //実行出来るメソッドはSharedメソッドに限定される。
                Xb.Util.Out("Parse By Silial Args");
                if (args.Length < 2)
                    Array.Resize(ref args, 2);

                for (int i = 2; i <= args.Length - 1; i++)
                {
                    methodArgs.Add(args[i]);
                }

                try
                {
                    method = new App.Class.Executer(args[0], args[1], methodArgs.ToArray());
                }
                catch (Exception)
                {
                    Xb.Util.Out("Fail Method Parse");
                    return false;
                }
            }

            Xb.Util.Out("Try Method Execute...");
            try
            {
                method.Execute();
            }
            catch (Exception)
            {
                Xb.Util.Out("Fail Method Parse");
                return false;
            }

            Xb.Util.Out("Method Execution Completed");
            return true;

        }


        /// <summary>
        /// メソッド情報を取得する。
        /// </summary>
        /// <param name="classType"></param>
        /// <returns></returns>
        public static List<App.Class.Method> GetClassMethodInfos(System.Type classType)
        {
            List<App.Class.Method> resultList = new List<Method>();

            foreach (System.Reflection.MethodInfo minf in classType.GetMethods())
            {
                resultList.Add(new App.Class.Method(minf));
            }
            return resultList;

            //LINQ構文byResharper。.NetFW2.0互換のために一旦採用見送り。
            //Return (From minf In classType.GetMethods() _
            //        Select New App.Class.Method(minf)).ToArray()
        }


        /// <summary>
        /// 指定クラスのコンストラクタが実行可能か否かを返す
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsClassCreatable(System.Type type, object[] args)
        {

            if (args == null)
                args = new object[]{};

            //クラスの全コンストラクタ情報を取得
            System.Reflection.ConstructorInfo[] constructors = type.GetConstructors();
            System.Reflection.ParameterInfo[] @params = null;

            //コンストラクタの数分ループ
            foreach (System.Reflection.ConstructorInfo constructor in constructors)
            {
                //コンストラクタの全引数情報を取得
                @params = constructor.GetParameters();

                //コンストラクタが渡し値で実行可能か否かを調べる。
                if (Xb.App.Class.HasArgsCompatibility(@params, args))
                {
                    //実行可能メソッドが存在するとき、OK
                    return true;
                }
            }

            //実行可能なコンストラクタがないとき、NG
            return false;
        }


        /// <summary>
        /// 引数配列と引数メタ情報を比較し、互換性があるか否かを調べる。
        /// </summary>
        /// <param name="paramInfos"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// メソッドが実行可能か否かを調べる。
        /// App.Thread等、メソッド情報を渡して実行依頼する処理などで使う。
        /// ※メソッド実行が可能でも、メソッド内で例外が発生するか否かは調査出来ないので、あしからず！
        /// </remarks>
        public static bool HasArgsCompatibility(System.Reflection.ParameterInfo[] paramInfos, object[] args)
        {
            //引数がメソッド定義より多いとき、NG
            if (paramInfos.Length < args.Length)
                return false;

            //引数が足りず、指定できない引数がOptionalでないとき、NG
            if (args.Length < paramInfos.Length)
            {
                for (int i = args.Length; i <= paramInfos.Length - 1; i++)
                {
                    if (!paramInfos[i].IsOptional)
                        return false;
                }
            }

            //引数の数分ループ
            for (int i = 0; i <= args.Length - 1; i++)
            {
                //引数が指定の型にキャスト出来ないとき、NG
                if (!Xb.Type.Util.IsConvertable(args[i], paramInfos[i].ParameterType))
                    return false;
            }
            return true;
        }
    }
}
