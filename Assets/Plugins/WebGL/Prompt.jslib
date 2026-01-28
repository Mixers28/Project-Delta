mergeInto(LibraryManager.library, {
  ShowPrompt: function (gameObjectNamePtr, callbackMethodPtr, messagePtr, defaultValuePtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    var callbackMethod = UTF8ToString(callbackMethodPtr);
    var message = UTF8ToString(messagePtr);
    var defaultValue = UTF8ToString(defaultValuePtr);
    var result = window.prompt(message, defaultValue);
    if (result === null) {
      result = "";
    }
    SendMessage(gameObjectName, callbackMethod, result);
  }
});
