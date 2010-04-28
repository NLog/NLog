
	function getStyleDictionary() {
	
		var dictionary = new Array();

		// iterate through stylesheets
		var sheets = document.styleSheets;
		for(var i=0; i<sheets.length;i++) {
			var sheet = sheets[i];

            // ignore sheets at ms-help Urls
            if (sheet.href.substr(0,8) == 'ms-help:') continue;

			// get sheet rules
			var rules = sheet.cssRules;
			if (rules == null) rules = sheet.rules;
			
			// iterate through rules
			for(j=0; j<rules.length; j++) {
				var rule = rules[j];
				
				// add rule to dictionary
				dictionary[rule.selectorText.toLowerCase()] = rule.style;

			}
		}

		return(dictionary);

	}

	function toggleVisibleLanguage(id) {

        if (id == 'cs') {
			sd['span.cs'].display = 'inline';
			sd['span.vb'].display = 'none';
			sd['span.cpp'].display = 'none';
        } else if (id == 'vb') {
			sd['span.cs'].display = 'none';
			sd['span.vb'].display = 'inline';
			sd['span.cpp'].display = 'none';
		} else if (id == 'cpp') {
			sd['span.cs'].display = 'none';
			sd['span.vb'].display = 'none';
			sd['span.cpp'].display = 'inline';
		} else {
		}

	}

