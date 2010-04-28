
	function toggleSection(sectionElement) {
		var children = sectionElement.childNodes;
		if (children.length != 2) return;

		var image = children[0].getElementsByTagName('IMG')[0];
		var content = children[1];

		if (content.style['display'] == 'none') {
			content.style['display'] = 'block';
			image.src = '../icons/collapse_all.gif';
		} else {
			content.style['display'] = 'none';
			image.src= '../icons/expand_all.gif';
		}

	}
