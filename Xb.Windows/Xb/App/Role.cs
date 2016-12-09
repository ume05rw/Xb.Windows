using System;
using System.Runtime.InteropServices;

namespace Xb.App
{
    /// <summary>
    /// アカウント権限ユーティリティクラス
    /// </summary>
    /// <remarks></remarks>
    public class Role
    {
        /// <summary>
        /// 現在アプリケーションを実行しているユーザーに管理者権限があるか調べる
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// ※注意※ UACの管理者昇格状態を検証することはできない。
        /// 
        /// アプリケーションを管理者として実行しているか調べる - そのままコピペ
        /// http://dobon.net/vb/dotnet/system/isadmin.html
        /// </remarks>
        public static bool IsAdmin()
        {
            //現在のユーザーを表すWindowsIdentityオブジェクトを取得する
            System.Security.Principal.WindowsIdentity wi = System.Security.Principal.WindowsIdentity.GetCurrent();
            //WindowsPrincipalオブジェクトを作成する
            System.Security.Principal.WindowsPrincipal wp = new System.Security.Principal.WindowsPrincipal(wi);
            //Administratorsグループに属しているか調べる
            return wp.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, ref uint returnLength);


        /// <summary>
        /// アカウント権限情報区分
        /// </summary>
        /// <remarks></remarks>
        public enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }


        /// <summary>
        /// UAC権限区分
        /// </summary>
        /// <remarks></remarks>
        public enum RoleType
        {
            /// <summary>
            /// 標準ユーザー or UAC無効
            /// </summary>
            /// <remarks></remarks>
            DefaultRole = 1,

            /// <summary>
            /// UAC有効かつ管理者昇格済み
            /// </summary>
            /// <remarks></remarks>
            Full,

            /// <summary>
            /// UAC有効かつ管理者未昇格
            /// </summary>
            /// <remarks></remarks>
            Limited
        }


        /// <summary>
        /// 昇格トークンの種類を取得する
        /// </summary>
        /// <returns>
        /// 参考：
        /// UACが有効で、管理者に昇格しているか調べる - そのままコピペ
        /// http://dobon.net/vb/dotnet/system/isadmin.html
        /// 
        /// 昇格トークンの種類を示すTOKEN_ELEVATION_TYPE。
        /// 取得に失敗した時でもTokenElevationTypeDefaultを返す。
        /// </returns>
        public static RoleType GetRoleType()
        {
            RoleType returnValue = RoleType.DefaultRole;

            //Windows Vista以上か確認
            if ((Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6))
            {
                return returnValue;
            }

            const RoleType tet = RoleType.DefaultRole;
            uint returnLength = 0;
            uint tetSize = Convert.ToUInt32(Marshal.SizeOf(Convert.ToInt32(tet)));
            IntPtr tetPtr = Marshal.AllocHGlobal(Convert.ToInt32(tetSize));

            try
            {
                //アクセストークンに関する情報を取得
                if (GetTokenInformation(System.Security.Principal.WindowsIdentity.GetCurrent().Token, TokenInformationClass.TokenElevationType, tetPtr, tetSize, ref returnLength))
                {
                    //結果を取得
                    returnValue = (RoleType)Marshal.ReadInt32(tetPtr);
                }
            }
            finally
            {
                //解放する
                Marshal.FreeHGlobal(tetPtr);
            }

            return returnValue;
        }
    }
}
