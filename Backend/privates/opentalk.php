<?php

/*
 * Open Talk 공통 헤더 파일입니다.
 * --
 * 그누보드와 연동하려면, 직접 Include 하지 말고,
 * 외부 3rd-party API를 호출하듯, 내부 서버 요청을 보내십시오.
 * 
 * 출력 관련 루틴이 충돌을 일으킵니다.
 * */
define ("OTK_ROOT", dirname(__DIR__));
define ("OTK_PRIVATE_ROOT", __DIR__);

include_once (__DIR__ . '/opentalk.lib.php');

// OTK 전역 변수를 설정합니다.
$GLOBALS['_OTK'] = $_OTK = [];
$GLOBALS['_OTK_STATE'] = $_OTK_STATE = [];

if (!isset($_OTK) || !isset($_OTK_STATE) || 
    !is_array($_OTK) || !is_array($_OTK_STATE)) 
{
    throw new Exception("Error: Your PHP environment disables \$GLOBALS set!");
}

include_once (__DIR__ . '/config.php');

// Randomizer를 초기화합니다.
srand(time());

// 서버 변수가 설정되어 있지 않다면.
if (!isset($_SERVER) || !is_array($_SERVER)) {
    throw new Exception("Error: Invalid PHP environment!");
}

if (!isset($_SERVER['DOCUMENT_ROOT']) || !$_SERVER['DOCUMENT_ROOT']) {
    throw new Exception("Error: can not estimate the absolute base URI!");
}

// OTK 루트 Path를 추정합니다.
$EstimatedRootPath = '/' . trim(str_replace('\\', '/', 
    substr(OTK_ROOT, strlen($_SERVER['DOCUMENT_ROOT']))), '/');

// $_OTK 전역 변수를 채웁니다.
$_OTK["PATH"] = isset($_SERVER["REQUEST_URI"]) ? $_SERVER["REQUEST_URI"] : '/';
$_OTK["PATH"] = urldecode(($p = strpos($_OTK["PATH"], '?')) !== false ? 
    substr($_OTK["PATH"], 0, $p) : $_OTK["PATH"]);

$_OTK['ROOT_PATH'] = $EstimatedRootPath;
$_OTK['BASE_PATH'] = dirname($_OTK['PATH']);

$_OTK['ENTRY'] = $_SERVER['PHP_SELF'];
$_OTK['ENTRY_PATH'] = dirname($_OTK['ENTRY']);

$_OTK['METHOD'] = isset($_SERVER['REQUEST_METHOD']) ? 
        strtoupper($_SERVER['REQUEST_METHOD']) : 'GET';
$_SERVER['PHP_SELF'] = $_OTK['PATH'];

$_OTK_STATE['RESPONSE'] = [
    'CODE' => 200,
    'MESSAGE' => "OK",
    'FILTERS' => [],
    'MODE' => "",
    'OVERWRITE' => "",
    'HEADERS' => []
];

spl_autoload_register(function($className) {
    $className = __DIR__ . '/classes/' . trim(
        str_replace('\\', '/', $className) . '.php', '/');
    
    if (file_exists($className)) {
        include_once ($className);
        return true;
    }
    
    return false;
});

set_exception_handler(function (Throwable $exception) {
    global $_OTK_STATE;
    
    otkStatusCode(500);
    otkDiscardOutputs();
    otkDiscardHeaders();
    otkRemoveAllOutputFilters();
    
    if ($exception instanceof OtkHttpException) {
        $exception->Handle($_OTK_STATE['RESPONSE']);
    }
});

/* 출력을 모두 캡쳐하고, 최종 출력을 필터링합니다. */
ob_start(function($output) {
    global $_OTK_STATE;    
    $finalState = $_OTK_STATE['RESPONSE'];
    
    header("HTTP/1.1 {$finalState['CODE']} {$finalState['MESSAGE']}");
    foreach ($finalState['HEADERS'] as $key => $value) {
        if ($value) {
            header("{$key}: {$value}");
        }
        
    }
    
    $responseMode = strtoupper($finalState['MODE']);
    switch ($responseMode) {
        case 'DISCARD':
            $output = "";
            break;

        case 'OVERWRITE':
            $output = $finalState['OVERWRITE'];
            break;
        
        default: // 'AS-IS':
            break;
    }
    
    // 출력을 순차적으로 필터링하고,
    if (count($finalState['FILTERS']) > 0) {
        foreach ($finalState['FILTERS'] as $filter) {
            if (!$output) {        
                break;
            }
            
            $output = call_user_func_array($filter, [ $output ]);
        }
    }
    
    // 최종 출력을 반환합니다.
    return $output;
});

// 컨텐츠 바디가 있는 요청일 때, 컨텐트 타입에 따라 파싱합니다.
if (isset($_SERVER['CONTENT_TYPE']) && 
    $_OTK['METHOD'] != 'GET' && $_OTK['METHOD'] != 'DELETE')
{
    $_SERVER['CONTENT_TYPE'] = strtolower($_SERVER['CONTENT_TYPE']);
    
    /* 컨텐트 타입이 JSON일 경우, JSON으로 파싱해서 $_POST에 채웁니다. */
    if (strpos($_SERVER['CONTENT_TYPE'], 'application/json') !== false ||
        strpos($_SERVER['CONTENT_TYPE'], 'text/json') !== false)
    {
        $JsonBody = json_decode(file_get_contents('php://input'), true);
        foreach ($JsonBody as $key => $value) {
            $_POST[$key] = $value;
        }
    }

    /* 요청 메서드가 POST인 경우, 이미 파싱 되어 있는 상태입니다.*/
    else if ($_OTK['METHOD'] != 'POST') {
        parse_str(file_get_contents('php://input'), $_POST);
    }
}

if (isset($_SERVER['HTTP_AUTHORIZATION']) && $_SERVER['HTTP_AUTHORIZATION']) {
    $Authorization = explode(' ', trim($_SERVER['HTTP_AUTHORIZATION']), 2);
    
    $Method = array_shift($Authorization);
    $Token = count($Authorization) > 0 ? end($Authorization) : null;
    
    /* 인증 헤더가 셋팅되어 있을 때, 그 값을 OTK 변수에 채웁니다.*/
    $_OTK['AUTH'] = [ 'METHOD' => strtoupper($Method), 'TOKEN' => $Token ];
}

else $_OTK['AUTH'] = [];

// DB에 접속합니다.
// 처음엔 읽기 전용 커넥션만 잡습니다.
// 쓰기가 가능해야 할 때 마스터로 접속, DB 커넥션을 교체하게 됩니다.
if (isset($_OTK_DB) && $_OTK_DB['MASTER']) {
    $_OTK['DB_RW'] = $_OTK['DB_RO'] = false;
    
    while(true) {    
        if (isset($_OTK_DB['SLAVES']) &&
            is_array($_OTK_DB['SLAVES']) &&
            count($_OTK_DB['SLAVES']) > 0) 
        {
            $i = time() % count($_OTK_DB['SLAVES']);
            $_OTK['DB_RO_INFO'] = $_OTK_DB['SLAVES'][$i];
            if (otkSqlGetConnection(false)) {
                break;
            }
        }

        $_OTK['DB_RW_INFO'] = $_OTK_DB['MASTER'];
        otkSqlGetConnection(true);
        break;
    }
    
    // PHP 스크립팅이 끝나면 연결을 끊습니다.
    register_shutdown_function(function() {
        global $_OTK;
        
        if ($_OTK['DB_RO']) {
            @mysqli_close($_OTK['DB_RO']);
        }
        
        if ($_OTK['DB_RW']) {
            @mysqli_close($_OTK['DB_RW']);
        }
    });
}
else {
    throw new OtkHttpException(503, 'Service Unavailable');
}

/*
 * OTK PHP 프레임워크 전체를 대상으로 한 사전 스크립트들을 실행합니다.
 * (글자 순으로 정렬됩니다)
 *  */
$Prepends = glob(__DIR__ . "/prepends/*.php");
sort($Prepends, SORT_STRING);

foreach ($Prepends as $EachFile) {
    include_once ($EachFile);
}

/*
 * 각 요청 메서드에 따른 Prefilter를 수행하고,
 * 2차 bootstrap 필터를 실행합니다.
 * ㅡ
 * 에러로 요청 처리를 차단하려면
 * Prefilter 구현 내에서 예외를 발생시키거나 'NO_MORE' 문자열을 리턴하십시오.
 * 
 * NO_MORE 문자열을 리턴한 경우엔, 즉시 모든 처리를 중단하고, 
 * 현재 상태 그대로 응답을 전송합니다.
 *  */
$otkMethod = strtolower($_OTK['METHOD']);
otkInvokePrefilter("http.{$otkMethod}");
otkInvokePrefilter("bootstrap");

