namespace MyosotisWeb.Infrastructure
{
    public sealed class UserSessionContext
    {
        public const string ProtectionPurpose = "Myosotis.AnonymousUserId.v1";

        public Guid? UserId { get; private set; }

        public bool IsInitialized => UserId.HasValue && UserId.Value != Guid.Empty;

        public void Initialize(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (UserId.HasValue && UserId.Value != userId)
                throw new InvalidOperationException(
                    "User session has already been initialized with another ID.");

            UserId = userId;
        }
    }
}