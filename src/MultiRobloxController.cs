using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

// MultiRoblox Controller
// One window, one button. Each press launches another Roblox client, so you
// can run as many instances as you want. It holds Roblox's singleton kernel
// objects for the app's lifetime, which is what allows more than one client
// to stay open at the same time.
internal sealed class ControllerForm : Form
{
    private Mutex _singletonMutex;
    private Mutex _singletonEventBlocker;
    private bool _guardActive;
    private int _launchCount;

    private readonly Label _countLabel = new Label();
    private readonly Label _guardLabel = new Label();
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
    private static readonly Color AccentColor2 = Color.FromArgb(109, 179, 255);
    private static readonly Color TextColor = Color.FromArgb(232, 234, 240);
    private static readonly Color MutedColor = Color.FromArgb(160, 165, 178);

    public ControllerForm()
    {
        Text = "MultiRoblox";
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
        }
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 520);
        MinimumSize = new Size(460, 480);
        BackColor = BackColorDark;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 9f);

        Label title = new Label();
        title.Text = "MultiRoblox";
        title.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
        title.ForeColor = AccentColor;
        title.AutoSize = true;
        title.Location = new Point(20, 16);
        Controls.Add(title);

        Label subtitle = new Label();
        subtitle.Text = "Launch as many Roblox instances as you want.";
        subtitle.AutoSize = true;
        subtitle.ForeColor = MutedColor;
        subtitle.Location = new Point(22, 56);
        Controls.Add(subtitle);

        _launchButton.Text = "Launch Instance";
        _launchButton.Size = new Size(476, 68);
        _launchButton.Location = new Point(22, 92);
        _launchButton.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
        _launchButton.FlatStyle = FlatStyle.Flat;
        _launchButton.FlatAppearance.BorderSize = 0;
        _launchButton.BackColor = AccentColor;
        _launchButton.ForeColor = Color.White;
        _launchButton.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        _launchButton.Cursor = Cursors.Hand;
        _launchButton.Click += new EventHandler(OnLaunchClick);
        _launchButton.MouseEnter += delegate { _launchButton.BackColor = AccentColor2; };
        _launchButton.MouseLeave += delegate { _launchButton.BackColor = AccentColor; };
        Controls.Add(_launchButton);

        _countLabel.AutoSize = true;
        _countLabel.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        _countLabel.Location = new Point(22, 176);
        Controls.Add(_countLabel);

        _guardLabel.AutoSize = true;
        _guardLabel.Location = new Point(22, 202);
        Controls.Add(_guardLabel);

        _stopButton.Text = "Stop All";
        _stopButton.Size = new Size(120, 32);
        _stopButton.Location = new Point(378, 176);
        _stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _stopButton.FlatStyle = FlatStyle.Flat;
        _stopButton.FlatAppearance.BorderSize = 1;
        _stopButton.FlatAppearance.BorderColor = Color.FromArgb(90, 96, 110);
        _stopButton.BackColor = PanelColor;
        _stopButton.ForeColor = TextColor;
        _stopButton.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        _stopButton.Cursor = Cursors.Hand;
        _stopButton.Click += new EventHandler(OnStopClick);
        Controls.Add(_stopButton);

        _topMost.Text = "Keep window on top";
        _topMost.AutoSize = true;
        _topMost.ForeColor = MutedColor;
        _topMost.Location = new Point(22, 230);
        _topMost.CheckedChanged += new EventHandler(OnTopMostChanged);
        Controls.Add(_topMost);

        _clientList.View = View.Details;
        _clientList.FullRowSelect = true;
        _clientList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _clientList.Location = new Point(22, 260);
        _clientList.Size = new Size(476, 138);
        _clientList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left | AnchorStyles.Right;
        _clientList.BackColor = PanelColor;
        _clientList.ForeColor = TextColor;
        _clientList.BorderStyle = BorderStyle.FixedSingle;
        _clientList.Columns.Add("#", 40);
        _clientList.Columns.Add("PID", 90);
        _clientList.Columns.Add("Type", 120);
        _clientList.Columns.Add("Uptime", 210);
        Controls.Add(_clientList);

        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.Location = new Point(22, 408);
        _log.Size = new Size(476, 92);
        _log.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            | AnchorStyles.Right;
        _log.BackColor = Color.FromArgb(16, 18, 22);
        _log.ForeColor = Color.FromArgb(150, 200, 150);
        _log.Font = new Font("Consolas", 8.5f);
        Controls.Add(_log);

        _timer.Interval = 1500;
        _timer.Tick += new EventHandler(OnTimerTick);
        _timer.Start();

        Load += new EventHandler(OnLoad);
        FormClosing += new FormClosingEventHandler(OnClosing);
    }

    private void OnLoad(object sender, EventArgs e)
    {
        AcquireGuard();
        RefreshStatus();
        AppendLog("Ready. Your first window uses your saved account.");
        AppendLog("Every extra window opens at the login screen for another account.");
    }

    private void OnTopMostChanged(object sender, EventArgs e)
    {
        TopMost = _topMost.Checked;
    }

    // Roblox refuses to open another client while these two named objects
    // already exist. By owning them ourselves for the app's lifetime, every
    // extra client is allowed to start. The second name is intentionally a
    // Mutex so Roblox's attempt to make an Event of the same name collides
    // and fails, which is what keeps additional instances alive.
    private void AcquireGuard()
    {
        try
        {
            bool a, b;
            _singletonMutex = CreateSharedMutex("ROBLOX_singletonMutex", out a);
            _singletonEventBlocker =
                CreateSharedMutex("ROBLOX_singletonEvent", out b);
            _guardActive = true;
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

        // The very first window keeps your saved login. Every window after
        // that opens at the Roblox login screen so you can sign into a
        // different account, without disturbing the accounts already open.
        bool freshLogin = _launchCount >= 1 || CountPlayers() >= 1;
        _launchCount++;

        _launchButton.Enabled = false;
        _launchButton.Text = "Launching...";

        // Run on a worker thread: the logged-out launch briefly waits while
        // the new client reads its files, and we don't want to freeze the UI.
        Thread worker = new Thread(delegate ()
        {
            try
            {
                LaunchOneInstance(freshLogin);
            }
            finally
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    _launchButton.Enabled = true;
                    _launchButton.Text = "Launch Instance";
                    RefreshStatus();
                });
            }
        });
        worker.IsBackground = true;
        worker.SetApartmentState(ApartmentState.STA);
        worker.Start();
    }

    // Each press starts one more Roblox player. Prefer the classic desktop
    // player; fall back to the Microsoft Store client if that's all that's
    // installed. When freshLogin is true the new window starts logged out.
    private void LaunchOneInstance(bool freshLogin)
    {
        string player = FindClassicPlayer();
        if (player == null)
        {
            LaunchStoreFallback();
            return;
        }

        // Roblox reads RobloxCookies.dat at startup. If we hide it before
        // launching, the new window comes up at the login screen while the
        // clients already running keep their sessions in memory. We keep the
        // file hidden until the new window has actually reached its login
        // screen, then put it back so your saved account still works next time.
        string cookie = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "Roblox\\LocalStorage\\RobloxCookies.dat");
        string held = cookie + ".mrhold";
        bool moved = false;

        var existing = new System.Collections.Generic.HashSet<int>();
        foreach (Process p in Process.GetProcessesByName("RobloxPlayerBeta"))
            existing.Add(p.Id);

        try
        {
            if (freshLogin && File.Exists(cookie))
            {
                try
                {
                    if (File.Exists(held)) File.Delete(held);
                    File.Move(cookie, held);
                    moved = true;
                }
                catch (Exception ex)
                {
                    AppendLog("Could not open a logged-out window ("
                        + ex.Message + "). Opening normally instead.");
                }
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo(player, "-app");
                info.UseShellExecute = true;
                Process.Start(info);
                AppendLog(freshLogin
                    ? "Opening a new window at the login screen..."
                    : "Opened your Roblox window (saved account).");
            }
            catch (Exception ex)
            {
                AppendLog("Classic launch failed: " + ex.Message);
                LaunchStoreFallback();
            }

            // Wait until the brand-new client shows a window (it has reached
            // the login screen by then). Only after that do we restore the
            // cookie, so the new instance can't pick the old session back up.
            if (moved)
            {
                if (WaitForNewWindow(existing, 60))
                    AppendLog("Login screen ready. Sign into another account.");
                else
                    AppendLog("New window is taking a while; sign in when it "
                        + "appears.");
                // Small extra margin past the auth read.
                System.Threading.Thread.Sleep(3000);
            }
        }
        finally
        {
            if (moved)
            {
                try
                {
                    if (File.Exists(cookie)) File.Delete(cookie);
                    File.Move(held, cookie);
                }
                catch (Exception ex)
                {
                    AppendLog("Warning: could not restore your saved login file ("
                        + ex.Message + "). It is safe at " + held);
                }
            }
        }
    }

    // Polls for a RobloxPlayerBeta process that didn't exist before the launch
    // and now has a visible main window. Returns true once found.
    private static bool WaitForNewWindow(
        System.Collections.Generic.HashSet<int> existing, int timeoutSeconds)
    {
        DateTime deadline = DateTime.Now.AddSeconds(timeoutSeconds);
        while (DateTime.Now < deadline)
        {
            foreach (Process p in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                if (existing.Contains(p.Id))
                    continue;
                try
                {
                    p.Refresh();
                    if (p.MainWindowHandle != IntPtr.Zero
                        && !string.IsNullOrEmpty(p.MainWindowTitle))
                        return true;
                }
                catch
                {
                }
            }
            System.Threading.Thread.Sleep(1000);
        }
        return false;
    }

    private void LaunchStoreFallback()
    {
        try
        {
            const string storeAppId =
                "ROBLOXCorporation.RobloxGDK_55nm5eh3cm0pr!Game";
            ProcessStartInfo info = new ProcessStartInfo("explorer.exe",
                "shell:AppsFolder\\" + storeAppId);
            info.UseShellExecute = true;
            Process.Start(info);
            AppendLog("Started the Microsoft Store Roblox client.");
        }
        catch (Exception ex)
        {
            AppendLog("Could not launch Roblox. Install it from roblox.com. ("
                + ex.Message + ")");
        }
    }

    private static string FindClassicPlayer()
    {
        string versions = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
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
        foreach (Process process in
            Process.GetProcessesByName("RobloxPlayerBeta"))
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

    private static int CountPlayers()
    {
        return Process.GetProcessesByName("RobloxPlayerBeta").Length;
    }

    private void RefreshStatus()
    {
        Process[] players = Process.GetProcessesByName("RobloxPlayerBeta");

        _countLabel.Text = players.Length == 1
            ? "1 Roblox instance running"
            : players.Length + " Roblox instances running";
        _countLabel.ForeColor = players.Length > 0 ? AccentColor2 : MutedColor;

        _guardLabel.Text = _guardActive
            ? "Multi-instance mode: ON"
            : "Multi-instance mode: off";
        _guardLabel.ForeColor = _guardActive
            ? Color.FromArgb(120, 220, 130)
            : Color.FromArgb(230, 120, 120);

        _clientList.BeginUpdate();
        _clientList.Items.Clear();
        int index = 1;
        foreach (Process process in players)
        {
            string path = SafePath(process);
            string kind = path != null
                && path.IndexOf("WindowsApps",
                    StringComparison.OrdinalIgnoreCase) >= 0
                ? "Store"
                : (path == null ? "Roblox" : "Classic");

            string uptime = "";
            try
            {
                TimeSpan up = DateTime.Now - process.StartTime;
                uptime = up.Hours > 0
                    ? string.Format("{0}h {1}m {2}s", up.Hours, up.Minutes, up.Seconds)
                    : string.Format("{0}m {1}s", up.Minutes, up.Seconds);
            }
            catch
            {
                uptime = "-";
            }

            ListViewItem item = new ListViewItem(index.ToString());
            item.SubItems.Add(process.Id.ToString());
            item.SubItems.Add(kind);
            item.SubItems.Add(uptime);
            _clientList.Items.Add(item);
            index++;
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
        if (_log.InvokeRequired)
        {
            _log.BeginInvoke((MethodInvoker)delegate { AppendLog(message); });
            return;
        }
        string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
        if (_log.TextLength > 0)
            _log.AppendText(Environment.NewLine);
        _log.AppendText(line);
    }

    private void OnClosing(object sender, FormClosingEventArgs e)
    {
        _timer.Stop();
        // Releasing the mutexes returns Roblox to single-instance mode.
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
