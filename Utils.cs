using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace WordLearner
{
    class Utils
    {
        public static string dialog(string options)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = options;


            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                return dlg.FileName;
            }

            return "";
        }
       
    }
}
