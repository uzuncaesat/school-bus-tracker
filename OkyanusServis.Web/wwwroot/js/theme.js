// Okyanus Servis — koyu/açık tema (demo ile aynı davranış)
window.okTheme = {
    apply: function (t) {
        if (t === 'dark' || t === 'light') document.documentElement.setAttribute('data-theme', t);
        else document.documentElement.removeAttribute('data-theme');
    },
    init: function () {
        try {
            const t = localStorage.getItem('ok_theme');
            if (t) this.apply(t);
        } catch (e) { }
    },
    toggle: function () {
        const cur = document.documentElement.getAttribute('data-theme');
        const isDark = cur ? cur === 'dark' : window.matchMedia('(prefers-color-scheme: dark)').matches;
        const next = isDark ? 'light' : 'dark';
        this.apply(next);
        try { localStorage.setItem('ok_theme', next); } catch (e) { }
        return next;
    }
};
// Sayfa boyanmadan temayı uygula (flaş olmasın)
window.okTheme.init();
