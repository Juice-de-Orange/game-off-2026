# SessionStart hook: prints a short team briefing into Claude's context.
# Read-only: fetches, never pulls or modifies files. Must never fail the session.
$ErrorActionPreference = 'SilentlyContinue'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $root

$names = @{ 'Juice-de-Orange' = 'max'; 'iMaterix' = 'martin' }
$gitUser = (git config user.name) 2>$null
$me = $names[$gitUser]
if (-not $me) { $me = 'unknown' }
$other = switch ($me) { 'max' { 'martin' } 'martin' { 'max' } default { 'unknown' } }

function Get-Field($lines, $key) {
    $line = $lines | Where-Object { $_ -match "^${key}:\s*(.+)$" } | Select-Object -First 1
    if ($line -match "^${key}:\s*(.+)$") { return $Matches[1].Trim() }
    return ''
}

Write-Output "=== TEAM BRIEFING (SessionStart hook) ==="
Write-Output "You are working with: $me (git user '$gitUser'). Teammate: $other."
if ($me -eq 'unknown') {
    Write-Output "WARNING: git user.name is not a known team member - check 'git config user.name' (see CLAUDE.md)."
}

git fetch --quiet 2>$null
if ($LASTEXITCODE -eq 0) {
    $counts = (git rev-list --left-right --count 'HEAD...@{u}') 2>$null
    if ($counts -match '(\d+)\s+(\d+)') {
        Write-Output "Git: $($Matches[2]) commit(s) to pull, $($Matches[1]) local commit(s) not pushed."
    }
} else {
    Write-Output "Git: fetch failed (offline or not logged in) - state may be stale."
}
$dirty = @(git status --porcelain 2>$null).Count
if ($dirty -gt 0) { Write-Output "Git: $dirty uncommitted change(s) in the working tree." }
Write-Output "(Briefing reflects the local checkout; run /sync to pull and see the latest.)"

$qDir = Join-Path $root 'docs/team/questions'
$forMe = @(); $answered = @()
foreach ($f in (Get-ChildItem $qDir -Filter '*.md' -File)) {
    $lines = Get-Content $f.FullName -Encoding UTF8 -TotalCount 12
    $status = Get-Field $lines 'status'
    if ((Get-Field $lines 'an') -eq $me -and $status -eq 'offen') {
        $forMe += "  - $($f.Name) (blockiert: $(Get-Field $lines 'blockiert'))"
    }
    if ((Get-Field $lines 'von') -eq $me -and $status -eq 'beantwortet') {
        $answered += "  - $($f.Name)"
    }
}
if ($forMe.Count) { Write-Output "OPEN QUESTIONS FOR ${me}:"; $forMe | Write-Output }
else { Write-Output "Open questions for ${me}: none." }
if ($answered.Count) { Write-Output "ANSWERED QUESTIONS (asked by $me, apply + set status: erledigt):"; $answered | Write-Output }

$hDir = Join-Path $root 'docs/team/handovers'
$latest = Get-ChildItem $hDir -Filter "*_$other.md" -File | Sort-Object Name -Descending | Select-Object -First 1
if ($latest) { Write-Output "Latest handover from ${other}: docs/team/handovers/$($latest.Name)" }
else { Write-Output "No handover from $other yet." }

$board = Join-Path $root 'docs/team/BOARD.md'
if (Test-Path $board) {
    $rows = Get-Content $board -Encoding UTF8 |
        Select-String -Pattern '^## In Arbeit' -Context 0, 30 |
        ForEach-Object { $_.Context.PostContext } |
        Where-Object { $_ -match '^\|\s*(max|martin)\s*\|' }
    if ($rows) { Write-Output "Claims (BOARD 'In Arbeit'):"; $rows | ForEach-Object { Write-Output "  $_" } }
    else { Write-Output "Claims: none." }
}
Write-Output "Follow the session rhythm in CLAUDE.md: greet the person in German, then run /sync."
Write-Output "=== END TEAM BRIEFING ==="
exit 0
