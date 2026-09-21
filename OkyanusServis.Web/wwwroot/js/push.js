// Okyanus Servis — web push abonelik yardımcıları (Blazor'dan çağrılır).
window.okPush = (function () {

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const raw = atob(base64);
        const arr = new Uint8Array(raw.length);
        for (let i = 0; i < raw.length; i++) arr[i] = raw.charCodeAt(i);
        return arr;
    }

    async function ensureSW() {
        if (!('serviceWorker' in navigator)) throw new Error('unsupported');
        return await navigator.serviceWorker.register('/service-worker.js');
    }

    return {
        // Tarayıcı push destekliyor mu?
        supported: function () {
            return ('serviceWorker' in navigator) && ('PushManager' in window) && ('Notification' in window);
        },

        // İzin durumu: 'granted' | 'denied' | 'default' | 'unsupported'
        permission: function () {
            if (!('Notification' in window)) return 'unsupported';
            return Notification.permission;
        },

        // Bildirimleri aç: izin iste + abone ol + sunucuya kaydet.
        // Sonuç: { ok: bool, message: string }
        enable: async function (apiBase, token) {
            try {
                if (!this.supported()) return { ok: false, message: 'Bu tarayıcı push bildirimini desteklemiyor.' };

                const perm = await Notification.requestPermission();
                if (perm !== 'granted') return { ok: false, message: 'Bildirim izni verilmedi.' };

                const reg = await ensureSW();
                await navigator.serviceWorker.ready;

                // VAPID public anahtarını al
                const keyResp = await fetch(apiBase + 'api/push/vapid-public-key');
                const key = (await keyResp.text()).replace(/^"|"$/g, '');
                if (!key) return { ok: false, message: 'Sunucu anahtarı alınamadı.' };

                // Abone ol (varsa mevcut aboneliği kullan)
                let sub = await reg.pushManager.getSubscription();
                if (!sub) {
                    sub = await reg.pushManager.subscribe({
                        userVisibleOnly: true,
                        applicationServerKey: urlBase64ToUint8Array(key)
                    });
                }

                const raw = sub.toJSON();
                const body = JSON.stringify({
                    endpoint: sub.endpoint,
                    p256dh: raw.keys.p256dh,
                    auth: raw.keys.auth
                });

                const resp = await fetch(apiBase + 'api/push/subscribe', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
                    body
                });
                if (!resp.ok) return { ok: false, message: 'Abonelik kaydedilemedi (' + resp.status + ').' };

                return { ok: true, message: 'Bildirimler açıldı. Servis yaklaşınca haber vereceğiz.' };
            } catch (e) {
                return { ok: false, message: 'Hata: ' + (e && e.message ? e.message : e) };
            }
        },

        // Deneme bildirimi gönder (kendine).
        test: async function (apiBase, token) {
            try {
                const resp = await fetch(apiBase + 'api/push/test', {
                    method: 'POST',
                    headers: { 'Authorization': 'Bearer ' + token }
                });
                return resp.ok
                    ? { ok: true, message: 'Deneme bildirimi gönderildi.' }
                    : { ok: false, message: 'Gönderilemedi (' + resp.status + ').' };
            } catch (e) {
                return { ok: false, message: 'Hata: ' + (e && e.message ? e.message : e) };
            }
        }
    };
})();
