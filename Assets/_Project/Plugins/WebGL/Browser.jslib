// The browser's user agent for the in-game bug report (P84).
mergeInto(LibraryManager.library, {
  GameOff_UserAgent: function () {
    var text = navigator.userAgent || 'unknown';
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  },

  // P129: the session id the page made for its "visit" event (index.html), so visit and play are one session; "" if none.
  GameOff_PageSession: function () {
    var text = (typeof window !== 'undefined' && window.K9_SESSION) ? String(window.K9_SESSION) : '';
    var size = lengthBytesUTF8(text) + 1;
    var buffer = _malloc(size);
    stringToUTF8(text, buffer, size);
    return buffer;
  }
});
