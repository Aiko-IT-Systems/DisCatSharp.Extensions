param
(
    [string] $DisCatSharpRepository
)

$ErrorActionPreference = "Stop"
$repositoryRoot = $PSScriptRoot
$toolingRoot = Join-Path $repositoryRoot "search-tooling"
$themePath = Join-Path $toolingRoot "DisCatSharp.Docs/dcs"

if ($DisCatSharpRepository)
{
    $resolvedRepository = (Resolve-Path -LiteralPath $DisCatSharpRepository).Path
    $resolvedTheme = Join-Path $resolvedRepository "DisCatSharp.Docs/dcs"
    if (-not (Test-Path -LiteralPath $resolvedTheme -PathType Container))
    {
        throw "The selected DisCatSharp repository does not contain DisCatSharp.Docs/dcs."
    }

    if (-not (Test-Path -LiteralPath $toolingRoot))
    {
        New-Item -ItemType Junction -Path $toolingRoot -Target $resolvedRepository | Out-Null
    }
}
elseif (-not (Test-Path -LiteralPath $themePath -PathType Container))
{
    git clone --depth 1 https://github.com/Aiko-IT-Systems/DisCatSharp.git $toolingRoot
    if ($LASTEXITCODE -ne 0)
    {
        throw "Unable to clone the shared DisCatSharp documentation tooling."
    }
}

if (-not (Test-Path -LiteralPath $themePath -PathType Container))
{
    throw "Shared DisCatSharp documentation theme not found at $themePath."
}

docfx (Join-Path $repositoryRoot "DisCatSharp.Extensions.Docs/docfx.json")
exit $LASTEXITCODE
