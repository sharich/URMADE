<!doctype html>
<html <?php language_attributes(); ?>>
  	<meta charset="<?php bloginfo( 'charset' ); ?>" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
	<title><?php wp_title(); ?></title>
	
	<script src="/Scripts/jquery-3.1.1.js"></script>
	<script src="/Scripts/bootstrap.js"></script>
	<script src="/Scripts/jquery.unobtrusive-ajax.js"></script>
	<link href='/Content/bootstrap.css?v=1' type='text/css' rel='stylesheet' />
	<link href='/Content/dragula.min.css?v=1' type='text/css' rel='stylesheet' />
	<link href='/Content/site.less?v=1' type='text/css' rel='stylesheet' />
    <script type='text/javascript' src='/Scripts/modernizr-2.8.3.js?v=3'></script>
	
  	<?php wp_head(); ?>
</head>

<body>
    <div class="navbar navbar-inverse navbar-fixed-top">
        <div class="container">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".topbar-nav">
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <a class="navbar-brand" href="/">UR Made</a>
            </div>
            <div class="topbar-nav navbar-collapse collapse">
                <ul class="nav navbar-nav">
                </ul>
				
			<form action="/Account/LogOff" class="navbar-right" id="logoutForm" method="post">
			</form>
            </div>
        </div>
		
		<script src="/wp/wp-content/themes/urmade/header.js"></script>
    </div>
    <style>
        .header {
    background-color: black;
    border-bottom:none;
}

.header .navbar-brand {
    width: 310px;
    height: 100px;
    margin-top: 0px;
    margin-bottom: 6px;
}
.header-nav .navbar-brand {
    margin-top:0px;
}
.header-nav {
    margin-top:30px;
}
.header a {
    font-size: 1.3em;
}
    </style>
    <div class="header navbar-inverse container body-content">
            <div class="navbar-header">
                <a class="navbar-brand" href="/">UR Made</a>
            </div>
            <div class="header-nav navbar-collapse collapse">
                <ul class="nav navbar-nav">
                    <li><a href="/">Home</a></li>
                    <li><a href="/wp/About">About Us</a></li>
                    <li><a href="/Song">Music</a></li>
					<li><a href="/Video">Video</a></li>
                    <li><a href="/Home/Rankings">Rankings</a></li>
                    <li><a href="/Contests/Vote">Contests</a></li>
                </ul>
            </div>
    </div>
        <div class="container body-content">