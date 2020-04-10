<?php

include_once (__DIR__ . '/../privates/opentalk.php');

// 요청 경로를 otk 디렉터리 내 경로로 변환합니다.
$Path = trim(substr($_OTK['PATH'], strlen($_OTK['ENTRY_PATH'])), '/');
$PathComponents = explode('/', $Path);

// 0 보다 커야 합니다.
if (count ($PathComponents) > 0) {
    // OTK 공통 필터를 실행합니다.
    otkInvokePrefilter('otk');
    if ($PathComponents[0] == 'mgmt') {
        // 관리 계열 요청일 때, 관리 요청 권한 필터를 실행합니다.
        // (인증 토큰이 셋팅되어 있어야 예외없이 진행됨)
        otkInvokePrefilter('otk.mgmt');
    }
    else {
        // 관리 계열 요청이 아니라면, 사용자 필터를 실행합니다.
        // (인증 토큰이 잘못되었을 때에만 예외가 발생하며, 없는 경우는 예외가 아닙니다)
        otkInvokePrefilter('otk.user');
    }
}

unset($PathComponents);

// 일단, 기본적인 응답코드는 404이며, 빈 객체를 반환합니다.
otkStatusCode(404);
otkReplaceOutputs("{}");

// 컨텐츠 타입을 application/json으로 설정합니다.
otkSetHeader("Content-Type", "application/json; charset=UTF-8");

// 지정된 이름과 일치하는 이름을 찾습니다.
if (file_exists($Target = __DIR__ . "/{$Path}.php")) {
    function otkBreakScope($Target) {
        global $_OTK, $_OTK_STATE;
        global $_OTK_TABLES;
        
        /*
         * 실행 결과가 배열이면 그대로 리턴,
         * 아니라면 빈 배열을 리턴합니다.
         */
        $retVal = include_once ($Target);
        if (is_array($retVal)) {
            return $retVal;
        }
        
        return [];
    }
    
    otkStatusCode(200);
    otkReplaceOutputs(json_encode(
        otkBreakScope($Target), 
        JSON_UNESCAPED_UNICODE | 
        JSON_UNESCAPED_SLASHES));
}
