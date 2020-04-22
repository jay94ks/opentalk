(function () {
    var screenReady;
    var armStatusTimer;
    var statusTimer;
    var pollInTick;
    var mutation;
    var handleMutation = null;
    var updaters = {};

    if (document.createEvent) {
        (screenReady = document.createEvent('HTMLEvents'))
            .initEvent("screen:ready", true, true);
    } else {
        (screenReady = document.createEventObject())
            .eventType = "screen:ready";
    }

    /*
     * CefScreen이 준비되었다는 이벤트를 발생시킵니다.
     * */
    window.__cefScreenCb = function () {
        //__cefScreenRd
        if (window.__cefScreenRd) {
            $("body").addClass("otk-screen-ready");
            $("body").trigger("screen:ready");
        }
    };

    if (window.__cefScreenRd !== undefined) {
        window.__cefScreenCb();
    }

    armStatusTimer = function () {
        var firstSuccess = true;
        var requestIndex = 0;

        if (statusTimer !== undefined) {
            clearInterval(statusTimer);
        }

        /*
         * 5초에 한번씩 상태를 갱신합니다.
         * */
        statusTimer = setInterval(pollInTick = function () {
            $.get("/status.json?r=" + requestIndex)
                .done(function (X) {
                    if (firstSuccess) {
                        $("body").addClass("otk-ready");
                        firstSuccess = false;
                    }

                    /* 갱신된 요소에 대한 실제 갱신을 시도합니다. */
                    for (var i = 0; i < X.targets.length; i++) {
                        if (updaters[X.targets[i]] !== undefined) {
                            var updater = updaters[X.targets[i]];

                            console.log(updater, X.targets[i]);
                            if (updater.func !== undefined) {
                                updater.func.apply(updater);
                            }
                        }
                    }
                })

                /* 이게 실패하는 경우는 세션 만료밖에 없습니다 ^_^; */
                .fail(function (X) {
                    clearInterval(statusTimer);
                    statusTimer = undefined;

                    /* 
                     * 에러 메시지 출력은 FrmMain.cs 측에서 하니 
                     * 여기선 신경쓰지 않도록 합니다. 
                     * */
                    $("body").removeClass("otk-ready");
                });

            requestIndex++;
        }, 5000);

        pollInTick();
    };


    /* DOM 요소가 변형되면 실행됩니다. */
    handleMutation = function (records) {
        for (var i = 0; i < records.length; i++) {
            if (records[i].type === "childList") {
                /* Element 노드가 아니면 모두 생략합니다. */
                if (records[i].target.nodeType !== 1) {
                    continue;
                }

                var sectionNode = records[i].target.tagName.toLowerCase() !== "section" ?
                    $(records[i].target).parent("section") : $(records[i].target);

                /* otk-list에 아이템이 하나도 들어있지 않게되면 감춥니다. */
                if (sectionNode.hasClass("otk-list")) {
                    var allItems = null;
                    if (sectionNode.length <= 0) {
                        sectionNode = $(records[i].target).find("section");
                    }

                    if ((allItems = sectionNode.find(".otk-item")).length <= 0) {
                        sectionNode.css("display", "none");
                    } else sectionNode.css("display", "");

                    /* 모든 항목들을 돌면서 클릭 이벤트 핸들러를 붙힙니다. */
                    allItems.each(function (i, e) {
                        (e = $(e)).off("click").on("click", function () {
                            $(".otk-list > .otk-item")
                                .removeClass("otk-item-selected");

                            e.addClass("otk-item-selected");
                        }).off("dblclick").on("dblclick", function () {
                            /*
                             * 더블 클릭 핸들러 존재 유무를 확인하고,
                             * 설정되어 있으면 실행시킵니다.
                             * */
                            if ($(e).get(0).__action !== undefined) {
                                $(e).get(0).__action();
                            }
                        });
                    });
                }
            }
        }

        $("#friends_count").text(
            $(".otk-just-friends > .otk-friend-item").length + "");
    };

    $(document).ready(function () {
        var dummyRecords = [];

        mutation = new MutationObserver(handleMutation);
        $(".otk-sidebar-item").each(function (i, item) {
            $(item).on("click", function () {
                $(".otk-page-previous")
                    .removeClass("otk-page-previous");

                $(".otk-page-current")
                    .addClass("otk-page-previous")
                    .removeClass("otk-page-current");

                $($(item).data("target"))
                    .addClass("otk-page-current");

                $(".otk-sidebar-item")
                    .removeClass("otk-sidebar-current");
                $(item).addClass("otk-sidebar-current");
            });
        });

        $("section.otk-list").each(function (i, e) {
            /* attributes: true, characterData: true */
            mutation.observe(e, { childList: true });
        });

        /* 모든 list에 대해서 onMutated 이벤트를 한번씩 실행시켜줍니다.*/
        $("section.otk-list").each(function (i, e) {
            dummyRecords.push({ type: "childList", target: e });
        });

        handleMutation(dummyRecords);
    });

    /* 내 프로필 아이템을 갱신하는 업데이트 핸들러입니다. */
    var profileSections = [
        ".otk-my-profile",
        ".otk-favorated-friends",
        ".otk-just-friends"
    ];

    ["myinfo", "favorates", "friends"].forEach(function (target, i) {
        var targetSection = profileSections[i];

        updaters[target] = {
            running: false,
            func: function () {
                var self = this;

                if (self.running) {
                    return;
                }

                self.running = true;
                $.get("/?" + target)
                    .done(function (X) {
                        $(targetSection)
                            .find(".otk-item")
                            .remove();

                        self.running = false;
                        $(targetSection).append(X);
                    })
                    .fail(function () {
                        self.running = false;
                    });
            }
        };
    });

    armStatusTimer();
})();
