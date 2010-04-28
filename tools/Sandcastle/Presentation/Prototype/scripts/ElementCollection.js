
	// a collection of elements whoose style can be changed based on the values of attributes

	function ElementCollection(parentId) {

		// get the tabbed region
		this.parent = document.getElementById(parentId);


		// get the children
		this.elements = new Array();
		if (this.parent.tagName == 'TABLE') {
			// special handling for tables
			var bodies = this.parent.tBodies;
			for(i=0; i<bodies.length; i++) {
				var rows = bodies[i].rows;
				for(j=0; j<rows.length; j++) {
					if (rows[j].nodeType == 1) this.elements.push(rows[j]);
				}
			}
			// this.elements = this.parent.tBodies[0].rows;
		} else {
			// all other cases
			var nodes = this.parent.childNodes;
			for(i=0; i<nodes.length; i++) {
				if (nodes[i].nodeType == 1) this.elements.push(nodes[i]);
			}
		}

	}

	ElementCollection.prototype.process = function(processFunction) {
		for(var i=0; i<this.elements.length; i++) {
			var element = this.elements[i];
			processFunction(element);
		}
	}

	ElementCollection.prototype.changeStyle = function(attributeName, attributeValue, styleName, styleValue) {
		for(var i=0; i<this.elements.length; i++) {
			var element = this.elements[i];
			var value = element.getAttribute(attributeName);
			if (value != null) {
				if (value == attributeValue) {
					element.style[styleName] = styleValue;
				}
			}
		}
	}


	ElementCollection.prototype.toggleStyle = function(attributeName, attributeValue, styleName, trueStyleValue, falseStyleValue) {
		for(var i=0; i<this.elements.length; i++) {
			var element = this.elements[i];
			if (element.nodeType != 1) continue;
			var value = element.getAttribute(attributeName);
			if (value == null) continue;

			if (value == attributeValue) {
				element.style[styleName] = trueStyleValue;
			} else {
				element.style[styleName] = falseStyleValue;
			}
		}
	}

	ElementCollection.prototype.toggleClass = function(attributeName, attributeValue, trueClass, falseClass) {
		for(var i=0; i<this.elements.length; i++) {
			var element = this.elements[i];
			if (element.nodeType != 1) continue;
			var value = element.getAttribute(attributeName);
			if (value == null) continue;

			if (value == attributeValue) {
				element.className = trueClass;
			} else {
				element.className = falseClass;
			}
		}
	}

	function useShowAttribute(element) {
		if (element == null) return;
		var value = element.getAttribute("show");
		if (value == null) return;
		if (value == "true") {
			element.style["display"] = "block";
		} else {
			element.style["display"] = "none";
		}
	}
