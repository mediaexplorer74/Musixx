using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Musixx.Models;
using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Musixx.Clouds
{
    public class GoogleDrive
    {
        private string accessToken;
        UserCredential credential;

        static string[] Scopes = 
        {
            DriveService.Scope.Drive,
            DriveService.Scope.DriveMetadata, //!
            DriveService.Scope.DriveMetadataReadonly, //!
            Oauth2Service.Scope.UserinfoProfile,
            Oauth2Service.Scope.UserinfoEmail
        };
        static string ApplicationName = "FamilyHub";//"Musixx";

        public GoogleDrive()
        {
            //
        }

        private Oauth2Service Oauth2Service { get; set; }
        private DriveService DriveService { get; set; }

        //
        public async Task<bool> Login()
        {
            var clientSecretUri = new Uri("ms-appx:///Resources/client_secret.json", UriKind.Absolute);
            try
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecretUri, Scopes, "user", CancellationToken.None);
                await credential.RefreshTokenAsync(CancellationToken.None);
            }
            catch (TokenResponseException e)
            {
                await credential.GetAccessTokenForRequestAsync();
            }
            catch (Exception e)
            {
                return false;
            }

            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            };
            accessToken = credential.Token.AccessToken;

            Oauth2Service = new Oauth2Service(initializer);
            DriveService = new DriveService(initializer);

            return true;
        }

        //
        public Task<bool> Logout()
        {
            return credential.RevokeTokenAsync(CancellationToken.None);
        }

        //
        public async Task<User> GetUser()
        {
            var userInfoPlus = await Oauth2Service.Userinfo.Get().ExecuteAsync();
            return new User(userInfoPlus.Name, userInfoPlus.Picture);
        }

        /*
        public async Task<IEnumerable<MusicHOLD>> GetMusics()
        {
            FilesResource.ListRequest listRequest = DriveService.Files.List();
            listRequest.MaxResults = 1000;
            listRequest.Fields = "items(downloadUrl,fileExtension,fileSize,id,title)";
            
            //listRequest.Q = "mimeType='audio/mp3' and trashed=false";
            listRequest.Q = "mimeType contains 'audio' and trashed=false"; 

            var fileList = await listRequest.ExecuteAsync();

            var baseUrl = "https://www.googleapis.com/drive/v2/files/";//"https://googledrive.com/host/";

            return fileList.Items.Select(f => new MusicHOLD(f.Title, baseUrl + f.Id + "?access_token=" + accessToken,
                f.FileSize.HasValue ? f.FileSize.Value : 0));
        }
        */

        //
        public async Task<List<IMusic>> GetMusic()
        {
            const string baseUrl = "https://www.googleapis.com/drive/v2/files/";//"https://googledrive.com/host/";

            FilesResource.ListRequest listRequest = DriveService.Files.List();
            listRequest.MaxResults = 1000;
            listRequest.Fields = "items(downloadUrl,fileExtension,fileSize,id,md5Checksum,title)";

            // RnD
            listRequest.Q = "mimeType contains 'audio' and trashed=false"; 
            //listRequest.Q = "mimeType ='audio/mp3' and trashed=false";

            Google.Apis.Drive.v2.Data.FileList fileList = await listRequest.ExecuteAsync();

            List<IMusic> songs = new List<IMusic>();

            foreach(var f in fileList.Items)
            {
                string path = "";

                try
                {
                    path = baseUrl + f.Id + "?access_token=" + accessToken;
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine(ex1.Message);
                }

                long size = 0;

                try
                {
                    size = f.FileSize.HasValue ? f.FileSize.Value : 0;
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine(ex2.Message);
                }

                File file = null;
                try
                { 
                    file = new File(f.Id, f.Title, f.Md5Checksum, size, path);
                }
                catch (Exception ex3)
                {
                    Debug.WriteLine(ex3.Message);
                }

                try
                {
                    songs.Add(new Song(file));
                }
                catch (Exception ex4)
                {
                    Debug.WriteLine(ex4.Message);
                }
            }

            return songs;
        }
    }
}
