(function()
{
	var request = new XMLHttpRequest();
	
	request.open("GET", "/User/Header");
	request.send(null);
	
    function dropdown(name)
    {
        var html = "";

        html += "<li class='dropdown'>";
        html += "<a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-expanded='true' aria-haspopup='true'>";
        html += name + "<span class='caret'></span>";
        html += "</a>";
        html += "<ul class='dropdown-menu'>"

        return html;
    }

	function loadHeader(header)
	{
		var html = "";
		
		if (header.Account)
		{
            html += dropdown("Account");

            for (var i = 0; i < header.Account.length; ++i)
            {
                html += "<li>";

                switch (header.Account[i])
                {
                    case "Profile":
                        html += "<a href='/User/Details'>Profile</a>";
                    break;
                    case "My Songs":
                        html += "<a href='/Song/MySongs'>My Songs</a>";
                        break;
                    case "My Videos":
                        html += "<a href='/Video/MyVideos'>My Videos</a>";
                        break;
                }

                html += "</li>";
            }

            html += "<li>";
            html += "<span class='navbar-heading'>Websites</span>";
            html += "</li>";

            for (var i = 0; i < header.Websites.length; ++i)
            {
                html += "<li>";
                html += "<a href='/Artist/Website/" + header.Websites[i].Value + "'>";
                html += header.Websites[i].Key + "</a>";
                html += "</li>";
            }

            html += "</ul>";
		}
		
		if (header.Manage)
		{
            html += dropdown("Manage");

            for (var i = 0; i < header.Manage.length; ++i)
            {
                html += "<li>";

                switch (header.Manage[i])
                {
                    case "Users":           html += "<a href='/User'>Profile</a>";                  break;
                    case "Artists":         html += "<a href='/Artist'>Artists</a>";                break;
                    case "User Groups":     html += "<a href='/UserGroup'>User Groups</a>";         break;
                    case "Select Options":  html += "<a href='/SelectOptions'>Select Options</a>";  break;
                    case "Contests":        html += "<a href='/Contest'>Contests</a>";              break;
                }

                html += "</li>";
            }
            
            html += "</ul>";
		}
		
		document.querySelector(".topbar-nav .nav ").innerHTML = html;
		html = "";
		
		if (header.Username)
		{
			html += "<ul class='nav navbar-nav navbar-right'>";
			html += "<li><a href='/User/Details/' title='Manage'>Hello " + header.Username + "!</a></li>";

            if (header.Token)
            {
                html += "<input name='__RequestVerificationToken' type='hidden' value='" + header.Token + "'>";
			    html += "<li><a href='#' onclick=\"javascript:document.getElementById('logoutForm').submit()\">Log off</a></li>";
            }
			
            html += "</ul>";
			
			document.querySelector("#logoutForm").innerHTML = html;
		}
		
		$('.dropdown-toggle').dropdown();
	}
	
	function loadEmpty()
	{
        var html = "";

		html += '<ul class="nav navbar-nav navbar-right">';
        html += '<li><a href="/Account/Register" id="registerLink">Register</a></li>';
        html += '<li><a href="/Account/Login" id="loginLink">Log in</a></li>';
        html += '</ul>';

        document.querySelector(".topbar-nav").innerHTML += html;
	}
	
	request.onloadend = function(response)
	{
        try
        {
		    var header = JSON.parse(response.currentTarget.response);
        }
        catch (e)
        {
            loadEmpty();
            return;
        }

		if (!header || !header.Username || header.ErrorMessage)
			loadEmpty();
		else
			loadHeader(header);
	}
})();