using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleDriveAPITransferCMD
{
    static class AuxFileCommCenter
    {
        public static bool Exists(string FilePath)
        {
            return File.Exists(FilePath);
        }
        public static string CreateDirIfNotExists(string DirPath)
        {
            if (!string.IsNullOrEmpty(DirPath))
            {
                if (File.Exists(DirPath))
                {
                    DirPath = DirPath.Remove(DirPath.LastIndexOf('\\')+1);
                }

                if (!Directory.Exists(DirPath))
                {
                    var info = Directory.CreateDirectory(DirPath);
                    return info.FullName;
                }
                return DirPath;
            }
            return null;
        }
        /// <summary>
        /// Adquire o nome e formato de um arquivo em um array, seguindo a ordem citada (0 para o nome e 1 para a extensão)
        /// </summary>
        /// <param name="Fullpath">Caminho completo do arquivo</param>
        /// <returns>Retorna o nome e formato dentro de um array, caso o arquivo não exista, retorna null</returns>
        public static string[] GetFileNameOnlyWithFormat(string Fullpath)
        {
            FileInfo fi = new FileInfo(Fullpath);
            string filename = fi.Name;
            string format = fi.Extension;

            return new string[] { filename, format };
        }

        /// <summary>
        /// Procura o ultimo arquivo criado em um diretório, utilizando a ultima data de alteração do mesmo.
        /// </summary>
        /// <param name="DirPath">Diretório onde se encontram os arquivos a serem selecionados (não suporta subdiretórios)</param>
        /// <returns>O último arquivo localizado por data de atualização, caso não encontre arquivos válidos, retorna nulo</returns>
        public static string FindFileThroughSystem(string DirPath)
        {
            if (Directory.Exists(DirPath))
            {
                string[] dirs = Directory.GetFiles(DirPath);
                string lastfile = string.Empty;

                List<FileInfo> fslist = new List<FileInfo>();

                foreach (string dir in dirs)
                {
                    FileInfo fi = new FileInfo(dir);
                    fslist.Add(fi);
                }

                if (fslist.Count > 0)
                {
                    fslist = fslist.OrderBy(x => x.LastWriteTime).ToList();
                    lastfile = fslist.Last().FullName;
                }
                else
                    return null;

                return lastfile;
            }
            else
                return null;
        }

        /// <summary>
        /// Ajusta o caminho para ter o nome do arquivo concatenado corretamente
        /// </summary>
        /// <param name="Path">Caminho do diretório do arquivo</param>
        /// <param name="Filename">O nome do arquivo com o respectivo formato</param>
        /// <returns>O caminho concatenado</returns>
        public static string AdjustPath(string Path, string Filename)
        {
            if (Path.Length >= 2)
            {
                string final = Path.Remove(0, Path.Length - 1);
                if (final != "\\")
                    Path += "\\";
            }

            return string.Format("{0}{1}", Path, Filename);

        }

        /// <summary>
        /// Encontra um arquivo seja com o nome diretamente passado ou o com a ultima data em seu nome.
        /// </summary>
        /// <param name="DirPath">Diretório onde se encontram os arquivos</param>
        /// <param name="FileName">(Opcional) O nome do arquivo procurado</param>
        /// <returns>Retorna o caminho do arquivo se o mesmo existir, caso contrário retorna null</returns>
        public static string FindFileThroughName(string DirPath, string FileName = "")
        {

            DateTime oggi = DateTime.Now;
            DateTime ieri = DateTime.Now.AddDays(-1);

            if (Directory.Exists(DirPath))
            {
                if (File.Exists(AdjustPath(DirPath, FileName)))
                {
                    return new FileInfo(AdjustPath(DirPath, FileName)).FullName;
                }
                return null;
            }
            return null;
        }

        public static string GuessMime(string format)
        {
            if (!string.IsNullOrEmpty(format))
            {
                IEnumerable<KeyValuePair<string, string>> res = SomeMimeTypes._mappings.Where(x => x.Key == format);
                if (res.Count() > 0)
                    return res.First().Value;
            }

            return "application/octet-stream";
        }
    }
}
