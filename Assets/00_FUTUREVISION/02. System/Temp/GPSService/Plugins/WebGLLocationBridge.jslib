// ── 상태를 전역 라이브러리 심볼($FV)로 보관 ──
var LibraryFutureVisionSensors = {
  // $-prefix는 Emscripten이 전역 라이브러리 심볼로 취급(여러 함수에서 공유 가능)
  $FV__deps: [],
  $FV: {
    state: null,
    getState: function () {
      if (!this.state) {
        this.state = {
          watchId: null,
          lat: 0, lon: 0, acc: 0,
          heading: 0,
          hasGeo: 0, hasHeading: 0,
          orientationBound: 0,
          _onOri: null
        };
      }
      return this.state;
    }
  },

  FV_StartSensors: function () {
    var s = FV.getState();

    // Geolocation
    try {
      if (typeof navigator !== 'undefined' && navigator.geolocation) {
        if (s.watchId !== null) {
          try { navigator.geolocation.clearWatch(s.watchId); } catch(e){}
          s.watchId = null;
        }
        s.watchId = navigator.geolocation.watchPosition(function (pos) {
          var c = pos && pos.coords ? pos.coords : {};
          s.lat = typeof c.latitude  === 'number' ? c.latitude  : 0;
          s.lon = typeof c.longitude === 'number' ? c.longitude : 0;
          s.acc = typeof c.accuracy  === 'number' ? c.accuracy  : 0;
          s.hasGeo = 1;
        }, function (err) {
          console.log('geolocation error:', err);
        }, { enableHighAccuracy: true, maximumAge: 1000, timeout: 10000 });
      }
    } catch (e) { console.log('geolocation exception:', e); }

    // Orientation
    try {
      if (!s.orientationBound) {
        s._onOri = function (e) {
          var heading;
          if (typeof e.webkitCompassHeading === 'number') {
            heading = e.webkitCompassHeading; // iOS
          } else if (typeof e.alpha === 'number') {
            heading = 360 - e.alpha;
            if (heading < 0) heading += 360;
          }
          if (typeof heading === 'number' && !isNaN(heading)) {
            s.heading = heading;
            s.hasHeading = 1;
          }
        };
        try { window.addEventListener('deviceorientationabsolute', s._onOri, true); } catch(_) {}
        try { window.addEventListener('deviceorientation',        s._onOri, true); } catch(_) {}
        s.orientationBound = 1;
      }
    } catch (e) { console.log('orientation exception:', e); }
  },

  FV_RequestOrientationPermission: function () {
    try {
      if (typeof DeviceOrientationEvent !== 'undefined' &&
          typeof DeviceOrientationEvent.requestPermission === 'function') {
        DeviceOrientationEvent.requestPermission()
          .then(function (res) {
            if (res !== 'granted') console.log('orientation permission denied');
          })
          .catch(function (e) { console.log('orientation permission error:', e); });
      }
    } catch (e) { console.log('permission exception:', e); }
  },

  FV_StopSensors: function () {
    var s = FV.getState();
    try {
      if (s.watchId !== null && typeof navigator !== 'undefined' && navigator.geolocation) {
        try { navigator.geolocation.clearWatch(s.watchId); } catch(e){}
      }
      s.watchId = null;
      s.hasGeo = 0;
      s.hasHeading = 0;

      if (s.orientationBound) {
        try { window.removeEventListener('deviceorientationabsolute', s._onOri, true); } catch(_) {}
        try { window.removeEventListener('deviceorientation',        s._onOri, true); } catch(_) {}
        s._onOri = null;
        s.orientationBound = 0;
      }
    } catch (e) { console.log('stop exception:', e); }
  },

  FV_GetLat:     function () { return +FV.getState().lat;     },
  FV_GetLon:     function () { return +FV.getState().lon;     },
  FV_GetAcc:     function () { return +FV.getState().acc;     },
  FV_GetHeading: function () { return +FV.getState().heading; },
  FV_HasGeo:     function () { return +FV.getState().hasGeo;  },
  FV_HasHeading: function () { return +FV.getState().hasHeading; }
};

mergeInto(LibraryManager.library, LibraryFutureVisionSensors);
