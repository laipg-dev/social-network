$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

if (-not (Test-Path .\.env)) {
    throw "Không tìm thấy backend\.env. Hãy tạo file này từ backend\.env.example."
}

Get-Content .\.env | ForEach-Object {
    $line = $_.Trim()
    if ($line -and -not $line.StartsWith("#") -and $line -match '^([^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable(
            $matches[1].Trim(),
            $matches[2].Trim(),
            "Process")
    }
}

if ($env:Email__Enabled -eq "true" -and
    ($env:Email__ApiKey -eq "your-brevo-api-key" -or
     $env:Email__FromAddress -eq "your-verified-brevo-sender@example.com")) {
    Write-Warning "Brevo đang dùng placeholder. Hãy cập nhật Email__ApiKey và Email__FromAddress trong .env."
}

dotnet run `
    --project .\src\SocialNetwork.Api\SocialNetwork.Api.csproj `
    --no-launch-profile `
    --urls http://localhost:5201
