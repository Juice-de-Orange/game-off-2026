// Fullscreen for the menu corner button (P43).
// Browsers only allow requestFullscreen inside a real user-gesture handler. Unity handles clicks a frame later and
// would apply Screen.fullScreen only on the NEXT click. So on pointer down the game arms a one-shot listener here,
// and the browser's own pointerup (a real gesture) toggles fullscreen right away.
mergeInto(LibraryManager.library, {
  GameOff_IsFullscreen: function () {
    return (document.fullscreenElement || document.webkitFullscreenElement) ? 1 : 0;
  },

  GameOff_ToggleFullscreenOnRelease: function () {
    var canvas = Module['canvas'];
    var handler = function () {
      document.removeEventListener('pointerup', handler, true);
      if (document.fullscreenElement || document.webkitFullscreenElement) {
        (document.exitFullscreen || document.webkitExitFullscreen).call(document);
      } else if (canvas) {
        var request = canvas.requestFullscreen || canvas.webkitRequestFullscreen;
        var result = request ? request.call(canvas) : null;
        if (result && result.catch) result.catch(function () {}); // e.g. iframe without allowfullscreen
      }
    };
    document.addEventListener('pointerup', handler, true);
  }
});
