using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ECR_Test_Application
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new ECR_Test_Application());
            }
            catch (Exception ex)
            {
                DialogResult result = MessageBox.Show("Service not started yet.", "Service Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    Application.Run(new ECR_Test_Application());
                }

                
                Console.WriteLine(ex.ToString());
            }
           
          
        }
    }
}
