var pendingOppoUnityMessages = [];
function sendOppoUnityMessage(objectName, methodName, value) {
    if (!unityInstance) {
        pendingOppoUnityMessages.push([objectName, methodName, value]);
        return;
    }
    unityInstance.SendMessage(objectName, methodName, value);
}
function flushOppoUnityMessages() {
    var pending = pendingOppoUnityMessages;
    pendingOppoUnityMessages = [];
    console.log('[OPPO_BRIDGE_READY] queued callbacks', pending.length);
    pending.forEach(function(args) { sendOppoUnityMessage.apply(null, args); });
}
function oppoUnityFetchWithProgress(url, options, networkFetch) {
    var prefix = 'http://localhost/StreamingAssets/';
    var path = typeof url === 'string' && url.indexOf(prefix) === 0
        ? 'StreamingAssets/' + url.substring(prefix.length) : null;
    if (!path && typeof url === 'string' && url.indexOf(qg.env.USER_DATA_PATH + '/') === 0) {
        path = url;
    }
    if (!path) { return networkFetch(url, options); }
    return Promise.resolve().then(function() {
        if (options && options.signal && options.signal.aborted) {
            var error = new Error('Aborted.');
            error.name = 'AbortError';
            throw error;
        }
        var data = qg.getFileSystemManager().readFileSync(path);
        if (data && data.data !== undefined) { data = data.data; }
        var bytes = ArrayBuffer.isView(data)
            ? new Uint8Array(data.buffer, data.byteOffset, data.byteLength) : new Uint8Array(data);
        var pairs = [['content-length', String(bytes.length)], ['content-type', 'application/octet-stream']];
        var response = { status: 200, ok: true, url: url, parsedBody: bytes, headers: {
            get: function(name) {
                name = name.toLowerCase();
                for (var i = 0; i < pairs.length; ++i) { if (pairs[i][0] === name) { return pairs[i][1]; } }
                return null;
            },
            entries: function() { return pairs[Symbol.iterator](); },
            forEach: function(callback) { pairs.forEach(function(pair) { callback(pair[1], pair[0]); }); }
        }};
        if (options && options.onProgress) {
            options.onProgress({ response: response, lengthComputable: true, loaded: bytes.length,
                total: bytes.length, chunk: options.enableStreamingDownload ? bytes : undefined });
        }
        console.log('[OPPO_PACKAGE_READ]', path, bytes.length);
        return response;
    });
}
