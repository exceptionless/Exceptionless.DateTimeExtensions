$root_dir = Resolve-Path "$PSScriptRoot\..\"
Push-Location $root_dir

If (Test-Path $root_dir\artifacts) {
    Remove-Item $root_dir\artifacts -Force -Recurse
}

Get-ChildItem $root_dir\src -Include bin,obj -Recurse |
    ForEach-Object ($_) {
        Remove-Item $_.FullName -Force -Recurse
    }

Get-ChildItem $root_dir\test -Include bin,obj -Recurse |
    ForEach-Object ($_) {
        Remove-Item $_.FullName -Force -Recurse
    }