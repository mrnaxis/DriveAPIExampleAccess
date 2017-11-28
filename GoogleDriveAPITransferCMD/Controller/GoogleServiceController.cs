using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveAPITransferCMD.Controller
{
    class GoogleServiceController
    {
        private DriveService Service { get; set; }
        private static string PathCertP12 = System.Configuration.ConfigurationSettings.AppSettings["PathCertP12"];
        private static string PathCertJSON = System.Configuration.ConfigurationSettings.AppSettings["PathCertJSON"];
        private static string AppName = System.Configuration.ConfigurationSettings.AppSettings["AppNameGoogle"];
        private static string ClientMail = System.Configuration.ConfigurationSettings.AppSettings["AccountAPI"];
        private static string ClientID = System.Configuration.ConfigurationSettings.AppSettings["ClientID"];
        private static string ClientSecret = System.Configuration.ConfigurationSettings.AppSettings["ClientSecret"];
        private static string RefreshToken = System.Configuration.ConfigurationSettings.AppSettings["RefreshTokenAPI"];

        public GoogleServiceController(string type = "User")
        {
            if (type.ToLower().Contains("serv"))
                this.AccessServiceAccount(AppName, ClientMail);
            else if (type.ToLower().Contains("use"))
                this.AccessUserAccount(AppName);
        }

        public void AccessServiceAccount(string AppName, string ClientEmail, string CertPass = "notasecret")
        {
            X509Certificate2 c = new X509Certificate2(PathCertP12, CertPass, X509KeyStorageFlags.Exportable);

            ServiceAccountCredential sac = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(ClientEmail)
                {
                    Scopes = new[] { DriveService.Scope.Drive }
                }.FromCertificate(c));

            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = sac,
                ApplicationName = AppName
            });
        }

        public void AccessUserAccount(string AppName, string ClientEmailDad = "")
        {
            TokenResponse tr = new TokenResponse() { RefreshToken = RefreshToken };

            UserCredential uc = new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
                {
                    ClientSecrets = new ClientSecrets()
                    { ClientId = ClientID, ClientSecret = ClientSecret },
                    Scopes = new[] {DriveService.Scope.Drive}
                }
            ), ClientEmailDad, tr);

            Service = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = uc, ApplicationName = AppName });
        }

        public DriveService GetDriveService()
        {
            return this.Service;
        }
    }
}
