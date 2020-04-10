<?php

include_once (__DIR__ . '/../privates/opentalk.php');

$GLOBALS['_OTK_WEB'] = [
    'STYLESHEETS' => [],
    'SCRIPTS' => [],
    'TITLE' => [],
    'MENUS' => [
        "Home" => "/",
        "History" => "/history.php",
        "About" => "/about.php",
        "Member" => "/member.php"
    ],
    'SHOWING' => 'Home',
    'CONTENTS' => '',
    'SUBCONTENTS' => ''
];

function otkWebTemplated(callable $fn, $mode = 'index', $subFn = null) {
    global $_OTK_WEB, $_OTK, $_OTK_STATE, $_OTK_TABLES;
    $Template = OTK_WEB_TEMPLATE;
    
    ob_start();
    call_user_func_array($fn, []);
    $_OTK_WEB['CONTENTS'] = ob_get_contents();
    ob_end_clean();
    
    if ($subFn) {
        ob_start();
        call_user_func_array($subFn, []);
        $_OTK_WEB['SUBCONTENTS'] = ob_get_contents();
        ob_end_clean();
    }
    
    $Path = OTK_ROOT . "/templates/{$Template}/{$mode}.tpl.php";
    if (file_exists($Path)) {
        include ($Path);
    } else {
        throw new OtkHttpException(503, 'Service Unavailable');
    }
}