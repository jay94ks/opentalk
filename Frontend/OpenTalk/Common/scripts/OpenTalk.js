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
     * CefScreen�� �غ�Ǿ��ٴ� �̺�Ʈ�� �߻���ŵ�ϴ�.
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
         * 5�ʿ� �ѹ��� ���¸� �����մϴ�.
         * */
        statusTimer = setInterval(pollInTick = function () {
            $.get("/status.json?r=" + requestIndex)
                .done(function (X) {
                    if (firstSuccess) {
                        $("body").addClass("otk-ready");
                        firstSuccess = false;
                    }

                    /* ���ŵ� ��ҿ� ���� ���� ������ �õ��մϴ�. */
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

                /* �̰� �����ϴ� ���� ���� ����ۿ� �����ϴ� ^_^; */
                .fail(function (X) {
                    clearInterval(statusTimer);
                    statusTimer = undefined;

                    /* 
                     * ���� �޽��� ����� FrmMain.cs ������ �ϴ� 
                     * ���⼱ �Ű澲�� �ʵ��� �մϴ�. 
                     * */
                    $("body").removeClass("otk-ready");
                });

            requestIndex++;
        }, 5000);

        pollInTick();
    };


    /* DOM ��Ұ� �����Ǹ� ����˴ϴ�. */
    handleMutation = function (records) {
        for (var i = 0; i < records.length; i++) {
            if (records[i].type === "childList") {
                /* Element ��尡 �ƴϸ� ��� �����մϴ�. */
                if (records[i].target.nodeType !== 1) {
                    continue;
                }

                var sectionNode = records[i].target.tagName.toLowerCase() !== "section" ?
                    $(records[i].target).parent("section") : $(records[i].target);

                /* otk-list�� �������� �ϳ��� ������� �ʰԵǸ� ����ϴ�. */
                if (sectionNode.hasClass("otk-list")) {
                    var allItems = null;
                    if (sectionNode.length <= 0) {
                        sectionNode = $(records[i].target).find("section");
                    }

                    if ((allItems = sectionNode.find(".otk-item")).length <= 0) {
                        sectionNode.css("display", "none");
                    } else sectionNode.css("display", "");

                    /* ��� �׸���� ���鼭 Ŭ�� �̺�Ʈ �ڵ鷯�� �����ϴ�. */
                    allItems.each(function (i, e) {
                        (e = $(e)).off("click").on("click", function () {
                            $(".otk-list > .otk-item")
                                .removeClass("otk-item-selected");

                            e.addClass("otk-item-selected");
                        }).off("dblclick").on("dblclick", function () {
                            /*
                             * ���� Ŭ�� �ڵ鷯 ���� ������ Ȯ���ϰ�,
                             * �����Ǿ� ������ �����ŵ�ϴ�.
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

        /* ��� list�� ���ؼ� onMutated �̺�Ʈ�� �ѹ��� ��������ݴϴ�.*/
        $("section.otk-list").each(function (i, e) {
            dummyRecords.push({ type: "childList", target: e });
        });

        handleMutation(dummyRecords);
    });

    /* �� ������ �������� �����ϴ� ������Ʈ �ڵ鷯�Դϴ�. */
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
