using System;
using System.Collections;

namespace Xb.File
{
    /// <summary>
    /// NEMA形式GPSデータファイル用ユーティリティクラス
    /// </summary>
    /// <remarks></remarks>
    public class Gps : IDisposable
    {
        /// <summary>
        /// 位置明細構造体
        /// </summary>
        /// <remarks></remarks>
        public struct GpsBody
        {
            public TimeSpan StartTime;
            public TimeSpan ArriveTime;
            public double StartLatitude;
            public double StartLongitude;
            public double ArriveLatitude;
            public double ArriveLongitude;
            public TimeSpan StayTime;
        }


        /// <summary>
        /// 位置情報構造体
        /// </summary>
        /// <remarks></remarks>
        public struct Point
        {
            public decimal Latitude;
            public decimal Longitude;
        }


        /// <summary>
        /// 平均値取得用クラス
        /// </summary>
        /// <remarks></remarks>
        private sealed class LocationAverage : IDisposable
        {
            private bool _isFirstSet;
            private int _count;
            private int _maxCount;
            private decimal[] _latitude;
            private decimal[] _longitude;
            private decimal _avgLat;

            private decimal _avgLng;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <remarks></remarks>
            public LocationAverage()
            {
                this._isFirstSet = true;
                this._count = 0;
                this._maxCount = 10;
                this._avgLat = 0;
                this._avgLng = 0;
                this._latitude = new decimal[11];
                this._longitude = new decimal[11];
            }


            /// <summary>
            /// 平均値取得用に保持する過去の緯度・経度件数
            /// </summary>
            /// <value></value>
            /// <remarks></remarks>
            public int MaxCount
            {
                //Get
                //    Return Me._maxCount
                //End Get
                set
                {
                    if (value < 0)
                        value = 0;
                    this._maxCount = value;
                    this._latitude = new decimal[this._maxCount + 1];
                    this._longitude = new decimal[this._maxCount + 1];
                }
            }


            /// <summary>
            /// 保持件数分の緯度平均値
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public decimal AvgLatitude
            {
                get { return this._avgLat; }
            }


            /// <summary>
            /// 保持権数分の経度平均値
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public decimal AvgLongitude
            {
                get { return this._avgLng; }
            }


            /// <summary>
            /// 緯度・経度をセットする
            /// </summary>
            /// <param name="latitude"></param>
            /// <param name="longitude"></param>
            /// <remarks></remarks>
            public void SetLocation(decimal latitude, decimal longitude)
            {
                //最初の値セットのとき、配列全てに渡し値をセットする。
                if (this._isFirstSet)
                {
                    for (int i = 0; i <= this._maxCount; i++)
                    {
                        this._latitude[i] = latitude;
                        this._longitude[i] = longitude;
                    }
                    this._isFirstSet = false;
                }

                //配列のカレント行に値をセットし、カレント行を進める
                this._latitude[this._count] = latitude;
                this._longitude[this._count] = longitude;
                this._count += 1;
                if (this._count > this._maxCount)
                    this._count = 0;

                //過去 iMaxCount 回数分の、緯度・経度の平均値を計算する
                this._avgLat = 0;
                this._avgLng = 0;
                for (int i = 0; i <= this._maxCount; i++)
                {
                    this._avgLat += this._latitude[i];
                    this._avgLng += this._longitude[i];
                }
                this._avgLat = this._avgLat / (this._maxCount + 1);
                this._avgLng = this._avgLng / (this._maxCount + 1);
            }


            #region " IDisposable Support "
            // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
            public void Dispose()
            {
                // ReSharper disable GCSuppressFinalizeForTypeWithoutDestructor
                GC.SuppressFinalize(this);
                // ReSharper restore GCSuppressFinalizeForTypeWithoutDestructor
            }
            #endregion
        }


        /// <summary>
        /// 移動判定状態区分
        /// </summary>
        /// <remarks></remarks>
        public enum Action
        {

            /// <summary>
            /// 移動中
            /// </summary>
            /// <remarks></remarks>
            Move,

            /// <summary>
            /// 移動開始評価中
            /// </summary>
            /// <remarks></remarks>
            MoveEval,

            /// <summary>
            /// 停止開始評価中
            /// </summary>
            /// <remarks></remarks>
            StayEval,

            /// <summary>
            /// 停止中
            /// </summary>
            /// <remarks></remarks>
            Stay
        }


        //アプリケーション設定値を読み込む
        //移動/停止確定基準緯度・経度値
        private const double PcdGpsPositionCommitLine = 0.0015;
        //移動/停止確定基準時間
        private const Int16 PciGpsTimeCommitLine = 3;

        private const double PcdGpsPositionTraceCommitLine = 0.0005;
        private DateTime _registDate;
        private GpsBody[] _body;
        private ArrayList _pointArray;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <remarks></remarks>
        public Gps()
        {
            this._body = new GpsBody[1];
        }


        /// <summary>
        /// 登録日付
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DateTime RegistDate
        {
            get { return _registDate; }
        }


        /// <summary>
        /// GPSファイルを読み込み、時刻・緯度・経度データのみを構造体配列に代入する
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool GetGpsData(string fileName)
        {
            System.IO.StreamReader reader = null;
            string[] elem = null;
            string line = null;
            string dateString = null;
            string timeString = null;
            int posIndex = 0;
            int lineIndex = 0;
            Action act = default(Action);
            decimal tmpLatitude = default(decimal);
            decimal tmpLongitude = default(decimal);
            decimal commitLatitude = default(decimal);
            decimal commitLongitude = default(decimal);
            decimal evalLatitude = default(decimal);
            decimal evalLongitude = default(decimal);
            DateTime tmpTime = default(DateTime);
            DateTime evalTime = default(DateTime);
            LocationAverage compLocation = null;
            Point tmpPoint = default(Point);

            //commitLatitude, commitLongitude        '移動開始・終了確定位置
            //evalLatitude,   evalLongitude          '移動終了判定用
            //evalTime                               '判定開始時刻保存用

            posIndex = 1;
            lineIndex = 0;
            act = Action.Stay;
            _pointArray = new ArrayList();
            //地図上へ移動軌跡を表示するための、地点記録配列

            tmpPoint = new Point();
            tmpPoint.Latitude = 0;
            tmpPoint.Longitude = 0;

            //過去10回分の緯度・経度を保持し、平均値を計算する
            //平均値計算はクラスlocationAverage　で行う。
            compLocation = new LocationAverage();
            compLocation.MaxCount = 10;

            _body = new GpsBody[2];

            //ファイルの存在チェック
            if ((!System.IO.File.Exists(fileName)))
            {
                return false;
            }

            //ファイルを開き、読み込みループ
            reader = new System.IO.StreamReader(fileName, System.Text.Encoding.Default);

            do
            {
                //１行分読み込み、データが無くなったらループを出る
                line = reader.ReadLine();
                if (line == null)
                    break;

                //位置データ行か否かの判定
                if (line.IndexOf("$GPRMC") < 0)
                    continue;

                //文字列の判定、位置データの行のとき、時刻・緯度・経度を構造体配列に代入する
                elem = line.Split(',');

                //フォーマットチェック
                if (elem.Length < 10)
                    continue;

                //位置、時間、速度のデータを読み取る
                try
                {
                    //今回の位置、時刻情報取得
                    tmpLatitude = ChangePositionData(elem[3]);
                    //緯度
                    tmpLongitude = ChangePositionData(elem[5]);
                    //経度

                    //緯度・経度情報を平均計算クラスへ渡す
                    compLocation.SetLocation(tmpLatitude, tmpLongitude);

                    //日付・時刻情報を取得する
                    dateString = elem[9].PadLeft(6, '0');
                    timeString = elem[1].PadLeft(6, '0');

                    //日時
                    tmpTime = DateTime.Parse("20" + dateString.Substring(4, 2) 
                                             + "/" + dateString.Substring(2, 2) 
                                             + "/" + dateString.Substring(0, 2) 
                                             + " " + timeString.Substring(0, 2) 
                                             + ":" + timeString.Substring(2, 2) 
                                             + ":" + timeString.Substring(4, 2));

                    //グリニッジ標準時間から9時間プラス
                    tmpTime = tmpTime.AddHours(9);

                    //最初のデータ行抽出時、位置・時間の比較元情報を取得する
                    if ((lineIndex == 0))
                    {
                        commitLatitude = tmpLatitude;
                        commitLongitude = tmpLongitude;
                    }

                    //移動トレース用基準点をセットする。
                    if ((tmpPoint.Latitude == 0 | tmpPoint.Longitude == 0))
                    {
                        tmpPoint.Latitude = tmpLatitude;
                        tmpPoint.Longitude = tmpLongitude;
                    }

                    if ((_registDate == null))
                        _registDate = tmpTime;
                }
                catch (Exception)
                {
                    continue;
                }


                if ((double)Math.Abs(tmpPoint.Latitude - tmpLatitude) > PcdGpsPositionTraceCommitLine 
                    || (double)Math.Abs(tmpPoint.Longitude - tmpLongitude) > PcdGpsPositionTraceCommitLine)
                {
                    _pointArray.Add(tmpPoint);
                    tmpPoint = new Point();
                    tmpPoint.Latitude = tmpLatitude;
                    tmpPoint.Longitude = tmpLongitude;
                }

                //読み込み行数を加算
                lineIndex += 1;

                //移動状況の判定
                switch (act)
                {
                    case Action.Move:
                        //動作状態が移動中のとき
                        //*************************
                        //***** 移動停止評価判定
                        //*************************

                        //前回確定位置と今回位置との差が、誤差範囲外

                        if ((double)Math.Abs(commitLatitude - tmpLatitude) > PcdGpsPositionCommitLine 
                            || (double)Math.Abs(commitLongitude - tmpLongitude) > PcdGpsPositionCommitLine)
                        {
                            act = Action.StayEval;
                            // -> 移動停止判定を開始

                            //評価開始時点の位置・時刻を保持
                            evalLatitude = tmpLatitude;
                            evalLongitude = tmpLongitude;
                            evalTime = tmpTime;
                        }
                        else
                        {
                            continue;
                        }

                        break;
                    case Action.StayEval:
                        //動作状態が停止評価中のとき
                        //*************************
                        //***** 移動停止確定判定
                        //*************************


                        if (
                            ((double)Math.Abs(commitLatitude - tmpLatitude) > PcdGpsPositionCommitLine 
                                || (double)Math.Abs(commitLongitude - tmpLongitude) > PcdGpsPositionCommitLine) 
                            && ((double)Math.Abs(evalLatitude - tmpLatitude) < PcdGpsPositionCommitLine 
                                && (double)Math.Abs(evalLongitude - tmpLongitude) < PcdGpsPositionCommitLine))
                        {
                            //   前回確定位置と今回評価位置との差が、誤差範囲外  -> 前回確定位置から移動している
                            //   停止評価開始位置と今回位置との差が、誤差範囲内  -> 評価開始位置から移動していない

                            if (Math.Abs(evalTime.Subtract(tmpTime).Minutes) >= PciGpsTimeCommitLine)
                            {
                                //判定開始時間から、「pdWaitingTimeLine」分以上経過した。 ->移動停止確定
                                act = Action.Stay;

                                //停止 - 到着点を記録
                                _body[posIndex].ArriveLatitude = (double)evalLatitude;
                                _body[posIndex].ArriveLongitude = (double)evalLongitude;
                                _body[posIndex].ArriveTime = TimeSpan.Parse(evalTime.Hour + ":" + evalTime.Minute);

                                //確定位置を記録
                                commitLatitude = evalLatitude;
                                commitLongitude = evalLongitude;

                                //最大値のカウントを加算
                                posIndex += 1;
                                Array.Resize(ref _body, posIndex + 1);
                            }
                            else
                            {
                                //所定時間を経過していない   ->停止判定を続ける
                                continue;
                            }
                        }
                        else
                        {
                            //所定時間内に、
                            //   １．前回確定位置と今回位置との差が、誤差範囲内になった。 もしくは、
                            //   ２．停止評価位置から離れた
                            //       -> 信号待ちなどの一時停止 or ＧＰＳの誤差と判断、移動フラグを移動中に戻す

                            act = Action.Move;
                            continue;
                        }
                        break;
                    case Action.Stay:
                        //動作状態が停止中のとき
                        //*************************
                        //***** 移動開始評価判定
                        //*************************

                        if ((double)Math.Abs(compLocation.AvgLatitude - tmpLatitude) > PcdGpsPositionCommitLine 
                            || (double)Math.Abs(compLocation.AvgLongitude - tmpLongitude) > PcdGpsPositionCommitLine)
                        {
                            //   前回確定位置と今回位置との差が、誤差範囲外
                            act = Action.MoveEval;
                            //->移動開始判定を開始

                            //評価開始時点の時刻を保持
                            evalTime = tmpTime;
                            commitLatitude = compLocation.AvgLatitude;
                            commitLongitude = compLocation.AvgLongitude;
                        }
                        else
                        {
                            //動作フラグと移動速度が矛盾していない　→　フラグと同じ動作を継続中　記録しない
                            continue;
                        }
                        break;
                    case Action.MoveEval:
                        //動作状態が移動開始判定中のとき
                        //*************************
                        //***** 移動開始確定判定
                        //*************************


                        if ((double)Math.Abs(commitLatitude - tmpLatitude) > PcdGpsPositionCommitLine 
                            || (double)Math.Abs(commitLongitude - tmpLongitude) > PcdGpsPositionCommitLine)
                        {
                            //   前回確定位置と今回位置との差が、誤差範囲外
                            if (Math.Abs(evalTime.Subtract(tmpTime).Minutes) >= PciGpsTimeCommitLine)
                            {
                                //判定開始時間から、「pdWaitingTimeLine」分以上経過した。 ->移動開始確定
                                act = Action.Move;

                                //移動開始 - 出発点を記録
                                _body[posIndex].StartLatitude = (double)commitLatitude;
                                _body[posIndex].StartLongitude = (double)commitLongitude;
                                _body[posIndex].StartTime = TimeSpan.Parse(evalTime.Hour + ":" + evalTime.Minute);

                                //初回の移動開始でないとき->滞在時間を記録
                                if ((posIndex != 1 & _body[posIndex].StartTime != TimeSpan.Parse("00:00")))
                                {
                                    if ((_body[posIndex - 1].ArriveTime < _body[posIndex].StartTime))
                                    {
                                        _body[posIndex - 1].StayTime = _body[posIndex].StartTime - _body[posIndex - 1].ArriveTime;
                                    }
                                    else
                                    {
                                        _body[posIndex - 1].StayTime = _body[posIndex].StartTime - _body[posIndex - 1].ArriveTime + TimeSpan.Parse("1.00:00");
                                    }
                                }
                            }
                            else
                            {
                                //所定時間を経過していない   ->停止判定を続ける
                                continue;
                            }
                        }
                        else
                        {
                            //所定時間内に、
                            //   １．前回確定位置と今回位置との差が、誤差範囲内になった。
                            //       -> ＧＰＳの誤差と判断、移動フラグを停止中に戻す
                            act = Action.Stay;
                            continue;
                        }
                        break;
                }
            } while (true);

            //ストリームリーダを閉じる
            reader.Close();
            reader.Dispose();

            //移動地点が計測出来ないとき、NGを返す
            if (posIndex == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// NEMA形式の緯度・経度データ数値の、分以下の数値を10進数に変換する
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private decimal ChangePositionData(string dataString)
        {
            int pointPos = dataString.IndexOf(".");
            decimal result, tmp;

            //渡し値（文字列）が数字がどうかを判定する
            //「3511.23523」のような文字列が渡される。
            if(!decimal.TryParse(dataString, out tmp))
            {
                return 0;
            }

            //小数点が存在するか
            if (pointPos == -1)
            {
                return Convert.ToDecimal(dataString);
            }

            tmp = Convert.ToDecimal(dataString.Substring(pointPos - 3));
            tmp = tmp / 60;
            result = Convert.ToDecimal(dataString.Substring(0, (pointPos - 3))) + tmp;

            return result;
        }


        #region " IDisposable Support "
        // このコードは、破棄可能なパターンを正しく実装できるように Visual Basic によって追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(ByVal disposing As Boolean) に記述します。
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}
