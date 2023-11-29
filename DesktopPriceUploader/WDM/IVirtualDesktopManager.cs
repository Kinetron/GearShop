using System;
using System.Runtime.InteropServices;

namespace DesktopPriceUploader.WDM
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
    [System.Security.SuppressUnmanagedCodeSecurity]
    public interface IVirtualDesktopManager
    {
        [PreserveSig]
        int IsWindowOnCurrentVirtualDesktop(
           [In] IntPtr TopLevelWindow,
           [Out] out int OnCurrentDesktop
           );
        [PreserveSig]
        int GetWindowDesktopId(
            [In] IntPtr TopLevelWindow,
            [Out] out Guid CurrentDesktop
            );

        [PreserveSig]
        int MoveWindowToDesktop(
            [In] IntPtr TopLevelWindow,
            [MarshalAs(UnmanagedType.LPStruct)]
            [In]Guid CurrentDesktop
            );
    }
}
