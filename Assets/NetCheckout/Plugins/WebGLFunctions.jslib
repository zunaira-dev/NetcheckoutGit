mergeInto(LibraryManager.library, {

  OpenLink: function (url, newTab) {
    url = Pointer_stringify(url);
    window.open(url, newTab ? '_blank' : '_self');
  },
});