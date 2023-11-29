using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopPriceUploader.WDM
{

    [ComImport, Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a")]
    public class CVirtualDesktopManager { }

    class VirtualDesktopManager
    {
        public VirtualDesktopManager()
        {
            cmanager = new CVirtualDesktopManager();
            manager = (IVirtualDesktopManager)cmanager;
        }
        ~VirtualDesktopManager()
        {
            manager = null;
            cmanager = null;
        }

        private CVirtualDesktopManager cmanager = null;
        private IVirtualDesktopManager manager;

        public bool IsWindowOnCurrentVirtualDesktop(IntPtr TopLevelWindow)
        {
            int hr;
            int result;
            if ((hr = manager.IsWindowOnCurrentVirtualDesktop(TopLevelWindow, out result)) != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return result != 0;
        }

        public Guid GetWindowDesktopId(IntPtr TopLevelWindow)
        {
            int hr;
            Guid result;
            if ((hr = manager.GetWindowDesktopId(TopLevelWindow, out result)) != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return result;
        }

        public void MoveWindowToDesktop(IntPtr TopLevelWindow, Guid CurrentDesktop)
        {
            int hr;
            if ((hr = manager.MoveWindowToDesktop(TopLevelWindow, CurrentDesktop)) != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }
    }


    public class NewWindow : Form
    {
    }

}
