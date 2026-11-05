// Browser pointer-lock state for the pause menu (P02).
// When the mouse is captured, the browser swallows Esc and only releases the lock; Unity's Cursor.lockState
// does not reliably notice that. document.pointerLockElement is the ground truth.
mergeInto(LibraryManager.library, {
  GameOff_IsPointerLocked: function () {
    return document.pointerLockElement ? 1 : 0;
  }
});
