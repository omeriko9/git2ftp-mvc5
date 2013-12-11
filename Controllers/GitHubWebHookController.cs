using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using git2ftp_mvc5.Models;
using jobj = System.Collections.Generic.Dictionary<string, object>;

namespace git2ftp_mvc5.Controllers
{
    public class GitHubWebHookController : Controller
    {
        //
        // GET: /GitHubWebHook/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ContentResult GitHubPost(string payload)
        {
            if (!String.IsNullOrEmpty(payload)) // we have a winner
            {
                var decoded = Server.UrlDecode(payload);
                System.Web.HttpContext.Current.Application["gitResponse"] = decoded;
                Deploy(decoded);
            }

            return new ContentResult { Content = "ok" };
        }

        private void Deploy(string gitResponse)
        {
            var res = new JavaScriptSerializer();
            jobj objResp = null;
            try
            {
                objResp = (jobj)res.DeserializeObject(gitResponse.Replace("payload=", ""));
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format
                ("Cannot parse git response: \"{0}\"", gitResponse), ex);

            }
            var repoName = (objResp["repository"] as jobj)["name"].ToString();
            git2ftp_Projects proj;
            git2ftp_Users user;
            using (var a = new omeriko9Entities())
            {
                proj = a.git2ftp_Projects.Where(x => x.GitRepositoryName == repoName).First();
                user = a.git2ftp_Users.Where(x => x.Username == proj.git2ftp_Users.Username).First();
            }
            Action<string> log = (x) =>
            {
                try
                {
                    using (var a = new omeriko9Entities())
                    {
                        a.git2ftp_Log.Add(new git2ftp_Log()
                        {
                            ProjectID = proj.pKey,
                            GitHubJSON = gitResponse.Length > 4000 ? string.Concat(gitResponse.Take(4000)) : gitResponse,
                            State = x,
                            DateTime = DateTime.Now
                        });
                        a.SaveChanges();
                    }
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        throw new Exception("DB Validation error: " + eve.ValidationErrors.First().ErrorMessage);
                    }
                }
            };

            log("Fetching");

            var ftppass = proj.FTPPassword;
            var gitHub = new GitHubFacade(proj.GitOwner, proj.GitRepositoryName, proj.GitApiKey);

            log("Connecting to FTP");

            var ftp = new FTPFacade(proj.FTPAddress, proj.FTPUsername, proj.FTPPassword);
            var head = objResp["head_commit"];
            var arrAdded = ((object[])((jobj)head)["added"]);
            var arrRemoved = ((object[])((jobj)head)["removed"]);
            var arrChanged = ((object[])((jobj)head)["modified"]);

            Array.ForEach(arrAdded, y =>
            {
                var fileFullPath = y.ToString();
                log("Adding file: " + fileFullPath);

                var fileContent = gitHub.GetFile(fileFullPath);
                if (fileContent == null)
                    ftp.CreateFolder(fileFullPath);
                else
                    ftp.UploadFile(fileContent, fileFullPath);

                log("Added file " + fileFullPath + " successfully.");
            });
            Array.ForEach(arrRemoved, y =>
            {
                var fileFullPath = y.ToString();
                log("deleting file: " + fileFullPath);
                ftp.DeleteFile(fileFullPath, false);
                log("Deleted file " + fileFullPath + " successfully.");
            });
            Array.ForEach(arrChanged, y =>
            {
                var fileFullPath = y.ToString();
                log("Uploading file: " + fileFullPath);

                var fileContent = gitHub.GetFile(fileFullPath);
                if (fileContent == null)
                    ftp.CreateFolder(fileFullPath);
                else
                    ftp.UploadFile(fileContent, fileFullPath);

                log("Uploaded file " + fileFullPath + " successfully.");
            });

            log("Completed Successfully");
            
        }

        public ContentResult LastOne()
        {
            return new ContentResult
            {
                Content =
                    System.Web.HttpContext.Current.Application["gitResponse"] != null ?
                    Server.UrlDecode(System.Web.HttpContext.Current.Application["gitResponse"].ToString())
                    : ""
            };

        }
    }
}