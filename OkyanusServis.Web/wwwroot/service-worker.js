// Okyanus Servis — push bildirim service worker'ı.
// Sadece push bildirimi ve tıklama ile ilgilenir; Blazor'a karışmaz.

self.addEventListener('install', (e) => self.skipWaiting());
self.addEventListener('activate', (e) => e.waitUntil(self.clients.claim()));

// Sunucudan push geldiğinde bildirimi göster.
self.addEventListener('push', (event) => {
    let data = {};
    try { data = event.data ? event.data.json() : {}; } catch (_) { data = {}; }

    const title = data.title || 'Okyanus Servis';
    const options = {
        body: data.body || '',
        icon: data.icon || '/icon-192.png',
        badge: '/icon-192.png',
        data: { url: data.url || '/portalim' },
        vibrate: [200, 100, 200],
        tag: 'okyanus-servis'
    };
    event.waitUntil(self.registration.showNotification(title, options));
});

// Bildirime tıklayınca uygulamayı aç / öne getir.
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const url = (event.notification.data && event.notification.data.url) || '/portalim';
    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((list) => {
            for (const c of list) {
                if ('focus' in c) { c.navigate(url); return c.focus(); }
            }
            if (self.clients.openWindow) return self.clients.openWindow(url);
        })
    );
});
