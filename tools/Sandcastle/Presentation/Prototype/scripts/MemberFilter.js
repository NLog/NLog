
	// a member filter

	function MemberFilter () {

		// set up defaults

		this.subgroup = "all";

		this.public = true;
		this.protected = true;
		this.private = true;

		this.instance = true;
		this.static = true;

		this.declared = true;
		this.inherited = true;

	}

	MemberFilter.prototype.filterElement = function(element) {

		// get the data for the element
		if (element == null) return;
		var data = element.getAttribute("data");
		if (data == null) return;
		var datum = data.split("; ");
		if (datum.length != 4) return;
        
		// extract the relevent member attributes
		var subgroup = datum[0];
		var visibility = datum[1];
		var binding = datum[2];
		var origin = datum[3];

		// determine whether to show the member
		var show = true;
		if (this[visibility] == false) show = false;
		if (this[binding] == false) show = false;
		if (this[origin] == false) show = false;
		if ((this.subgroup != null) && (this.subgroup != 'all')) {
			if (subgroup != this.subgroup) show = false;
		}

		// show or hide the element
		if (show) {
			// either block or table-row, depending on browswer, so use default
			element.style["display"] = "";
		} else {
			element.style["display"] = "none";
		}

	}

	// a type filter

	function TypeFilter () {

		// set up defaults

		this.subgroup = "all";

		this.public = true;
		this.internal = true;

	}

	TypeFilter.prototype.filterElement = function(element) {

		// get the data for the element
		if (element == null) return;
		var data = element.getAttribute("data");
		if (data == null) return;
		var datum = data.split("; ");
		if (datum.length != 2) return;
        
		// extract the relevent member attributes
		var subgroup = datum[0];
		var visibility = datum[1];

		// determine whether to show the member
		var show = true;
		if (this[visibility] == false) show = false;
		if ((this.subgroup != null) && (this.subgroup != 'all')) {
			if (subgroup != this.subgroup) show = false;
		}

		// show or hide the element
		if (show) {
			// either block or table-row, depending on browser, so use default
			element.style["display"] = "";
		} else {
			element.style["display"] = "none";
		}

	}

	