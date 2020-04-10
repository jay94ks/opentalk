<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<!--
Design by TEMPLATED
http://templated.co
Released for free under the Creative Commons Attribution License

Name       : Lounging 
Description: A two-column, fixed-width design with dark color scheme.
Version    : 1.0
Released   : 20130607
----
Modified for OpenTalk Example site at 20200410.
-->
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>OpenTalk</title>
<meta name="keywords" content="" />
<meta name="description" content="" />
<link href="http://fonts.googleapis.com/css?family=Source+Sans+Pro:200,300,400,600,700,900" rel="stylesheet" />
<link href="<?='/'. trim("{$_OTK['ROOT_PATH']}/templates/lounging/default.css", '/')?>" rel="stylesheet" type="text/css" media="all" />
</head>
<body>
<div id="header-wrapper">
    <div id="header-wrapper2">
        <div id="header" class="container">
            <div id="logo">
                <h1><a href="#">Open Talk</a></h1>
            </div>
            <div id="menu">
                <ul>
                    <?php
                    $MenuIndex = 0;
                    foreach ($_OTK_WEB['MENUS'] as $Title => $Href) {
                        $MenuClass = ($_OTK_WEB['SHOWING'] == $Title ? " class=\"current_page_item\"" : "");
                        $Title = htmlspecialchars($Title);
                        ?><li<?=$MenuClass?>>
                            <a href="<?=$Href?>" accesskey="<?=(++$MenuIndex)?>" title="<?=$Title?>"><?=$Title?></a>
                        </li><?php
                    }
                    ?>
                </ul>
            </div>
        </div>
    </div>
</div>
<div id="wrapper">
    <div id="page" class="container">
        <div id="content">
            <?=$_OTK_WEB['CONTENTS']?>
        </div>
        <div id="sidebar">
            <?=$_OTK_WEB['SUBCONTENTS']?>
        </div>
    </div>
</div>
<div id="portfolio-wrapper">
    <div id="portfolio" class="container">
        <div class="title">
            <h2>Suspendisse lacus turpis</h2>
            <span class="byline">Lorem ipsum dolor sit amet, consectetuer adipiscing elit</span> </div>
        <div id="column1">
            <p>Etiam non felis. Donec ut ante. In id eros. Suspendisse lacus turpis, cursus egestas at sem. Mauris quam enim, molestie.</p>
            <a href="#" class="button">Read More</a> </div>
        <div id="column2">
            <p>Etiam non felis. Donec ut ante. In id eros. Suspendisse lacus turpis, cursus egestas at sem. Mauris quam enim, molestie.</p>
            <a href="#" class="button">Read More</a> </div>
        <div id="column3">
            <p>Etiam non felis. Donec ut ante. In id eros. Suspendisse lacus turpis, cursus egestas at sem. Mauris quam enim, molestie.</p>
            <a href="#" class="button">Read More</a> </div>
        <div id="column4">
            <p>Etiam non felis. Donec ut ante. In id eros. Suspendisse lacus turpis, cursus egestas at sem. Mauris quam enim, molestie.</p>
            <a href="#" class="button">Read More</a> </div>
    </div>
</div>
<div id="copyright" class="container">
	<p>&copy; ~~~. All rights reserved. | Photos by <a href="http://fotogrph.com/">Fotogrph</a> | Design by <a href="http://templated.co" rel="nofollow">TEMPLATED</a>.</p>
</div>
</body>
</html>
