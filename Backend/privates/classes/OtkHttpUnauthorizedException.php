<?php

class OtkHttpUnauthorizedException extends OtkHttpException {
    function __construct() {
        parent::__construct(401);
    }
}

