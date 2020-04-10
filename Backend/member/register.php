<?php

include_once (__DIR__ . '/../privates/opentalk.web.php');
session_start();

otkWebTemplated(function() {
    global $_OTK_WEB, $_OTK;
    $_OTK_WEB['SHOWING'] = 'Member';
    
    
    $FailedToCreate = false;
    $FailureMessage = "";
    
    if ($_OTK['METHOD'] == 'POST') {
        $EmailUser = $_POST['email_user'];
        $EmailServer = $_POST['email_server'];
        $Email = strtolower("{$EmailUser}@{$EmailServer}");
        $Name = $_POST['name'];
        $Password = $_POST['password'];
        
        $Member = otkMemberNew($Name, $Email, $Password);
        if (!$Member) {
            $FailedToCreate = true;
            $FailureMessage = "해당 이메일 주소로 이미 등록된 계정이 존재합니다.";
        }
    }
    
    
    ?>
    <div class="title">
        <h2>Member Registration</h2>
        <span class="byline">Fill below form for registering yourself!</span> 
    </div>
    <div style="display: <?=($FailedToCreate ? 'block' : 'none')?>">
        <?=$FailureMessage?>
    </div>
    <form method="POST">
        <table>
            <tr>
                <td>이메일</td>
                <td>
                    <input type="text" name="email_user" /> @ <input type="text" name="email_server" />
                </td>
            </tr>
            <tr>
                <td>이름 (실명)</td>
                <td>
                    <input type="text" name="name" />
                </td>
            </tr>
            <tr>
                <td>비밀번호</td>
                <td>
                    <input type="password" name="password" />
                </td>
            </tr>
        </table>
        <input type="submit" value="계정 만들기" />
    </form>
    <?php
}, 'content');