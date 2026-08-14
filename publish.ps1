param(
    [Parameter(Mandatory = $true)]
    [string]$m
)

Remove-Item -Recurse -Force .\obj, .\bin -ErrorAction SilentlyContinue

git add .
git commit -m $m
git push