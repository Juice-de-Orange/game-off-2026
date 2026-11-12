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
  },

  // P138: the game wants the mouse captured (walking, seat, telescope, flight) or not (menus, counter). While it does, every
  // mouse press on the canvas asks for the lock inside the press itself: the one moment every browser accepts as the user's
  // action. So after Esc, a tab switch or a refused request the next click always catches the mouse again.
  GameOff_SetPointerLockWanted: function (wanted) {
    var canvas = Module['canvas'];
    if (!canvas) return;
    Module.gameOffLockWanted = wanted !== 0;
    if (Module.gameOffLockListener) return;
    Module.gameOffLockListener = function (event) {
      if (!Module.gameOffLockWanted || event.button !== 0 || document.pointerLockElement === canvas || !canvas.requestPointerLock) return;
      var result = canvas.requestPointerLock();
      if (result && result.catch) result.catch(function () {}); // e.g. Chrome right after Esc: the click after the next one locks
    };
    canvas.addEventListener('mousedown', Module.gameOffLockListener);
  }
});
