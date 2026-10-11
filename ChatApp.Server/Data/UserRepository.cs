using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ChatApp.Server.Data
{
    public class UserRepository
    {
        public class UserRow
        {
            public int UserId { get; set; }
            public string Username { get; set; } = "";
            public string PasswordHash { get; set; } = "";
            public string PasswordSalt { get; set; } = "";
            public string FullName { get; set; } = "";
        }

        public class AdminUserRow
        {
            public int UserId { get; set; }
            public string Username { get; set; } = "";
            public string FullName { get; set; } = "";
            public string CreatedAt { get; set; } = "";
            public string? LastSeen { get; set; }
        }

        public UserRow? GetByUsername(string username)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserId, Username, PasswordHash, PasswordSalt, FullName FROM Users WHERE Username = $u";
            cmd.Parameters.AddWithValue("$u", username);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new UserRow
            {
                UserId = r.GetInt32(0),
                Username = r.GetString(1),
                PasswordHash = r.GetString(2),
                PasswordSalt = r.GetString(3),
                FullName = r.GetString(4)
            };
        }

        public UserRow? GetById(int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserId, Username, PasswordHash, PasswordSalt, FullName FROM Users WHERE UserId = $id;";
            cmd.Parameters.AddWithValue("$id", userId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new UserRow
            {
                UserId = r.GetInt32(0),
                Username = r.GetString(1),
                PasswordHash = r.GetString(2),
                PasswordSalt = r.GetString(3),
                FullName = r.GetString(4)
            };
        }

        public int Create(string username, string passwordHash, string passwordSalt, string fullName)
        {
            using var conn = Database.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash, PasswordSalt, FullName, Bio, CreatedAt)
                                    VALUES ($u, $h, $s, $f, '', $c);";
                cmd.Parameters.AddWithValue("$u", username);
                cmd.Parameters.AddWithValue("$h", passwordHash);
                cmd.Parameters.AddWithValue("$s", passwordSalt);
                cmd.Parameters.AddWithValue("$f", fullName);
                cmd.Parameters.AddWithValue("$c", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void AddToConversation(int userId, int conversationId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT OR IGNORE INTO ConversationMembers (ConversationId, UserId, Role, JoinedAt, LastReadMessageId)
                                VALUES ($c, $u, 'Member', $j, 0);";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$u", userId);
            cmd.Parameters.AddWithValue("$j", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public int GetGeneralConversationId()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ConversationId FROM Conversations WHERE Type='Group' AND Name='General' LIMIT 1;";
            var result = cmd.ExecuteScalar();
            return result == null ? 0 : Convert.ToInt32(result);
        }

        public UserRow? GetProfile(int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserId, Username, PasswordHash, PasswordSalt, FullName FROM Users WHERE UserId = $id;";
            cmd.Parameters.AddWithValue("$id", userId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new UserRow
            {
                UserId = r.GetInt32(0),
                Username = r.GetString(1),
                PasswordHash = r.GetString(2),
                PasswordSalt = r.GetString(3),
                FullName = r.GetString(4)
            };
        }

        public (string bio, string? photoPath) GetBioAndPhoto(int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Bio, PhotoPath FROM Users WHERE UserId = $id;";
            cmd.Parameters.AddWithValue("$id", userId);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return ("", null);
            return (r.IsDBNull(0) ? "" : r.GetString(0),
                    r.IsDBNull(1) ? null : r.GetString(1));
        }

        public void UpdateProfile(int userId, string fullName, string bio, string? photoPath)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            if (photoPath == null)
            {
                cmd.CommandText = "UPDATE Users SET FullName = $f, Bio = $b WHERE UserId = $id;";
            }
            else
            {
                cmd.CommandText = "UPDATE Users SET FullName = $f, Bio = $b, PhotoPath = $p WHERE UserId = $id;";
                cmd.Parameters.AddWithValue("$p", photoPath);
            }
            cmd.Parameters.AddWithValue("$f", fullName);
            cmd.Parameters.AddWithValue("$b", bio);
            cmd.Parameters.AddWithValue("$id", userId);
            cmd.ExecuteNonQuery();
        }

        public List<AdminUserRow> GetAllUsers()
        {
            var list = new List<AdminUserRow>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserId, Username, FullName, CreatedAt, LastSeen FROM Users ORDER BY UserId ASC;";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new AdminUserRow
                {
                    UserId = r.GetInt32(0),
                    Username = r.GetString(1),
                    FullName = r.IsDBNull(2) ? "" : r.GetString(2),
                    CreatedAt = r.IsDBNull(3) ? "" : r.GetString(3),
                    LastSeen = r.IsDBNull(4) ? null : r.GetString(4)
                });
            }
            return list;
        }

        public int GetUserCount()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users;";
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public int GetMessageCount()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Messages;";
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public int GetFileCount()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Files;";
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }
        public void UpdateLastSeen(int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET LastSeen = $d WHERE UserId = $id;";
            cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("$id", userId);
            cmd.ExecuteNonQuery();
        }
    }
}