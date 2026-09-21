// Okyanus Servis — tarayıcıdan dosya indirme (base64 -> indir)
window.okFiles = {
    download: function (fileName, base64, contentType) {
        const link = document.createElement('a');
        link.href = 'data:' + (contentType || 'application/octet-stream') + ';base64,' + base64;
        link.download = fileName || 'belge';
        document.body.appendChild(link);
        link.click();
        link.remove();
    },

    // HTML içeriğini yeni sekmede açıp yazdırma penceresini getirir (tablo çıktısı için)
    printHtml: function (html) {
        const w = window.open('', '_blank');
        if (!w) { alert('Açılır pencere engellendi. Lütfen bu site için pop-up izni verin.'); return; }
        w.document.open();
        w.document.write(html);
        w.document.close();
        w.onload = function () { w.focus(); w.print(); };
    },

    // base64 -> yeni sekmede aç (sözleşmeyi indirmeden görüntülemek için)
    open: function (base64, contentType) {
        try {
            const bin = atob(base64);
            const bytes = new Uint8Array(bin.length);
            for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
            const blob = new Blob([bytes], { type: contentType || 'application/octet-stream' });
            const url = URL.createObjectURL(blob);
            window.open(url, '_blank');
            setTimeout(() => URL.revokeObjectURL(url), 60000);
        } catch (e) {
            console.log('open error', e);
        }
    }
};
