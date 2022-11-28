$files = "";

foreach ($File in (Get-ChildItem -Path '.\test\coverage.opencover.xml' -Recurse -Force)) {
    if ($files.Length -gt 0) {
        $files += ";"
    }

    $files += "opencover=$File"
}

& csmacnz.Coveralls --useRelativePaths --multiple --input "$files"
