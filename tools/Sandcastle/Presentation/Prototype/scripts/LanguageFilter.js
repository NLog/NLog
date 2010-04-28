
function LanguageFilterController() {
        this.tabCollections = new Array();
        this.blockCollections = new Array();
        this.tabCollectionIds = new Array();
        this.blockCollectionIds = new Array();
}

LanguageFilterController.prototype.registerTabbedArea = function(tabCollection, blockCollection, tabCollectionId, blockCollectionId) {
        this.tabCollections.push(tabCollection);
        this.blockCollections.push(blockCollection);
        this.tabCollectionIds.push(tabCollectionId);
        this.blockCollectionIds.push(blockCollectionId);
}

LanguageFilterController.prototype.switchLanguage = function(languageId) {
        for(var i=0; i<this.tabCollections.length; i++) {
          var tabs = this.tabCollections[i];
          var blocks = this.blockCollections[i];
          tabs.toggleClass('x-lang',languageId,'activeTab','tab');
          blocks.toggleStyle('x-lang',languageId,'display','block','none');
        }
}

LanguageFilterController.prototype.switchLanguage2 = function(languageId) {

}
