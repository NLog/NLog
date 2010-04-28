window.onload=LoadPage;
window.onunload=Window_Unload;
//window.onresize=ResizeWindow;
window.onbeforeprint = set_to_print;
window.onafterprint = reset_form;

var languageFilter;
var data;
var tf;
var mf;
var lang = 'CSharp';

var vbDeclaration;
var vbUsage;
var csLang;
var cLang;
var jsharpLang;
var jsLang;
var xamlLang;

var scrollPos = 0;

var inheritedMembers;
var protectedMembers;
var netcfMembersOnly;
var netXnaMembersOnly;

// Initialize array of section states

var sectionStates = new Array();
var sectionStatesInitialized = false;

//Hide sample source in select element
function HideSelect()
{
	var selectTags = document.getElementsByTagName("SELECT");
	var spanEles = document.getElementsByTagName("span");
	var i = 10;
	var m;
	
	if (selectTags.length != null || selectTags.length >0)
	{
		for (n=0; n<selectTags.length; n++)
		{			
			var lan = selectTags(n).getAttribute("id").substr("10");
			//hide the first select that is on
			switch (lan.toLowerCase())
			{
				case "visualbasic":
					//alert(lan);
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "VisualBasic" && spanEles[m].style.display != "none" && n <i)
							i = n;				
					}
					break;
				case "visualbasicdeclaration":
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "VisualBasicDeclaration" && spanEles[m].style.display != "none" && n < i)
							i = n;
					}
					break;
				case "visualbasicusage":
					//alert(lan);
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "VisualBasicUsage" && spanEles[m].style.display != "none" && n <i)
							i = n;				
					}
					break;
				case "csharp":
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "CSharp" && spanEles[m].style.display != "none" && n < i)
							i = n;
					}
					break;
				case "managedcplusplus":
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "ManagedCPlusPlus" && spanEles[m].style.display != "none" && n < i)
							i = n;
					}
					break;
				case "jsharp":
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "JSharp" && spanEles[m].style.display != "none" && n < i)
							i = n;
					}
					break;
				case "jscript":
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "JScript" && spanEles[m].style.display != "none" && n < i)
							i = n;
					}
					break;				
				case "xaml":
					//alert(lan);
					for (m=0; m<spanEles.length; m++)
					{					
						if (spanEles[m].getAttribute("codeLanguage") == "XAML" && spanEles[m].style.display != "none" && n <i)
							i = n;				
					}
					break;
			}							
		}
		if (i != 10)		
			selectTags(i).style.visibility = "hidden";
	}
	else{ alert("Not found!");}	
}

function UnHideSelect()
{		
	var selectTags = document.getElementsByTagName("SELECT");
	var n;
	
	//un-hide all the select sections
	if (selectTags.length != null || selectTags.length >0)
	{
		for (n=0; n<selectTags.length; n++)
			selectTags(n).style.visibility = "visible";
	}	
}

function InitSectionStates()
{
   sectionStatesInitialized = true;
    
    if (globals == null) globals = GetGlobals();

    // SectionStates has the format:
    //
    //     firstSectionId:state;secondSectionId:state;thirdSectionId:state; ... ;lastSectionId:state
    //
    // where state is either "e" (expanded) or "c" (collapsed)
    
    // get the SectionStates from the previous topics
    var states = Load("SectionStates");

    var start = 0;
    var end;
    var section;
    var state;
    var allCollapsed = false;
    // copy the previous section states to the sectionStates array for the current page
	if (states != null && states != "")
	{
	    allCollapsed = true;
        while (start < states.length)
        {
            end = states.indexOf(":", start);
            
            section = states.substring(start, end);
            
            start = end + 1;
            end = states.indexOf(";", start);
            if (end == -1) end = states.length;
            state = states.substring(start, end);
            sectionStates[section] = state;
    	    allCollapsed = allCollapsed && (state == "c");
    	    start = end + 1;
        }
	}
    
    // now set the state for any section ids in the current document that weren't in previous
	var imgElements = document.getElementsByName("toggleSwitch");
	var i;
	for (i = 0; i < imgElements.length; ++i)
        sectionStates[imgElements[i].id] = GetInitialSectionState(imgElements[i].id, allCollapsed);
}

function GetInitialSectionState(itemId, allCollapsed)
{
    // if the global state is "allCollapsed", set all section states to collapsed
    if (allCollapsed) return "c";
    
    // generic <section> node ids begin with "sectionToggle", so the same id can refer to different sections in different topics
    // we don't want to persist their state; set it to expanded 
    if (itemId.indexOf("sectionToggle", 0) == 0) return "e";
    
    // the default state for new section ids is expanded 
    if (sectionStates[itemId] == null) return "e";
    
    // otherwise, persist the passed in state 
    return sectionStates[itemId];
}

var noReentry = false;

function OnLoadImage(eventObj)
{
    if (noReentry) return;
    
    if (!sectionStatesInitialized) 
	    InitSectionStates(); 
   
    var elem;
    if(document.all) elem = eventObj.srcElement;
    else elem = eventObj.target;

        
    if ((sectionStates[elem.id] == "e"))
		ExpandSection(elem);
	else if((sectionStates[elem.id] == "c"))
		CollapseSection(elem);
}

/*	
**********
**********   Begin
**********
*/

function LoadPage()
{
	// If not initialized, grab the DTE.Globals object
	if (globals == null) globals = GetGlobals();
	
	// show correct language
	LoadLanguages();
	LoadMembersOptions();
	
	Set_up_checkboxes();

	DisplayLanguages();
	
	DisplayFilteredMembers();
		
	ChangeMembersOptionsFilterLabel();
	
    if (!sectionStatesInitialized) 
	    InitSectionStates(); 
	var imgElements = document.getElementsByName("toggleSwitch");
	
	for (i = 0; i < imgElements.length; i++)
	{
		if ((sectionStates[imgElements[i].id] == "e"))
			ExpandSection(imgElements[i]);
		else
			CollapseSection(imgElements[i]);
	}
	
	SetCollapseAll();

//	ResizeWindow();
	// split screen
	var screen = new SplitScreen('header', 'mainSection');

	// filtering dropdowns
	if (document.getElementById('languageSpan') != null) {
		var languageMenu = new Dropdown('languageFilterToolTip', 'languageSpan');
		languageFilter = new Selector('languageSpan');
        languageFilter.register(codeBlockHandler); 
        languageFilter.register(styleSheetHandler); 
        languageFilter.register(persistenceHandler);
        languageFilter.register(languageHandler);
        toggleLanguage('languageSpan', 'x-lang', 'CSharp');
        toggleInlineStyle('cs');
	}
	if (document.getElementById('membersOptionsFilterToolTip') != null) {
		var languageMenu = new Dropdown('membersOptionsFilterToolTip', 'membersOptionsSpan');
	}

	data = new DataStore('docs');
    registerEventHandler(window, 'load', function() {if (languageFilter != null) languageFilter.select(data)});
    
    // process tab behavior for syntax, snippets, type and member sections
    tf = new TypeFilter();
    mf = new MemberFilter();
    setUpSyntax();
    setUpSnippets();
    setUpType();
    setUpAllMembers();
    var mainSection = document.getElementById("mainSection");
	
	// vs70.js did this to allow up/down arrow scrolling, I think
	try { mainSection.setActive(); } catch(e) { }

	//set the scroll position
	try{mainSection.scrollTop = scrollPos;}
	catch(e){}
}

function Window_Unload()
{
	SaveLanguages();
	SaveMembersOptions();
	SaveSections();
}

/*
function ResizeWindow()
{
	if (document.body.clientWidth==0) return;
	var header = document.all.item("header");
	var mainSection = document.all.item("mainSection");
	if (mainSection == null) return;
	
	
	document.body.scroll = "no"
	mainSection.style.overflow= "auto";
	header.style.width= document.body.offsetWidth - 2;
	//mainSection.style.paddingRight = "20px"; // Width issue code
	mainSection.style.width= document.body.offsetWidth - 2;
	mainSection.style.top=0;  
	if (document.body.offsetHeight > header.offsetHeight + 10)
		mainSection.style.height= document.body.offsetHeight - (header.offsetHeight + 2);
	else
		mainSection.style.height=0;
	
	try
	{
		mainSection.setActive();
	}
	catch(e)
	{
	}
}
*/

function set_to_print()
{
	//breaks out of divs to print
	var i;

	if (window.text)document.all.text.style.height = "auto";
			
	for (i=0; i < document.all.length; i++)
	{
		if (document.all[i].tagName == "body")
		{
			document.all[i].scroll = "yes";
		}
		if (document.all[i].id == "header")
		{
			document.all[i].style.margin = "0px 0px 0px 0px";
			document.all[i].style.width = "100%";
		}
		if (document.all[i].id == "mainSection")
		{
			document.all[i].style.overflow = "visible";
			document.all[i].style.top = "5px";
			document.all[i].style.width = "100%";
			document.all[i].style.padding = "0px 10px 0px 30px";
		}
	}
}

function reset_form()
{
	//returns to the div nonscrolling region after print
	 document.location.reload();
}

function Set_up_checkboxes()
{
	var checkbox;
	
	checkbox = document.getElementById("vbDeclarationCheckbox");
	if(checkbox != null)
	{
		if(vbDeclaration == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("vbUsageCheckbox");
	if(checkbox != null)
	{
		if(vbUsage == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
		
	checkbox = document.getElementById("csCheckbox");
	if(checkbox != null)
	{
		if(csLang == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
		
	checkbox = document.getElementById("cCheckbox");
	if(checkbox != null)
	{
		if(cLang == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("jsharpCheckbox");
	if(checkbox != null)
	{
		if(jsharpLang == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
		
	checkbox = document.getElementById("jsCheckbox");
	if(checkbox != null)
	{
		if(jsLang == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("xamlCheckbox");
	if(checkbox != null)
	{
		if(xamlLang == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("inheritedCheckbox");
	if(checkbox != null)
	{
		if(inheritedMembers == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("protectedCheckbox");
	if(checkbox != null)
	{
		if(protectedMembers == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("netcfCheckbox");
	if(checkbox != null)
	{
		if(netcfMembersOnly == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
	
	checkbox = document.getElementById("netXnaCheckbox");
	if(checkbox != null)
	{
		if(netXnaMembersOnly == "on")
			checkbox.checked = true;
		else
			checkbox.checked = false;
	}
}

/*	
**********
**********   End
**********
*/


/*	
**********
**********   Begin Language Filtering
**********
*/

function SetLanguage(key)
{
    var i = 0;
	if(vbDeclaration == "on")
		i++;
	if(vbUsage == "on")
		i++;
	if(csLang == "on")
		i++;
	if(cLang == "on")
		i++;
	if(jsharpLang == "on")
		i++;
	if(jsLang == "on")
		i++;
	if(xamlLang == "on")
		i++;
	
	if(key.id == "vbDeclarationCheckbox")
	{
	    if(vbDeclaration == "on")
		{
		    if(i == 1)
			{
			    key.checked = true;
				return;
			}
			vbDeclaration = "off";
		}
		else
			vbDeclaration = "on";
	}
	if(key.id == "vbUsageCheckbox")
	{
		if(vbUsage == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
				
			vbUsage = "off";
		}
		else
			vbUsage = "on";
	}
	if(key.id == "csCheckbox")
	{
		if(csLang == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
			
			csLang = "off";
		}
		else
			csLang = "on";
	}
	if(key.id == "cCheckbox")
	{
		if(cLang == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
				
			cLang = "off";
		}
		else
			cLang = "on";
	}
	if(key.id == "jsharpCheckbox")
	{
		if(jsharpLang == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
				
			jsharpLang = "off";
		}
		else
			jsharpLang = "on";
	}
	if(key.id == "jsCheckbox")
	{
		if(jsLang == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
				
			jsLang = "off";
		}
		else
			jsLang = "on";
	}
	if(key.id == "xamlCheckbox")
	{
		if(xamlLang == "on")
		{
			if(i == 1)
			{
				key.checked = true;
				return;
			}
				
			xamlLang = "off";
		}
		else
			xamlLang = "on";
	}
	
	DisplayLanguages();
}

function DisplayLanguages()
{
	var spanElements = document.getElementsByTagName("span");
	var x = 0;
	if(vbDeclaration == "on")
		x++;
	if(vbUsage == "on")
		x++;
	if(csLang == "on")
		x++;
	if(cLang == "on")
		x++;
	if(jsharpLang == "on")
		x++;
	if(jsLang == "on")
		x++;
	if(xamlLang == "on")
		x++;

	var i;
	for(i = 0; i < spanElements.length; ++i)
	{
	    if(spanElements[i].getAttribute("codeLanguage") != null)
		{
		    if(spanElements[i].getAttribute("codeLanguage") == "VisualBasic")
			{
				if(vbDeclaration == "on" || vbUsage == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "VisualBasicDeclaration")
			{
			
				if(vbDeclaration == "on")
					spanElements[i].style.display = "";
				else{
				
					spanElements[i].style.display = "none";
					}
			}
			if(spanElements[i].getAttribute("codeLanguage") == "VisualBasicUsage")
			{
				if(vbUsage == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "CSharp")
			{
				if(csLang == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "ManagedCPlusPlus")
			{
				if(cLang == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "JSharp")
			{
				if(jsharpLang == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "JScript")
			{
				if(jsLang == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			if(spanElements[i].getAttribute("codeLanguage") == "XAML")
			{
				if(xamlLang == "on")
					spanElements[i].style.display = "";
				else
					spanElements[i].style.display = "none";
			}
			
			if(spanElements[i].getAttribute("codeLanguage") == "NotVisualBasicUsage")
			{
				if((x == 1) && (vbUsage == "on"))
				{
					spanElements[i].style.display = "none";
				}
				else
				{
					spanElements[i].style.display = "";
				}
			}
		}
	}
	ChangeLanguageFilterLabel();
}

function ChangeLanguageFilterLabel()
{	
	var i = 0;
	if(vbDeclaration == "on")
		i++;
	if(vbUsage == "on")
		i++;
	if(csLang == "on")
		i++;
	if(cLang == "on")
		i++;
	if(jsharpLang == "on")
		i++;
	if(jsLang == "on")
		i++;
	if(xamlLang == "on")
		i++;
		
	var labelElement;
	
	labelElement = document.getElementById("showAllLabel");
	
	if(labelElement == null)
		return;
		
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("multipleLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("vbLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("csLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("cLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("jsharpLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("jsLabel");
	labelElement.style.display = "none";
	
	labelElement = document.getElementById("xamlLabel");
	labelElement.style.display = "none";
	
	if(i == 7)
	{
		labelElement = document.getElementById("showAllLabel");
		labelElement.style.display = "inline";
	}
	else if ((i > 1) && (i < 7))
	{
		if((i == 2) && ((vbDeclaration == "on") && (vbUsage == "on")))
		{
			labelElement = document.getElementById("vbLabel");
			labelElement.style.display = "inline";
		}
		else
		{
			labelElement = document.getElementById("multipleLabel");
			labelElement.style.display = "inline";
		}
	}
	else if (i == 1)
	{
		if(vbDeclaration == "on" || vbUsage == "on")
		{
			labelElement = document.getElementById("vbLabel");
			labelElement.style.display = "inline";
		}
		if(csLang == "on")
		{
			labelElement = document.getElementById("csLabel");
			labelElement.style.display = "inline";
		}
		if(cLang == "on")
		{
			labelElement = document.getElementById("cLabel");
			labelElement.style.display = "inline";
		}
		if(jsharpLang == "on")
		{
			labelElement = document.getElementById("jsharpLabel");
			labelElement.style.display = "inline";
		}
		if(jsLang == "on")
		{
			labelElement = document.getElementById("jsLabel");
			labelElement.style.display = "inline";
		}
		if(xamlLang == "on")
		{
			labelElement = document.getElementById("xamlLabel");
			labelElement.style.display = "inline";
		}
	}
}

function LoadLanguages()
{
	var value;
	value = Load("vbDeclaration");
	if(value == null)
		vbDeclaration = "on";
	else
		vbDeclaration = value;
		
	value = Load("vbUsage");
	if(value == null)
		vbUsage = "on";
	else
		vbUsage = value;
		
	value = Load("csLang");
	if(value == null)
		csLang = "on";
	else
		csLang = value;
		
	value = Load("cLang");
	if(value == null)
		cLang = "on";
	else
		cLang = value;
	
	value = Load("jsharpLang");
	if(value == null)
		jsharpLang = "on";
	else
		jsharpLang = value;
		
	value = Load("jsLang");
	if(value == null)
		jsLang = "on";
	else
		jsLang = value;

	value = Load("xamlLang");
	if(value == null)
		xamlLang = "on";
	else
		xamlLang = value;
}

function SaveLanguages()
{
	Save("vbDeclaration", vbDeclaration);
	Save("vbUsage", vbUsage);
	Save("csLang", csLang);
	Save("cLang", cLang);
	Save("jsharpLang", jsharpLang);
	Save("jsLang", jsLang);
	Save("xamlLang", xamlLang);
}

/*	
**********
**********   End Language Filtering
**********
*/


/*	
**********
**********   Begin Members Options Filtering
**********
*/

function SetMembersOptions(key)
{
	if(key.id == "inheritedCheckbox")
	{
		if(key.checked == true)
			inheritedMembers = "on";
		else
			inheritedMembers = "off";
	}
	if(key.id == "protectedCheckbox")
	{
		if(key.checked == true)
			protectedMembers = "on";
		else
			protectedMembers = "off";
	}
	if(key.id == "netcfCheckbox")
	{
		if(key.checked == true)
			netcfMembersOnly = "on";
		else
			netcfMembersOnly = "off";
	}
	if(key.id == "netXnaCheckbox")
	{
		if(key.checked == true)
			netXnaMembersOnly = "on";
		else
			netXnaMembersOnly = "off";
	}
	DisplayFilteredMembers();
	
	ChangeMembersOptionsFilterLabel();
}

function DisplayFilteredMembers()
{
	var iAllMembers = document.getElementsByTagName("tr");
	var i;
	
	for(i = 0; i < iAllMembers.length; ++i)
	{
		if (((iAllMembers[i].getAttribute("protected") == "true") && (protectedMembers == "off")) ||
		    ((iAllMembers[i].notSupportedOnXna == "true") && (netXnaMembersOnly == "on")) ||
			((iAllMembers[i].getAttribute("name") == "inheritedMember") && (inheritedMembers == "off")) ||
			((iAllMembers[i].getAttribute("notSupportedOn") == "netcf") && (netcfMembersOnly == "on")))
			iAllMembers[i].style.display = "none";
		else
			iAllMembers[i].style.display = "";
	}
}

function ChangeMembersOptionsFilterLabel()
{	

	var showAllMembersLabelElement = document.getElementById("showAllMembersLabel");
	var filteredMembersLabelElement = document.getElementById("filteredMembersLabel");
	
	if(showAllMembersLabelElement == null || filteredMembersLabelElement == null)
		return;
		
	if ((inheritedMembers=="off") || (protectedMembers=="off") || (netXnaMembersOnly == "on") || (netcfMembersOnly=="on"))
	{
		filteredMembersLabelElement.style.display = "inline";
	    showAllMembersLabelElement.style.display = "none";
	}
	else
	{
		filteredMembersLabelElement.style.display = "none";
	    showAllMembersLabelElement.style.display = "inline";
	}
}

function LoadMembersOptions()
{
	var value;
	value = Load("inheritedMembers");
	if(value == null)
		inheritedMembers = "on";
	else
		inheritedMembers = value;
		
	value = Load("protectedMembers");
	if(value == null)
		protectedMembers = "on";
	else
		protectedMembers = value;
		
	value = Load("netcfMembersOnly");
	if(value == null)
		netcfMembersOnly = "off";
	else
		netcfMembersOnly = value;
	
	value = Load("netXnaMembersOnly");
	if(value == null)
		netXnaMembersOnly = "off";
	else
		netXnaMembersOnly = value;
}

function SaveMembersOptions()
{
	Save("inheritedMembers", inheritedMembers);
	Save("protectedMembers", protectedMembers);
	Save("netcfMembersOnly", netcfMembersOnly);
	Save("netXnaMembersOnly", netXnaMembersOnly);
}

/*	
**********
**********   End Members Options Filtering
**********
*/


/*	
**********
**********   Begin Expand/Collapse
**********
*/

// expand or collapse a section
function ExpandCollapse(imageItem)
{
	if (sectionStates[imageItem.id] == "e")
		CollapseSection(imageItem);
	else
		ExpandSection(imageItem);
	
	SetCollapseAll();
}

// expand or collapse all sections
function ExpandCollapseAll(imageItem)
{
    var collapseAllImage = document.getElementById("collapseAllImage");
    var expandAllImage = document.getElementById("expandAllImage");
    if (imageItem == null || collapseAllImage == null || expandAllImage == null) return;
    noReentry = true; // Prevent entry to OnLoadImage
    
	var imgElements = document.getElementsByName("toggleSwitch");
	var i;
	var collapseAll = (imageItem.src == collapseAllImage.src);
	if (collapseAll)
	{
		imageItem.src = expandAllImage.src;
		imageItem.alt = expandAllImage.alt;

		for (i = 0; i < imgElements.length; ++i)
		{
			CollapseSection(imgElements[i]);
		}
	}
	else
	{
		imageItem.src = collapseAllImage.src;
		imageItem.alt = collapseAllImage.alt;

		for (i = 0; i < imgElements.length; ++i)
		{
			ExpandSection(imgElements[i]);
		}
	}
	SetAllSectionStates(collapseAll);
	SetToggleAllLabel(collapseAll);
	
	noReentry = false;
}

function ExpandCollapse_CheckKey(imageItem, eventObj)
{
	if(eventObj.keyCode == 13)
		ExpandCollapse(imageItem);
}

function ExpandCollapseAll_CheckKey(imageItem, eventObj)
{
	if(eventObj.keyCode == 13)
		ExpandCollapseAll(imageItem);
}

function SetAllSectionStates(collapsed)
{
    for (var sectionId in sectionStates) 
        sectionStates[sectionId] = (collapsed) ? "c" : "e";
}

function ExpandSection(imageItem)
{
    noReentry = true; // Prevent re-entry to OnLoadImage
    try
    {
        var collapseImage = document.getElementById("collapseImage");
		imageItem.src = collapseImage.src;
		imageItem.alt = collapseImage.alt;
		
	    imageItem.parentNode.parentNode.nextSibling.style.display = "";
	    sectionStates[imageItem.id] = "e";
    }
    catch (e)
    {
    }
    noReentry = false;
}

function CollapseSection(imageItem)
{
    noReentry = true; // Prevent re-entry to OnLoadImage
    var expandImage = document.getElementById("expandImage");
	imageItem.src = expandImage.src;
	imageItem.alt = expandImage.alt;
	imageItem.parentNode.parentNode.nextSibling.style.display = "none";
	sectionStates[imageItem.id] = "c";
    noReentry = false;
}

function AllCollapsed()
{
	var imgElements = document.getElementsByName("toggleSwitch");
	var allCollapsed = true;
	var i;
		
	for (i = 0; i < imgElements.length; i++) allCollapsed = allCollapsed && (sectionStates[imgElements[i].id] == "c");
	
	return allCollapsed;
}

function SetCollapseAll()
{
	var imageElement = document.getElementById("toggleAllImage");
	if (imageElement == null) return;
	
	var allCollapsed = AllCollapsed();
	if (allCollapsed)
	{
        var expandAllImage = document.getElementById("expandAllImage");
	    if (expandAllImage == null) return;
		imageElement.src = expandAllImage.src;
		imageElement.alt = expandAllImage.alt;
	}
	else
	{
        var collapseAllImage = document.getElementById("collapseAllImage");
	    if (collapseAllImage == null) return;
		imageElement.src = collapseAllImage.src;
		imageElement.alt = collapseAllImage.alt;
	}
	
	SetToggleAllLabel(allCollapsed);
}

function SetToggleAllLabel(allCollapsed)
{
	var collapseLabelElement = document.getElementById("collapseAllLabel");
	var expandLabelElement = document.getElementById("expandAllLabel");
	
	if (collapseLabelElement == null || expandLabelElement == null) return;
		
	if (allCollapsed)
	{
		collapseLabelElement.style.display = "none";
		expandLabelElement.style.display = "inline";
	}
	else
	{
		collapseLabelElement.style.display = "inline";
		expandLabelElement.style.display = "none";
	}
}

function SaveSections()
{
    try
    {
        var states = "";
    
        for (var sectionId in sectionStates) states += sectionId + ":" + sectionStates[sectionId] + ";";

        Save("SectionStates", states.substring(0, states.length - 1));
    }
    catch (e)
    {
    }
    
}

function OpenSection(imageItem)
{
	if (sectionStates[imageItem.id] == "c") ExpandCollapse(imageItem);
}

/*	
**********
**********   End Expand/Collapse
**********
*/



/*	
**********
**********   Begin Copy Code
**********
*/

function CopyCode(key)
{
	var trElements = document.getElementsByTagName("tr");
	var i;
	for(i = 0; i < trElements.length; ++i)
	{
		if(key.parentNode.parentNode.parentNode == trElements[i].parentNode)
		{
		    if (window.clipboardData) 
            {
                // the IE-manner
                window.clipboardData.setData("Text", trElements[i].innerText);
            }
            else if (window.netscape) 
            { 
                // Gives unrestricted access to browser APIs using XPConnect
		try
		{
			netscape.security.PrivilegeManager.enablePrivilege("UniversalXPConnect");
		}
	        catch(e)
		{
			alert("Universal Connect was refused, cannot copy to " +
				"clipboard.  Go to about:config and set " +
				"signed.applets.codebase_principal_support to true to " +
				"enable clipboard support.");
			return;
		}
                
                // Creates an instance of nsIClipboard
                var clip = Components.classes['@mozilla.org/widget/clipboard;1'].createInstance(Components.interfaces.nsIClipboard);
                if (!clip) return;
   
                // Creates an instance of nsITransferable
                var trans = Components.classes['@mozilla.org/widget/transferable;1'].createInstance(Components.interfaces.nsITransferable);
                if (!trans) return;
   
                // register the data flavor
                trans.addDataFlavor('text/unicode');
   
                // Create object to hold the data
                var str = new Object();
                                
                // Creates an instance of nsISupportsString
                var str = Components.classes["@mozilla.org/supports-string;1"].createInstance(Components.interfaces.nsISupportsString);
                
                //Assigns the data to be copied
                var copytext = trElements[i].textContent;
                str.data = copytext;
                
                // Add data objects to transferable
                trans.setTransferData("text/unicode",str,copytext.length*2);
                var clipid = Components.interfaces.nsIClipboard;
                if (!clip) return false;
        
                // Transfer the data to clipboard
                clip.setData(trans,null,clipid.kGlobalClipboard);
            }
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
				key.className = 'copyCodeOnHover';
			}
			else
			{
				imageElements[i].src = copyImage.src;
				imageElements[i].alt = copyImage.alt;
				key.className = 'copyCode';
			}
		}
	}
}

function CopyCode_CheckKey(key, eventObj)
{
	if(eventObj.keyCode == 13)
		CopyCode(key);
}

/*	
**********
**********   End Copy Code
**********
*/


/*	
**********
**********   Begin Maintain Scroll Position
**********
*/

function loadAll(){
	try 
	{
		scrollPos = allHistory.getAttribute("Scroll");
	}
	catch(e){}
}

function saveAll(){
	try
	{
		allHistory.setAttribute("Scroll", mainSection.scrollTop);
	}
	catch(e){}
}

/*	
**********
**********   End Maintain Scroll Position
**********
*/


/*	
**********
**********   Begin Send Mail
**********
*/

function formatMailToLink(anchor)
{
	var release = "Release: " + anchor.doc_Release;
	var topicId = "Topic ID: " + anchor.doc_TopicID;
	var topicTitle = "Topic Title: " + anchor.doc_TopicTitle;
	var url = "URL: " + document.URL;
	var browser = "Browser: " + window.navigator.userAgent;

	var crlf = "%0d%0a"; 
	var body = release + crlf + topicId + crlf + topicTitle + crlf + url + crlf + browser + crlf + crlf + "Comments:" + crlf + crlf;
	
	anchor.href = anchor.href + "&body=" + body;
}

/*	
**********
**********   End Send Mail
**********
*/


/*	
**********
**********   Begin Persistence
**********
*/

var globals;

function GetGlobals()
{
	var tmp;
	
	// Try to get VS implementation
	try { tmp = window.external.Globals; }
	catch (e) { tmp = null; }
	
	// Try to get DExplore implementation
	try { if (tmp == null) tmp = window.external.GetObject("DTE", "").Globals; }
	catch (e) { tmp = null; }
	
	return tmp;
}

function Load(key)
{
	try 
	{
		return globals.VariableExists(key) ? globals.VariableValue(key) : null;
	}
	catch (e)
	{
		return null;
	}
}

function Save(key, value)
{
	try
	{
		globals.VariableValue(key) = value;
		globals.VariablePersists(key) = true;
	}
	catch (e)
	{
	}
}

/*	
**********
**********   End Persistence
**********
*/

/* This is the part for Glossary popups */
// The method is called when the user positions the mouse cursor over a glossary term in a document.
// Current implementation assumes the existence of an associative array (g_glossary). 
// The keys of the array correspond to the argument passed to this function.

var bGlossary=true;
var oDialog;
var oTimeout="";
var oTimein="";
var iTimein=.5;
var iTimeout=30;
var oLastNode;
var oNode;
var bInit=false;
var aTerms=new Array();

// Called from mouseover and when the contextmenu behavior fires oncontextopen.
function clearDef(eventObj){
    if(eventObj){
        var elem;
        if(document.all) elem = eventObj.toElement;
        else elem = eventObj.relatedTarget;
	    if(elem!=null || elem!="undefined"){
		    if(typeof(oTimein)=="number"){
			    window.clearTimeout(oTimein);
		    }
		    if(oDialog.dlg_status==true){
			    hideDef();
		    }
		}
	}
}
function hideDef(eventObj){
	window.clearTimeout(oTimeout);
	oTimeout="";
	oDialog.style.display="none";
	oDialog.dlg_status=false;	
}
function showDef(oSource){
	if(bInit==false){
		glossaryInit();
		bInit=true;
	}
	if(bGlossary==true){
		if(typeof(arguments[0])=="object"){
			oNode=oSource;
		}
		else{
		    if(document.all) oNode = eventObj.srcElement;
		    else oNode = eventObj.target;
		}
		var bStatus=oDialog.dlg_status; // BUGBUG: oDialog is null.
		if((oLastNode!=oNode)||(bStatus==false)){
			if((typeof(oTimein)=="number")&& eventObj){
			    
			    var elem;
			    if(document.all) elem = eventObj.fromElement;
			    else elem = eventObj.relatedTarget;
			    
			    if( elem != null || elem != "undefined")
				    window.clearTimeout(oTimein);
			}
			oTimein=window.setTimeout("openDialog(oNode)",iTimein*1000);
		}	
	}
}



function glossaryInit(){
		oDialog=fnCreateDialog(150,50);
}

function navigateTerm(eventObj){
    var oNode;
    if(document.all) oNode = eventObj.srcElement;
    else oNode = eventObj.target;
	
	var iTermID=oNode.termID;
	if(oNode!=aTerms[iTermID]){
		var iAbsTop=getAbsoluteTop(aTerms[iTermID]);
		if(iAbsTop<document.body.scrollTop){
			window.scrollTo(document.body.scrollLeft,getAbsoluteTop(aTerms[iTermID]));
		}
		openDialog(aTerms[iTermID]);
	}
}
function disableGlossary(eventObj){
	if(bGlossary==true){
	    if(document.all) eventObj.srcElement.innerText="Enable Automatic Glossary";
		else eventObj.target.innerText="Enable Automatic Glossary";
		bGlossary=false;
		hideDef();		
	}
	else{
	    if(document.all) eventObj.srcElement.innerText="Disable Automatic Glossary";
		else eventObj.target.innerText="Disable Automatic Glossary";
		bGlossary=true;
	}
}
function openGlossary(){

}
function fnSetMenus(eventObj){
    var oNode;
    if(document.all) oNode = eventObj.srcElement;
    else oNode = eventObj.target;
	
	var oMenu=oNode.createMenu("SPAN","G_RID");
	var oSubItem1=oNode.createMenuItem("Glossary",fnStub,oMenu,true);
	document.body.createMenuItem("Open External Glossary",openGlossary,oSubItem1.subMenu);
	document.body.createMenuItem("Disable Automatic Glossary",disableGlossary,oSubItem1.subMenu);	
	for(var i=0;i<aTerms.length;i++){
		var oItem=document.body.createMenuItem(aTerms[i].innerText,navigateTerm,oMenu);
		oItem.termID=i;
	}
}
// This is a bogus stub.  It should be sniffed out rather than added in.
function fnStub(){

}
function fnAttachMenus(aTips){
	// This walk is only necessary for the context menu.
	var aTips=document.getElementsByTagName("SPAN");
	for(var i=0;i<aTips.length;i++){
		var oNode=aTips[i];
		if(oNode.getAttribute("G_RID")){
			var sTerm=oNode.getAttribute("G_RID");
			if(typeof(g_glossary[sTerm])=="string"){
				// Removed client-side scripting to add events.  This entire process should be singled out for IE 5 and later .. and, its only for the context menu.
				aTerms[aTerms.length]=oNode;
			}
		}
	}
	if(oBD.majorVer>=5){
		document.body.addBehavior(gsContextMenuPath);
		document.body.onbehaviorready="fnSetMenus()";
		document.body.oncontextopen="clearDef()";
	}

}
// Called by showDef.  The showDef function sniffs for initialization.
function openDialog(oNode,x,y){
 	var bStatus=oDialog.dlg_status; // BUGBUG: This code assumes that oDialog has been initialized
	if(bStatus==false){
		oDialog.dlg_status=true;
		oDialog.style.display="block";
	}
	else{
		if(typeof(oTimeout)=="number"){
			window.clearTimeout(oTimeout);
		}
	}
	
	var sTerm=oNode.getAttribute("G_RID");	
	var oDef=oNode.children(0);
	var sDef=oDef.text;
	sDef=sDef.substr(4,sDef.length-7);	//Strips the html comment markers from the definition.
	oDialog.innerHTML=sDef
	
	
	//oDialog.innerHTML=g_glossary[sTerm];
		
	var iScrollLeft=document.body.scrollLeft;
	var iScrollTop=document.body.scrollTop;
	var iOffsetLeft=getAbsoluteLeft(oNode)// - iScrollLeft;
	var iOffsetWidth=oNode.offsetWidth;
	var oParent=oNode.parentNode;
	var iOffsetParentLeft=getAbsoluteLeft(oParent);
	var iOffsetTop=getAbsoluteTop(oNode); //- iScrollTop;
	var iOffsetDialogWidth=oDialog.offsetWidth;
	
	
	if((iOffsetLeft + iOffsetWidth) > (iOffsetParentLeft + oParent.offsetWidth)){
		iOffsetLeft=iOffsetParentLeft;
		if(iOffsetLeft - iOffsetDialogWidth>0){
			iOffsetTop+=oNode.offsetHeight;
		}
	}
	var iLeft=0;
	var iTop=0;
	if((iOffsetLeft + iOffsetWidth - iScrollLeft + iOffsetDialogWidth) < document.body.offsetWidth ){
		iLeft=iOffsetLeft + iOffsetWidth;
	}
	else{
		if(iOffsetLeft - iOffsetDialogWidth>0){
			iLeft=iOffsetLeft - iOffsetDialogWidth;
		}
		else{
			iLeft=iOffsetParentLeft;
		}
	}
	if(iOffsetTop - iScrollTop<oDialog.offsetHeight){
		iTop=iOffsetTop + oNode.offsetHeight;
	}
	else{
		iTop=iOffsetTop - oDialog.offsetHeight;
	}
	oDialog.style.top=iTop;
	oDialog.style.left=iLeft;
	oTimeout=window.setTimeout("hideDef()",iTimeout*1000);	
}
function getAbsoluteTop(oNode){
	var oCurrentNode=oNode;
	var iTop=0;
	while(oCurrentNode.tagName!="BODY"){
		iTop+=oCurrentNode.offsetTop;
		oCurrentNode=oCurrentNode.offsetParent;
	}
	return iTop;
}
function getAbsoluteLeft(oNode){
	var oCurrentNode=oNode;
	var iLeft=0;
	while(oCurrentNode.tagName!="BODY"){
		iLeft+=oCurrentNode.offsetLeft;
		oCurrentNode=oCurrentNode.offsetParent;
	}
	return iLeft;
}
function fnCreateDialog(iWidth,iHeight){
	document.body.insertAdjacentHTML("BeforeEnd","<DIV></DIV>");
	oNewDialog=document.body.children(document.body.children.length-1);
	oNewDialog.className="clsTooltip";
	oNewDialog.style.width=iWidth;
	oNewDialog.dlg_status=false;
	return oNewDialog;
}

function sendfeedback(subject, id,alias){
	var rExp = /\"/gi;
	var url = location.href;
	// Need to replace the double quotes with single quotes for the mailto to work.
	var rExpSingleQuotes = /\'\'"/gi;

	var title;
	if(document.getElementsByTagName("TITLE")[0].innerText) title = document.getElementsByTagName("TITLE")[0].innerText.replace(rExp, "''") 
	else title = document.getElementsByTagName("TITLE")[0].textContent.replace(rExp, "''");
	location.href = "mailto:" + alias + "?subject=" + subject + title + "&body=Topic%20ID:%20" + id + "%0d%0aURL:%20" + url + "%0d%0a%0d%0aComments:%20";
}

function setUpSyntax() {
    var syntaxSection = document.getElementById('syntaxCodeBlocks');
    if (syntaxSection == null) return;
    
    processSection(syntaxSection, 'x-lang', lang, true, true, true, true);
}

function setUpSnippets() {
    var divs = document.getElementsByTagName("DIV");
   
    for (var i = 0; i < divs.length; i++)
    {
        var name =  divs[i].getAttribute("name");
        if (name == null || name != "snippetGroup") continue;
        processSection(divs[i], 'x-lang', lang, true, true, true, true);
    }
}

function setUpType() {
    var typeSection = document.getElementById("typeSection");
    if (typeSection == null) return;
    
    processSection(typeSection, 'value', 'all', true, false, true, false);
}

function setUpAllMembers() {
    var allMembersSection = document.getElementById("allMembersSection");
    if (allMembersSection == null) return;
    
    processSection(allMembersSection, 'value', 'all', true, false, true, false);
}

function processSection(section, attribute, value, toggleClassValue, toggleStyleValue, curvedToggleClassValue, registerValue) {
    var nodes = section.childNodes;
        
    var curvedTabId;
    var tabId;
    var blockId;
       
    if (nodes.length != 2) return;
    
    if (nodes[0].tagName == 'TABLE') {
        var rows = nodes[0].getElementsByTagName('tr');
           
        if (rows.length != 2) return;
        
        curvedTabId = rows[0].getAttribute('id');
        tabId = rows[1].getAttribute('id');
    }
  
    if(nodes[1].tagName == 'DIV') {
        blockId = nodes[1].getAttribute('id');
    }
       
    if (toggleClassValue) toggleClass(tabId,attribute,value,'activeTab','tab');
	if (toggleStyleValue) toggleStyle(blockId,attribute,value,'display','block','none');
    if (curvedToggleClassValue) curvedToggleClass(curvedTabId, attribute, value);
    
    if (languageFilter == null) return;
    
    if (registerValue) languageFilter.registerTabbedArea(curvedTabId, tabId, blockId);
}

