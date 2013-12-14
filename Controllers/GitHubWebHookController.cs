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
            var tt = @"{""ref"":""refs/heads/master"",""after"":""d0fb0edd0cf831e06ca0661fbc8ced59fa9124a8"",""before"":""4c7cd396f97c90becfe9bacf640c17c09bacda2e"",""created"":false,""deleted"":false,""forced"":false,""compare"":""https://github.com/omeriko9/zohargallery/compare/4c7cd396f97c...d0fb0edd0cf8"",""commits"":[{""id"":""5753d52c794ef2d7cb7322c509aa81f39a3db5e4"",""distinct"":true,""message"":""remove test"",""timestamp"":""2013-12-07T09:26:22-08:00"",""url"":""https://github.com/omeriko9/zohargallery/commit/5753d52c794ef2d7cb7322c509aa81f39a3db5e4"",""author"":{""name"":""omeriko9"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""committer"":{""name"":""omeriko9"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""added"":[],""removed"":[""test.txt""],""modified"":[]},{""id"":""46b3e10eb56b189b9f9ab3d7073e900abe3d6f50"",""distinct"":true,""message"":""removed caching from web.config since it screws up the website"",""timestamp"":""2013-12-07T10:49:21-08:00"",""url"":""https://github.com/omeriko9/zohargallery/commit/46b3e10eb56b189b9f9ab3d7073e900abe3d6f50"",""author"":{""name"":""omeriko9"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""committer"":{""name"":""omeriko9"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""added"":[],""removed"":[],""modified"":[""Web.config""]},{""id"":""d0fb0edd0cf831e06ca0661fbc8ced59fa9124a8"",""distinct"":true,""message"":""Create robots.txt"",""timestamp"":""2013-12-09T06:11:15-08:00"",""url"":""https://github.com/omeriko9/zohargallery/commit/d0fb0edd0cf831e06ca0661fbc8ced59fa9124a8"",""author"":{""name"":""Omer Agmon"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""committer"":{""name"":""Omer Agmon"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""added"":[""robots.txt""],""removed"":[],""modified"":[]}],""head_commit"":{""id"":""d0fb0edd0cf831e06ca0661fbc8ced59fa9124a8"",""distinct"":true,""message"":""Create robots.txt"",""timestamp"":""2013-12-09T06:11:15-08:00"",""url"":""https://github.com/omeriko9/zohargallery/commit/d0fb0edd0cf831e06ca0661fbc8ced59fa9124a8"",""author"":{""name"":""Omer Agmon"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""committer"":{""name"":""Omer Agmon"",""email"":""omeriko9@gmail.com"",""username"":""omeriko9""},""added"":[""robots.txt""],""removed"":[],""modified"":[]},""repository"":{""id"":14746532,""name"":""zohargallery"",""url"":""https://github.com/omeriko9/zohargallery"",""description"":"""",""watchers"":0,""stargazers"":0,""forks"":1,""fork"":false,""size"":44938,""owner"":{""name"":""omeriko9"",""email"":""omeriko9@gmail.com""},""private"":false,""open_issues"":0,""has_issues"":true,""has_downloads"":true,""has_wiki"":true,""language"":""JavaScript"",""created_at"":1385555639,""pushed_at"":1386598275,""master_branch"":""master""},""pusher"":{""name"":""none""}}";
            //payload = tt;
            if (!String.IsNullOrEmpty(payload)) // we have a winner
            {
                var decoded = Server.UrlDecode(payload);
                System.Web.HttpContext.Current.Application["gitResponse"] = decoded;
                if (decoded != "omer")
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