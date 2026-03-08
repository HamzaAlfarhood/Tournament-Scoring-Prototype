using System;
using System.Windows.Forms;

namespace Tournament_Scoring_Prototype
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // تشغيل النافذة الرئيسية
            Application.Run(new MainForm());
        }
    }
}