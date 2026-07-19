using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

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
    private readonly Label _versionLabel = new Label();
    private readonly Label _instancesHeading = new Label();
    private readonly Label _activityHeading = new Label();
    private readonly ListView _clientList = new ListView();
    private readonly TextBox _log = new TextBox();
    private readonly Button _launchButton = new Button();
    private readonly Button _stopButton = new Button();
    private readonly CheckBox _topMost = new CheckBox();
    private readonly Panel _header = new Panel();
    private readonly Panel _statusBar = new Panel();
    private readonly System.Windows.Forms.Timer _timer =
        new System.Windows.Forms.Timer();

    private static readonly Color BackColorDark = Color.FromArgb(14, 16, 22);
    private static readonly Color PanelColor = Color.FromArgb(28, 32, 42);
    private static readonly Color PanelElevated = Color.FromArgb(36, 41, 54);
    private static readonly Color AccentColor = Color.FromArgb(56, 152, 255);
    private static readonly Color AccentColor2 = Color.FromArgb(110, 188, 255);
    private static readonly Color TextColor = Color.FromArgb(236, 239, 246);
    private static readonly Color MutedColor = Color.FromArgb(138, 146, 164);
    private static readonly Color GoodColor = Color.FromArgb(72, 210, 140);
    private static readonly Color BadColor = Color.FromArgb(240, 110, 120);
    private static readonly Color BorderColor = Color.FromArgb(52, 58, 74);

    public ControllerForm()
    {
        Text = "MultiRoblox " + VersionText();
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
        }
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(560, 620);
        MinimumSize = new Size(500, 560);
        BackColor = BackColorDark;
        ForeColor = TextColor;
        Font = new Font("Segoe UI Semibold", 9.25f);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        Padding = new Padding(0);

        // Soft header band
        _header.Location = new Point(0, 0);
        _header.Size = new Size(ClientSize.Width, 108);
        _header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _header.BackColor = Color.FromArgb(18, 22, 32);
        _header.Paint += new PaintEventHandler(OnHeaderPaint);
        Controls.Add(_header);

        Label title = new Label();
        title.Text = "MultiRoblox";
        title.Font = new Font("Segoe UI", 26f, FontStyle.Bold);
        title.ForeColor = TextColor;
        title.AutoSize = true;
        title.Location = new Point(28, 22);
        title.BackColor = Color.Transparent;
        _header.Controls.Add(title);

        Label subtitle = new Label();
        subtitle.Text = "Unlimited Roblox windows · one click each";
        subtitle.AutoSize = true;
        subtitle.ForeColor = MutedColor;
        subtitle.Font = new Font("Segoe UI", 10f);
        subtitle.Location = new Point(30, 64);
        subtitle.BackColor = Color.Transparent;
        _header.Controls.Add(subtitle);

        _versionLabel.Text = VersionText();
        _versionLabel.AutoSize = true;
        _versionLabel.ForeColor = AccentColor2;
        _versionLabel.Font = new Font("Segoe UI Semibold", 9f);
        _versionLabel.BackColor = Color.FromArgb(32, 56, 152, 255);
        _versionLabel.Padding = new Padding(10, 4, 10, 4);
        _versionLabel.Location = new Point(ClientSize.Width - 92, 28);
        _versionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _header.Controls.Add(_versionLabel);

        _launchButton.Text = "Launch Instance";
        _launchButton.Size = new Size(504, 64);
        _launchButton.Location = new Point(28, 128);
        _launchButton.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
        _launchButton.FlatStyle = FlatStyle.Flat;
        _launchButton.FlatAppearance.BorderSize = 0;
        _launchButton.FlatAppearance.MouseOverBackColor = AccentColor2;
        _launchButton.FlatAppearance.MouseDownBackColor =
            Color.FromArgb(40, 130, 230);
        _launchButton.BackColor = AccentColor;
        _launchButton.ForeColor = Color.White;
        _launchButton.Font = new Font("Segoe UI Semibold", 15f);
        _launchButton.Cursor = Cursors.Hand;
        _launchButton.Click += new EventHandler(OnLaunchClick);
        Controls.Add(_launchButton);

        // Status pills row
        _statusBar.Location = new Point(28, 208);
        _statusBar.Size = new Size(504, 44);
        _statusBar.Anchor = AnchorStyles.Top | AnchorStyles.Left
            | AnchorStyles.Right;
        _statusBar.BackColor = BackColorDark;
        Controls.Add(_statusBar);

        StylePill(_countLabel, "0 instances");
        _countLabel.Location = new Point(0, 6);
        _statusBar.Controls.Add(_countLabel);

        StylePill(_guardLabel, "Lock: …");
        _guardLabel.Location = new Point(148, 6);
        _statusBar.Controls.Add(_guardLabel);

        _stopButton.Text = "Stop All";
        _stopButton.Size = new Size(110, 36);
        _stopButton.Location = new Point(394, 4);
        _stopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _stopButton.FlatStyle = FlatStyle.Flat;
        _stopButton.FlatAppearance.BorderSize = 1;
        _stopButton.FlatAppearance.BorderColor = BorderColor;
        _stopButton.FlatAppearance.MouseOverBackColor = PanelElevated;
        _stopButton.BackColor = PanelColor;
        _stopButton.ForeColor = TextColor;
        _stopButton.Font = new Font("Segoe UI Semibold", 9.5f);
        _stopButton.Cursor = Cursors.Hand;
        _stopButton.Click += new EventHandler(OnStopClick);
        _statusBar.Controls.Add(_stopButton);

        _topMost.Text = "  Keep on top";
        _topMost.AutoSize = true;
        _topMost.ForeColor = MutedColor;
        _topMost.Font = new Font("Segoe UI", 9.5f);
        _topMost.Location = new Point(28, 262);
        _topMost.CheckedChanged += new EventHandler(OnTopMostChanged);
        Controls.Add(_topMost);

        _instancesHeading.Text = "RUNNING INSTANCES";
        _instancesHeading.AutoSize = true;
        _instancesHeading.ForeColor = MutedColor;
        _instancesHeading.Font = new Font("Segoe UI Semibold", 8f);
        _instancesHeading.Location = new Point(30, 298);
        Controls.Add(_instancesHeading);

        _clientList.View = View.Details;
        _clientList.FullRowSelect = true;
        _clientList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _clientList.Location = new Point(28, 320);
        _clientList.Size = new Size(504, 150);
        _clientList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
            | AnchorStyles.Left | AnchorStyles.Right;
        _clientList.BackColor = PanelColor;
        _clientList.ForeColor = TextColor;
        _clientList.BorderStyle = BorderStyle.None;
        _clientList.Font = new Font("Segoe UI", 9.5f);
        _clientList.OwnerDraw = true;
        _clientList.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(OnDrawColumnHeader);
        _clientList.DrawItem += new DrawListViewItemEventHandler(OnDrawItem);
        _clientList.DrawSubItem += new DrawListViewSubItemEventHandler(OnDrawSubItem);
        _clientList.Columns.Add("#", 44);
        _clientList.Columns.Add("PID", 90);
        _clientList.Columns.Add("Type", 120);
        _clientList.Columns.Add("Uptime", 230);
        Controls.Add(_clientList);

        _activityHeading.Text = "ACTIVITY";
        _activityHeading.AutoSize = true;
        _activityHeading.ForeColor = MutedColor;
        _activityHeading.Font = new Font("Segoe UI Semibold", 8f);
        _activityHeading.Location = new Point(30, 484);
        _activityHeading.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        Controls.Add(_activityHeading);

        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.BorderStyle = BorderStyle.None;
        _log.Location = new Point(28, 506);
        _log.Size = new Size(504, 90);
        _log.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            | AnchorStyles.Right;
        _log.BackColor = Color.FromArgb(10, 12, 16);
        _log.ForeColor = Color.FromArgb(130, 210, 170);
        _log.Font = new Font("Consolas", 8.5f);
        Controls.Add(_log);

        _timer.Interval = 1500;
        _timer.Tick += new EventHandler(OnTimerTick);
        _timer.Start();

        Load += new EventHandler(OnLoad);
        FormClosing += new FormClosingEventHandler(OnClosing);
        Resize += new EventHandler(OnFormResize);
    }

    private static void StylePill(Label pill, string text)
    {
        pill.Text = text;
        pill.AutoSize = true;
        pill.Font = new Font("Segoe UI Semibold", 9f);
        pill.ForeColor = TextColor;
        pill.BackColor = PanelColor;
        pill.Padding = new Padding(12, 8, 12, 8);
    }

    private void OnHeaderPaint(object sender, PaintEventArgs e)
    {
        Rectangle r = _header.ClientRectangle;
        using (LinearGradientBrush brush = new LinearGradientBrush(
            r,
            Color.FromArgb(22, 28, 42),
            Color.FromArgb(14, 16, 22),
            LinearGradientMode.Vertical))
        {
            e.Graphics.FillRectangle(brush, r);
        }
        using (Pen pen = new Pen(Color.FromArgb(40, 56, 152, 255), 2f))
        {
            e.Graphics.DrawLine(pen, 0, r.Bottom - 1, r.Right, r.Bottom - 1);
        }
    }

    private void OnFormResize(object sender, EventArgs e)
    {
        // Keep version chip tucked into the header's right edge.
        _versionLabel.Left = Math.Max(200, _header.Width - _versionLabel.Width - 28);
        _stopButton.Left = Math.Max(280, _statusBar.Width - _stopButton.Width);
    }

    private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
    {
        using (SolidBrush bg = new SolidBrush(PanelElevated))
            e.Graphics.FillRectangle(bg, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            e.Header.Text,
            new Font("Segoe UI Semibold", 8.25f),
            e.Bounds,
            MutedColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.LeftAndRightPadding);
    }

    private void OnDrawItem(object sender, DrawListViewItemEventArgs e)
    {
        e.DrawDefault = false;
    }

    private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
    {
        Color row = (e.ItemIndex % 2 == 0) ? PanelColor : PanelElevated;
        using (SolidBrush bg = new SolidBrush(row))
            e.Graphics.FillRectangle(bg, e.Bounds);
        Color ink = e.ColumnIndex == 0 ? AccentColor2 : TextColor;
        TextRenderer.DrawText(
            e.Graphics,
            e.SubItem.Text,
            _clientList.Font,
            e.Bounds,
            ink,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                | TextFormatFlags.LeftAndRightPadding
                | TextFormatFlags.EndEllipsis);
    }

    private void OnLoad(object sender, EventArgs e)
    {
        // Roblox must not already own the singleton Event, otherwise the next
        // launch will take over and close the first client. Only when Roblox
        // holds that lock do we close the open clients to free it.
        if (EventNameHeldByRoblox())
        {
            int closed = StopAllPlayers();
            if (closed > 0)
                AppendLog("Closed " + closed
                    + " already-open Roblox window(s) so multi-instance can start.");
        }

        EnsureGuard(true);
        RefreshStatus();
        AppendLog("Ready. Your first window uses your saved account.");
        AppendLog("Every extra window opens at the login screen for another account.");

        StartUpdateCheck();
    }

    private void OnTopMostChanged(object sender, EventArgs e)
    {
        TopMost = _topMost.Checked;
    }

    // ----- Version & auto-update ---------------------------------------

    private const string UpdateRepo = "HyperlinksSpace/MultiRoblox";

    private static Version CurrentVersion()
    {
        return typeof(ControllerForm).Assembly.GetName().Version;
    }

    private static string VersionText()
    {
        Version v = CurrentVersion();
        if (v.Major == 0 && v.Minor == 0)
            return "dev";
        return "v" + v.Major + "." + v.Minor + "." + v.Build;
    }

    // Once at startup: ask GitHub for the newest release. If it is newer than
    // this build, download it and swap it in. A running exe can be renamed on
    // Windows, so the update takes effect the next time the app starts.
    private void StartUpdateCheck()
    {
        // Remove leftovers from a previous update.
        try
        {
            string old = Application.ExecutablePath + ".old";
            if (File.Exists(old)) File.Delete(old);
        }
        catch
        {
        }

        Thread checker = new Thread(delegate ()
        {
            try
            {
                CheckAndInstallUpdate();
            }
            catch (Exception ex)
            {
                AppendLog("Update check failed: " + ex.Message);
            }
        });
        checker.IsBackground = true;
        checker.Start();
    }

    private void CheckAndInstallUpdate()
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        string json;
        using (WebClient web = new WebClient())
        {
            web.Headers.Add("User-Agent", "MultiRoblox");
            web.Headers.Add("Accept", "application/vnd.github+json");
            json = web.DownloadString(
                "https://api.github.com/repos/" + UpdateRepo + "/releases/latest");
        }

        Match tagMatch = Regex.Match(json,
            "\"tag_name\"\\s*:\\s*\"v(\\d+)\\.(\\d+)\\.(\\d+)\"");
        if (!tagMatch.Success)
            return;

        Version latest = new Version(
            int.Parse(tagMatch.Groups[1].Value),
            int.Parse(tagMatch.Groups[2].Value),
            int.Parse(tagMatch.Groups[3].Value));
        Version current = CurrentVersion();

        // Dev builds (0.0.*) never self-update; release builds only move forward.
        if (current.Major == 0 || latest <= current)
            return;

        AppendLog("Update v" + latest.Major + "." + latest.Minor + "."
            + latest.Build + " found. Downloading...");

        string exe = Application.ExecutablePath;
        string fresh = exe + ".new";
        string url = "https://github.com/" + UpdateRepo
            + "/releases/latest/download/MultiRoblox.exe";
        using (WebClient web = new WebClient())
        {
            web.Headers.Add("User-Agent", "MultiRoblox");
            web.DownloadFile(url, fresh);
        }

        FileInfo info = new FileInfo(fresh);
        if (!info.Exists || info.Length < 50 * 1024)
        {
            try { File.Delete(fresh); } catch { }
            AppendLog("Update download looked wrong; keeping current version.");
            return;
        }

        // Swap: running exe is renamed aside, new one takes its place.
        string old = exe + ".old";
        try { if (File.Exists(old)) File.Delete(old); } catch { }
        File.Move(exe, old);
        try
        {
            File.Move(fresh, exe);
        }
        catch
        {
            // Roll back so the app still exists on disk.
            File.Move(old, exe);
            throw;
        }

        AppendLog("Updated to v" + latest.Major + "." + latest.Minor + "."
            + latest.Build + ". It will run the next time you open MultiRoblox.");
        BeginInvoke((MethodInvoker)delegate
        {
            _versionLabel.Text = VersionText() + "  (v" + latest.Major + "."
                + latest.Minor + "." + latest.Build + " on next start)";
        });
    }

    // Roblox refuses to open another client while these two named objects
    // already exist. By owning them ourselves for the app's lifetime, every
    // extra client is allowed to start. The second name is intentionally a
    // Mutex so Roblox's attempt to make an Event of the same name collides
    // and fails, which is what keeps additional instances alive.
    private void EnsureGuard(bool allowStopRoblox)
    {
        if (_guardActive && GuardStillOurs())
            return;

        ReleaseGuardHandles();

        // If Roblox already created ROBLOX_singletonEvent as an Event, we
        // cannot claim that name as a Mutex. Closing Roblox frees it.
        if (allowStopRoblox && EventNameHeldByRoblox())
        {
            int closed = StopAllPlayers();
            if (closed > 0)
                AppendLog("Freed singleton lock by closing " + closed
                    + " Roblox client(s).");
            Thread.Sleep(500);
        }

        try
        {
            bool a, b;
            _singletonMutex = CreateSharedMutex("ROBLOX_singletonMutex", out a);
            _singletonEventBlocker =
                CreateSharedMutex("ROBLOX_singletonEvent", out b);

            // Confirm the Event name is ours (a Mutex), not Roblox's Event.
            if (EventNameHeldByRoblox())
            {
                ReleaseGuardHandles();
                _guardActive = false;
                AppendLog("Could not take multi-instance lock. Close every "
                    + "Roblox window, then press Launch again.");
                return;
            }

            _guardActive = true;
            AppendLog("Multi-instance lock held.");
        }
        catch (Exception ex)
        {
            ReleaseGuardHandles();
            _guardActive = false;
            AppendLog("Guard error: " + ex.Message);
            AppendLog("Close every Roblox window, then press Launch again.");
        }
    }

    private void ReleaseGuardHandles()
    {
        try { if (_singletonMutex != null) _singletonMutex.Dispose(); }
        catch { }
        try { if (_singletonEventBlocker != null) _singletonEventBlocker.Dispose(); }
        catch { }
        _singletonMutex = null;
        _singletonEventBlocker = null;
        _guardActive = false;
    }

    // True when something (almost always Roblox) already created
    // ROBLOX_singletonEvent as an EventWaitHandle. In that state a second
    // client will tell the first one to quit.
    private static bool EventNameHeldByRoblox()
    {
        try
        {
            using (EventWaitHandle.OpenExisting("ROBLOX_singletonEvent"))
                return true;
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            return false;
        }
        catch
        {
            // Wrong type (our Mutex) or access denied — treat as not Roblox's Event.
            return false;
        }
    }

    private bool GuardStillOurs()
    {
        if (_singletonMutex == null || _singletonEventBlocker == null)
            return false;
        return !EventNameHeldByRoblox();
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
        // Never launch while Roblox still owns the singleton Event — that is
        // exactly the "first window quits" failure mode.
        EnsureGuard(CountPlayers() == 0);
        if (!_guardActive)
        {
            AppendLog("Launch blocked: multi-instance lock is not held.");
            return;
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
        int killed = StopAllPlayers();
        AppendLog("Stopped " + killed + " Roblox client(s).");
        RefreshStatus();
    }

    private static int StopAllPlayers()
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
        if (killed > 0)
        {
            // Give Windows a moment to release the named Event/Mutex.
            Thread.Sleep(800);
        }
        return killed;
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        if (_guardActive && EventNameHeldByRoblox())
        {
            _guardActive = false;
            AppendLog("Warning: Roblox took the singleton lock back. "
                + "Close all Roblox windows and press Launch again.");
        }
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
            ? "1 instance"
            : players.Length + " instances";
        _countLabel.ForeColor = players.Length > 0 ? AccentColor2 : MutedColor;
        _countLabel.BackColor = players.Length > 0
            ? Color.FromArgb(36, 56, 152, 255)
            : PanelColor;

        _guardLabel.Text = _guardActive ? "Lock ON" : "Lock OFF";
        _guardLabel.ForeColor = _guardActive ? GoodColor : BadColor;
        _guardLabel.BackColor = _guardActive
            ? Color.FromArgb(36, 72, 210, 140)
            : Color.FromArgb(36, 240, 110, 120);

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
        ReleaseGuardHandles();
    }
}

internal static class Program
{
    // Groups the window under one pinned Start Menu / taskbar identity so
    // Windows picks our embedded multi-size icon instead of a generic one.
    private const string AppUserModelId = "HyperlinksSpace.MultiRoblox";

    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    [STAThread]
    private static void Main()
    {
        try { SetCurrentProcessExplicitAppUserModelID(AppUserModelId); }
        catch { }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ControllerForm());
    }
}
