using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace ChatApp.Server.Data
{
    public static class Database
    {
        public static string DataFolder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServerData");

        public static string DbPath => Path.Combine(DataFolder, "chatapp.db");

        public static SqliteConnection Open()
        {
            var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=3000;";
                cmd.ExecuteNonQuery();
            }
            return conn;
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(Path.Combine(DataFolder, "Files"));
            Directory.CreateDirectory(Path.Combine(DataFolder, "Photos"));

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    UserId       INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    PasswordSalt TEXT NOT NULL,
    FullName     TEXT NOT NULL DEFAULT '',
    Bio          TEXT NOT NULL DEFAULT '',
    PhotoPath    TEXT,
    CreatedAt    TEXT NOT NULL,
    LastSeen     TEXT
);
CREATE TABLE IF NOT EXISTS Conversations (
    ConversationId INTEGER PRIMARY KEY AUTOINCREMENT,
    Type           TEXT NOT NULL,
    Name           TEXT,
    PairKey        TEXT UNIQUE,
    CreatedBy      INTEGER,
    CreatedAt      TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS ConversationMembers (
    ConversationId    INTEGER NOT NULL,
    UserId            INTEGER NOT NULL,
    Role              TEXT NOT NULL DEFAULT 'Member',
    JoinedAt          TEXT NOT NULL,
    LastReadMessageId INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (ConversationId, UserId)
);
CREATE TABLE IF NOT EXISTS Messages (
    MessageId      INTEGER PRIMARY KEY AUTOINCREMENT,
    ConversationId INTEGER NOT NULL,
    SenderId       INTEGER,
    Kind           TEXT NOT NULL DEFAULT 'Text',
    Content        TEXT NOT NULL DEFAULT '',
    FileId         INTEGER,
    SentAt         TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_Messages_Conv ON Messages(ConversationId, MessageId);
CREATE INDEX IF NOT EXISTS IX_Members_User ON ConversationMembers(UserId);
CREATE TABLE IF NOT EXISTS Files (
    FileId       INTEGER PRIMARY KEY AUTOINCREMENT,
    UploaderId   INTEGER NOT NULL,
    OriginalName TEXT NOT NULL,
    StoredPath   TEXT NOT NULL,
    SizeBytes    INTEGER NOT NULL,
    UploadedAt   TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Friendships (
    FriendshipId  INTEGER PRIMARY KEY AUTOINCREMENT,
    RequesterId   INTEGER NOT NULL,
    AddresseeId   INTEGER NOT NULL,
    Status        TEXT NOT NULL,
    CreatedAt     TEXT NOT NULL,
    RespondedAt   TEXT
);
CREATE INDEX IF NOT EXISTS IX_Friendships_Addressee ON Friendships(AddresseeId, Status);
CREATE INDEX IF NOT EXISTS IX_Friendships_Requester ON Friendships(RequesterId, Status);
";
            cmd.ExecuteNonQuery();

            // Seed the General room once
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Conversations WHERE Type='Group' AND Name='General';";
            long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
            if (count == 0)
            {
                using var seed = conn.CreateCommand();
                seed.CommandText = "INSERT INTO Conversations (Type, Name, CreatedBy, CreatedAt) VALUES ('Group', 'General', NULL, $now);";
                seed.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
                seed.ExecuteNonQuery();
            }
        }
    }
}