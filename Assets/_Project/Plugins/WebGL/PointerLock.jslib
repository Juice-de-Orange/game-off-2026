// Browser pointer-lock state and request (P02 pause menu, P46 windows).
// When the mouse is captured, the browser swallows Esc and only releases the lock; Unity's Cursor.lockState
// does not reliably notice that. document.pointerLockElement is the ground truth.
mergeInto(LibraryManager.library, {
  GameOff_IsPointerLocked: function () {
    return document.pointerLockElement ? 1 : 0;
  },

  // Asks the browser for the lock right away. Called just after a click or key press that closed a window: the browser
  // still counts that as the user's action, so the camera can turn at once instead of waiting for another click.
  GameOff_RequestPointerLock: function () {
    var canvas = Module['canvas'];
    if (!canvas || document.pointerLockElement === canvas || !canvas.requestPointerLock) return;
    var result = canvas.requestPointerLock();
    if (result && result.catch) result.catch(function () {}); // refused (no recent user action): the next click locks
  }
});
