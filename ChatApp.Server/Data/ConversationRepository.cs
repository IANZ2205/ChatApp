using System;
using System.Collections.Generic;
using ChatApp.Common.Models;
using Microsoft.Data.Sqlite;

namespace ChatApp.Server.Data
{
    public class ConversationRepository
    {
        public int GetOrCreatePrivate(int userId1, int userId2)
        {
            int a = Math.Min(userId1, userId2);
            int b = Math.Max(userId1, userId2);
            string pairKey = $"{a}-{b}";

            using var conn = Database.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT ConversationId FROM Conversations WHERE PairKey = $p LIMIT 1;";
                cmd.Parameters.AddWithValue("$p", pairKey);
                var existing = cmd.ExecuteScalar();
                if (existing != null) return Convert.ToInt32(existing);
            }

            int newId;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Conversations (Type, Name, PairKey, CreatedBy, CreatedAt)
                                    VALUES ('Private', NULL, $p, $c, $d);";
                cmd.Parameters.AddWithValue("$p", pairKey);
                cmd.Parameters.AddWithValue("$c", userId1);
                cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            foreach (int uid in new[] { userId1, userId2 })
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT OR IGNORE INTO ConversationMembers (ConversationId, UserId, Role, JoinedAt, LastReadMessageId)
                                    VALUES ($c, $u, 'Member', $j, 0);";
                cmd.Parameters.AddWithValue("$c", newId);
                cmd.Parameters.AddWithValue("$u", uid);
                cmd.Parameters.AddWithValue("$j", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }

            return newId;
        }

        public int CreateGroup(string name, int creatorId, IEnumerable<int> memberIds)
        {
            using var conn = Database.Open();

            int convId;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Conversations (Type, Name, PairKey, CreatedBy, CreatedAt)
                                    VALUES ('Group', $n, NULL, $c, $d);";
                cmd.Parameters.AddWithValue("$n", name);
                cmd.Parameters.AddWithValue("$c", creatorId);
                cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                convId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO ConversationMembers (ConversationId, UserId, Role, JoinedAt, LastReadMessageId)
                                    VALUES ($c, $u, 'Admin', $j, 0);";
                cmd.Parameters.AddWithValue("$c", convId);
                cmd.Parameters.AddWithValue("$u", creatorId);
                cmd.Parameters.AddWithValue("$j", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }

            foreach (int uid in memberIds)
            {
                if (uid == creatorId) continue;
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT OR IGNORE INTO ConversationMembers (ConversationId, UserId, Role, JoinedAt, LastReadMessageId)
                                    VALUES ($c, $u, 'Member', $j, 0);";
                cmd.Parameters.AddWithValue("$c", convId);
                cmd.Parameters.AddWithValue("$u", uid);
                cmd.Parameters.AddWithValue("$j", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }

            return convId;
        }

        public List<ConversationInfo> GetConversationsForUser(int userId)
        {
            var list = new List<ConversationInfo>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.ConversationId, c.Type, c.Name,
                       (SELECT COUNT(*) FROM ConversationMembers WHERE ConversationId = c.ConversationId) AS MemberCount,
                       (SELECT Content FROM Messages WHERE ConversationId = c.ConversationId ORDER BY MessageId DESC LIMIT 1) AS LastMsg,
                       (SELECT IFNULL(MAX(MessageId), 0) FROM Messages WHERE ConversationId = c.ConversationId) AS LastMessageId,
                       (SELECT COUNT(*) FROM Messages
                        WHERE ConversationId = c.ConversationId
                          AND MessageId > cm.LastReadMessageId
                          AND (SenderId IS NULL OR SenderId <> $u)) AS UnreadCount
                FROM Conversations c
                JOIN ConversationMembers cm ON cm.ConversationId = c.ConversationId
                WHERE cm.UserId = $u
                ORDER BY c.ConversationId ASC;";
            cmd.Parameters.AddWithValue("$u", userId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var info = new ConversationInfo
                {
                    ConversationId = r.GetInt32(0),
                    Type = r.GetString(1),
                    Name = r.IsDBNull(2) ? "" : r.GetString(2),
                    MemberCount = r.GetInt32(3),
                    LastMessage = r.IsDBNull(4) ? null : r.GetString(4),
                    UnreadCount = (int)r.GetInt64(6)
                };
                if (info.Type == "Private")
                    info.Name = GetOtherUserName(conn, info.ConversationId, userId);
                list.Add(info);
            }
            return list;
        }

        public void MarkRead(int conversationId, int userId, int lastMessageId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE ConversationMembers
                                SET LastReadMessageId = MAX(LastReadMessageId, $m)
                                WHERE ConversationId = $c AND UserId = $u;";
            cmd.Parameters.AddWithValue("$m", lastMessageId);
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$u", userId);
            cmd.ExecuteNonQuery();
        }

        public int GetLatestMessageId(int conversationId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IFNULL(MAX(MessageId), 0) FROM Messages WHERE ConversationId = $c;";
            cmd.Parameters.AddWithValue("$c", conversationId);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public ConversationInfo? GetConversationInfo(int convId, int forUserId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT c.ConversationId, c.Type, c.Name,
                       (SELECT COUNT(*) FROM ConversationMembers WHERE ConversationId = c.ConversationId) AS MemberCount,
                       (SELECT Content FROM Messages WHERE ConversationId = c.ConversationId ORDER BY MessageId DESC LIMIT 1) AS LastMsg
                FROM Conversations c WHERE c.ConversationId = $id LIMIT 1;";
            cmd.Parameters.AddWithValue("$id", convId);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            var info = new ConversationInfo
            {
                ConversationId = r.GetInt32(0),
                Type = r.GetString(1),
                Name = r.IsDBNull(2) ? "" : r.GetString(2),
                MemberCount = r.GetInt32(3),
                LastMessage = r.IsDBNull(4) ? null : r.GetString(4)
            };
            r.Close();
            if (info.Type == "Private")
                info.Name = GetOtherUserName(conn, info.ConversationId, forUserId);
            return info;
        }

        private string GetOtherUserName(SqliteConnection conn, int convId, int selfId)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT u.Username FROM ConversationMembers cm
                                JOIN Users u ON u.UserId = cm.UserId
                                WHERE cm.ConversationId = $c AND cm.UserId <> $s LIMIT 1;";
            cmd.Parameters.AddWithValue("$c", convId);
            cmd.Parameters.AddWithValue("$s", selfId);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "(private)";
        }
        public bool AddMember(int conversationId, int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT OR IGNORE INTO ConversationMembers
                                (ConversationId, UserId, Role, JoinedAt, LastReadMessageId)
                                VALUES ($c, $u, 'Member', $j, 0);";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$u", userId);
            cmd.Parameters.AddWithValue("$j", DateTime.UtcNow.ToString("o"));
            return cmd.ExecuteNonQuery() > 0;
        }

        public string? GetGroupName(int conversationId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Conversations WHERE ConversationId = $c AND Type='Group' LIMIT 1;";
            cmd.Parameters.AddWithValue("$c", conversationId);
            var result = cmd.ExecuteScalar();
            return result?.ToString();
        }
    }
}