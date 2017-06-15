(function()
{
    function renderSearchList(list, label, moreLink)
    {
        if (list == null || list.length < 1)
            return "";

        var html = "";

        html += "<li><div>" + label + "<a class=\"search-more\" href=\"" + moreLink + "\">More</a></div><ul>";

        for (var i = 0; i < list.length; ++i)
        {
            if (list[i].Url)
                html += "<li><a href=\"" + list[i].Url + "\">" + list[i].Label + "</a></li>";
            else
                html += "<li><a href=\"" + moreLink + "&autoPlay=" + i + "\">" + list[i].Label + "</a></li>";
        }

        html += "</ul></li>";

        return html;
    }

    function renderSearchResults(data)
    {
        var results = document.querySelector(".quick-search-results");
        var match   = document.querySelector(".quick-search input[name='match']").value;

        if (!results)
            return;

        var html= "";

        if (data.Artists || data.Songs || data.Videos || data.Contests)
        {
            html += renderSearchList(data.Artists, "Artists", "/Artist/?Name=" + match);
            html += renderSearchList(data.Songs, "Songs", "/Song/?Name=" + match);
            html += renderSearchList(data.Videos, "Videos", "/Video/?Name=" + match);
            html += renderSearchList(data.Contests, "Contests", "/Contest/ViewAll/?Name=" + match);
        }
        else
            html += "No results found.";

        results.innerHTML = html;
    }

    var previousSearch = null;

    function quickSearch(event)
    {
        event.preventDefault();

        var searchText      = $(event.target).find("input[name='match']")[0];
        var searchResults   = $(event.target).find(".quick-search-results")[0];

        if (searchText.value == previousSearch)
        {
            searchText.value        = "";
            searchResults.innerHTML = "";
            previousSearch          = "";

            event.target.className = event.target.className.replace(" active", "");
        }
        else if (searchText.value.trim() != "")
        {
            $.post("/Home/Search", $(event.target).serialize(), renderSearchResults, "json");
            previousSearch = searchText.value;

            event.target.className += " active";
        }

        return false;
    }

    function changeSearch(event)
    {
        if (event.target.value != previousSearch)
            event.target.parentNode.className = event.target.parentNode.className.replace(" active", "");
        else
            event.target.parentNode.className += " active";
    }

    window.addEventListener("load", function()
    {
        var quickForms = document.querySelectorAll(".quick-search");

        if (quickForms != null && quickForms.length > 0)
        {
            for (var i = 0; i < quickForms.length; ++i)
            {
                quickForms[i].onsubmit = quickSearch;
                var searchText = $(quickForms[i]).find("input[name='match']")[0];
                searchText.onkeydown = changeSearch;
            }
        }
    });
})();