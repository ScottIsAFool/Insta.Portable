using AsyncOAuth;
using Insta.Portable.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Insta.Portable
{
    public partial class InstapaperClient
    {
        private const string BookmarksBaseUrl = BaseUrl + "/1.1/bookmarks";

        public async Task<InstaResponse<BookmarksResponse>> GetBookmarksAsync(
            int? limit = null,
            string folderType = null,
            IEnumerable<int> alreadyHave = null,
            IEnumerable<int> highlights = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/list";

            var parameters = new Dictionary<string, string>();

            if (limit.HasValue)
            {
                parameters.Add("limit", limit.Value.ToString());
            }

            if (!string.IsNullOrEmpty(folderType))
            {
                parameters.Add("folder_id", folderType);
            }

            if (alreadyHave != null)
            {
                string have = string.Join(",", alreadyHave);
                if (string.IsNullOrEmpty(have) == false)
                {
                    parameters.Add("have", have);
                }
            }

            if (highlights != null)
            {
                string highlightsStr = string.Join("-", highlights);
                if (string.IsNullOrEmpty(highlightsStr) == false)
                {
                    parameters.Add("highlights", highlightsStr);
                } 
            }

            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ProcessResponse<BookmarksResponse>(json);
        }

        public async Task<string> GetBookmarkContentAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            string url = BookmarksBaseUrl + "/get_text";

            var parameters = new Dictionary<string, string>
            {
                {"bookmark_id", bookmarkId.ToString()}
            };
            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<InstaResponse<Bookmark>> AddBookmarkAsync(string bookmarkUrl, string title = null, string description = null, string folderId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bookmarkUrl == null) throw new ArgumentNullException("bookmarkUrl");

            const string url = BookmarksBaseUrl + "/add";

            var parameters = new Dictionary<string, string>
            {
                {"url", bookmarkUrl}
            };

            if (string.IsNullOrEmpty(title) == false)
            {
                parameters["title"] = title;
            }
            if (string.IsNullOrEmpty(description) == false)
            {
                parameters["description"] = description;
            }
            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ProcessResponse<Bookmark>(json);
        }

        public Task<InstaResponse<Bookmark>> UpdateReadProgressAsync(int bookmarkId, float readPercentage, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/update_read_progress";

            var epoch = new DateTime(1970, 1, 1);
            var timestamp = DateTime.UtcNow.Ticks - epoch.Ticks;
            var parameters = new Dictionary<string, string>
            {
                {"progress", readPercentage.ToString()},
                {"progress_timestamp", timestamp.ToString()}
            };

            return GetBookmarkAsync(url, bookmarkId, parameters, cancellationToken);
        }

        public async Task<bool> DeleteBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/delete";

            var parameters = new Dictionary<string, string>
            {
                {"bookmark_id", bookmarkId.ToString()}
            };
            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public Task<InstaResponse<Bookmark>> StarBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/star";

            return GetBookmarkAsync(url, bookmarkId, cancellationToken: cancellationToken);
        }

        public Task<InstaResponse<Bookmark>> UnstarBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/unstar";

            return GetBookmarkAsync(url, bookmarkId, cancellationToken: cancellationToken);
        }

        public Task<InstaResponse<Bookmark>> ArchiveBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/archive";

            return GetBookmarkAsync(url, bookmarkId, cancellationToken: cancellationToken);
        }

        public Task<InstaResponse<Bookmark>> UnarchiveBookmarkAsync(int bookmarkId, CancellationToken cancellationToken = default(CancellationToken))
        {
            const string url = BookmarksBaseUrl + "/unarchive";

            return GetBookmarkAsync(url, bookmarkId, cancellationToken: cancellationToken);
        }

        private async Task<InstaResponse<Bookmark>> GetBookmarkAsync(string url, int bookmarkId, IDictionary<string, string> additionalParameters = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var parameters = new Dictionary<string, string>
            {
                {"bookmark_id", bookmarkId.ToString()}
            };
            if (additionalParameters != null)
            {
                foreach (var additionalParameter in additionalParameters)
                {
                    parameters.Add(additionalParameter.Key, additionalParameter.Value);
                }
            }

            var response = await GetResponse(url, parameters, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ProcessResponse<Bookmark>(json);
        }

        private Task<HttpResponseMessage> GetResponse(string url, IEnumerable<KeyValuePair<string, string>> parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = new HttpClient(new OAuthMessageHandler(_consumerKey, _consumerSecret, AccessToken));
            var message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Content = new FormUrlEncodedContent(parameters ?? new Dictionary<string, string>());
            return client.SendAsync(message, cancellationToken);
        }
    }
}