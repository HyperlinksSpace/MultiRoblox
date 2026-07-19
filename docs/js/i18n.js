/* MultiRoblox — EN / РУ / 中文 (same switcher pattern as AbraKadaVra) */

export const LANGS = [
  { id: "en", label: "EN" },
  { id: "ru", label: "РУ" },
  { id: "zh", label: "中文" },
];

const dict = {
  en: {
    docTitle: "MultiRoblox — Run as many Roblox instances as you want",
    docDesc:
      "MultiRoblox lets you run unlimited Roblox instances on one PC. One button per instance. Free, open source, no admin required.",
    badge: "Free · Open source · No admin needed",
    tagHtml:
      'Run <strong>as many Roblox instances</strong> as you want on one PC.',
    download: "↓ Download MultiRoblox.exe",
    hintHtml:
      'A single .exe — no install needed. Windows 10/11. See the <a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">latest release</a>.',
    card1Title: "One button",
    card1Body:
      'Press “Launch Instance” once per Roblox window you want. Every press opens another client.',
    card2Title: "Live status",
    card2Body:
      "See every running instance with its uptime and stop them all with a single button.",
    card3Title: "Safe & simple",
    card3Body:
      "No files patched, no admin rights. It just holds Roblox’s single-instance lock while open.",
    howTitle: "How to use",
    how1Html:
      "Download <strong>MultiRoblox.exe</strong> and run it.",
    how2Html:
      "Press <strong>Launch Instance</strong> — once for every Roblox window you want open.",
    how3Html:
      "Sign into a <strong>different Roblox account</strong> in each window and join a game.",
    how4Html:
      "Keep MultiRoblox open while you play. Use <strong>Stop All</strong> when finished.",
    psTitle: "Optional: install with auto-update",
    psLead:
      "Want Start Menu shortcuts and automatic updates? Run this in PowerShell:",
    reqTitle: "Requirements",
    req1: "Windows 10 or 11.",
    req2Html:
      'The <strong>Roblox player</strong> installed (from roblox.com). Keep MultiRoblox open while playing — it holds the lock that lets extra instances stay alive.',
    footerHtml:
      'MultiRoblox is an independent tool and is not affiliated with or endorsed by Roblox Corporation.<br />Source code on <a href="https://github.com/HyperlinksSpace/MultiRoblox">GitHub</a>.',
  },
  ru: {
    docTitle: "MultiRoblox — сколько угодно окон Roblox одновременно",
    docDesc:
      "MultiRoblox позволяет запускать неограниченное число окон Roblox на одном ПК. Одна кнопка — одно окно. Бесплатно, с открытым кодом, без прав администратора.",
    badge: "Бесплатно · Open source · Без прав админа",
    tagHtml:
      'Запускайте <strong>сколько угодно окон Roblox</strong> на одном ПК.',
    download: "↓ Скачать MultiRoblox.exe",
    hintHtml:
      'Один .exe — установка не нужна. Windows 10/11. Смотрите <a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">последний релиз</a>.',
    card1Title: "Одна кнопка",
    card1Body:
      'Нажимайте «Launch Instance» — по одному разу на каждое нужное окно Roblox. Каждое нажатие открывает ещё один клиент.',
    card2Title: "Живой статус",
    card2Body:
      "Видно каждый запущенный клиент и время его работы; остановить все можно одной кнопкой.",
    card3Title: "Безопасно и просто",
    card3Body:
      "Никаких патчей файлов и прав администратора. Приложение лишь удерживает блокировку одного экземпляра Roblox, пока оно открыто.",
    howTitle: "Как пользоваться",
    how1Html:
      "Скачайте <strong>MultiRoblox.exe</strong> и запустите его.",
    how2Html:
      "Нажимайте <strong>Launch Instance</strong> — по одному разу на каждое нужное окно Roblox.",
    how3Html:
      "Войдите в <strong>разные аккаунты Roblox</strong> в каждом окне и зайдите в игру.",
    how4Html:
      "Держите MultiRoblox открытым во время игры. Нажмите <strong>Stop All</strong>, когда закончите.",
    psTitle: "Дополнительно: установка с автообновлением",
    psLead:
      "Нужны ярлыки в меню «Пуск» и автообновление? Выполните в PowerShell:",
    reqTitle: "Требования",
    req1: "Windows 10 или 11.",
    req2Html:
      'Установленный <strong>Roblox Player</strong> (с roblox.com). Держите MultiRoblox открытым во время игры — он удерживает блокировку, благодаря которой дополнительные окна не закрываются.',
    footerHtml:
      'MultiRoblox — независимый инструмент и не связан с Roblox Corporation и не одобрен ею.<br />Исходный код на <a href="https://github.com/HyperlinksSpace/MultiRoblox">GitHub</a>.',
  },
  zh: {
    docTitle: "MultiRoblox — 想开多少个 Roblox 就开多少个",
    docDesc:
      "MultiRoblox 可在一台电脑上运行任意数量的 Roblox 实例。一键一个窗口。免费、开源、无需管理员权限。",
    badge: "免费 · 开源 · 无需管理员",
    tagHtml: "在一台电脑上运行<strong>任意数量的 Roblox 实例</strong>。",
    download: "↓ 下载 MultiRoblox.exe",
    hintHtml:
      '单个 .exe 文件——无需安装。支持 Windows 10/11。查看<a href="https://github.com/HyperlinksSpace/MultiRoblox/releases/latest">最新版本</a>。',
    card1Title: "一个按钮",
    card1Body:
      "想开几个 Roblox 窗口，就按几次 “Launch Instance”。每按一次就多开一个客户端。",
    card2Title: "实时状态",
    card2Body: "查看每个正在运行的实例及其运行时间，并用一个按钮全部停止。",
    card3Title: "安全简单",
    card3Body:
      "不修改游戏文件，不需要管理员权限。只是在窗口打开时占用 Roblox 的单实例锁。",
    howTitle: "使用方法",
    how1Html: "下载 <strong>MultiRoblox.exe</strong> 并运行。",
    how2Html:
      "按 <strong>Launch Instance</strong>——想开几个 Roblox 窗口就按几次。",
    how3Html:
      "在每个窗口登录<strong>不同的 Roblox 账号</strong>并进入游戏。",
    how4Html:
      "游玩时保持 MultiRoblox 打开。结束后使用 <strong>Stop All</strong>。",
    psTitle: "可选：带自动更新的安装方式",
    psLead: "想要开始菜单快捷方式和自动更新？在 PowerShell 中运行：",
    reqTitle: "系统要求",
    req1: "Windows 10 或 11。",
    req2Html:
      '已安装 <strong>Roblox 客户端</strong>（来自 roblox.com）。游玩时保持 MultiRoblox 打开——它占用的锁能让额外的实例保持运行。',
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
