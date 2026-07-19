Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class Win {
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
}
"@

function Shot($name) {
    $b = [System.Windows.Forms.SystemInformation]::VirtualScreen
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.X, $b.Y, 0, 0, $bmp.Size)
    $bmp.Save("C:\1\1\1\1\1\MultiRoblox\$name", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

$procs = Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | Sort-Object StartTime
Write-Host "Roblox instances: $($procs.Count)"
$i = 1
foreach ($p in $procs) {
    if ($p.MainWindowHandle -ne 0) {
        [Win]::ShowWindow($p.MainWindowHandle, 9) | Out-Null  # restore
        [Win]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
        Start-Sleep 3
        Shot "shot-instance$i.png"
        Write-Host "instance $i pid=$($p.Id) title='$($p.MainWindowTitle)'"
    }
    $i++
}
