using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xb.App
{

    public partial class Class
    {
        /// <summary>
        /// クラス情報保持クラス
        /// </summary>
        /// <remarks>
        /// Typeクラスに依存している。
        /// </remarks>
        public class Executer : IDisposable
        {
            private string _className;
            private System.Type _classType;
            private object[] _classArgs;
            private object _classInstance;
            private bool _isClassDisposable;

            private bool _isCreateInstanceOnly;
            private string _methodName;
            private System.Type _methodType;
            private object[] _methodArgs;
            private bool _isMethodStatic;
            private System.Reflection.MethodInfo _methodInfo;

            private List<System.Reflection.ParameterInfo> _methodParamInfo;
            #region "プロパティ"

            /// <summary>
            /// メソッドの所属クラス名
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public string ClassName
            {
                get { return this._className; }
            }


            /// <summary>
            /// メソッドの所属するクラス型値
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public System.Type ClassType
            {
                get { return this._classType; }
            }


            /// <summary>
            /// クラスのコンストラクタ引数配列
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public object[] ClassArgs
            {
                get { return this._classArgs; }
            }


            /// <summary>
            /// クラスインスタンス
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public object ClassInstance
            {
                get { return this._classInstance; }
            }


            /// <summary>
            /// メソッド名
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public string MethodName
            {
                get { return this._methodName; }
            }


            /// <summary>
            /// メソッドの戻り値型
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public System.Type MethodType
            {
                get { return this._methodType; }
            }


            /// <summary>
            /// メソッド引数配列
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public object[] MethodArgs
            {
                get { return this._methodArgs; }
            }


            /// <summary>
            /// メソッドのメタデータ
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public MethodInfo MethodInfo
            {
                get { return this._methodInfo; }
            }


            /// <summary>
            /// メソッドの引数情報配列
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public List<ParameterInfo> MethodParamInfo
            {
                get { return this._methodParamInfo; }
            }

            #endregion

            /// <summary>
            /// コンストラクタ１ - メソッドが Shared/Static でないときにインスタンス生成する。
            /// </summary>
            /// <param name="classType"></param>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <remarks>
            /// 主にSharedメソッド用。同プロジェクト内から使用するときに使用する。
            /// </remarks>

            public Executer(System.Type classType, string methodName, object[] methodArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = null;
                this._classType = classType;
                this._classArgs = null;
                this._classInstance = null;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = false;

                this._methodName = methodName;
                this._methodType = null;
                this._methodArgs = methodArgs;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();
            }


            /// <summary>
            /// コンストラクタ２ - 渡し値インスタンス上で指定メソッドを実行する。
            /// </summary>
            /// <param name="classInstance"></param>
            /// <param name="classType"></param>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <remarks>
            /// インスタンスメソッド用。同プロジェクト内から使用するときに使用する。
            /// ※インスタンスとその型を両方渡すのは冗長だが、一度Object型に
            /// ※キャストされたインスタンスから元の型を類推することが難しいため、
            /// ※この実装にする。
            /// </remarks>

            public Executer(object classInstance, System.Type classType, string methodName, object[] methodArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = null;
                this._classType = classType;
                this._classArgs = null;
                this._classInstance = classInstance;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = false;

                this._methodName = methodName;
                this._methodType = null;
                this._methodArgs = methodArgs;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();
            }


            /// <summary>
            /// コンストラクタ３ - インスタンス生成のみを行う
            /// </summary>
            /// <param name="classType"></param>
            /// <param name="classArgs"></param>
            /// <remarks>
            /// インスタンス生成のみを行う
            /// </remarks>

            public Executer(System.Type classType, object[] classArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = null;
                this._classType = classType;
                this._classArgs = classArgs;
                this._classInstance = null;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = true;

                this._methodName = null;
                this._methodType = null;
                this._methodArgs = null;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();
            }


            /// <summary>
            /// コンストラクタ４ - メソッドが Shared/Static でないときにインスタンス生成する。
            /// </summary>
            /// <param name="className"></param>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <remarks>
            /// ※別exeでの任意クラス実行を意図した実装※
            /// 主にSharedメソッド用。
            /// </remarks>

            public Executer(string className, string methodName, object[] methodArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = className;
                this._classType = null;
                this._classArgs = null;
                this._classInstance = null;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = false;

                this._methodName = methodName;
                this._methodType = null;
                this._methodArgs = methodArgs;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();
            }


            /// <summary>
            /// コンストラクタ５ - クラスインスタンスを生成した上でメソッドを実行する。
            /// </summary>
            /// <param name="className"></param>
            /// <param name="classArgs"></param>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <remarks>
            /// ※別exeでの任意クラス実行を意図した実装※
            /// インスタンスメソッド用。
            /// </remarks>
            public Executer(string className, object[] classArgs, string methodName, object[] methodArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = className;
                this._classType = null;
                this._classArgs = classArgs;
                this._classInstance = null;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = false;

                this._methodName = methodName;
                this._methodType = null;
                this._methodArgs = methodArgs;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();

            }


            /// <summary>
            /// コンストラクタ６ - インスタンス生成のみを行う
            /// </summary>
            /// <param name="className"></param>
            /// <param name="classArgs"></param>
            /// <remarks>
            /// ※別exeでの任意クラス実行を意図した実装※
            /// インスタンス生成のみを行う
            /// </remarks>
            public Executer(string className, object[] classArgs)
            {
                //渡し値を取得、他クラス変数を初期化する。
                this._className = className;
                this._classType = null;
                this._classArgs = classArgs;
                this._classInstance = null;
                this._isClassDisposable = false;
                this._isCreateInstanceOnly = true;

                this._methodName = null;
                this._methodType = null;
                this._methodArgs = null;
                this._isMethodStatic = false;
                this._methodInfo = null;
                this._methodParamInfo = null;

                //クラス・メソッド実行準備を行う。
                this.GetReady();
            }


            /// <summary>
            /// 各コンストラクタで渡された値を整形・検証し、メソッド実行準備を行う。
            /// </summary>
            /// <remarks>
            /// 準備のフローは以下の通り。
            /// 　1.クラスの型を検証
            /// 　	　型が取得出来ていないとき、
            /// 　	　1-1.クラス名を整形
            /// 　	　1-2.クラス型を取得
            ///
            /// 　2.メソッド情報を取得
            ///
            /// 　3.メソッド引数を検証
            /// 
            /// 　4.クラスインスタンス生成可否チェック(メソッドがStaticでないとき)
            /// </remarks>
            private void GetReady()
            {
                //1.クラスの型を検証
                //クラスの型指定がないとき、取得する。
                if (this._classType == null)
                {
                    if (this._className == null)
                    {
                        Xb.Util.Out("Xb.App.Class.Executer.GetReady: クラス型指定が無いとき、クラス名指定は必須です。");
                        throw new ArgumentException("Xb.App.Class.Executer.GetReady: クラス型指定が無いとき、クラス名指定は必須です。");
                    }

                    //名前空間にアプリケーションルート名を追加する。
                    //インスタンス生成時にルート名無しでは通らないため。
                    var myType = this.GetType();
                    if (myType.Namespace != null 
                        && this._className.IndexOf(myType.Namespace, StringComparison.Ordinal) == -1)
                    {
                        this._className = myType.Namespace + "." + this._className;
                    }

                    this._classType = System.Type.GetType(this._className);
                    if (this._classType == null)
                    {
                        Xb.Util.Out("Xb.App.Class.Executer.GetReady: クラス型取得に失敗しました。クラス名: " + this._className);
                        throw new ArgumentException("Xb.App.Class.Executer.GetReady: クラス型取得に失敗しました。クラス名: " + this._className);
                    }
                }

                //実行対象がクラスインスタンス生成のみの場合、メソッドの準備をしない。
                if (this._isCreateInstanceOnly)
                    return;

                //2.メソッド情報を取得
                this.GetReadyMethodInfo();

                //3.メソッド引数を検証
                this.GetReadyMethodParams();

                //4.クラスインスタンス生成可否チェック
                if (!this._isMethodStatic & this._classInstance == null)
                {
                    if (!Xb.App.Class.IsClassCreatable(this._classType, this._classArgs))
                    {
                        Xb.Util.Out("Xb.App.Class.Executer.GetReady: 渡し値クラスのインスタンス生成が出来ません。");
                        throw new ArgumentException("Xb.App.Class.Executer.GetReady: 渡し値クラスのインスタンス生成が出来ません。");
                    }
                }

            }


            /// <summary>
            /// メソッド情報を検証する。
            /// </summary>
            /// <remarks></remarks>
            private void GetReadyMethodInfo()
            {
                if (this._methodArgs == null)
                    this._methodArgs = new object[]{};

                //クラスの全メソッド情報を取得
                System.Reflection.MethodInfo[] methods = this._classType.GetMethods();
                System.Reflection.ParameterInfo[] @params = null;

                //破棄可能フラグを初期化
                this._isClassDisposable = false;

                //メソッドの数分ループ
                foreach (System.Reflection.MethodInfo info in methods)
                {
                    //Disposeメソッドを検出したとき、破棄可能フラグをONにする。
                    if (info.Name == "Dispose")
                        this._isClassDisposable = true;

                    //メソッド名が指定値と異なるとき、NG
                    if (info.Name != this._methodName)
                        continue;

                    //メソッドの全引数情報を取得
                    @params = info.GetParameters();

                    //メソッドが渡し値で実行可能か否かを調べる。
                    if (Xb.App.Class.HasArgsCompatibility(@params, this._methodArgs) & this._methodInfo == null)
                    {
                        //最初に検出した実行可能メソッドを実行対象にする。
                        this._methodInfo = info;
                        this._isMethodStatic = info.IsStatic;
                    }
                }

                if (this._methodInfo == null)
                {
                    Util.Out("Xb.App.Class.Executer.GetReadyMethodInfo: 実行可能なメソッドが見つかりませんでした。");
                    throw new ArgumentException("Xb.App.Class.Executer.GetReadyMethodInfo: 実行可能なメソッドが見つかりませんでした。");
                }
            }


            /// <summary>
            /// メソッド引数情報を検証する。
            /// </summary>
            /// <remarks></remarks>
            private void GetReadyMethodParams()
            {
                this._methodParamInfo = new List<ParameterInfo>();

                //メソッドの引数情報を取得する。
                System.Reflection.ParameterInfo[] paramInfos = this._methodInfo.GetParameters();

                if ((paramInfos != null) && paramInfos.Length > 0)
                {
                    this._methodParamInfo.AddRange(paramInfos);
                }

                this._methodType = this._methodInfo.ReturnType;

                if (this._methodArgs == null || this._methodArgs.Length < this._methodParamInfo.Count)
                {
                    Array.Resize(ref this._methodArgs, this._methodParamInfo.Count);
                }

                //値セット前に引数個数と型チェックを行う。
                //メソッドの引数情報が存在しないとき、メソッド引数はObject型の空配列にする。
                for (int i = 0; i <= this._methodParamInfo.Count - 1; i++)
                {
                    //オプショナル引数が渡されていないとき、デフォルト値をセットする。
                    if (this._methodArgs[i] == null && this._methodParamInfo[i].IsOptional)
                    {
                        this._methodArgs[i] = this._methodParamInfo[i].DefaultValue;
                    }

                    //引数が規定パラメータの型にキャスト可能か否かを検証する。
                    if (!Xb.Type.Util.IsConvertable(this._methodArgs[i], this._methodParamInfo[i].ParameterType))
                    {
                        Util.Out("App.Class.Executer.GetReadyMethodParams: Parameter Cast Error");
                        throw new ApplicationException("Parameter Cast Error");
                    }
                }

            }


            /// <summary>
            /// メソッドを実行する
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// 注意：通常のメソッドコールと同じく、呼び出し元スレッドと同じスレッドで動作する。
            /// TODO: 実行結果を標準出力に書き出すようにしたい。
            /// </remarks>
            public object Execute()
            {
                object result = null;
                //クラスインスタンスを生成する。
                if (!this._isMethodStatic & this._classInstance == null)
                {
                    try
                    {
                        //渡し値クラスのインスタンスを生成する。
                        //  (=InvokeMemberで"New"メソッドを実行している。)
                        this._classInstance = this._classType.InvokeMember(null, System.Reflection.BindingFlags.CreateInstance, null, null, this._classArgs);
                    }
                    catch (Exception ex)
                    {
                        Util.Out("App.Class.Executer.Execute: " + ex.Message);
                        throw ex.InnerException;
                    }
                }


                //メソッドを実行する。
                if (!this._isCreateInstanceOnly)
                {
                    //メソッド実行時はメソッドの戻り値を返す。
                    result = this._classType.InvokeMember(this._methodName, System.Reflection.BindingFlags.InvokeMethod, null, this._classInstance, this._methodArgs);
                }
                else
                {
                    //インスタンス生成のみの場合は、インスタンスを返す。
                    result = this._isCreateInstanceOnly;
                }

                return result;
            }


            /// <summary>
            /// 実行するメソッド／引数を指定し、検証する。
            /// </summary>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <remarks></remarks>

            public void SetMethod(string methodName, object[] methodArgs = null)
            {
                this._methodName = methodName;
                this._methodArgs = methodArgs;

                //2.メソッド情報を取得
                this.GetReadyMethodInfo();

                //3.メソッド引数を検証
                this.GetReadyMethodParams();
            }

            /// <summary>
            /// 引数付きメソッドを実行する
            /// </summary>
            /// <param name="methodName"></param>
            /// <param name="methodArgs"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public object Execute(string methodName, object[] methodArgs = null)
            {
                this.SetMethod(methodName, methodArgs);

                return this.Execute();
            }


            #region "IDisposable Support"
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
                        //インスタンスメソッドのとき
                        if ((!this._isMethodStatic))
                        {
                            //Dispose可能なとき、試行する。
                            if ((this._isClassDisposable))
                            {
                                try
                                {
                                    this._classType.InvokeMember("Dispose", System.Reflection.BindingFlags.InvokeMethod, null, this._classInstance, null);
                                }
                                catch (Exception)
                                {
                                }
                            }

                            this._classInstance = null;
                        }
                    }
                }
                this._disposedValue = true;
            }

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
