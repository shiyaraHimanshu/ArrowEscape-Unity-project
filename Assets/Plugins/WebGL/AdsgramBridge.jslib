mergeInto(LibraryManager.library, {
    ShowAdInternal: function (blockIdStr, isRewarded) {
        var blockId = UTF8ToString(blockIdStr);
        const AdController = window.Adsgram.init({ blockId: blockId });

        AdController.show().then((result) => {
            // result.done is true if the ad was fully watched
            if (isRewarded) {
                SendMessage('AdsManager', 'OnRewardedFinished', result.done ? 1 : 0);
            } else {
                SendMessage('AdsManager', 'OnVideoFinished');
            }
        }).catch((result) => {
            console.error("Adsgram error: ", result);
            if (isRewarded) {
                SendMessage('AdsManager', 'OnRewardedFinished', 0);
            }
        });
    }
});