// cookie data store
function DataStore(name) 
{
    this.name = name;
    this.load();
}
			
DataStore.prototype.load = function () 
{
    // create a key/value store
	this.language = new Object();

	// get cookie text
	var text = getCookie(this.name);
	
	if (text == null) return;

	// populate the store using the cookie text
	var data = text.split(';');

	for (var i=0; i<data.length; i++) 
	{
	    var datum = data[i];
		var index = datum.indexOf('=');
		
		if (index > 0) 
		{
		    var key = datum.substring(0,index);
			var value = datum.substring(index+1);
			this.language[key] = value;
		}
	}
	
}
			
function setCookie(name, value, expires, path, domain, secure) 
{
    var text = name + "=" + escape(value);
	
	if (expires) 
	{
	
	    var currentDate = new Date();
		var expireDate = new Date( currentDate.getTime() + expires*24*60*60*1000 );
		text = text + ";expires=" + expireDate.toGMTString();
	}
	if (path) text = text + ";path=" + path;
	if (domain) text = text + ";domain=" + domain;
	if (secure) text = text + ";secure";

	document.cookie = text;
}

function removeCookie(name) 
{
    setCookie(name, "", -1);
}

function getCookie(name) 
{
    var text = document.cookie;
    
	var index = text.indexOf(name + "=");
				
	if (index < 0) return(null);
    
    var start = index + name.length + 1;
    var end = text.indexOf(";", start);
	
	if (end < 0) end = text.length;

	var value = unescape( text.substring(start, end) );
	return(value);
}

DataStore.prototype.set = function(key, value) 
{
    this.language[key] = value;
}

DataStore.prototype.get = function(key) 
{
    return(this.language[key]);
}
			
DataStore.prototype.clear = function () 
{
    this.language = new Object();
}	

DataStore.prototype.save = function () 
{
    // prepare a cookie string
	var text = "";

	// construct the string
	for (var key in this.language) 
	{
	    var datum = key + "=" + this.language[key];
		text = text + datum + ";";
	}
				
	// set it
	setCookie(this.name, text);
}

DataStore.prototype.count = function() 
{
    var i = 0;
	for (var key in this.data) 
	{
	    i++;
	}
	return(i);				
}
			