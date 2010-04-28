function CopyCode(key)
{
	var trElements = document.getElementsByTagName("tr");
	var i;
	for(i = 0; i < trElements.length; ++i)
	{
		if(key.parentNode.parentNode.parentNode == trElements[i].parentNode)
		{
			window.clipboardData.setData("Text", trElements[i].innerText);
		}
	}
}

function ChangeCopyCodeIcon(key)
{
	var i;
	var imageElements = document.getElementsByName("ccImage")
	for(i=0; i<imageElements.length; ++i)
	{
		if(imageElements[i].parentNode == key)
		{
			if(imageElements[i].src == copyImage.src)
			{
				imageElements[i].src = copyHoverImage.src;
				imageElements[i].alt = copyHoverImage.alt;
			}
			else
			{
				imageElements[i].src = copyImage.src;
				imageElements[i].alt = copyImage.alt;
			}
		}
	}
}
