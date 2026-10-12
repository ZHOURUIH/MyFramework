mergeInto(LibraryManager.library, {
	$QuickGamePackagePath: function (path) {
		var value = UTF8ToString(path);
		return qg.getProvider && qg.getProvider() === 'vivo' && value.charAt(0) !== '/' ? '/' + value : value;
	},
	$QuickGamePackageStat__deps: ['$QuickGamePackagePath'],
	$QuickGamePackageStat: function (path) {
		var result = qg.getFileSystemManager().statSync(QuickGamePackagePath(path), false);
		// SDKs expose either a Stats object, QGStat[], or a wrapped QGStatResponse.
		for (var depth = 0; depth < 4; ++depth) {
			if (Array.isArray(result)) { result = result[0]; }
			else if (result && result.stats !== undefined) { result = result.stats; }
			else { break; }
		}
		if (!result || typeof result !== 'object') { throw new Error('Invalid package file stat'); }
		return result;
	},
	GetQuickGamePersistentPath: function () {
		var path = qg.env.USER_DATA_PATH;
		var size = lengthBytesUTF8(path) + 1;
		var buffer = _malloc(size);
		stringToUTF8(path, buffer, size);
		return buffer;
	},
	QuickGamePackageAccess__deps: ['$QuickGamePackagePath'],
	QuickGamePackageAccess: function (path) {
		try {
			var result = qg.getFileSystemManager().accessSync(QuickGamePackagePath(path));
			return result === false ? 0 : 1;
		}
		catch (e) { return 0; }
	},
	QuickGamePackageIsDirectory__deps: ['$QuickGamePackageStat'],
	QuickGamePackageIsDirectory: function (path) {
		try {
			var stat = QuickGamePackageStat(path);
			return (typeof stat.isDirectory === 'function' ? stat.isDirectory() : (stat.mode & 0xF000) === 0x4000) ? 1 : 0;
		} catch (e) { return 0; }
	},
	QuickGamePackageSize__deps: ['$QuickGamePackageStat'],
	QuickGamePackageSize: function (path) {
		try {
			var size = QuickGamePackageStat(path).size;
			return typeof size === 'number' && isFinite(size) && size >= 0 ? size : -1;
		} catch (e) { return -1; }
	},
	QuickGamePackageDirectory__deps: ['$QuickGamePackagePath'],
	QuickGamePackageDirectory: function (path) {
		var names = "";
		try {
			var result = qg.getFileSystemManager().readdirSync(QuickGamePackagePath(path));
			var files = Array.isArray(result) ? result : result && result.files;
			if (Array.isArray(files)) { names = files.join("\n"); }
		} catch (e) {
			// 与框架用户目录读取一致：不存在或不可读时返回空列表。
			// C# 会立即 Split 返回值，因此仍需分配有效的空字符串，不能返回 null。
		}
		var buffer = _malloc(lengthBytesUTF8(names) + 1);
		stringToUTF8(names, buffer, lengthBytesUTF8(names) + 1);
		return buffer;
	},
	QuickGamePackageRead__deps: ['$QuickGamePackagePath'],
	QuickGamePackageRead: function (path, length) {
		try {
			var result = qg.getFileSystemManager().readFileSync(QuickGamePackagePath(path));
			var data = result.data !== undefined ? result.data : result;
			var bytes = ArrayBuffer.isView(data) ? new Uint8Array(data.buffer, data.byteOffset, data.byteLength) : new Uint8Array(data);
			var pointer = _malloc(Math.max(1, bytes.length));
			HEAPU8.set(bytes, pointer);
			HEAP32[length >> 2] = bytes.length;
			return pointer;
		} catch (e) { HEAP32[length >> 2] = 0; return 0; }
	},
	QuickGamePackageFree: function (pointer) { _free(pointer); }
});
