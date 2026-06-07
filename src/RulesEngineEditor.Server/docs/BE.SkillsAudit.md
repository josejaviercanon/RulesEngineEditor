# Skills Alignment Audit

## Overview
Audit of all installed opencode skill plugins and their alignment with the RulesEngineEditor ASP.NET Core 10 MVC WebAPI project, covering both Deepseek and Qwen agent models.

## Model-Agnostic Assessment
All skill plugins listed below are **model-agnostic** — their relevance does not change between Deepseek and Qwen agents. Skills provide framework and language-specific guidance that applies regardless of which LLM agent executes the task. The distinction between `deepseek-*.md` and `qwen-*.md` skill files is about model assignment and permissions, not about which opencode skills are needed.

## dotnet Plugin (3 skills)

| Skill | Relevance | Assessment |
|-------|-----------|------------|
| `csharp-scripts` | Relevant | Useful for quick C# experiments and prototyping outside the main project |
| `dotnet-pinvoke` | Low relevance | P/Invoke is unlikely needed for a Web API project; keep installed (no harm) |
| `nuget-trusted-publishing` | Relevant | Useful if publishing NuGet packages via CI/CD; aligns with deployment requirements |

## dotnet-aspnet Plugin (4 skills)

| Skill | Relevance | Assessment |
|-------|-----------|------------|
| `dotnet-webapi` | **Highly relevant** | Core skill for MVC WebAPI endpoint creation with correct HTTP semantics, OpenAPI metadata, and error handling |
| `configuring-opentelemetry-dotnet` | **Highly relevant** | Aligns with observability governance requirement; covers distributed tracing, metrics, logging |
| `minimal-api-file-upload` | Relevant | Useful if file upload endpoints are added in future; file-based workflow import features |
| `convert-blazor-server-to-webapp` | **Not relevant** | Project uses MVC WebAPI, not Blazor Server; skill can be safely ignored |

## dotnet-test Plugin (25 skills + 11 agents)

| Category | Skills | Relevance | Assessment |
|----------|--------|-----------|------------|
| Test execution | `run-tests`, `mtp-hot-reload` | **Highly relevant** | Core .NET test running with platform auto-detection and filter support |
| Test generation | `code-testing-agent`, `writing-mstest-tests` | Partially relevant | `code-testing-agent` is polyglot and useful; `writing-mstest-tests` not needed (xUnit project) |
| Test migration | `migrate-*` (7 skills) | Low relevance | No migration needed yet; useful if switching frameworks in future |
| Test quality | `test-anti-patterns`, `test-smell-detection`, `assertion-quality`, `test-gap-analysis`, `test-tagging`, `grade-tests` | **Highly relevant** | All 6 polyglot analysis skills are applicable for backend and frontend test suites |
| Coverage & risk | `coverage-analysis`, `crap-score` | **Highly relevant** | .NET coverage tools directly applicable; identify risk hotspots |
| Testability | `detect-static-dependencies`, `generate-testability-wrappers`, `migrate-static-to-wrapper` | **Highly relevant** | C# testability improvement tools for legacy/service code |
| Reference | `platform-detection`, `filter-syntax`, `dotnet-test-frameworks` | **Highly relevant** | Supporting data for test operations and framework detection |

## Coverage Gaps Identified

| Gap | Impact | Resolution |
|-----|--------|------------|
| No EF Core 10-specific skill | Medium | EF Core patterns documented in both `deepseek-builder.md` and `qwen-builder.md` agent skill files |
| No PostgreSQL/Npgsql skill | Low | Npgsql patterns documented in both `deepseek-builder.md` and `qwen-builder.md` |
| No OpenAPI/Scalar skill | Low | `dotnet-webapi` skill covers OpenAPI metadata; Scalar setup is in `Program.cs` |
| No CORS configuration skill | Low | CORS already configured in `Program.cs`; documented in debug guide |

## Recommendations

1. **Keep all installed plugins** — even low-relevance skills don't cause harm and may serve edge cases
2. **No new skill plugins required** — identified gaps are adequately addressed through the Deepseek and Qwen agent skill files (`.agents/skills/deepseek-*.md` and `.agents/skills/qwen-*.md`)
3. **Qwen skill files are sufficient** — Qwen agents share the same technology stack and coding conventions as Deepseek agents; skill files mirror the structure with model-specific assignments only
4. **Ignore `convert-blazor-server-to-webapp`** — not applicable to MVC WebAPI project
5. **Leverage `coverage-analysis` and `crap-score`** — these are particularly valuable for the .NET test suite to identify risk hotspots
6. **Use `detect-static-dependencies`** early in the project lifecycle to prevent testability debt
