// Explicit usings and a block-scoped namespace are used here (instead of relying on
// ImplicitUsings/file-scoped namespaces) because Unity compiles this file directly as part of
// the com.voidkargo.shared local package, ignoring this project's .csproj SDK settings, and
// Unity's compiler is pinned to C# 9.0 (file-scoped namespaces need C# 10+).
using System.Collections.Generic;
using System.Linq;

namespace Game.Shared.Art.Rendering
{
    /// <summary>
    /// The outcome of validating a genome, palette, or rendered sprite. Every validator in the
    /// art pipeline (see ADR 0006 - "validation is enforced as part of the pipeline, including
    /// accessibility") returns one of these instead of throwing, so callers (editor tooling,
    /// tests, future automated content ingestion) can present *all* problems at once.
    /// </summary>
    public sealed record ValidationResult(IReadOnlyList<string> Errors)
    {
        public bool IsValid => Errors.Count == 0;

        public static readonly ValidationResult Success = new ValidationResult(new List<string>());

        public static ValidationResult Failure(params string[] errors) => new ValidationResult(errors);

        public static ValidationResult Combine(params ValidationResult[] results) =>
            new ValidationResult(results.SelectMany(r => r.Errors).ToList());
    }
}
