// Copies text to the clipboard (P56 traffic dev tool, "Copy values"). Called right after a click, so the browser allows it.
mergeInto(LibraryManager.library, {
  GameOff_CopyText: function (ptr) {
    var text = UTF8ToString(ptr);
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).catch(function () { console.log(text); });
    } else {
      console.log(text);
    }
  }
});
