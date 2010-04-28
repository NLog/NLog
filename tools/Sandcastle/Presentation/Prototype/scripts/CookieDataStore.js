

			function setCookie(name, value, expires, path, domain, secure) {
				
				var text = name + "=" + escape(value);

				if (expires) {
					var currentDate = new Date();
					var expireDate = new Date( currentDate.getTime() + expires*24*60*60*1000 );
					text = text + ";expires=" + expireDate.toGMTString();
				}
				if (path) text = text + ";path=" + path;
				if (domain) text = text + ";domain=" + domain;
				if (secure) text = text + ";secure";

				document.cookie = text;
			}

			function getCookie(name) {

				var text = document.cookie;

				var index = text.indexOf(name + "=");
				if (index < 0) return(null);

				var start = index + name.length + 1;

				var end = text.indexOf(";", start);
				if (end < 0) end = text.length;

				var value = unescape( text.substring(start, end) );
				return(value);
				
			}

			function removeCookie(name) {
				setCookie(name, "", -1);
			}


			// cookie data store

			function CookieDataStore(name) {
				this.name = name;
				this.load();
			}

			CookieDataStore.prototype.load = function () {

				// create a key/value store
				this.data = new Object();

				// get cookie text
				var text = getCookie(this.name);
				if (text == null) return;

				// populate the store using the cookie text
				var data = text.split(';');

				for (var i=0; i<data.length; i++) {
					var datum = data[i];
					var index = datum.indexOf('=');
					if (index > 0) {
						var key = datum.substring(0,index);
						var value = datum.substring(index+1);
						this.data[key] = value;
					}
				}

			}

			CookieDataStore.prototype.save = function () {

				// prepare a cookie string
				var text = "";

				// construct the string
				for (var key in this.data) {
					var datum = key + "=" + this.data[key];
					text = text + datum + ";";
				}

				// set it
				setCookie(this.name, text);

			}

			CookieDataStore.prototype.clear = function () {
				this.data = new Object();
			}

			CookieDataStore.prototype.set = function(key, value) {
				this.data[key] = value;
			}

			CookieDataStore.prototype.get = function(key) {
				return(this.data[key]);
			}

			CookieDataStore.prototype.remove = function(key) {
				delete(this.data[key]);
			}

			CookieDataStore.prototype.count = function() {
				var i = 0;
				for (var key in this.data) {
					i++;
				}
				return(i);				
			}

		// The following logic needs to be re-factored out of this file

		function selectLanguage(value) {

			if (value == null) return;

			var selector = document.getElementById('languageSelector');
			if (selector == null) return;

			var options = selector.options;
			for(var i=0; i<options.length; i++) {
				if (options[i].value == value) {
					selector.selectedIndex = i;
					setLanguage(value);
				}
			}						

		}


		function setLanguage(value) {
			var names = value.split(' ');
			toggleVisibleLanguage(names[1]);
			lfc.switchLanguage(names[0]);
		}