
function closeMe()
{
	window.close();
}

function ChangeViewCodeIcon(key)
{
	var i;
	var imageElements = document.getElementsByName("vcImage");
	for(i=0; i<imageElements.length; ++i)
	{
		if(imageElements[i].parentElement == key)
		{
			if(imageElements[i].src == viewImage.src)
				imageElements[i].src = viewHoverImage.src;
			else
				imageElements[i].src = viewImage.src;
		}
	}
}

function ChangeDownloadCodeIcon(key)
{
	var i;
	var imageElements = document.getElementsByName("dcImage");
	for(i=0; i<imageElements.length; ++i)
	{
		if(imageElements[i].parentElement == key)
		{
			if(imageElements[i].src == downloadImage.src)
				imageElements[i].src = downloadHoverImage.src;
			else
				imageElements[i].src = downloadImage.src;
		}
	}
}

function ViewSampleSource(name)
{
	// variables
	var wConfig;
	var oSelectBox = document.all.item(name);
	var url;
	var sIndex;
	
	// Get the selectedIndex
	sIndex = oSelectBox.selectedIndex;
	
	if (sIndex >= 0)
	{
		// Get the URL to the file
		url = oSelectBox.options[sIndex].value;

		// Set the configuration
		wConfig += 'directories=0,';
		wConfig += 'location=0,';
		wConfig += 'menubar=0,';
		wConfig += 'resizable=1,';
		wConfig += 'scrollbars=1,';
		wConfig += 'status=0,';
		wConfig += 'toolbar=0';

		// Launch the window
		window.open(url, 'ViewSampleSource', wConfig);
	}
}