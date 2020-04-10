<?php

if (!defined("OTK_ROOT")) {
    throw new Exception("opentalk.lib.php file should not be stand-alone!");
}

/* Prefilter 스크립트를 실행하는 함수입니다. */
function otkInvokePrefilter($filterName) {
    global $_OTK, $_OTK_STATE, $_OTK_TABLES;
    
    $prefilter = __DIR__ . '/prefilters/' . $filterName . '.php';
    
    if (file_exists($prefilter)) {
        $RetVal = include_once ($prefilter);
        
        if ($RetVal == 'NO_MORE') {
            exit(0);
        }
        
        return true;
    }
    
    return false;
}

function otkDMd5($Value) {
    return md5(md5($Value) . OTK_MD5_SALT);
}

function otkStatusCode($Code = 0, $Message = null) {
    global $_OTK_STATE;
    
    if ($Code >= 100) {
        if (!$Message) {
            switch ($Code) {
                case 200: $Message = 'OK'; break;
                case 301: $Message = 'Moved Permanently'; break;
                case 302: $Message = 'Moved Temporarily'; break;
                case 400: $Message = 'Bad Request'; break;
                case 401: $Message = 'Unauthorized'; break;
                case 403: $Message = 'Forbidden'; break;
                case 404: $Message = 'Not Found'; break;
                case 500: $Message = 'Internal Server Error'; break;
                case 501: $Message = 'Not Implemented'; break;
                case 503: $Message = 'Service Unavailable'; break;
            }
        }
        
        $_OTK_STATE['RESPONSE']['CODE'] = $Code;
        $_OTK_STATE['RESPONSE']['MESSAGE'] = $Message;
        return true;
    }
    
    return $_OTK_STATE['RESPONSE']['CODE'];
}

function otkReplaceOutputs($NewContents) {
    global $_OTK_STATE;
    
    $_OTK_STATE['RESPONSE']['MODE'] = 'OVERWRITE';
    $_OTK_STATE['RESPONSE']['OVERWRITE'] = $NewContents;
}

function otkDiscardOutputs() {
    global $_OTK_STATE;
    
    $_OTK_STATE['RESPONSE']['MODE'] = 'DISCARD';
    $_OTK_STATE['RESPONSE']['OVERWRITE'] = '';
}

function otkSetHeader($key, $value = null) {
    global $_OTK_STATE;
    
    if ($value) {
        $_OTK_STATE['RESPONSE']['HEADERS'][$key] = $value;
        return true;
    }
    
    if (array_key_exists($_OTK_STATE['RESPONSE']['HEADERS'], $key)) {
        unset($_OTK_STATE['RESPONSE']['HEADERS'][$key]);
        return true;
    }
    
    return false;
}

function otkGetHeader($key) {
    global $_OTK_STATE;
    
    if (array_key_exists($_OTK_STATE['RESPONSE']['HEADERS'], $key)) {
        return $_OTK_STATE['RESPONSE']['HEADERS'][$key];
    }
    
    return false;
}

function otkDiscardHeaders() {
    global $_OTK_STATE;
    
    $_OTK_STATE['RESPONSE']['HEADERS'] = [];
}

function otkRemoveAllOutputFilters() {
    global $_OTK_STATE;
    
    $_OTK_STATE['RESPONSE']['FILTERS'] = [];
}

function otkRemoveOutputFilter($filter) {
    global $_OTK_STATE;
    
    foreach ($_OTK_STATE['RESPONSE']['FILTERS'] as $i => $eachFilter) {
        if ($eachFilter == $filter) {
            array_splice($_OTK_STATE['RESPONSE']['FILTERS'], $i, 1);
            return true;
        }
    }
    
    return false;
}

function otkAddOutputFilter($filter) {
    global $_OTK_STATE;
    
    otkRemoveOutputFilter($filter);
    $_OTK_STATE['RESPONSE']['FILTERS'][] = $filter;
}

function otkSqlGetConnection($RequireWrite = false) {
    global $_OTK;
    
    if ($_OTK['DB_RW']) {
        return $_OTK['DB_LA'] = $_OTK['DB_RW'];
    }
    else if ($_OTK['DB_RO'] && !$RequireWrite) {
        return $_OTK['DB_LA'] = $_OTK['DB_RO'];
    }
    else if ($RequireWrite) {
        if (!$_OTK['DB_RW'] && $_OTK['DB_RW_INFO']) {
            $ConnInfo = $_OTK['DB_RW_INFO'];
            
            $_OTK['DB_RW'] = @mysqli_connect($ConnInfo['HOST'], 
                $ConnInfo['USER'], $ConnInfo['PASS'], 
                $ConnInfo['SCHM'], $ConnInfo['PORT']);
            
            if (!$_OTK['DB_RW']) {
                throw new OtkHttpException(503, 'Service Unavailable');
            }
            
            mysqli_set_charset($_OTK['DB_RW'], 'utf8');
        }
        
        if ($_OTK['DB_RO']) {
            mysqli_close($_OTK['DB_RO']);
            $_OTK['DB_RO'] = false;
        }
        
        return $_OTK['DB_LA'] = $_OTK['DB_RW'];
    }
    
    else {
        if (!$_OTK['DB_RO'] && $_OTK['DB_RO_INFO']) {
            $ConnInfo = $_OTK['DB_RO_INFO'];
            
            $_OTK['DB_RO'] = @mysqli_connect($ConnInfo['HOST'], 
                $ConnInfo['USER'], $ConnInfo['PASS'], 
                $ConnInfo['SCHM'], $ConnInfo['PORT']);
            
            if (!$_OTK['DB_RO']) {
                throw new OtkHttpException(503, 'Service Unavailable');
            }
            
            mysqli_set_charset($_OTK['DB_RO'], 'utf8');
        }
        
        return $_OTK['DB_LA'] = $_OTK['DB_RO'];
    }
    
    return $_OTK['DB_LA'] = $_OTK['DB_RO'];
}

function otkSqlEscape($Text) {
    global $_OTK;
    
    if ($_OTK['DB_LA']) {
        return mysqli_real_escape_string($_OTK['DB_LA'], $Text);
    }
    
    return mysqli_real_escape_string(
        otkSqlGetConnection(), $Text);
}

function otkSqlQuery($Sql) {
    global $_OTK;
    
    $Sql = trim($Sql);
    $Type = strtoupper(substr($Sql, 0, 2));
    $Connection = null;
    
    switch ($Type) {
        case 'SE'; // SELECT
            $Connection = otkSqlGetConnection();
            break;
        
        case 'LO':  // LOCK
        case 'UN':  // UNLOCK
        case 'IN'; // INSERT
        case 'UP'; // UPDATE
        case 'DE'; // DELETE
            $Connection = otkSqlGetConnection(true);
            break;
        
        default:
            $Connection = otkSqlGetConnection();
            break;
    }
    
    if ($Connection) {
        $RetVal = mysqli_query($Connection, $Sql);
        
        switch ($Type) {
            case 'UP'; // UPDATE
            case 'DE'; // DELETE
                $RetVal = mysqli_affected_rows($Connection) > 0;
                break;
            
            case 'IN'; // INSERT
                $RetVal = mysqli_affected_rows($Connection) > 0;
                
                if ($RetVal) {
                    $_OTK['DB_INSERT_ID'] = mysqli_insert_id($Connection);
                } else {
                    $_OTK['DB_INSERT_ID'] = null;
                }
                break;

            /*
            case 'SE'; // SELECT
            case 'LO':  // LOCK
            case 'UN':  // UNLOCK
            */
            default:
                break;
        }
        
        return $RetVal;
    }
    
    return false;
}

function otkRandomString($Length = 40) {
    $Characters  = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    $TotalCharacters = strlen($Characters);
    $RetVal = "";  
      
    while ($Length--) {
        $RetVal .= substr($Characters, rand() % $TotalCharacters, 1); 
    }
    
    return $RetVal;
}

function otkSqlGenUniqueString($Label = "COMMON", $Length = 40) {
    global $_OTK_TABLES;
    $FinalUniqueString = null;
    $Label = strtoupper($Label);
    
    if (strlen($Label) > 7) {
        $Label = substr($Label, 0, 7);
    }
    
    // SQL 커넥션을 쓰기 가능으로 업그레이드 하고,
    // 테이블 락을 획득한 후, 생성 절차를 시작합니다.
    otkSqlGetConnection(true);
    otkSqlQuery("LOCK TABLES `{$_OTK_TABLES['UNIQUE_STRINGS']}` WRITE");
    
    while(true) {
        $UniqueString = otkRandomString($Length);
        
        if (otkSqlQuery("INSERT INTO `{$_OTK_TABLES['UNIQUE_STRINGS']}` ".
            "(`unique_string`, `unique_label`, `generated_at`) ".
            "VALUES ('{$UniqueString}', '{$Label}', NOW());"))
        {
            $FinalUniqueString = $UniqueString;
            break;
        }
    }
    
    otkSqlQuery("UNLOCK TABLES");
    return $FinalUniqueString;
}

function otkSqlInsertId($Sql) {
    global $_OTK;
    
    if ($_OTK['DB_INSERT_ID']) {
        return $_OTK['DB_INSERT_ID'];
    }
    
    return false;
}

function otkSqlQueryOne($Sql) {
    $RetVal = false;
    $Result = otkSqlQuery($Sql);
    
    if ($Result) {
        $RetVal = mysqli_fetch_array($Result, MYSQLI_ASSOC);
        mysqli_free_result($Result);
    }
    
    return $RetVal;
}

function otkSqlQuerySingle($Sql, $Filter = null) {
    $RetVal = otkSqlQueryOne($Sql);
    
    if (is_array($RetVal)) {
        $RetVal = array_values($RetVal);
        if (count($RetVal) <= 0) {
            return false;
        }
        
        $RetVal = end($RetVal);
        if ($Filter) {
            return call_user_func_array($Filter, [ $RetVal ]);
        }
        
        return $RetVal;
    }
    
    return false;
}

function otkSqlQueryAll($Sql) {
    $RetVal = null;
    $Result = otkSqlQuery($Sql);
    
    if ($Result) {
        $RetVal = [];
        
        while(($Row = mysqli_fetch_array($Result, MYSQLI_ASSOC)) != null) {
            $RetVal[] = $Row;
        }
        
        mysqli_free_result($Result);
    }
    
    return $RetVal;
}