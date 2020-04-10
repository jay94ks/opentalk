<?php

/*
 * 멤버 테이블을 조작하는 함수들을 모아놓은 라이브러리입니다.
 *  */

function otkMemberGet($Identifier) {
    global $_OTK_TABLES;
    
    $EscapedId = otkSqlEscape(strtolower($Identifier));
    $Member = otkSqlQueryOne(
        "SELECT * FROM `{$_OTK_TABLES['MEMBER_TABLE']}` " .
        "WHERE `email` = '{$EscapedId}' OR `cellphone` = '{$EscapedId}' ". 
        "LIMIT 0, 1");

    if (is_array($Member) && !isset($Member['__runtime_data'])) {
        $Member['__runtime_data'] = [
            'Fields' => array_keys($Member),
            'FlushTargets' => [],
            'ChangedValues' => []
        ];
    }
    
    return $Member;
}

function otkMemberNew($Name, $Email, $Password) {
    global $_OTK_TABLES;
    
    $Identifier = $Email;
    $Member = otkMemberGet($Email);
    
    // 이미 해당 이메일로 가입된 계정이 있음.
    if ($Member) {
        return false;
    }
    
    $Name = otkSqlEscape($Name);
    $Email = otkSqlEscape(strtolower($Email));
    $Password = otkSqlEscape(otkDMd5($Password));
    $RetVal = false;
    
    otkSqlQuery("LOCK TABLES `{$_OTK_TABLES['MEMBER_TABLE']}` WRITE");
    $RetVal = otkSqlQuery(
        "INSERT INTO `{$_OTK_TABLES['MEMBER_TABLE']}` (`created_at`, ".
        "`updated_at`, `password`, `password_date`, `name`, `display_name`, `email`, `telphone`, `cellphone`, ".
        "`active_sessions`, `introduction`) VALUES (NOW(), NOW(), ".
        "'{$Password}', NOW(), '{$Name}', '{$Name}', '{$Email}', '', '', '0', '')");
    
    otkSqlQuery("UNLOCK TABLES");
    if ($RetVal) {
        return otkMemberGet($Identifier);
    }
    
    return false;
}

function otkMemberCheckPassword(array &$Member, $Password, $HashedPassword = false) {
    if (is_array($Member) && $Member['password']) {
        if ($HashedPassword) {
            return $Member['password'] == $Password;
        }
        
        return otkDMd5($Password) == $Member['password'];
    }
    
    return false;
}

function otkMemberSetValue(array &$Member, $FieldName, $FieldValue) {
    if (is_array($Member)) {
        if (!isset($Member['__runtime_data'])) {
            $Member['__runtime_data'] = [
                'Fields' => array_keys($Member),
                'FlushTargets' => [],
                'ChangedValues' => []
            ];
        }
        
        if ($FieldName == "password") {
            $FieldValue = otkDMd5($FieldValue);
        }
        
        if ($Member[$FieldName] != $FieldValue && $FieldName != 'no' &&
            $FieldName != 'updated_at' && $FieldName != 'created_at' &&
            array_search($FieldName, $Member['__runtime_data']['Fields']) !== false) 
        {
            $Member['__runtime_data']['FlushTargets'][] = $FieldName;
            $Member['__runtime_data']['FlushTargets']
                = array_unique($Member['__runtime_data']['FlushTargets']);
            
            $Member['__runtime_data']['ChangedValues'][$FieldName] = $FieldValue;
            
            if ($FieldName == "password") {
                otkMemberSetValue($Member, 'password_date', date('Y-m-d H:i:s'));
            }
            
            return true;
        }    
    }
    return false;
}

function otkMemberFlushChanges(array &$Member) {
    global $_OTK_TABLES;
    
    if (is_array($Member) && isset($Member['__runtime_data']) &&
        count($Member['__runtime_data']['FlushTargets']) > 0) 
    {
        $MemberNo = $Member['no'];
        $UpdateTargets = [];
        
        foreach ($Member['__runtime_data']['FlushTargets'] as $FieldName) {
            $EscapedValue = otkSqlEscape($Member['__runtime_data']['ChangedValues'][$FieldName]);
            $UpdateTargets[] = "`{$FieldName}` = '{$EscapedValue}'";
        }
        
        $UpdateTargets[] = "`updated_at` = NOW()";
        $UpdateTargets = implode(',', $UpdateTargets);
        $RetVal = otkSqlQuery("UPDATE `{$_OTK_TABLES['MEMBER_TABLE']}` " . 
            "SET {$UpdateTargets} WHERE `no` = '{$MemberNo}'");
        
        if ($RetVal) {
            foreach ($Member['__runtime_data']['ChangedValues'] as $FieldName => $FieldValue) {
                $Member[$FieldName] = $FieldValue;
            }
            
            $Member['__runtime_data']['FlushTargets'] = [];
            $Member['__runtime_data']['ChangedValues'] = [];
        }
        
        return $RetVal;
    }
    
    return false;
}