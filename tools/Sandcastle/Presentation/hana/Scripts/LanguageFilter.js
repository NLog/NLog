/*

function Selector2 (id, tag, attribute) {
    this.root = document.getElementById(id);
    this.elements = new Array();
    this.values = new Array();
    this.registerChildren(root, attribute);
    
    this.handlers = new Array();
}

Selector2.prototype.registerChildren(parent, tag, attribute) {
    var children = parent.childNodes;
    for (var i = 0; i<children.length; i++) {
        var child = children[i];
        if (child.nodeType == 1) {
            var value = child.getAttribute(attribute);
            if (value) {
                this.elements.add(child);
                this.values.add(value);
            }
            this.registerChildren(child, tag, attribute);
        }
    }
}

Selector2.prototype.registerHandler(handler) {
    this.handlers.add(handler);
}

Selector2.prototype.select(value) {
    for (var i=0; i<handlers.length; i++) {
        this.handlers[i](this, value);
    }
}
*/

function Selector(id)
{
    this.selectorHandlers = new Array();
    this.curvedTabCollections = new Array();
    this.tabCollections = new Array();
    this.blockCollections = new Array();
    this.id = id;
}

Selector.prototype.register=function(handler)
{
   this.selectorHandlers.push(handler);
}

Selector.prototype.registerTabbedArea = function(curvedTabCollection, tabCollection, blockCollection)
{
    this.curvedTabCollections.push(curvedTabCollection);
    this.tabCollections.push(tabCollection);
    this.blockCollections.push(blockCollection);
}

Selector.prototype.changeLanguage = function(data, name, style)
{

    var value= name + ' ' + style;
	
    for(var handler in this.selectorHandlers)
    {
        this.selectorHandlers[handler](this.id, data, value, this.curvedTabCollections, this.tabCollections, this.blockCollections);
    }
}

Selector.prototype.select = function(data) 
{
    var value = data.get('lang');

    if (value == null) return;
    var names = value.split(' ');
   
	var nodes = getChildNodes(this.id);

	for( var i=0; i<nodes.length; i++)
	{
	    if(nodes[i].getAttribute('id') == names[0])
		{	
			styleSheetHandler(this.id, data, value, this.curvedTabCollections, this.tabCollections, this.blockCollections);
			codeBlockHandler(this.id, data, value, this.curvedTabCollections, this.tabCollections, this.blockCollections);	
			languageHandler(this.id, data, value, this.curvedTabCollections, this.tabCollections, this.blockCollections);	
		}
	}
}


	
