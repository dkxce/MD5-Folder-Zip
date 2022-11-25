//
//                    MD5-Zip-Origin
//
// Calculate MD5-Origin of files in folder (Recursive) or zip
// *** (lowercase numbers --> _ --> letters) with / ***
//
//     Т.к. в структуре zip файла присутствуют 
//     last modification time и last modification date,
//     то для одних и тех же файлов в архиве будут разные MD5.
//     По этому, высчитываем MD5 от исходных файлов.
//
// ZIP FORMAT https://en.wikipedia.org/wiki/ZIP_(file_format)#Data_descriptor
//

using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class MD5Origin
    {
        /// <summary>
        ///     Расчет MD5 файла
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string CalculateMD5File(string filename)
        {
            using (MD5 md5 = MD5.Create())
                using (FileStream stream = File.OpenRead(filename))
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        ///     Расчет MD5 директории (все файлы внутри с именами lowercase)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CalculateMD5Folder(string path)
        {
            List<string> fileList = new List<string>(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
            if (fileList.Count == 0) return null;

            MD5 md5 = MD5.Create();

            // важно для последовательного правильного подсчета (lowercase numbers-->_-->letters)            
            fileList.Sort((Comparison<string>)((string s1, string s2) => { return string.CompareOrdinal(s1.ToLower(), s2.ToLower()); }));
            // Python: fileList.sort(key = lambda f: ([str,int].index(type(f)), f.lower()))            

            for (int i = 0; i < fileList.Count; i++)
            {
                string fileCurr = fileList[i];

                // Hash of File Path
                string relativePath = fileCurr.Substring(path.Length + 1).ToLower().Replace("\\", "/");
                byte[] pathBytes = Text.Encoding.UTF8.GetBytes(relativePath);
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // Hash of Files                
                using (FileStream fs = new FileStream(fileCurr, FileMode.Open, FileAccess.Read))
                {
                    int read = 0;
                    byte[] buff = new byte[16 * 1024 * 1024]; // 16 MB                    
                    while ((read = fs.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (i == fileList.Count - 1)
                            md5.TransformFinalBlock(buff, 0, read);
                        else
                            md5.TransformBlock(buff, 0, read, buff, 0);
                    };
                };
            };
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        ///     Расчет MD5 zip архива (все файлы внутри с именами lowercase)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CalculateMD5Zip(string fileName)
        {
            using (SharpCompress.Archives.Zip.ZipArchive za = SharpCompress.Archives.Zip.ZipArchive.Open(fileName))
            {
                List<string> fileList = new List<string>();
                foreach (SharpCompress.Common.IEntry en in za.Entries)
                    if(!en.IsDirectory)
                        fileList.Add(en.Key);
                
                MD5 md5 = MD5.Create();

                // важно для последовательного правильного подсчета (lowercase numbers-->_-->letters)                
                fileList.Sort((Comparison<string>)((string s1, string s2) => { return string.CompareOrdinal(s1.ToLower(), s2.ToLower()); }));
                // Python: fileList.sort(key = lambda f: ([str,int].index(type(f)), f.lower()))                

                for (int i = 0; i < fileList.Count; i++)
                {
                    string fileCurr = fileList[i];

                    // Hash of File Path
                    string relativePath = fileCurr.ToLower().Replace("\\", "/");
                    byte[] pathBytes = Text.Encoding.UTF8.GetBytes(relativePath);
                    md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                    // Hash of Files                    
                    using (SharpCompress.Readers.IReader reader = za.ExtractAllEntries())
                    {
                        do reader.MoveToNextEntry();
                        while (reader.Entry.Key != fileCurr);

                        using (Stream ens = reader.OpenEntryStream())
                        {
                            int read = 0;
                            byte[] buff = new byte[16 * 1024 * 1024]; // 16 MB                            
                            while ((read = ens.Read(buff, 0, buff.Length)) > 0)
                            {
                                if (i == fileList.Count - 1)
                                    md5.TransformFinalBlock(buff, 0, read);
                                else
                                    md5.TransformBlock(buff, 0, read, buff, 0);
                            };
                        };
                    };
                };
                return BitConverter.ToString(md5.Hash).Replace("-", "").ToUpperInvariant();
            };
        }

        public static void Test()
        {
            string myPath = @"C:\Downloads\Test";
            string myFile = @"C:\Downloads\Test.zip";
            Console.WriteLine("MD5-Path : {0} ~{1}", MD5Origin.CalculateMD5Folder(myPath), myPath.Substring(Math.Max(0, myPath.Length - 12)));
            Console.WriteLine("MD5-Zip  : {0} ~{1}", MD5Origin.CalculateMD5Zip(myFile), myFile.Substring(Math.Max(0, myFile.Length - 12)));
            Console.WriteLine("MD5-File : {0} ~{1}", MD5Origin.CalculateMD5File(myFile), myFile.Substring(Math.Max(0, myFile.Length - 12)));
            Console.ReadLine();
        }
    }
}
