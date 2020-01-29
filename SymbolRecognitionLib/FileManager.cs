using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        public static FileStream Decompress(string filePath)
        {
            FileInfo fileToDecompress = new FileInfo(filePath); 

            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                FileStream decompressedFileStream = new FileStream(filePath + ".temp", FileMode.Create, FileAccess.ReadWrite);

                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedFileStream);
                }

                decompressedFileStream.Seek(0, SeekOrigin.Begin);
                return decompressedFileStream;
            }
        }

        public static void SaveFile(object obj, string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
            }
        }

        public static object LoadFile(string filePath)
        {
            Stream fileStream;

            if (filePath.Contains(".gz"))
                fileStream = Decompress(filePath);
            else
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(fileStream);
        }
    }
}
