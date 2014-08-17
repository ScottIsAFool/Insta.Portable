using Insta.Portable.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Insta.Portable
{
    public partial class InstapaperClient
    {
        private const string FoldersUrl = BaseUrl + "/1.1/folders";

        public async Task<InstaResponse<IEnumerable<Folder>>> GetFoldersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = FoldersUrl + "/list";

            var response = await GetResponse(url, null, cancellationToken).ConfigureAwait(false);
            
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ProcessResponse<IEnumerable<Folder>>(json);
        }

        public async Task<InstaResponse<Folder>> AddFolderAsync(string title, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (title == null) throw new ArgumentNullException("title");

            const string url = FoldersUrl + "/add";

            var parameters = new Dictionary<string, string>
            {
                {"title", title}
            };

            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ProcessResponse<Folder>(json);
        }

        public async Task<bool> DeleteFolderAsync(string folderId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (folderId == null) throw new ArgumentNullException("folderId");

            const string url = FoldersUrl + "/delete";

            var parameters = new Dictionary<string, string>
            {
                {"folder_id", folderId}
            };

            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }
}
