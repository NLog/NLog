function codeBlockHandler(id, data, value, curvedTabCollections, tabCollections, blockCollections)
{
    var names = value.split(' ');
    
    //Blocks
    for(var blockCount = 0; blockCount < blockCollections.length; blockCount++)
    {
        toggleStyle(blockCollections[blockCount], 'x-lang', names[0], 'display', 'block', 'none');
    }
       
   //curvedTabs
   for(var curvedTabCount = 0; curvedTabCount < curvedTabCollections.length; curvedTabCount++)
   {
        curvedToggleClass(curvedTabCollections[curvedTabCount], 'x-lang',names[0]);
   }

   //Tabs
   for(var tabCount = 0; tabCount < tabCollections.length; tabCount++)
   {
        toggleClass(tabCollections[tabCount], 'x-lang', names[0], 'activeTab', 'tab');
   }    
}

function styleSheetHandler(id, data, value, curvedTabCollections, tabCollections, blockCollections)
{
    var names = value.split(' ');
    var name = names[1];
    toggleInlineStyle(name);
}

function persistenceHandler(id, data, value, curvedTabCollections, tabCollections, blockCollections)
{
    data.set('lang', value);
    data.save();
}

function languageHandler(id, data, value, curvedTabCollections, tabCollections, blockCollections)
{
	var names = value.split(' ');
	toggleLanguage(id, 'x-lang', names[0]);
}

toggleInlineStyle = function(name)
{
	var sd = getStyleDictionary();
	if (name == 'cs') {
		sd['span.cs'].display = 'inline';
		sd['span.vb'].display = 'none';
		sd['span.cpp'].display = 'none';
    	} else if (name == 'vb') {
		sd['span.cs'].display = 'none';
		sd['span.vb'].display = 'inline';
		sd['span.cpp'].display = 'none';
	} else if (name == 'cpp') {
		sd['span.cs'].display = 'none';
		sd['span.vb'].display = 'none';
		sd['span.cpp'].display = 'inline';
	} else {
	}
}

toggleLanguage = function(id, data, value)
{
	var tNodes = getChildNodes('languageFilterToolTip');
	
	for(var labelCount=0; labelCount < tNodes.length; labelCount++)
	{
		if(tNodes[labelCount].tagName != 'IMG' && tNodes[labelCount].tagName != '/IMG')
		{
            if(tNodes[labelCount].getAttribute('id').indexOf(value) >= 0)
			{
				tNodes[labelCount].style['display'] = 'inline';
			}
			else
			{
				tNodes[labelCount].style['display'] = 'none';
			}
		}
	}
		
	var languageNodes = getChildNodes(id);

	for(var languageCount=0; languageCount < languageNodes.length; languageCount++)
	{
		if(languageNodes[languageCount].tagName == 'DIV');
		{
			if(languageNodes[languageCount].getAttribute('id'))
		    {
		        var imageNodes = getChildNodes(languageNodes[languageCount].getAttribute('id'))[0];
                if (languageNodes[languageCount].getAttribute('id') == value)
		        {
			        imageNodes.src = radioSelectImage.src;
		        }
		        else
		        {
			        imageNodes.src = radioUnSelectImage.src;
		        }
            }
		}
	}
}

toggleStyle = function(blocks, attributeName, attributeValue, styleName, trueStyleValue, falseStyleValue) 
{
    var blockNodes = getChildNodes(blocks);
    
    for(var blockCount=0; blockCount < blockNodes.length; blockCount++)
    {
        var blockElement = blockNodes[blockCount].getAttribute(attributeName);
        if (blockElement == attributeValue) blockNodes[blockCount].style[styleName] = trueStyleValue;
        else blockNodes[blockCount].style[styleName] = falseStyleValue;
    }
}

curvedToggleClass = function(curvedTabs, attributeName, attributeValue) 
{
   var curvedTabNodes = getChildNodes(curvedTabs);
   
   for(var curvedTabCount=0; curvedTabCount < curvedTabNodes.length; curvedTabCount++)
   {
        var curvedTabElement = curvedTabNodes[curvedTabCount].getAttribute(attributeName);
	    if (curvedTabElement == attributeValue)
	    {
	        if (curvedTabNodes[curvedTabCount].className == 'leftTab' || curvedTabNodes[curvedTabCount].className == 'activeLeftTab')
		    {
		        curvedTabNodes[curvedTabCount].className = 'activeLeftTab';
		    }
		    else if(curvedTabNodes[curvedTabCount].className == 'rightTab' || curvedTabNodes[curvedTabCount].className == 'activeRightTab')
		    {
		        curvedTabNodes[curvedTabCount].className = 'activeRightTab';
		    }
		    else if(curvedTabNodes[curvedTabCount].className == 'middleTab' || curvedTabNodes[curvedTabCount].className == 'activeMiddleTab')
		    {
			    curvedTabNodes[curvedTabCount].className = 'activeMiddleTab';
		    }
	    }
	    else
	    {
		    if (curvedTabNodes[curvedTabCount].className == 'leftTab' || curvedTabNodes[curvedTabCount].className == 'activeLeftTab')
		    {
		        curvedTabNodes[curvedTabCount].className = 'leftTab';
		    }
		    else if(curvedTabNodes[curvedTabCount].className == 'rightTab' || curvedTabNodes[curvedTabCount].className == 'activeRightTab')
		    {
		        curvedTabNodes[curvedTabCount].className = 'rightTab';
		    }
		    else if(curvedTabNodes[curvedTabCount].className == 'middleTab' || curvedTabNodes[curvedTabCount].className == 'activeMiddleTab')
		    {
			    curvedTabNodes[curvedTabCount].className = 'middleTab';
		    }
	    }
    }
}

toggleClass = function(tabs, attributeName, attributeValue, trueClass, falseClass) 
{
   var tabNodes = getChildNodes(tabs);
   
   for(var tabCount=0; tabCount < tabNodes.length; tabCount++)
   {
	    var tabElement = tabNodes[tabCount].getAttribute(attributeName);
	
	    if (tabElement == attributeValue)
	    {
		    if(tabNodes[tabCount].className == 'leftGrad' || tabNodes[tabCount].className == 'activeLeftGrad')
		    { 											
		        tabNodes[tabCount].className = 'activeLeftGrad';
		    }
	        else if (tabNodes[tabCount].className == 'rightGrad' || tabNodes[tabCount].className == 'activeRightGrad')
		    { 
		        tabNodes[tabCount].className = 'activeRightGrad';
		    }
            else tabNodes[tabCount].className = trueClass;
        }
	    else
	    {
		    if(tabNodes[tabCount].className == 'leftGrad' || tabNodes[tabCount].className == 'activeLeftGrad') 
		    {									
			    tabNodes[tabCount].className = 'leftGrad';
		    }
	        else if (tabNodes[tabCount].className == 'rightGrad' || tabNodes[tabCount].className == 'activeRightGrad')
		    { 
			    tabNodes[tabCount].className = 'rightGrad';
		    }
		    else tabNodes[tabCount].className = falseClass;
	    }
    }
}

getChildNodes = function(node)
{
    var element = document.getElementById(node);

    // get the children
	if (element.tagName == 'TABLE') 
	{
	    // special handling for tables
		var bodies = element.tBodies;
		for(i = 0; i < bodies.length; i++) 
		{
		    var nodes = bodies[i].rows;
		    return nodes;
	    } 
    }
    else 
    {
	    // all other cases
		var nodes = element.childNodes;
		return nodes;
	}
}

process = function(list, methodName, typeName) {
    var listNodes = getChildNodes(list);
     
    for(var i=0; i < listNodes.length; i++) 
    {
	    var listElement = listNodes[i];
	    
	    if (typeName == 'type' && tf != null) getInstanceDelegate(tf,methodName)(listElement);
        else if (typeName == 'member' && mf != null) getInstanceDelegate(mf, methodName)(listElement);
    }
}
		
function getStyleDictionary() {
		var styleDictionary = new Array();

		// iterate through stylesheets
		var sheets = document.styleSheets;
		
		for(var i=0; i<sheets.length;i++) {
			var sheet = sheets[i];

			// Ignore sheets at ms-help Urls
        		if (sheet.href.substr(0,8) == 'ms-help:') continue;

			// get sheet rules
			var rules = sheet.cssRules;
			
			if (rules == null) rules = sheet.rules;

			// iterate through rules
			for(j=0; j<rules.length; j++) {
				var rule = rules[j];

				// Ignore ones that aren't defined
            			if(rule.selectorText == null)
                			continue;

				// add rule to dictionary
				styleDictionary[rule.selectorText.toLowerCase()] = rule.style;
            }
		}

		return(styleDictionary);
}

function toggleCheck(imageElement)
{
	if(imageElement.src == checkBoxSelectImage.src)
	{
		imageElement.src = checkBoxUnSelectImage.src;
		return false;
	}
	else
	{
		imageElement.src = checkBoxSelectImage.src;
		return true;
	}
}

function mouseOverCheck(imageElement, selected, unselected, selected_hover, unselected_hover)
{
	if(imageElement.src == selected.src)
	{
		imageElement.src = selected_hover.src;
	}
	else if(imageElement.src == unselected.src)
	{
		imageElement.src = unselected_hover.src;
	}
}


function mouseOutCheck(imageElement, selected, unselected, selected_hover, unselected_hover)
{
	if(imageElement.src == selected_hover.src)
	{
		imageElement.src = selected.src;
	}
	else if(imageElement.src == unselected_hover.src)
	{
		imageElement.src = unselected.src;
	}
}

function toggleSelect(imageElement, section)
{
	if(imageElement.src == twirlSelectImage.src)
	{
		imageElement.src = twirlUnSelectImage.src;
		section.style['display'] = 'none'; 
	}
	else
	{
		imageElement.src = twirlSelectImage.src;
		section.style['display'] = 'block';
	}
}

function changeLanguage(data, name, style) {
    if (languageFilter == null) return;
    
    languageFilter.changeLanguage(data, name, style);
}

function processSubgroup(subgroup, typeName) {
    if (typeName == 'type' && tf != null) tf.subgroup = subgroup;
    else if (typeName == 'member' && mf != null) mf.subgroup = subgroup;
}

function toggleCheckState(visibility, value) {
    if (mf == null) return;
    mf[visibility] = value;
}

	
	
