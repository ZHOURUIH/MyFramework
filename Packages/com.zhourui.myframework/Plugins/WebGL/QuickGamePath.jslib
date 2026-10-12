mergeInto(LibraryManager.library, {
	GetQuickGamePersistentPath: function () {
		var path = qg.env.USER_DATA_PATH;
		var size = lengthBytesUTF8(path) + 1;
		var buffer = _malloc(size);
		stringToUTF8(path, buffer, size);
		return buffer;
	},
	QuickGamePackageAccess: function (path) {
		try { qg.getFileSystemManager().accessSync(UTF8ToString(path)); return 1; }
		catch (e) { return 0; }
	},
	QuickGamePackageIsDirectory: function (path) {
		try {
			var result = qg.getFileSystemManager().statSync(UTF8ToString(path), false);
			var stat = result.stats || result;
			return stat.isDirectory() ? 1 : 0;
		} catch (e) { return 0; }
	},
	QuickGamePackageSize: function (path) {
		try {
			var result = qg.getFileSystemManager().statSync(UTF8ToString(path), false);
			return (result.stats || result).size;
		} catch (e) { return -1; }
	},
	QuickGamePackageDirectory: function (path) {
		var names = qg.getFileSystemManager().readdirSync(UTF8ToString(path)).join("\n");
		var buffer = _malloc(lengthBytesUTF8(names) + 1);
		stringToUTF8(names, buffer, lengthBytesUTF8(names) + 1);
		return buffer;
	},
	QuickGamePackageRead: function (path, length) {
		try {
			var data = qg.getFileSystemManager().readFileSync(UTF8ToString(path));
			var bytes = ArrayBuffer.isView(data) ? new Uint8Array(data.buffer, data.byteOffset, data.byteLength) : new Uint8Array(data);
			var pointer = _malloc(Math.max(1, bytes.length));
			HEAPU8.set(bytes, pointer);
			HEAP32[length >> 2] = bytes.length;
			return pointer;
		} catch (e) { HEAP32[length >> 2] = 0; return 0; }
	},
	QuickGamePackageFree: function (pointer) { _free(pointer); }
});
