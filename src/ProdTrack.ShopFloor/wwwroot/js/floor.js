// Shop-floor helpers: camera scanning (BarcodeDetector where supported) and feedback tones.
window.prodtrackFloor = (function () {
    let stream = null;
    let timer = null;

    function tone(ok) {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.frequency.value = ok ? 880 : 220;
            osc.type = ok ? 'sine' : 'square';
            gain.gain.value = 0.15;
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + (ok ? 0.12 : 0.4));
        } catch (e) { /* audio not available */ }
    }

    async function cameraSupported() {
        if (!('BarcodeDetector' in window) || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            return false;
        }
        try {
            const formats = await window.BarcodeDetector.getSupportedFormats();
            return formats.includes('qr_code');
        } catch (e) {
            return false;
        }
    }

    async function startCamera(video, dotnet) {
        stopCamera(video);
        const detector = new window.BarcodeDetector({ formats: ['qr_code', 'code_128', 'code_39'] });
        stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' }, audio: false });
        video.srcObject = stream;
        await video.play();
        timer = setInterval(async () => {
            try {
                const codes = await detector.detect(video);
                if (codes.length > 0) {
                    const value = codes[0].rawValue;
                    stopCamera(video);
                    await dotnet.invokeMethodAsync('OnCameraScanned', value);
                }
            } catch (e) { /* frame not ready */ }
        }, 250);
    }

    function stopCamera(video) {
        if (timer) { clearInterval(timer); timer = null; }
        if (stream) { stream.getTracks().forEach(t => t.stop()); stream = null; }
        if (video) { video.srcObject = null; }
    }

    function focus(element) { if (element) { element.focus(); } }

    return { tone, cameraSupported, startCamera, stopCamera, focus };
})();
