// Local platform detection only. No portal SDK, network requests or identifiers.
mergeInto(LibraryManager.library, {
  NativePlatform_IsMobile: function () {
    return /Android|iPhone|iPad|iPod|Mobile/i.test(navigator.userAgent) ||
      (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1) ? 1 : 0;
  }
});
