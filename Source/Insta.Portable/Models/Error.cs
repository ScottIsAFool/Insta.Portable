using Newtonsoft.Json;

namespace Insta.Portable.Models
{
    public class Error
    {
        [JsonProperty("error_code")]
        public ErrorCode ErrorCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public enum ErrorCode
    {
        RateLimitExceeded = 1040,
        SubscriptionAccountRequired = 1041,
        ApplicationIsSuspended = 1042,
        
        DomainRequiresFullContent = 1220,
        DomainHasOptedOut = 1221,
        InvalidUrl = 1240,
        InvalidOrMissingBookmarkId = 1241,
        InvalidOrMissingFolderId = 1242,
        InvalidOrMissingProgress = 1243,
        InvalidOrMissingProgressTimestamp = 1244,
        PrivateBookmarksRequireContent = 1245,
        ErrorSavingBookmarkOrMissingFolderTitle = 1250,

        TitleAlreadyExists = 1251,
        CannotAddBookmarksToThisFolder = 1252,

        UnexpectedServiceError = 1500,
        ErrorGeneratingText = 1550,

        HighlightRequiresText = 1600,
        DuplicateHighlight = 1601
    }
}