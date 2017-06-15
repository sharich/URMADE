(function()
{
    var getDescendant, getDescendants;

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

    var playerContainer = document.getElementById("urmade-player");
    var amsPlayer       = null;
    var playlists       = document.querySelectorAll(".playlist");
    var currentPlaylist = null;
    var currentQueue    = null;
    var currentIndex    = -1;
    var currentItem     = null;
    var currentTimeline = null;
    var currentToggle   = null;

    // ==========================================================================================================
    // 
    // ==========================================================================================================

    function favorite(event)
    {
        var btn     = event.target;
        var request = new XMLHttpRequest();

        request.onloadend = function(response)
        {
            var result = JSON.parse(response.currentTarget.response);

            if (result.FanCount < 0)
                return;

            btn.className = result.Active ? btn.className + " favorited" : btn.className.replace(" favorited", "");
            btn.parentNode.children[0].innerHTML = result.FanCount;
        }

        request.open("POST", btn.dataset.post);
        request.send(null);
    }

    // ==========================================================================================================
    // Media Player
    // ==========================================================================================================

    function playItem(item, attachment)
    {
        if (item == null || item.dataset == null || item.dataset.mediaurl == null || item.dataset.mediaurl == "")
            return;

        if (currentItem != item)
        {
            var url = item.dataset.mediaurl.replace(/http:|https:/, "");
            amsPlayer.src([{src: url, type: "application/vnd.ms-sstr+xml"}]);

            if (currentTimeline != null)
                currentTimeline.style.width = "0";

            if (currentToggle != null)
                currentToggle.className = currentToggle.className.replace("media-stop", "media-play");

            currentItem     = item;
            currentToggle   = item.children.length > 0 ? getDescendant(item, ".media-toggle") : item;

            if (attachment)
            {
                if (playerContainer.parentNode != attachment)
                {
                    playerContainer.className = "";
                    attachment.appendChild(playerContainer);
                }
            }
            else if (item.dataset.mediatype == "Video")
            {
                playerContainer.className = "";
                item.parentNode.insertBefore(playerContainer, item);
            }
            else
            {
                playerContainer.className   = "urmade-player-hidden";
                currentTimeline             = getDescendant(item, ".media-timeline > div");

                document.body.appendChild(playerContainer);
            }

            if (currentToggle != null)
                currentToggle.className = currentToggle.className.replace("media-play", "media-stop");
        }
        else
        {
            if (amsPlayer.paused())
            {
                amsPlayer.play();

                if (currentToggle != null)
                    currentToggle.className = currentToggle.className.replace("media-play", "media-stop");
            }
            else
            {
                amsPlayer.pause();

                if (currentToggle != null)
                    currentToggle.className = currentToggle.className.replace("media-stop", "media-play");
            }
        }
    }

    function playSingle(item, attachment)
    {
        currentPlaylist = null;
        playItem(item, attachment);
    }

    function queuePlaylist(playlist, start)
    {
        currentPlaylist = playlist;

        if (!start)
            start = 0;

        if (currentPlaylist != null)
        {
            currentItem     = null;
            currentQueue    = getDescendants(playlist, ".media-item");
            currentIndex    = start;
            playItem(currentQueue[currentIndex]);
        }
        else
        {
            currentQueue = null;
            currentIndex = -1;
        }
    }

    // ==========================================================================================================
    // Playlist
    // ==========================================================================================================

    function Playlist(element)
    {
        this.element    = element;
        this.loader     = document.createElement("div");
        this.isLoading  = false;
        this.pending    = [];
        this.total      = 0;
        this.page       = 0;
        this.lastScroll = 0;
        this.threshold  = element.getBoundingClientRect().height * 1.25;
        this.attachment = getDescendant(element.parentElement, ".playlist-player");

        this.parse(element.dataset.playlist);

        this.loader.className = "loader";
        this.loader.innerHTML = "<span class=\"spinner\"></span>";
        this.element.appendChild(this.loader);
        
        var playlist = this;
        this.element.onscroll = function(event) {playlist.scroll(event);}

        this.sortSelect = getDescendant(element.parentElement, ".playlist-sort");
        if (this.sortSelect)
            this.sortSelect.onchange = function() {playlist.changeSorting();}

        this.nextPage();
    }

    Playlist.list = [];

    // ============================================================================================
    // Create
    // ============================================================================================

    Playlist.prototype.create = function(data, item)
    {
        var result  = document.createElement("div");
        var html    = "";

        result.className            = "media-item";
        result.dataset.mediaurl     = item.MediaURL;
        result.dataset.mediaid      = item.MediaId;
        result.dataset.mediaindex   = (++this.total);
        result.dataset.mediatype    = data.mediaType;

        html += "<div class=\"media-index\">" + this.total + ".</div>";

        if (item.AlbumArtURL)
        {
            html += "<div class=\"media-image\" style=\"background-image: url('" + item.AlbumArtURL + "');\">"
            html += (item.State == 3) ? "<button class=\"btn-image media-toggle media-play\"></button>" : "<span class=\"media-image-label\">Processing</span>";
            html += "</div>";
        }
        else
        {
            html += "<div class=\"media-image\"";
            html += "<button class=\"btn-image media-toggle media-play\"></button>";
            html += "</div>";
        }

        html += "<div class=\"media-details\">";
        html += "<span class=\"media-title\">" + item.Title + "</span>";
        html += "<span class=\"media-fans\">";
        html += "<span class=\"media-fanCount\">" + item.FanCount + "</span>";

        if (__loggedIn)
            html += "<button data-post=\"/" + data.Controller + "/Favorite/" + item.MediaId + "\" class=\"btn-image media-favorite" + (item.IsFavorited ? " favorited" : "") + "\"></button>";
        else
            html += "<a href=\"/Account/Register?RedirectUrl=" + window.location.pathname + "\" class=\"btn-image media-favorite\"></a>";

        html += "</span>";
        html += "<div class=\"media-artist\">";

        if (item.ArtistSlug != null && item.ArtistSlug != "")
            html += "<a href=\"/Artists/" + item.ArtistSlug + "\">" + item.ArtistName + "</a>";
        else if (data.CanEdit)
            html += "<span>Not Published</span>";

        if (data.CanEdit && item.CanEdit)
        {
            html += "<div class=\"media-actions\">";
            html += "<a href=\"/" + data.Controller + "/Edit/" + item.MediaId + "\" class=\"btn btn-primary btn-xs\">Edit</a>";
            html += "<a href=\"javascript: void(0);\" class=\"btn btn-danger btn-xs\" data-deleteaction=\"/" + data.Controller + "/Delete/" + item.MediaId + "\">Delete</a>";
            html += "</div>";
        }

        html += "</div></div>";
        html += "<div class=\"media-timeline\"><div></div></div>";
        html += "</div>";

        result.innerHTML    = html;
        var attachment      = this.attachment;

        var playButton      = getDescendant(result, ".media-toggle");
        playButton.onclick  = function() {playSingle(result, attachment);}

        var favoriteButton = getDescendant(result, ".media-favorite");
        if (favoriteButton)
            favoriteButton.onclick = favorite;
        else
        {
            var voteButton = getDescendant(result, ".media-vote");
            if (voteButton)
                voteButton.onclick = vote;
        }

        return result;
    }

    // ============================================================================================
    // Load
    // ============================================================================================

    Playlist.prototype.load = function(data)
    {
        var items = data.Items;

        if (items == null || items.length < 1)
        {
            --this.page;

            var playlist = this;
            setTimeout(function() {playlist.loader.style.display = "none"; requestAnimationFrame(function (){playlist.isLoading = false;});}, 5000);

            return;
        }

        this.isLoading              = false;
        this.loader.style.display   = "none";

        for (var i = 0, s = items.length; i < s; ++i)
            this.element.appendChild(this.create(data, items[i]));
    }

    // ============================================================================================
    // NextPage
    // ============================================================================================

    Playlist.prototype.nextPage = function()
    {
        if (this.isLoading)
            return;

        this.loader.style.display = "block";
        this.element.appendChild(this.loader);

        this.isLoading = true;

        var playlist    = this;
        var query       = {};

        query.Sort          = this.sort;
        query.UserId        = this.userid;
        query.ArtistId      = this.artistid;
        query.Type          = this.type;
        query.Name          = this.name;
        query.FavoritesOnly = this.favorites;
        query.Max           = this.max;
        query.Page          = this.page;

        jQuery.get(this.source, query, function(data) {playlist.load(data);});

        ++this.page;
    }

    // ============================================================================================
    // Clear
    // ============================================================================================

    Playlist.prototype.clear = function()
    {
        this.element.removeChild(this.loader);
        this.element.innerHTML = "";
        this.element.appendChild(this.loader);
        this.page   = 0;
        this.total  = 0;
    }

    // ============================================================================================
    // Scroll
    // ============================================================================================

    Playlist.prototype.scroll = function(event)
    {
        var current = this.element.scrollTop;

        if (current >= this.element.scrollHeight - this.threshold && this.lastScroll <= current)
            this.nextPage();

        this.lastScroll = current;
    }

    // ============================================================================================
    // ParseQuery
    // ============================================================================================

    Playlist.prototype.parse = function(query)
    {
        query = query.split(', ');

        this.source     = query[0];
        this.max        = query.length > 1 ? parseInt(query[1]) : 10;
        this.sort       = query.length > 2 ? parseInt(query[2]) : 0;
        this.userid     = query.length > 3 ? query[3] : null;
        this.artistid   = query.length > 4 ? parseInt(query[4]) : null;
        this.type       = query.length > 5 ? query[5] : null;
        this.name       = query.length > 6 ? query[6] : null;
        this.favorites  = query.length > 7 ? query[7] == "True" : null;
    }

    // ============================================================================================
    // ChangeSorting
    // ============================================================================================

    Playlist.prototype.changeSorting = function()
    {
        this.clear();
        this.sort = this.sortSelect.value;
        this.nextPage();
    }

    window.addEventListener("load", function()
    {
        var elements    = document.querySelectorAll(".playlist.dynamic");
        var list        = Playlist.list;

        for (var i = 0, s = elements.length; i < s; ++i)
            list.push(new Playlist(elements[i]));
    }, false);

    window.addEventListener("load", function()
    {
        var myOptions = {
	        "nativeControlsForTouch":   false,
	        controls:                   true,
	        autoplay:                   true,
	        width:                      "640",
	        height:                     "400"
        }

        amsPlayer = amp("azuremediaplayer", myOptions);

        amsPlayer.addEventListener("timeupdate", function()
        {
            if (currentTimeline)
                currentTimeline.style.width = (amsPlayer.currentTime() / amsPlayer.duration()) * 100.0 + "%";
        });

        amsPlayer.addEventListener("ended", function()
        {
            if (currentPlaylist != null && currentQueue != null)
            {
                if (++currentIndex >= currentQueue.length)
                    currentIndex = 0;

                playItem(currentQueue[currentIndex], currentTimeline == null);
            }
            else
            {
                if (currentTimeline)
                    currentTimeline.style.width = "0";

                if (currentToggle != null)
                    currentToggle.className = currentToggle.className.replace("media-stop", "media-play");

                currentItem     = null;
                currentTimeline = null;
                currentToggle   = null;
            }
        });

        var attachment  = document.getElementById("media-video-attachment");
        var player      = document.getElementById("azuremediaplayer");

        if (attachment != null && player != null)
            attachment.appendChild(player);

        for (var i = 0; i < playlists.length; ++i)
        {
            (function()
            {
                var items       = getDescendants(playlists[i], ".media-item");
                var queue       = getDescendant(playlists[i].parentNode, ".playlist-queueAll");
                var attachment  = getDescendant(playlists[i].parentNode, ".playlist-player");

                if (queue != null)
                {
                    (function()
                    {
                        var playlist    = playlists[i];
                        queue.onclick   = function() {queuePlaylist(playlist);}
                    })();
                }

                for (var j = 0; j < items.length; ++j)
                {
                    var button = getDescendant(items[j], ".media-toggle");

                    if (button != null)
                    {
                        (function()
                        {
                            var item        = items[j];
                            button.onclick  = function() {playSingle(item, attachment);}
                        })();
                    }
                }
            })();
        }

        var favoriteButtons = document.querySelectorAll(".media-favorite");
        if (favoriteButtons && favoriteButtons.length > 0)
        {
            for (var i = 0; i < favoriteButtons.length; ++i)
                favoriteButtons[i].onclick = favorite;
        }

        var contests = document.querySelectorAll(".contest .playlist");

        if (contests != null && contests.length > 0)
        {
            var voteButtons     = null;
            var alreadyVoted    = false;
            var token           = document.querySelector("input[name='__RequestVerificationToken']");
            var modalVote       = document.querySelector("#confirmVote");
            var modalVoteForm   = getDescendant(modalVote, "form");
            var currentVoteBtn  = null;

            function confirmVote(event)
            {
                event.preventDefault();

                var request     = new XMLHttpRequest();
                var formData    = new FormData(modalVoteForm);

                request.onloadend = function(response)
                {
                    var result  = JSON.parse(response.currentTarget.response);
                    var btn     = currentVoteBtn;

                    if (result.Success)
                    {
                        btn.parentNode.parentNode.parentNode.parentNode.className += " voted";

                        var html = "";
                        html += "<span class=\"media-fanCount\">" + result.Votes + "</span>";
                        html += "<span class=\"glyphicon glyphicon-ok-circle media-vote voted\"></span>";
                        btn.parentNode.innerHTML = html;
                    }
                }

                request.open("POST", modalVoteForm.action);
                request.send(formData);

                $(modalVote).modal("hide");
            }

            function vote(event)
            {
                event.preventDefault();

                modalVoteForm.action    = event.target.dataset.post;
                currentVoteBtn          = event.target;
                $(modalVote).modal();
            }

            if (modalVoteForm)
            {
                if (token)
                    modalVoteForm.appendChild(token.cloneNode());

                getDescendant(modalVoteForm, "input[type='submit']").onclick = confirmVote;
            }

            for (var i = 0; i < contests.length; ++i)
            {
                voteButtons     = getDescendants(contests[i], "button.media-vote");
                alreadyVoted    = getDescendant(contests[i], ".media-vote.voted") != null;

                if (voteButtons == null || voteButtons.length < 1)
                    continue;

                if (alreadyVoted)
                    contests[i].className += " voted";
                else
                    for (var j = 0; j < voteButtons.length; ++j)
                        voteButtons[j].onclick = vote;
            }
        }

        var slider = document.querySelector(".slides");
        if (slider)
        {
            (function()
            {
                var currentSlide = 0, intervalId = null;

                function nextSlide()
                {
                    var list = slider.children;

                    if (list.length < 1)
                        return;

                    if (currentSlide < list.length && currentSlide >= 0)
                        list[currentSlide].className = "";

                    if (++currentSlide >= list.length)
                        currentSlide = 0;

                    list[currentSlide].className = "active";
                }

                function startSlider()
                {
                    if (intervalId)
                        clearInterval(intervalId);

                    currentSlide    = -1;
                    intervalId      = setInterval(nextSlide, 5000);

                    nextSlide();
                }

                startSlider();
            })();
        }
}, false);
})();