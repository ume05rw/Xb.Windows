using System.Collections.Generic;

namespace Xb.App
{

    /// <summary>
    /// コマンドライン引数をパースして保持するクラス
    /// </summary>
    /// <remarks></remarks>
    public class Arguments
    {
        public const string KeyNotFound = "KEY-NOT-FOUND";

        private Dictionary<string, List<string>> _argumens;

        /// <summary>
        /// 引数リスト
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public Dictionary<string, List<string>> Values
        {
            get { return this._argumens; }
        }


        //C#、引き数付きプロパティが作れない。
        ///// <summary>
        ///// 指定キーのアイテム
        ///// </summary>
        ///// <param name="key"></param>
        ///// <value></value>
        ///// <returns></returns>
        ///// <remarks></remarks>
        //public List<string> Item
        //{
        //    get
        //    {
        //        if ((this._argumens == null || key == null || !this._argumens.ContainsKey(key)))
        //            return null;

        //        return this._argumens[key];
        //    }
        //}


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>
        public Arguments(string[] args = null)
        {
            //引数リストオブジェクトを生成・保持しておく
            this._argumens = new Dictionary<string, List<string>>();

            //渡し値に引数文字列配列があるとき、パースする。
            if (args != null)
                this.ParseArguments(args);
        }


        /// <summary>
        /// コマンドライン引数の文字列配列を、Key-Valueセットにパースする。
        /// </summary>
        /// <param name="args"></param>
        /// <remarks></remarks>

        public void ParseArguments(string[] args)
        {
            string currentKey = null;

            //コマンドライン引数が取得出来なかったとき、空の文字列配列を生成しておく。
            if (args == null)
                args = new string[] {};

            foreach (string arg in args)
            {
                //キー Or 値 で分岐する。
                if (arg.IndexOf("-") == 0)
                {
                    //キーが検出されたとき
                    currentKey = arg;

                    //既に応答値に保持中のキーと同じものであるか否かで分岐
                    if ((!this._argumens.ContainsKey(currentKey)))
                    {
                        //保持していないキーのとき、応答値にキー、値Listを生成する。
                        this._argumens.Add(currentKey, new List<string>());
                    }
                    else
                    {
                        //既に検出済みキーと重複したとき、カレントキーを更新するのみ、応答値に変更なし。
                    }
                }
                else
                {
                    //値が検出されたとき

                    //キーが検出されないまま値を取得したとき、固定値をカレントキーにセットしてキー・値Listを生成する。
                    if ((currentKey == null))
                    {
                        currentKey = KeyNotFound;
                        this._argumens.Add(currentKey, new List<string>());
                    }
                    //カレントキーのListに値を追加する。
                    this._argumens[currentKey].Add(arg);
                }
            }
        }


        /// <summary>
        /// コマンドライン引数値で、同じ意味合いの値をマージする。
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <param name="slaveKeys"></param>
        /// <remarks></remarks>

        public void MergeArguments(string primaryKey, string[] slaveKeys)
        {
            if (this._argumens == null 
                || primaryKey == null 
                || slaveKeys == null 
                || !this._argumens.ContainsKey(primaryKey))
                return;

            foreach (string key in slaveKeys)
            {
                if (!this._argumens.ContainsKey(key))
                    continue;

                this._argumens[primaryKey].AddRange(this._argumens[key].ToArray());
                this._argumens.Remove(key);
            }
        }


        /// <summary>
        /// 指定キーの値が存在するか否かを検証する。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Has(string key)
        {
            if (this._argumens == null
                || key == null
                || !this._argumens.ContainsKey(key)
                || this._argumens[key].Count <= 0)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 指定キーの先頭の値を取り出す。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Find(string key)
        {

            if (this._argumens == null
                || key == null
                || !this._argumens.ContainsKey(key)
                || this._argumens[key].Count <= 0)
            {
                return null;
            }

            return this._argumens[key][0];
        }


        /// <summary>
        /// 指定キーの全ての値を取り出す。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[] FindAll(string key)
        {

            if (this._argumens == null
                || key == null
                || !this._argumens.ContainsKey(key)
                || this._argumens[key].Count <= 0)
            {
                return null;
            }

            return this._argumens[key].ToArray();

        }


        /// <summary>
        /// キーを追加する。
        /// </summary>
        /// <param name="key"></param>
        /// <remarks>
        /// 同じキーを何度もセット可能、値は重複しない。
        /// </remarks>

        public void SetKey(string key)
        {
            if (this._argumens == null)
                this._argumens = new Dictionary<string, List<string>>();

            if (this._argumens.ContainsKey(key))
                return;

            this._argumens.Add(key, new List<string>());
        }


        /// <summary>
        /// キー、値を追加する。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public void SetKeyValue(string key, string value)
        {
            if (this._argumens == null)
                this._argumens = new Dictionary<string, List<string>>();

            if (this._argumens.ContainsKey(key))
            {
                this._argumens[key].Add(value);
            }
            else
            {
                this._argumens.Add(key, new List<string>());
                this._argumens[key].Add(value);
            }
        }


        /// <summary>
        /// キー、値を追加する。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <remarks></remarks>
        public void SetKeyValue(string key, string[] values)
        {
            foreach (string val in values)
            {
                this.SetKeyValue(key, val);
            }
        }


        /// <summary>
        /// 引数文字列を生成する。
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetArgString()
        {
            string result = "";
            if (this._argumens == null)
                return result;


            foreach (string key in this._argumens.Keys)
            {
                if (key == null || key != App.Arguments.KeyNotFound)
                {
                    result += (key.IndexOf(" ") != -1 ? "\"" + key + "\"" : key) + " ";
                }

                foreach (string val in this._argumens[key])
                {
                    if (val == null)
                        continue;

                    result += (val.IndexOf(" ") != -1 ? "\"" + val + "\"" : val) + " ";
                }
            }

            return result;
        }
    }
}
