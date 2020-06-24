var AVProVideoWebGL = {
    /*isNumber: function (item) {
        return typeof(item) === "number" && !isNaN(item);
    },
    assert: function (equality, message) {
        if (!equality)
            console.log(message);
    },*/
    count: 0,
    videos: [],
    hasVideos__deps: ["videos"],
    hasVideos: function (videoIndex) {
        if (videoIndex) {
            if (videoIndex == -1) {
                return false;
            }

            if (_videos) {
                if (_videos[videoIndex]) {
                    return true;
                }
            }
        } else {
            if (_videos) {
                if (_videos.length > 0) {
                    return true;
                }
            }
        }

        return false;
    },
    AVPPlayerInsertVideoElement__deps: ["count", "videos"],
    AVPPlayerInsertVideoElement: function (path, idValues, externalLibrary) {
        if (!path) {
            return false;
        }

        // NOTE: When loading from the indexedDB (Application.persistantDataPath), 
        //       URL.createObjectURL() must be used get a valid URL.  See:
        //       http://www.misfitgeek.com/html5-off-line-storing-and-retrieving-videos-with-indexeddb/
        path = Pointer_stringify(path);
        _count++;

        var vid = document.createElement("video");
        var useNativeSrcPath = true;

        if (externalLibrary == 1)
        {
            useNativeSrcPath = false;
            var player = dashjs.MediaPlayer().create();
            player.initialize(vid, path, true);
        }
        else if (externalLibrary == 2)
        {
            useNativeSrcPath = false;
            var hls = new Hls();
            hls.loadSource(path);
            hls.attachMedia(vid);
            hls.on(Hls.Events.MANIFEST_PARSED,function() 
            {
                //video.play();
            });
        }
        else if (externalLibrary == 3)
        {
            //useNativeSrcPath = false;
        }

		// Some sources say that this is the proper way to catch errors...
		/*vid.addEventListener('error', function(event) {
			console.log("Error: " + event);
		}, true);*/

        var hasSetCanPlay = false;
        var playerIndex;
        var id = _count;
        
        var vidData = {
            id: id,
            video: vid,
            ready: false,
            hasMetadata: false,
            isStalled: false,
            buffering: false,
            lastErrorCode: 0
        };

        _videos.push(vidData);
        playerIndex = (_videos.length > 0) ? _videos.length - 1 : 0;

        vid.oncanplay = function () {
            if (!hasSetCanPlay) {
                hasSetCanPlay = true;
                vidData.ready = true;
            }
            //console.log("ONCANPLAY");
        };

        vid.onloadedmetadata = function () {
            vidData.hasMetadata = true;
        };

        vid.oncanplaythrough = function () {
            vidData.buffering = false;
            //console.log("CANPLAYTHROUGH");
        };

        vid.onplaying = function () {
            vidData.buffering = false;
            vidData.isStalled = false;
            //console.log("PLAYING");
        };

        vid.onwaiting = function () {
            vidData.buffering = true;
            //console.log("WAITING");
        };

        vid.onstalled = function () {
            vidData.isStalled = true;
            //console.log("STALLED");
        }

        /*vid.onpause = function () {
        };*/

        vid.onended = function () {
            vidData.buffering = false;
            vidData.isStalled = false;
            //console.log("ENDED");
        };

        vid.ontimeupdate = function() {
            vidData.buffering = false;
            vidData.isStalled = false;
            //console.log("vid current time: ", this.currentTime);
        };

        vid.onerror = function (texture) {
            var err = "unknown error";

            switch (vid.error.code) {
                case 1:
                    err = "video loading aborted";
                    break;
                case 2:
                    err = "network loading error";
                    break;
                case 3:
                    err = "video decoding failed / corrupted data or unsupported codec";
                    break;
                case 4:
                    err = "video not supported";
                    break;
            }

            vidData.lastErrorCode = vid.error.code;

            console.log("Error: " + err + " (errorcode=" + vid.error.code + ")", "color:red;");
        };

        vid.crossOrigin = "anonymous";
        vid.autoplay = false;
        if (useNativeSrcPath)
        {
            vid.src = path;
        }

		HEAP32[(idValues>>2)] = playerIndex;
		HEAP32[(idValues>>2)+1] = id;

		return true;
    },
    AVPPlayerGetLastError__deps: ["videos", "hasVideos"],
    AVPPlayerGetLastError: function(playerIndex){
        if(!_hasVideos(playerIndex))
        {
            return 0;
        }

        var ret = _videos[playerIndex].lastErrorCode
        _videos[playerIndex].lastErrorCode = 0;

        return ret;
    },
    AVPPlayerFetchVideoTexture__deps: ["videos", "hasVideos"],
    AVPPlayerFetchVideoTexture: function (playerIndex, texture, init) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
        if (!init)
        {
        	GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _videos[playerIndex].video);
        }
        else
		{
        	GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _videos[playerIndex].video);
        }
		
        //NB: This line causes the texture to not show unless something else is rendered (not sure why)
		//GLctx.bindTexture(GLctx.TEXTURE_2D, null);
	},
    AVPPlayerUpdatePlayerIndex__deps: ["videos", "hasVideos"],
    AVPPlayerUpdatePlayerIndex: function (id) {
        var result = -1;

        if (!_hasVideos()) {
            return result;
        }

        _videos.forEach(function (currentVid, index)
        {
        	if (currentVid != null && currentVid.id == id)
        	{
                result = index;
            }
        });

        return result;
    },
    AVPPlayerWidth__deps: ["videos", "hasVideos"],
    AVPPlayerWidth: function (playerIndex) {
    	if (!_hasVideos(playerIndex)) {
    		return 0;
    	}

    	return _videos[playerIndex].video.videoWidth;
    },
    AVPPlayerHeight__deps: ["videos", "hasVideos"],
    AVPPlayerHeight: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0;
        }

        return _videos[playerIndex].video.videoHeight;
    },
    AVPPlayerReady__deps: ["videos", "hasVideos"],
    AVPPlayerReady: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        if (_videos) {
            if (_videos.length > 0) {
                if (_videos[playerIndex]) {
                    return _videos[playerIndex].ready;
                }
            }
        } else {
            return false;
        }

        //return _videos[playerIndex].video.readyState >= _videos[playerIndex].video.HAVE_CURRENT_DATA;
    },
    AVPPlayerClose__deps: ["videos", "hasVideos"],
    AVPPlayerClose: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        var vid = _videos[playerIndex].video;

        vid.src = "";
        vid.load();

        _videos[playerIndex].video = null;
        _videos[playerIndex] = null;

        var allEmpty = true;
        for (i = 0; i < _videos.length; i++) {
        	if (_videos[i] != null) {
        		allEmpty = false;
        		break;
        	}
        }
        if (allEmpty)
        {
        	_videos = [];
        }
        //_videos = _videos.splice(playerIndex, 1);

		// Remove from DOM
        //vid.parentNode.removeChild(vid);
    },
    AVPPlayerSetLooping__deps: ["videos", "hasVideos"],
    AVPPlayerSetLooping: function (playerIndex, loop) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        _videos[playerIndex].video.loop = loop;
    },
    AVPPlayerIsLooping__deps: ["videos", "hasVideos"],
    AVPPlayerIsLooping: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.loop;
    },
    AVPPlayerHasMetadata__deps: ["videos", "hasVideos"],
    AVPPlayerHasMetadata: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return (_videos[playerIndex].video.readyState >= 1);
        //return _videos[playerIndex].video.hasMetadata;
    },
    AVPPlayerIsPlaying__deps: ["videos", "hasVideos"],
    AVPPlayerIsPlaying: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        var video = _videos[playerIndex].video;

        return !(video.paused || video.ended || video.seeking || video.readyState < video.HAVE_FUTURE_DATA);
    },
    AVPPlayerIsSeeking__deps: ["videos", "hasVideos"],
    AVPPlayerIsSeeking: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.seeking;
    },
    AVPPlayerIsPaused__deps: ["videos", "hasVideos"],
    AVPPlayerIsPaused: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.paused;
    },
    AVPPlayerIsFinished__deps: ["videos", "hasVideos"],
    AVPPlayerIsFinished: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.ended;
    },
    AVPPlayerIsBuffering__deps: ["videos", "hasVideos"],
    AVPPlayerIsBuffering: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].buffering;
    },
    AVPPlayerIsPlaybackStalled__deps: ["videos", "hasVideos"],
    AVPPlayerIsPlaybackStalled: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].isStalled;
    },
    AVPPlayerPlay__deps: ["videos", "hasVideos", "AVPPlayerIsMuted", "AVPPlayerSetMuted", "AVPPlayerPlay"],
    AVPPlayerPlay: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

		// https://webkit.org/blog/7734/auto-play-policy-changes-for-macos/
		// https://developers.google.com/web/updates/2017/06/play-request-was-interrupted
        var playPromise = _videos[playerIndex].video.play();
		if (playPromise !== undefined) 
		{
			playPromise.then(function() {
			  // Automatic playback started!
			  // Show playing UI.
			 })
			 .catch(function(error) {
			  // Auto-play was prevented
			  // Show paused UI.
			  if (!_AVPPlayerIsMuted(playerIndex))
			  {
			  	console.error("[AVProVideo] Video refused to start playback - check your browser permission settings as videos that contain audio can be blocked by default.  Muting video and attempting playback again.");
			  	_AVPPlayerSetMuted(playerIndex, true);
			  	_AVPPlayerPlay(playerIndex);
			  }
			});
		}
		return true;
    },
    AVPPlayerPause__deps: ["videos", "hasVideos"],
    AVPPlayerPause: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        _videos[playerIndex].video.pause();
    },
    AVPPlayerSeekToTime__deps: ["videos", "hasVideos"],
    AVPPlayerSeekToTime: function (playerIndex, timeSec, fast) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        var vid = _videos[playerIndex].video;

        if (vid.seekable && vid.seekable.length > 0) {
        	for (i = 0; i < vid.seekable.length; i++) {
            	if (timeSec >= vid.seekable.start(i) && timeSec <= vid.seekable.end(i)) {
            		vid.currentTime = timeSec;
                    return;
                }
            }

            if (timeSec == 0) {
                _videos[playerIndex].video.load();
            }
        }
    },
    AVPPlayerGetCurrentTime__deps: ["videos", "hasVideos"],
    AVPPlayerGetCurrentTime: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        return _videos[playerIndex].video.currentTime;
    },
    AVPPlayerGetPlaybackRate__deps: ["videos", "hasVideos"],
    AVPPlayerGetPlaybackRate: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        return _videos[playerIndex].video.playbackRate;
    },
    AVPPlayerSetPlaybackRate__deps: ["videos", "hasVideos"],
    AVPPlayerSetPlaybackRate: function (playerIndex, rate) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        _videos[playerIndex].video.playbackRate = rate;
    },
    AVPPlayerSetMuted__deps: ["videos", "hasVideos"],
    AVPPlayerSetMuted: function (playerIndex, mute) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        _videos[playerIndex].video.muted = mute;
    },
    AVPPlayerGetDuration__deps: ["videos", "hasVideos"],
    AVPPlayerGetDuration: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        return _videos[playerIndex].video.duration;
    },
    AVPPlayerIsMuted__deps: ["videos", "hasVideos"],
    AVPPlayerIsMuted: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.muted;
    },
    AVPPlayerSetVolume__deps: ["videos", "hasVideos"],
    AVPPlayerSetVolume: function (playerIndex, volume) {
        if (!_hasVideos(playerIndex)) {
            return;
        }

        _videos[playerIndex].video.volume = volume;
    },
    AVPPlayerGetVolume__deps: ["videos", "hasVideos"],
    AVPPlayerGetVolume: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        return _videos[playerIndex].video.volume;
    },
    AVPPlayerHasVideo__deps: ["videos", "hasVideos"],
    AVPPlayerHasVideo: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        var isChrome = !!window.chrome && !!window.chrome.webstore;

        if(isChrome){
            return Boolean(_videos[playerIndex].video.webkitVideoDecodedByteCount);
        }
        
        if(_videos[playerIndex].video.videoTracks){
            return Boolean(_videos[playerIndex].video.videoTracks.length);
        }
        
        return true;
    },
    AVPPlayerHasAudio__deps: ["videos", "hasVideos"],
    AVPPlayerHasAudio: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        return _videos[playerIndex].video.mozHasAudio || Boolean(_videos[playerIndex].video.webkitAudioDecodedByteCount) || Boolean(_videos[playerIndex].video.audioTracks && _videos[playerIndex].video.audioTracks.length);
    },
    AVPPlayerAudioTrackCount__deps: ["videos", "hasVideos"],
    AVPPlayerAudioTrackCount: function (playerIndex) {
    	if (!_hasVideos(playerIndex)) {
    		return 0;
    	}
    	var result = 0;
    	if (_videos[playerIndex].video.audioTracks)
    	{
    		result = _videos[playerIndex].video.audioTracks.length;
    	}
    	return result;
    },
    AVPPlayerSetAudioTrack__deps: ["videos", "hasVideos"],
    AVPPlayerSetAudioTrack: function (playerIndex, trackIndex) {
    	if (!_hasVideos(playerIndex)) {
    		return;
    	}
    	if (_videos[playerIndex].video.audioTracks) {
    		var audioTracks = _videos[playerIndex].video.audioTracks;
    		for (i = 0; i < audioTracks.length; i++) {
    			audioTracks[i].enabled = (i == trackIndex);
    		}
    	}
    },
    AVPPlayerGetDecodedFrameCount__deps: ["videos", "hasVideos"],
    AVPPlayerGetDecodedFrameCount: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return 0;
        }

        var vid = _videos[playerIndex].video;
        if (vid.readyState <= HTMLMediaElement.HAVE_CURRENT_DATA) {
            return 0;
        }

        var frameCount = 0;

        if (vid.webkitDecodedFrameCount)
        {
        	frameCount = vid.webkitDecodedFrameCount;
        }
        else if (vid.mozDecodedFrames)
        {
        	frameCount = vid.mozDecodedFrames;
        }

        return frameCount;
    },
    AVPPlayerSupportedDecodedFrameCount__deps: ["videos", "hasVideos"],
    AVPPlayerSupportedDecodedFrameCount: function (playerIndex) {
        if (!_hasVideos(playerIndex)) {
            return false;
        }

        var vid = _videos[playerIndex].video;

        if (vid.webkitDecodedFrameCount)
        {
        	return true;
        }
        else if (vid.mozDecodedFrames)
        {
        	return true;
        }

        return false;
    },    
    AVPPlayerGetNumBufferedTimeRanges__deps: ["videos", "hasVideos"],
    AVPPlayerGetNumBufferedTimeRanges: function(playerIndex){   
        if (!_hasVideos(playerIndex)) {
            return 0;
        }

        return _videos[playerIndex].video.buffered.length;
    },
    AVPPlayerGetTimeRangeStart__deps: ["videos", "hasVideos"],
    AVPPlayerGetTimeRangeStart: function(playerIndex, rangeIndex){
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        if(rangeIndex >= _videos[playerIndex].video.buffered.length){
            return 0.0;
        }

        return _videos[playerIndex].video.buffered.start(rangeIndex);
    },
    AVPPlayerGetTimeRangeEnd__deps: ["videos", "hasVideos"],
    AVPPlayerGetTimeRangeEnd: function(playerIndex, rangeIndex){
        if (!_hasVideos(playerIndex)) {
            return 0.0;
        }

        if(rangeIndex >= _videos[playerIndex].video.buffered.length){
            return 0.0;
        }

        return _videos[playerIndex].video.buffered.end(rangeIndex);
    }
};

autoAddDeps(AVProVideoWebGL, 'count');
autoAddDeps(AVProVideoWebGL, 'videos');
autoAddDeps(AVProVideoWebGL, 'hasVideos');
mergeInto(LibraryManager.library, AVProVideoWebGL);