namespace ChatApp.Common
{
    public static class Constants
    {
        public const int DefaultPort = 8765;
        public const int MaxLineLength = 8 * 1024 * 1024;  // 8 MB — enough for a 2 MB photo Base64-encoded
        public const int MaxMessageLength = 4000;
        public const int FileChunkSize = 32 * 1024;       // 32 KB
        public const int MaxFileSize = 10 * 1024 * 1024;  // 10 MB

        // Error codes
        public const string ErrAuthRequired = "AUTH_REQUIRED";
        public const string ErrBadCredentials = "BAD_CREDENTIALS";
        public const string ErrUsernameTaken = "USERNAME_TAKEN";
        public const string ErrInvalid = "INVALID";
        public const string ErrNotFound = "NOT_FOUND";
        public const string ErrServerError = "SERVER_ERROR";
    }
}