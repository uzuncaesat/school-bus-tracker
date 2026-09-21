// Okyanus Servis — Leaflet harita köprüsü (Blazor JS interop)
window.okMap = {
    maps: {},
    markers: {},

    _setMarker: function (elementId, lat, lng) {
        const map = this.maps[elementId];
        if (this.markers[elementId]) {
            this.markers[elementId].setLatLng([lat, lng]);
        } else {
            this.markers[elementId] = L.marker([lat, lng]).addTo(map);
        }
    },

    // Tıklanabilir harita: konum seçtirir, .NET'e geri bildirir
    initPicker: function (elementId, dotnetRef, lat, lng) {
        const start = (lat && lng) ? [lat, lng] : [40.992, 29.128]; // Ataşehir civarı
        const map = L.map(elementId).setView(start, 13);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png', {
            maxZoom: 20, subdomains: 'abcd', attribution: '© OpenStreetMap © CARTO'
        }).addTo(map);
        this.maps[elementId] = map;

        if (lat && lng) this._setMarker(elementId, lat, lng);

        map.on('click', (e) => {
            this._setMarker(elementId, e.latlng.lat, e.latlng.lng);
            dotnetRef.invokeMethodAsync('OnPointPicked', e.latlng.lat, e.latlng.lng);
        });

        setTimeout(() => map.invalidateSize(), 200);
    },

    // Adres arama (Nominatim / OpenStreetMap). Bulursa haritayı taşır + pin koyar + .NET'e bildirir.
    searchAddress: async function (elementId, query, dotnetRef) {
        const map = this.maps[elementId];
        if (!map || !query) return false;
        const url = 'https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=tr&q='
            + encodeURIComponent(query);
        try {
            const res = await fetch(url, { headers: { 'Accept': 'application/json' } });
            const data = await res.json();
            if (!data || data.length === 0) return false;
            const lat = parseFloat(data[0].lat), lng = parseFloat(data[0].lon);
            map.setView([lat, lng], 16);
            this._setMarker(elementId, lat, lng);
            dotnetRef.invokeMethodAsync('OnPointPicked', lat, lng);
            return true;
        } catch {
            return false;
        }
    },

    // Salt-okunur harita
    initViewer: function (elementId, lat, lng) {
        const map = L.map(elementId).setView([lat, lng], 15);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png', {
            maxZoom: 20, subdomains: 'abcd', attribution: '© OpenStreetMap © CARTO'
        }).addTo(map);
        L.marker([lat, lng]).addTo(map);
        this.maps[elementId] = map;
        setTimeout(() => map.invalidateSize(), 200);
    },

    // ---- Canlı takip haritası (veli portalı) ----
    liveBuses: {},

    initLive: function (elementId, homeLat, homeLng) {
        if (this.maps[elementId]) { this.maps[elementId].remove(); delete this.maps[elementId]; }
        const center = (homeLat && homeLng) ? [homeLat, homeLng] : [40.992, 29.128];
        const map = L.map(elementId).setView(center, 14);
        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}.png', {
            maxZoom: 20, subdomains: 'abcd', attribution: '© OpenStreetMap © CARTO'
        }).addTo(map);
        this.maps[elementId] = map;
        this.liveBuses[elementId] = [];
        if (homeLat && homeLng) {
            const homeIcon = L.divIcon({
                html: '<div style="font-size:24px;line-height:1">🏠</div>',
                className: '', iconSize: [28, 28], iconAnchor: [14, 14]
            });
            L.marker([homeLat, homeLng], { icon: homeIcon }).addTo(map).bindTooltip('Ev');
        }
        setTimeout(() => map.invalidateSize(), 200);
    },

    // buses: [{ lat, lng, plate }]
    updateBuses: function (elementId, buses) {
        const map = this.maps[elementId];
        if (!map) return;
        (this.liveBuses[elementId] || []).forEach(m => map.removeLayer(m));
        this.liveBuses[elementId] = [];
        const pts = [];
        (buses || []).forEach(b => {
            const busIcon = L.divIcon({
                html: '<div style="font-size:26px;line-height:1">🚌</div>',
                className: '', iconSize: [30, 30], iconAnchor: [15, 15]
            });
            const mk = L.marker([b.lat, b.lng], { icon: busIcon }).addTo(map);
            if (b.plate) mk.bindTooltip(b.plate, { permanent: true, direction: 'top', offset: [0, -12] });
            this.liveBuses[elementId].push(mk);
            pts.push([b.lat, b.lng]);
        });
        if (pts.length === 1) {
            map.panTo(pts[0]);
        } else if (pts.length > 1) {
            map.fitBounds(pts, { padding: [40, 40], maxZoom: 15 });
        }
    },

    dispose: function (elementId) {
        const m = this.maps[elementId];
        if (m) { m.remove(); delete this.maps[elementId]; }
        delete this.markers[elementId];
        delete this.liveBuses[elementId];
    }
};
