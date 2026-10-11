using System;
using System.Collections.Generic;
using ChatApp.Common.Models;
using Microsoft.Data.Sqlite;

namespace ChatApp.Server.Data
{
    public class MessageRepository
    {
        public int Save(int conversationId, int? senderId, string kind, string content)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Messages (ConversationId, SenderId, Kind, Content, FileId, SentAt)
                                VALUES ($c, $s, $k, $t, NULL, $d);";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$s", (object?)senderId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$k", kind);
            cmd.Parameters.AddWithValue("$t", content);
            cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();

            using var idCmd = conn.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public int SaveWithFile(int conversationId, int senderId, int fileId, string fileName)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Messages (ConversationId, SenderId, Kind, Content, FileId, SentAt)
                                VALUES ($c, $s, 'File', $t, $f, $d);";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$s", senderId);
            cmd.Parameters.AddWithValue("$t", fileName);
            cmd.Parameters.AddWithValue("$f", fileId);
            cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();

            using var idCmd = conn.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public List<ChatMessage> GetHistory(int conversationId, int beforeId, int limit)
        {
            var list = new List<ChatMessage>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT m.MessageId, m.ConversationId, m.SenderId, u.Username, m.Kind, m.Content, m.SentAt
                FROM Messages m LEFT JOIN Users u ON u.UserId = m.SenderId
                WHERE m.ConversationId = $c AND m.MessageId < $b
                ORDER BY m.MessageId DESC LIMIT $l;";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$b", beforeId);
            cmd.Parameters.AddWithValue("$l", limit);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new ChatMessage
                {
                    MessageId = r.GetInt32(0),
                    ConversationId = r.GetInt32(1),
                    SenderId = r.IsDBNull(2) ? 0 : r.GetInt32(2),
                    SenderName = r.IsDBNull(3) ? "(system)" : r.GetString(3),
                    Kind = r.GetString(4),
                    Content = r.GetString(5),
                    SentAt = DateTime.Parse(r.GetString(6))
                });
            }
            list.Reverse();
            return list;
        }
    }
}