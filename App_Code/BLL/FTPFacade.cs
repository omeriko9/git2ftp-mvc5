using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// Summary description for FTPFacade
/// </summary>
namespace git2ftp_mvc5.BLL
{

    public class FTPFacade
    {
        private string _Username;
        private string _Password;
        private string _Url;

        public FTPFacade(string pUrl, string pUsername, string pPassword)
        {
            _Url = pUrl;
            _Username = pUsername;
            _Password = pPassword;
        }

        public FtpWebRequest Connect(string fileFullPath)
        {
            string url = String.Format("ftp://{0}/{1}", _Url, fileFullPath);
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Credentials = new NetworkCredential(_Username, _Password);
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to connect to FTP at URL: " + url);
            }

        }
        public void DeleteFile(string DestinationFileFullPath, bool isFolder)
        {
            try
            {
                // file might not exist
                var request = Connect(DestinationFileFullPath);
                request.Method = isFolder ? WebRequestMethods.Ftp.RemoveDirectory : WebRequestMethods.Ftp.DeleteFile;
                var UriTemp = new Uri(new Uri("http://dummy.com/"), DestinationFileFullPath);

                if (UriTemp.Segments.Length > 2) // if subfolder
                {
                    var folder = UriTemp.Segments[UriTemp.Segments.Length - 2].Replace("/", "");
                    // If file was last one, delete folder
                    if (String.IsNullOrEmpty(ListFilesInFolder(DestinationFileFullPath)))
                        DeleteFile(folder, true);
                }

                ExecuteAndValidate(request, FtpStatusCode.ActionNotTakenFileUnavailable, FtpStatusCode.FileActionOK);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void CreateFolder(string DestinationFolderFullPath)
        {
            var request = Connect(DestinationFolderFullPath);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            ExecuteAndValidate(request, FtpStatusCode.PathnameCreated);
        }

        public void UploadFile(byte[] fileContents, string DestinationFileFullPath)
        {
            // Make Subfolders if required
            foreach (var seg in new Uri("Http://dummy/" + DestinationFileFullPath).Segments)
            {
                if (seg == @"/") continue;

                if (seg.EndsWith(@"/")) // folder
                {
                    var sub = (seg.Replace(@"/", ""));
                    var folderNameFull = DestinationFileFullPath.Split(new string[] { sub }, StringSplitOptions.None)[0] + sub;
                    //if (!FtpDirectoryExists(folderNameFull))
                    CreateFolder(folderNameFull);

                }
            }

            var request = Connect(DestinationFileFullPath);
            request.ContentLength = fileContents.Length;

            try
            {

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            ExecuteAndValidate(request, FtpStatusCode.ClosingData);

        }

        public string ListFilesInFolder(string directoryPath)
        {
            string toReturn = "";
            var request = Connect(directoryPath);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                toReturn = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                // todo: handle problems with deletion
            }

            return toReturn;
        }

        public bool FtpDirectoryExists(string directoryPath)
        {
            bool IsExists = false;
            try
            {

                FtpWebRequest request = Connect(directoryPath);
                request.UsePassive = false;
                request.UseBinary = false;
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    var res = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    IsExists = true;
                }
            }
            catch (WebException ex)
            {

            }
            return IsExists;
        }

        private void ExecuteAndValidate(FtpWebRequest request, params FtpStatusCode[] IgnoreStatusCodes)
        {
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                var responseCode = response.StatusCode;

                if (!new List<FtpStatusCode>(IgnoreStatusCodes).Contains(responseCode))
                {
                    throw new Exception(responseCode.ToString());
                }

                response.Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote server returned an error: (550) File unavailable (e.g., file not found, no access)"))
                    return;

                throw;
            }
        }
    }
}