<?php

/*
 * OpenTalk Textile 프로토콜 서버를 등록하는 API입니다.
 * */

function onRegisterServer($UniqueId, $PortNumber) {
    global $_OTK_TABLES;
    //$_SERVER['REMOTE_ADDR'];
    
    $AccessToken = otkSqlGenUniqueString('TEXTILE', 30);
    
    $UniqueId = otkSqlEscape($UniqueId);
    $PortNumber = otkSqlEscape($PortNumber);
    
    $Insertion = otkSqlQuery("INSERT INTO `{$_OTK_TABLES['TEXTILE_SERVERS']}` ".
        "(`unique_id`, `access_token`, `registered_at`, `boot_time`, `remote_address`, `primary_port`) VALUES ".
        "('{$UniqueId}', '{$AccessToken}', NOW(), NOW(), '{$_SERVER['REMOTE_ADDR']}', '{$PortNumber}')");
    
    if (!$Insertion) {
        /*
         * 이미 등록되어 있는 경우,
         * 테이블에 엑세스 토큰 필드를 업데이트 합니다.
         * */
        otkSqlQuery("UPDATE `{$_OTK_TABLES['TEXTILE_SERVERS']}` " .
            "SET `access_token` = '{$AccessToken}', `boot_time` = NOW(), ".
                "`remote_address` = '{$_SERVER['REMOTE_ADDR']}', " . 
                "`primary_port` = '{$PortNumber}', `is_online` = 'Y' " . 
            "WHERE `unique_id` = '{$UniqueId}' LIMIT 1");
    }
    
    return [ 'token' => $AccessToken ];
}

function onUnregisterServer() {
    global $_OTK, $_OTK_TABLES;
    
    $AccessToken = otkSqlEscape($_OTK['AUTH']['TOKEN']);
    if (!otkSqlQuery("UPDATE `{$_OTK_TABLES['TEXTILE_SERVERS']}` " .
        "SET `access_token` = '', `remote_address` = '', ".
            "`primary_port` = '0', `is_online` = 'N' " . 
        "WHERE `access_token` = '{$AccessToken}' LIMIT 1"))
    {
        otkStatusCode(404);
    }
    
    return [];
}

switch ($_OTK['METHOD']) {
    case 'POST':
        $handler = "onRegisterServer";
        $args = [$_POST['unique_id'], intval($_POST['port'])];
        break;
    
    case 'DELETE':
        $handler = "onUnregisterServer";
        $args = [];
        break;
    
    default:
        throw new OtkHttpException(501, null);
}

return call_user_func_array($handler, $args);