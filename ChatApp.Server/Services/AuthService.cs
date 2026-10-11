using System.Text.RegularExpressions;
using ChatApp.Common;
using ChatApp.Common.Protocol;
using ChatApp.Server.Data;
using ChatApp.Server.Security;

namespace ChatApp.Server.Services
{
    public class AuthService
    {
        private readonly UserRepository _users = new UserRepository();

        public AuthResult Register(RegisterRequest req)
        {
            var result = new AuthResult();

            string username = (req.Username ?? "").Trim();
            string fullName = (req.FullName ?? "").Trim();

            if (!Regex.IsMatch(username, "^[a-zA-Z0-9_]{3,20}$"))
            {
                result.ErrorCode = Constants.ErrInvalid;
                result.Message = "Username must be 3-20 letters, digits or underscores.";
                return result;
            }
            if (string.IsNullOrEmpty(req.Password) || req.Password.Length < 8)
            {
                result.ErrorCode = Constants.ErrInvalid;
                result.Message = "Password must be at least 8 characters.";
                return result;
            }
            if (fullName.Length == 0) fullName = username;

            if (_users.GetByUsername(username) != null)
            {
                result.ErrorCode = Constants.ErrUsernameTaken;
                result.Message = "That username is already taken.";
                return result;
            }

            var (hash, salt) = PasswordHasher.Hash(req.Password);
            int userId = _users.Create(username, hash, salt, fullName);

            int generalId = _users.GetGeneralConversationId();
            if (generalId > 0) _users.AddToConversation(userId, generalId);

            result.Success = true;
            result.UserId = userId;
            result.Username = username;
            result.FullName = fullName;
            return result;
        }

        public AuthResult Login(LoginRequest req)
        {
            var result = new AuthResult();

            string username = (req.Username ?? "").Trim();
            var user = _users.GetByUsername(username);
            if (user == null)
            {
                result.ErrorCode = Constants.ErrBadCredentials;
                result.Message = "Wrong username or password.";
                return result;
            }

            if (!PasswordHasher.Verify(req.Password ?? "", user.PasswordHash, user.PasswordSalt))
            {
                result.ErrorCode = Constants.ErrBadCredentials;
                result.Message = "Wrong username or password.";
                return result;
            }

            result.Success = true;
            result.UserId = user.UserId;
            result.Username = user.Username;
            result.FullName = user.FullName;
            return result;
        }
    }
}