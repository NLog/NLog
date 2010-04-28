
	function SplitScreen (nonScrollingRegionId, scrollingRegionId) {

		// store references to the two regions
		this.nonScrollingRegion = document.getElementById(nonScrollingRegionId);
		this.scrollingRegion = document.getElementById(scrollingRegionId);

        // set the position model for each region
		this.nonScrollingRegion.style.position = "fixed";
		this.scrollingRegion.style.position = "absolute";

		// fix the size of the scrolling region
		this.resize(null);

		// add an event handler to resize the scrolling region when the window is resized		
		registerEventHandler(window, 'resize', getInstanceDelegate(this, "resize"));

	}

	SplitScreen.prototype.resize = function(e) {

		if(navigator.userAgent.indexOf("Firefox")==-1)
  		{	
			var height = document.body.clientHeight - this.nonScrollingRegion.offsetHeight;

			if(height > 0) this.scrollingRegion.style.height = height + "px";
			else this.scrollingRegion.style.height = 0 + "px";

			this.scrollingRegion.style.width = document.body.clientWidth + "px";
		}

	
        // update the vertical offset of the scrolling region to account for the height of the non-scrolling region
        this.scrollingRegion.style.top = this.nonScrollingRegion.offsetHeight + "px";
	}
