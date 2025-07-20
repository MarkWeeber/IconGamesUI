mergeInto(LibraryManager.library, {
    SyncFiles: function() {
        FS.syncfs(false, function(err) {
            if (err) console.error("File sync failed:", err);
            // No callback to C# needed
        });
    }
});
mergeInto(LibraryManager.library, {
    AsyncDelay: function(ms, callback) {
        setTimeout(function() {
            Runtime.dynCall('v', callback);
        }, ms);
    }
});