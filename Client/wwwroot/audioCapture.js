window.captureAudio = function captureAudio(durationMs) {
    if (durationMs === undefined) durationMs = 5000;
    var sampleRate = 44100;
    // Browsers may ignore this hint and use the hardware rate (e.g. 48 000 Hz).
    // The server resamples to 44 100 Hz when the WAV header declares a different rate.

    return navigator.mediaDevices.getUserMedia({ audio: true })
        .then(function(stream) {
            var ctx = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: sampleRate
            });
            var source = ctx.createMediaStreamSource(stream);
            var samples = [];
            var node = ctx.createScriptProcessor(4096, 1, 1);

            node.onaudioprocess = function(e) {
                var channel = e.inputBuffer.getChannelData(0);
                for (var i = 0; i < channel.length; i++) {
                    samples.push(channel[i]);
                }
            };

            source.connect(node);
            node.connect(ctx.destination);

            return new Promise(function(resolve, reject) {
                setTimeout(function() {
                    source.disconnect();
                    node.disconnect();
                    stream.getTracks().forEach(function(t) { t.stop(); });
                    ctx.close();

                    var numSamples = samples.length;
                    var dataSize = numSamples * 2;
                    var buffer = new ArrayBuffer(44 + dataSize);
                    var view = new DataView(buffer);

                    function writeString(offset, str) {
                        for (var i = 0; i < str.length; i++) {
                            view.setUint8(offset + i, str.charCodeAt(i));
                        }
                    }

                    writeString(0, 'RIFF');
                    view.setUint32(4, 36 + dataSize, true);
                    writeString(8, 'WAVE');
                    writeString(12, 'fmt ');
                    view.setUint32(16, 16, true);
                    view.setUint16(20, 1, true);
                    view.setUint16(22, 1, true);
                    view.setUint32(24, sampleRate, true);
                    view.setUint32(28, sampleRate * 2, true);
                    view.setUint16(32, 2, true);
                    view.setUint16(34, 16, true);
                    writeString(36, 'data');
                    view.setUint32(40, dataSize, true);

                    var offset = 44;
                    for (var i = 0; i < numSamples; i++) {
                        var s = Math.max(-1, Math.min(1, samples[i]));
                        s = s < 0 ? s * 32768 : s * 32767;
                        view.setInt16(offset, s, true);
                        offset += 2;
                    }

                    var blob = new Blob([buffer], { type: 'audio/wav' });
                    var reader = new FileReader();
                    reader.onloadend = function() {
                        resolve(reader.result.split(',')[1]);
                    };
                    reader.readAsDataURL(blob);
                }, durationMs);
            });
        })
        .catch(function(err) {
            if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
                throw new Error('Microphone permission denied. Allow access and try again.');
            }
            throw new Error('Could not start recording: ' + err.message);
        });
};
