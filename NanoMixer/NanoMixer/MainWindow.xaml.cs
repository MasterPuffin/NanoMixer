using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NanoMixer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private uint fPreviousExecutionState;
        public MainWindow() {

            // Set new state to prevent system sleep
            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(
                NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            if (fPreviousExecutionState == 0) {
                Console.WriteLine("SetThreadExecutionState failed. Do something here...");
                Close();
            }


            InitializeComponent();
            Midi midi = new Midi();
            midi.run();
        }

        protected override void OnClosed(System.EventArgs e) {
            base.OnClosed(e);

            // Restore previous state
            if (NativeMethods.SetThreadExecutionState(fPreviousExecutionState) == 0) {
                // No way to recover; already exiting
            }
        }

        internal static class NativeMethods {
            // Import SetThreadExecutionState Win32 API and necessary flags
            [DllImport("kernel32.dll")]
            public static extern uint SetThreadExecutionState(uint esFlags);
            public const uint ES_CONTINUOUS = 0x80000000;
            public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        }
    }

}