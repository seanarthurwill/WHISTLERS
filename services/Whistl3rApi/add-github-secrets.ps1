# Quick Reference: Adding GitHub Secrets

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  GitHub Secrets Setup Guide" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Step 1: Go to GitHub Secrets page" -ForegroundColor Yellow
Write-Host "https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions`n" -ForegroundColor Cyan

Write-Host "Step 2: Click 'New repository secret'`n" -ForegroundColor Yellow

Write-Host "Step 3: Add the following secrets:`n" -ForegroundColor Yellow

Write-Host "Secret 1:" -ForegroundColor White
Write-Host "  Name: AWS_ACCESS_KEY_ID" -ForegroundColor Gray
Write-Host "  Value: (from .aws\github-credentials.txt)`n" -ForegroundColor Gray

Write-Host "Secret 2:" -ForegroundColor White
Write-Host "  Name: AWS_SECRET_ACCESS_KEY" -ForegroundColor Gray
Write-Host "  Value: (from .aws\github-credentials.txt)`n" -ForegroundColor Gray

Write-Host "Your credentials file location:" -ForegroundColor Yellow
Write-Host "  C:\dev\services\Whistl3rApi\.aws\github-credentials.txt`n" -ForegroundColor Cyan

Write-Host "Opening credentials file..." -ForegroundColor Gray
Start-Process notepad.exe -ArgumentList ".\.aws\github-credentials.txt"

Write-Host "`nOpening GitHub Secrets page in browser..." -ForegroundColor Gray
Start-Process "https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "After adding secrets:" -ForegroundColor Yellow
Write-Host "- Every push to 'master' will auto-deploy" -ForegroundColor White
Write-Host "- Monitor deployments at:" -ForegroundColor White
Write-Host "  https://github.com/seanarthurwill/WHISTLERS/actions" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan
