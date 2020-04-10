<?php

class OtkHttpException extends Exception {
    function __construct($Code, $Message = null) {
        parent::__construct();
        
        $this->Code = $Code;
        $this->Message = $Message;
    }
    
    public $Code;
    public $Message;
    
    /*
     * 상태 제어값을 이 예외의 특성에 맞게 변경합니다.
     * */
    function Handle(array& $State) {
        $State['CODE'] = $this->Code;
        $State['MESSAGE'] = $this->Message;
    }
}

