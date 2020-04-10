<?php

/*
 * HTTP CORS 요청에 대한 PREFILTER입니다.
 * 
 * OPTIONS /resources/post-here/ HTTP/1.1 
    Host: bar.other 
    Accept: *
    Accept-Language: en-us,en;q=0.5 
    Accept-Encoding: gzip,deflate 
    Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7 
    Connection: keep-alive 
    Origin: http://foo.example 
    Access-Control-Request-Method: POST 
    Access-Control-Request-Headers: X-PINGOTHER, Content-Type
 * 
 * HTTP/1.1 200 OK
    Date: Mon, 01 Dec 2008 01:15:39 GMT 
    Server: Apache/2.0.61 (Unix) 
    Access-Control-Allow-Origin: http://foo.example 
    Access-Control-Allow-Methods: POST, GET, OPTIONS 
    Access-Control-Allow-Headers: X-PINGOTHER, Content-Type 
    Access-Control-Max-Age: 86400 
    Vary: Accept-Encoding, Origin 
    Content-Encoding: gzip 
    Content-Length: 0 
    Keep-Alive: timeout=2, max=100 
    Connection: Keep-Alive 
    Content-Type: text/plain
 *  */

if (!isset($_SERVER['HTTP_ORIGIN']) || !$_SERVER['HTTP_ORIGIN']) {
    throw new OtkHttpException(403);
}

$Origin = $_SERVER['HTTP_ORIGIN'];

/*
 * TODO: 입력된 Origin이 등록되어 있는지 검사합니다.
 *  */
otkSetHeader('Access-Control-Allow-Origin', $Origin);
otkSetHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE');
otkSetHeader('Access-Control-Allow-Headers', 'Authorization, Content-Type');
otkSetHeader('Access-Control-Max-Age', '3600');
otkSetHeader('Vary', 'Authorization, Origin');
return 'NO_MORE';