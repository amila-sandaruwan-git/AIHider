using System;
using System.Windows.Forms;

namespace AIHider
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the real AI form (hidden from screen share)
            Application.Run(new TestForm());
        }
    }
}