using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Octokit;
using Octokit.Internal;

/// <summary>
/// Summary description for GitHubFacade
/// </summary>
public class GitHubFacade
{
    private string _APIkey;
    private string _Owner;
    private string _Repository;

    private GitHubClient _client;
    private Commit _LastCommit;
    private string _LastCommitTreeSha;

    public GitHubFacade(string pOwner, string pRepository, string pAPIkey)
    {
        _APIkey = pAPIkey;
        _Owner = pOwner;
        _Repository = pRepository;
        var con = new Connection(new ProductHeaderValue(pRepository), new InMemoryCredentialStore(new Credentials(_APIkey)));
        _client = new GitHubClient(con);
    }

    public Commit GetLastCommit()
    {
        var allCommitsResults = Task.WhenAll(_client.Activity.Events.GetAllForRepositoryNetwork(_Owner, _Repository));
        var allCommits = allCommitsResults.Result[0].OrderByDescending(x => x.CreatedAt);
        var refLastCommit = allCommits.First().Payload.Head;
        var lastCommitResults = Task.WhenAll(_client.GitDatabase.Commit.Get(_Owner, _Repository, refLastCommit));
        var lastCommit = lastCommitResults.Result.First();
        _LastCommit = lastCommit;
        _LastCommitTreeSha = lastCommit.Tree.Sha;

        return lastCommit;
    }

    public Dictionary<string, string> GetChangedFilesLastCommit()
    {
        return GetChangedFiles(_LastCommitTreeSha);
    }

    public byte[] GetFile(string fileFullPath)
    {
        var res = Task.WhenAll(_client.Repository.GetContent(_Owner, _Repository, fileFullPath)).Result;
        var content = res[0];
        if (content.type.ToLower().Equals("dir"))
            return null;

        var contentBase64 = content.Content;
        return Convert.FromBase64String(contentBase64);
    }

    public Dictionary<string, string> GetChangedFiles(string sha)
    {
        var refID = sha;
        
        var lastCommit = Task.WhenAll(_client.Repository.GetCommit(_Owner, _Repository, refID)).Result;

        foreach (var file in lastCommit[0].Files)
        {
            var shaFile = file.sha;
            var blob = Task.WhenAll(_client.Blob.Get(_Owner, _Repository, shaFile)).Result;
            //var trees = Task.WhenAll(_client.Tree.Get(_Owner, _Repository, shaFile)).Result;
            var con = Task.WhenAll(_client.Repository.GetContent(_Owner, _Repository, file.filename)).Result;
                
        }

        var lastCommitTreeResults = Task.WhenAll(_client.Tree.Get(_Owner, _Repository, refID));
        var lastCommitTree = lastCommitTreeResults.Result.First();

        var ChangedFiles = new Dictionary<string, string>();

        foreach (var treeitem in lastCommitTree.Tree)
        {
            if (treeitem.Type == TreeType.Blob)
            {
                var blobResults = Task.WhenAll(_client.Blob.Get(_Owner, _Repository, treeitem.Sha));
                var firstBlobResults = blobResults.Result;
                var blobf = firstBlobResults.First();
                var blob = blobf.Content;

                ChangedFiles.Add(treeitem.Path, blob);
            }

        }

        return ChangedFiles;

    }

}