using Microsoft.Practices.Unity;
using System;
using System.Windows.Forms;

namespace Occ.Client.WinForm
{
    public static class Program
    {
        private static ClientSystem system = null;

        public static void InitializeSystem(string name) => system = new ClientSystem(name);

        public static IUnityContainer Container => system.Container;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
