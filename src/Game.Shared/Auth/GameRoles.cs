// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
namespace Game.Shared.Auth
{
    /// <summary>
    /// The three authorization levels used across Game.Backend and Game.Website. Kept as a
    /// single shared source of truth so role names are never duplicated as magic strings —
    /// see docs/01-architecture/data-model.md.
    /// </summary>
    public static class GameRoles
    {
        /// <summary>Full administrative access — user/role management, all game data.</summary>
        public const string Admin = "Admin";

        /// <summary>A regular registered player — the default role for self-registration.</summary>
        public const string Player = "Player";

        /// <summary>Elevated in-game moderation/support powers, short of full Admin.</summary>
        public const string GameMaster = "GameMaster";

        /// <summary>All known roles, in the order they should be seeded/displayed.</summary>
        public static readonly string[] All = { Admin, GameMaster, Player };
    }
}
