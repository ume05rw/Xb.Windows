using System;

namespace Xb.Type
{
    public class Marshaler
    {
        /// <summary>
        /// .Net上の文字列(UTF16)／UTF8の相互変換マーシャラ
        /// アンマネージDLLメソッドの渡し値／戻り値文字列がUTF8のときに使用する。
        /// </summary>
        /// <remarks>
        /// <!--
        /// 使用例：
        /// <DllImport("libvlc")> _
        /// Public Shared Function libvlc_media_new_path(ByVal instanceHandle As IntPtr, _
        ///        <MarshalAs(UnmanagedType.CustomMarshaler, _
        ///        MarshalTypeRef:=GetType(Type.String.Utf8Marshaler))> ByVal url As String) As IntPtr
        ///                                                ↑コレ
        /// コピー元：
        /// http://airphone-tv.googlecode.com/svn-history/r11/trunk/Development/ZeroconfService/mDNSImports.cs
        /// -->
        /// </remarks>
        public class Utf8Marshaler : System.Runtime.InteropServices.ICustomMarshaler
        {
            //GetInstanceメソッドから取得されるインスタンス。
            public static Utf8Marshaler Marshaler = new Utf8Marshaler();

            /// <summary>
            /// Dll → .Net に戻ってくるバイト配列をUTF-8エンコードを通して文字列化する。
            /// </summary>
            /// <param name="pNativeData"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public static string GetString(IntPtr pNativeData)
            {
                if (pNativeData == IntPtr.Zero)
                {
                    return null;
                }

                int size = 0;
                while (System.Runtime.InteropServices.Marshal.ReadByte(pNativeData, size) != Convert.ToByte(0))
                {
                    size += 1;
                }

                byte[] utf8Bytes = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(pNativeData, utf8Bytes, 0, size);

                return System.Text.Encoding.UTF8.GetString(utf8Bytes);
            }


            /// <summary>
            /// マーシャラインスタンスを生成する。
            /// </summary>
            /// <param name="cookie"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public static System.Runtime.InteropServices.ICustomMarshaler GetInstance(System.String cookie)
            {
                return Marshaler;
            }

            private int _nativeDataSize = 0;


            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <remarks></remarks>
            public Utf8Marshaler()
            {
            }


            /// <summary>
            /// Dll → .Net に戻ってくるバイト配列をUTF-8エンコードを通して文字列化する。
            /// </summary>
            /// <param name="pNativeData"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                return Xb.Type.Marshaler.Utf8Marshaler.GetString(pNativeData);
            }

            /// <summary>
            /// .Net → Dllへ渡す文字列をUTF-8エンコードに変換する。
            /// </summary>
            /// <param name="managedObject"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public IntPtr MarshalManagedToNative(object managedObject)
            {
                if (managedObject == null)
                {
                    return IntPtr.Zero;
                }

                byte[] array;
                try
                {
                    array = System.Text.Encoding.UTF8.GetBytes((System.String)managedObject);
                }
                catch (Exception)
                {
                    Xb.Util.Out("Xb.Type.Marshaler.MarshalManagedToNative: ManagedObj: Can only marshal type of System.String");
                    throw new ArgumentException("Xb.Type.Marshaler.MarshalManagedToNative: ManagedObj: Can only marshal type of System.String");
                }

                _nativeDataSize = System.Runtime.InteropServices.Marshal.SizeOf(new byte()) * (array.Length + 1);

                IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(_nativeDataSize);

                System.Runtime.InteropServices.Marshal.Copy(array, 0, ptr, array.Length);
                System.Runtime.InteropServices.Marshal.WriteByte(ptr, (_nativeDataSize - System.Runtime.InteropServices.Marshal.SizeOf(new byte())), Convert.ToByte(0));

                return ptr;
            }


            /// <summary>
            /// ネイティブデータを破棄する。
            /// </summary>
            /// <param name="pNativeData"></param>
            /// <remarks></remarks>

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                System.Runtime.InteropServices.Marshal.Release(pNativeData);
            }

            /// <summary>
            /// .Netデータを破棄する。(何もしてない。)
            /// </summary>
            /// <param name="managedObj"></param>
            /// <remarks></remarks>
            public void CleanUpManagedData(object managedObj)
            {
            }


            /// <summary>
            /// Dll上のバイト配列サイズを取得する。
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public int GetNativeDataSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(typeof(byte));
            }


            /// <summary>
            /// .Net上のバイト配列サイズを取得する。
            /// </summary>
            /// <param name="ptr"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public int GetNativeDataSize(IntPtr ptr)
            {
                int size = 0;
                size = 0;
                while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, size) > 0)
                {
                    size += 1;
                }

                return size;
            }


            /// <summary>
            /// ICustomMarshaler定義のエイリアス：CleanUpManagedData
            /// </summary>
            /// <param name="managedObj"></param>
            /// <remarks></remarks>
            public void CleanUpManagedData1(object managedObj)
            {
                this.CleanUpManagedData(managedObj);
            }
            void System.Runtime.InteropServices.ICustomMarshaler.CleanUpManagedData(object managedObj)
            {
                CleanUpManagedData1(managedObj);
            }

            /// <summary>
            /// ICustomMarshaler定義のエイリアス：CleanUpNativeData
            /// </summary>
            /// <param name="pNativeData"></param>
            /// <remarks></remarks>
            public void CleanUpNativeData1(System.IntPtr pNativeData)
            {
                this.CleanUpNativeData(pNativeData);
            }
            void System.Runtime.InteropServices.ICustomMarshaler.CleanUpNativeData(System.IntPtr pNativeData)
            {
                CleanUpNativeData1(pNativeData);
            }

            /// <summary>
            /// ICustomMarshaler定義のエイリアス：GetNativeDataSize
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public int GetNativeDataSize1()
            {
                return this.GetNativeDataSize();
            }
            int System.Runtime.InteropServices.ICustomMarshaler.GetNativeDataSize()
            {
                return GetNativeDataSize1();
            }

            /// <summary>
            /// ICustomMarshaler定義のエイリアス：MarshalManagedToNative
            /// </summary>
            /// <param name="managedObj"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public System.IntPtr MarshalManagedToNative1(object managedObj)
            {
                return this.MarshalManagedToNative(managedObj);
            }
            System.IntPtr System.Runtime.InteropServices.ICustomMarshaler.MarshalManagedToNative(object managedObj)
            {
                return MarshalManagedToNative1(managedObj);
            }

            /// <summary>
            /// ICustomMarshaler定義のエイリアス：MarshalNativeToManaged
            /// </summary>
            /// <param name="pNativeData"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public object MarshalNativeToManaged1(System.IntPtr pNativeData)
            {
                return this.MarshalNativeToManaged(pNativeData);
            }
            object System.Runtime.InteropServices.ICustomMarshaler.MarshalNativeToManaged(System.IntPtr pNativeData)
            {
                return MarshalNativeToManaged1(pNativeData);
            }
        }
    }
}
