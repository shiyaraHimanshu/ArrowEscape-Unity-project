mergeInto(LibraryManager.library, {
    BuyAgentWithStars: function () {
        if (window.BuyAgentWithStars) {
            window.BuyAgentWithStars();
        } else {
            console.error("BuyAgentWithStars is not defined in the browser.");
            // For testing in browser without Telegram
            // SendMessage('UiManager', 'RewardAgent');
        }
    }
});
