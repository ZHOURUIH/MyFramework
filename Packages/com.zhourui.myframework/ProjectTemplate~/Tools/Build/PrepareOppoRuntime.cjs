'use strict';
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const marker = '// MyFramework quick-game OPPO runtime compatibility v1';
function replaceOnce(source, pattern, value, label) {
    const matches = source.match(new RegExp(pattern.source, 'g')) || [];
    if (matches.length !== 1) { throw new Error(`${label}: expected one SDK/Unity hook, found ${matches.length}; review the upgraded template`); }
    return source.replace(pattern, value);
}
function prepareRuntime(packagePath) {
    const sdkPath = path.join(packagePath, 'sdk-main.js');
    const frameworkPath = path.join(packagePath, 'Build', 'webgl.framework.js');
    let sdk = fs.readFileSync(sdkPath, 'utf8');
    let framework = fs.readFileSync(frameworkPath, 'utf8');
    // Fail before writing either file when competing native libraries leaked into the build.
    if (/GameGlobal\.USED_TEXTURE_COMPRESSION/.test(framework) || /function _QGWriteFileSync\(uri,/.test(framework)) {
        throw new Error('VIVO JS plugins leaked into the OPPO build. Rebuild Unity with QuickGamePluginScope; do not repackage the stale framework.');
    }
    if (sdk.includes(marker)) {
        if (/unityInstance\.SendMessage\(/.test(framework) || !framework.includes('oppoUnityFetchWithProgress(')) {
            throw new Error('Partially adapted OPPO export; regenerate with the SDK.');
        }
        return { alreadyPrepared: true };
    }
    if (/sendOppoUnityMessage|oppoUnityFetchWithProgress/.test(sdk + framework)) {
        throw new Error('Unrecognized existing runtime patch; regenerate with the SDK.');
    }
    const compat = fs.readFileSync(path.join(__dirname, 'OppoRuntimeCompat.js'), 'utf8');
    sdk = replaceOnce(sdk, /var unityInstance\s*=\s*null\s*;/,
        `var unityInstance = null;\n${marker}\n${compat}`, 'Unity bridge initialization');
    sdk = replaceOnce(sdk, /unityInstance\s*=\s*unity_instance\s*;/,
        'unityInstance = unity_instance;\n        flushOppoUnityMessages();', 'Unity bridge ready');
    const callbackCount = (framework.match(/unityInstance\.SendMessage\(/g) || []).length;
    if (callbackCount === 0) { throw new Error('No OPPO bridge callbacks found; review the Unity output.'); }
    framework = framework.replaceAll('unityInstance.SendMessage(', 'sendOppoUnityMessage(');
    framework = replaceOnce(framework, /var fetchImpl=Module\.fetchWithProgress;/,
        'var fetchImpl=function(url,options){return oppoUnityFetchWithProgress(url,options,Module.fetchWithProgress)};', 'UnityWebRequest fetch');
    framework = replaceOnce(framework, /var entries=response\.headers\.entries\(\);/,
        'var entries=typeof response.headers.entries==="function"?response.headers.entries():(function(){var pairs=[];response.headers.forEach(function(value,key){pairs.push([key,value])});return pairs[Symbol.iterator]()})();', 'UnityWebRequest headers');
    // Parse both complete scripts before changing the export. An upstream change must stop packing.
    new vm.Script(sdk, { filename: sdkPath });
    new vm.Script(framework, { filename: frameworkPath });
    fs.writeFileSync(sdkPath, sdk);
    fs.writeFileSync(frameworkPath, framework);
    return { callbackCount, alreadyPrepared: false };
}
function prepare(packagePath, options = {}) {
    // Resolve the optional framework tool before mutating the SDK export.
    const compression = options.compressPlayer ? require(path.join(
        options.frameworkTools || path.resolve(__dirname, '../../Packages/com.zhourui.myframework/Tools~/QuickGame'),
        'OppoPlayerCompression.cjs')) : null;
    const result = prepareRuntime(packagePath);
    if (compression) result.compression = compression.prepare(packagePath);
    return result;
}
module.exports = { prepare };
if (require.main === module) {
    try {
        if (!process.argv[2]) { throw new Error('Usage: node PrepareOppoRuntime.cjs <quickgame-directory>'); }
        const options = {};
        for (let i = 3; i < process.argv.length; ++i) {
            if (process.argv[i] === '--compress-player') options.compressPlayer = true;
            else if (process.argv[i] === '--framework-tools' && process.argv[i + 1]) options.frameworkTools = path.resolve(process.argv[++i]);
            else throw new Error('Unknown OPPO prepare option: ' + process.argv[i]);
        }
        console.log('OPPO runtime prepared:', JSON.stringify(prepare(path.resolve(process.argv[2]), options)));
    } catch (error) { console.error(error.stack); process.exitCode = 1; }
}
