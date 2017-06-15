var Editor = (function(){
    // ============================================================================================
    // Utility
    // ============================================================================================

    var getElementStyles, getScrollX, getScrollY, resizeTextArea, getDescendant, getDescendants;
    var isExplorer, isEdge;

    // TODO: Find better method of determining if user is using internet explorer, edge, or a good browser.
    if (document.body.currentStyle)
    {
        isExplorer  = true;
        isEdge      = false;

        getElementStyles    = function(element) {return element.currentStyle;}
        getScrollX          = function() {return window.pageXOffset;}
        getScrollY          = function() {return window.pageYOffset;}
        getDescendant       = function(element, selector) {return element.querySelector(selector);}
        getDescendants      = function(element, selector) {return element.querySelectorAll(selector);}
    }
    else
    {
        isExplorer  = false;
        isEdge      = false;

        getElementStyles    = function(element) {return getComputedStyle(element);}
        getScrollX          = function() {return window.scrollX;}
        getScrollY          = function() {return window.scrollY;}
        getDescendant       = function(element, selector) {return element.querySelector(":scope " + selector);}
        getDescendants      = function(element, selector) {return element.querySelectorAll(":scope " + selector);}
    }

    function createLoader(form)
    {
        var loader  = document.createElement("div");
        var html    = "";

        html += "<img src=\"rolling.gif\" class=\"spinner\" />";
        html += "<div>" + form.dataset.loader + "</div>";

        loader.className = "loader";
        loader.innerHTML = html;

        form.parentNode.appendChild(loader);
        form.style.visibility = "hidden";
    }

    // ============================================================================================
    // Core
    // ============================================================================================

    var controller          = "";
    var defaultParameters   = null;

    var root        = document.createElement("div");
    root.className  = "_editor";

    var forms = {};

    var hoverPrevious = "";

    var dialog = (function()
    {
        var element         = document.createElement("div");
        element.className   = "dialog closed";
        element.innerHTML   = "<div><h3></h3><div class='body'></div><div class='footer'></div><button class='close-dialog' data-dialog='close'>Cancel</button><button class='save-dialog' data-dialog='save'>Save</button></div>";

        var titleElement    = element.children[0].children[0];
        var bodyElement     = element.children[0].children[1];
        var footerElement   = element.children[0].children[2];
        var closeButton     = element.children[0].children[3];
        var saveButton      = element.children[0].children[4];
        var currentForm     = null;
        var currentTool     = null;

        root.appendChild(element);

        return {
            getRoot: function() {return element;},
            isOpen: function() {return currentForm || currentTool;},
            setForm: function(pending)
            {
                this.close();

                if (pending)
                {
                    element.className       = "dialog";
                    titleElement.innerText  = pending.title;
                    bodyElement.appendChild(pending.element);
                    currentForm = pending;
                }
            },
            setFormById: function(id)
            {
                dialog.setForm(EditForm.forms[id]);
            },
            setTool: function(pending)
            {
                this.close();

                currentTool = pending;
                bodyElement.appendChild(currentTool.element);

                element.className = "dialog";

                if (currentTool.dialogTitle)
                    titleElement.innerText = currentTool.dialogTitle;
                else
                    titleElement.innerText = "Dialog";
            },
            close: function()
            {
                if (currentTool)
                    currentTool.close();

                currentForm = null;
                currentTool = null;

                if (bodyElement.children.length > 0)
                    root.appendChild(bodyElement.children[0]);

                element.className = "dialog closed";
            },
            save: function()
            {
                if (currentForm)
                    EditForm.post(currentForm);
                else if (currentTool)
                    currentTool.applyEdit();

                this.close();
            }
        }
    })();

    // ============================================================================================
    // Resources
    // ============================================================================================

    var css     = document.createElement("link");
    css.rel     = "stylesheet";
    css.href    = "/Content/editor.css";

    document.body.appendChild(css);

    // ============================================================================================
    // EditHandle
    // ============================================================================================

    function EditHandle(editableElement)
    {
        this.element            = document.createElement("div");
        this.element.className  = "handle";
        this.element.innerHTML  = "<span class='icon edit'></span><span>Editable</span>";

        this.icon = this.element.children[0];
        this.text = this.element.children[1];

        this.lastEdit   = 0;
        this.timeoutID  = null;
        
        this.editableElement = editableElement;

        this.initialize();
        this.resetPosition();

        root.appendChild(this.element);
    }

    EditHandle.current  = null;
    EditHandle.num      = 0;
    EditHandle.handles  = {};
    EditHandle.anchors  =
    {
        topleft:        {x: 0,      y: 0},
        top:            {x: 0.5,    y: 0},
        topright:       {x: 1.0,    y: 0},
        left:           {x: 0,      y: 0.5},
        center:         {x: 0.5,    y: 0.5},
        right:          {x: 1.0,    y: 0.5},
        bottomleft:     {x: 0,      y: 1.0},
        bottom:         {x: 0.5,    y: 1.0},
        bottomright:    {x: 1.0,    y: 1.0},
    };

    EditHandle.setCurrent = function(handle)
    {
        if (dialog.isOpen())
            return;

        var current = EditHandle.current;

        if (current)
        {
            current.element.className   = "handle";
            current.text.innerText      = current.defaultText ? current.defaultText : "Editable";
        }

        if (handle == current)
            handle = null;

        if (handle)
        {
            EditHandle.current = handle;

            if (handle.element.className.indexOf("current") < 0)
                handle.element.className += " current";

            if (Tool.current)
                Tool.current.close();

            if (handle.editingTool)
            {
                handle.editingTool.open(handle.editableElement);
                Tool.current = handle.editingTool;
            }
            else if (handle.editableForm)
            {
                dialog.setFormById(handle.editableForm);
            }

            if (Tool.current.saveText || Tool.current.cancelText)
                handle.text.innerText = Tool.current.isChanged(null) ? Tool.current.saveText : Tool.current.cancelText;
        }
        else if (Tool.current)
        {
            Tool.current.close();
            Tool.current = null;
        }

        EditHandle.current = handle;
    }

    EditHandle.reset = function(handle)
    {
        if (handle != EditHandle.current)
            handle.text.innerText = handle.defaultText ? handle.defaultText : "Editable";
        else if (Tool.current.saveText || Tool.current.cancelText)
            handle.text.innerText = Tool.current.isChanged(null) ? Tool.current.saveText : Tool.current.cancelText;
    }

    EditHandle.hover = function(event)
    {
        var handle = EditHandle.handles[event.target.dataset.editid];

        if (handle)
            handle.element.className += " rollover";
    }

    EditHandle.blur = function(event)
    {
        var handle = EditHandle.handles[event.target.dataset.editid];

        if (handle)
            handle.element.className = handle.element.className.replace(" rollover", "");
    }

    EditHandle.prototype.initialize = function()
    {
        var element = this.editableElement;
        var editor  = element.dataset.editor.split(";");

        this.editableProperty   = editor[1];
        this.editingTool        = Tool.tools[editor[0].toLowerCase()];

        if (editor.length >= 3)
        {
            this.defaultText        = editor[2];
            this.text.innerText     = editor[2];
        }
        else
            this.element.style.display = "none";

        if (editor.length >= 4)
        {
            this.anchor = EditHandle.anchors[editor[3].toLowerCase()];
        }

        if (editor.length >= 5)
        {
            var i = editor[4].indexOf(",");
            var x = parseFloat(editor[4].substring(0, i));
            var y = parseFloat(editor[4].substring(i + 1));

            this.offset = {x: x, y: y};
        }

        if (editor.length >= 6)
            this.dialogTitle = editor[5];

        if (editor.indexOf("dialog") >= 0)
            this.editInDialog = true;

        if (!this.anchor)
            this.anchor = EditHandle.anchors.topleft;

        if (!this.offset)
            this.offset = {x: 0.0, y: 0.0};

        var name = (++EditHandle.num).toString();

        EditHandle.handles[name]    = this;
        element.dataset.editid      = name;
        this.element.dataset.editid = name;

        element.addEventListener("mouseover", EditHandle.hover);
        element.addEventListener("mouseout", EditHandle.blur);
    }

    EditHandle.prototype.setPosition = function(x1, y1, x2, y2)
    {
        var element = this.element;
        var rc      = this.editableElement.getBoundingClientRect();
        var rc2     = element.getBoundingClientRect();

        element.style.left  = (rc.left + rc.width * x1 + rc2.width * x2 + getScrollX()) + "px";
        element.style.top   = (rc.top + rc.height * y1 + rc2.height * y2 + getScrollY()) + "px";
    }

    EditHandle.prototype.resetPosition = function()
    {
        this.setPosition(this.anchor.x, this.anchor.y, this.offset.x, this.offset.y);
    }

    EditHandle.prototype.displayMessage = function(icon, text, duration)
    {
        if (this.timeoutID)
        {
            window.clearTimeout(this.timeoutID);
            this.timeoutID = null;
        }

        this.icon.className = "icon " + icon;
        this.text.innerText = text;

        if (typeof duration === "number")
            this.timeoutID = window.setTimeout(EditHandle.reset, duration * 1000, this);
    }

    // ============================================================================================
    // EditForm
    // ============================================================================================

    function EditForm(element)
    {
        this.element = element;

        this.initialize();
    }

    EditForm.forms      = {};
    EditForm.callbacks  = {};

    EditForm.post = function(form)
    {
        var data    = new FormData(form.element);
        var request = new XMLHttpRequest();

        if (defaultParameters)
            for (var i in defaultParameters)
                data.append(i, defaultParameters[i]);

        var callback = EditForm.callbacks[form.id];

        request.open("POST", controller + form.action, true);

        if (callback)
        {
            request.responseType    = callback.responseType;
            request.onloadend       = callback.fn;
        }

        request.send(data);
    }

    EditForm.prototype.initialize = function()
    {
        var element = this.element;
        var editor  = element.dataset.editor.split(";");

        element.onSubmit = function(e) {EditForm.post(this); e.preventDefault();}

        this.id     = editor[0];
        this.action = editor[1];
        this.title  = editor[2];

        EditForm.forms[this.id] = this;
    }

    // ============================================================================================
    // Changes
    // ============================================================================================

    var Change = (function()
    {
        var request             = new XMLHttpRequest();
        request.onloadend       = requestFinish;

        var pending = null;
        var last    = null;

        function post(node)
        {
            request.open("POST", controller + node.handle.editableProperty, true);
            request.setRequestHeader("Content-Type", node.contentType ? node.contentType : "application/json; charset=utf-8");
            request.send(node.pendingValue);
        }

        function requestFinish(response)
        {
            var result = response.currentTarget.response;
            var handle = pending.handle;
            
            result = JSON.parse(result);

            if (request.status == 200)
            {
                if (result.Status == 0)
                {
                    if (result.Value)
                        handle.editingTool.setValue(handle.editableElement, result.Value);

                    handle.displayMessage("", "Saved!", 2);
                }
                else
                {
                    console.log(pending);
                    if (pending.currentValue)
                        handle.editingTool.setValue(handle.editableElement, pending.currentValue);

                    handle.displayMessage("", "Error!", 2);
                }
            }
            else
            {
                if (pending.currentValue)
                    handle.editingTool.setValue(handle.editableElement, pending.currentValue);

                pending.handle.displayMessage("", "Error!", 2);
            }

            pending = pending.next;

            if (pending)
                post(pending);
            else
                last = null;
        }

        return {
            queue: function(_handle, _currentValue, _pendingValue, _contentType)
            {
                var data = _pendingValue;

                if (data instanceof FormData)
                {
                    if (defaultParameters)
                        for (var i in defaultParameters)
                            data.append(i, defaultParameters[i]);
                }
                else if (typeof data === "object")
                {
                    if (defaultParameters)
                        for (var i in defaultParameters)
                            data[i] = defaultParameters[i];

                    data = JSON.stringify(data);
                }

                var node =
                {
                    handle:         _handle,
                    currentValue:   _currentValue,
                    pendingValue:   data,
                    contentType:    _contentType
                };

                if (!pending)
                {
                    pending = node;
                    post(pending);
                }
                
                _handle.displayMessage("save", "Saving");

                if (last)
                    last.next = node;

                last = node;
            }
        }
    })();

    // ============================================================================================
    // Tools
    // ============================================================================================

    function Tool(options)
    {
        this.element            = document.createElement("div");
        this.element.className  = "tool " + options.name;
        this.element.innerHTML  = options.html;

        this.current        = null;
        this.previousValue  = null;

        this.closeOnSubmit  = options.closeOnSubmit;
        this.applyOnClose   = options.applyOnClose;

        this.isChanged  = options.isChanged;
        this.setValue   = options.setValue;
        this.applyEdit  = options.applyEdit;
        this.preEdit    = options.preEdit;
        this.beginEdit  = options.beginEdit;
        this.endEdit    = options.endEdit;

        this.saveText   = options.saveText;
        this.cancelText = options.cancelText;
        
        if (options.initialize)
            options.initialize.call(this);

        root.appendChild(this.element);
    }

    Tool.tools      = {};
    Tool.current    = null;

    Tool.prototype.apply = function()
    {
        if (!this.current)
            return;

        if (this.applyEdit)
            this.applyEdit();
    }

    Tool.prototype.open = function(target)
    {
        var element = this.element;
        var rc      = target.getBoundingClientRect();

        if (this.preEdit)
            this.preEdit();

        element.style.left      = (rc.left - 4) + "px";
        element.style.top       = (getScrollY() + rc.top - 4) + "px";
        element.style.width     = (rc.width + 8) + "px";
        element.style.height    = (rc.height + 8) + "px";

        if (element.className.indexOf("active") < 0)
            element.className = element.className + " active";

        this.current = target;

        if (this.beginEdit)
            this.beginEdit();

        element.children[0].focus();
    }

    Tool.prototype.close = function()
    {
        this.element.className = this.element.className.replace(" active", "");

        if (this.endEdit)
            this.endEdit();

        this.current = null;
    }

    function resizeTextArea()
    {
        var area            = Tool.tools.text.input;
        var currentHeight   = area.style.height;

        area.style.height = 0;
        area.style.height = area.scrollHeight + "px";

        EditHandle.reset(EditHandle.current);

        if (area.style.height != currentHeight)
        {
            Tool.current.current.style.minHeight = area.style.height;

            for (var i in EditHandle.handles)
                EditHandle.handles[i].resetPosition();
        }
    }

    /* General Text Editor */
    Tool.tools["text"] = new Tool(
    {
        name:           "text",
        displayName:    "Text Editor",
        html:           "<textarea></textarea>",
        saveText:       "Save",
        cancelText:     "Cancel",
        initialize: function()
        {
            this.input              = this.element.children[0];
            this.input.onkeydown    = function(e)
            {
                var current = EditHandle.current;
                var value   = e.target.value;
                var cursor  = e.target.selectionStart;

                if ((current.singleLine && e.which == 13) ||
                    //(current.characterLimit && value.length >= current.characterLimit && e.which != 8 && e.which != 13 && e.which != 46) ||
                    (e.which == 13 && value[cursor - 1] == "\n" && value[cursor - 2] == "\n"))
                {
                    e.preventDefault();
                    return false;
                }

                requestAnimationFrame(resizeTextArea);
            };
        },
        isChanged: function(element)
        {
            return this.input.value !== this.previousValue;
        },
        setValue: function(element, value)
        {
            element.innerText = value;
            Editor.refreshHandles();
        },
        applyEdit: function()
        {
            if (this.isChanged(this.current))
            {
                Change.queue(EditHandle.current, this.previousValue, {value: this.input.value.replace(/\r|\n/g, "<br>")});
                return true;
            }
            
            return false;
        },
        preEdit: function()
        {
            this.input.style.height = 0;
        },
        beginEdit: function()
        {
            var style = getElementStyles(this.current);

            this.input.style.color          = style.color;
            this.input.style.fontSize       = style.fontSize;
            this.input.style.textDecoration = style.textDecoration;
            this.input.style.fontStyle      = style.fontStyle;
            this.input.style.fontWeight     = style.fontWeight;
            this.input.style.padding        = style.padding;
            this.input.style.lineHeight     = style.lineHeight;
            this.input.value                = this.current.innerText;
            this.previousColor              = style.color;
            this.current.style.color        = "transparent";

            var area = this.input;

            requestAnimationFrame(function()
            {
                area.style.height = area.scrollHeight + "px";
            });

            this.previousValue  = this.current.innerText;
            this.previousHeight = this.current.style.minHeight;

            var rc = this.current.getBoundingClientRect();

            area.style.minWidth = rc.width + "px";
        },
        endEdit: function()
        {
            this.current.style.minHeight    = this.previousHeight;
            this.current.style.color        = this.previousColor;
            this.setValue(this.current, this.input.value);

            this.applyEdit();
        }
    });

    /* Image Browser Inline */
    Tool.tools["image"] = new Tool(
    {
        name:           "image",
        displayName:    "Image Editor",
        html:           "<form><input type='file' accept='image/*' data-allowdefault='true' /></form>",
        initialize: function()
        {
            this.input      = this.element.children[0].children[0];
            var imageEditor = this;

            this.input.onchange = function(event)
            {
                imageEditor.applyEdit();
            }
        },
        isChanged: function(element)
        {
            if (this.input.files.length < 1)
                return false;
        },
        setValue: function(element, value)
        {
            element.src = value;
        },
        applyEdit:  function()
        {
            if (this.input.files.length < 1)
                return false;

            var data = new FormData();
            var image = this.current;

            if (defaultParameters)
                for (var i in defaultParameters)
                    data.append(i, defaultParameters[i]);

            data.append(this.input.files[0].name, this.input.files[0]);
            data.append("images[]", this.input.files[0].name);

            var request = new XMLHttpRequest();
            request.open("POST", "/Artist/" + EditHandle.current.editableProperty, true);
            request.send(data);

            request.onloadend = function(response)
            {
                var result = JSON.parse(response.currentTarget.response);

                image.onload = function(){Editor.refreshHandles();}

                if (Array.isArray(result.ImageURL))
                    image.src = result.ImageURL[0];
                else
                    image.src = result.ImageURL;
            }

            EditHandle.setCurrent(null);

            return true;
        },
        beginEdit:  function()
        {
            this.input.value = "";
            this.input.click();
        },
        endEdit:    function()
        {
            this.input.value = "";
        }
    });

    var galleryPreview = new FileReader();
    var galleryCurrent = null;

    galleryPreview.addEventListener("load", function()
    {
        galleryCurrent.style = "background-image: url('" + galleryPreview.result + "');";
    }, false);

    function eraseGalleryImage(event)
    {
        var target = event.target;

        if (target.parentNode.style.backgroundImage)
        {
            target.parentNode.className = "delete";
            target.innerText            = "Undelete";
            target.onclick              = uneraseGalleryImage;
        }
        else
            target.parentNode.parentNode.removeChild(target.parentNode);

        event.preventDefault();
    }

    function uneraseGalleryImage(event)
    {
        var target = event.target;

        target.parentNode.className = "";
        target.innerText            = "Delete";
        target.onclick              = eraseGalleryImage;

        event.preventDefault();
    }

    function addGalleryImage(gallery, src)
    {
        if (typeof src === "string" && src.indexOf("/bannerimage/") < 0)
            return;

        var image       = document.createElement("span");
        image.innerHTML = "<button class='erase'>Delete</button>";
        image.children[0].onclick = eraseGalleryImage;

        image.dataset.candrag = true;
        image.dataset.candrop = true;

        if (typeof src !== "string")
        {
            src = src.cloneNode();

            galleryCurrent = image;
            galleryPreview.readAsDataURL(src.files[0]);

            image.appendChild(src);
        }
        else
            image.style = "background-image: url('" + src + "');";

        gallery.images.appendChild(image);
    }

    /* Gallery Editor */
    Tool.tools["gallery"] = new Tool(
    {
        name:           "gallery",
        displayName:    "Gallery Editor",
        html:           "<form data-processtext=\"Saving slideshow!\"></form><button></button><input type='file' accept='image/*' style='display: none' />",
        dialog:         false,
        initialize: function()
        {
            this.images     = this.element.children[0];
            this.modified   = false;

            var galleryEdit = this;
            var fileBrowser = this.element.children[2];

            this.element.children[0].onsubmit = function(event) {event.preventDefault(); return false;}
            this.element.children[1].onclick = function(event) {fileBrowser.click();};

            fileBrowser.onchange = function(event)
            {
                if (event.target.files.length > 0 && event.target.files[0])
                    addGalleryImage(galleryEdit, event.target);
            }
        },
        isChanged: function(element)
        {
            return this.modifed;
        },
        setValue: function(element, value)
        {

        },
        applyEdit:  function()
        {
            var data = new FormData();

            if (defaultParameters)
                for (var i in defaultParameters)
                    data.append(i, defaultParameters[i]);

            var slides  = this.element.children[0].children;
            var image   = null;
            var url     = null;

            for (var i = 0, s = slides.length; i < s; ++i)
            {
                if (slides[i].className.indexOf("delete") >= 0)
                    continue;

                if (slides[i].children.length > 1)
                {
                    image = slides[i].children[1];

                    if (image.files.length > 0 && image.files[0])
                    {
                        image = slides[i].children[1];

                        data.append(image.files[0].name, image.files[0]);
                        data.append("images[]", image.files[0].name);
                    }
                }
                else 
                {
                    url = slides[i].style.backgroundImage;
                    url = /url\("([^"]*)"\)/.exec(url);

                    if (url && url[1].indexOf("/bannerimage/") >= 0)
                        data.append("images[]", url[1]);
                }
            }

            var request = new XMLHttpRequest();
            request.open("POST", "/Artist/" + EditHandle.current.editableProperty, true);
            request.send(data);

            var galleryElement = this.current;

            request.onloadend = function(response)
            {
                var result  = JSON.parse(response.currentTarget.response);
                var list    = result.ImageURL;
                var html    = "";

                if (list)
                    for (var i = 0, s = list.length; i < s; ++i)
                        html += "<img src='" + list[i] + "' />";



                galleryElement.innerHTML = html;
            }

            return true;
        },
        beginEdit:  function()
        {
            var gallery = this.current;
            var image;

            for (var i = 0, s = gallery.children.length; i < s; ++i)
                addGalleryImage(this, gallery.children[i].src);

            dialog.setTool(this);
        },
        endEdit:    function()
        {
            var gallery = this.images;
            gallery.innerHTML = "";
            this.modifed = false;
        }
    });

    // ============================================================================================
    // Dragging
    // ============================================================================================

    var drag        = null;
    var dropTarget  = null;
    var dragRoot    = document.createElement("div");

    var dragOldPosition, dragOldLeft, dragOldTop, dragOldPointer, dragOldZIndex, dragOffsetX, dragOffsetY, dragX, dragY;

    dragRoot.className = "drag";
    root.appendChild(dragRoot);

    function dragStart(element, x, y)
    {
        if (drag)
            return;

        drag        = element;
        dropTarget  = null;

        var rc = drag.getBoundingClientRect();

        dragOffsetX = rc.left - x;
        dragOffsetY = rc.top - y;

        dragOldPosition             = drag.style.position;
        dragOldLeft                 = drag.style.left;
        dragOldTop                  = drag.style.top;
        dragOldPointer              = drag.style.pointerEvents;
        dragOldZIndex               = drag.style.zIndex;
        drag.style.position         = "fixed";
        drag.style.left             = (x + dragOffsetX) + "px";
        drag.style.top              = (y + dragOffsetY) + "px";
        drag.style.pointerEvents    = "none";
        drag.style.zIndex           = "100";

        dragX = x;
        dragY = y;

        event.preventDefault();
    }

    function dragMove(x, y)
    {
        drag.style.left = (x + dragOffsetX) + "px";
        drag.style.top  = (y + dragOffsetY) + "px";
        dragX = x;
        dragY = y;
    }

    function dragOver(element)
    {
        dropTarget = element;
    }

    function dragEnd(element, x, y)
    {
        if (element.dataset && element.dataset.candrop)
        {
            var rc1     = element.getBoundingClientRect();
            var rc2     = drag.getBoundingClientRect();

            if (rc2.left + rc2.width * 0.5 > rc1.left + rc1.width * 0.8)
            {
                var list = element.parentNode.children;
                var i = 0, s = list.length;

                for (; i < s; ++i)
                    if (list[i] == element)
                        break;

                if (++i < s)
                    element.parentNode.insertBefore(drag, list[i]);
                else
                    element.parentNode.appendChild(drag);
            }
            else
                element.parentNode.insertBefore(drag, element);
        }

        drag.style.position         = dragOldPosition;
        drag.style.left             = dragOldLeft;
        drag.style.top              = dragOldTop;
        drag.style.pointerEvents    = dragOldPointer;
        drag.style.zIndex           = dragOldZIndex;
        drag = null;
    }

    function touchDragEnd()
    {
        var element = document.elementFromPoint(dragX, dragY);

        // TODO: Determine whether to insert before or after depending on the locations of both the dragged item and drop target
        // Unfortunately, getBoundingClientRect returns extremely weird and unreliable values on mobile
        if (element && element.dataset && element.dataset.candrop)
            element.parentNode.insertBefore(drag, element);

        drag.style.position         = dragOldPosition;
        drag.style.left             = dragOldLeft;
        drag.style.top              = dragOldTop;
        drag.style.pointerEvents    = dragOldPointer;
        drag.style.zIndex           = dragOldZIndex;
        drag = null;
    }

    window.addEventListener("mousedown", function(event)
    {
        if (!event.target.dataset || !event.target.dataset.candrag)
            return;

        dragStart(event.target, event.clientX, event.clientY);
    }, false);

    window.addEventListener("mousemove", function(event)
    {
        if (!drag)
            return;

        dragMove(event.clientX, event.clientY);
    }, false);

    window.addEventListener("mouseup", function(event)
    {
        if (!drag)
            return;

        dragEnd(dropTarget, dragX, dragY);
    }, false);

    window.addEventListener("mouseover", function(event)
    {
        if (!drag)
            return;

        dragOver(event.target);
    }, false);

    window.addEventListener("touchstart", function(event)
    {
        var touch = event.touches[0];

        if (!touch.target.dataset || !touch.target.dataset.candrag)
            return;

        dragStart(touch.target, touch.clientX, touch.clientY);
    }, false);

    window.addEventListener("touchmove", function(event)
    {
        if (!drag)
            return;

        var touch = event.touches[0];
        dragMove(touch.clientX, touch.clientY);
    }, false);

    window.addEventListener("touchend", function(event)
    {
        if (!drag)
            return;

        touchDragEnd();
    }, false);

    // ============================================================================================
    // Event Handling
    // ============================================================================================

    window.addEventListener("click", function(event)
    {
        if (event.target.dataset.editid != null)
        {
            EditHandle.setCurrent(EditHandle.handles[event.target.dataset.editid]);
            event.preventDefault();
        }
        else
            EditHandle.setCurrent(null);
    }, false);

    root.addEventListener("click", function(event)
    {
        var t = event.target;

        if (!t.dataset || !t.dataset.allowdefault)
        {
            if (t.dataset.dialog)
            {
                switch (t.dataset.dialog)
                {
                    case "close":
                        dialog.close();
                        break;

                    case "save":
                        dialog.save();
                        break;
                }

                return;
            }

            while (t && t != root)
            {
                if (t.dataset && t.dataset.editid != null)
                {
                    if (EditHandle.current != t)
                        EditHandle.setCurrent(EditHandle.handles[t.dataset.editid]);
                    else
                        EditHandle.setCurrent(null);

                    break;
                }

                if (t == dialog.getRoot()) // avoid preventDefault and stopPropagation
                    return;

                t = t.parentNode;
            }

            event.preventDefault();
            event.stopPropagation();
        }
    }, true);

    // ============================================================================================
    // 
    // ============================================================================================

    function initializeElement(element)
    {
        var editor = element.dataset.editor;

        if (element.tagName == "FORM")
            new EditForm(element);
        else if (editor.indexOf("form") == 0)
        {
            var str = editor.substring(editor.indexOf(";") + 1);
            element.onclick = function(e) {dialog.setFormById(str); e.preventDefault();};
        }
        else
        {
            var handle = new EditHandle(element);

            switch (handle.editableProperty)
            {
                case "EditName":
                    handle.singleLine       = true;
                    handle.characterLimit   = 20;
                    break;

                case "EditShortBiography":
                    handle.characterLimit = 256;
                    break;

                case "EditStatus":
                    break;
            }
        }
    }

    window.addEventListener("load", function()
    {
        $("*[data-editor]").each(function()
        {
            initializeElement(this)
        });

        requestAnimationFrame(Editor.refreshHandles);
    }, false);

    document.body.appendChild(root);

    var isRefreshing = false;

    function refreshHandlesFinish()
    {
        var list    = EditHandle.handles;
        var handle  = null;

        for (var i in list)
            list[i].resetPosition();

        isRefreshing = false;
    }

    function refreshHandles()
    {
        if (isRefreshing)
            return;
            
        requestAnimationFrame(refreshHandlesFinish);
        isRefreshing = true;
    }

    window.onresize = function() {refreshHandles()};

    return {
        setup: function(_controller, _parameters)
        {
            controller          = _controller;
            defaultParameters   = _parameters;
        },
        onEditFormResponse: function(formId, _responseType, _callback)
        {
            EditForm.callbacks[formId] = {responseType: _responseType, fn: _callback};
        },
        refreshHandles: refreshHandles
    };
})();