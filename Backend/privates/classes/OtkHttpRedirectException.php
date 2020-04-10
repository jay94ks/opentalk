<?php

class OtkHttpRedirectException extends OtkHttpException {
    function __construct($Location, $Permanent = false) {
        parent::__construct($Permanent ? 301 : 302);
        $this->Location = $Location;
    }
    
    public $Location;
    
    /*
     * 상태 제어값을 이 예외의 특성에 맞게 변경합니다.
     * */
    function Handle(array& $State) {
        parent::Handle($State);
        otkSetHeader('Location', $this->Location);
    }
}

