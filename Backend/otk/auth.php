<?php

/*
 * OpenTalk Textile 프로토콜 서버에 인증하기 위한 토큰을 발급받는 API입니다.
 * */

switch ($_OTK['METHOD']) {
    case 'POST':
        $handler = "onAuthorizeUser";
        $args = [$_POST['unique_id'], intval($_POST['port'])];
        break;
    
    case 'GET':
        $handler = "onGetUserProfile";
        break;
    
    case 'PUT':
        $handler = "onSetUserProfile";
        break;
    
    case 'DELETE':
        $handler = "onDeauthorizeUser";
        $args = [];
        break;
    
    default:
        throw new OtkHttpException(501, null);
}

return call_user_func_array($handler, $args);