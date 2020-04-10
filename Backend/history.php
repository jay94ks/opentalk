<?php

include_once (__DIR__ . '/privates/opentalk.web.php');

otkWebTemplated(function() {
    global $_OTK_WEB;
    $_OTK_WEB['SHOWING'] = 'History';
    
    ?>
    <div class="title">
        <h2>Version History</h2>
        <span class="byline">The road where we are came.</span> 
    </div> 

    <span style="font-weight: bold">First Release, 2020/04/11</span>
    <p>
        OpenTalk의 최초 버젼입니다. <br />
        상용 메신저인 '카카오 톡'을 모작하는 프로젝트로 시작하여, <br />
        최초 공개까지 왔습니다.
    </p>
    <?php
}, "content");