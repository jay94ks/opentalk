<?php

define ('OTK_DEBUG', true);
define ('OTK_TABLE_PREFIX', 'otk_');
define ('OTK_MD5_SALT', "_SALT_OPENTALK");
define ('OTK_WEB_TEMPLATE', 'lounging');

define ('OTK_MGMT_HARDCORDED_TOKEN', 'this-is-server-side-password');

$GLOBALS['_OTK_DB'] = [
    'MASTER' => [
        'HOST' => '127.0.0.1',
        'USER' => 'root',
        'PASS' => '',
        'PORT' => 3306,
        'SCHM' => 'opentalk'
    ],
    
    'SLAVES' => [ ]
];

$GLOBALS['_OTK_TABLES'] = [
    'UNIQUE_STRINGS' =>     OTK_TABLE_PREFIX . 'unique_strings',
    'TEXTILE_SERVERS' =>    OTK_TABLE_PREFIX . 'textile_servers',
    'MEMBER_TABLE' =>       OTK_TABLE_PREFIX . 'member_table'
];