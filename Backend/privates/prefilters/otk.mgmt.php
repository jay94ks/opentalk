<?php

/*
 * MGMT 요청 필터입니다.
 *  */

$Authorization = $_OTK['AUTH'];

if ($Authorization['METHOD'] != 'BEARER') {
    throw new OtkHttpUnauthorizedException();
}

if ($Authorization['TOKEN'] == 'DEBUG') 
{
    if (!defined('OTK_DEBUG') || !OTK_DEBUG) {
        throw new OtkHttpUnauthorizedException();
    }
}

else
{
    /*
     * 1차적으로, 설정파일에 하드코딩된 토큰과 일치하는지 검사합니다.
     * 2차적으로, 서버 등록정보의 access-token과 일치하는 항목이 있는지 검사합니다.
     * */
    if (OTK_MGMT_HARDCORDED_TOKEN != $Authorization['TOKEN']) {
        $Token = otkSqlEscape($Authorization['TOKEN']);
        $Validity = otkSqlQuerySingle(
            "SELECT COUNT(*) FROM `{$_OTK_TABLES['TEXTILE_SERVERS']}` ".
            "WHERE `access_token` = '{$Token}' AND `is_online` = 'Y'");
            
        if (!$Validity) {
            throw new OtkHttpUnauthorizedException();
        }
    }
}