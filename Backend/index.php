<?php

include_once (__DIR__ . '/privates/opentalk.web.php');

otkWebTemplated(function() {
    ?>
    <div class="title">
        <h2>Welcome to OpenTalk Website!</h2>
        <span class="byline">OpenTalk is an open source messanger solution.</span> 
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
}, "index", function() {
    ?><div class="box1">
            <div class="title">
                    <h2>Mauris vulputate</h2>
            </div>
            <ul class="style2">
                    <li><a href="#">Semper mod quis eget mi dolore</a></li>
                    <li><a href="#">Quam turpis feugiat sit dolor</a></li>
                    <li><a href="#">Amet ornare in hendrerit in lectus</a></li>
                    <li><a href="#">Consequat etiam lorem phasellus</a></li>
                    <li><a href="#">Amet turpis, feugiat et sit amet</a></li>
                    <li><a href="#">Semper mod quisturpis nisi</a></li>
            </ul>
    </div>
    <div class="box2">
            <div class="title">
                    <h2>Integer gravida</h2>
            </div>
            <ul class="style2">
                    <li><a href="#">Amet turpis, feugiat et sit amet</a></li>
                    <li><a href="#">Ornare in hendrerit in lectus</a></li>
                    <li><a href="#">Semper mod quis eget mi dolore</a></li>
                    <li><a href="#">Quam turpis feugiat sit dolor</a></li>
                    <li><a href="#">Amet ornare in hendrerit in lectus</a></li>
                    <li><a href="#">Semper mod quisturpis nisi</a></li>
                    <li><a href="#">Consequat etiam lorem phasellus</a></li>
            </ul>
    </div><?php
});