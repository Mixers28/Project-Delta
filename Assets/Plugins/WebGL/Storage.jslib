mergeInto(LibraryManager.library, {
  SetApiUrl: function (urlPtr) {
    const url = UTF8ToString(urlPtr);
    if (!url) return;
    try {
      localStorage.setItem('api_url', url);
    } catch (err) {
      console.warn('SetApiUrl failed', err);
    }
  }
});
