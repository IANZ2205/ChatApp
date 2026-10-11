using System;
using System.Collections.Generic;
using ChatApp.Common.Protocol;
using Microsoft.Data.Sqlite;

namespace ChatApp.Server.Data
{
    public class FriendRepository
    {
        public enum AddResult { Ok, AlreadyFriends, AlreadyPending, NotFound }

        public (AddResult, int, int) SendRequest(int requesterId, string targetUsername)
        {
            using var conn = Database.Open();

            int targetId;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT UserId FROM Users WHERE Username = $u;";
                cmd.Parameters.AddWithValue("$u", targetUsername);
                var res = cmd.ExecuteScalar();
                if (res == null) return (AddResult.NotFound, 0, 0);
                targetId = Convert.ToInt32(res);
            }
            if (targetId == requesterId) return (AddResult.NotFound, 0, 0);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT FriendshipId, Status FROM Friendships
                                    WHERE (RequesterId=$a AND AddresseeId=$b)
                                       OR (RequesterId=$b AND AddresseeId=$a) LIMIT 1;";
                cmd.Parameters.AddWithValue("$a", requesterId);
                cmd.Parameters.AddWithValue("$b", targetId);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    string status = r.GetString(1);
                    if (status == "Accepted") return (AddResult.AlreadyFriends, 0, targetId);
                    if (status == "Pending") return (AddResult.AlreadyPending, 0, targetId);
                }
            }

            int newId;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Friendships (RequesterId, AddresseeId, Status, CreatedAt)
                                    VALUES ($a, $b, 'Pending', $d);";
                cmd.Parameters.AddWithValue("$a", requesterId);
                cmd.Parameters.AddWithValue("$b", targetId);
                cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT last_insert_rowid();";
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return (AddResult.Ok, newId, targetId);
        }

        public (bool ok, int otherUserId) Respond(int friendshipId, int userId, bool accept)
        {
            using var conn = Database.Open();
            int requesterId = 0, addresseeId = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT RequesterId, AddresseeId FROM Friendships WHERE FriendshipId = $id AND Status='Pending';";
                cmd.Parameters.AddWithValue("$id", friendshipId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return (false, 0);
                requesterId = r.GetInt32(0);
                addresseeId = r.GetInt32(1);
            }
            if (addresseeId != userId) return (false, 0);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Friendships SET Status=$s, RespondedAt=$d WHERE FriendshipId=$id;";
                cmd.Parameters.AddWithValue("$s", accept ? "Accepted" : "Declined");
                cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
                cmd.Parameters.AddWithValue("$id", friendshipId);
                cmd.ExecuteNonQuery();
            }
            return (true, requesterId);
        }

        public List<FriendInfo> GetFriends(int userId)
        {
            var list = new List<FriendInfo>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.UserId, u.Username
                FROM Friendships f
                JOIN Users u ON u.UserId = CASE
                    WHEN f.RequesterId = $me THEN f.AddresseeId
                    ELSE f.RequesterId END
                WHERE f.Status = 'Accepted'
                  AND (f.RequesterId = $me OR f.AddresseeId = $me);";
            cmd.Parameters.AddWithValue("$me", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new FriendInfo
                {
                    UserId = r.GetInt32(0),
                    Username = r.GetString(1)
                });
            }
            return list;
        }

        public List<PendingRequestInfo> GetPendingIncoming(int userId)
        {
            var list = new List<PendingRequestInfo>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT f.FriendshipId, u.UserId, u.Username, f.CreatedAt
                FROM Friendships f
                JOIN Users u ON u.UserId = f.RequesterId
                WHERE f.AddresseeId = $me AND f.Status = 'Pending'
                ORDER BY f.CreatedAt DESC;";
            cmd.Parameters.AddWithValue("$me", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new PendingRequestInfo
                {
                    FriendshipId = r.GetInt32(0),
                    RequesterId = r.GetInt32(1),
                    RequesterUsername = r.GetString(2),
                    CreatedAt = r.GetString(3)
                });
            }
            return list;
        }
    }
}