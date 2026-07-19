/* MultiRoblox — EN / РУ / 中文 (same switcher pattern as AbraKadaVra) */

export const LANGS = [
  { id: "en", label: "EN" },
  { id: "ru", label: "РУ" },
  { id: "zh", label: "中文" },
];

const dict = {
  en: {
    docTitle: "MultiRoblox — Run two Roblox clients at once",
    docDesc:
      "MultiRoblox lets you run two Roblox clients side by side on one PC. Free, open source, no admin required.",
    badge: "Free · Open source · No admin needed",
    tagHtml:
      'Run <strong>two Roblox clients</strong> at the same time on one PC.',
    download: "↓ Download for Windows",
    hintHtml:
      'One installer with auto-update and shortcuts. Windows 10/11. See the <a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">latest release</a>.',
    card1Title: "One click",
    card1Body:
      'Open the app and press “Launch 2 Clients”. Two Roblox windows open, ready for two accounts.',
    card2Title: "Live status",
    card2Body:
      "See exactly which clients are running and stop them all with a single button.",
    card3Title: "Safe & simple",
    card3Body:
      "No files patched, no admin rights. It just holds Roblox’s single-instance lock while open.",
    howTitle: "How to use",
    how1Html:
      "Download and run the <strong>installer</strong> — it installs MultiRoblox and keeps it up to date.",
    how2Html:
      "Click <strong>Launch 2 Clients</strong>. The classic player and the Microsoft Store client both open.",
    how3Html:
      "Sign into a <strong>different Roblox account</strong> in each window and join a game.",
    how4Html:
      "Keep the controller window open while you play. Use <strong>Stop All Clients</strong> when finished.",
    psTitle: "Install from PowerShell",
    psLead:
      "Prefer a command line? This downloads the latest release and adds shortcuts:",
    reqTitle: "Requirements",
    req1: "Windows 10 or 11.",
    req2Html:
      'The <strong>classic Roblox player</strong> (from roblox.com) <em>and</em> the <strong>Microsoft Store Roblox app</strong> both installed — this pairing is what allows two clients to stay open.',
    footerHtml:
      'MultiRoblox is an independent tool and is not affiliated with or endorsed by Roblox Corporation.<br />Source code on <a href="https://github.com/HyperlinksSpace/MultiRoblox">GitHub</a>.',
  },
  ru: {
    docTitle: "MultiRoblox — два клиента Roblox одновременно",
    docDesc:
      "MultiRoblox позволяет запускать два клиента Roblox на одном ПК. Бесплатно, с открытым кодом, без прав администратора.",
    badge: "Бесплатно · Open source · Без прав админа",
    tagHtml:
      'Запускайте <strong>два клиента Roblox</strong> одновременно на одном ПК.',
    download: "↓ Скачать для Windows",
    hintHtml:
      'Один установщик с автообновлением и ярлыками. Windows 10/11. Смотрите <a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">последний релиз</a>.',
    card1Title: "В один клик",
    card1Body:
      'Откройте приложение и нажмите «Launch 2 Clients». Откроются два окна Roblox — для двух аккаунтов.',
    card2Title: "Живой статус",
    card2Body:
      "Видно, какие клиенты запущены, и можно остановить все одной кнопкой.",
    card3Title: "Безопасно и просто",
    card3Body:
      "Никаких патчей файлов и прав администратора. Приложение лишь удерживает блокировку одного экземпляра Roblox, пока оно открыто.",
    howTitle: "Как пользоваться",
    how1Html:
      "Скачайте и запустите <strong>установщик</strong> — он поставит MultiRoblox и будет обновлять его.",
    how2Html:
      "Нажмите <strong>Launch 2 Clients</strong>. Откроются классический плеер и клиент из Microsoft Store.",
    how3Html:
      "Войдите в <strong>разные аккаунты Roblox</strong> в каждом окне и зайдите в игру.",
    how4Html:
      "Держите окно контроллера открытым во время игры. Нажмите <strong>Stop All Clients</strong>, когда закончите.",
    psTitle: "Установка через PowerShell",
    psLead:
      "Предпочитаете командную строку? Эта команда скачает последний релиз и создаст ярлыки:",
    reqTitle: "Требования",
    req1: "Windows 10 или 11.",
    req2Html:
      'Установлены и <strong>классический Roblox Player</strong> (с roblox.com), <em>и</em> <strong>приложение Roblox из Microsoft Store</strong> — именно эта пара позволяет держать два клиента открытыми.',
    footerHtml:
      'MultiRoblox — независимый инструмент и не связан с Roblox Corporation и не одобрен ею.<br />Исходный код на <a href="https://github.com/HyperlinksSpace/MultiRoblox">GitHub</a>.',
  },
  zh: {
    docTitle: "MultiRoblox — 同时运行两个 Roblox 客户端",
    docDesc:
      "MultiRoblox 可在一台电脑上并排运行两个 Roblox 客户端。免费、开源、无需管理员权限。",
    badge: "免费 · 开源 · 无需管理员",
    tagHtml: "在一台电脑上同时运行<strong>两个 Roblox 客户端</strong>。",
    download: "↓ 下载 Windows 版",
    hintHtml:
      '一键安装程序，含自动更新与快捷方式。支持 Windows 10/11。查看<a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">最新版本</a>。',
    card1Title: "一键启动",
    card1Body:
      "打开应用并点击 “Launch 2 Clients”。会打开两个 Roblox 窗口，可登录两个账号。",
    card2Title: "实时状态",
    card2Body: "清楚查看正在运行的客户端，并用一个按钮全部停止。",
    card3Title: "安全简单",
    card3Body:
      "不修改游戏文件，不需要管理员权限。只是在窗口打开时占用 Roblox 的单实例锁。",
    howTitle: "使用方法",
    how1Html:
      "下载并运行<strong>安装程序</strong>——它会安装 MultiRoblox 并保持更新。",
    how2Html:
      "点击 <strong>Launch 2 Clients</strong>。经典客户端与 Microsoft Store 客户端都会打开。",
    how3Html:
      "在每个窗口登录<strong>不同的 Roblox 账号</strong>并进入游戏。",
    how4Html:
      "游玩时保持控制器窗口打开。结束后使用 <strong>Stop All Clients</strong>。",
    psTitle: "通过 PowerShell 安装",
    psLead: "更喜欢命令行？这条命令会下载最新版本并创建快捷方式：",
    reqTitle: "系统要求",
    req1: "Windows 10 或 11。",
    req2Html:
      '需同时安装<strong>经典 Roblox 客户端</strong>（来自 roblox.com）<em>以及</em><strong>Microsoft Store 版 Roblox</strong>——这种组合才能让两个客户端同时保持打开。',
    footerHtml:
      'MultiRoblox 为独立工具，与 Roblox Corporation 无关，亦未获其认可。<br />源代码见 <a href="https://github.com/HyperlinksSpace/MultiRoblox">GitHub</a>。',
  },
};

const STORAGE_KEY = "multiroblox-lang";

export function getLang() {
  const saved = localStorage.getItem(STORAGE_KEY);
  if (saved && dict[saved]) return saved;
  const nav = (navigator.language || "en").toLowerCase();
  if (nav.startsWith("ru")) return "ru";
  if (nav.startsWith("zh")) return "zh";
  return "en";
}

export function setLang(id) {
  if (!dict[id]) return getLang();
  localStorage.setItem(STORAGE_KEY, id);
  return id;
}

export function t(lang) {
  return dict[lang] || dict.en;
}

export function applyI18n(lang) {
  const d = t(lang);
  document.documentElement.lang = lang === "zh" ? "zh-CN" : lang;
  document.title = d.docTitle;
  const meta = document.querySelector('meta[name="description"]');
  if (meta) meta.setAttribute("content", d.docDesc);

  document.querySelectorAll("[data-i18n]").forEach((el) => {
    const key = el.getAttribute("data-i18n");
    const value = key.split(".").reduce((acc, k) => (acc == null ? acc : acc[k]), d);
    if (typeof value === "string") {
      if (el.hasAttribute("data-i18n-html")) el.innerHTML = value;
      else el.textContent = value;
    }
  });

  document.querySelectorAll(".lang-switch [data-lang]").forEach((btn) => {
    const active = btn.getAttribute("data-lang") === lang;
    btn.classList.toggle("is-active", active);
    btn.setAttribute("aria-pressed", active ? "true" : "false");
  });
}

export function initI18n() {
  const lang = getLang();
  applyI18n(lang);

  document.querySelectorAll(".lang-switch [data-lang]").forEach((btn) => {
    btn.addEventListener("click", () => {
      const next = setLang(btn.getAttribute("data-lang"));
      applyI18n(next);
    });
  });

  return lang;
}
