// LawService — ponte entre o C# (Blazor) e o navegador:
// armazenamento (IndexedDB), editor de texto rico (iframe), impressão/PDF, downloads e atalhos.
window.LS = (() => {
  'use strict';
  let app = null;            // referência .NET do aplicativo
  let dirty = false;
  const editors = new Map(); // id -> { frame, ref, ready, queue: [] }
  const views = new Map();   // id -> { frame, ref }

  // ───────────────────────── IndexedDB
  let dbp = null;
  function db() {
    if (!dbp) dbp = new Promise((res, rej) => {
      const r = indexedDB.open('lawservice', 1);
      r.onupgradeneeded = () => {
        const d = r.result;
        if (!d.objectStoreNames.contains('kv')) d.createObjectStore('kv');
        if (!d.objectStoreNames.contains('imgs')) d.createObjectStore('imgs');
      };
      r.onsuccess = () => res(r.result);
      r.onerror = () => rej(r.error);
    });
    return dbp;
  }
  async function req(store, mode, fn) {
    const d = await db();
    return new Promise((res, rej) => {
      const t = d.transaction(store, mode);
      const r = fn(t.objectStore(store));
      t.oncomplete = () => res(r && 'result' in r ? r.result : undefined);
      t.onerror = () => rej(t.error);
      t.onabort = () => rej(t.error);
    });
  }
  const kvGet = k => req('kv', 'readonly', s => s.get(k));
  const kvSet = (k, v) => req('kv', 'readwrite', s => s.put(v, k));
  const kvDel = k => req('kv', 'readwrite', s => s.delete(k));

  // ───────────────────────── imagens (guardadas pelo nome do arquivo, como na pasta "imagens")
  const urls = new Map(), names = new Map();
  const MIME = { png: 'image/png', jpg: 'image/jpeg', jpeg: 'image/jpeg', gif: 'image/gif', bmp: 'image/bmp', webp: 'image/webp', svg: 'image/svg+xml' };
  const mimeOf = n => MIME[(n.split('.').pop() || '').toLowerCase()] || 'image/png';
  function uid() { return Math.random().toString(16).slice(2, 8) + Date.now().toString(16).slice(-6); }
  async function imgPut(name, bytes) {
    const blob = bytes instanceof Blob ? bytes : new Blob([bytes], { type: mimeOf(name) });
    await req('imgs', 'readwrite', s => s.put(blob, name));
    if (urls.has(name)) { names.delete(urls.get(name)); URL.revokeObjectURL(urls.get(name)); urls.delete(name); }
    return name;
  }
  async function imgBlob(name) { return await req('imgs', 'readonly', s => s.get(name)); }
  async function imgUrl(name) {
    if (urls.has(name)) return urls.get(name);
    const b = await imgBlob(name);
    if (!b) return null;
    const u = URL.createObjectURL(b);
    urls.set(name, u); names.set(u, name);
    return u;
  }
  const baseName = s => decodeURIComponent(s.split(/[\\/]/).pop().split('?')[0]);
  async function resolveImgs(html) {
    if (!html || html.indexOf('<img') < 0) return html || '';
    const re = /(<img\b[^>]*?\bsrc\s*=\s*")([^"]*)(")/gi;
    const found = new Set();
    let m;
    while ((m = re.exec(html))) if (!/^(blob:|data:|https?:)/i.test(m[2])) found.add(m[2]);
    const map = new Map();
    for (const src of found) { const u = await imgUrl(baseName(src)); if (u) map.set(src, u); }
    return html.replace(re, (all, a, src, c) => map.has(src) ? a + map.get(src) + c : all);
  }
  function unresolve(html) {
    if (!html || html.indexOf('blob:') < 0) return html || '';
    return html.replace(/(<img\b[^>]*?\bsrc\s*=\s*")(blob:[^"]*)(")/gi, (all, a, u, c) => names.has(u) ? a + names.get(u) + c : all);
  }
  async function saveDataUrl(dataUrl, fname) {
    const m = /^data:image\/([a-zA-Z0-9.+-]+);base64,(.*)$/s.exec(dataUrl || '');
    if (!m) return null;
    let ext = { jpeg: '.jpg', jpg: '.jpg', gif: '.gif', bmp: '.bmp', webp: '.webp' }[m[1].toLowerCase()] || '.png';
    const fe = (fname || '').toLowerCase().match(/\.(png|jpe?g|gif|bmp|webp)$/);
    if (fe) ext = fe[0].replace('.jpeg', '.jpg');
    const bin = atob(m[2]);
    const bytes = new Uint8Array(bin.length);
    for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
    const name = uid() + ext;
    await imgPut(name, bytes);
    return name;
  }

  // ───────────────────────── editor de texto rico (editor.html dentro de um iframe)
  function edOf(src) { for (const [id, e] of editors) if (e.frame.contentWindow === src) return [id, e]; return [null, null]; }
  function viewOf(src) { for (const [id, v] of views) if (v.frame.contentWindow === src) return [id, v]; return [null, null]; }

  async function copyRich(html, text) {
    try {
      if (window.ClipboardItem) {
        await navigator.clipboard.write([new ClipboardItem({
          'text/html': new Blob([html], { type: 'text/html' }), 'text/plain': new Blob([text], { type: 'text/plain' }),
        })]);
        return;
      }
    } catch (e) { }
    try { await navigator.clipboard.writeText(text); } catch (e) { }
  }

  window.addEventListener('message', async ev => {
    const o = ev.data;
    if (!o || typeof o !== 'object' || !o.t) return;
    const [vid, v] = viewOf(ev.source);
    if (v) {
      if (o.t === 'link') v.ref.invokeMethodAsync('OnLink', o.href);
      else if (o.t === 'key') key(o.k);
      else if (o.t === 'click') key('click');
      return;
    }
    const [id, e] = edOf(ev.source);
    if (!e) return;
    switch (o.t) {
      case 'ready':
        e.ready = true;
        for (const f of e.queue.splice(0)) await f();
        e.ref.invokeMethodAsync('OnReady');
        break;
      case 'change':
        e.ref.invokeMethodAsync('OnChange', o.g, unresolve(o.html), o.text);
        break;
      case 'state':
        e.ref.invokeMethodAsync('OnState', JSON.stringify(o));
        break;
      case 'ctx': {
        const r = e.frame.getBoundingClientRect();
        o.x = (o.x || 0) + r.left; o.y = (o.y || 0) + r.top;
        if (o.src) o.src = names.get(o.src) || baseName(o.src);
        e.ref.invokeMethodAsync('OnCtx', JSON.stringify(o));
        break;
      }
      case 'pasteImage': {
        const name = await saveDataUrl(o.data, o.name);
        if (name) await edCmd(id, 'image', [name, null]);
        break;
      }
      case 'copy':
        await copyRich(o.html, o.text);
        break;
      case 'key':
        key(o.k);
        break;
      case 'click':
        key('click');
        break;
    }
  });

  function attachEditor(id, frame, ref) {
    const old = editors.get(id);
    editors.set(id, { frame, ref, ready: !!(old && old.frame === frame && old.ready), queue: [] });
    try { if (frame.contentWindow && frame.contentWindow.ed) editors.get(id).ready = true; } catch (e) { }
  }
  function detach(id) { editors.delete(id); views.delete(id); }
  function whenReady(id, f) {
    const e = editors.get(id);
    if (!e) return Promise.resolve();
    if (e.ready) return f();
    return new Promise(res => e.queue.push(async () => { await f(); res(); }));
  }
  async function edLoad(id, gen, html, opts) {
    const h = await resolveImgs(html);
    return whenReady(id, () => { const e = editors.get(id); if (e) e.frame.contentWindow.ed.load(gen, h, opts); });
  }
  async function edCmd(id, name, arg) {
    if (name === 'image' && Array.isArray(arg)) { const u = await imgUrl(arg[0]); if (u) arg = [u, arg[1]]; }
    if (name === 'insertHtml' && typeof arg === 'string') arg = await resolveImgs(arg);
    return whenReady(id, () => {
      const e = editors.get(id);
      if (!e) return;
      try { e.frame.focus(); } catch (x) { }
      e.frame.contentWindow.ed.cmd(name, arg);
    });
  }
  async function edPaste(id, plain) {
    try {
      if (!plain && navigator.clipboard.read) {
        const items = await navigator.clipboard.read();
        for (const it of items) {
          const img = it.types.find(t => t.startsWith('image/'));
          if (img) {
            const blob = await it.getType(img);
            const name = uid() + '.' + (img.split('/')[1] || 'png').replace('jpeg', 'jpg');
            await imgPut(name, blob);
            return edCmd(id, 'image', [name, null]);
          }
          if (it.types.includes('text/html')) {
            const h = await (await it.getType('text/html')).text();
            const frag = (/<!--StartFragment-->([\s\S]*)<!--EndFragment-->/.exec(h) || [, h])[1];
            if (frag.replace(/<[^>]+>/g, '').trim()) return edCmd(id, 'insertHtml', frag);
          }
        }
      }
      const t = await navigator.clipboard.readText();
      if (t) return edCmd(id, 'insertText', t);
    } catch (e) {
      if (app) app.invokeMethodAsync('Toast', 'O navegador bloqueou a colagem pelo menu — use Ctrl+V');
    }
  }
  async function edInsertFiles(id, input) {
    for (const f of input.files || []) {
      if (!/^image\//.test(f.type)) continue;
      const ext = (f.name.match(/\.(png|jpe?g|gif|bmp|webp)$/i) || ['.png'])[0].toLowerCase().replace('.jpeg', '.jpg');
      const name = uid() + ext;
      await imgPut(name, f);
      await edCmd(id, 'image', [name, null]);
    }
    input.value = '';
  }
  function edFocus(id) { const e = editors.get(id); if (e) { try { e.frame.focus(); e.frame.contentWindow.ed.cmd('focus'); } catch (x) { } } }

  // ───────────────────────── visualizador de HTML (somente leitura)
  const VIEW_SCRIPT = "<script>document.addEventListener('click',e=>{const a=e.target.closest&&e.target.closest('a[href]');"
    + "if(a){e.preventDefault();parent.postMessage({t:'link',href:a.getAttribute('href')},'*');}},true);"
    + "document.addEventListener('mousedown',()=>parent.postMessage({t:'click'},'*'));"
    + "document.addEventListener('keydown',e=>{const k=e.key.toLowerCase();if((e.ctrlKey||e.metaKey)&&!e.altKey&&(k==='s'||k==='f'||k==='k')){e.preventDefault();parent.postMessage({t:'key',k:'ctrl+'+k},'*');}"
    + "else if(e.altKey&&!e.ctrlKey&&/^[1-7n]$/.test(k)){e.preventDefault();parent.postMessage({t:'key',k:'alt+'+k},'*');}"
    + "else if(k==='escape'){parent.postMessage({t:'key',k:'escape'},'*');}},true);<\/script>";
  async function viewSet(id, frame, ref, html) {
    views.set(id, { frame, ref });
    const h = await resolveImgs(html);
    frame.srcdoc = h.indexOf('</body>') >= 0 ? h.replace('</body>', VIEW_SCRIPT + '</body>') : h + VIEW_SCRIPT;
  }

  // ───────────────────────── impressão (PDF) e arquivos
  async function printHtml(html, ml, mt, mr, mb) {
    const f = document.createElement('iframe');
    f.style.cssText = 'position:fixed; right:0; bottom:0; width:0; height:0; border:0; visibility:hidden;';
    document.body.appendChild(f);
    const page = `<style>@page { size: A4; margin: ${mt}mm ${mr}mm ${mb}mm ${ml}mm; } html,body{background:#fff;}
      body{-webkit-print-color-adjust:exact; print-color-adjust:exact;} img{max-width:100%;}</style>`;
    let h = await resolveImgs(html);
    h = h.indexOf('</head>') >= 0 ? h.replace('</head>', page + '</head>') : page + h;
    await new Promise(res => { f.onload = res; f.srcdoc = h; });
    await new Promise(res => setTimeout(res, 300));
    const imgs = [...f.contentDocument.images].filter(i => !i.complete);
    await Promise.all(imgs.map(i => new Promise(r => { i.onload = i.onerror = r; })));
    f.contentWindow.focus();
    f.contentWindow.print();
    setTimeout(() => f.remove(), 60000);
  }
  function download(name, mime, data) {
    const blob = data instanceof Blob ? data : new Blob([data], { type: mime });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = name;
    document.body.appendChild(a);
    a.click();
    setTimeout(() => { URL.revokeObjectURL(a.href); a.remove(); }, 5000);
  }
  async function copyText(t) {
    try { await navigator.clipboard.writeText(t); return true; }
    catch (e) {
      const ta = document.createElement('textarea');
      ta.value = t; document.body.appendChild(ta); ta.select();
      let ok = false; try { ok = document.execCommand('copy'); } catch (x) { }
      ta.remove(); return ok;
    }
  }
  async function imgNames() { return (await req('imgs', 'readonly', s => s.getAllKeys())) || []; }
  async function imgBytes(name) {
    const b = await imgBlob(name);
    return b ? new Uint8Array(await b.arrayBuffer()) : null;
  }

  // ───────────────────────── atalhos de teclado
  function key(k) { if (app) app.invokeMethodAsync('Key', k); }
  document.addEventListener('keydown', e => {
    const k = (e.key || '').toLowerCase();
    if ((e.ctrlKey || e.metaKey) && !e.altKey && !e.shiftKey && (k === 's' || k === 'f' || k === 'k')) { e.preventDefault(); key('ctrl+' + k); }
    else if (e.altKey && !e.ctrlKey && !e.metaKey && /^[1-7n]$/.test(k)) { e.preventDefault(); key('alt+' + k); }
    else if (k === 'escape') key('escape');
  }, true);
  // o Firefox só inicia o arrastar se houver dados no dataTransfer
  document.addEventListener('dragstart', e => {
    const t = e.target && e.target.closest ? e.target.closest('[draggable="true"]') : null;
    if (t && e.dataTransfer) { try { e.dataTransfer.setData('text/plain', 'lawservice'); e.dataTransfer.effectAllowed = 'move'; } catch (x) { } }
  });
  document.addEventListener('visibilitychange', () => { if (document.visibilityState === 'hidden' && app) app.invokeMethodAsync('Flush'); });
  window.addEventListener('beforeunload', e => {
    if (app) app.invokeMethodAsync('Flush');
    if (dirty) { e.preventDefault(); e.returnValue = ''; }
  });

  return {
    init(ref) {
      app = ref;
      try { if (navigator.storage && navigator.storage.persist) navigator.storage.persist(); } catch (e) { }
    },
    setDirty(v) { dirty = !!v; },
    kvGet, kvSet, kvDel, imgPut, imgNames, imgBytes, imgUrl, resolveImgs,
    attachEditor, detach, edLoad, edCmd, edFocus, edPaste, edInsertFiles, viewSet,
    printHtml, download, copyText,
    openUrl(u) { window.open(u, '_blank', 'noopener'); },
    focus(el, select) { if (!el) return; el.focus(); if (select && el.select) el.select(); },
    scrollIntoView(el) { if (el) el.scrollIntoView({ block: 'nearest' }); },
    scrollTo(el, end) { if (el) el.scrollTop = end ? el.scrollHeight : 0; },
    rect(el) { if (!el) return null; const r = el.getBoundingClientRect(); return { x: r.left, y: r.top, w: r.width, h: r.height }; },
    viewport() { return { w: window.innerWidth, h: window.innerHeight }; },
    click(el) { if (el) el.click(); },
    reload() { location.reload(); },
    async openImage(n) { const u = await imgUrl(n); if (u) window.open(u, '_blank'); },
    fitMenu(el) {
      if (!el) return;
      const r = el.getBoundingClientRect(), W = window.innerWidth, H = window.innerHeight;
      let x = r.left, y = r.top;
      if (r.right > W - 6) x = Math.max(6, W - r.width - 6);
      if (r.bottom > H - 6) y = Math.max(6, H - r.height - 6);
      el.style.left = x + 'px'; el.style.top = y + 'px'; el.style.visibility = 'visible';
    },
  };
})();
