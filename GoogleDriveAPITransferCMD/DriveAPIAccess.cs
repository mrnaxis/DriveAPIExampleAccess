using Google.Apis.Drive.v3;
using GoogleDriveAPITransferCMD.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveAPITransferCMD
{
    class DriveAPIAccess
    {
        public async Task DoUpload(string PathFile, string GDriveFolder = null, string MimeType = "application/octet-stream")
        {
            Console.WriteLine("Iniciando Upload");

            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS);

            string GDriveFolderID = null;
            if (!string.IsNullOrEmpty(GDriveFolder))
            {
                GDriveFolderID = await CreateGDriveDir(GDriveFolder);
            }

            await dc.UploadFile(PathFile, GDriveFolderID, MimeType);
        }

        public async Task ShowFiles()
        {

            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS);

            dc.FindFiles();
        }

        public async Task<string> GetFileID(string PartialOrFullFileName = "")
        {
            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS);

            Dictionary<string, string> Files = dc.FindFiles(false);
            if (Files.Count > 0)
            {
                List<string> FileID = (from f in Files
                                       where f.Value.Contains(PartialOrFullFileName)
                                       select f.Key).ToList();

                if (FileID.Count >= 1)
                    return FileID[0];
            }

            return null;
        }

        public async Task DoDownload(string FileName, string PathForDownload = null ,string FileID = "")
        {
            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS); 
            string OnlyFileName = AuxFileCommCenter.GetFileNameOnlyWithFormat(FileName)[0];

            if (string.IsNullOrEmpty(FileID))
            {
                //Find the file
                Dictionary<string, string> FilesInDrive = dc.FindFiles(false);
                FileID = FilesInDrive.Where(x => x.Value == OnlyFileName).First().Key;
            }

            if (!string.IsNullOrEmpty(FileID))
            {
                if (string.IsNullOrEmpty(PathForDownload))
                    await dc.DownloadFile(FileID, FileName);
                else
                    await dc.DownloadFile(FileID, FileName, PathForDownload);
            }
            else
            {
                Console.WriteLine("O Arquivo não foi localizado, abortando a operação.");
            }
        }

        public async Task<string> CreateGDriveDir(string FolderName)
        {
            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS);

            var f = dc.FindFiles(false, "application/vnd.google-apps.folder");

            bool exist = f.ContainsValue(FolderName);
            if (exist)
                return f.Where(x => x.Value == FolderName).First().Key;

            return dc.CreateGDriveDir(FolderName);
        }

        public async Task DeleteFile(string FileName = "", string FileID = "")
        {
            GoogleServiceController GSC = new GoogleServiceController();
            DriveService DS = GSC.GetDriveService();
            DriveController dc = new DriveController(DS);

            string OnlyFileName = AuxFileCommCenter.GetFileNameOnlyWithFormat(FileName)[0];

            if (string.IsNullOrEmpty(FileID) && string.IsNullOrEmpty(FileName))
            {
                //Find the file
                Dictionary<string, string> FilesInDrive = dc.FindFiles(false);
                FileID = FilesInDrive.Where(x => x.Value == OnlyFileName).First().Key;
            }
            else if (string.IsNullOrEmpty(FileID))
                dc.DeleteFile(DS, FileID);
            else
                Console.WriteLine("Nomes em branco, abortando operação");
        }
    }
}
