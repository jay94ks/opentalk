<?php

/*
 * OpenTalk Textile 프로토콜 서버에 인증하기 위한 토큰을 발급받는 API입니다.
 * */

function onAuthorizeUser() {
    global $_OTK_TABLES;
    
    $Member = otkMemberGet($_POST['identifier']);
    $KeyData1 = $KeyData2 = "";
    
    if (isset($_POST['first_key']) && $_POST['first_key']) {
        $KeyData1 = trim($_POST['first_key']);
    }
    
    if (isset($_POST['second_key']) && $_POST['second_key']) {
        $KeyData2 = trim($_POST['second_key']);
    }
    
    if (!$Member) {
        /* 인증 정보가 유효하지 않음. */
        throw new OtkHttpException(404);
    }
    
    switch(strtolower($_POST['type'])){
        case 'generic':
        case 'password': // <-- 실수로 ㅠㅠ... 구현이 두개가 되어버렸음 !! 제길!!
            // Email/Password 로그인.
            if (!$KeyData1 || 
                !otkMemberCheckPassword($Member, $KeyData1))
            {
                /* 인증 정보가 유효하지 않음. */
                throw new OtkHttpException(400);
            }
            
            break;
        
        case 'indirect':
            // 자동 로그인.
            if (!$KeyData1 || !$KeyData2) {
                /* 인증 정보가 유효하지 않음. */
                throw new OtkHttpException(400);
            } else {
                $KeyData1 = otkSqlEscape($KeyData1);
                $KeyData2 = otkSqlEscape($KeyData2);
                
                $MemberNo = otkSqlQuerySingle(
                    "SELECT `member_no` FROM `{$_OTK_TABLES['MEMBER_RESTORATIONS']}` " .
                    "WHERE `authorization` = '{$KeyData1}' AND `token` = '{$KeyData2}' AND ".
                          "`is_active_key` = 'Y' LIMIT 1");
                    
                if ($MemberNo != $Member['no']) {
                    /* 복원 토큰이 만료됨. */
                    throw new OtkHttpException(403);
                }
                
                // 복원 키를 삭제합니다.
                otkSqlQuery("DELETE FROM `{$_OTK_TABLES['MEMBER_RESTORATIONS']}` " .
                    "WHERE `authorization` = '{$KeyData1}' AND `token` = '{$KeyData2}'");
            }
            
            break;
            
        default:
            throw new OtkHttpException(501);
            break;
    }
    
    // Authorization 키는 HTTP REST 요청 토큰으로,
    // TextileToken 키는 Textile 서버 인증 토큰으로 사용됩니다.
    
    $Authorization = otkSqlGenUniqueString('MEMAUTH', 64);
    $TextileToken = otkSqlGenUniqueString('TEXTOKEN', 64);
    
    /*
     * 인증 정보를 생성합니다.
     * authorization 필드는 PHP 코드에서 해당 사용자 세션을 식별하기 위한 키로 사용하고,
     * textile_token 필드는 Textile 서버, 즉 C# 코드 측에서 사용자 세션을 식별하기 위한 키로 사용합니다.
     * 
     * textile_server_id 필드, authorized_at 필드와 
     * unauthorized_at 필드는 Textile 서버쪽에서 채웁니다.
     * */
    otkSqlQuery("INSERT INTO `{$_OTK_TABLES['MEMBER_AUTHORIZATIONS']}`(`authorization`, `textile_token`, ". 
        "`member_no`) VALUES ('{$Authorization}', '{$TextileToken}', '{$Member['no']}')");
    
    /* 
     * 자동 로그인용 검증값을 생성해둡니다. 자동 로그인 Hit되면 삭제처리됩니다.
     * is_active_key값은 처음엔 N로 설정 (사용불가)되어 있다가, 
     * 해당 사용자가 Textile 서버 인증에 성공하고, Textile 서버와 연결이 끊어질때 Y로 변경됩니다.
     * 결국 양쪽 인증을 모두 받아야 자동 로그인 토큰이 유효해집니다.
     *  */
    otkSqlQuery("INSERT INTO `{$_OTK_TABLES['MEMBER_RESTORATIONS']}`(`authorization`, `token`, ". 
        "`member_no`, `is_active_key`) VALUES ('{$Authorization}', '{$TextileToken}', '{$Member['no']}', 'N')");
    
    
    return [
        "identifier" => $_POST['identifier'],
        'type' => $_POST['identifier'],
        'first_key' => $Authorization,
        'second_key' => $TextileToken
    ];
}

function onGetUserProfile() {
    
}

function onSetUserProfile() {
    
}

function onDeauthorizeUser() {
    
}

switch ($_OTK['METHOD']) {
    case 'POST':
        $handler = "onAuthorizeUser";
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

return call_user_func_array($handler, []);