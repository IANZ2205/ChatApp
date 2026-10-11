using System;
using System.Collections.Concurrent;
using System.IO;
using ChatApp.Common;
using ChatApp.Server.Data;

namespace ChatApp.Server.Services
{
    public class FileService
    {
        private class UploadSession
        {
            public int UserId;
            public int ConversationId;
            public string OriginalName = "";
            public long TotalSize;
            public string TempPath = "";
            public FileStream? Stream;
            public long BytesWritten;
        }

        private static readonly ConcurrentDictionary<string, UploadSession> _uploads = new();

        public (bool ok, string uploadId, string? error) Start(int userId, int convId, string fileName, long size)
        {
            if (size <= 0 || size > Constants.MaxFileSize)
                return (false, "", $"File must be between 1 byte and {Constants.MaxFileSize / (1024 * 1024)} MB.");

            string safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName)) safeName = "file";

            string uploadId = Guid.NewGuid().ToString("N");
            string tempPath = Path.Combine(Database.DataFolder, "Files", $"upload_{uploadId}.tmp");

            try
            {
                var stream = File.Create(tempPath);
                _uploads[uploadId] = new UploadSession
                {
                    UserId = userId,
                    ConversationId = convId,
                    OriginalName = safeName,
                    TotalSize = size,
                    TempPath = tempPath,
                    Stream = stream,
                    BytesWritten = 0
                };
                return (true, uploadId, null);
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }

        public bool AppendChunk(string uploadId, int seq, byte[] data)
        {
            if (!_uploads.TryGetValue(uploadId, out var s)) return false;
            if (s.Stream == null) return false;
            try
            {
                lock (s)
                {
                    s.Stream.Write(data, 0, data.Length);
                    s.BytesWritten += data.Length;
                }
                return true;
            }
            catch { return false; }
        }

        public int Finish(string uploadId, int senderId, out int conversationId)
        {
            conversationId = 0;
            if (!_uploads.TryRemove(uploadId, out var s)) return 0;
            try
            {
                s.Stream?.Close();
                s.Stream?.Dispose();
            }
            catch { }

            if (s.BytesWritten != s.TotalSize)
            {
                try { File.Delete(s.TempPath); } catch { }
                return 0;
            }

            string stored = Path.Combine(Database.DataFolder, "Files", $"f_{Guid.NewGuid():N}_{Path.GetExtension(s.OriginalName)}");
            try { File.Move(s.TempPath, stored); }
            catch { return 0; }

            int fileId;
            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Files (UploaderId, OriginalName, StoredPath, SizeBytes, UploadedAt)
                                    VALUES ($u, $n, $p, $s, $d);";
                cmd.Parameters.AddWithValue("$u", senderId);
                cmd.Parameters.AddWithValue("$n", s.OriginalName);
                cmd.Parameters.AddWithValue("$p", stored);
                cmd.Parameters.AddWithValue("$s", s.BytesWritten);
                cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var conn = Database.Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                fileId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            conversationId = s.ConversationId;
            return fileId;
        }

        public (string path, string name, long size)? GetFile(int fileId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT OriginalName, StoredPath, SizeBytes FROM Files WHERE FileId = $id;";
            cmd.Parameters.AddWithValue("$id", fileId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return (r.GetString(1), r.GetString(0), r.GetInt64(2));
        }
    }
}