// The browser's user agent for the in-game bug report (P84).
mergeInto(LibraryManager.library, {
  GameOff_UserAgent: function () {
    var text = navigator.userAgent || 'unknown';
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  }
});
