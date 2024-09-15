using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace logFileAnaly
{
    public class ClsFile
    {
        public static string selectLogFile(string filter)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C\\";

            // Set the title of the dialog (optional)
            openFileDialog.Title = "Select a File";

            // Set the filter to specify the allowed file types (optional)
            //openFileDialog.Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Filter = filter;

            // Show the dialog and check if the user clicked the OK button
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Retrieve the selected file path
                string filePath = openFileDialog.FileName;
                return filePath; 

            }


            return string.Empty;
        }
    }
}
