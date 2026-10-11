using System.Collections.Generic;
using ChatApp.Common.Protocol;
using ChatApp.Server.Data;

namespace ChatApp.Server.Services
{
    public class FriendService
    {
        private readonly FriendRepository _repo = new FriendRepository();

        public (FriendRepository.AddResult result, int friendshipId, int targetId) Send(int requesterId, string targetUsername)
            => _repo.SendRequest(requesterId, targetUsername);

        public (bool ok, int otherUserId) Respond(int friendshipId, int userId, bool accept)
            => _repo.Respond(friendshipId, userId, accept);

        public List<FriendInfo> GetFriends(int userId) => _repo.GetFriends(userId);

        public List<PendingRequestInfo> GetPendingIncoming(int userId) => _repo.GetPendingIncoming(userId);
    }
}