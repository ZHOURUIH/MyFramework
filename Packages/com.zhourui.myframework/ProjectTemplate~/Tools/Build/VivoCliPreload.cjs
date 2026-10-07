'use strict';
// Node compression workers inherit --require but do not execute the CLI reporter.
if (!require('worker_threads').isMainThread)
{
    return;
}
const Module = require('module');
const path = require('path');
const fs = require('fs');
const crypto = require('crypto');
const expectedVersion = '1.27.45';
const expectedReporterHash = '4d10c96266a2ee99642c87b89ac2371dd459116c7347f8d9a54a2d0845f0c2cf';
const expectedCallerHash = 'dc2ea65ce13cb87ac3e4bfe2d7499616e3bc0d786719270098a719a65b3b15cf';
if (!process.argv[1] || process.env.BABEL_ENV === 'debug')
{
    throw new Error('vivo reporter guard requires the official compiled CLI entry.');
}
const cliRequire = Module.createRequire(path.resolve(process.argv[1]));
const packagePath = cliRequire.resolve('@vivo-minigame/cli-packager/package.json');
const packageInfo = JSON.parse(fs.readFileSync(packagePath, 'utf8'));
if (packageInfo.name !== '@vivo-minigame/cli-packager' || packageInfo.version !== expectedVersion)
{
    throw new Error('Unsupported vivo CLI packager version. Review its reporter before building.');
}
const packageRoot = path.dirname(packagePath);
const reporterPath = fs.realpathSync(path.join(packageRoot, 'dist/lib/reporter.js'));
const callerPath = path.join(packageRoot, 'dist/index.js');
const reporterHash = crypto.createHash('sha256').update(fs.readFileSync(reporterPath)).digest('hex');
const callerHash = crypto.createHash('sha256').update(fs.readFileSync(callerPath)).digest('hex');
if (reporterHash !== expectedReporterHash || callerHash !== expectedCallerHash)
{
    throw new Error('vivo CLI reporter or caller changed. Review it before building.');
}
if (require.cache[reporterPath])
{
    throw new Error('vivo CLI reporter was loaded before its guard.');
}
const normalize = process.platform === 'win32' ? value => value.toLowerCase() : value => value;
const reporterKey = normalize(reporterPath);
const replacement = { default: async function reporter()
{
    process.stdout.write('vivo CLI monitor reporter suppressed.\n');
} };
Object.defineProperty(replacement, '__esModule', { value: true });
Object.freeze(replacement);
const originalLoad = Module._load;
let intercepted = false;
Module._load = function load(request, parent, isMain)
{
    const resolved = Module._resolveFilename(request, parent, isMain);
    if (typeof resolved === 'string' && normalize(resolved) === reporterKey)
    {
        intercepted = true;
        return replacement;
    }
    return originalLoad.apply(this, arguments);
};
process.on('exit', function checkReporterGuard()
{
    if (!intercepted)
    {
        process.stderr.write('vivo reporter guard was not used. Treat this build as failed.\n');
        process.exitCode = 1;
    }
});
