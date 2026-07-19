using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

// MultiRoblox Controller
// A single-window app that lets you run two Roblox clients at once.
// It holds Roblox's singleton kernel objects (so a second client is allowed)
// and launches the classic desktop player plus the Microsoft Store client.
internal sealed class ControllerForm : Form
{
    private const string StoreAppId =
        "ROBLOXCorporation.RobloxGDK_55nm5eh3cm0pr!Game";

    private Mutex _singletonMutex;
    private Mutex _singletonEventBlocker;
    private bool _guardActive;

    private readonly Label _guardLabel = new Label();
    private readonly Label _countLabel = new Label();
    private readonly ListView _clientList = new ListView();
    private readonly TextBox _log = new TextBox();
    private readonly Button _launchButton = new Button();
    private readonly Button _stopButton = new Button();
    private readonly CheckBox _topMost = new CheckBox();
    private readonly System.Windows.Forms.Timer _timer =
        new System.Windows.Forms.Timer();

    private static readonly Color BackColorDark = Color.FromArgb(24, 26, 32);
    private static readonly Color PanelColor = Color.FromArgb(34, 37, 46);
    private static readonly Color AccentColor = Color.FromArgb(64, 156, 255);
    private static readonly Color TextColor = Color.FromArgb(232, 234, 240);

    public ControllerForm()
    {
        Text = "MultiRoblox Controller";
        try
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                System.Windows.Forms.Application.ExecutablePath);
        }
        catch
        {
        }
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 460);
        MinimumSize = new Size(520, 460);
        BackColor = BackColorDark;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9f);

        Label title = new Label();
        title.Text = "MultiRoblox";
        title.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
        title.ForeColor = AccentColor;
        title.AutoSize = true;
        title.Location = new Point(18, 14);
        Controls.Add(title);

        Label subtitle = new Label();
        subtitle.Text = "Run two Roblox clients at the same time.";
        subtitle.AutoSize = true;
        subtitle.ForeColor = Color.FromArgb(160, 165, 178);
        subtitle.Location = new Point(20, 48);
        Controls.Add(subtitle);

        _guardLabel.AutoSize = true;
        _guardLabel.Location = new Point(20, 78);
        Controls.Add(_guardLabel);

        _countLabel.AutoSize = true;
        _countLabel.Location = new Point(20, 100);
        Controls.Add(_countLabel);

        _launchButton.Text = "Launch 2 Clients";
        _launchButton.Size = new Size(220, 44);
        _launchButton.Location = new Point(20, 130);
        _launchButton.FlatStyle = FlatStyle.Flat;
        _launchButton.FlatAppearance.BorderSize = 0;
        _launchButton.BackColor = AccentColor;
        _launchButton.ForeColor = Color.White;
        _launchButton.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _launchButton.Click += new EventHandler(OnLaunchClick);
        Controls.Add(_launchButton);

        _stopButton.Text = "Stop All Clients";
        _stopButton.Size = new Size(220, 44);
        _stopButton.Location = new Point(260, 130);
        _stopButton.FlatStyle = FlatStyle.Flat;
        _stopButton.FlatAppearance.BorderSize = 1;
        _stopButton.FlatAppearance.BorderColor = Color.FromArgb(90, 96, 110);
        _stopButton.BackColor = PanelColor;
        _stopButton.ForeColor = TextColor;
        _stopButton.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        _stopButton.Click += new EventHandler(OnStopClick);
        Controls.Add(_stopButton);

        _topMost.Text = "Keep window on top";
        _topMost.AutoSize = true;
        _topMost.ForeColor = TextColor;
        _topMost.Location = new Point(20, 184);
        _topMost.CheckedChanged += new EventHandler(OnTopMostChanged);
        Controls.Add(_topMost);

        _clientList.View = View.Details;
        _clientList.FullRowSelect = true;
        _clientList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _clientList.Location = new Point(20, 212);
        _clientList.Size = new Size(460, 110);
        _clientList.BackColor = PanelColor;
        _clientList.ForeColor = TextColor;
        _clientList.BorderStyle = BorderStyle.FixedSingle;
        _clientList.Columns.Add("PID", 80);
        _clientList.Columns.Add("Client", 130);
        _clientList.Columns.Add("Path", 240);
        Controls.Add(_clientList);

        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Location = new Point(20, 332);
        _log.Size = new Size(460, 108);
        _log.BackColor = Color.FromArgb(16, 18, 22);
        _log.ForeColor = Color.FromArgb(150, 200, 150);
        _log.Font = new Font("Consolas", 8.5f);
        Controls.Add(_log);

        Resize += new EventHandler(OnResize);

        _timer.Interval = 2000;
        _timer.Tick += new EventHandler(OnTimerTick);
        _timer.Start();

        Load += new EventHandler(OnLoad);
        FormClosing += new FormClosingEventHandler(OnClosing);
    }

    private void OnResize(object sender, EventArgs e)
    {
        int width = ClientSize.Width - 40;
        if (width < 100) return;
        _clientList.Width = width;
        _log.Width = width;
        _clientList.Columns[2].Width =
            Math.Max(120, width - _clientList.Columns[0].Width
                - _clientList.Columns[1].Width - 4);
    }

    private void OnLoad(object sender, EventArgs e)
    {
        AcquireGuard();
        RefreshStatus();
    }

    private void OnTopMostChanged(object sender, EventArgs e)
    {
        TopMost = _topMost.Checked;
    }

    // Roblox refuses to open a second client while these two named objects
    // already exist. By owning them ourselves for the app's lifetime we let
    // multiple clients start. The second name is intentionally a Mutex so
    // Roblox's attempt to make an Event of the same name collides and fails.
    private void AcquireGuard()
    {
        try
        {
            bool a, b;
            _singletonMutex = CreateSharedMutex("ROBLOX_singletonMutex", out a);
            _singletonEventBlocker =
                CreateSharedMutex("ROBLOX_singletonEvent", out b);
            _guardActive = true;
            AppendLog("Singleton guard active. You can now launch clients.");
        }
        catch (Exception ex)
        {
            _guardActive = false;
            AppendLog("Guard error: " + ex.Message);
        }
    }

    private static Mutex CreateSharedMutex(string name, out bool created)
    {
        MutexSecurity security = new MutexSecurity();
        SecurityIdentifier everyone =
            new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        security.AddAccessRule(new MutexAccessRule(
            everyone, MutexRights.FullControl, AccessControlType.Allow));
        return new Mutex(true, name, out created, security);
    }

    private void OnLaunchClick(object sender, EventArgs e)
    {
        if (!_guardActive)
        {
            AcquireGuard();
        }

        _launchButton.Enabled = false;
        try
        {
            LaunchClassic();
            System.Threading.Thread.Sleep(800);
            LaunchStore();
            AppendLog("Launch requested. Sign into a different account in each "
                + "window. Give the second client a few seconds to appear.");
        }
        finally
        {
            _launchButton.Enabled = true;
            RefreshStatus();
        }
    }

    private void LaunchClassic()
    {
        string player = FindClassicPlayer();
        if (player == null)
        {
            AppendLog("Classic Roblox Player not found. Install Roblox from "
                + "roblox.com, then try again.");
            return;
        }
        try
        {
            ProcessStartInfo info = new ProcessStartInfo(player, "-app");
            info.UseShellExecute = true;
            Process.Start(info);
            AppendLog("Started classic client.");
        }
        catch (Exception ex)
        {
            AppendLog("Could not start classic client: " + ex.Message);
        }
    }

    private void LaunchStore()
    {
        try
        {
            ProcessStartInfo info = new ProcessStartInfo("explorer.exe",
                "shell:AppsFolder\\" + StoreAppId);
            info.UseShellExecute = true;
            Process.Start(info);
            AppendLog("Started Microsoft Store client.");
        }
        catch (Exception ex)
        {
            AppendLog("Could not start Store client. Install Roblox from the "
                + "Microsoft Store. (" + ex.Message + ")");
        }
    }

    private static string FindClassicPlayer()
    {
        string versions = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Roblox\\Versions");
        if (!Directory.Exists(versions))
            return null;

        string best = null;
        DateTime bestTime = DateTime.MinValue;
        try
        {
            string[] found = Directory.GetFiles(
                versions, "RobloxPlayerBeta.exe", SearchOption.AllDirectories);
            foreach (string path in found)
            {
                if (path.IndexOf("WindowsApps",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                DateTime time = File.GetLastWriteTime(path);
                if (time > bestTime)
                {
                    bestTime = time;
                    best = path;
                }
            }
        }
        catch
        {
        }
        return best;
    }

    private void OnStopClick(object sender, EventArgs e)
    {
        int killed = 0;
        foreach (Process process in Process.GetProcessesByName("RobloxPlayerBeta"))
        {
            try
            {
                process.Kill();
                killed++;
            }
            catch
            {
            }
        }
        AppendLog("Stopped " + killed + " Roblox client(s).");
        RefreshStatus();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        Process[] players = Process.GetProcessesByName("RobloxPlayerBeta");

        _guardLabel.Text = _guardActive
            ? "Guard: ACTIVE  (multi-client enabled)"
            : "Guard: inactive";
        _guardLabel.ForeColor = _guardActive
            ? Color.FromArgb(120, 220, 130)
            : Color.FromArgb(230, 120, 120);

        _countLabel.Text = "Roblox clients running: " + players.Length;

        _clientList.BeginUpdate();
        _clientList.Items.Clear();
        foreach (Process process in players)
        {
            string path = SafePath(process);
            string kind = path != null
                && path.IndexOf("WindowsApps",
                    StringComparison.OrdinalIgnoreCase) >= 0
                ? "Store"
                : (path == null ? "Unknown" : "Classic");
            ListViewItem item =
                new ListViewItem(process.Id.ToString());
            item.SubItems.Add(kind);
            item.SubItems.Add(path == null ? "(unavailable)" : path);
            _clientList.Items.Add(item);
        }
        _clientList.EndUpdate();
    }

    private static string SafePath(Process process)
    {
        try
        {
            return process.MainModule.FileName;
        }
        catch
        {
            return null;
        }
    }

    private void AppendLog(string message)
    {
        string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
        if (_log.TextLength > 0)
            _log.AppendText(Environment.NewLine);
        _log.AppendText(line);
    }

    private void OnClosing(object sender, FormClosingEventArgs e)
    {
        _timer.Stop();
        // Releasing the mutexes lets Roblox return to single-instance mode.
        try { if (_singletonMutex != null) _singletonMutex.Dispose(); }
        catch { }
        try { if (_singletonEventBlocker != null) _singletonEventBlocker.Dispose(); }
        catch { }
    }
}

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ControllerForm());
    }
}
