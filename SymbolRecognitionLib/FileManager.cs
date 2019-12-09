using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SymbolRecognitionLib
{
    public static class FileManager
    {
        public static string OpenFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "c:\\", string title = "Choose file")
        {
            string filePath = "";

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.Title = title;
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            return filePath;
        }

        public static string OpenSaveFileDialog(string filter = "All files (*.*)|*.*", string initialDirectory = "c:\\", string title = "Save file")
        {
            string filePath = "";

            using (System.Windows.Forms.SaveFileDialog openFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.Title = title;
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            return filePath;
        }
    }
}
