(function()
{
    // ============================================================================================
    // Utility
    // ============================================================================================

    var getDescendant   = null;
    var getDescendants  = null;

    if (document.body.currentStyle || navigator.appName == "Netscape")
    {
        getDescendant   = function(element, selector) {return element.querySelector(selector);}
        getDescendants  = function(element, selector) {return element.querySelectorAll(selector);}
    }
    else
    {
        getDescendant   = function(element, selector) {return element.querySelector(":scope " + selector);}
        getDescendants  = function(element, selector) {return element.querySelectorAll(":scope " + selector);}
    }

    function isArrayEmpty(array)
    {
        return array == null || array.length <= 0;
    }

    // ============================================================================================
    // Editors (old)
    // ============================================================================================

    var editList    = document.querySelectorAll(".edit");
    var edit        = null;

    for (var i = 0; i < editList.length; ++i)
    {
        edit = editList[i];

        if (edit.className.indexOf("file-upload-selector") >= 0)
        {
            (function()
            {
                var container   = editList[i];
                var fileName    = getDescendant(container, ".media-file-title");
                var fileInput   = getDescendant(container, "input[type='file']");
                var rect        = container.getBoundingClientRect();
                var prevHover   = false;
                var minX        = rect.left + rect.width    * 0.5 - 45;
                var maxX        = minX + 90;
                var minY        = rect.top  + rect.height   * 0.5 - 8;
                var maxY        = minY + 32;

                fileInput.onchange = function(e)
                {
                    if (fileInput.files.length > 0)
                        fileName.innerHTML = fileInput.files[0].name;
                }

                function mousemove(e)
                {
                    var isHovered = e.clientX > minX &&
                                    e.clientX < maxX &&
                                    e.clientY > minY &&
                                    e.clientY < maxY;

                    if (prevHover != isHovered)
                    {
                        if (isHovered)
                            container.className += " hover";
                        else
                            container.className = container.className.replace(" hover", "");

                        prevHover = isHovered;
                    }
                }

                fileInput.onmousemove   = mousemove;
                fileInput.onmouseleave  = mousemove;
            })();
        }
        if (edit.className.indexOf("image-selector") >= 0)
        {
            (function()
            {
                var container   = editList[i];
                var btnDelete   = getDescendant(container, "button[data-btnaction='delete']");
                var inputFile   = getDescendant(container, "input[type='file']");
                var inputDelete = getDescendant(container, "input[name='DeleteAlbumArt']");
                var rect        = container.getBoundingClientRect();
                var prevHover   = false;
                var minX        = rect.left + rect.width    * 0.5 - 45;
                var maxX        = minX + 90;
                var minY        = rect.top  + rect.height   * 0.5 - 8;
                var maxY        = minY + 32;

                function mousemove(e)
                {
                    var isHovered = e.clientX > minX &&
                                    e.clientX < maxX &&
                                    e.clientY > minY &&
                                    e.clientY < maxY;

                    if (prevHover != isHovered)
                    {
                        if (isHovered)
                            container.className += " hover";
                        else
                            container.className = container.className.replace(" hover", "");

                        prevHover = isHovered;
                    }
                }

                inputFile.onmousemove   = mousemove;
                inputFile.onmouseleave  = mousemove;

                function updateImage(e)
                {
                    container.style     = "background-image: url('" + e.target.result + "');";
                    container.className = container.className.replace(" empty", "");
                }

                btnDelete.onclick = function(e)
                {
                    inputFile.value     = "";
                    container.style     = "";
                    inputDelete.value   = "true";

                    if (container.className.indexOf("empty") < 0)
                    {
                        container.style     = "";
                        container.className += " empty";
                    }

                    e.preventDefault();
                }

                inputFile.onchange = function(e)
                {
                    if (inputFile.files.length > 0)
                    {
                        var reader      = new FileReader();
                        reader.onload   = updateImage;
                        reader.readAsDataURL(inputFile.files[0]);

                        inputDelete.value = "false";
                    }
                }
            })();
        }
    }

    // ============================================================================================
    // Delete Buttons (old)
    // ============================================================================================

    var btnsDelete = document.querySelectorAll("a[data-deleteaction]");
    if (!isArrayEmpty(btnsDelete))
    {
        function showDeleteModal(event)
        {
            var btn = event.target;
            $("#confirmDelete form")[0].action = btn.dataset.deleteaction;
            $("#confirmDelete").modal();
        }

        for (var i = 0; i < btnsDelete.length; ++i)
            btnsDelete[i].onclick = showDeleteModal;
    }

    // ============================================================================================
    // Request Verification Token (old)
    // ============================================================================================

    var modalDelete = document.querySelector("#confirmDelete form");
    var token       = document.querySelector("input[name='__RequestVerificationToken']");

    if (modalDelete && token)
        modalDelete.appendChild(token.cloneNode());

    // ============================================================================================
    // Editors (New)
    // ============================================================================================

    var listEditor = document.querySelectorAll("*[data-editor]");
    if (!isArrayEmpty(listEditor))
    {
        var editor;

        function Selector(editor)
        {
            var rect = editor.getBoundingClientRect();

            this.container      = editor;
            this.inputFile      = getDescendant(editor, "input[type='file']");
            this.buttonDelete   = getDescendant(editor, "button[data-btnaction='delete']");
            this.preview        = null;
            this.hover          = null;
            this.minX           = rect.left + rect.width    * 0.5 - 45;
            this.minY           = minX + 90;
            this.maxX           = rect.top  + rect.height   * 0.5 - 8;
            this.maxY           = minY + 32;
        }

        for (var i = 0; i < listEditor.length; ++i)
        {
            editor = listEditor[i];
            switch (editor.dataset.editor)
            {
                case "image":   new Selector(listEditor[i]); break;
                case "song":    new Selector(listEditor[i]); break;
                case "video":   new Selector(listEditor[i]); break;
            } 
        }
    }

    // ============================================================================================
    // Delete Buttons (new)
    // ============================================================================================

    var listDelete = document.querySelectorAll("a[data-delete]");
    if (!isArrayEmpty(listDelete))
    {
        var formDelete = document.querySelector("#confirmDelete form");

        function showDeleteModal(event)
        {
            formDelete.action = event.target.dataset.delete;
            $(formDelete).modal();
        }

        for (var i = 0; i < listDelete.length; ++i)
            listDelete[i].onclick = showDeleteModal;
    }

    // ============================================================================================
    // Request Verification Token (new)
    // ============================================================================================

    var listToken = document.querySelectorAll("form[data-requirestokens]");
    if (!isArrayEmpty(listToken))
    {
        var token = document.querySelector("input[name='__RequestVerificationToken']");
        if (token)
            for (var i = 0; i < listToken.length; ++i)
                listToken[i].appendChild(token.cloneNode());
    }

    // ============================================================================================
    // Loader
    // ============================================================================================

    var listLoader = document.querySelectorAll("button[data-loader]");
    if (!isArrayEmpty(listLoader))
    {
        var editForm = document.getElementById("editForm");

        function createLoader(event)
        {
            var t       = event.currentTarget;
            var loader  = document.createElement("div");
            var html    = "";

            editForm.style = "display: none !important;";

            html += "<div>" + t.dataset.loader + "</div>";
            html += "<span class=\"spinner\"></span>";
            loader.className = "loader";
            loader.innerHTML = html;

            t.parentNode.insertBefore(loader, t);
            t.parentNode.removeChild(t);

            editForm.submit();
            event.stopPropogation();
        }

        for (var i = 0; i < listLoader.length; ++i)
            listLoader[i].onclick = createLoader;
    }
})();