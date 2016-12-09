using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Xb.App
{

    /// <summary>
    /// スレッド管理ユーティリティクラス
    /// </summary>
    /// <remarks>
    /// App.Process.Execterと名前を合わせるために作る。
    /// 機能を追加するかは未定。
    /// カレントプロセスが保持中のスレッドを取ってくるとか、キックしたスレッドを複数管理するとか...？
    /// 機能が欲しくなったら実装する。
    /// </remarks>
    public partial class Thread
    {
        /// <summary>
        /// 汎用コールバック定義
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <remarks></remarks>
        public delegate void EventDelegate(object value1, object value2);
    }
}
