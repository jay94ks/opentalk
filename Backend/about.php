<?php

include_once (__DIR__ . '/privates/opentalk.web.php');

otkWebTemplated(function() {
    global $_OTK_WEB;
    $_OTK_WEB['SHOWING'] = 'About';
    
    ?>
    <div class="title">
        <h2>About This Project</h2>
        <span class="byline">Introduction of `Open Talk` project.</span> 
    </div>
    <div id="two-column">
        <div id="tbox1">
            <div class="title">
                <h2>Full Source</h2>
            </div>
            <a href="#" class="image image-full"><img src="templates/lounging/images/pic01.jpg" alt="" /></a>
            <p>
                OpenTalk의 전체 소스코드는 공개 되어있습니다.
                따라서, 누구나 수정할 수 있으며, 누구나 OpenTalk 기반 서비스를 제공할 수 있습니다.
                지금 바로 GitHub에서 확인하십시오.
            </p>
            <a href="#" class="button">Visit Repository</a>
        </div>
        <div id="tbox2">
            <div id="tbox2">
                <div class="title">
                    <h2>Road-map</h2>
                </div>
                <a href="#" class="image image-full"><img src="templates/lounging/images/pic02.jpg" alt="" /></a>
                <p>
                    OpenTalk은 아직 많이 부족하지만, PC 메신저라면 구현해야 할 일반적인 기능 대부분을 구현하고 있습니다.
                    현재 구현된 기능과, 앞으로 구현될 기능을 지금 살펴보세요.
                </p>
                <a href="#" class="button">Goto Road-map</a>
            </div>
        </div>
    </div>
    <?php
}, "content");