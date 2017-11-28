using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System.IO;
using Google.Apis.Download;

namespace GoogleDriveAPITransferCMD.Controller
{
    class DriveController
    {
        public const int KBMultiplier = 0x400;
        public const int DownloadSliceOfCake = KBMultiplier * 256;
        public static long BytesTotal = 0;
        public static long BytesProgress = 0;
        public static bool FirstExec = true;
        public DriveService Service { get; set; }
        public static DriveService ServiceStatic;
        public DriveController(DriveService ds) { this.Service = ds; DriveController.ServiceStatic = ds; }

        //Thanks Ms. Lawton
        public static Permission InsertPermission(DriveService service, string fileId, string email, string type, string role)
        {
            Permission newPermission = new Permission();
            newPermission.EmailAddress = email;
            newPermission.Type = type;
            newPermission.Role = role;
            try
            {
                return service.Permissions.Create(newPermission, fileId).Execute();   
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
            return null;
        }

        public async Task UploadFile(string FileName, string GDriveID = null, string MIMEType = "application/octet-stream")
        {
            string format = AuxFileCommCenter.GetFileNameOnlyWithFormat(FileName)[1];
            if (!string.IsNullOrEmpty(format))
            {
                MIMEType  = AuxFileCommCenter.GuessMime(format) ?? "application/octet-stream";
            } 
            await UploadFileAsync(this.Service, FileName, GDriveID ,MIMEType);
        }

        public Task<IUploadProgress> UploadFileAsync(DriveService service, string fileName, string GDFolderID = null, string MIMEType = "application/octet-stream")
        {
            Dictionary<string, string> fls = FindFiles(false);
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            string filenamewithoutpath = AuxFileCommCenter.GetFileNameOnlyWithFormat(fileName)[0];

            Google.Apis.Drive.v3.Data.File FileMeta = null;

            bool Existe = fls.ContainsValue(filenamewithoutpath);
            Task<IUploadProgress> tk;
            if (Existe)
            {
                string fileid = fls.Where(x => x.Value == filenamewithoutpath).First().Key;

                var insert = service.Files.Update(new Google.Apis.Drive.v3.Data.File { Name = filenamewithoutpath ?? fileName }, fileid, fs, MIMEType);

                IList<string> oldparents = null;

                if (!string.IsNullOrEmpty(GDFolderID))
                {
                    var getolderfolder = service.Files.Get(fileid);
                    getolderfolder.Fields="parents";
                    var fileData = getolderfolder.Execute();

                    oldparents = fileData.Parents;
                    if (oldparents != null)
                    {
                        if (!oldparents.Contains(GDFolderID))
                        {
                            insert.RemoveParents = string.Join(",", oldparents);
                            insert.AddParents = GDFolderID;
                        }
                    }
                    else
                        insert.AddParents = GDFolderID;

                }

                insert.ProgressChanged += Upload_Progress;
                insert.ResponseReceived += Upload_Anwser;
                BytesTotal = fs.Length;

                tk = insert.UploadAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(GDFolderID))
                {
                    FileMeta = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = filenamewithoutpath ?? fileName,
                        Parents = new List<string>() { GDFolderID }
                    };
                }
                else
                {
                    FileMeta = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = filenamewithoutpath ?? fileName
                    };
                }

                var insert = service.Files.Create(FileMeta, fs, MIMEType);

                insert.ProgressChanged += Upload_Progress;
                insert.ResponseReceived += Upload_Anwser;
                BytesTotal = fs.Length;

                 tk = insert.UploadAsync();
            }

            tk.ContinueWith(t => { Console.WriteLine("Falha no upload do arquivo"); } , TaskContinuationOptions.NotOnRanToCompletion);
            tk.ContinueWith(t => { fs.Close(); fs.Dispose(); Console.WriteLine("Done! Upload Complete"); });

            return tk;
        }

        public static void StatusInConsole(long atual, long total, bool firstExec)
        {
            long porc = (atual * 100) / total;
            if (firstExec)
                Console.WriteLine("");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(string.Format("Progresso: {0} %", porc.ToString()));
        }

        public static void StatusInConsole(string data, bool firstExec)
        {
            if (firstExec)
                Console.WriteLine("");
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine(string.Format("Info: {0} %", data));
        }

        public static void Upload_Progress(IUploadProgress progress)
        {
            BytesProgress = (progress.BytesSent * 100) / BytesTotal;
            StatusInConsole(progress.BytesSent, BytesTotal, FirstExec);
            FirstExec = false;
        }

        public static void Upload_Anwser(Google.Apis.Drive.v3.Data.File file)
        {
            Console.SetCursorPosition(0, Console.CursorTop + 1);
            //string DadMail = System.Configuration.ConfigurationSettings.AppSettings["AccountFather"];
            //InsertPermission(ServiceStatic, file.Id, DadMail, "user", "writer");
        }


        public async Task<IDownloadProgress> DownloadFileAsync(DriveService service, string fileID, string fileName, string folderPath)
        {
            var downloader = service.Files.Get(fileID);
            downloader.Fields = "size";
            downloader.MediaDownloader.ChunkSize = DownloadSliceOfCake;
            var files = downloader.Execute();
            downloader.MediaDownloader.ProgressChanged += Download_ProgressChanged;

            BytesTotal = files.Size.Value;

            using (var fileStream = new System.IO.FileStream(fileName,
                System.IO.FileMode.Append, System.IO.FileAccess.Write))
            {
                FirstExec = true;
                var download = await downloader.DownloadAsync(fileStream);

                if (download.Status == DownloadStatus.Completed)
                {
                    Console.WriteLine(fileName + " was downloaded successfully");
                }
                else
                {
                    Console.WriteLine("Download {0} suffer an interruption. Only {1} bytes were downloaded. ",
                        fileName, download.BytesDownloaded);
                }

                return download;
            }
        }
        
        public static void Download_ProgressChanged(IDownloadProgress progress)
        {
            if (BytesTotal <= 0)
                StatusInConsole(progress.BytesDownloaded.ToString(), FirstExec);
            else
                StatusInConsole(progress.BytesDownloaded, BytesTotal, FirstExec);
            FirstExec = false;
        }

        public async Task DownloadFile(string FileID, string FileName, string FolderPath = "C:\\DownloadsAPIGoogle")
        {
            await DownloadFileAsync(Service, FileID, FileName, FolderPath);
        }

        public Dictionary<string, string> FindFiles(bool ShowFiles = true, string Type = "")
        {
            string pageToken = null;
            var request = Service.Files.List();
            string requestQ = "trashed=false";

            if (!string.IsNullOrEmpty(Type))
                requestQ = string.Format("{0} and mimeType='{1}'", requestQ, Type);

            request.Q = requestQ;
            request.Spaces = "drive";
            request.Fields = "nextPageToken, files(id, name)";
            request.PageToken = pageToken;
            var result = request.Execute();

            Dictionary<string, string> Files = new Dictionary<string, string>();

            result.Files = result.Files.OrderByDescending(x => x.ModifiedTime).ToList();
            if (result.Files.Count > 0)
            {
                foreach (var file in result.Files)
                {
                    if (ShowFiles)
                        Console.WriteLine(string.Format(
                           "Found file: {0} ({1})", file.Name, file.Id));
                    Files.Add(file.Id, file.Name);
                }
            }
            else if (ShowFiles)
                Console.WriteLine("Nenhum Arquivo Localizado.");
            pageToken = result.NextPageToken;

            return Files;
        }

        public string CreateGDriveDir(string NameDir)
        {
            var file = new Google.Apis.Drive.v3.Data.File()
            {
                Name = NameDir,
                MimeType = "application/vnd.google-apps.folder"
            };

            FilesResource.CreateRequest req = Service.Files.Create(file);
            req.Fields = "id";
            file = req.Execute();

            return file.Id;
        }

        public void DeleteFile(DriveService Service, string FileID)
        {
            var resp = Service.Files.Delete(FileID).Execute();
            Console.WriteLine(resp);
        }
    }
}
