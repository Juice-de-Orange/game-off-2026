// New tab and user agent for the bug report button in the pause menu (P78).
// Browsers block window.open outside a real user-gesture handler, and Unity handles clicks a frame later (see
// Fullscreen.jslib). So on pointer down the game arms a one-shot listener here, and the browser's own pointerup opens the tab.
mergeInto(LibraryManager.library, {
  GameOff_OpenUrlOnRelease: function (ptr) {
    var url = UTF8ToString(ptr);
    var handler = function () {
      document.removeEventListener('pointerup', handler, true);
      // Not 'noopener' as a feature: then window.open always returns null and a blocked tab can't be told apart.
      var tab = window.open(url, '_blank');
      if (tab) tab.opener = null;
      else console.log('[BugReport] new tab blocked, open this link: ' + url);
    };
    document.addEventListener('pointerup', handler, true);
  },

  GameOff_UserAgent: function () {
    var text = navigator.userAgent || 'unknown';
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  }
});
